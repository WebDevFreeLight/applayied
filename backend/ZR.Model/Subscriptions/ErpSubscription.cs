using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Subscription - الإشتراك
    /// </summary>
    [SugarTable("erp_subscriptions")]
    [SqlDict(
        dictType: "erp_subscription",
        dictName: "Subscription - الإشتراك",
        valueColumn: "subscriptionid",
        labelColumns: "subscriptionnumber",
        RemarkColumn = "notes",
        OrderBy = "startdate DESC",
        Description = "Auto-generated from ErpSubscription table"
    )]
    public class ErpSubscription : SysBase
    {
        /// <summary>
        /// Subscription ID - معرف الإشتراك
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SubscriptionId { get; set; }

        /// <summary>
        /// Subscription Number - رقم الإشتراك
        /// </summary>
        public string SubscriptionNumber { get; set; }

        /// <summary>
        /// Member ID - معرف العضو
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Package ID - معرف الباقة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PackageId { get; set; }

        /// <summary>
        /// Start Date - تاريخ البداية
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ الانتهاء
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Original End Date - تاريخ الانتهاء الأصلي
        /// </summary>
        public DateTime OriginalEndDate { get; set; }

        /// <summary>
        /// Status - الحالة (active/expired/frozen/cancelled)
        /// </summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Discount Amount - مبلغ الخصم
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Net Amount - المبلغ الصافي
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Remaining Amount - المبلغ المتبقي
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Max Visits - الحد الأقصى للزيارات (0 = غير محدود)
        /// </summary>
        public int MaxVisits { get; set; }

        /// <summary>
        /// Used Visits - الزيارات المستخدمة
        /// </summary>
        public int UsedVisits { get; set; }

        /// <summary>
        /// Remaining Visits - الزيارات المتبقية
        /// </summary>
        public int RemainingVisits { get; set; }

        /// <summary>
        /// Max Freeze Days - أيام التجميد المسموحة
        /// </summary>
        public int MaxFreezeDays { get; set; }

        /// <summary>
        /// Used Freeze Days - أيام التجميد المستخدمة
        /// </summary>
        public int UsedFreezeDays { get; set; }

        /// <summary>
        /// Is Currently Frozen - مجمد حالياً
        /// </summary>
        public bool IsCurrentlyFrozen { get; set; }

        /// <summary>
        /// Freeze Start Date - تاريخ بدء التجميد
        /// </summary>
        public DateTime? FreezeStartDate { get; set; }

        /// <summary>
        /// Freeze End Date - تاريخ انتهاء التجميد
        /// </summary>
        public DateTime? FreezeEndDate { get; set; }

        /// <summary>
        /// Trainer ID - معرف المدرب
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TrainerId { get; set; }

        /// <summary>
        /// Salesperson ID - معرف البائع
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SalespersonId { get; set; }
 

        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InvoiceId { get; set; }

        /// <summary>
        /// Promo Code ID - معرف كود الخصم
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PromoCodeId { get; set; }

        /// <summary>
        /// Gift Card ID - معرف بطاقة الهدية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GiftCardId { get; set; }

        /// <summary>
        /// Auto Renewal - تجديد تلقائي
        /// </summary>
        public bool AutoRenewal { get; set; }

        /// <summary>
        /// Renewed From ID - مجدد من الإشتراك
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RenewedFromId { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}

