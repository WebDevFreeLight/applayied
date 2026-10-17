using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Model.System;
using ZR.ServiceCore;
using ZR.Service.PMO.IPmoService;

namespace ZR.Service.PMO
{
    /// <summary>
    /// Executive dashboard analytics. Every value is derived live from SQL within the filter context —
    /// there are no stored KPI totals. Aggregates per child table are composed by project_id so that
    /// joining multiple one-to-many tables never inflates spend/budget/counts.
    /// </summary>
    [AppService(ServiceType = typeof(IPmoDashboardService), ServiceLifetime = LifeTime.Transient)]
    public class PmoDashboardService : BaseService<PmoProject>, IPmoDashboardService
    {
        /// <summary>Resolve the filtered project set (also the scope boundary for all aggregates).</summary>
        private List<PmoProject> FilteredProjects(PmoDashboardFilter f, PmoActor actor)
        {
            var exp = Expressionable.Create<PmoProject>();
            exp.AndIF(f.DeptId != null, p => p.DeptId == f.DeptId);
            exp.AndIF(f.ProjectId != null, p => p.ProjectId == f.ProjectId);
            exp.AndIF(f.ManagerId != null, p => p.OwnerUserId == f.ManagerId);
            exp.AndIF(!string.IsNullOrEmpty(f.Status), p => p.Status == f.Status);
            if (!actor.IsProjectManager)
            {
                long uid = actor.UserId;
                exp.And(p => p.OwnerUserId == uid
                    || SqlFunc.Subqueryable<PmoProjectMember>().Where(m => m.ProjectId == p.ProjectId && m.UserId == uid).Any());
            }
            return Context.Queryable<PmoProject>().Where(exp.ToExpression()).ToList();
        }

        public PmoDashboardKpis GetKpis(PmoDashboardFilter filter, PmoActor actor)
        {
            var cutoff = filter.Cutoff ?? DateTime.Today;
            var due14 = cutoff.AddDays(14);
            var projects = FilteredProjects(filter, actor);
            var ids = projects.Select(p => p.ProjectId).ToList();

            var k = new PmoDashboardKpis { Cutoff = cutoff, TotalProjects = projects.Count };
            if (ids.Count == 0) return k;

            k.ActiveProjects = projects.Count(p => p.Status == "Active");

            // Weighted portfolio progress — single aggregate over all tasks in scope.
            var prog = Context.Queryable<PmoTask>().Where(t => ids.Contains(t.ProjectId))
                .Select(t => new { W = SqlFunc.AggregateSum(t.Weight), WP = SqlFunc.AggregateSum(t.Weight * t.ProgressPct) }).First();
            k.PortfolioProgress = prog != null && prog.W > 0 ? Math.Round(prog.WP / prog.W, 1) : 0;

            k.OverdueTasks = Context.Queryable<PmoTask>()
                .Count(t => ids.Contains(t.ProjectId) && t.DueDate != null && t.DueDate < cutoff && t.Status != "Done");
            k.BlockedTasks = Context.Queryable<PmoTask>()
                .Count(t => ids.Contains(t.ProjectId) && t.Status == "Blocked");
            k.MilestonesDue14 = Context.Queryable<PmoMilestone>()
                .Count(m => ids.Contains(m.ProjectId) && m.Status == "Pending" && m.DueDate != null && m.DueDate >= cutoff && m.DueDate <= due14);
            k.OpenHighExposureRisks = Context.Queryable<PmoRisk>()
                .Count(r => ids.Contains(r.ProjectId) && r.Status == "Open" && r.Score >= 12);
            k.PendingChangeRequests = Context.Queryable<PmoChangeRequest>()
                .Count(c => ids.Contains(c.ProjectId) && (c.Status == "Submitted" || c.Status == "UnderReview"));

            // Finance — per-project composition (no cross-join fan-out).
            var initialByProject = projects.ToDictionary(p => p.ProjectId, p => p.InitialApprovedBudget);
            var revByProject = Context.Queryable<PmoBudgetRevision>()
                .Where(r => ids.Contains(r.ProjectId) && r.Status == "Approved")
                .GroupBy(r => r.ProjectId).Select(r => new { r.ProjectId, Amt = SqlFunc.AggregateSum(r.Amount) })
                .ToList().ToDictionary(x => x.ProjectId, x => x.Amt);
            var actualByProject = Context.Queryable<PmoCostTransaction>()
                .Where(t => ids.Contains(t.ProjectId) && t.Status == "Posted" && t.TxnDate <= cutoff)
                .GroupBy(t => t.ProjectId).Select(t => new { t.ProjectId, Amt = SqlFunc.AggregateSum(t.Amount) })
                .ToList().ToDictionary(x => x.ProjectId, x => x.Amt);
            var ftcByProject = Context.Queryable<PmoFinancialForecast>()
                .Where(fc => ids.Contains(fc.ProjectId) && fc.Kind == "ForecastToComplete")
                .GroupBy(fc => fc.ProjectId).Select(fc => new { fc.ProjectId, Amt = SqlFunc.AggregateSum(fc.ForecastAmount) })
                .ToList().ToDictionary(x => x.ProjectId, x => x.Amt);

            decimal approved = 0, actual = 0, eac = 0, overrunAmt = 0;
            int overrunCount = 0;
            foreach (var id in ids)
            {
                var cur = initialByProject.GetValueOrDefault(id) + revByProject.GetValueOrDefault(id);
                var act = actualByProject.GetValueOrDefault(id);
                var projEac = act + ftcByProject.GetValueOrDefault(id);
                approved += cur; actual += act; eac += projEac;
                if (projEac > cur) { overrunCount++; overrunAmt += projEac - cur; }
            }
            k.ApprovedBudget = approved;
            k.ActualSpend = actual;
            k.RemainingBudget = approved - actual;
            k.UtilizationPct = approved > 0 ? Math.Round(actual / approved * 100m, 1) : (decimal?)null;
            k.Eac = eac;
            k.ProjectedOverrunCount = overrunCount;
            k.ProjectedOverrunAmount = overrunAmt;
            return k;
        }

