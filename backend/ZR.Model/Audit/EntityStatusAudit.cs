using System;

namespace ZR.Model.Audit
{
    [SugarTable("entity_status_audit")]
    public class EntityStatusAudit
    {
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }

        [SugarColumn(ColumnName = "entity_type", Length = 50)]
        public string EntityType { get; set; }

        [SugarColumn(ColumnName = "entity_id", Length = 100)]
        public string EntityId { get; set; }

        [SugarColumn(ColumnName = "old_status", Length = 50, IsNullable = true)]
        public string OldStatus { get; set; }

        [SugarColumn(ColumnName = "new_status", Length = 50)]
        public string NewStatus { get; set; }

        [SugarColumn(ColumnName = "notes", ColumnDataType = "text", IsNullable = true)]
        public string Notes { get; set; }

        [SugarColumn(ColumnName = "changed_by", Length = 64)]
        public string ChangedBy { get; set; }

        [SugarColumn(ColumnName = "changed_at")]
        public DateTime ChangedAt { get; set; }
    }
}
