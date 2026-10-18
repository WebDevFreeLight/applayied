using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Project change request moving through a controlled approval workflow.
    /// Permitted transitions and self-approval prevention are enforced server-side.
    /// </summary>
    [SugarTable("pmo_change_request", "PMO Change Request")]
    [SugarIndex("idx_pmo_cr_project", nameof(ProjectId), OrderByType.Asc)]
    [SugarIndex("idx_pmo_cr_status", nameof(Status), OrderByType.Asc)]
    public class PmoChangeRequest : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "change_request_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ChangeRequestId { get; set; }

        [SugarColumn(ColumnName = "project_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProjectId { get; set; }

        [SugarColumn(ColumnName = "title", Length = 200)]
        public string Title { get; set; }

        [SugarColumn(ColumnName = "description", Length = 2000, IsNullable = true)]
        public string Description { get; set; }

        [SugarColumn(ColumnName = "requested_by_user_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequestedByUserId { get; set; }

        /// <summary>Draft | Submitted | UnderReview | Approved | Rejected | Returned</summary>
        [SugarColumn(ColumnName = "status", Length = 20)]
        public string Status { get; set; } = "Draft";

        [SugarColumn(ColumnName = "decision_by_user_id", IsNullable = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DecisionByUserId { get; set; }

        [SugarColumn(ColumnName = "decision_reason", Length = 1000, IsNullable = true)]
        public string DecisionReason { get; set; }

        [SugarColumn(ColumnName = "decision_at", IsNullable = true)]
        public DateTime? DecisionAt { get; set; }

        /// <summary>Estimated cost impact (informational; does NOT auto-post spend or budget).</summary>
        [SugarColumn(ColumnName = "estimated_cost_impact", Length = 18, DecimalDigits = 2, IsNullable = true)]
        public decimal? EstimatedCostImpact { get; set; }

        /// <summary>Optimistic-concurrency token to prevent double-submission of decisions.</summary>
        // Optimistic-concurrency token, incremented explicitly on each decision in the service. The
        // workflow's primary double-submission guard is the state machine (no edge out of a terminal state).
        [SugarColumn(ColumnName = "version", IsNullable = true)]
        public long? Version { get; set; }

        [SugarColumn(IsIgnore = true)] public string RequestedByName { get; set; }
        [SugarColumn(IsIgnore = true)] public string DecisionByName { get; set; }
        [SugarColumn(IsIgnore = true)] public string ProjectName { get; set; }
    }
}
