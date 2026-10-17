using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Gate History - سجل تاريخ البوابات
    /// </summary>
    [SugarTable("event_gate_history")]
    public class EventGateHistory : SysBase
    {
        /// <summary>
        /// History ID - معرف السجل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }

        /// <summary>
        /// Gate ID - معرف البوابة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GateId { get; set; }

        /// <summary>
        /// Gate Code - رمز البوابة
        /// </summary>
        public string GateCode { get; set; }

        /// <summary>
        /// Gate Name - اسم البوابة
        /// </summary>
        public string GateName { get; set; }

        /// <summary>
        /// Event ID - معرف الفعالية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }

        /// <summary>
        /// Event Name - اسم الفعالية
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Action Type - نوع الإجراء (created, updated, status_changed, opened, closed, maintenance)
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Previous Status - الحالة السابقة
        /// </summary>
        public string PreviousStatus { get; set; }

        /// <summary>
        /// New Status - الحالة الجديدة
        /// </summary>
        public string NewStatus { get; set; }

        /// <summary>
        /// Previous Gate Type - النوع السابق للبوابة
        /// </summary>
        public string PreviousGateType { get; set; }

        /// <summary>
        /// New Gate Type - النوع الجديد للبوابة
        /// </summary>
        public string NewGateType { get; set; }

        /// <summary>
        /// Previous Capacity - السعة السابقة
        /// </summary>
        public int? PreviousCapacity { get; set; }

        /// <summary>
        /// New Capacity - السعة الجديدة
        /// </summary>
        public int? NewCapacity { get; set; }

        /// <summary>
        /// Previous Manager ID - معرف المسؤول السابق
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PreviousManagerId { get; set; }

        /// <summary>
        /// Previous Manager Name - اسم المسؤول السابق
        /// </summary>
        public string PreviousManagerName { get; set; }

        /// <summary>
        /// New Manager ID - معرف المسؤول الجديد
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? NewManagerId { get; set; }

        /// <summary>
        /// New Manager Name - اسم المسؤول الجديد
        /// </summary>
        public string NewManagerName { get; set; }

        /// <summary>
        /// Action Date - تاريخ الإجراء
        /// </summary>
        public DateTime ActionDate { get; set; }

        /// <summary>
        /// Performed By User ID - معرف المستخدم الذي قام بالإجراء
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PerformedByUserId { get; set; }

        /// <summary>
        /// Performed By Username - اسم المستخدم الذي قام بالإجراء
        /// </summary>
        public string PerformedByUsername { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// IP Address - عنوان IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Visitor ID - معرف الزائر (for check-in tracking)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? VisitorId { get; set; }

        /// <summary>
        /// Visitor Name - اسم الزائر
        /// </summary>
        public string VisitorName { get; set; }

        /// <summary>
        /// Visitor Code - رمز الزائر
        /// </summary>
        public string VisitorCode { get; set; }

        /// <summary>
        /// Ticket ID - معرف التذكرة (for check-in tracking)
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TicketId { get; set; }

        /// <summary>
        /// Ticket Number - رقم التذكرة
        /// </summary>
        public string TicketNumber { get; set; }

        /// <summary>
        /// Checkin Action - إجراء الدخول/الخروج (checkin, checkout)
        /// </summary>
        public string CheckinAction { get; set; }

        /// <summary>
        /// Device Info - معلومات الجهاز
        /// </summary>
        public string DeviceInfo { get; set; }
    }
}

