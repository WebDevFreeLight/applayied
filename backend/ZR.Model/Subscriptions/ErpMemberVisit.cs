using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Member Visit - زيارة العضو / حضور الصالة
    /// </summary>
    [SugarTable("erp_member_visits")]
    public class ErpMemberVisit : SysBase
    {
        /// <summary>
        /// Visit ID - معرف الزيارة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long VisitId { get; set; }

        /// <summary>
        /// Member ID - معرف العضو
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Subscription ID - معرف الإشتراك
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SubscriptionId { get; set; }

        /// <summary>
        /// Visit Date - تاريخ الزيارة
        /// </summary>
        public DateTime VisitDate { get; set; }

        /// <summary>
        /// Check In Time - وقت الدخول
        /// </summary>
        public DateTime CheckInTime { get; set; }

        /// <summary>
        /// Check Out Time - وقت الخروج
        /// </summary>
        public DateTime? CheckOutTime { get; set; }

        /// <summary>
        /// Duration Minutes - مدة الزيارة بالدقائق
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// Check In Method - طريقة تسجيل الدخول (fingerprint/qr/manual/card)
        /// </summary>
        public string CheckInMethod { get; set; }

        /// <summary>
        /// Check Out Method - طريقة تسجيل الخروج
        /// </summary>
        public string CheckOutMethod { get; set; }

        /// <summary>
        /// Device ID - معرف الجهاز
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DeviceId { get; set; }

        /// <summary>
        /// Zone ID - معرف المنطقة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; }

        /// <summary>
        /// Activity ID - معرف النشاط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ActivityId { get; set; }

        /// <summary>
        /// Trainer ID - معرف المدرب
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TrainerId { get; set; }

        /// <summary>
        /// Is Counted - محسوبة من الزيارات
        /// </summary>
        public bool IsCounted { get; set; } = true;

        /// <summary>
        /// Status - الحالة (active/completed/cancelled)
        /// </summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

