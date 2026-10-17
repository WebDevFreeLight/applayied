using System;
using ZR.Model.Contracts.Enums;

namespace ZR.Model.Sales
{
    /// <summary>
    /// Sales Deal Unit - وحدة صفقة المبيعات
    /// Junction table linking deals to boulevard shops with per-unit pricing.
    /// This is the single source of truth for what was negotiated BEFORE contract conversion.
    /// </summary>
    [SugarTable("sales_deal_unit")]
    public class SalesDealUnit : SysBase
    {
        /// <summary>
        /// Deal Unit ID - معرف وحدة الصفقة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "deal_unit_id", IsPrimaryKey = true, IsIdentity = false)]
        public long DealUnitId { get; set; }

        /// <summary>
        /// Deal ID - معرف الصفقة
        /// FK to sales_deal
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "deal_id")]
        public long DealId { get; set; }

        /// <summary>
        /// Shop ID - معرف المحل
        /// FK to blv_boulevard_shops
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "shop_id")]
        public long ShopId { get; set; }

        /// <summary>
        /// Season ID - معرف الموسم
        /// FK to blv_boulevard_seasons (for context/filtering)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "season_id")]
        public long SeasonId { get; set; }

        /// <summary>
        /// Pricing Type - نوع التسعير
        /// Fixed or RevenueShare
        /// </summary>
        [SugarColumn(ColumnName = "pricing_type")]
        public PricingType PricingType { get; set; } = PricingType.Fixed;

        /// <summary>
        /// Fixed Rent Amount - مبلغ الإيجار الثابت
        /// Required when PricingType = Fixed. Per billing period.
        /// </summary>
        [SugarColumn(ColumnName = "fixed_rent_amount", DecimalDigits = 2, IsNullable = true)]
        public decimal? FixedRentAmount { get; set; }

        /// <summary>
        /// Revenue Percentage - نسبة الإيرادات
        /// Required when PricingType = RevenueShare. Value 0-100.
        /// </summary>
        [SugarColumn(ColumnName = "revenue_percentage", DecimalDigits = 2, IsNullable = true)]
        public decimal? RevenuePercentage { get; set; }

        /// <summary>
        /// Minimum Guarantee - الحد الأدنى المضمون
        /// Optional floor for RevenueShare. Invoice = MAX(calculated, minimum_guarantee).
        /// </summary>
        [SugarColumn(ColumnName = "minimum_guarantee", DecimalDigits = 2, IsNullable = true)]
        public decimal? MinimumGuarantee { get; set; }

        /// <summary>
        /// Settlement Frequency - تكرار التسوية
        /// How often revenue share is settled. Default Monthly.
        /// </summary>
        [SugarColumn(ColumnName = "settlement_frequency", IsNullable = true)]
        public SettlementFrequency? SettlementFrequency { get; set; } = ZR.Model.Contracts.Enums.SettlementFrequency.Monthly;

        #region Base Price Reference & Discount Tracking

        /// <summary>
        /// Shop Base Price (snapshot) - السعر الأساسي للمحل
        /// Captured from shop's TotalPrice at time of deal unit creation.
        /// Used for discount calculation only.
        /// </summary>
        [SugarColumn(ColumnName = "shop_base_price", DecimalDigits = 2, IsNullable = true)]
        public decimal? ShopBasePrice { get; set; }

        /// <summary>
        /// Discount Amount - مبلغ الخصم
        /// ShopBasePrice - FixedRentAmount (analytics only, does NOT block transaction)
        /// </summary>
        [SugarColumn(ColumnName = "discount_amount", DecimalDigits = 2, IsNullable = true)]
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// Discount Percentage - نسبة الخصم
        /// (ShopBasePrice - FixedRentAmount) / ShopBasePrice * 100 (analytics only)
        /// </summary>
        [SugarColumn(ColumnName = "discount_percentage", DecimalDigits = 2, IsNullable = true)]
        public decimal? DiscountPercentage { get; set; }

        #endregion

        #region Snapshot Fields (from shop at time of linking)

        /// <summary>
        /// Shop Name (snapshot) - اسم المحل
        /// </summary>
        [SugarColumn(ColumnName = "shop_name", Length = 200, IsNullable = true)]
        public string ShopName { get; set; }

        /// <summary>
        /// Shop Area (snapshot) - مساحة المحل
        /// </summary>
        [SugarColumn(ColumnName = "shop_area", DecimalDigits = 2, IsNullable = true)]
        public decimal? ShopArea { get; set; }

        /// <summary>
        /// Zone ID (snapshot) - معرف المنطقة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "zone_id", IsNullable = true)]
        public long? ZoneId { get; set; }

        /// <summary>
        /// Shop Category ID (snapshot) - معرف فئة المحل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(ColumnName = "shop_category_id", IsNullable = true)]
        public long? ShopCategoryId { get; set; }

        #endregion

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(ColumnName = "notes", ColumnDataType = "text", IsNullable = true)]
        public string Notes { get; set; }

        /// <summary>
        /// Is Converted - هل تم التحويل
        /// True after deal is Won and contract unit is created from this record.
        /// </summary>
        [SugarColumn(ColumnName = "is_converted", IsNullable = false)]
        public bool IsConverted { get; set; } = false;

        #region Navigation Properties (not mapped)

        [SugarColumn(IsIgnore = true)]
        public string ShopStatusName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ShopCategoryName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string ZoneName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string SeasonName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string DealTitle { get; set; }

        #endregion
    }
}
