using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
using SqlSugar.IOC;
using ZR.Model.PMO;
using ZR.Model.System;
using ZR.Service.PMO.IPmoService;
using ZR.ServiceCore.Signalr;

namespace ZR.Service.PMO
{
    /// <summary>Per-rule outcome, returned to the caller (scheduler log / manual-trigger API).</summary>
    public class PmoAutomationRuleResult
    {
        public string RuleKey { get; set; }
        public string Status { get; set; }   // Success | Failed
        public int Candidates { get; set; }  // matching entities found
        public int Created { get; set; }      // new notifications written
        public int Skipped { get; set; }      // duplicates suppressed by dedupe
        public string Detail { get; set; }
        public long RunId { get; set; }
    }

    /// <summary>
    /// PMO business-automation engine. Runs idempotent rules against the current data and writes:
    ///   • pmo_outbox_notification — one in-app notification per logical event, keyed by a UNIQUE
    ///     dedupe_key so the same alert is never sent twice (safe retry), and
    ///   • pmo_automation_run — one execution-log row per rule per run (persistent run history).
    ///
    /// Rules implemented:
    ///   • overdue_task_escalation — tasks past their due date and not Done → notify the project owner.
    ///   • budget_overrun_alert   — projects whose posted actual spend exceeds the approved budget.
    ///
    /// Trigger: the background worker (schedule) or POST /pmo/admin/run-automation (manual demo).
    /// Failure handling: each rule is wrapped in try/catch; a failure is recorded as a Failed run and
    /// the next scheduled tick simply retries — nothing already sent is resent (dedupe guarantees it).
    /// Optionally uses the multi-provider AI chain to draft the notification body (template fallback).
    /// </summary>
    public class PmoAutomationRunner
    {
        private readonly ISqlSugarClient _db;
        private readonly IPmoAiService _ai;
        private readonly bool _useAi;
        private readonly IPmoNotifier _notifier;

        public const string RuleOverdue = "overdue_task_escalation";
        public const string RuleOverrun = "budget_overrun_alert";

        public PmoAutomationRunner(ISqlSugarClient db = null, IPmoAiService ai = null, bool useAi = true, IPmoNotifier notifier = null)
        {
            _db = db ?? DbScoped.SugarScope.GetConnectionScope(0);
            _ai = ai;
            _useAi = useAi;
            // Best-effort resolve from the container so the Messages + SignalR delivery works even when
            // the runner is constructed directly (manual trigger / scheduler) rather than DI-injected.
            _notifier = notifier ?? TryResolve();
        }

        private static IPmoNotifier TryResolve()
        {
            try { return (IPmoNotifier)global::Infrastructure.App.GetRequiredService(typeof(IPmoNotifier)); }
            catch { return null; }
        }

        /// <summary>Run every rule. <paramref name="asOf"/> defaults to today (override for demos).</summary>
        public List<PmoAutomationRuleResult> RunAll(string triggeredBy, DateTime? asOf = null)
        {
            var today = (asOf ?? DateTime.Now).Date;
            return new List<PmoAutomationRuleResult>
            {
                RunRule(RuleOverdue, triggeredBy, today, RunOverdueEscalation),
                RunRule(RuleOverrun, triggeredBy, today, RunBudgetOverrun),
            };
        }

        /// <summary>Wrap one rule: run it, log the execution, never let an exception escape.</summary>
        private PmoAutomationRuleResult RunRule(string ruleKey, string triggeredBy, DateTime today,
            Func<DateTime, PmoAutomationRuleResult> body)
        {
            var now = DateTime.Now;
            PmoAutomationRuleResult r;
            try
            {
                r = body(today);
                r.RuleKey = ruleKey;
                r.Status = "Success";
            }
            catch (Exception ex)
            {
                r = new PmoAutomationRuleResult
                {
                    RuleKey = ruleKey,
                    Status = "Failed",
                    Detail = $"Error: {ex.Message}"
                };
            }

            // Persist the execution-log row. dedupe_key is unique per execution (timestamp) so every
            // run is logged; the "no double send" guarantee lives on the outbox, not here.
            var run = new PmoAutomationRun
            {
                RunId = SnowFlakeSingle.Instance.NextId(),
                RuleKey = ruleKey,
                TriggeredAt = now,
                Status = r.Status,
                Detail = $"[{triggeredBy}] asOf={today:yyyy-MM-dd} candidates={r.Candidates} created={r.Created} skipped={r.Skipped}. {r.Detail}".Trim(),
                DedupeKey = $"{ruleKey}:{now:yyyyMMddHHmmssfff}"
            };
            _db.Insertable(run).ExecuteCommand();
            r.RunId = run.RunId;
            return r;
        }

        // ---------------- Rule 1: overdue task escalation ----------------

