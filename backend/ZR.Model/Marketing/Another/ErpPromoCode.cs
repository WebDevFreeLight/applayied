using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Another
{
    /// <summary>
    /// Promo Codes Entity - اكواد الخصم
    /// </summary>
    [SugarTable("erp_promo_code")]
    public class ErpPromoCode : SysBase
    {
        /// <summary>
        /// Promo Code ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromoCodeId { get; set; }

        /// <summary>
        /// Unique Code (e.g., "SUMMER2024")
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Code { get; set; }

        /// <summary>
        /// Associated Promotion ID (optional)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? PromotionId { get; set; }

        /// <summary>
        /// Code Type: single_use, multi_use, unique_per_member
        /// </summary>
        [SugarColumn(Length = 30)]
        public string CodeType { get; set; }

        /// <summary>
        /// Discount Percentage (e.g., 10 for 10%)
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Fixed Discount Amount
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? DiscountFixed { get; set; }

        /// <summary>
        /// Maximum Discount Cap
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? MaxDiscount { get; set; }

        /// <summary>
        /// Valid From Date
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// Valid To Date
        /// </summary>
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// Maximum Redemptions allowed
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxRedemptions { get; set; }

        /// <summary>
        /// Current Redemption Count
        /// </summary>
        public int CurrentRedemptions { get; set; } = 0;

        /// <summary>
        /// Applicable Products: all, subscriptions, products, specific
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ApplicableProducts { get; set; }

        /// <summary>
        /// JSON array of specific item IDs
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true)]
        public string SpecificItemIds { get; set; }

        /// <summary>
        /// Is Active flag
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Created for specific Member ID (personalized codes)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? CreatedForMemberId { get; set; }
    }

    /// <summary>
    /// Promo Code Redemption History
    /// </summary>
    [SugarTable("erp_promo_code_redemption")]
    public class ErpPromoCodeRedemption : SysBase
    {
        /// <summary>
        /// Redemption ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RedemptionId { get; set; }

        /// <summary>
        /// Promo Code ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromoCodeId { get; set; }

        /// <summary>
        /// Member ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Invoice ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? InvoiceId { get; set; }

        /// <summary>
        /// Original Amount before discount
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Discount Amount Applied
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Final Amount after discount
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal FinalAmount { get; set; }

        /// <summary>
        /// Redemption Date/Time
        /// </summary>
        public DateTime RedeemedAt { get; set; }
    }
}

