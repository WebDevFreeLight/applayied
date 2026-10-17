using System;
using SqlSugar;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.Zatca
{
    /// <summary>
    /// ZATCA Invoice Hash - هاش فاتورة زاتكا (PIH Tracking)
    /// Tracks Previous Invoice Hash (PIH) for invoice chain integrity
    /// Each invoice must reference the hash of the previous invoice
    /// </summary>
    [SugarTable("zatca_invoice_hash")]
    [SugarIndex("IX_zatca_hash_company_counter", nameof(CompanyID), OrderByType.Asc, nameof(InvoiceCounterValue), OrderByType.Desc)]
    [SugarIndex("IX_zatca_hash_invoice", nameof(InvoiceId), OrderByType.Asc)]
    public class ZatcaInvoiceHash : SysBase
    {
        /// <summary>
        /// Hash Record ID - معرف سجل الهاش
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Invoice ID - معرف الفاتورة (FK to AR/AP Invoice)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "invoice_id")]
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Invoice Type (AR/AP/CreditNote/DebitNote) - نوع الفاتورة
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 20, ColumnName = "invoice_type")]
        public string InvoiceSourceType { get; set; }

        /// <summary>
        /// ZATCA Invoice Type - نوع فاتورة زاتكا
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "zatca_invoice_type")]
        public ZatcaInvoiceType ZatcaInvoiceType { get; set; }

        /// <summary>
        /// ZATCA Invoice Sub Type - النوع الفرعي للفاتورة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "zatca_invoice_sub_type")]
        public ZatcaInvoiceSubType ZatcaInvoiceSubType { get; set; }

        /// <summary>
        /// Invoice Counter Value (ICV) - قيمة عداد الفاتورة
        /// Sequential counter for all invoices
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "invoice_counter_value")]
        public long InvoiceCounterValue { get; set; }

        /// <summary>
        /// Invoice Hash (SHA-256 of XML) - هاش الفاتورة
        /// Base64 encoded SHA-256 hash of the invoice XML
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 100, ColumnName = "invoice_hash")]
        public string InvoiceHash { get; set; }

        /// <summary>
        /// Previous Invoice Hash (PIH) - هاش الفاتورة السابقة
        /// For first invoice, use base64 of "0"
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 100, ColumnName = "previous_invoice_hash")]
        public string PreviousInvoiceHash { get; set; }

        /// <summary>
        /// Invoice UUID - المعرف الفريد العالمي للفاتورة
        /// UUIDv4 assigned to the invoice
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 36, ColumnName = "invoice_uuid")]
        public string InvoiceUuid { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 50, ColumnName = "invoice_number")]
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Issue Date Time - تاريخ ووقت الإصدار
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "issue_date_time")]
        public DateTime IssueDateTime { get; set; }

        /// <summary>
        /// Device ID - معرف الجهاز
        /// FK to ZatcaDevice that generated this invoice
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "device_id")]
        public Guid DeviceId { get; set; }

        /// <summary>
        /// Signed XML - XML الموقّع
        /// Complete signed invoice XML
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "signed_xml")]
        public string SignedXml { get; set; }

        /// <summary>
        /// QR Code (TLV Base64) - رمز QR
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "qr_code")]
        public string QrCode { get; set; }

        /// <summary>
        /// Cryptographic Stamp - الختم المشفر
        /// Digital signature value
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "cryptographic_stamp")]
        public string CryptographicStamp { get; set; }

        #region Submission Status

        /// <summary>
        /// Submission Status - حالة الإرسال
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "submission_status")]
        public ZatcaSubmissionStatus SubmissionStatus { get; set; } = ZatcaSubmissionStatus.NotSubmitted;

        /// <summary>
        /// Submitted At - تاريخ الإرسال
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "submitted_at")]
        public DateTime? SubmittedAt { get; set; }

        /// <summary>
        /// ZATCA Response Code - كود استجابة زاتكا
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 20, ColumnName = "zatca_response_code")]
        public string ZatcaResponseCode { get; set; }

        /// <summary>
        /// ZATCA Clearance Status - حالة المقاصة
        /// For standard invoices
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 50, ColumnName = "clearance_status")]
        public string ClearanceStatus { get; set; }

        /// <summary>
        /// ZATCA Reporting Status - حالة الإبلاغ
        /// For simplified invoices
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 50, ColumnName = "reporting_status")]
        public string ReportingStatus { get; set; }

        /// <summary>
        /// Validation Results (JSON) - نتائج التحقق
        /// Stores warnings and info messages from ZATCA
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "validation_results")]
        public string ValidationResults { get; set; }

        /// <summary>
        /// Error Messages (JSON) - رسائل الخطأ
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "error_messages")]
        public string ErrorMessages { get; set; }

        /// <summary>
        /// Warning Count - عدد التحذيرات
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "warning_count")]
        public int WarningCount { get; set; } = 0;

        /// <summary>
        /// Error Count - عدد الأخطاء
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "error_count")]
        public int ErrorCount { get; set; } = 0;

        /// <summary>
        /// Retry Count - عدد المحاولات
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "retry_count")]
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Last Retry At - آخر محاولة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_retry_at")]
        public DateTime? LastRetryAt { get; set; }

        #endregion

        /// <summary>
        /// Initial PIH value for first invoice (base64 of "0")
        /// </summary>
        public const string InitialPreviousInvoiceHash = "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
    }
}