        public PmoDashboardAnalytics GetAnalytics(PmoDashboardFilter filter, PmoActor actor)
        {
            var cutoff = filter.Cutoff ?? DateTime.Today;
            var projects = FilteredProjects(filter, actor);
            var ids = projects.Select(p => p.ProjectId).ToList();
            var a = new PmoDashboardAnalytics();
            if (ids.Count == 0) return a;

            // Per-project weighted progress and finance for health + budget-vs-actual.
            var progByProject = Context.Queryable<PmoTask>().Where(t => ids.Contains(t.ProjectId))
                .GroupBy(t => t.ProjectId)
                .Select(t => new { t.ProjectId, W = SqlFunc.AggregateSum(t.Weight), WP = SqlFunc.AggregateSum(t.Weight * t.ProgressPct) })
                .ToList().ToDictionary(x => x.ProjectId, x => x);
            var revByProject = Context.Queryable<PmoBudgetRevision>()
                .Where(r => ids.Contains(r.ProjectId) && r.Status == "Approved")
                .GroupBy(r => r.ProjectId).Select(r => new { r.ProjectId, Amt = SqlFunc.AggregateSum(r.Amount) })
                .ToList().ToDictionary(x => x.ProjectId, x => x.Amt);
            var actualByProject = Context.Queryable<PmoCostTransaction>()
                .Where(t => ids.Contains(t.ProjectId) && t.Status == "Posted" && t.TxnDate <= cutoff)
                .GroupBy(t => t.ProjectId).Select(t => new { t.ProjectId, Amt = SqlFunc.AggregateSum(t.Amount) })
                .ToList().ToDictionary(x => x.ProjectId, x => x.Amt);
            var ftcByProject = Context.Queryable<PmoFinancialForecast>()
                .Where(fc => ids.Contains(fc.ProjectId) && fc.Kind == "ForecastToComplete")
                .GroupBy(fc => fc.ProjectId).Select(fc => new { fc.ProjectId, Amt = SqlFunc.AggregateSum(fc.ForecastAmount) })
                .ToList().ToDictionary(x => x.ProjectId, x => x.Amt);

            foreach (var p in projects.OrderBy(p => p.Name))
            {
                var cur = p.InitialApprovedBudget + revByProject.GetValueOrDefault(p.ProjectId);
                var act = actualByProject.GetValueOrDefault(p.ProjectId);
                var eac = act + ftcByProject.GetValueOrDefault(p.ProjectId);
                decimal prog = 0;
                if (progByProject.TryGetValue(p.ProjectId, out var pr) && pr.W > 0) prog = Math.Round(pr.WP / pr.W, 1);

                a.ProjectHealth.Add(new PmoProjectHealthRow
                {
                    ProjectId = p.ProjectId, ProjectName = p.Name, Status = p.Status, Progress = prog,
                    UtilizationPct = cur > 0 ? Math.Round(act / cur * 100m, 1) : (decimal?)null,
                    IsProjectedOverrun = eac > cur
                });
                a.BudgetVsActual.Add(new PmoBudgetVsActualRow
                {
                    ProjectId = p.ProjectId, ProjectName = p.Name, ApprovedBudget = cur, ActualSpend = act, Eac = eac
                });
            }

            // Monthly trend (portfolio) — posted rows from SQL, bucketed by month.
            var posted = Context.Queryable<PmoCostTransaction>()
                .Where(t => ids.Contains(t.ProjectId) && t.Status == "Posted" && t.TxnDate <= cutoff)
                .Select(t => new { t.TxnDate, t.Amount, t.CategoryId }).ToList();
            a.MonthlyTrend = posted.GroupBy(t => t.TxnDate.ToString("yyyy-MM"))
                .Select(g => new PmoMonthlySpendDto { Period = g.Key, Amount = g.Sum(x => x.Amount) })
                .OrderBy(m => m.Period).ToList();

            // Spend by category (portfolio).
            var catAgg = posted.GroupBy(t => t.CategoryId).Select(g => new { CategoryId = g.Key, Amount = g.Sum(x => x.Amount) }).ToList();
            if (catAgg.Count > 0)
            {
                var catIds = catAgg.Select(c => c.CategoryId).ToList();
                var catNames = Context.Queryable<PmoCostCategory>().Where(c => catIds.Contains(c.CategoryId))
                    .ToList().ToDictionary(c => c.CategoryId, c => c.Name);
                a.SpendByCategory = catAgg.Select(c => new PmoCategorySpendDto
                {
                    CategoryName = catNames.TryGetValue(c.CategoryId, out var n) ? n : "Uncategorised",
                    Amount = c.Amount
                }).OrderByDescending(c => c.Amount).ToList();
            }

            // Risk matrix — open risks bucketed by probability × impact.
            a.RiskMatrix = Context.Queryable<PmoRisk>()
                .Where(r => ids.Contains(r.ProjectId) && r.Status == "Open")
                .GroupBy(r => new { r.Probability, r.Impact })
                .Select(r => new PmoRiskMatrixCell { Probability = r.Probability, Impact = r.Impact, Count = SqlFunc.AggregateCount(r.RiskId) })
                .ToList();

            // Overdue work by owner (assignee).
            var overdue = Context.Queryable<PmoTask>()
                .Where(t => ids.Contains(t.ProjectId) && t.DueDate != null && t.DueDate < cutoff && t.Status != "Done")
                .Select(t => new { t.AssigneeUserId }).ToList();
            var ownerIds = overdue.Where(o => o.AssigneeUserId != null).Select(o => o.AssigneeUserId.Value).Distinct().ToList();
            var ownerNames = Context.Queryable<SysUser>().Where(u => ownerIds.Contains(u.UserId))
                .ToList().ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);
            a.OverdueByOwner = overdue.GroupBy(o => o.AssigneeUserId)
                .Select(g => new PmoOwnerOverdueRow
                {
                    OwnerName = g.Key == null ? "Unassigned" : (ownerNames.TryGetValue(g.Key.Value, out var n) ? n : "User " + g.Key),
                    OverdueCount = g.Count()
                }).OrderByDescending(x => x.OverdueCount).ToList();

