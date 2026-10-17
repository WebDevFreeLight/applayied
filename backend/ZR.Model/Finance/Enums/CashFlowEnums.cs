using System;
using System.Collections.Generic;

namespace ZR.Model.Finance.Enums
{
    /// <summary>
    /// Cash Flow Category - تصنيف التدفق النقدي حسب IAS 7
    /// IAS 7 compliant classification for Cash Flow Statement
    /// </summary>
    public enum CashFlowCategory
    {
        /// <summary>
        /// None - غير محدد
        /// Account is not directly related to cash flow classification
        /// </summary>
        None = 0,

        /// <summary>
        /// Operating - تشغيلي
        /// Cash flows from operating activities
        /// IAS 7.14-15: Principal revenue-producing activities
        /// </summary>
        Operating = 1,

        /// <summary>
        /// Investing - استثماري
        /// Cash flows from investing activities
        /// IAS 7.16: Acquisition/disposal of long-term assets and investments
        /// </summary>
        Investing = 2,

        /// <summary>
        /// Financing - تمويلي
        /// Cash flows from financing activities
        /// IAS 7.17: Changes in equity and borrowings
        /// </summary>
        Financing = 3
    }

    /// <summary>
    /// Working Capital Type - نوع التغيير في رأس المال العامل
    /// Classification for working capital accounts in Cash Flow Statement
    /// </summary>
    public enum WorkingCapitalType
    {
        /// <summary>
        /// Receivables - المدينون
        /// Trade receivables and other receivables
        /// Increase = cash outflow (negative), Decrease = cash inflow (positive)
        /// </summary>
        Receivables = 1,

        /// <summary>
        /// Inventory - المخزون
        /// Raw materials, work in progress, finished goods
        /// Increase = cash outflow (negative), Decrease = cash inflow (positive)
        /// </summary>
        Inventory = 2,

        /// <summary>
        /// Prepaid - المصروفات المدفوعة مقدماً
        /// Prepaid expenses and other prepaid items
        /// Increase = cash outflow (negative), Decrease = cash inflow (positive)
        /// </summary>
        Prepaid = 3,

        /// <summary>
        /// Payables - الدائنون
        /// Trade payables and other payables
        /// Increase = cash inflow (positive), Decrease = cash outflow (negative)
        /// </summary>
        Payables = 4,

        /// <summary>
        /// Accrued - المصروفات المستحقة
        /// Accrued expenses and other accruals
        /// Increase = cash inflow (positive), Decrease = cash outflow (negative)
        /// </summary>
        Accrued = 5,

        /// <summary>
        /// OtherCurrentAsset - أصول متداولة أخرى
        /// Other current assets not classified elsewhere
        /// Increase = cash outflow (negative), Decrease = cash inflow (positive)
        /// </summary>
        OtherCurrentAsset = 6,

        /// <summary>
        /// OtherCurrentLiability - التزامات متداولة أخرى
        /// Other current liabilities not classified elsewhere
        /// Increase = cash inflow (positive), Decrease = cash outflow (negative)
        /// </summary>
        OtherCurrentLiability = 7
    }

    /// <summary>
    /// GL Account Classification Flags - أعلام تصنيف الحساب
    /// Allows multiple classifications per account using bitwise flags
    /// Used for Cash Flow Statement indirect method adjustments
    /// </summary>
    [Flags]
    public enum GLAccountFlags
    {
        /// <summary>
        /// None - لا شيء
        /// No special classification
        /// </summary>
        None = 0,

        /// <summary>
        /// Depreciation - إهلاك
        /// Account represents depreciation expense or accumulated depreciation
        /// Added back to net income in Cash Flow Statement (non-cash)
        /// </summary>
        Depreciation = 1,

        /// <summary>
        /// Amortization - إطفاء
        /// Account represents amortization expense or accumulated amortization
        /// Added back to net income in Cash Flow Statement (non-cash)
        /// </summary>
        Amortization = 2,

        /// <summary>
        /// Provision - مخصص
        /// Account represents provisions (allowances, reserves)
        /// Movement added back to net income in Cash Flow Statement (non-cash)
        /// </summary>
        Provision = 4,

        /// <summary>
        /// WorkingCapital - رأس المال العامل
        /// Account is a working capital account (receivables, payables, inventory)
        /// Changes affect operating cash flow
        /// </summary>
        WorkingCapital = 8,

        /// <summary>
        /// FixedAsset - أصول ثابتة
        /// Account represents property, plant, and equipment
        /// Affects investing activities in Cash Flow Statement
        /// </summary>
        FixedAsset = 16,

        /// <summary>
        /// IntangibleAsset - أصول غير ملموسة
        /// Account represents intangible assets (patents, goodwill, etc.)
        /// Affects investing activities in Cash Flow Statement
        /// </summary>
        IntangibleAsset = 32,

        /// <summary>
        /// Investment - استثمار
        /// Account represents investments in securities or other entities
        /// Affects investing activities in Cash Flow Statement
        /// </summary>
        Investment = 64,

