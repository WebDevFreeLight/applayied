using Infrastructure.Converter;
using System;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing.Another
{
    /// <summary>
    /// Member Loyalty Points Entity - نقاط الولاء
    /// </summary>
    [SugarTable("erp_loyalty_points")]
    public class ErpLoyaltyPoints : SysBase
    {
        /// <summary>
        /// Points ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PointsId { get; set; }

        /// <summary>
        /// Member ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Total Points Earned (lifetime)
        /// </summary>
        public int TotalEarned { get; set; } = 0;

        /// <summary>
        /// Total Points Redeemed (lifetime)
        /// </summary>
        public int TotalRedeemed { get; set; } = 0;

        /// <summary>
        /// Current Available Balance
        /// </summary>
        public int CurrentBalance { get; set; } = 0;

        /// <summary>
        /// Current Loyalty Tier ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CurrentTierId { get; set; }

        /// <summary>
        /// Date when tier was last updated
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? TierUpdatedAt { get; set; }

        /// <summary>
        /// Points needed to reach next tier
        /// </summary>
        public int PointsToNextTier { get; set; }

        /// <summary>
        /// Last Activity Date
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? LastActivityDate { get; set; }
    }

    /// <summary>
    /// Loyalty Points Transaction Entity - سجل المعاملات
    /// </summary>
    [SugarTable("erp_loyalty_transaction")]
    public class ErpLoyaltyTransaction : SysBase
    {
        /// <summary>
        /// Transaction ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TransactionId { get; set; }

        /// <summary>
        /// Member ID
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Transaction Type: earn_purchase, earn_referral, earn_birthday, earn_checkin,
        /// earn_review, redeem_discount, redeem_product, redeem_service, expire, adjust
        /// </summary>
        [SugarColumn(Length = 50)]
        public string TransactionType { get; set; }

        /// <summary>
        /// Points (positive for earn, negative for redeem)
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Balance Before Transaction
        /// </summary>
        public int BalanceBefore { get; set; }

        /// <summary>
        /// Balance After Transaction
        /// </summary>
        public int BalanceAfter { get; set; }

        /// <summary>
        /// Source: invoice, referral, bonus, manual
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Source { get; set; }

        /// <summary>
        /// Source ID (e.g., Invoice ID, Referral ID)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        [SugarColumn(IsNullable = true)]
        public long? SourceId { get; set; }

        /// <summary>
        /// Description of transaction
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string Description { get; set; }

        /// <summary>
        /// Points Expiry Date
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? ExpiryDate { get; set; }
    }
}

