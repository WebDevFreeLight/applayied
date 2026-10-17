using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Delivery task. Progress drives project completion % and the executive dashboard.
    /// </summary>
    [SugarTable("pmo_task", "PMO Task")]
    [SugarIndex("idx_pmo_task_project", nameof(ProjectId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_task_assignee", nameof(AssigneeUserId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_task_status", nameof(Status), OrderByType.Asc)]
    [SugarIndex("idx_pmo_task_due", nameof(DueDate), OrderByType.Asc)]
    public class PmoTask : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "task_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }

        [SugarColumn(ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        [SugarColumn(ColumnName = "title", Length = 200)]
        public string Title { get; set; }

        /// <summary>Nullable — supports the required "unassigned task" edge case.</summary>
        [SugarColumn(ColumnName = "assignee_user_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssigneeUserId { get; set; }

        /// <summary>ToDo | InProgress | Blocked | Done</summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "ToDo";

        /// <summary>Low | Medium | High | Critical</summary>
        [SugarColumn(ColumnName = "priority", Length = 20)]
        public string Priority { get; set; } = "Medium";

        [SugarColumn(ColumnName = "start_date", IsNullable = true)]
        public DateTime? StartDate { get; set; }

        [SugarColumn(ColumnName = "due_date", IsNullable = true)]
        public DateTime? DueDate { get; set; }

        /// <summary>Relative weight used in weighted project progress.</summary>
        [SugarColumn(ColumnName = "weight", Length = 6, DecimalDigits = 2)]
        public decimal Weight { get; set; } = 1;

        /// <summary>0–100.</summary>
        [SugarColumn(ColumnName = "progress_pct")]
        public int ProgressPct { get; set; }

        /// <summary>Optimistic-concurrency token (SqlSugar version validation).</summary>
        [SugarColumn(ColumnName = "version", IsEnableUpdateVersionValidation = true, IsNullable = true)]
        public long? Version { get; set; }

        [SugarColumn(IsIgnore = true)] public string AssigneeName { get; set; }
        [SugarColumn(IsIgnore = true)] public string ProjectName { get; set; }
        /// <summary>Derived: due_date &lt; cutoff and status != Done.</summary>
        [SugarColumn(IsIgnore = true)] public bool IsOverdue { get; set; }
    }
}
