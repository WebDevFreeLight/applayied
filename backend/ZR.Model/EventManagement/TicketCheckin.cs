using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Ticket Check-in/Checkout History - سجل دخول وخروج التذاكر
    /// </summary>
    [SugarTable("ticket_checkins")]
    public class TicketCheckin : SysBase
    {
        /// <summary>
        /// Check-in ID - معرف السجل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CheckinId { get; set; }

        /// <summary>
        /// Ticket ID - معرف التذكرة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TicketId { get; set; }

        /// <summary>
        /// Event ID - معرف الفعالية (for indexing)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }

        /// <summary>
        /// Visitor ID - معرف الزائر (for reference)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VisitorId { get; set; }

        /// <summary>
        /// Gate ID - معرف البوابة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GateId { get; set; }

        /// <summary>
        /// Action - الإجراء (checkin, checkout, manual, system)
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Action At - وقت الإجراء
        /// </summary>
        public DateTime ActionAt { get; set; }

        /// <summary>
        /// Operator ID - معرف المشغل
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? OperatorId { get; set; }

        /// <summary>
        /// Device Info - معلومات الجهاز
        /// </summary>
        public string DeviceInfo { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// IP Address - عنوان IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Location - الموقع
        /// </summary>
        public string Location { get; set; }
    }
}

