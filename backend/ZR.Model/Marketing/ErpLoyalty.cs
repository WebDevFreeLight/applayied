using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Marketing
{
    /// <summary>
    /// Loyalty Program - برنامج الولاء
    /// </summary>
    [SugarTable("erp_loyalty_program")]
    public class ErpLoyaltyProgram : SysBase
    {
        /// <summary>
        /// Program ID - معرف البرنامج
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProgramId { get; set; }

        /// <summary>
        /// Program Name - اسم البرنامج
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// Program Name Arabic - اسم البرنامج بالعربي
        /// </summary>
        public string ProgramNameAr { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Points Per Amount - نقاط لكل مبلغ (1 ريال = X نقطة)
        /// </summary>
        public decimal PointsPerAmount { get; set; } = 1;

        /// <summary>
        /// Amount Per Point - المبلغ لكل نقطة (X نقطة = 1 ريال)
        /// </summary>
        public decimal AmountPerPoint { get; set; } = 10;

        /// <summary>
        /// Minimum Redemption Points - الحد الأدنى للاستبدال
        /// </summary>
        public int MinimumRedemptionPoints { get; set; } = 100;

        /// <summary>
        /// Points Expiry Days - أيام انتهاء النقاط
        /// </summary>
        public int PointsExpiryDays { get; set; } = 365;

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Loyalty Tier - مستوى الولاء
    /// </summary>
    [SugarTable("erp_loyalty_tiers")]
    public class ErpLoyaltyTier : SysBase
    {
        /// <summary>
        /// Tier ID - معرف المستوى
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TierId { get; set; }

        /// <summary>
        /// Program ID - معرف البرنامج
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProgramId { get; set; }

        /// <summary>
        /// Tier Name - اسم المستوى
        /// </summary>
        public string TierName { get; set; }

        /// <summary>
        /// Tier Name Arabic - اسم المستوى بالعربي
        /// </summary>
        public string TierNameAr { get; set; }

        /// <summary>
        /// Minimum Points - الحد الأدنى من النقاط
        /// </summary>
        public int MinimumPoints { get; set; }

        /// <summary>
        /// Discount Percent - نسبة الخصم
        /// </summary>
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Points Multiplier - مضاعف النقاط
        /// </summary>
        public decimal PointsMultiplier { get; set; } = 1;

        /// <summary>
        /// Has Priority Booking - له أولوية في الحجز
        /// </summary>
        public bool HasPriorityBooking { get; set; }

        /// <summary>
        /// Has Free Sessions - له جلسات مجانية
        /// </summary>
        public int FreeSessionsPerMonth { get; set; }

        /// <summary>
        /// Color - اللون
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Icon - الأيقونة
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Sort Order - ترتيب العرض
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Member Loyalty - ولاء العضو
    /// </summary>
    [SugarTable("erp_member_loyalty")]
    public class ErpMemberLoyalty : SysBase
    {
        /// <summary>
        /// ID - المعرف
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long Id { get; set; }

        /// <summary>
        /// Member ID - معرف العضو
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Program ID - معرف البرنامج
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProgramId { get; set; }

        /// <summary>
        /// Current Tier ID - معرف المستوى الحالي
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CurrentTierId { get; set; }

        /// <summary>
        /// Total Points Earned - إجمالي النقاط المكتسبة
        /// </summary>
        public int TotalPointsEarned { get; set; }

        /// <summary>
        /// Total Points Redeemed - إجمالي النقاط المستبدلة
        /// </summary>
        public int TotalPointsRedeemed { get; set; }

        /// <summary>
        /// Current Balance - الرصيد الحالي
        /// </summary>
        public int CurrentBalance { get; set; }

        /// <summary>
        /// Points Expiring Soon - نقاط تنتهي قريباً
        /// </summary>
        public int PointsExpiringSoon { get; set; }

        /// <summary>
        /// Expiry Date - تاريخ الانتهاء
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Enrollment Date - تاريخ التسجيل
        /// </summary>
        public DateTime EnrollmentDate { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Member Name - اسم العضو (للعرض)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string MemberName { get; set; }

        /// <summary>
        /// Tier Name - اسم المستوى (للعرض)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string TierName { get; set; }
    }

    /// <summary>
    /// Loyalty Transaction - معاملة الولاء
    /// </summary>
    [SugarTable("erp_loyalty_transactions")]
    public class ErpLoyaltyTransaction : SysBase
    {
        /// <summary>
        /// Transaction ID - معرف المعاملة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TransactionId { get; set; }

        /// <summary>
        /// Member ID - معرف العضو
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Transaction Type - نوع المعاملة (earn/redeem/expire/adjust)
        /// </summary>
        public string TransactionType { get; set; }

        /// <summary>
        /// Points - النقاط
        /// </summary>
        public int Points { get; set; }

        /// <summary>
        /// Balance After - الرصيد بعد المعاملة
        /// </summary>
        public int BalanceAfter { get; set; }

        /// <summary>
        /// Reference Type - نوع المرجع (invoice/subscription/referral/bonus)
        /// </summary>
        public string ReferenceType { get; set; }

        /// <summary>
        /// Reference ID - معرف المرجع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReferenceId { get; set; }

        /// <summary>
        /// Amount - المبلغ (للكسب/الاستبدال)
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Transaction Date - تاريخ المعاملة
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Expiry Date - تاريخ انتهاء النقاط
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Is Expired - منتهي
        /// </summary>
        public bool IsExpired { get; set; }

        /// <summary>
        /// Member Name - اسم العضو (للعرض)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string MemberName { get; set; }
    }
}

