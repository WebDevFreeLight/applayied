using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;

namespace ZR.Model.EventManagement
{
    /// <summary>
    /// Event Booth History - سجل الأكشاك
    /// Tracks all booth assignments and releases
    /// </summary>
    [SugarTable("event_booth_history")]
    public class EventBoothHistory:SysBase
    {
        /// <summary>
        /// History ID - معرف السجل
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }

        /// <summary>
        /// Booth ID - معرف الكشك
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BoothId { get; set; }

        /// <summary>
        /// Booth Code - رمز الكشك
        /// </summary>
        public string BoothCode { get; set; }

        /// <summary>
        /// Booth Name - اسم الكشك
        /// </summary>
        public string BoothName { get; set; }

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
        /// Tenant ID - معرف المستأجر
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TenantId { get; set; }

        /// <summary>
        /// Tenant Name - اسم المستأجر
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// Action Type - نوع الإجراء (assigned, released)
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
        /// Previous Tenant ID - معرف المستأجر السابق
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PreviousTenantId { get; set; }

        /// <summary>
        /// Previous Tenant Name - اسم المستأجر السابق
        /// </summary>
        public string PreviousTenantName { get; set; }

        /// <summary>
        /// Rental Price - سعر الإيجار
        /// </summary>
        public decimal? RentalPrice { get; set; }

        /// <summary>
        /// Action Date - تاريخ الإجراء
        /// </summary>
        public DateTime ActionDate { get; set; }

        /// <summary>
        /// Performed By User ID - معرف المستخدم المنفذ
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PerformedByUserId { get; set; }

        /// <summary>
        /// Performed By Username - اسم المستخدم المنفذ
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
    }
}

