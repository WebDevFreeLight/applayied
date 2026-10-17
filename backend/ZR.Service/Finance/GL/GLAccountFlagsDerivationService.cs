using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ZR.Model.Finance.Enums;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// GL Account Flags Derivation Service - خدمة اشتقاق أعلام الحساب تلقائياً
    /// Automatically derives GLAccountFlags, CashFlowCategory, and WorkingCapitalType
    /// based on AccountType, FullAccountNumber, and AccountName
    /// 
    /// FIXED: Added comprehensive null checks to prevent NullReferenceException during import
    /// </summary>
    public class GLAccountFlagsDerivationService
    {
        #region Keyword Dictionaries

        private static readonly Dictionary<string, GLAccountFlags> EnglishKeywords = new()
        {
            // Depreciation & Amortization
            { "depreciation", GLAccountFlags.Depreciation },
            { "accumulated depreciation", GLAccountFlags.Depreciation },
            { "amortization", GLAccountFlags.Amortization },
            { "accumulated amortization", GLAccountFlags.Amortization },
            
            // Provisions & Allowances
            { "provision", GLAccountFlags.Provision },
            { "allowance", GLAccountFlags.Provision },
            { "doubtful", GLAccountFlags.Provision },
            { "bad debt", GLAccountFlags.Provision },
            { "impairment", GLAccountFlags.Impairment },
            
            // Fixed Assets
            { "property", GLAccountFlags.FixedAsset },
            { "plant", GLAccountFlags.FixedAsset },
            { "equipment", GLAccountFlags.FixedAsset },
            { "machinery", GLAccountFlags.FixedAsset },
            { "vehicles", GLAccountFlags.FixedAsset },
            { "furniture", GLAccountFlags.FixedAsset },
            { "buildings", GLAccountFlags.FixedAsset },
            { "land", GLAccountFlags.FixedAsset },
            
            // Intangible Assets
            { "intangible", GLAccountFlags.IntangibleAsset },
            { "goodwill", GLAccountFlags.IntangibleAsset },
            { "patent", GLAccountFlags.IntangibleAsset },
            { "trademark", GLAccountFlags.IntangibleAsset },
            { "software", GLAccountFlags.IntangibleAsset },
            { "license", GLAccountFlags.IntangibleAsset },
            
            // Investments
            { "investment", GLAccountFlags.Investment },
            { "securities", GLAccountFlags.Investment },
            { "shares", GLAccountFlags.Investment },
            { "bonds", GLAccountFlags.Investment },
            
            // Loans
            { "loan", GLAccountFlags.Loan },
            { "borrowing", GLAccountFlags.Loan },
            { "mortgage", GLAccountFlags.Loan },
            
            // Interest & Dividends
            { "interest income", GLAccountFlags.Interest },
            { "interest expense", GLAccountFlags.Interest },
            { "finance cost", GLAccountFlags.Interest },
            { "dividend", GLAccountFlags.Dividend },
            
            // Tax
            { "tax", GLAccountFlags.Tax },
            { "vat", GLAccountFlags.Tax },
            { "zakat", GLAccountFlags.Tax },
            { "income tax", GLAccountFlags.Tax },
            
            // Gains/Losses
            { "gain on sale", GLAccountFlags.GainLossOnDisposal },
            { "loss on sale", GLAccountFlags.GainLossOnDisposal },
            { "gain on disposal", GLAccountFlags.GainLossOnDisposal },
            { "loss on disposal", GLAccountFlags.GainLossOnDisposal },
            
            // Foreign Exchange
            { "foreign exchange", GLAccountFlags.ForeignExchange },
            { "exchange gain", GLAccountFlags.ForeignExchange },
            { "exchange loss", GLAccountFlags.ForeignExchange },
            { "currency translation", GLAccountFlags.ForeignExchange }
        };

        private static readonly Dictionary<string, GLAccountFlags> ArabicKeywords = new()
        {
            // إهلاك وإطفاء
            { "إهلاك", GLAccountFlags.Depreciation },
            { "اهلاك", GLAccountFlags.Depreciation },
            { "مجمع الإهلاك", GLAccountFlags.Depreciation },
            { "مجمع الاهلاك", GLAccountFlags.Depreciation },
            { "إطفاء", GLAccountFlags.Amortization },
            { "اطفاء", GLAccountFlags.Amortization },
            
            // مخصصات
            { "مخصص", GLAccountFlags.Provision },
            { "مخصصات", GLAccountFlags.Provision },
            { "ديون مشكوك", GLAccountFlags.Provision },
            { "انخفاض القيمة", GLAccountFlags.Impairment },
            
            // أصول ثابتة
            { "ممتلكات", GLAccountFlags.FixedAsset },
            { "آلات", GLAccountFlags.FixedAsset },
            { "الات", GLAccountFlags.FixedAsset },
            { "معدات", GLAccountFlags.FixedAsset },
            { "سيارات", GLAccountFlags.FixedAsset },
            { "أثاث", GLAccountFlags.FixedAsset },
            { "اثاث", GLAccountFlags.FixedAsset },
            { "مباني", GLAccountFlags.FixedAsset },
            { "أراضي", GLAccountFlags.FixedAsset },
            { "اراضي", GLAccountFlags.FixedAsset },
            
            // أصول غير ملموسة
            { "غير ملموسة", GLAccountFlags.IntangibleAsset },
            { "شهرة", GLAccountFlags.IntangibleAsset },
            { "براءة اختراع", GLAccountFlags.IntangibleAsset },
            { "علامة تجارية", GLAccountFlags.IntangibleAsset },
            
            // استثمارات
            { "استثمار", GLAccountFlags.Investment },
            { "أوراق مالية", GLAccountFlags.Investment },
            { "اوراق مالية", GLAccountFlags.Investment },
            
            // قروض
            { "قرض", GLAccountFlags.Loan },
            { "قروض", GLAccountFlags.Loan },
            
            // فوائد وأرباح
            { "فوائد", GLAccountFlags.Interest },
            { "تكاليف تمويل", GLAccountFlags.Interest },
            { "أرباح موزعة", GLAccountFlags.Dividend },
            { "ارباح موزعة", GLAccountFlags.Dividend },
            
            // ضرائب
            { "ضريبة", GLAccountFlags.Tax },
            { "زكاة", GLAccountFlags.Tax },
            
            // أرباح/خسائر البيع
            { "ربح بيع", GLAccountFlags.GainLossOnDisposal },
            { "خسارة بيع", GLAccountFlags.GainLossOnDisposal },
            
            // صرف أجنبي
            { "صرف أجنبي", GLAccountFlags.ForeignExchange },
            { "فروق العملة", GLAccountFlags.ForeignExchange }
        };

        #endregion

        #region Account Number Patterns

        // Pre-compiled regex patterns for better performance
        private static readonly List<(Regex Pattern, GLAccountFlags Flag)> CompiledPatterns = new()
        {
            // Non-Current Assets (1.11.xxx)
            (new Regex(@"^1\.11\.111", RegexOptions.Compiled), GLAccountFlags.FixedAsset),      // Property, Plant & Equipment
            (new Regex(@"^1\.11\.112", RegexOptions.Compiled), GLAccountFlags.Depreciation),    // Accumulated Depreciation
            (new Regex(@"^1\.11\.113", RegexOptions.Compiled), GLAccountFlags.IntangibleAsset), // Intangible Assets
            (new Regex(@"^1\.11\.114", RegexOptions.Compiled), GLAccountFlags.Investment),      // Long-term Investments
            
            // Current Assets (1.12.xxx)
            (new Regex(@"^1\.12\.12[1-6]", RegexOptions.Compiled), GLAccountFlags.WorkingCapital), // All current assets
            
            // Current Liabilities (2.21.xxx)
            (new Regex(@"^2\.21\.21[1-7]", RegexOptions.Compiled), GLAccountFlags.WorkingCapital), // All current liabilities
            
            // Non-Current Liabilities (2.22.xxx)
            (new Regex(@"^2\.22\.221", RegexOptions.Compiled), GLAccountFlags.Loan),            // Long-term Loans
            (new Regex(@"^2\.22\.222", RegexOptions.Compiled), GLAccountFlags.Provision),       // Provisions
        };

        #endregion

        #region Main Derivation Methods

        /// <summary>
        /// Derive all flags for a GL Account automatically
        /// All parameters can be null - will use safe defaults
        /// </summary>
        public GLAccountDerivedFlags DeriveFlags(
            string fullAccountNumber,
            string accountNameEn,
            string accountNameAr,
            AccountType accountType)
        {
            // ALWAYS return a valid result, never null
            var result = new GLAccountDerivedFlags();

            try
            {
                // Normalize inputs (handle nulls safely)
                var safeFullAccountNumber = fullAccountNumber?.Trim() ?? "";
                var safeAccountNameEn = accountNameEn?.Trim() ?? "";
                var safeAccountNameAr = accountNameAr?.Trim() ?? "";

                // 1. Derive GLAccountFlags
                result.AccountFlags = DeriveAccountFlags(safeFullAccountNumber, safeAccountNameEn, safeAccountNameAr, accountType);

                // 2. Derive CashFlowCategory
                result.CashFlowCategory = DeriveCashFlowCategory(accountType, result.AccountFlags);

                // 3. Derive WorkingCapitalType
                result.WorkingCapitalType = DeriveWorkingCapitalType(safeFullAccountNumber, safeAccountNameEn, accountType, result.AccountFlags);

                // 4. Derive AccountNature (Cash/Bank)
                result.AccountNature = DeriveAccountNature(safeFullAccountNumber, safeAccountNameEn);

                // 5. Derive DimensionRequirements
                result.DimensionRequirements = DeriveDimensionRequirements(accountType);
            }
            catch (Exception ex)
            {
                // Log error but return default flags rather than failing
                System.Diagnostics.Debug.WriteLine($"[GLAccountFlagsDerivationService] Error deriving flags for account '{fullAccountNumber}': {ex.Message}");
                // Return default values (already set in result)
            }

            return result;
        }

        /// <summary>
        /// Derive GLAccountFlags from account properties
        /// All string parameters are expected to be non-null (pre-normalized)
        /// </summary>
        public GLAccountFlags DeriveAccountFlags(
            string fullAccountNumber,
            string accountNameEn,
            string accountNameAr,
            AccountType accountType)
        {
            var flags = GLAccountFlags.None;

            // Safety check - should never happen if called from DeriveFlags
            if (fullAccountNumber == null) fullAccountNumber = "";
            if (accountNameEn == null) accountNameEn = "";
            if (accountNameAr == null) accountNameAr = "";

            try
            {
                // 1. Check account number patterns first (most reliable)
                if (!string.IsNullOrEmpty(fullAccountNumber))
                {
                    foreach (var (pattern, flag) in CompiledPatterns)
                    {
                        if (pattern.IsMatch(fullAccountNumber))
                        {
                            flags |= flag;
                            break; // Use first match only for number patterns
                        }
                    }
                }

                // 2. Check English keywords in account name
                if (!string.IsNullOrEmpty(accountNameEn))
                {
                    var nameLower = accountNameEn.ToLowerInvariant();
                    foreach (var keyword in EnglishKeywords)
                    {
                        if (nameLower.Contains(keyword.Key))
                        {
                            flags |= keyword.Value;
                        }
                    }
                }

                // 3. Check Arabic keywords in account name
                if (!string.IsNullOrEmpty(accountNameAr))
                {
                    foreach (var keyword in ArabicKeywords)
                    {
                        if (accountNameAr.Contains(keyword.Key))
                        {
                            flags |= keyword.Value;
                        }
                    }
                }

                // 4. Apply AccountType-based rules
                flags = ApplyAccountTypeRules(flags, accountType, fullAccountNumber);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GLAccountFlagsDerivationService] Error in DeriveAccountFlags: {ex.Message}");
                // Return whatever flags we've accumulated so far
            }

            return flags;
        }

        /// <summary>
        /// Derive CashFlowCategory based on AccountType and Flags
        /// </summary>
        public CashFlowCategory DeriveCashFlowCategory(AccountType accountType, GLAccountFlags flags)
        {
            try
            {
                // Rule 1: Revenue and Expenses → Operating
                if (accountType == AccountType.Revenue || accountType == AccountType.Expense)
                    return CashFlowCategory.Operating;

                // Rule 2: Equity → Financing
                if (accountType == AccountType.Equity)
                    return CashFlowCategory.Financing;

                // Rule 3: Investment flags → Investing
                if (flags.HasFlag(GLAccountFlags.Investment) ||
                    flags.HasFlag(GLAccountFlags.FixedAsset) ||
                    flags.HasFlag(GLAccountFlags.IntangibleAsset))
                    return CashFlowCategory.Investing;

                // Rule 4: Loan flag → Financing
                if (flags.HasFlag(GLAccountFlags.Loan))
                    return CashFlowCategory.Financing;

                // Rule 5: Liabilities (non-current) → Financing
                if (accountType == AccountType.Liability && !flags.HasFlag(GLAccountFlags.WorkingCapital))
                    return CashFlowCategory.Financing;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GLAccountFlagsDerivationService] Error in DeriveCashFlowCategory: {ex.Message}");
            }

            return CashFlowCategory.None;
        }

        /// <summary>
        /// Derive WorkingCapitalType for working capital accounts
        /// </summary>
        public WorkingCapitalType? DeriveWorkingCapitalType(
            string fullAccountNumber,
            string accountNameEn,
            AccountType accountType,
            GLAccountFlags flags)
        {
            try
            {
                // Only for WorkingCapital flagged accounts
                if (!flags.HasFlag(GLAccountFlags.WorkingCapital))
                    return null;

                var nameLower = (accountNameEn ?? "").ToLowerInvariant();
                var accNum = fullAccountNumber ?? "";

                // Assets
                if (accountType == AccountType.Asset)
                {
                    if (nameLower.Contains("receivable") || accNum.StartsWith("1.12.122"))
                        return WorkingCapitalType.Receivables;

                    if (nameLower.Contains("inventory") || accNum.StartsWith("1.12.123"))
                        return WorkingCapitalType.Inventory;

                    if (nameLower.Contains("prepaid") || accNum.StartsWith("1.12.124"))
                        return WorkingCapitalType.Prepaid;

                    return WorkingCapitalType.OtherCurrentAsset;
                }

                // Liabilities
                if (accountType == AccountType.Liability)
                {
                    if (nameLower.Contains("payable") || accNum.StartsWith("2.21.211"))
                        return WorkingCapitalType.Payables;

                    if (nameLower.Contains("accrued") || accNum.StartsWith("2.21.212"))
                        return WorkingCapitalType.Accrued;

                    return WorkingCapitalType.OtherCurrentLiability;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GLAccountFlagsDerivationService] Error in DeriveWorkingCapitalType: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Derive AccountNature (Cash/Bank)
        /// </summary>
        public AccountNature DeriveAccountNature(string fullAccountNumber, string accountNameEn)
        {
            try
            {
                var nameLower = (accountNameEn ?? "").ToLowerInvariant();
                var accNum = fullAccountNumber ?? "";

                // Cash accounts
                if (nameLower.Contains("cash on hand") || nameLower.Contains("petty cash") ||
                    accNum.StartsWith("1.12.121.1211"))
                    return AccountNature.Cash;

                // Bank accounts - but NOT "bank charges"
                if ((nameLower.Contains("bank") && !nameLower.Contains("charge")) ||
                    accNum.StartsWith("1.12.121.1212"))
                    return AccountNature.Bank;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GLAccountFlagsDerivationService] Error in DeriveAccountNature: {ex.Message}");
            }

            return AccountNature.None;
        }

        /// <summary>
        /// Derive DimensionRequirements based on AccountType
        /// </summary>
        public DimensionRequirements DeriveDimensionRequirements(AccountType accountType)
        {
            try
            {
                // Expenses require Cost Center by default
                if (accountType == AccountType.Expense)
                    return DimensionRequirements.CostCenter;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GLAccountFlagsDerivationService] Error in DeriveDimensionRequirements: {ex.Message}");
            }

            return DimensionRequirements.None;
        }

        #endregion

        #region Helper Methods

        private GLAccountFlags ApplyAccountTypeRules(GLAccountFlags flags, AccountType accountType, string fullAccountNumber)
        {
            try
            {
                var accNum = fullAccountNumber ?? "";

                // Current Assets (1.12.xxx) → WorkingCapital if not already flagged
                if (accountType == AccountType.Asset &&
                    accNum.StartsWith("1.12") &&
                    !flags.HasFlag(GLAccountFlags.FixedAsset) &&
                    !flags.HasFlag(GLAccountFlags.IntangibleAsset) &&
                    !flags.HasFlag(GLAccountFlags.Investment))
                {
                    flags |= GLAccountFlags.WorkingCapital;
                }

                // Current Liabilities (2.21.xxx) → WorkingCapital
                if (accountType == AccountType.Liability && accNum.StartsWith("2.21"))
                {
                    flags |= GLAccountFlags.WorkingCapital;
                }

                // Remove incompatible combinations
                if (flags.HasFlag(GLAccountFlags.WorkingCapital))
                {
                    // WorkingCapital cannot have Depreciation, FixedAsset, etc.
                    flags &= ~GLAccountFlags.Depreciation;
                    flags &= ~GLAccountFlags.FixedAsset;
                    flags &= ~GLAccountFlags.IntangibleAsset;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GLAccountFlagsDerivationService] Error in ApplyAccountTypeRules: {ex.Message}");
            }

            return flags;
        }

        #endregion
    }

    /// <summary>
    /// Result class for derived flags
    /// All properties have safe default values
    /// </summary>
    public class GLAccountDerivedFlags
    {
        public GLAccountFlags AccountFlags { get; set; } = GLAccountFlags.None;
        public CashFlowCategory CashFlowCategory { get; set; } = CashFlowCategory.None;
        public WorkingCapitalType? WorkingCapitalType { get; set; } = null;
        public AccountNature AccountNature { get; set; } = AccountNature.None;
        public DimensionRequirements DimensionRequirements { get; set; } = DimensionRequirements.None;
    }
}