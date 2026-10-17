using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Member Fingerprint Log - سجل بصمات العملاء
    /// </summary>
    [SugarTable("erp_member_fingerprint_logs")]
    public class ErpMemberFingerprintLog : SysBase
    {
        /// <summary>
        /// Log ID - معرف السجل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LogId { get; set; }

        /// <summary>
        /// Member ID - معرف العضو
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

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
        /// Log Time - وقت السجل
        /// </summary>
        public DateTime LogTime { get; set; }

        /// <summary>
        /// Log Type - نوع السجل (check_in/check_out/access_denied/registration)
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// Is Successful - ناجح
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Failure Reason - سبب الفشل
        /// </summary>
        public string FailureReason { get; set; }

        /// <summary>
        /// Subscription ID - معرف الإشتراك
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SubscriptionId { get; set; }

        /// <summary>
        /// Visit ID - معرف الزيارة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VisitId { get; set; }

       

        /// <summary>
        /// Raw Data - البيانات الخام
        /// </summary>
        public string RawData { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