        /// <summary>
        /// Loan - قرض
        /// Account represents loans (given or received)
        /// Affects financing or investing activities
        /// </summary>
        Loan = 128,

        /// <summary>
        /// Interest - فوائد
        /// Account represents interest income or expense
        /// May be classified as operating, investing, or financing per IAS 7.33
        /// </summary>
        Interest = 256,

        /// <summary>
        /// Dividend - أرباح موزعة
        /// Account represents dividend income or expense
        /// May be classified as operating, investing, or financing per IAS 7.33-34
        /// </summary>
        Dividend = 512,

        /// <summary>
        /// Tax - ضريبة
        /// Account represents income tax expense or liability
        /// Generally classified as operating unless specifically identified
        /// </summary>
        Tax = 1024,

        /// <summary>
        /// GainLossOnDisposal - ربح/خسارة بيع أصول
        /// Account represents gains or losses on asset disposal
        /// Non-cash item adjusted in operating activities
        /// </summary>
        GainLossOnDisposal = 2048,

        /// <summary>
        /// ForeignExchange - أرباح/خسائر صرف
        /// Account represents unrealized foreign exchange gains/losses
        /// Non-cash item adjusted in operating activities
        /// </summary>
        ForeignExchange = 4096,

        /// <summary>
        /// Impairment - انخفاض القيمة
        /// Account represents impairment losses
        /// Non-cash item added back in operating activities
        /// </summary>
        Impairment = 8192
    }

    /// <summary>
    /// GLAccountFlags Helper Extensions - مساعدات أعلام الحساب
    /// Provides logical grouping constants and helper methods for GLAccountFlags
    /// </summary>
    public static class GLAccountFlagsExtensions
    {
        #region Logical Flag Groupings

        /// <summary>
        /// Non-Cash Adjustment Flags - أعلام التعديلات غير النقدية
        /// Flags for accounts that represent non-cash adjustments in the indirect method
        /// These items are added back to net income in operating cash flow
        /// </summary>
        public const GLAccountFlags NonCashAdjustments =
            GLAccountFlags.Depreciation |
            GLAccountFlags.Amortization |
            GLAccountFlags.Provision |
            GLAccountFlags.GainLossOnDisposal |
            GLAccountFlags.ForeignExchange |
            GLAccountFlags.Impairment;

        /// <summary>
        /// Balance Sheet Classification Flags - أعلام تصنيف الميزانية
        /// Flags for balance sheet account classifications (mutually exclusive)
        /// An account should only have ONE of these flags set
        /// </summary>
        public const GLAccountFlags BalanceSheetClassifications =
            GLAccountFlags.FixedAsset |
            GLAccountFlags.IntangibleAsset |
            GLAccountFlags.Investment;

        /// <summary>
        /// Flexible Classification Flags - أعلام التصنيف المرن
        /// Flags for items with flexible IAS 7 classification (IAS 7.33-34)
        /// These can be classified as Operating, Investing, or Financing per accounting policy
        /// </summary>
        public const GLAccountFlags FlexibleClassifications =
            GLAccountFlags.Interest |
            GLAccountFlags.Dividend |
            GLAccountFlags.Tax;

        /// <summary>
        /// Flags Requiring Investing Category - أعلام تتطلب تصنيف استثماري
        /// Flags that require CashFlowCategory to be Investing
        /// </summary>
        public const GLAccountFlags RequiresInvesting =
            GLAccountFlags.FixedAsset |
            GLAccountFlags.IntangibleAsset |
            GLAccountFlags.Investment;

        /// <summary>
        /// Flags Incompatible with WorkingCapital - أعلام غير متوافقة مع رأس المال العامل
        /// Flags that cannot be combined with WorkingCapital flag
        /// </summary>
        public const GLAccountFlags IncompatibleWithWorkingCapital =
            GLAccountFlags.Depreciation |
            GLAccountFlags.Amortization |
            GLAccountFlags.FixedAsset |
            GLAccountFlags.IntangibleAsset |
            GLAccountFlags.Investment |
            GLAccountFlags.Impairment |
            GLAccountFlags.ForeignExchange;

        #endregion

        #region Extension Methods

        /// <summary>
        /// Check if flags contain any non-cash adjustment
        /// </summary>
        public static bool HasNonCashAdjustment(this GLAccountFlags flags)
            => (flags & NonCashAdjustments) != 0;

        /// <summary>
        /// Check if flags contain any balance sheet classification
        /// </summary>
        public static bool HasBalanceSheetClassification(this GLAccountFlags flags)
            => (flags & BalanceSheetClassifications) != 0;

        /// <summary>
        /// Count how many balance sheet classifications are set (should be 0 or 1)
        /// </summary>
        public static int CountBalanceSheetClassifications(this GLAccountFlags flags)
        {
            int count = 0;
            if (flags.HasFlag(GLAccountFlags.FixedAsset)) count++;
            if (flags.HasFlag(GLAccountFlags.IntangibleAsset)) count++;
            if (flags.HasFlag(GLAccountFlags.Investment)) count++;
            return count;
        }

