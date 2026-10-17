using System;
using System.Collections.Generic;
using SqlSugar;
using ZR.Model.Finance.Enums;
using ZR.Model.System;

namespace ZR.Model.Finance.GL
{
    /// <summary>
    /// GL Account - دليل الحسابات المالية
    /// IFRS Compliant Chart of Accounts entity
    /// </summary>
    [SugarTable("gl_account")]
    // Unique constraint: FullAccountNumber must be unique per company
    [SugarIndex("UX_gl_account_company_full_number", nameof(CompanyID), OrderByType.Asc, nameof(FullAccountNumber), OrderByType.Asc, true)]
    // Unique constraint: AccountNumber must be unique among siblings (same parent) per company
    [SugarIndex("UX_gl_account_company_parent_number", nameof(CompanyID), OrderByType.Asc, nameof(ParentAccountId), OrderByType.Asc, nameof(AccountNumber), OrderByType.Asc, true)]
    // Performance indexes
    [SugarIndex("IX_gl_account_parent", nameof(ParentAccountId), OrderByType.Asc)]
    [SugarIndex("IX_gl_account_type", nameof(AccountType), OrderByType.Asc)]
    [SugarIndex("IX_gl_account_company_active", nameof(CompanyID), OrderByType.Asc, nameof(IsActive), OrderByType.Asc)]
    public class GLAccount : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Account ID - معرف الحساب
        /// Using Guid with sequential generation recommended for performance
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "account_id")]
        public Guid AccountId { get; set; }

        #endregion

        #region Account Identification

        /// <summary>
        /// Account Number - رقم الحساب المحلي
        /// Local code within the current level
        /// e.g., 1000, 1100, 1110
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnName = "account_number")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// Full Account Number - رقم الحساب الكامل
        /// Complete hierarchical account code
        /// e.g., "1000.1100.1110" for nested accounts
        /// Auto-generated: Parent.FullAccountNumber + "." + AccountNumber
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false, ColumnName = "full_account_number")]
        public string FullAccountNumber { get; set; }

        /// <summary>
        /// Account Name (Arabic) - اسم الحساب بالعربي
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false, ColumnName = "account_name_ar")]
        public string AccountNameAr { get; set; }

        /// <summary>
        /// Account Name (English) - اسم الحساب بالإنجليزي
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false, ColumnName = "account_name_en")]
        public string AccountNameEn { get; set; }

        #endregion

        #region Account Classification (IFRS)

        /// <summary>
        /// Account Type - نوع الحساب
        /// IFRS compliant classification: Asset, Liability, Equity, Revenue, Expense, etc.
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "account_type")]
        public AccountType AccountType { get; set; }

        /// <summary>
        /// Normal Balance - طبيعة الحساب
        /// Debit for Assets/Expenses, Credit for Liabilities/Equity/Revenue
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "normal_balance")]
        public NormalBalance NormalBalance { get; set; }

        #endregion

        #region Hierarchy Structure

        /// <summary>
        /// Account Level - مستوى الحساب
        /// 1 = Root/Main category, 2 = Sub-category, etc.
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "level")]
        public int Level { get; set; } = 1;

        /// <summary>
        /// Parent Account ID - معرف الحساب الأب
        /// Null for root accounts
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "parent_account_id")]
        public Guid? ParentAccountId { get; set; }

        /// <summary>
        /// Is Leaf Account - حساب نهائي
        /// Only leaf accounts can have journal entries posted to them
        /// Automatically managed: set to false when children exist
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_leaf", DefaultValue = "1")]
        public bool IsLeaf { get; set; } = true;

        /// <summary>
        /// Account Path - مسار الحساب
        /// Materialized path for efficient hierarchy queries
        /// Format: "parentId1/parentId2/currentId"
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "account_path")]
        public string AccountPath { get; set; }

        /// <summary>
        /// Display Order - ترتيب العرض
        /// For custom sorting within the same level
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "display_order", DefaultValue = "0")]
        public int DisplayOrder { get; set; } = 0;

        /// <summary>
        /// Child Count - عدد الحسابات الفرعية المباشرة
        /// Number of direct children, auto-updated on child create/delete
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "child_count", DefaultValue = "0")]
        public int ChildCount { get; set; } = 0;

        /// <summary>
        /// Descendant Count - عدد جميع الحسابات التابعة
        /// Total descendants at all levels, auto-updated recursively
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "descendant_count", DefaultValue = "0")]
        public int DescendantCount { get; set; } = 0;

        #endregion

        #region Account Behavior Flags

        /// <summary>
        /// Is Active - نشط
        /// Inactive accounts cannot be used in new transactions
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_active", DefaultValue = "1")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Allow Manual Entries - السماح بإدخال قيود يدوية
        /// Some accounts may only accept system-generated entries
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "allow_manual_entries", DefaultValue = "1")]
        public bool AllowManualEntries { get; set; } = true;

        /// <summary>
        /// Allow Posting To Parent - السماح بالترحيل للحساب الأب
        /// Exception flag: allows posting even if not a leaf account
        /// Use with caution - against standard practice
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "allow_posting_to_parent", DefaultValue = "0")]
        public bool AllowPostingToParent { get; set; } = false;

        /// <summary>
        /// Dimension Requirements - متطلبات الأبعاد المحاسبية
        /// Flags to indicate which dimensions are required for journal entries on this account
        /// Replaces RequiresCostCenter, RequiresDepartment, RequiresProject boolean fields
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "dimension_requirements", DefaultValue = "0")]
        public DimensionRequirements DimensionRequirements { get; set; } = DimensionRequirements.None;

        /// <summary>
        /// Requires Cost Center - يتطلب مركز تكلفة
        /// Computed property for backward compatibility
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool RequiresCostCenter
        {
            get => DimensionRequirements.HasFlag(DimensionRequirements.CostCenter);
            set
            {
                if (value)
                    DimensionRequirements |= DimensionRequirements.CostCenter;
                else
                    DimensionRequirements &= ~DimensionRequirements.CostCenter;
            }
        }

        /// <summary>
        /// Requires Department - يتطلب قسم
        /// Computed property for backward compatibility
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool RequiresDepartment
        {
            get => DimensionRequirements.HasFlag(DimensionRequirements.Department);
            set
            {
                if (value)
                    DimensionRequirements |= DimensionRequirements.Department;
                else
                    DimensionRequirements &= ~DimensionRequirements.Department;
            }
        }

        /// <summary>
        /// Requires Project - يتطلب مشروع
        /// Computed property for backward compatibility
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool RequiresProject
        {
            get => DimensionRequirements.HasFlag(DimensionRequirements.Project);
            set
            {
                if (value)
                    DimensionRequirements |= DimensionRequirements.Project;
                else
                    DimensionRequirements &= ~DimensionRequirements.Project;
            }
        }

        #endregion

        #region Account Nature Flags

        /// <summary>
        /// Account Nature - طبيعة الحساب (نقدي/بنكي)
        /// Replaces IsCashAccount and IsBankAccount boolean fields
        /// None = neither cash nor bank, Cash = petty cash, Bank = bank account
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "account_nature", DefaultValue = "0")]
        public AccountNature AccountNature { get; set; } = AccountNature.None;

        /// <summary>
        /// Is Control Account - حساب مراقبة
        /// For AR/AP control accounts that aggregate subsidiary ledgers
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_control_account", DefaultValue = "0")]
        public bool IsControlAccount { get; set; } = false;

        /// <summary>
        /// Reconciliation Mode - وضع التسوية
        /// Indicates whether this account requires periodic reconciliation
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "reconciliation_mode", DefaultValue = "0")]
        public ReconciliationMode ReconciliationMode { get; set; } = ReconciliationMode.None;

        /// <summary>
        /// Last Reconciled Date - تاريخ آخر تسوية
        /// Date when the account was last reconciled
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_reconciled_date")]
        public DateTime? LastReconciledDate { get; set; }

        /// <summary>
        /// Last Reconciled By - آخر من قام بالتسوية
        /// User who performed the last reconciliation
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "last_reconciled_by")]
        public string LastReconciledBy { get; set; }

        /// <summary>
        /// Reconciliation Status - حالة التسوية
        /// Current reconciliation state of the account
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "reconciliation_status", DefaultValue = "0")]
        public ReconciliationStatus ReconciliationStatus { get; set; } = ReconciliationStatus.NotReconciled;

        /// <summary>
        /// Is System Account - حساب نظام
        /// Cannot be modified or deleted by users
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_system_account", DefaultValue = "0")]
        public bool IsSystemAccount { get; set; } = false;

        /// <summary>
        /// Is Retained Earnings Account - حساب الأرباح المحتجزة
        /// Used for Year-End closing to transfer net income/loss
        /// Only one account should have this flag set to true per company
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_retained_earnings_account", DefaultValue = "0")]
        public bool IsRetainedEarningsAccount { get; set; } = false;

        #endregion

        #region Cash Flow & Reporting Classification (IAS 7)

        /// <summary>
        /// Cash Flow Category - تصنيف التدفق النقدي حسب IAS 7
        /// Determines which section of the Cash Flow Statement this account affects
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "cash_flow_category", DefaultValue = "0")]
        public CashFlowCategory CashFlowCategory { get; set; } = CashFlowCategory.None;

        /// <summary>
        /// GL Account Classification Flags - أعلام تصنيف الحساب
        /// Bitwise flags for account classification (Depreciation, Amortization, Provision, etc.)
        /// Allows multiple classifications per account for accurate Cash Flow reporting
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "account_flags", DefaultValue = "0")]
        public GLAccountFlags AccountFlags { get; set; } = GLAccountFlags.None;

        /// <summary>
        /// Working Capital Type - نوع رأس المال العامل
        /// Specifies the type of working capital account for Cash Flow calculations
        /// Only applicable when AccountFlags includes WorkingCapital flag
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "working_capital_type")]
        public WorkingCapitalType? WorkingCapitalType { get; set; }

        #endregion

        #region Currency Settings

        /// <summary>
        /// Currency - العملة
        /// Account currency, defaults to base currency
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnName = "currency", DefaultValue = "SAR")]
        public string Currency { get; set; } = "SAR";

        /// <summary>
        /// Is Foreign Currency Account - حساب عملة أجنبية
        /// Requires exchange rate tracking
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_foreign_currency", DefaultValue = "0")]
        public bool IsForeignCurrency { get; set; } = false;

        #endregion

        #region Description

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

        #endregion

        #region Audit Trail (Version Control)

        /// <summary>
        /// Row Version - إصدار السجل
        /// For optimistic concurrency control
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "row_version", DefaultValue = "1")]
        public int RowVersion { get; set; } = 1;

        /// <summary>
        /// Last Transaction Date - تاريخ آخر عملية
        /// Auto-updated when entries are posted
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_transaction_date")]
        public DateTime? LastTransactionDate { get; set; }

        #endregion

        #region External Integration

        /// <summary>
        /// External Code - الرمز الخارجي
        /// For integration with external systems
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnName = "external_code")]
        public string ExternalCode { get; set; }

        /// <summary>
        /// Tax Code - رمز الضريبة
        /// Associated tax treatment code (VAT, etc.)
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnName = "tax_code")]
        public string TaxCode { get; set; }

        #endregion

        #region Navigation Properties (Ignored in DB)

        /// <summary>
        /// Parent Account - الحساب الأب
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public GLAccount ParentAccount { get; set; }

        /// <summary>
        /// Child Accounts - الحسابات الفرعية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<GLAccount> ChildAccounts { get; set; }

        /// <summary>
        /// Opening Balances - الأرصدة الافتتاحية
        /// Per fiscal year
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<GLAccountOpeningBalance> OpeningBalances { get; set; }

        /// <summary>
        /// Balance Summary - ملخص الأرصدة
        /// Computed balances for performance
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public GLAccountBalanceSummary BalanceSummary { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Build the full account number from parent's full account number
        /// </summary>
        /// <param name="parentFullAccountNumber">Parent's FullAccountNumber, null for root accounts</param>
        /// <returns>The computed FullAccountNumber</returns>
        public string BuildFullAccountNumber(string parentFullAccountNumber)
        {
            if (string.IsNullOrEmpty(parentFullAccountNumber))
            {
                // Root account: FullAccountNumber = AccountNumber
                return AccountNumber;
            }
            // Child account: FullAccountNumber = Parent.FullAccountNumber + "." + AccountNumber
            return $"{parentFullAccountNumber}.{AccountNumber}";
        }

        /// <summary>
        /// Check if this account can accept journal entries
        /// </summary>
        public bool CanAcceptEntries()
        {
            // Must be active
            if (!IsActive) return false;

            // Must be leaf OR have explicit permission for parent posting
            if (!IsLeaf && !AllowPostingToParent) return false;

            return true;
        }

        /// <summary>
        /// Get the expected balance sign based on normal balance
        /// Positive for natural direction, negative for unnatural
        /// </summary>
        public int GetBalanceSign()
        {
            return NormalBalance == NormalBalance.Debit ? 1 : -1;
        }

        /// <summary>
        /// Calculate balance from debit and credit totals
        /// Respects the account's normal balance type
        /// </summary>
        public decimal CalculateBalance(decimal totalDebit, decimal totalCredit)
        {
            return NormalBalance == NormalBalance.Debit
                ? totalDebit - totalCredit
                : totalCredit - totalDebit;
        }

        #endregion

        #region Cash Flow Classification Helper Methods

        /// <summary>
        /// Check if account is depreciation or amortization
        /// Used for non-cash adjustments in Cash Flow Statement
        /// </summary>
        public bool IsDepreciationOrAmortization()
        {
            return AccountFlags.HasFlag(GLAccountFlags.Depreciation) ||
                   AccountFlags.HasFlag(GLAccountFlags.Amortization);
        }

        /// <summary>
        /// Check if account is a provision account
        /// Used for non-cash adjustments in Cash Flow Statement
        /// </summary>
        public bool IsProvision()
        {
            return AccountFlags.HasFlag(GLAccountFlags.Provision);
        }

        /// <summary>
        /// Check if account is a working capital account
        /// Used for working capital changes in Cash Flow Statement
        /// </summary>
        public bool IsWorkingCapital()
        {
            return AccountFlags.HasFlag(GLAccountFlags.WorkingCapital);
        }

        /// <summary>
        /// Check if account is a fixed asset account
        /// Used for investing activities in Cash Flow Statement
        /// </summary>
        public bool IsFixedAsset()
        {
            return AccountFlags.HasFlag(GLAccountFlags.FixedAsset);
        }

        /// <summary>
        /// Check if account is an intangible asset account
        /// Used for investing activities in Cash Flow Statement
        /// </summary>
        public bool IsIntangibleAsset()
        {
            return AccountFlags.HasFlag(GLAccountFlags.IntangibleAsset);
        }

        /// <summary>
        /// Check if account is an investment account
        /// Used for investing activities in Cash Flow Statement
        /// </summary>
        public bool IsInvestment()
        {
            return AccountFlags.HasFlag(GLAccountFlags.Investment);
        }

        /// <summary>
        /// Check if account is a loan account
        /// Used for investing/financing activities in Cash Flow Statement
        /// </summary>
        public bool IsLoan()
        {
            return AccountFlags.HasFlag(GLAccountFlags.Loan);
        }

        /// <summary>
        /// Check if account represents interest
        /// Used for Cash Flow classification per IAS 7.33
        /// </summary>
        public bool IsInterest()
        {
            return AccountFlags.HasFlag(GLAccountFlags.Interest);
        }

        /// <summary>
        /// Check if account represents dividends
        /// Used for Cash Flow classification per IAS 7.33-34
        /// </summary>
        public bool IsDividend()
        {
            return AccountFlags.HasFlag(GLAccountFlags.Dividend);
        }

        /// <summary>
        /// Check if account is a tax account
        /// Used for Cash Flow classification per IAS 7.35
        /// </summary>
        public bool IsTax()
        {
            return AccountFlags.HasFlag(GLAccountFlags.Tax);
        }

        /// <summary>
        /// Check if account represents gain/loss on disposal
        /// Used for non-cash adjustments in Cash Flow Statement
        /// </summary>
        public bool IsGainLossOnDisposal()
        {
            return AccountFlags.HasFlag(GLAccountFlags.GainLossOnDisposal);
        }

        /// <summary>
        /// Check if account represents foreign exchange
        /// Used for non-cash adjustments in Cash Flow Statement
        /// </summary>
        public bool IsForeignExchange()
        {
            return AccountFlags.HasFlag(GLAccountFlags.ForeignExchange);
        }

        /// <summary>
        /// Check if account represents impairment
        /// Used for non-cash adjustments in Cash Flow Statement
        /// </summary>
        public bool IsImpairment()
        {
            return AccountFlags.HasFlag(GLAccountFlags.Impairment);
        }

        /// <summary>
        /// Check if account is a cash or bank account
        /// Used for opening/closing cash balance in Cash Flow Statement
        /// </summary>
        public bool IsCashOrBank()
        {
            return AccountNature == AccountNature.Cash || AccountNature == AccountNature.Bank;
        }

        /// <summary>
        /// Check if this is a non-cash adjustment account for indirect method
        /// Includes: depreciation, amortization, provisions, impairment, FX, gain/loss on disposal
        /// </summary>
        public bool IsNonCashAdjustment()
        {
            return IsDepreciationOrAmortization() ||
                   IsProvision() ||
                   IsImpairment() ||
                   IsForeignExchange() ||
                   IsGainLossOnDisposal();
        }

        /// <summary>
        /// Check if working capital change is an asset (increase = outflow)
        /// Returns null if not a working capital account
        /// </summary>
        public bool? IsWorkingCapitalAsset()
        {
            if (!IsWorkingCapital() || !WorkingCapitalType.HasValue)
                return null;

            return WorkingCapitalType.Value switch
            {
                Enums.WorkingCapitalType.Receivables => true,
                Enums.WorkingCapitalType.Inventory => true,
                Enums.WorkingCapitalType.Prepaid => true,
                Enums.WorkingCapitalType.OtherCurrentAsset => true,
                Enums.WorkingCapitalType.Payables => false,
                Enums.WorkingCapitalType.Accrued => false,
                Enums.WorkingCapitalType.OtherCurrentLiability => false,
                _ => null
            };
        }

        /// <summary>
        /// Get the cash flow impact sign for working capital changes
        /// For assets: negative (increase = outflow)
        /// For liabilities: positive (increase = inflow)
        /// </summary>
        public int GetWorkingCapitalSign()
        {
            var isAsset = IsWorkingCapitalAsset();
            if (isAsset == null) return 0;
            return isAsset.Value ? -1 : 1;
        }

        #endregion
    }
}

