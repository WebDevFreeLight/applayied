using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Another
{
    /// <summary>
    /// Loyalty Tier Entity - مستويات الولاء
    /// Regular, Silver, Gold, Platinum
    /// </summary>
    [SugarTable("erp_loyalty_tier")]
    public class ErpLoyaltyTier : SysBase
    {
        /// <summary>
        /// Tier ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TierId { get; set; }

        /// <summary>
        /// Tier Code (unique identifier)
        /// </summary>
        [SugarColumn(Length = 30)]
        public string TierCode { get; set; }

        /// <summary>
        /// Tier Name (English)
        /// </summary>
        [SugarColumn(Length = 100)]
        public string TierName { get; set; }

        /// <summary>
        /// Tier Name (Arabic)
        /// </summary>
        [SugarColumn(Length = 100)]
        public string TierNameAr { get; set; }

        /// <summary>
        /// Minimum Points to qualify for this tier
        /// </summary>
        public int MinPoints { get; set; }

        /// <summary>
        /// Maximum Points for this tier
        /// </summary>
        public int MaxPoints { get; set; }

        /// <summary>
        /// Points Earning Multiplier (e.g., 1.5x for Gold)
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal PointsMultiplier { get; set; } = 1.0m;

        /// <summary>
        /// Automatic Discount Percentage for this tier
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// JSON array of tier benefits
        /// </summary>
        [SugarColumn(ColumnDataType = "text", IsNullable = true)]
        public string BenefitsJson { get; set; }

        /// <summary>
        /// Badge Icon URL or icon name
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = true)]
        public string BadgeIcon { get; set; }

        /// <summary>
        /// Badge Color (hex code)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true)]
        public string BadgeColor { get; set; }

        /// <summary>
        /// Sort Order for display
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Is Active flag
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Loyalty Rules Entity - قواعد النقاط
    /// </summary>
    [SugarTable("erp_loyalty_rule")]
    public class ErpLoyaltyRule : SysBase
    {
        /// <summary>
        /// Rule ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RuleId { get; set; }

        /// <summary>
        /// Rule Code
        /// </summary>
        [SugarColumn(Length = 50)]
        public string RuleCode { get; set; }

        /// <summary>
        /// Rule Name (English)
        /// </summary>
        [SugarColumn(Length = 100)]
        public string RuleName { get; set; }

        /// <summary>
        /// Rule Name (Arabic)
        /// </summary>
        [SugarColumn(Length = 100)]
        public string RuleNameAr { get; set; }

        /// <summary>
        /// Rule Type: purchase_points, checkin_bonus, birthday_bonus, referral_bonus, review_bonus
        /// </summary>
        [SugarColumn(Length = 50)]
        public string RuleType { get; set; }

        /// <summary>
        /// Points earned per SAR spent
        /// </summary>
        [SugarColumn(DecimalDigits = 2)]
        public decimal PointsPerSar { get; set; }

        /// <summary>
        /// Fixed Points for actions (e.g., birthday bonus)
        /// </summary>
        public int FixedPoints { get; set; }

        /// <summary>
        /// Applicable To: all, subscriptions, products
        /// </summary>
        [SugarColumn(Length = 50)]
        public string ApplicableTo { get; set; }

        /// <summary>
        /// Maximum Points per single transaction
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxPointsPerTransaction { get; set; }

        /// <summary>
        /// Daily Points Limit
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? DailyLimit { get; set; }

        /// <summary>
        /// Is Active flag
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

