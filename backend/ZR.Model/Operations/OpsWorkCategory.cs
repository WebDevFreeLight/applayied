using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Work Category Lookup - تصنيف العمل
    /// </summary>
    [SugarTable("ops_work_category")]
    public class OpsWorkCategory : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "work_category_id", IsPrimaryKey = true, IsIdentity = false)]
        public long WorkCategoryId { get; set; }

        /// <summary>
        /// Short code (HOT, COLD, etc.)
        /// </summary>
        [SugarColumn(ColumnName = "code", Length = 20)]
        public string Code { get; set; }

        /// <summary>
        /// Display name (EN)
        /// </summary>
        [SugarColumn(ColumnName = "name", Length = 100)]
        public string Name { get; set; }

        /// <summary>
        /// Display name (AR)
        /// </summary>
        [SugarColumn(ColumnName = "name_ar", Length = 100, IsNullable = true)]
        public string NameAr { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        [SugarColumn(ColumnName = "display_order", IsNullable = true)]
        public int? DisplayOrder { get; set; }

        /// <summary>
        /// Active flag
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;
    }
}