        private PmoAutomationRuleResult RunOverdueEscalation(DateTime today)
        {
            var res = new PmoAutomationRuleResult();

            var overdue = _db.Queryable<PmoTask>()
                .Where(t => t.DueDate != null && t.DueDate < today && t.Status != "Done")
                .ToList();
            res.Candidates = overdue.Count;
            if (overdue.Count == 0) { res.Detail = "No overdue tasks."; return res; }

            // Resolve owning projects (one IN query, no fan-out).
            var projectIds = overdue.Select(t => t.ProjectId).Distinct().ToList();
            var projects = _db.Queryable<PmoProject>().Where(p => projectIds.Contains(p.ProjectId))
                .ToList().ToDictionary(p => p.ProjectId);

            foreach (var t in overdue)
            {
                projects.TryGetValue(t.ProjectId, out var proj);
                var dedupe = $"overdue:{t.TaskId}:{today:yyyyMMdd}";
                if (Exists(dedupe)) { res.Skipped++; continue; }

                var daysLate = (today - t.DueDate.Value.Date).Days;
                var facts = $"{{\"rule\":\"overdue\",\"project\":\"{Esc(proj?.Name)}\",\"task\":\"{Esc(t.Title)}\",\"dueDate\":\"{t.DueDate:yyyy-MM-dd}\",\"daysLate\":{daysLate},\"status\":\"{t.Status}\"}}";
                var subject = $"Overdue task: {t.Title}";
                var body = Draft(RuleOverdue, facts)
                    ?? $"Task \"{t.Title}\" in project \"{proj?.Name}\" is {daysLate} day(s) overdue (due {t.DueDate:yyyy-MM-dd}, status {t.Status}). Please reassign or update the due date.";

                if (Insert("OverdueEscalation", subject, body, proj?.OwnerUserId, dedupe)) res.Created++;
                else res.Skipped++;
            }

            res.Detail = $"{res.Created} escalation(s) queued, {res.Skipped} already sent.";
            return res;
        }

        // ---------------- Rule 2: budget overrun alert ----------------

        private PmoAutomationRuleResult RunBudgetOverrun(DateTime today)
        {
            var res = new PmoAutomationRuleResult();
            var projects = _db.Queryable<PmoProject>().ToList();
            res.Candidates = projects.Count;
            if (projects.Count == 0) { res.Detail = "No projects."; return res; }

            // Posted actual spend per project (one grouped query).
            var spendByProject = _db.Queryable<PmoCostTransaction>()
                .Where(c => c.Status == "Posted")
                .GroupBy(c => c.ProjectId)
                .Select(c => new { c.ProjectId, Total = SqlFunc.AggregateSum(c.Amount) })
                .ToList().ToDictionary(x => x.ProjectId, x => x.Total);

            // Approved budget = initial + approved revisions (one grouped query).
            var revByProject = _db.Queryable<PmoBudgetRevision>()
                .Where(b => b.Status == "Approved")
                .GroupBy(b => b.ProjectId)
                .Select(b => new { b.ProjectId, Total = SqlFunc.AggregateSum(b.Amount) })
                .ToList().ToDictionary(x => x.ProjectId, x => x.Total);

            int overrun = 0;
            foreach (var p in projects)
            {
                var spend = spendByProject.TryGetValue(p.ProjectId, out var s) ? s : 0m;
                var approved = p.InitialApprovedBudget + (revByProject.TryGetValue(p.ProjectId, out var rv) ? rv : 0m);
                if (approved <= 0 || spend <= approved) continue;
                overrun++;

                var dedupe = $"overrun:{p.ProjectId}:{today:yyyyMMdd}";
                if (Exists(dedupe)) { res.Skipped++; continue; }

                var over = spend - approved;
                var pct = approved > 0 ? (double)(spend / approved) * 100 : 0;
                var facts = $"{{\"rule\":\"overrun\",\"project\":\"{Esc(p.Name)}\",\"approvedBudget\":{approved},\"actualSpend\":{spend},\"overrunAmount\":{over},\"utilizationPct\":{pct:F0},\"currency\":\"{p.Currency}\"}}";
                var subject = $"Budget overrun: {p.Name}";
                var body = Draft(RuleOverrun, facts)
                    ?? $"Project \"{p.Name}\" has exceeded its approved budget: spent {spend:N0} {p.Currency} of {approved:N0} {p.Currency} ({pct:F0}%). Overrun {over:N0} {p.Currency}. Review the cost plan or raise a budget revision.";

                if (Insert("OverrunAlert", subject, body, p.OwnerUserId, dedupe)) res.Created++;
                else res.Skipped++;
            }

            res.Detail = overrun == 0 ? "No projects over budget." : $"{overrun} project(s) over budget; {res.Created} alert(s) queued, {res.Skipped} already sent.";
            return res;
        }

        // ---------------- helpers ----------------

        private bool Exists(string dedupeKey)
            => _db.Queryable<PmoOutboxNotification>().Any(n => n.DedupeKey == dedupeKey);

        /// <summary>Insert a notification; returns false if a concurrent run already sent it (unique hit).</summary>
        private bool Insert(string type, string subject, string body, long? targetUserId, string dedupeKey)
        {
            try
            {
                var n = new PmoOutboxNotification
                {
                    NotificationId = SnowFlakeSingle.Instance.NextId(),
                    Type = type,
                    Subject = subject.Length > 200 ? subject[..200] : subject,
                    Body = body != null && body.Length > 2000 ? body[..2000] : body,
                    TargetUserId = targetUserId,
                    CreatedAt = DateTime.Now,
                    SentAt = DateTime.Now,    // in-app "send" is immediate (the row IS the delivery)
                    Status = "Sent",
                    DedupeKey = dedupeKey
                };
                _db.Insertable(n).ExecuteCommand();
                // Deliver into the user's Messages inbox + realtime header popup (best-effort).
                _notifier?.Notify(targetUserId, subject, body, type);
                return true;
            }
            catch (Exception)
            {
                // Unique-index violation => another run beat us to it. Treat as "already sent".
                return false;
            }
        }

        /// <summary>Draft the body via the AI fallback chain; null if AI is off/unavailable.</summary>
        private string Draft(string ruleKey, string factsJson)
        {
            if (!_useAi || _ai == null || !_ai.IsConfigured) return null;
            try
            {
                var text = _ai.DraftNotificationAsync(ruleKey, factsJson).GetAwaiter().GetResult();
                return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
            }
            catch { return null; }   // never let AI failure break the automation
        }

        private static string Esc(string s) => (s ?? "").Replace("\\", " ").Replace("\"", "'");
    }
}
