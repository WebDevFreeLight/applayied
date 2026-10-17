using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Safety Precaution Template - احتياطات السلامة
    /// </summary>
    [SugarTable("ops_safety_precaution")]
    public class OpsSafetyPrecaution : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "precaution_id", IsPrimaryKey = true, IsIdentity = false)]
        public long PrecautionId { get; set; }

        /// <summary>
        /// Short code (PPE, FIRE_EXT, etc.)
        /// </summary>
        [SugarColumn(ColumnName = "code", Length = 20)]
        public string Code { get; set; }

        /// <summary>
        /// Description (EN)
        /// </summary>
        [SugarColumn(ColumnName = "description", Length = 500)]
        public string Description { get; set; }

        /// <summary>
        /// Description (AR)
        /// </summary>
        [SugarColumn(ColumnName = "description_ar", Length = 500, IsNullable = true)]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        [SugarColumn(ColumnName = "display_order")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Active flag
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;
    }
}
