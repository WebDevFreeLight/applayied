using System;
using SqlSugar;
using ZR.Model.System;

namespace ZR.Model.Finance.GL
{
    /// <summary>
    /// GL Account Opening Balance - الأرصدة الافتتاحية للحسابات
    /// Stores opening balances per account per fiscal year
    /// Essential for multi-year accounting and year-end closing
    /// </summary>
    [SugarTable("gl_account_opening_balance")]
    [SugarIndex("UK_gl_opening_balance", nameof(AccountId), OrderByType.Asc, nameof(FiscalYear), OrderByType.Asc, true)]
    [SugarIndex("IX_gl_opening_balance_year", nameof(FiscalYear), OrderByType.Asc)]
    [SugarIndex("IX_gl_opening_balance_company", nameof(CompanyID), OrderByType.Asc)]
    public class GLAccountOpeningBalance : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Opening Balance ID - معرف الرصيد الافتتاحي
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

        #region Fiscal Period

        /// <summary>
        /// Fiscal Year - السنة المالية
        /// e.g., 2024, 2025
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "fiscal_year")]
        public int FiscalYear { get; set; }

        /// <summary>
        /// Period Start Date - تاريخ بداية الفترة
        /// Typically Jan 1 or company fiscal year start
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period_start_date")]
        public DateTime PeriodStartDate { get; set; }

        /// <summary>
        /// Period End Date - تاريخ نهاية الفترة
        /// Typically Dec 31 or company fiscal year end
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period_end_date")]
        public DateTime PeriodEndDate { get; set; }

        #endregion

        #region Opening Balance Values

        /// <summary>
        /// Opening Debit - الرصيد الافتتاحي مدين
        /// Beginning of period debit balance
        /// </summary>
        [SugarColumn(Length = 18, DecimalDigits = 4, IsNullable = false, ColumnName = "opening_debit", DefaultValue = "0")]
        public decimal OpeningDebit { get; set; } = 0;

        /// <summary>
        /// Opening Credit - الرصيد الافتتاحي دائن
        /// Beginning of period credit balance
        /// </summary>
        [SugarColumn(Length = 18, DecimalDigits = 4, IsNullable = false, ColumnName = "opening_credit", DefaultValue = "0")]
        public decimal OpeningCredit { get; set; } = 0;

        /// <summary>
        /// Opening Balance (Net) - صافي الرصيد الافتتاحي
        /// Computed: Debit - Credit (positive for debit balance accounts)
        /// This should match the closing balance of the previous year
        /// </summary>
        [SugarColumn(Length = 18, DecimalDigits = 4, IsNullable = false, ColumnName = "opening_balance", DefaultValue = "0")]
        public decimal OpeningBalance { get; set; } = 0;

        #endregion

        #region Foreign Currency Support

        /// <summary>
        /// Currency - العملة
        /// Currency of the opening balance
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency", DefaultValue = "SAR")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Opening Debit (Base Currency) - الرصيد الافتتاحي مدين بالعملة الأساسية
        /// For foreign currency accounts
        /// </summary>
        [SugarColumn(Length = 18, DecimalDigits = 4, IsNullable = false, ColumnName = "base_opening_debit", DefaultValue = "0")]
        public decimal BaseOpeningDebit { get; set; } = 0;

        /// <summary>
        /// Opening Credit (Base Currency) - الرصيد الافتتاحي دائن بالعملة الأساسية
        /// For foreign currency accounts
        /// </summary>
        [SugarColumn(Length = 18, DecimalDigits = 4, IsNullable = false, ColumnName = "base_opening_credit", DefaultValue = "0")]
        public decimal BaseOpeningCredit { get; set; } = 0;

        /// <summary>
        /// Exchange Rate - سعر الصرف
        /// Rate at time of opening balance
        /// </summary>
        [SugarColumn(DecimalDigits = 6, IsNullable = false, ColumnName = "exchange_rate", DefaultValue = "1")]
        public decimal ExchangeRate { get; set; } = 1;

        #endregion

        #region Source Tracking

        /// <summary>
        /// Source Type - مصدر الرصيد
        /// "Manual" = User entered, "Carryforward" = Auto from previous year closing, "Migration" = Data migration
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnName = "source_type", DefaultValue = "Manual")]
        public string SourceType { get; set; } = "Manual";

        /// <summary>
        /// Source Reference - مرجع المصدر
        /// Reference to source document (e.g., closing voucher ID)
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "source_reference")]
        public string SourceReference { get; set; }

        /// <summary>
        /// Is Locked - مقفل
        /// Locked after year-end close, cannot be modified
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_locked", DefaultValue = "0")]
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Locked At - تاريخ القفل
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "locked_at")]
        public DateTime? LockedAt { get; set; }

        /// <summary>
        /// Locked By - قفل بواسطة
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "locked_by")]
        public Guid? LockedBy { get; set; }

        #endregion

        #region Notes

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

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
        /// Calculate net opening balance based on normal balance type
        /// </summary>
        public decimal GetNetBalance(Enums.NormalBalance normalBalance)
        {
            return normalBalance == Enums.NormalBalance.Debit
                ? OpeningDebit - OpeningCredit
                : OpeningCredit - OpeningDebit;
        }

        /// <summary>
        /// Update opening balance from debit/credit values
        /// </summary>
        public void RecalculateOpeningBalance()
        {
            OpeningBalance = OpeningDebit - OpeningCredit;
        }

        #endregion
    }
}
