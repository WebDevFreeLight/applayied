using System;

namespace ZR.Model.Operations.Readiness
{
    /// <summary>
    /// Handover Requirement - متطلبات التسليم (flat master list, each with a fixed percentage)
    /// </summary>
    [SugarTable("ops_handover_requirement")]
    public class OpsHandoverRequirement : SysBase
    {
        /// <summary>
        /// Requirement ID (Snowflake)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "requirement_id", IsPrimaryKey = true, IsIdentity = false)]
        public long RequirementId { get; set; }

        /// <summary>
        /// Unique code e.g. "ELEC-01"
        /// </summary>
        [SugarColumn(ColumnName = "code", Length = 50, IsNullable = false)]
        public string Code { get; set; }

        /// <summary>
        /// English name
        /// </summary>
        [SugarColumn(ColumnName = "name_en", Length = 300, IsNullable = false)]
        public string NameEn { get; set; }

        /// <summary>
        /// Arabic name
        /// </summary>
        [SugarColumn(ColumnName = "name_ar", Length = 300, IsNullable = false)]
        public string NameAr { get; set; }

        /// <summary>
        /// English description
        /// </summary>
        [SugarColumn(ColumnName = "description_en", Length = 1000, IsNullable = true)]
        public string DescriptionEn { get; set; }

        /// <summary>
        /// Arabic description
        /// </summary>
        [SugarColumn(ColumnName = "description_ar", Length = 1000, IsNullable = true)]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Fixed percentage contribution (e.g. 5.0 means 5%). All active requirements must sum to 100
        /// </summary>
        [SugarColumn(ColumnName = "percentage", DecimalDigits = 2, IsNullable = false)]
        public decimal Percentage { get; set; }

        /// <summary>
        /// Must upload photo to mark complete
        /// </summary>
        [SugarColumn(ColumnName = "photo_required", IsNullable = false)]
        public bool PhotoRequired { get; set; }

        /// <summary>
        /// Blocks handover if incomplete
        /// </summary>
        [SugarColumn(ColumnName = "is_blocking", IsNullable = false)]
        public bool IsBlocking { get; set; }

        /// <summary>
        /// Display sort order
        /// </summary>
        [SugarColumn(ColumnName = "sort_order", IsNullable = false)]
        public int SortOrder { get; set; }

        /// <summary>
        /// Soft delete flag (default true)
        /// </summary>
        [SugarColumn(ColumnName = "is_active", IsNullable = false)]
        public bool IsActive { get; set; } = true;
    }
}