        /// <summary>
        /// Check if flags are incompatible with WorkingCapital
        /// </summary>
        public static bool IsIncompatibleWithWorkingCapital(this GLAccountFlags flags)
            => (flags & IncompatibleWithWorkingCapital) != 0;

        /// <summary>
        /// Check if flags require Investing CashFlowCategory
        /// </summary>
        public static bool RequiresInvestingCategory(this GLAccountFlags flags)
            => (flags & RequiresInvesting) != 0;

        /// <summary>
        /// Check if flags are flexible classification items (IAS 7.33-34)
        /// </summary>
        public static bool IsFlexibleClassification(this GLAccountFlags flags)
            => (flags & FlexibleClassifications) != 0;

        /// <summary>
        /// Get list of active flag names
        /// </summary>
        public static List<string> GetActiveFlagNames(this GLAccountFlags flags)
        {
            var names = new List<string>();
            foreach (GLAccountFlags flag in global::System.Enum.GetValues(typeof(GLAccountFlags)))
            {
                if (flag != GLAccountFlags.None && flags.HasFlag(flag))
                    names.Add(flag.ToString());
            }
            return names;
        }

        /// <summary>
        /// Validate flag combinations and return validation errors
        /// </summary>
        public static List<string> ValidateFlagCombinations(
            this GLAccountFlags flags,
            WorkingCapitalType? workingCapitalType,
            CashFlowCategory cashFlowCategory)
        {
            var errors = new List<string>();

            // Rule: WorkingCapital flag requires WorkingCapitalType
            if (flags.HasFlag(GLAccountFlags.WorkingCapital) && !workingCapitalType.HasValue)
            {
                errors.Add("WorkingCapital flag requires a WorkingCapitalType to be specified");
            }

            // Rule: WorkingCapitalType requires WorkingCapital flag
            if (workingCapitalType.HasValue && !flags.HasFlag(GLAccountFlags.WorkingCapital))
            {
                errors.Add("WorkingCapitalType specified without WorkingCapital flag");
            }

            // Rule: Check incompatible flags with WorkingCapital
            if (flags.HasFlag(GLAccountFlags.WorkingCapital) && flags.IsIncompatibleWithWorkingCapital())
            {
                var incompatible = flags & IncompatibleWithWorkingCapital;
                errors.Add($"WorkingCapital flag cannot be combined with: {incompatible}");
            }

            // Rule: Only one balance sheet classification allowed
            if (flags.CountBalanceSheetClassifications() > 1)
            {
                errors.Add("Account can only have one balance sheet classification (FixedAsset, IntangibleAsset, or Investment)");
            }

            // Rule: Balance sheet classifications require Investing category
            if (flags.RequiresInvestingCategory() &&
                cashFlowCategory != CashFlowCategory.None &&
                cashFlowCategory != CashFlowCategory.Investing)
            {
                errors.Add("FixedAsset, IntangibleAsset, and Investment accounts should be in Investing category");
            }

            // Rule: Loan requires Financing or Investing
            if (flags.HasFlag(GLAccountFlags.Loan) &&
                cashFlowCategory != CashFlowCategory.None &&
                cashFlowCategory != CashFlowCategory.Financing &&
                cashFlowCategory != CashFlowCategory.Investing)
            {
                errors.Add("Loan accounts should be in Financing or Investing category");
            }

            // Rule: ForeignExchange + FixedAsset is invalid
            if (flags.HasFlag(GLAccountFlags.ForeignExchange) &&
                flags.HasFlag(GLAccountFlags.FixedAsset))
            {
                errors.Add("Account cannot be both ForeignExchange and FixedAsset");
            }

            return errors;
        }

        /// <summary>
        /// Get validation warnings for flag combinations
        /// </summary>
        public static List<string> GetValidationWarnings(
            this GLAccountFlags flags,
            CashFlowCategory cashFlowCategory)
        {
            var warnings = new List<string>();

            // Warning: Interest without category
            if (flags.HasFlag(GLAccountFlags.Interest) &&
                cashFlowCategory == CashFlowCategory.None)
            {
                warnings.Add("Interest account without CashFlowCategory. Per IAS 7.33, can be Operating, Investing, or Financing.");
            }

            // Warning: Dividend without category
            if (flags.HasFlag(GLAccountFlags.Dividend) &&
                cashFlowCategory == CashFlowCategory.None)
            {
                warnings.Add("Dividend account without CashFlowCategory. Per IAS 7.33-34, can be Operating, Investing, or Financing.");
            }

            // Warning: Tax without category
            if (flags.HasFlag(GLAccountFlags.Tax) &&
                cashFlowCategory == CashFlowCategory.None)
            {
                warnings.Add("Tax account without CashFlowCategory. Per IAS 7.35, generally Operating.");
            }

            return warnings;
        }

        #endregion
    }
}
