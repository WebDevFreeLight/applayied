using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Subscription Freeze - تجميد الإشتراك
    /// </summary>
    [SugarTable("erp_subscription_freezes")]
    public class ErpSubscriptionFreeze : SysBase
    {
        /// <summary>
        /// Freeze ID - معرف التجميد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long FreezeId { get; set; }

        /// <summary>
        /// Subscription ID - معرف الإشتراك
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SubscriptionId { get; set; }

        /// <summary>
        /// Member ID - معرف العضو
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Start Date - تاريخ بدء التجميد
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End Date - تاريخ انتهاء التجميد
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Requested Days - الأيام المطلوبة
        /// </summary>
        public int RequestedDays { get; set; }

        /// <summary>
        /// Actual Days - الأيام الفعلية
        /// </summary>
        public int ActualDays { get; set; }

        /// <summary>
        /// Reason - السبب
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Status - الحالة (active/completed/cancelled)
        /// </summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// Unfrozen By - ألغي التجميد بواسطة
        /// </summary>
        public string UnfrozenBy { get; set; }

        /// <summary>
        /// Unfrozen Date - تاريخ إلغاء التجميد
        /// </summary>
        public DateTime? UnfrozenDate { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

