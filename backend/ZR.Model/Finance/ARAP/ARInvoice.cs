using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.Customers;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.ARAP
{
    /// <summary>
    /// AR Invoice - فاتورة العملاء
    /// </summary>
    [SugarTable("arap_ar_invoice")]
    public class ARInvoice : SysBase
    {
        /// <summary>
        /// Invoice ID - معرف الفاتورة
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Invoice Number - رقم الفاتورة
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "invoice_number")]
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Customer ID - معرف العميل
        /// </summary>
        [SugarColumn(ColumnName = "customer_id")]
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Invoice Date - تاريخ الفاتورة
        /// </summary>
        [SugarColumn(ColumnName = "invoice_date")]
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Due Date - تاريخ الاستحقاق
        /// </summary>
        [SugarColumn(ColumnName = "due_date")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 10, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Exchange Rate - سعر الصرف
        /// </summary>
        [SugarColumn(ColumnName = "exchange_rate", DecimalDigits = 6)]
        public decimal ExchangeRate { get; set; } = 1;

        /// <summary>
        /// Subtotal (Sum of line amounts before tax) - المجموع الفرعي
        /// </summary>
        [SugarColumn(ColumnName = "subtotal", DecimalDigits = 4)]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Tax Amount - مبلغ الضريبة
        /// </summary>
        [SugarColumn(ColumnName = "tax_amount", DecimalDigits = 4)]
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>
        /// Total Amount - المبلغ الإجمالي
        /// </summary>
        [SugarColumn(ColumnName = "total_amount", DecimalDigits = 4)]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Paid Amount - المبلغ المدفوع
        /// </summary>
        [SugarColumn(ColumnName = "paid_amount", DecimalDigits = 4)]
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Remaining Amount - المبلغ المتبقي
        /// </summary>
        [SugarColumn(ColumnName = "remaining_amount", DecimalDigits = 4)]
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Status - الحالة
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        /// <summary>
        /// Tax Group ID - معرف مجموعة الضريبة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "tax_group_id")]
        public Guid? TaxGroupId { get; set; }

        /// <summary>
        /// Is Tax Inclusive - هل الضريبة مشمولة
        /// If true, line amounts include tax; if false, tax is added on top
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_tax_inclusive")]
        public bool IsTaxInclusive { get; set; } = false;

        /// <summary>
        /// Description - الوصف
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reference_number")]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// GL Journal ID - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "gl_journal_id")]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// Fiscal Year ID - معرف السنة المالية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "fiscal_year_id")]
        public Guid? FiscalYearId { get; set; }

        /// <summary>
        /// Fiscal Period ID - معرف الفترة المالية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "fiscal_period_id")]
        public Guid? FiscalPeriodId { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        #region ZATCA E-Invoicing Fields - حقول الفوترة الإلكترونية زاتكا

        /// <summary>
        /// ZATCA UUID - المعرف الفريد العالمي لزاتكا
        /// UUIDv4 assigned when invoice is prepared for ZATCA submission
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 36, ColumnName = "zatca_uuid")]
        public string ZatcaUuid { get; set; }

        /// <summary>
        /// ZATCA Invoice Type - نوع فاتورة زاتكا
        /// Standard (388) or Simplified (3880)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_invoice_type")]
        public ZatcaInvoiceType? ZatcaInvoiceType { get; set; }

        /// <summary>
        /// ZATCA Invoice Sub Type - النوع الفرعي للفاتورة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_invoice_sub_type")]
        public ZatcaInvoiceSubType? ZatcaInvoiceSubType { get; set; }

        /// <summary>
        /// ZATCA Submission Status - حالة الإرسال لزاتكا
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_status")]
        public ZatcaSubmissionStatus? ZatcaStatus { get; set; }

        /// <summary>
        /// ZATCA Invoice Hash ID - معرف هاش الفاتورة
        /// FK to zatca_invoice_hash table
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_hash_id")]
        public Guid? ZatcaHashId { get; set; }

        /// <summary>
        /// Invoice Counter Value (ICV) - قيمة عداد الفاتورة
        /// Sequential counter used for ZATCA
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_counter")]
        public long? ZatcaCounter { get; set; }

        /// <summary>
        /// QR Code - رمز QR
        /// TLV Base64 encoded QR code for ZATCA
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString, ColumnName = "zatca_qr_code")]
        public string ZatcaQrCode { get; set; }

        /// <summary>
        /// Invoice Hash - هاش الفاتورة
        /// SHA-256 hash of the signed XML
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 100, ColumnName = "zatca_hash")]
        public string ZatcaHash { get; set; }

        /// <summary>
        /// ZATCA Submitted At - تاريخ الإرسال لزاتكا
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_submitted_at")]
        public DateTime? ZatcaSubmittedAt { get; set; }

        /// <summary>
        /// ZATCA Cleared/Reported At - تاريخ المقاصة/الإبلاغ
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_cleared_at")]
        public DateTime? ZatcaClearedAt { get; set; }

        /// <summary>
        /// ZATCA Payment Means Code - كود وسيلة الدفع
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "zatca_payment_means_code")]
        public ZatcaPaymentMeansCode? ZatcaPaymentMeansCode { get; set; }

        /// <summary>
        /// Is B2B Invoice - فاتورة B2B (تتطلب مقاصة)
        /// If true, requires clearance; if false, requires reporting only
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "is_b2b")]
        public bool? IsB2B { get; set; }

        /// <summary>
        /// Supply Date - تاريخ التوريد
        /// Date when goods/services were supplied (required by ZATCA)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "supply_date")]
        public DateTime? SupplyDate { get; set; }

        /// <summary>
        /// Supply End Date - تاريخ نهاية التوريد
        /// For services spanning multiple dates
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "supply_end_date")]
        public DateTime? SupplyEndDate { get; set; }

        #endregion

        /// <summary>
        /// Invoice Lines (Navigation Property) - بنود الفاتورة
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<ARInvoiceLine> Lines { get; set; }

        /// <summary>
        /// Customer (Navigation Property) - العميل
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public Customer Customer { get; set; }
    }
}


