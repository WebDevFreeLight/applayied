using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Another
{
    /// <summary>
    /// Promotional Offers Entity - العروض الترويجية
    /// </summary>
    [SugarTable("erp_promotion")]
    public class ErpPromotion : SysBase
    {
        /// <summary>
        /// Promotion ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromotionId { get; set; }

        /// <summary>
        /// Promotion Code (unique identifier)
        /// </summary>
        [SugarColumn(Length = 50)]
        public string PromotionCode { get; set; }

        /// <summary>
        /// Promotion Name (English)
        /// </summary>
        [SugarColumn(Length = 200)]
        public string PromotionName { get; set; }

        /// <summary>
        /// Promotion Name (Arabic)
        /// </summary>
        [SugarColumn(Length = 200)]
        public string PromotionNameAr { get; set; }

        /// <summary>
        /// Promotion Type: percentage, fixed_amount, buy_x_get_y, package_upgrade, free_days
        /// </summary>
        [SugarColumn(Length = 50)]
        public string PromotionType { get; set; }

        /// <summary>
        /// Discount Value (percentage or fixed amount)
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Maximum Discount Amount (cap for percentage discounts)
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? MaxDiscountAmount { get; set; }

        /// <summary>
        /// Minimum Purchase Amount required
        /// </summary>
        [SugarColumn(DecimalDigits = 2, IsNullable = true)]
        public decimal? MinPurchaseAmount { get; set; }

        /// <summary>
        /// Start Date of promotion
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date of promotion
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Applicable To: all, subscriptions, products, services
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ApplicableTo { get; set; }

        /// <summary>
        /// JSON array of applicable item IDs
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true)]
        public string ApplicableItemsJson { get; set; }

        /// <summary>
        /// Total Usage Limit
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? UsageLimit { get; set; }

        /// <summary>
        /// Current Usage Count
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// Usage Limit Per Member
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? UsageLimitPerMember { get; set; }

        /// <summary>
        /// Target Audience: all, new_members, existing, specific_tier
        /// </summary>
        [SugarColumn(Length = 50)]
        public string TargetAudience { get; set; }

        /// <summary>
        /// JSON array of target tier IDs
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true)]
        public string TargetTiersJson { get; set; }

        /// <summary>
        /// Requires Promo Code to apply
        /// </summary>
        public bool RequiresPromoCode { get; set; } = false;

        /// <summary>
        /// Can be stacked with other promotions
        /// </summary>
        public bool IsStackable { get; set; } = false;

        /// <summary>
        /// Priority for applying multiple promotions
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Is Active flag
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Status: draft, active, paused, expired
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "draft";
    }

    /// <summary>
    /// Promotion Usage Tracking Entity
    /// </summary>
    [SugarTable("erp_promotion_usage")]
    public class ErpPromotionUsage : SysBase
    {
        /// <summary>
        /// Usage ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long UsageId { get; set; }

        /// <summary>
        /// Promotion ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PromotionId { get; set; }

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
        /// Discount Applied Amount
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal DiscountApplied { get; set; }

        /// <summary>
        /// Date/Time when used
        /// </summary>
        public DateTime UsedAt { get; set; }
    }
}

