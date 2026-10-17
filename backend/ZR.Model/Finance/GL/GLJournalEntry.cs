using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.GL
{
    /// <summary>
    /// GL Journal Entry - قيد دفتر اليومية (خط القيد)
    /// This is now the main entry entity, replacing GLVoucherEntry
    /// </summary>
    [SugarTable("gl_journal_entry")]
    // FK indexes - critical for joins and balance calculations
    [SugarIndex("IX_gl_entry_journal", nameof(JournalId), OrderByType.Asc)]
    [SugarIndex("IX_gl_entry_account", nameof(AccountId), OrderByType.Asc)]
    // Combined index for balance calculations per account (most critical)
    [SugarIndex("IX_gl_entry_account_journal", nameof(AccountId), OrderByType.Asc, nameof(JournalId), OrderByType.Asc)]
    // Cost center analysis indexes
    [SugarIndex("IX_gl_entry_cost_center", nameof(CostCenterId), OrderByType.Asc)]
    [SugarIndex("IX_gl_entry_account_cost_center", nameof(AccountId), OrderByType.Asc, nameof(CostCenterId), OrderByType.Asc)]
    // Project analysis indexes
    [SugarIndex("IX_gl_entry_project", nameof(ProjectId), OrderByType.Asc)]
    [SugarIndex("IX_gl_entry_account_project", nameof(AccountId), OrderByType.Asc, nameof(ProjectId), OrderByType.Asc)]
    // Department analysis index
    [SugarIndex("IX_gl_entry_department", nameof(DepartmentId), OrderByType.Asc)]
    // Company isolation index
    [SugarIndex("IX_gl_entry_company", nameof(CompanyID), OrderByType.Asc)]
    [SugarIndex("IX_gl_entry_company_account", nameof(CompanyID), OrderByType.Asc, nameof(AccountId), OrderByType.Asc)]
    public class GLJournalEntry : SysBase
    {
        /// <summary>
        /// Entry ID - معرف القيد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Journal ID - معرف دفتر اليومية (FK)
        /// </summary>
        [SugarColumn(ColumnName = "journal_id")]
        public Guid JournalId { get; set; }

        /// <summary>
        /// Account ID - معرف الحساب (FK to gl_account)
        /// </summary>
        [SugarColumn(ColumnName = "account_id")]
        public Guid AccountId { get; set; }

        /// <summary>
        /// Line Number - رقم السطر
        /// </summary>
        [SugarColumn(ColumnName = "line_number")]
        public int LineNumber { get; set; }

        /// <summary>
        /// Debit Amount (Foreign Currency) - مبلغ المدين بالعملة الأجنبية
        /// PHASE 1: This is the foreign currency amount
        /// </summary>
        [SugarColumn(ColumnName = "debit", Length = 18, DecimalDigits = 4)]
        public decimal Debit { get; set; }

        /// <summary>
        /// Credit Amount (Foreign Currency) - مبلغ الدائن بالعملة الأجنبية
        /// PHASE 1: This is the foreign currency amount
        /// </summary>
        [SugarColumn(ColumnName = "credit", Length = 18, DecimalDigits = 4)]
        public decimal Credit { get; set; }

        /// <summary>
        /// Debit Amount in Foreign Currency (Alias for Debit)
        /// المبلغ المدين بالعملة الأجنبية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal AmountForeignDebit
        {
            get => Debit;
            set => Debit = value;
        }

        /// <summary>
        /// Credit Amount in Foreign Currency (Alias for Credit)
        /// المبلغ الدائن بالعملة الأجنبية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal AmountForeignCredit
        {
            get => Credit;
            set => Credit = value;
        }

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
        /// Cost Center ID - معرف مركز التكلفة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "cost_center_id")]
        public Guid? CostCenterId { get; set; }

        /// <summary>
        /// Department ID - معرف القسم
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "department_id")]
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// Project ID - معرف المشروع
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "project_id")]
        public Guid? ProjectId { get; set; }

        /// <summary>
        /// Reference Number - رقم المرجع
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "reference_number")]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 10, ColumnName = "currency")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Exchange Rate - سعر الصرف
        /// </summary>
        [SugarColumn(ColumnName = "exchange_rate", Length = 18, DecimalDigits = 6)]
        public decimal ExchangeRate { get; set; } = 1;

        /// <summary>
        /// Base Currency Debit - المدين بالعملة الأساسية
        /// PHASE 1: Auto-calculated = Debit × ExchangeRate, rounded to 2 decimals
        /// RULE: BaseDebit total MUST equal BaseCredit total (exact 0.00)
        /// </summary>
        [SugarColumn(ColumnName = "base_debit", Length = 18, DecimalDigits = 4)]
        public decimal BaseDebit { get; set; }

        /// <summary>
        /// Base Currency Credit - الدائن بالعملة الأساسية
        /// PHASE 1: Auto-calculated = Credit × ExchangeRate, rounded to 2 decimals
        /// RULE: BaseCredit total MUST equal BaseDebit total (exact 0.00)
        /// </summary>
        [SugarColumn(ColumnName = "base_credit", Length = 18, DecimalDigits = 4)]
        public decimal BaseCredit { get; set; }

        /// <summary>
        /// Base Currency Debit Amount (Alias for BaseDebit)
        /// المبلغ المدين بالعملة الأساسية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal AmountBaseDebit
        {
            get => BaseDebit;
            set => BaseDebit = value;
        }

        /// <summary>
        /// Base Currency Credit Amount (Alias for BaseCredit)
        /// المبلغ الدائن بالعملة الأساسية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal AmountBaseCredit
        {
            get => BaseCredit;
            set => BaseCredit = value;
        }

        /// <summary>
        /// Journal (Navigation Property) - دفتر اليومية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public GLJournal Journal { get; set; }

        /// <summary>
        /// Account (Navigation Property) - الحساب
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public GLAccount Account { get; set; }
    }
}
