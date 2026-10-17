using System;
using System.Collections.Generic;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Model.Finance.Zatca.Dto
{
    /// <summary>
    /// ZATCA Invoice Submission Request DTO - طلب إرسال فاتورة زاتكا
    /// </summary>
    public class ZatcaSubmitInvoiceRequestDto
    {
        /// <summary>
        /// Invoice ID (AR Invoice ID)
        /// </summary>
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Invoice Type (AR, CreditNote, DebitNote)
        /// </summary>
        public string InvoiceSourceType { get; set; } = "AR";

        /// <summary>
        /// Is B2B (requires clearance) or B2C (reporting only)
        /// If not specified, determined by customer VAT number
        /// </summary>
        public bool? IsB2B { get; set; }

        /// <summary>
        /// Force re-submission even if already submitted
        /// </summary>
        public bool ForceResubmit { get; set; } = false;
    }

    /// <summary>
    /// ZATCA Invoice Submission Response DTO - استجابة إرسال فاتورة زاتكا
    /// </summary>
    public class ZatcaSubmitInvoiceResponseDto
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string ZatcaUuid { get; set; }
        public long? InvoiceCounterValue { get; set; }
        public ZatcaSubmissionStatus Status { get; set; }
        public string DispositionStatus { get; set; }
        public string InvoiceHash { get; set; }
        public string QrCode { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ClearedAt { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ZatcaValidationResultDto> ValidationResults { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// ZATCA Validation Result DTO - نتيجة التحقق من زاتكا
    /// </summary>
    public class ZatcaValidationResultDto
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// ZATCA Invoice Status DTO - حالة فاتورة زاتكا
    /// </summary>
    public class ZatcaInvoiceStatusDto
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string ZatcaUuid { get; set; }
        public ZatcaInvoiceType? ZatcaInvoiceType { get; set; }
        public ZatcaInvoiceSubType? ZatcaInvoiceSubType { get; set; }
        public ZatcaSubmissionStatus? Status { get; set; }
        public long? InvoiceCounterValue { get; set; }
        public string InvoiceHash { get; set; }
        public bool HasQrCode { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ClearedAt { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// ZATCA Bulk Submit Request DTO - طلب إرسال دفعة من الفواتير
    /// </summary>
    public class ZatcaBulkSubmitRequestDto
    {
        public List<Guid> InvoiceIds { get; set; } = new();
        public string InvoiceSourceType { get; set; } = "AR";
        public bool StopOnFirstError { get; set; } = false;
    }

    /// <summary>
    /// ZATCA Bulk Submit Response DTO - استجابة إرسال دفعة من الفواتير
    /// </summary>
    public class ZatcaBulkSubmitResponseDto
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<ZatcaSubmitInvoiceResponseDto> Results { get; set; } = new();
    }

    /// <summary>
    /// ZATCA Invoice Hash Query DTO - استعلام هاش الفاتورة
    /// </summary>
    public class ZatcaInvoiceHashQueryDto
    {
        public Guid? InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceSourceType { get; set; }
        public ZatcaInvoiceType? ZatcaInvoiceType { get; set; }
        public ZatcaSubmissionStatus? SubmissionStatus { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// ZATCA Invoice Hash List DTO - قائمة هاش الفواتير
    /// </summary>
    public class ZatcaInvoiceHashListDto
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public string InvoiceSourceType { get; set; }
        public ZatcaInvoiceType ZatcaInvoiceType { get; set; }
        public ZatcaInvoiceSubType ZatcaInvoiceSubType { get; set; }
        public long InvoiceCounterValue { get; set; }
        public string InvoiceHash { get; set; }
        public string InvoiceUuid { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime IssueDateTime { get; set; }
        public ZatcaSubmissionStatus SubmissionStatus { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string ZatcaResponseCode { get; set; }
        public string ClearanceStatus { get; set; }
        public string ReportingStatus { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// ZATCA Get QR Code Response DTO - استجابة الحصول على رمز QR
    /// </summary>
    public class ZatcaQrCodeResponseDto
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string QrCodeBase64 { get; set; }
        public string QrCodeTlv { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// ZATCA Invoice XML Response DTO - استجابة XML الفاتورة
    /// </summary>
    public class ZatcaInvoiceXmlResponseDto
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string UnsignedXml { get; set; }
        public string SignedXml { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Pending ZATCA Invoices Query DTO - استعلام الفواتير المعلقة
    /// </summary>
    public class PendingZatcaInvoicesQueryDto
    {
        public string InvoiceSourceType { get; set; }
        public bool? IsB2B { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    /// <summary>
    /// Pending ZATCA Invoice DTO - فاتورة زاتكا معلقة
    /// </summary>
    public class PendingZatcaInvoiceDto
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceSourceType { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; }
        public string CustomerVatNumber { get; set; }
        public bool IsB2B { get; set; }
        public ZatcaSubmissionStatus? CurrentStatus { get; set; }
        public int FailedAttempts { get; set; }
        public string LastError { get; set; }
    }
}
