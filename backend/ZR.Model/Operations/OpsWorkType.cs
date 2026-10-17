using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Work Type Lookup - نوع العمل
    /// </summary>
    [SugarTable("ops_work_type")]
    public class OpsWorkType : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "work_type_id", IsPrimaryKey = true, IsIdentity = false)]
        public long WorkTypeId { get; set; }

        /// <summary>
        /// Short code (CIVIL, MECH, etc.)
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
