using System;
using SqlSugar;
using ZR.Model.Finance.GL;
using ZR.Model.System;

namespace ZR.Model.Finance.Tax
{
    /// <summary>
    /// Tax Journal Line - سطر ضريبة القيد
    /// Records tax calculations for audit trail
    /// Links to GL Journal entries and source documents (invoices, payments)
    /// IMMUTABLE: Once journal is posted, tax lines cannot be modified
    /// </summary>
    [SugarTable("tax_journal_line")]
    [SugarIndex("IX_tax_journal_line_journal", nameof(JournalId), OrderByType.Asc)]
    [SugarIndex("IX_tax_journal_line_invoice", nameof(InvoiceId), OrderByType.Asc)]
    [SugarIndex("IX_tax_journal_line_company_date", nameof(CompanyID), OrderByType.Asc, nameof(TaxDate), OrderByType.Desc)]
    public class TaxJournalLine : SysBase
    {
        /// <summary>
        /// Tax Journal Line ID
        /// معرف سطر ضريبة القيد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Company ID for multi-tenant isolation
        /// معرف الشركة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "company_id")]
        public new long? CompanyID { get; set; }

        /// <summary>
        /// GL Journal ID (FK to gl_journal)
        /// معرف قيد اليومية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "journal_id")]
        public Guid? JournalId { get; set; }

        /// <summary>
        /// GL Journal Entry ID (specific line within journal)
        /// معرف سطر القيد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "journal_entry_id")]
        public Guid? JournalEntryId { get; set; }

        /// <summary>
        /// Invoice ID (FK to AR/AP invoices)
        /// معرف الفاتورة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "invoice_id")]
        public Guid? InvoiceId { get; set; }

        /// <summary>
        /// Invoice Line ID (specific line item)
        /// معرف سطر الفاتورة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "invoice_line_id")]
        public Guid? InvoiceLineId { get; set; }

