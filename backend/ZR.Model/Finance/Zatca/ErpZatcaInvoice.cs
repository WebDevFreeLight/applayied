using Infrastructure.Converter;
using SqlSugar;
using Newtonsoft.Json;
using ZR.Model.System;

namespace ZR.Model.Finance.ZATCA
{
    /// <summary>
    /// ZATCA Invoice - فاتورة هيئة الزكاة
    /// </summary>
    [SugarTable("erp_zatca_invoices")]
    public class ErpZatcaInvoice : SysBase
    {
        /// <summary>
        /// ZATCA Invoice ID - معرف فاتورة الزكاة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ZatcaInvoiceId { get; set; }

        /// <summary>
        /// Invoice ID - معرف الفاتورة الأصلية
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InvoiceId { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Invoice Type - نوع الفاتورة (388=Tax Invoice, 381=Credit Note, 383=Debit Note)
        /// </summary>
        public string InvoiceType { get; set; } = "388";

        /// <summary>
        /// Invoice Sub Type - نوع الفاتورة الفرعي (01=B2B, 02=B2C)
        /// </summary>
        public string InvoiceSubType { get; set; } = "02";

        /// <summary>
        /// UUID - المعرف الفريد
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// Invoice Counter Value - قيمة عداد الفواتير
        /// </summary>
        public int InvoiceCounterValue { get; set; }

        /// <summary>
        /// Previous Invoice Hash - تجزئة الفاتورة السابقة
        /// </summary>
        public string PreviousInvoiceHash { get; set; }

        /// <summary>
        /// Invoice Hash - تجزئة الفاتورة
        /// </summary>
        public string InvoiceHash { get; set; }

        /// <summary>
        /// XML Content - محتوى XML
        /// </summary>
        public string XmlContent { get; set; }

        /// <summary>
        /// Signed XML - XML الموقع
        /// </summary>
        public string SignedXml { get; set; }

        /// <summary>
        /// QR Code - رمز QR
        /// </summary>
        public string QrCode { get; set; }

        /// <summary>
        /// Cryptographic Stamp - الختم المشفر
        /// </summary>
        public string CryptographicStamp { get; set; }

        /// <summary>
        /// Submission Status - حالة الإرسال (pending/submitted/accepted/rejected)
        /// </summary>
        public string SubmissionStatus { get; set; } = "pending";

        /// <summary>
        /// Clearance Status - حالة التحقق (pending/cleared/rejected)
        /// </summary>
        public string ClearanceStatus { get; set; }

        /// <summary>
        /// Reporting Status - حالة الإبلاغ
        /// </summary>
        public string ReportingStatus { get; set; }

        /// <summary>
        /// Submission Date - تاريخ الإرسال
        /// </summary>
        public DateTime? SubmissionDate { get; set; }

        /// <summary>
        /// Response Date - تاريخ الرد
        /// </summary>
        public DateTime? ResponseDate { get; set; }

        /// <summary>
        /// ZATCA Response - رد الهيئة (JSON)
        /// </summary>
        public string ZatcaResponse { get; set; }

        /// <summary>
        /// Validation Results - نتائج التحقق (JSON)
        /// </summary>
        public string ValidationResults { get; set; }

        /// <summary>
        /// Error Messages - رسائل الخطأ (JSON)
        /// </summary>
        public string ErrorMessages { get; set; }

        /// <summary>
        /// Warning Messages - رسائل التحذير (JSON)
        /// </summary>
        public string WarningMessages { get; set; }

        /// <summary>
        /// Retry Count - عدد المحاولات
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Last Retry Date - تاريخ آخر محاولة
        /// </summary>
        public DateTime? LastRetryDate { get; set; }

        

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        public string Notes { get; set; }
    }
}

