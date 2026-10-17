using System;
using System.Collections.Generic;
using System.Linq;
using SqlSugar;
using ZR.Model.Contracts.Enums;
using ZR.Model.System;

namespace ZR.Model.Contracts
{
    /// <summary>
    /// Contract Unit - وحدة العقد
    /// Represents a physical unit, property, or item included in a contract
    /// Examples: Office space, retail shop, parking spot, equipment, vehicle
    /// </summary>
    [SugarTable("erp_contract_unit")]
    [SugarIndex("IX_contract_unit_contract", nameof(ContractId), OrderByType.Asc)]
    [SugarIndex("IX_contract_unit_code", nameof(UnitCode), OrderByType.Asc)]
    [SugarIndex("UX_contract_unit_unique", nameof(ContractId), OrderByType.Asc, nameof(UnitCode), OrderByType.Asc, true)]
    public class ErpContractUnit : SysBase, ISoftDelete
    {
        /// <summary>
        /// Unit ID - معرف الوحدة
        /// Primary key (GUID)
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "unit_id")]
        public Guid UnitId { get; set; }

        /// <summary>
        /// Contract ID - معرف العقد
        /// Foreign key to ErpContract
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "contract_id")]
        public Guid ContractId { get; set; }

        /// <summary>
        /// Unit Type - نوع الوحدة
        /// Type of the unit (Office, RetailShop, Warehouse, etc.)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "unit_type")]
        public ContractUnitType UnitType { get; set; }

        /// <summary>
        /// Unit Code - رمز الوحدة
        /// Unique identifier/code for the unit (e.g., "SHOP-A101", "PKG-B25")
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "unit_code")]
        public string UnitCode { get; set; }

        /// <summary>
        /// Unit Name - اسم الوحدة
        /// Descriptive name for the unit
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "unit_name")]
        public string UnitName { get; set; }

        /// <summary>
        /// Unit Name (Arabic) - اسم الوحدة بالعربية
        /// Arabic name for bilingual support
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "unit_name_ar")]
        public string UnitNameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// Detailed description of the unit
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Location/Address - الموقع/العنوان
        /// Physical location or address of the unit
        /// </summary>
        [SugarColumn(Length = 300, IsNullable = true, ColumnName = "location")]
        public string Location { get; set; }

        /// <summary>
        /// Building - المبنى
        /// Building name or number
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "building")]
        public string Building { get; set; }

        /// <summary>
        /// Floor - الطابق
        /// Floor number
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "floor")]
        public string Floor { get; set; }

        /// <summary>
        /// Area (Square Meters) - المساحة (متر مربع)
        /// Total area of the unit in square meters
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "area")]
        public decimal? Area { get; set; }

        /// <summary>
        /// Area Unit - وحدة المساحة
        /// Unit of measurement for area (sqm, sqft, etc.)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "area_unit")]
        public string AreaUnit { get; set; } = "sqm";

        /// <summary>
        /// Base Price - السعر الأساسي
        /// Base price for this unit per billing period
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "base_price")]
        public decimal? BasePrice { get; set; }

        /// <summary>
        /// Price Per Unit Area - السعر لكل وحدة مساحة
        /// Price per square meter/foot (for area-based pricing)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "price_per_area")]
        public decimal? PricePerArea { get; set; }

        /// <summary>
        /// Calculated Price - السعر المحسوب
        /// Calculated price based on area × price per area
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = true, ColumnName = "calculated_price")]
        public decimal? CalculatedPrice { get; set; }

        /// <summary>
        /// Currency - العملة
        /// Currency for pricing (inherits from contract if null)
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// Whether this unit is currently active in the contract
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Start Date - تاريخ البداية
        /// When this unit becomes active in the contract (null = contract start)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "start_date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ النهاية
        /// When this unit ends in the contract (null = contract end)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "end_date")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// External Reference ID - معرف المرجع الخارجي
        /// Reference to external property/asset management system
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "external_ref_id")]
        public Guid? ExternalRefId { get; set; }

        /// <summary>
        /// External Reference Code - رمز المرجع الخارجي
        /// External system reference code
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "external_ref_code")]
        public string ExternalRefCode { get; set; }

        /// <summary>
        /// Sequence Number - رقم التسلسل
        /// Display order within the contract
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "sequence_number")]
        public int SequenceNumber { get; set; } = 0;

        /// <summary>
        /// Notes - ملاحظات
        /// Additional notes about the unit
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        [SugarColumn(IsNullable = false, ColumnName = "is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [SugarColumn(IsNullable = true, ColumnName = "deleted_at")]
        public DateTime? DeletedAt { get; set; }

        #region Shop & Pricing Fields - حقول المحل والتسعير

        /// <summary>
        /// Boulevard Shop ID - معرف محل البوليفارد
        /// FK to blv_boulevard_shops. Links this contract unit to the master shop record.
        /// Nullable for non-Boulevard units (equipment, vehicles, services).
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "blv_shop_id")]
        public long? BlvShopId { get; set; }

        /// <summary>
        /// Pricing Type - نوع التسعير
        /// Fixed or RevenueShare. This is the contract-level financial truth.
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "pricing_type")]
        public PricingType PricingType { get; set; } = PricingType.Fixed;

        /// <summary>
        /// Fixed Rent Amount - مبلغ الإيجار الثابت
        /// Per billing period. Required when PricingType = Fixed.
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "fixed_rent_amount")]
        public decimal? FixedRentAmount { get; set; }

        /// <summary>
        /// Revenue Percentage - نسبة الإيرادات
        /// 0-100. Required when PricingType = RevenueShare.
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "revenue_percentage")]
        public decimal? RevenuePercentage { get; set; }

        /// <summary>
        /// Minimum Guarantee - الحد الأدنى المضمون
        /// Floor amount for RevenueShare. Invoice = MAX(calculated, this value).
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "minimum_guarantee")]
        public decimal? MinimumGuarantee { get; set; }

        /// <summary>
        /// Settlement Frequency - تكرار التسوية
        /// Monthly or Quarterly. Applies to RevenueShare pricing.
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "settlement_frequency")]
        public SettlementFrequency? SettlementFrequency { get; set; }

        /// <summary>
        /// Pricing Locked - التسعير مقفل
        /// Set to true when deal converts to contract. Prevents retroactive pricing changes.
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "pricing_locked")]
        public bool PricingLocked { get; set; } = false;

        /// <summary>
        /// Discount Amount (snapshot) - مبلغ الخصم
        /// Analytics only. Captured from deal at conversion.
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "discount_amount")]
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// Discount Percentage (snapshot) - نسبة الخصم
        /// Analytics only. Captured from deal at conversion.
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnName = "discount_percentage")]
        public decimal? DiscountPercentage { get; set; }

        #endregion

        #region Handover/Return Fields - حقول التسليم والاستلام

        /// <summary>
        /// Handover Date - تاريخ التسليم
        /// Date when unit was handed over to tenant
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "handover_date")]
        public DateTime? HandoverDate { get; set; }

        /// <summary>
        /// Handover Condition - حالة التسليم
        /// Condition of unit at handover
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "handover_condition")]
        public string HandoverCondition { get; set; }

        /// <summary>
        /// Return Date - تاريخ الإرجاع
        /// Date when unit was returned
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "return_date")]
        public DateTime? ReturnDate { get; set; }

        /// <summary>
        /// Return Condition - حالة الإرجاع
        /// Condition of unit at return
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "return_condition")]
        public string ReturnCondition { get; set; }

        #endregion

        #region Equipment/Vehicle Specific Fields - حقول المعدات/المركبات

        /// <summary>
        /// Serial Number - الرقم التسلسلي
        /// For equipment/vehicles
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "serial_number")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Model - الموديل
        /// For equipment/vehicles
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "model")]
        public string Model { get; set; }

        /// <summary>
        /// Manufacturer - الشركة المصنعة
        /// For equipment/vehicles
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "manufacturer")]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Year - السنة
        /// Manufacturing year for equipment/vehicles
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "year")]
        public int? Year { get; set; }

        /// <summary>
        /// Plate Number - رقم اللوحة
        /// For vehicles
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "plate_number")]
        public string PlateNumber { get; set; }

        #endregion

        #region Navigation Properties - خصائص الملاحة

        /// <summary>
        /// Contract - العقد
        /// Parent contract
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public ErpContract Contract { get; set; }

        /// <summary>
        /// Guarantees - الضمانات
        /// List of guarantees/warranties for this unit
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(ErpContractUnitGuarantee.UnitId))]
        public List<ErpContractUnitGuarantee> Guarantees { get; set; }

        /// <summary>
        /// Unit Type Name - اسم نوع الوحدة
        /// For display purposes
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string UnitTypeName { get; set; }

        #endregion

        #region Computed Properties - الخصائص المحسوبة

        /// <summary>
        /// Effective Price - السعر الفعلي
        /// Returns CalculatedPrice if set, otherwise BasePrice
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal? EffectivePrice => CalculatedPrice ?? BasePrice;

        /// <summary>
        /// Has Active Guarantees - لديه ضمانات نشطة
        /// Whether the unit has any active guarantees
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool HasActiveGuarantees => Guarantees?.Any(g => g.IsActive) ?? false;

        #endregion
    }
}
