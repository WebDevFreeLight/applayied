using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.GL
{
    /// <summary>
    /// GL Account Balance Summary - ملخص أرصدة الحسابات
    /// Stores computed/aggregated balances for performance optimization
    /// Updated via triggers, background jobs, or on-demand calculation
    /// Essential for high-volume ERP systems
    /// </summary>
    [SugarTable("gl_account_balance_summary")]
    [SugarIndex("UK_gl_balance_summary", nameof(AccountId), OrderByType.Asc, nameof(FiscalYear), OrderByType.Asc, nameof(FiscalPeriod), OrderByType.Asc, true)]
    [SugarIndex("IX_gl_balance_summary_year_period", nameof(FiscalYear), OrderByType.Asc, nameof(FiscalPeriod), OrderByType.Asc)]
    [SugarIndex("IX_gl_balance_summary_company", nameof(CompanyID), OrderByType.Asc)]
    public class GLAccountBalanceSummary : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Balance Summary ID - معرف ملخص الرصيد
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "id")]
        public Guid Id { get; set; }

        #endregion

        #region Foreign Keys

        /// <summary>
        /// Account ID - معرف الحساب (FK to gl_account)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "account_id")]
        public Guid AccountId { get; set; }

        #endregion

        #region Period Definition

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Fiscal Period - الفترة المالية
        /// 0 = Annual summary, 1-12 = Monthly periods
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "fiscal_period")]
        public int FiscalPeriod { get; set; }

        /// <summary>
        /// Period Start Date - تاريخ بداية الفترة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period_start_date")]
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Period End Date - تاريخ نهاية الفترة
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period_end_date")]
        public DateTime PeriodEndDate { get; set; }

        #endregion

        #region Opening Balance (Beginning of Period)

        /// <summary>
        /// Opening Debit - الرصيد الافتتاحي مدين
        /// Balance at the beginning of the period
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "opening_debit", DefaultValue = "0")]
        public decimal OpeningDebit { get; set; } = 0;

        /// <summary>
        /// Opening Credit - الرصيد الافتتاحي دائن
        /// Balance at the beginning of the period
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "opening_credit", DefaultValue = "0")]
        public decimal OpeningCredit { get; set; } = 0;

        #endregion

        #region Period Movement (Transactions during Period)

        /// <summary>
        /// Period Debit - إجمالي الحركات مدين
        /// Total debits during the period
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "period_debit", DefaultValue = "0")]
        public decimal PeriodDebit { get; set; } = 0;

        /// <summary>
        /// Period Credit - إجمالي الحركات دائن
        /// Total credits during the period
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "period_credit", DefaultValue = "0")]
        public decimal PeriodCredit { get; set; } = 0;

        /// <summary>
        /// Period Net Movement - صافي الحركة
        /// Net change during the period (Debit - Credit)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "period_net", DefaultValue = "0")]
        public decimal PeriodNet { get; set; } = 0;

        #endregion

        #region Closing Balance (End of Period)

        /// <summary>
        /// Closing Debit - الرصيد الختامي مدين
        /// Balance at the end of the period
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "closing_debit", DefaultValue = "0")]
        public decimal ClosingDebit { get; set; } = 0;

        /// <summary>
        /// Closing Credit - الرصيد الختامي دائن
        /// Balance at the end of the period
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "closing_credit", DefaultValue = "0")]
        public decimal ClosingCredit { get; set; } = 0;

        /// <summary>
        /// Closing Balance - الرصيد الختامي
        /// Net closing balance (positive for debit balance accounts)
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "closing_balance", DefaultValue = "0")]
        public decimal ClosingBalance { get; set; } = 0;

        #endregion

        #region Year-to-Date (YTD) Values

        /// <summary>
        /// YTD Debit - إجمالي المدين من بداية السنة
        /// Year-to-date total debits
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "ytd_debit", DefaultValue = "0")]
        public decimal YtdDebit { get; set; } = 0;

        /// <summary>
        /// YTD Credit - إجمالي الدائن من بداية السنة
        /// Year-to-date total credits
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "ytd_credit", DefaultValue = "0")]
        public decimal YtdCredit { get; set; } = 0;

        /// <summary>
        /// YTD Balance - الرصيد من بداية السنة
        /// Year-to-date net balance
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "ytd_balance", DefaultValue = "0")]
        public decimal YtdBalance { get; set; } = 0;

        #endregion

        #region Foreign Currency Support

        /// <summary>
        /// Currency - العملة
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency", DefaultValue = "SAR")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Base Currency Period Debit - حركة المدين بالعملة الأساسية
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "base_period_debit", DefaultValue = "0")]
        public decimal BasePeriodDebit { get; set; } = 0;

        /// <summary>
        /// Base Currency Period Credit - حركة الدائن بالعملة الأساسية
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "base_period_credit", DefaultValue = "0")]
        public decimal BasePeriodCredit { get; set; } = 0;

        /// <summary>
        /// Base Currency Closing Balance - الرصيد الختامي بالعملة الأساسية
        /// </summary>
        [SugarColumn(DecimalDigits = 4, IsNullable = false, ColumnName = "base_closing_balance", DefaultValue = "0")]
        public decimal BaseClosingBalance { get; set; } = 0;

        #endregion

        #region Transaction Counts

        /// <summary>
        /// Transaction Count - عدد الحركات
        /// Number of transactions in the period
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "transaction_count", DefaultValue = "0")]
        public int TransactionCount { get; set; } = 0;

        /// <summary>
        /// Debit Transaction Count - عدد حركات المدين
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "debit_count", DefaultValue = "0")]
        public int DebitCount { get; set; } = 0;

        /// <summary>
        /// Credit Transaction Count - عدد حركات الدائن
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "credit_count", DefaultValue = "0")]
        public int CreditCount { get; set; } = 0;

        #endregion

        #region Calculation Metadata

        /// <summary>
        /// Last Calculated At - تاريخ آخر حساب
        /// When the balance was last computed
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "last_calculated_at")]
        public DateTime LastCalculatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last Voucher ID - معرف آخر سند
        /// Last voucher included in this calculation
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_voucher_id")]
        public Guid? LastVoucherId { get; set; }

        /// <summary>
        /// Last Voucher Date - تاريخ آخر سند
        /// Date of last voucher included
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_voucher_date")]
        public DateTime? LastVoucherDate { get; set; }

        /// <summary>
        /// Is Stale - يحتاج إعادة حساب
        /// Flag indicating balance needs recalculation
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_stale", DefaultValue = "0")]
        public bool IsStale { get; set; } = false;

        /// <summary>
        /// Is Period Closed - الفترة مقفلة
        /// Balance is finalized and locked
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_period_closed", DefaultValue = "0")]
        public bool IsPeriodClosed { get; set; } = false;

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Account - الحساب
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public GLAccount Account { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Recalculate all derived fields
        /// </summary>
        public void Recalculate()
        {
            // Period net movement
            PeriodNet = PeriodDebit - PeriodCredit;

            // Closing balance = Opening + Period movement
            ClosingDebit = OpeningDebit + PeriodDebit;
            ClosingCredit = OpeningCredit + PeriodCredit;
            ClosingBalance = ClosingDebit - ClosingCredit;

            // Total transaction count
            TransactionCount = DebitCount + CreditCount;

            // Update timestamp
            LastCalculatedAt = DateTime.Now;
            IsStale = false;
        }

        /// <summary>
        /// Add a transaction to the summary
        /// </summary>
        public void AddTransaction(decimal debit, decimal credit, decimal baseDebit, decimal baseCredit)
        {
            PeriodDebit += debit;
            PeriodCredit += credit;
            BasePeriodDebit += baseDebit;
            BasePeriodCredit += baseCredit;

            if (debit > 0) DebitCount++;
            if (credit > 0) CreditCount++;

            Recalculate();
        }

        /// <summary>
        /// Get closing balance based on account normal balance type
        /// </summary>
        public decimal GetClosingBalance(Enums.NormalBalance normalBalance)
        {
            return normalBalance == Enums.NormalBalance.Debit
                ? ClosingDebit - ClosingCredit
                : ClosingCredit - ClosingDebit;
        }

        #endregion
    }
}
