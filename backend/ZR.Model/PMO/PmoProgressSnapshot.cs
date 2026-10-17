using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Historical progress snapshot per project (derived values captured at a point in time).
    /// Source for the monthly progress trend; distinct from live derived KPIs.
    /// </summary>
    [SugarTable("pmo_progress_snapshot", "PMO Progress Snapshot")]
    [SugarIndex("idx_pmo_snapshot_project", nameof(ProjectId), OrderByType.Asc)]
    public class PmoProgressSnapshot : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "snapshot_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SnapshotId { get; set; }

        [SugarColumn(ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        [SugarColumn(ColumnName = "snapshot_date")]
        public DateTime SnapshotDate { get; set; }

        [SugarColumn(ColumnName = "progress_pct")]
        public int ProgressPct { get; set; }

        [SugarColumn(ColumnName = "tasks_total")]
        public int TasksTotal { get; set; }

        [SugarColumn(ColumnName = "tasks_done")]
        public int TasksDone { get; set; }

        [SugarColumn(ColumnName = "open_risk_count")]
        public int OpenRiskCount { get; set; }
    }
}
