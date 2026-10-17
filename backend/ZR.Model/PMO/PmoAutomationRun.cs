using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Execution log for the background automation worker. dedupe_key is UNIQUE so a rule
    /// cannot fire twice for the same logical event (idempotency / no double-run).
    /// </summary>
    [SugarTable("pmo_automation_run", "PMO Automation Run")]
    [SugarIndex("uk_pmo_autorun_dedupe", nameof(DedupeKey), OrderByType.Asc, true)]
    public class PmoAutomationRun : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "run_id")]
        public long RunId { get; set; }

        /// <summary>e.g. overdue-escalation | overrun-alert | month-end-summary</summary>
        [SugarColumn(ColumnName = "rule_key", Length = 64)]
        public string RuleKey { get; set; }

        [SugarColumn(ColumnName = "triggered_at")]
        public DateTime TriggeredAt { get; set; }

        /// <summary>Success | Failed | Skipped</summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; }

        [SugarColumn(ColumnName = "detail", Length = 2000, IsNullable = true)]
        public string Detail { get; set; }

        /// <summary>Unique idempotency key (e.g. rulekey:yyyyMMdd:entityId).</summary>
        [SugarColumn(ColumnName = "dedupe_key", Length = 128)]
        public string DedupeKey { get; set; }
    }
}
