using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Activity Attendance - حضور النشاط / سجل حضور المجموعات
    /// </summary>
    [SugarTable("erp_activity_attendances")]
    public class ErpActivityAttendance : SysBase
    {
        /// <summary>
        /// Attendance ID - معرف الحضور
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AttendanceId { get; set; }

        /// <summary>
        /// Activity ID - معرف النشاط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ActivityId { get; set; }

        /// <summary>
        /// Group ID - معرف المجموعة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? GroupId { get; set; }

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
        /// Session Date - تاريخ الحصة
        /// </summary>
        public DateTime SessionDate { get; set; }

        /// <summary>
        /// Check In Time - وقت الحضور
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// Check Out Time - وقت الانصراف
        /// </summary>
        public DateTime? CheckOutTime { get; set; }

        /// <summary>
        /// Status - الحالة (present/absent/late/excused)
        /// </summary>
        public string Status { get; set; } = "present";

        /// <summary>
        /// Trainer ID - معرف المدرب
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TrainerId { get; set; }
 
        /// <summary>
        /// Is Paid Session - حصة مدفوعة
        /// </summary>
        public bool IsPaidSession { get; set; }

        /// <summary>
        /// Amount Paid - المبلغ المدفوع
        /// </summary>
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InvoiceId { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

