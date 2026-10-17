using Infrastructure.Converter;
using ZR.Model.System;

namespace ZR.Model.PMO
{
    /// <summary>
    /// Generic durable audit event for governance/finance actions (e.g. cost posting, budget-revision
    /// approval). Written in the SAME DB transaction as the action it records, so the state change and
    /// its audit trail commit atomically (or both roll back).
    /// </summary>
    [SugarTable("pmo_audit_event", "PMO Audit Event")]
    [SugarIndex("idx_pmo_audit_entity", nameof(EntityType), OrderByType.Asc, nameof(EntityId), OrderByType.Asc)]
    public class PmoAuditEvent : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "audit_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AuditId { get; set; }

        /// <summary>e.g. CostTransaction | BudgetRevision | ChangeRequest</summary>
        [SugarColumn(ColumnName = "entity_type", Length = 40)]
        public string EntityType { get; set; }

        [SugarColumn(ColumnName = "entity_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EntityId { get; set; }

        /// <summary>e.g. Posted | Voided | RevisionApproved | RevisionRejected</summary>
        [SugarColumn(ColumnName = "action", Length = 40)]
        public string Action { get; set; }

        [SugarColumn(ColumnName = "actor_user_id")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ActorUserId { get; set; }

        [SugarColumn(ColumnName = "detail", Length = 1000, IsNullable = true)]
        public string Detail { get; set; }

        [SugarColumn(ColumnName = "acted_at")]
        public DateTime ActedAt { get; set; }

        [SugarColumn(IsIgnore = true)] public string ActorName { get; set; }
    }
}
