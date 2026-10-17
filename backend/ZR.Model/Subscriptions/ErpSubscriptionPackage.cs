using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Subscription Package - باقة الإشتراك
    /// </summary>
    [SugarTable("erp_subscription_packages")]
    [SqlDict(
        dictType: "erp_subscription_package",
        dictName: "Subscription Package - الباقة",
        valueColumn: "packageid",
        labelColumns: "packagename",
        RemarkColumn = "packagenamear",
        OrderBy = "sortorder ASC",
        Description = "Auto-generated from ErpSubscriptionPackage table"
    )]
    public class ErpSubscriptionPackage : SysBase
    {
        /// <summary>
        /// Package ID - معرف الباقة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PackageId { get; set; }

        /// <summary>
        /// Package Code - كود الباقة
        /// </summary>
        public string PackageCode { get; set; }

        /// <summary>
        /// Package Name - اسم الباقة
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Package Name Arabic - اسم الباقة بالعربي
        /// </summary>
        public string PackageNameAr { get; set; }

        /// <summary>
        /// Package Type - نوع الباقة (individual/family/corporate)
        /// </summary>
        public string PackageType { get; set; } = "individual";

        /// <summary>
        /// Duration Days - مدة الباقة بالأيام
        /// </summary>
        public int DurationDays { get; set; }

        /// <summary>
        /// Duration Months - مدة الباقة بالأشهر
        /// </summary>
        public int DurationMonths { get; set; }

        /// <summary>
        /// Price - السعر
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Discount Percent - نسبة الخصم
        /// </summary>
        public decimal DiscountPercent { get; set; }

        /// <summary>
        /// Final Price - السعر النهائي
        /// </summary>
        public decimal FinalPrice { get; set; }

        /// <summary>
        /// VAT Percent - نسبة الضريبة
        /// </summary>
        public decimal VatPercent { get; set; } = 15;

        /// <summary>
        /// VAT Amount - مبلغ الضريبة
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Total Price - السعر الإجمالي
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Max Visits - الحد الأقصى للزيارات (0 = غير محدود)
        /// </summary>
        public int MaxVisits { get; set; }

        /// <summary>
        /// Max Freeze Days - أيام التجميد المسموحة
        /// </summary>
        public int MaxFreezeDays { get; set; }

        /// <summary>
        /// Includes Trainer - تشمل مدرب
        /// </summary>
        public bool IncludesTrainer { get; set; }

        /// <summary>
        /// Includes Locker - تشمل خزانة
        /// </summary>
        public bool IncludesLocker { get; set; }

        /// <summary>
        /// Includes Towel - تشمل منشفة
        /// </summary>
        public bool IncludesTowel { get; set; }

        /// <summary>
        /// Allowed Activities - الأنشطة المسموحة (JSON)
        /// </summary>
        public string AllowedActivities { get; set; }

        /// <summary>
        /// Allowed Branches - الفروع المسموحة (JSON)
        /// </summary>
        public string AllowedBranches { get; set; }

        /// <summary>
        /// Allowed Time Slots - أوقات الدخول المسموحة (JSON)
        /// </summary>
        public string AllowedTimeSlots { get; set; }

        /// <summary>
        /// Features - المميزات (JSON)
        /// </summary>
        public string Features { get; set; }

        /// <summary>
        /// Description - الوصف
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Color - اللون
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Image URL - رابط الصورة
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Sort Order - ترتيب العرض
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Is Featured - مميز
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