            // Milestone roadmap — upcoming pending milestones.
            var projNames = projects.ToDictionary(p => p.ProjectId, p => p.Name);
            a.MilestoneRoadmap = Context.Queryable<PmoMilestone>()
                .Where(m => ids.Contains(m.ProjectId) && m.DueDate != null && m.DueDate >= cutoff)
                .OrderBy(m => m.DueDate).Take(20).ToList()
                .Select(m => new PmoMilestoneRoadmapRow
                {
                    ProjectId = m.ProjectId,
                    ProjectName = projNames.GetValueOrDefault(m.ProjectId),
                    Name = m.Name, DueDate = m.DueDate, Status = m.Status
                }).ToList();

            return a;
        }

        public List<PmoTask> DrilldownTasks(PmoDashboardFilter filter, string mode, PmoActor actor)
        {
            var cutoff = filter.Cutoff ?? DateTime.Today;
            var ids = FilteredProjects(filter, actor).Select(p => p.ProjectId).ToList();
            if (ids.Count == 0) return new List<PmoTask>();

            var exp = Expressionable.Create<PmoTask>();
            exp.And(t => ids.Contains(t.ProjectId));
            if (mode == "overdue") exp.And(t => t.DueDate != null && t.DueDate < cutoff && t.Status != "Done");
            else if (mode == "blocked") exp.And(t => t.Status == "Blocked");

            var list = Context.Queryable<PmoTask>().Where(exp.ToExpression())
                .OrderBy(t => t.DueDate).ToList();

            var projNames = Context.Queryable<PmoProject>().Where(p => ids.Contains(p.ProjectId))
                .ToList().ToDictionary(p => p.ProjectId, p => p.Name);
            var assigneeIds = list.Where(t => t.AssigneeUserId != null).Select(t => t.AssigneeUserId.Value).Distinct().ToList();
            var names = Context.Queryable<SysUser>().Where(u => assigneeIds.Contains(u.UserId))
                .ToList().ToDictionary(u => u.UserId, u => string.IsNullOrEmpty(u.NickName) ? u.UserName : u.NickName);
            foreach (var t in list)
            {
                t.ProjectName = projNames.GetValueOrDefault(t.ProjectId);
                t.IsOverdue = t.DueDate != null && t.DueDate < cutoff && t.Status != "Done";
                if (t.AssigneeUserId != null) t.AssigneeName = names.GetValueOrDefault(t.AssigneeUserId.Value);
            }
            return list;
        }

        public List<PmoRisk> DrilldownRisks(PmoDashboardFilter filter, bool highExposureOnly, PmoActor actor)
        {
            var ids = FilteredProjects(filter, actor).Select(p => p.ProjectId).ToList();
            if (ids.Count == 0) return new List<PmoRisk>();

            var exp = Expressionable.Create<PmoRisk>();
            exp.And(r => ids.Contains(r.ProjectId));
            if (highExposureOnly) exp.And(r => r.Status == "Open" && r.Score >= 12);

            var list = Context.Queryable<PmoRisk>().Where(exp.ToExpression())
                .OrderBy(r => r.Score, OrderByType.Desc).ToList();
            var projNames = Context.Queryable<PmoProject>().Where(p => ids.Contains(p.ProjectId))
                .ToList().ToDictionary(p => p.ProjectId, p => p.Name);
            foreach (var r in list) r.ProjectName = projNames.GetValueOrDefault(r.ProjectId);
            return list;
        }
    }
}