        /// <summary>
        /// Source Module (AP, AR, MANUAL, ASSET, EXPENSE, etc.)
        /// الوحدة المصدرية
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 50, ColumnName = "source_module")]
        public string SourceModule { get; set; }

        /// <summary>
        /// Source Document ID (FK to invoices, payments, etc.)
        /// معرف المستند المصدري
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "source_document_id")]
        public Guid? SourceDocumentId { get; set; }

        /// <summary>
        /// Source Document Number (invoice number, journal number, etc.)
        /// رقم المستند المصدري
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 100, ColumnName = "source_document_number")]
        public string SourceDocumentNumber { get; set; }

        /// <summary>
        /// Tax Code ID (FK to tax_code)
        /// معرف رمز الضريبة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "tax_code_id")]
        public Guid TaxCodeId { get; set; }

        /// <summary>
        /// Tax Code (denormalized for reporting)
        /// رمز الضريبة
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 50, ColumnName = "tax_code")]
        public string TaxCodeValue { get; set; }

        /// <summary>
        /// Tax Rate applied (percentage or fixed)
        /// معدل الضريبة المطبق
        /// </summary>
        [SugarColumn(IsNullable = false, DecimalDigits = 6, ColumnName = "tax_rate")]
        public decimal TaxRate { get; set; }

        /// <summary>
        /// Is tax inclusive in the line amount?
        /// هل الضريبة شاملة في مبلغ السطر؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_tax_inclusive")]
        public bool IsTaxInclusive { get; set; }

        /// <summary>
        /// Taxable Amount in Base Currency
        /// المبلغ الخاضع للضريبة بالعملة الأساسية
        /// </summary>
        [SugarColumn(IsNullable = false, DecimalDigits = 4, ColumnName = "taxable_amount_base")]
        public decimal TaxableAmountBase { get; set; }

        /// <summary>
        /// Taxable Amount in Foreign Currency
        /// المبلغ الخاضع للضريبة بالعملة الأجنبية
        /// </summary>
        [SugarColumn(IsNullable = true, DecimalDigits = 4, ColumnName = "taxable_amount_foreign")]
        public decimal? TaxableAmountForeign { get; set; }

        /// <summary>
        /// Tax Amount in Base Currency
        /// مبلغ الضريبة بالعملة الأساسية
        /// </summary>
        [SugarColumn(IsNullable = false, DecimalDigits = 4, ColumnName = "tax_amount_base")]
        public decimal TaxAmountBase { get; set; }

        /// <summary>
        /// Tax Amount in Foreign Currency
        /// مبلغ الضريبة بالعملة الأجنبية
        /// </summary>
        [SugarColumn(IsNullable = true, DecimalDigits = 4, ColumnName = "tax_amount_foreign")]
        public decimal? TaxAmountForeign { get; set; }

        /// <summary>
        /// Currency Code (ISO 4217)
        /// رمز العملة
        /// </summary>
        [SugarColumn(IsNullable = false, Length = 10, ColumnName = "currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Exchange Rate used for conversion
        /// سعر الصرف المستخدم للتحويل
        /// </summary>
        [SugarColumn(IsNullable = false, DecimalDigits = 6, ColumnName = "exchange_rate")]
        public decimal ExchangeRate { get; set; } = 1m;

        /// <summary>
        /// Date used for tax calculation (may differ from posting date)
        /// التاريخ المستخدم لحساب الضريبة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "tax_date")]
        public DateTime TaxDate { get; set; }

        /// <summary>
        /// Tax GL Account ID (payable or recoverable)
        /// معرف حساب الضريبة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "tax_account_id")]
        public Guid? TaxAccountId { get; set; }

        /// <summary>
        /// Is this a recoverable (input) tax? (false = payable/output)
        /// هل هذه ضريبة قابلة للاسترداد (مدخلات)؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_recoverable")]
        public bool IsRecoverable { get; set; }

        /// <summary>
        /// Is this a reverse charge entry?
        /// هل هذا قيد احتساب عكسي؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_reverse_charge")]
        public bool IsReverseCharge { get; set; } = false;

        /// <summary>
        /// Rounding delta (difference due to rounding)
        /// فرق التقريب
        /// </summary>
        [SugarColumn(IsNullable = false, DecimalDigits = 4, ColumnName = "rounding_delta")]
        public decimal RoundingDelta { get; set; } = 0m;

        /// <summary>
        /// Rounding mode used
        /// نمط التقريب المستخدم
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 20, ColumnName = "rounding_mode")]
        public string RoundingMode { get; set; }

        /// <summary>
        /// Is this line posted (immutable)?
        /// هل هذا السطر مرحّل (غير قابل للتعديل)؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_posted")]
        public bool IsPosted { get; set; } = false;

        /// <summary>
        /// Posted at timestamp
        /// تاريخ الترحيل
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "posted_at")]
        public DateTime? PostedAt { get; set; }

        /// <summary>
        /// Posted by user
        /// مرحّل بواسطة
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 64, ColumnName = "posted_by")]
        public string PostedBy { get; set; }

        /// <summary>
        /// Is this a manual override?
        /// هل هذا تجاوز يدوي؟
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_manual_override")]
        public bool IsManualOverride { get; set; } = false;

        /// <summary>
        /// Override reason (required if manual override)
        /// سبب التجاوز
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500, ColumnName = "override_reason")]
        public string OverrideReason { get; set; }

        /// <summary>
        /// Notes/Comments
        /// ملاحظات
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 1000, ColumnName = "notes")]
        public string Notes { get; set; }

        // Navigation properties
        [SugarColumn(IsIgnore = true)]
        public TaxCode TaxCodeEntity { get; set; }

        [SugarColumn(IsIgnore = true)]
        public GLJournal Journal { get; set; }

        [SugarColumn(IsIgnore = true)]
        public GLAccount TaxAccount { get; set; }
    }
}
