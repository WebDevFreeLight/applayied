using System;
using SqlSugar;
using ZR.Model.Finance.PaymentHub.Enums;

namespace ZR.Model.Finance.PaymentHub
{
    /// <summary>
    /// Payment Event - حدث الدفعة
    /// Append-only audit trail for all payment lifecycle events.
    /// IMMUTABLE - once created, never updated or deleted.
    /// </summary>
    [SugarTable("hub_payment_event", "Payment Events - أحداث الدفعات")]
    public class PaymentEvent
    {
        #region Primary Key

        /// <summary>
        /// Event ID - معرف الحدث
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        #endregion

        #region Payment Reference

        /// <summary>
        /// Payment ID - معرف الدفعة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "payment_id")]
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Payment Number - رقم الدفعة
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "payment_number")]
        public string PaymentNumber { get; set; }

        #endregion

        #region Event Details

        /// <summary>
        /// Event Type - نوع الحدث
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "event_type")]
        public PaymentEventType EventType { get; set; }

        /// <summary>
        /// Event Timestamp (UTC) - الطابع الزمني
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "event_timestamp")]
        public DateTime EventTimestamp { get; set; }

        /// <summary>
        /// Sequence Number within Payment - الرقم التسلسلي ضمن الدفعة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "sequence_number")]
        public int SequenceNumber { get; set; }

        #endregion

        #region State Snapshot

        /// <summary>
        /// Previous Status - الحالة السابقة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "previous_status")]
        public PaymentStatus? PreviousStatus { get; set; }

        /// <summary>
        /// New Status - الحالة الجديدة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "new_status")]
        public PaymentStatus? NewStatus { get; set; }

        /// <summary>
        /// Amount - المبلغ
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "amount", DecimalDigits = 4)]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 3, IsNullable = true, ColumnName = "currency")]
        public string Currency { get; set; }

        #endregion

        #region Related Entities

        /// <summary>
        /// Allocation ID (for allocation events) - معرف التخصيص
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "allocation_id")]
        public Guid? AllocationId { get; set; }

        /// <summary>
        /// Document ID (invoice ID for allocations) - معرف المستند
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "document_id")]
        public Guid? DocumentId { get; set; }

        /// <summary>
        /// GL Journal ID - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_journal_id")]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Balance Ledger Entry ID - معرف قيد سجل الرصيد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "balance_ledger_entry_id")]
        public Guid? BalanceLedgerEntryId { get; set; }

        #endregion

        #region User & Context

        /// <summary>
        /// User ID - معرف المستخدم
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, ColumnName = "user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// User Name - اسم المستخدم
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnName = "user_name")]
        public string UserName { get; set; }

        /// <summary>
        /// IP Address - عنوان IP
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "ip_address")]
        public string IpAddress { get; set; }

        /// <summary>
        /// User Agent - وكيل المستخدم
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "user_agent")]
        public string UserAgent { get; set; }

        #endregion

        #region Tracing

        /// <summary>
        /// Correlation ID (for distributed tracing) - معرف الارتباط
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "correlation_id")]
        public string CorrelationId { get; set; }

        /// <summary>
        /// Request ID - معرف الطلب
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "request_id")]
        public string RequestId { get; set; }

        #endregion

        #region Details

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Event Data (JSON) - بيانات الحدث
        /// Stores additional context-specific data
        /// </summary>
        [SugarColumn(Length = 4000, IsNullable = true, ColumnName = "event_data")]
        public string EventData { get; set; }

        /// <summary>
        /// Error Message (if event failed) - رسالة الخطأ
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "error_message")]
        public string ErrorMessage { get; set; }

        #endregion

        #region Multi-Tenant

        /// <summary>
        /// Company ID - معرف الشركة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "company_id")]
        public long? CompanyId { get; set; }

        #endregion

        #region Factory Methods

        /// <summary>
        /// Create a new payment event
        /// </summary>
        public static PaymentEvent Create(
            Guid paymentId,
            string paymentNumber,
            PaymentEventType eventType,
            string userId,
            string description = null,
            PaymentStatus? previousStatus = null,
            PaymentStatus? newStatus = null,
            decimal? amount = null,
            string currency = null,
            Guid? allocationId = null,
            Guid? documentId = null,
            Guid? glJournalId = null,
            string eventData = null,
            long? companyId = null)
        {
            return new PaymentEvent
            {
                Id = Guid.NewGuid(),
                PaymentId = paymentId,
                PaymentNumber = paymentNumber,
                EventType = eventType,
                EventTimestamp = DateTime.UtcNow,
                UserId = userId,
                Description = description,
                PreviousStatus = previousStatus,
                NewStatus = newStatus,
                Amount = amount,
                Currency = currency,
                AllocationId = allocationId,
                DocumentId = documentId,
                GLJournalId = glJournalId,
                EventData = eventData,
                CompanyId = companyId
            };
        }

        #endregion
    }
}
