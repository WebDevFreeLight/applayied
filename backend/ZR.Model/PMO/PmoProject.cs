using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// PMO Project — the root of the delivery/finance graph.
    /// </summary>
    [SugarTable("pmo_project", "PMO Project")]
    [SugarIndex("uk_pmo_project_code", nameof(Code), OrderByType.Asc, true)]
    [SugarIndex("idx_pmo_project_dept", nameof(DeptId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_project_owner", nameof(OwnerUserId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_project_status", nameof(Status), OrderByType.Asc)]
    public class PmoProject : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        /// <summary>Human-readable unique project code, e.g. PRJ-001</summary>
        [SugarColumn(ColumnName = "code", Length = 32)]
        public string Code { get; set; }

        [SugarColumn(ColumnName = "name", Length = 200)]
        public string Name { get; set; }

        /// <summary>Owning department (sys_dept.dept_id)</summary>
        [SugarColumn(ColumnName = "dept_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DeptId { get; set; }

        /// <summary>Project owner (sys_user.user_id)</summary>
        [SugarColumn(ColumnName = "owner_user_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OwnerUserId { get; set; }

        /// <summary>Planning | Active | OnHold | Completed | Cancelled</summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "Planning";

        /// <summary>Low | Medium | High | Critical</summary>
        [SugarColumn(ColumnName = "priority", Length = 20)]
        public string Priority { get; set; } = "Medium";

        [SugarColumn(ColumnName = "start_date", IsNullable = true)]
        public DateTime? StartDate { get; set; }

        [SugarColumn(ColumnName = "planned_end_date", IsNullable = true)]
        public DateTime? PlannedEndDate { get; set; }

        /// <summary>Set only when the project is Completed/Cancelled.</summary>
        [SugarColumn(ColumnName = "actual_end_date", IsNullable = true)]
        public DateTime? ActualEndDate { get; set; }

        [SugarColumn(ColumnName = "description", Length = 2000, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>Initial approved budget. Current approved budget = this + approved revisions.</summary>
        [SugarColumn(ColumnName = "initial_approved_budget", Length = 18, DecimalDigits = 2)]
        public decimal InitialApprovedBudget { get; set; }

        [SugarColumn(ColumnName = "currency", Length = 8)]
        public string Currency { get; set; } = "AED";

        // --- Derived (never persisted) — populated by the service for list/detail responses ---
        [SugarColumn(IsIgnore = true)] public string OwnerName { get; set; }
        [SugarColumn(IsIgnore = true)] public string DeptName { get; set; }
        [SugarColumn(IsIgnore = true)] public int TaskCount { get; set; }
        [SugarColumn(IsIgnore = true)] public decimal ProgressPct { get; set; }
    }
}
