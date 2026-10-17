using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.EventManagement
{

    /// <summary>
    /// Event Ticket - تذكرة الفعالية
    /// </summary>
    [SugarTable("event_tickets")]
    
    [SqlDict(
        dictType: "ticket",
        dictName: "Event Ticket - التذاكر",
        valueColumn: "ticketnumber",
        labelColumns: "ticketnumber",
        RemarkColumn = "notes",
        OrderBy = "ticketnumber ASC",
        Description = "Auto-generated from EventTicket table"
    )]
    public class EventTicket : SysBase
    { 
        /// <summary>
        /// Ticket ID - معرف التذكرة
        /// </summary>
                [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TicketId { get; set; }

        /// <summary>
        /// Event ID - معرف الفعالية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }

        /// <summary>
        /// Visitor ID - معرف الزائر
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long VisitorId { get; set; }

        /// <summary>
        /// Ticket Number - رقم التذكرة
        /// </summary>
        
        public string TicketNumber { get; set; }

        /// <summary>
        /// Ticket Type - نوع التذكرة (general, vip, premium, free)
        /// </summary>
        
        public string TicketType { get; set; }

        /// <summary>
        /// Ticket Category - فئة التذكرة
        /// </summary>
        
        public string TicketCategory { get; set; }

        /// <summary>
        /// Price - السعر
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        
        public string Currency { get; set; }

        /// <summary>
        /// QR Code - رمز الاستجابة السريع
        /// </summary>
        
        public string QRCode { get; set; }

        /// <summary>
        /// Barcode - الباركود
        /// </summary>
        
        public string Barcode { get; set; }

        /// <summary>
        /// Status - الحالة (active, used, cancelled, expired)
        /// </summary>
        
        public string Status { get; set; }

        /// <summary>
        /// Is Paid - تم الدفع
        /// </summary>
        public bool IsPaid { get; set; }

        /// <summary>
        /// Payment Method - طريقة الدفع
        /// </summary>
        
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Payment Reference - مرجع الدفع
        /// </summary>
        
        public string PaymentReference { get; set; }

        /// <summary>
        /// Seat Number - رقم المقعد
        /// </summary>
        
        public string SeatNumber { get; set; }

        /// <summary>
        /// Zone Access - الوصول للمناطق (JSON array of zone IDs)
        /// </summary>
        
        public string ZoneAccess { get; set; }

        /// <summary>
        /// Can be Transferred - قابل للتحويل
        /// </summary>
        public bool CanBeTransferred { get; set; }

        /// <summary>
        /// Transferred To Visitor ID - تم التحويل إلى
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TransferredToVisitorId { get; set; }

        /// <summary>
        /// Transfer Date - تاريخ التحويل
        /// </summary>
        public DateTime? TransferDate { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Is Active - نشط
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Issue Date - تاريخ الإصدار
        /// </summary>
        public DateTime IssueDate { get; set; }
        
     

        /// <summary>
        /// Payment Status - حالة الدفع
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Is Checked In - هل تم تسجيل الدخول (Quick reference - automatically updated from TicketCheckin table)
        /// </summary>
        public bool IsCheckedIn { get; set; }

        /// <summary>
        /// Last Check-in At - آخر وقت دخول (Quick reference - automatically updated from TicketCheckin table)
        /// </summary>
        public DateTime? LastCheckinAt { get; set; }
    }
}

