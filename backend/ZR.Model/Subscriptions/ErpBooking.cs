using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Subscriptions
{
    /// <summary>
    /// Booking - الحجز
    /// </summary>
    [SugarTable("erp_bookings")]
    [SqlDict(
        dictType: "erp_booking",
        dictName: "Booking - الحجز",
        valueColumn: "bookingid",
        labelColumns: "bookingnumber",
        RemarkColumn = "notes",
        OrderBy = "bookingdate DESC",
        Description = "Auto-generated from ErpBooking table"
    )]
    public class ErpBooking : SysBase
    {
        /// <summary>
        /// Booking ID - معرف الحجز
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BookingId { get; set; }

        /// <summary>
        /// Booking Number - رقم الحجز
        /// </summary>
        public string BookingNumber { get; set; }

        /// <summary>
        /// Member ID - معرف العضو
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MemberId { get; set; }

        /// <summary>
        /// Resource ID - معرف المورد/الغرفة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ResourceId { get; set; }

        /// <summary>
        /// Activity ID - معرف النشاط
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ActivityId { get; set; }

        /// <summary>
        /// Service Staff ID - معرف موظف الخدمات
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ServiceStaffId { get; set; }

        /// <summary>
        /// Booking Date - تاريخ الحجز
        /// </summary>
        public DateTime BookingDate { get; set; }

        /// <summary>
        /// Start Time - وقت البداية
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End Time - وقت النهاية
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Duration Minutes - المدة بالدقائق
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Status - الحالة (pending/confirmed/completed/cancelled/no_show)
        /// </summary>
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Price - السعر
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Discount Amount - مبلغ الخصم
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Net Amount - المبلغ الصافي
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Advance Amount - المبلغ المقدم
        /// </summary>
        public decimal AdvanceAmount { get; set; }

        /// <summary>
        /// Remaining Amount - المبلغ المتبقي
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Is Paid - مدفوع
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InvoiceId { get; set; }

        /// <summary>
        /// Subscription ID - معرف الإشتراك
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SubscriptionId { get; set; }

     
        /// <summary>
        /// Booked By Employee ID - حُجز بواسطة الموظف
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? BookedByEmployeeId { get; set; }

        /// <summary>
        /// Confirmed At - وقت التأكيد
        /// </summary>
        public DateTime? ConfirmedAt { get; set; }

        /// <summary>
        /// Cancelled At - وقت الإلغاء
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Cancellation Reason - سبب الإلغاء
        /// </summary>
        public string CancellationReason { get; set; }

        /// <summary>
        /// Is Reminder Sent - أُرسل تذكير
        /// </summary>
        public bool IsReminderSent { get; set; }

        /// <summary>
        /// Reminder Sent At - وقت إرسال التذكير
        /// </summary>
        public DateTime? ReminderSentAt { get; set; }

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

