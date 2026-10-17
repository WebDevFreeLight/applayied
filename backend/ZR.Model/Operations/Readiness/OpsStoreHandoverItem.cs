using System;

namespace ZR.Model.Operations.Readiness
{
    /// <summary>
    /// Store Handover Item - بند تسليم المحل (per-requirement per-store tracking)
    /// </summary>
    [SugarTable("ops_store_handover_item")]
    public class OpsStoreHandoverItem : SysBase
    {
        /// <summary>
        /// Item ID (Snowflake)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "item_id", IsPrimaryKey = true, IsIdentity = false)]
        public long ItemId { get; set; }

        /// <summary>
        /// FK → ops_store_handover_v2
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "handover_id", IsNullable = false)]
        public long HandoverId { get; set; }

        /// <summary>
        /// FK → ops_handover_requirement
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "requirement_id", IsNullable = false)]
        public long RequirementId { get; set; }

        /// <summary>
        /// Whether this requirement is completed
        /// </summary>
        [SugarColumn(ColumnName = "is_completed", IsNullable = false)]
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Photo URLs (comma-separated)
        /// </summary>
        [SugarColumn(ColumnName = "photo_urls", ColumnDataType = "TEXT", IsNullable = true)]
        public string PhotoUrls { get; set; }

        /// <summary>
        /// When the item was completed
        /// </summary>
        [SugarColumn(ColumnName = "completed_at", IsNullable = true)]
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Who completed the item
        /// </summary>
        [SugarColumn(ColumnName = "completed_by", Length = 100, IsNullable = true)]
        public string CompletedBy { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        [SugarColumn(ColumnName = "notes", ColumnDataType = "TEXT", IsNullable = true)]
        public string Notes { get; set; }

        // ─── Navigation Properties (from requirement) ───

        [SugarColumn(IsIgnore = true)]
        public string NameEn { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string NameAr { get; set; }

        [SugarColumn(IsIgnore = true)]
        public decimal Percentage { get; set; }

        [SugarColumn(IsIgnore = true)]
        public bool PhotoRequired { get; set; }

        [SugarColumn(IsIgnore = true)]
        public bool IsBlocking { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string Code { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int SortOrder { get; set; }
    }
}
