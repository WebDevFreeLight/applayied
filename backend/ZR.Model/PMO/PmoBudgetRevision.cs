using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Dated budget revision. Only Approved revisions affect the current approved budget:
    /// current approved budget = project.initial_approved_budget + Σ(approved revision amounts).
    /// </summary>
    [SugarTable("pmo_budget_revision", "PMO Budget Revision")]
    [SugarIndex("idx_pmo_rev_project", nameof(ProjectId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_rev_status", nameof(Status), OrderByType.Asc)]
    public class PmoBudgetRevision : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "revision_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RevisionId { get; set; }

        [SugarColumn(ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        /// <summary>Signed delta to the approved budget (may be negative).</summary>
        [SugarColumn(ColumnName = "amount", Length = 18, DecimalDigits = 2)]
        public decimal Amount { get; set; }

        [SugarColumn(ColumnName = "reason", Length = 1000)]
        public string Reason { get; set; }

        /// <summary>Pending | Approved | Rejected</summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "Pending";

        [SugarColumn(ColumnName = "requested_by_user_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequestedByUserId { get; set; }

        [SugarColumn(ColumnName = "approved_by_user_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApprovedByUserId { get; set; }

        [SugarColumn(ColumnName = "approved_at", IsNullable = true)]
        public DateTime? ApprovedAt { get; set; }

        [SugarColumn(IsIgnore = true)] public string ProjectName { get; set; }
    }
}
