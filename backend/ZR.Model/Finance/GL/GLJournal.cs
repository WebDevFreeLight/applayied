using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.Finance.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.GL
{
    /// <summary>
    /// GL Journal - دفتر اليومية (القيد المحاسبي)
    /// This is now the main ledger entity, replacing GLVoucher
    /// </summary>
    [SugarTable("gl_journal")]
    // Unique constraint: JournalNumber must be unique per company
    [SugarIndex("UX_gl_journal_company_number", nameof(CompanyID), OrderByType.Asc, nameof(JournalNumber), OrderByType.Asc, true)]
    // Performance indexes for date-based queries
    [SugarIndex("IX_gl_journal_date", nameof(JournalDate), OrderByType.Desc)]
    [SugarIndex("IX_gl_journal_company_date", nameof(CompanyID), OrderByType.Asc, nameof(JournalDate), OrderByType.Desc)]
    // Status filtering indexes
    [SugarIndex("IX_gl_journal_status", nameof(Status), OrderByType.Asc)]
    [SugarIndex("IX_gl_journal_company_status", nameof(CompanyID), OrderByType.Asc, nameof(Status), OrderByType.Asc)]
    // Fiscal period reporting indexes
    [SugarIndex("IX_gl_journal_fiscal_period", nameof(FiscalYear), OrderByType.Asc, nameof(FiscalPeriod), OrderByType.Asc)]
    [SugarIndex("IX_gl_journal_company_fiscal", nameof(CompanyID), OrderByType.Asc, nameof(FiscalYear), OrderByType.Asc, nameof(FiscalPeriod), OrderByType.Asc)]
    // Source document lookup index
    [SugarIndex("IX_gl_journal_source_doc", nameof(SourceDocumentId), OrderByType.Asc)]
    // Posted date index for audit queries
    [SugarIndex("IX_gl_journal_posted_date", nameof(PostedDate), OrderByType.Desc)]
    // Reversal tracking indexes
    [SugarIndex("IX_gl_journal_original", nameof(OriginalJournalId), OrderByType.Asc)]
    [SugarIndex("IX_gl_journal_reversal", nameof(ReversalJournalId), OrderByType.Asc)]
    public class GLJournal : SysBase
    {
        /// <summary>
        /// Journal ID - معرف دفتر اليومية
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Journal Number - رقم القيد (unique per tenant)
        /// </summary>
        [SugarColumn(Length = 50, ColumnName = "journal_number")]
        public string JournalNumber { get; set; }

        /// <summary>
        /// Journal Date - تاريخ القيد
        /// </summary>
        [SugarColumn(ColumnName = "journal_date")]
        public DateTime JournalDate { get; set; }

        /// <summary>
        /// Description (Arabic) - الوصف بالعربي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description_ar")]
        public string DescriptionAr { get; set; }

        /// <summary>
        /// Description (English) - الوصف بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "description_en")]
        public string DescriptionEn { get; set; }

        /// <summary>
        /// Journal Status - حالة القيد
        /// </summary>
        [SugarColumn(ColumnName = "status")]
        public JournalStatus Status { get; set; } = JournalStatus.Draft;

        /// <summary>
        /// Source - المصدر (manual/auto/import/invoice/payment/depreciation)
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Source Document Type - نوع المستند المصدر
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "source_document_type")]
        public string SourceDocumentType { get; set; }

        /// <summary>
        /// Source Document ID - معرف المستند المصدر
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "source_document_id")]
        public Guid? SourceDocumentId { get; set; }

        /// <summary>
        /// Total Debit - إجمالي المدين
        /// </summary>
        [SugarColumn(ColumnName = "total_debit", Length = 18, DecimalDigits = 4)]
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// Total Credit - إجمالي الدائن
        /// </summary>
        [SugarColumn(ColumnName = "total_credit", Length = 18, DecimalDigits = 4)]
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Currency Code (legacy field, use CurrencyId instead)
        /// العملة (حقل قديم)
        /// </summary>
        [SugarColumn(Length = 10, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Currency ID (FK to currencies table) - PHASE 1
        /// معرف العملة
        /// RULE: Mandatory for all journals
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "currency_id")]
        public Guid? CurrencyId { get; set; }

        /// <summary>
        /// Exchange Rate - PHASE 1
        /// سعر الصرف
        /// RULE: Must exist for journal date, must be > 0
        /// RULE: For base currency, rate = 1.0
        /// </summary>
        [SugarColumn(Length = 18, DecimalDigits = 10, ColumnName = "exchange_rate")]
        public decimal ExchangeRate { get; set; } = 1.0m;

        /// <summary>
        /// Is Exchange Rate Manually Overridden? - PHASE 1
        /// هل سعر الصرف معدّل يدوياً؟
        /// </summary>
        [SugarColumn(ColumnName = "is_rate_override")]
        public bool IsRateOverride { get; set; }

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reference_number")]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Posted Date - تاريخ الترحيل
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "posted_date")]
        public DateTime? PostedDate { get; set; }

        /// <summary>
        /// Posted By - مرحّل بواسطة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "posted_by")]
        public Guid? PostedBy { get; set; }

        /// <summary>
        /// Approved Date - تاريخ الاعتماد
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_date")]
        public DateTime? ApprovedDate { get; set; }

        /// <summary>
        /// Approved By - معتمد بواسطة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "approved_by")]
        public Guid? ApprovedBy { get; set; }

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "fiscal_year")]
        public int? FiscalYear { get; set; }

        /// <summary>
        /// Fiscal Period - الفترة المالية
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "fiscal_period")]
        public int? FiscalPeriod { get; set; }

        /// <summary>
        /// Reversal Journal ID - معرف قيد العكس (if this is a reversal)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "reversal_journal_id")]
        public Guid? ReversalJournalId { get; set; }

        /// <summary>
        /// Original Journal ID - معرف القيد الأصلي (if this was reversed)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "original_journal_id")]
        public Guid? OriginalJournalId { get; set; }

        /// <summary>
        /// Journal Entries - قيود اليومية (Navigation Property)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<GLJournalEntry> Entries { get; set; }

        /// <summary>
        /// Check if journal is balanced - التحقق من توازن القيد
        /// </summary>
        public bool IsBalanced => TotalDebit == TotalCredit;
    }
}
