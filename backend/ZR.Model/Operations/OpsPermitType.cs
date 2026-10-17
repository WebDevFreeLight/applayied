using System;

namespace ZR.Model.Operations
{
    /// <summary>
    /// Permit Type Definitions - أنواع التصاريح
    /// </summary>
    [SugarTable("ops_permit_type")]
    public class OpsPermitType : SysBase
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "permit_type_id", IsPrimaryKey = true, IsIdentity = false)]
        public long PermitTypeId { get; set; }

        /// <summary>
        /// Unique code (e.g., "DAILY", "TEMP", "VEH")
        /// </summary>
        [SugarColumn(ColumnName = "type_code", Length = 20)]
        public string TypeCode { get; set; }

        /// <summary>
        /// Name (EN)
        /// </summary>
        [SugarColumn(ColumnName = "type_name", Length = 100)]
        public string TypeName { get; set; }

        /// <summary>
        /// Name (AR)
        /// </summary>
        [SugarColumn(ColumnName = "type_name_ar", Length = 100, IsNullable = true)]
        public string TypeNameAr { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [SugarColumn(ColumnName = "description", Length = 500, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// Icon identifier
        /// </summary>
        [SugarColumn(ColumnName = "icon", Length = 50, IsNullable = true)]
        public string Icon { get; set; }

        /// <summary>
        /// Display color
        /// </summary>
        [SugarColumn(ColumnName = "color_code", Length = 20, IsNullable = true)]
        public string ColorCode { get; set; }

        /// <summary>
        /// Whether approval workflow is needed
        /// </summary>
        [SugarColumn(ColumnName = "requires_approval")]
        public bool RequiresApproval { get; set; } = true;

        /// <summary>
        /// Number of approval levels (1-3)
        /// </summary>
        [SugarColumn(ColumnName = "approval_level")]
        public int ApprovalLevel { get; set; } = 1;

        /// <summary>
        /// Default validity period in days
        /// </summary>
        [SugarColumn(ColumnName = "default_validity_days", IsNullable = true)]
        public int? DefaultValidityDays { get; set; }

        /// <summary>
        /// Whether vehicle info is required
        /// </summary>
        [SugarColumn(ColumnName = "requires_vehicle_info")]
        public bool RequiresVehicleInfo { get; set; } = false;

        /// <summary>
        /// Whether photo is required
        /// </summary>
        [SugarColumn(ColumnName = "requires_photo")]
        public bool RequiresPhoto { get; set; } = false;

        /// <summary>
        /// Max permits of this type
        /// </summary>
        [SugarColumn(ColumnName = "max_quantity", IsNullable = true)]
        public int? MaxQuantity { get; set; }

        /// <summary>
        /// Fee for this permit type
        /// </summary>
        [SugarColumn(ColumnName = "price", IsNullable = true, DecimalDigits = 2)]
        public decimal? Price { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        [SugarColumn(ColumnName = "display_order", IsNullable = true)]
        public int? DisplayOrder { get; set; }

        /// <summary>
        /// Active/inactive
        /// </summary>
        [SugarColumn(ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;
    }
}
