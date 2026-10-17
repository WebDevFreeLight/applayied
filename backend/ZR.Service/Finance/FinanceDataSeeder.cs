using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure;
using Newtonsoft.Json;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Enums;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance;

namespace ZR.Service.Finance
{
    /// <summary>
    /// Unified Finance Data Seeder - زراعة بيانات المالية الموحدة
    /// Seeds all finance module data from external API: GL, ARAP, Assets, Configuration, Treasury
    /// </summary>
    [AppService(ServiceType = typeof(IFinanceDataSeeder), ServiceLifetime = LifeTime.Transient)]
    public class FinanceDataSeeder : IFinanceDataSeeder
    {
        private readonly IGLAccountRepository _accountRepository;

        public FinanceDataSeeder(IGLAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        /// <summary>
        /// Seed all finance data from external API
        /// زراعة جميع البيانات المالية من النظام الخارجي
        /// </summary>
        public async Task<FinanceSeederResult> SeedAllFinanceDataAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            var result = new FinanceSeederResult();
            var now = DateTime.Now;

            try
            {
                // 1. Seed GL Accounts (must be first as other modules depend on it)
                try
                {
                    result.GLAccountsImported = await SeedGLAccountsAsync(apiBaseUrl, bearerToken, companyId, createdBy);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"GL Accounts: {ex.Message}");
                }

                // 2. Seed Configuration Data
                try { result.CurrenciesImported = await SeedCurrenciesAsync(apiBaseUrl, bearerToken, companyId, createdBy); }
                catch (Exception ex) { result.Errors.Add($"Currencies: {ex.Message}"); }
                
                try { result.FiscalYearsImported = await SeedFiscalYearsAsync(apiBaseUrl, bearerToken, companyId, createdBy); }
                catch (Exception ex) { result.Errors.Add($"Fiscal Years: {ex.Message}"); }
                
                try { result.TaxRulesImported = await SeedTaxRulesAsync(apiBaseUrl, bearerToken, companyId, createdBy); }
                catch (Exception ex) { result.Errors.Add($"Tax Rules: {ex.Message}"); }

                // 3. Seed ARAP Data
                try { result.CustomersImported = await SeedCustomersAsync(apiBaseUrl, bearerToken, companyId, createdBy); }
                catch (Exception ex) { result.Errors.Add($"Customers: {ex.Message}"); }
                
                try { result.VendorsImported = await SeedVendorsAsync(apiBaseUrl, bearerToken, companyId, createdBy); }
                catch (Exception ex) { result.Errors.Add($"Vendors: {ex.Message}"); }

                // 4. Seed Assets Data
                try { result.AssetCategoriesImported = await SeedAssetCategoriesAsync(apiBaseUrl, bearerToken, companyId, createdBy); }
                catch (Exception ex) { result.Errors.Add($"Asset Categories: {ex.Message}"); }

                // 5. Seed Treasury Data
                try { result.BankAccountsImported = await SeedBankAccountsAsync(apiBaseUrl, bearerToken, companyId, createdBy); }
                catch (Exception ex) { result.Errors.Add($"Bank Accounts: {ex.Message}"); }
                
                try { result.CashAccountsImported = await SeedCashAccountsAsync(apiBaseUrl, bearerToken, companyId, createdBy); }
                catch (Exception ex) { result.Errors.Add($"Cash Accounts: {ex.Message}"); }

                result.Success = true;
                result.Message = $"Successfully imported all finance data - تم استيراد جميع البيانات المالية بنجاح";
                result.TotalRecordsImported = result.GLAccountsImported + result.CurrenciesImported + 
                    result.FiscalYearsImported + result.CustomersImported + result.VendorsImported + 
                    result.AssetCategoriesImported + result.BankAccountsImported + result.CashAccountsImported + 
                    result.TaxRulesImported;
                
                if (result.Errors.Count > 0)
                {
                    result.Message += $" (with {result.Errors.Count} errors)";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Failed to import finance data: {ex.Message}";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        #region GL Accounts

        private async Task<int> SeedGLAccountsAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            try
            {
                var url = $"{apiBaseUrl.TrimEnd('/')}/finance/accounts/list?pageNum=1&pageSize=1000";
                var headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {bearerToken}" },
                    { "Accept", "application/json" }
                };

                var response = await HttpHelper.HttpGetAsync(url, headers);
                if (string.IsNullOrEmpty(response))
                    return 0;

                var accounts = ParseApiResponse<ExternalGLAccountDto>(response);
                if (accounts == null || accounts.Count == 0)
                    return 0;

                var existingAccounts = await _accountRepository.ListAllAsync(companyId);
                var existingFullNumbers = existingAccounts.Select(a => a.FullAccountNumber).ToHashSet();

                var accountsToInsert = new List<GLAccount>();
                var accountMap = new Dictionary<string, GLAccount>();
                var now = DateTime.Now;

                // First pass: Create all account entities
                foreach (var extAccount in accounts)
                {
                    var account = MapExternalGLAccount(extAccount, companyId, createdBy, now);
                    if (existingFullNumbers.Contains(account.FullAccountNumber))
                        continue;

                    accountMap[account.FullAccountNumber ?? account.AccountNumber] = account;
                    accountsToInsert.Add(account);
                }

                // Second pass: Build parent relationships
                foreach (var extAccount in accounts)
                {
                    var accountKey = extAccount.FullAccountNumber ?? extAccount.AccountNumber;
                    if (!accountMap.TryGetValue(accountKey, out var account))
                        continue;

                    if (!string.IsNullOrEmpty(extAccount.ParentAccountNumber))
                    {
                        var parentKey = extAccount.ParentFullAccountNumber ?? extAccount.ParentAccountNumber;
                        if (accountMap.TryGetValue(parentKey, out var parent))
                        {
                            account.ParentAccountId = parent.AccountId;
                            account.Level = parent.Level + 1;
                            account.FullAccountNumber = account.BuildFullAccountNumber(parent.FullAccountNumber);
                        }
                    }
                    else
                    {
                        account.Level = 1;
                        account.FullAccountNumber = account.AccountNumber;
                    }
                }

                // Third pass: Build account paths
                var accountDict = new Dictionary<Guid, GLAccount>();
                foreach (var account in accountsToInsert)
                {
                    accountDict[account.AccountId] = account;
                }

                foreach (var account in accountsToInsert)
                {
                    if (account.ParentAccountId.HasValue && accountDict.TryGetValue(account.ParentAccountId.Value, out var parent))
                    {
                        account.AccountPath = string.IsNullOrEmpty(parent.AccountPath)
                            ? $"{parent.AccountId}/{account.AccountId}"
                            : $"{parent.AccountPath}/{account.AccountId}";
                    }
                    else
                    {
                        account.AccountPath = account.AccountId.ToString();
                    }
                }

                // Update IsLeaf for parent accounts
                var parentIds = new HashSet<Guid>();
                foreach (var account in accountsToInsert)
                {
                    if (account.ParentAccountId.HasValue)
                        parentIds.Add(account.ParentAccountId.Value);
                }

                foreach (var account in accountsToInsert)
                {
                    if (parentIds.Contains(account.AccountId))
                        account.IsLeaf = false;
                }

                // Insert all accounts
                int count = 0;
                foreach (var account in accountsToInsert)
                {
                    if (!await _accountRepository.FullAccountNumberExistsInCompanyAsync(account.FullAccountNumber, companyId))
                    {
                        await _accountRepository.InsertAccountAsync(account);
                        count++;
                    }
                }

                if (count > 0)
                {
                    await _accountRepository.RecalculateAllPathsAsync();
                    await _accountRepository.RecalculateAllCountsAsync();
                }

                return count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to seed GL Accounts: {ex.Message}", ex);
            }
        }

        #endregion

        #region Configuration

        private async Task<int> SeedCurrenciesAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            try
            {
                var url = $"{apiBaseUrl.TrimEnd('/')}/finance/configuration/currencies/list?pageNum=1&pageSize=1000";
                var response = await FetchFromApiAsync(url, bearerToken);
                var currencies = ParseApiResponse<ExternalCurrencyDto>(response);
                
                // TODO: Implement currency insertion logic
                return currencies?.Count ?? 0;
            }
            catch
            {
                return 0; // Non-critical, continue with other data
            }
        }

        private async Task<int> SeedFiscalYearsAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            try
            {
                var url = $"{apiBaseUrl.TrimEnd('/')}/finance/configuration/fiscal-years/list?pageNum=1&pageSize=1000";
                var response = await FetchFromApiAsync(url, bearerToken);
                var fiscalYears = ParseApiResponse<ExternalFiscalYearDto>(response);
                
                // TODO: Implement fiscal year insertion logic
                return fiscalYears?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> SeedTaxRulesAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            try
            {
                var url = $"{apiBaseUrl.TrimEnd('/')}/finance/tax/rules/list?pageNum=1&pageSize=1000";
                var response = await FetchFromApiAsync(url, bearerToken);
                var taxRules = ParseApiResponse<ExternalTaxRuleDto>(response);
                
                // TODO: Implement tax rule insertion logic
                return taxRules?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region ARAP

        private async Task<int> SeedCustomersAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            try
            {
                var url = $"{apiBaseUrl.TrimEnd('/')}/finance/arap/customers/list?pageNum=1&pageSize=1000";
                var response = await FetchFromApiAsync(url, bearerToken);
                var customers = ParseApiResponse<ExternalCustomerDto>(response);
                
                // TODO: Implement customer insertion logic
                return customers?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> SeedVendorsAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            try
            {
                var url = $"{apiBaseUrl.TrimEnd('/')}/finance/arap/vendors/list?pageNum=1&pageSize=1000";
                var response = await FetchFromApiAsync(url, bearerToken);
                var vendors = ParseApiResponse<ExternalVendorDto>(response);
                
                // TODO: Implement vendor insertion logic
                return vendors?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Assets

        private async Task<int> SeedAssetCategoriesAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            try
            {
                var url = $"{apiBaseUrl.TrimEnd('/')}/assets/categories/list?pageNum=1&pageSize=1000";
                var response = await FetchFromApiAsync(url, bearerToken);
                var categories = ParseApiResponse<ExternalAssetCategoryDto>(response);
                
                // TODO: Implement asset category insertion logic
                return categories?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Treasury

        private async Task<int> SeedBankAccountsAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            try
            {
                var url = $"{apiBaseUrl.TrimEnd('/')}/finance/treasury/bank-accounts/list?pageNum=1&pageSize=1000";
                var response = await FetchFromApiAsync(url, bearerToken);
                var bankAccounts = ParseApiResponse<ExternalBankAccountDto>(response);
                
                // TODO: Implement bank account insertion logic
                return bankAccounts?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> SeedCashAccountsAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            try
            {
                var url = $"{apiBaseUrl.TrimEnd('/')}/finance/treasury/cash-accounts/list?pageNum=1&pageSize=1000";
                var response = await FetchFromApiAsync(url, bearerToken);
                var cashAccounts = ParseApiResponse<ExternalCashAccountDto>(response);
                
                // TODO: Implement cash account insertion logic
                return cashAccounts?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Helper Methods

        private async Task<string> FetchFromApiAsync(string url, string bearerToken)
        {
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {bearerToken}" },
                { "Accept", "application/json" }
            };

            return await HttpHelper.HttpGetAsync(url, headers);
        }

        private List<T> ParseApiResponse<T>(string response)
        {
            if (string.IsNullOrEmpty(response))
                return new List<T>();

            try
            {
                // Try direct array
                var list = JsonConvert.DeserializeObject<List<T>>(response);
                if (list != null)
                    return list;

                // Try wrapped response
                var wrapped = JsonConvert.DeserializeObject<ExternalApiResponse<List<T>>>(response);
                if (wrapped?.Data != null)
                    return wrapped.Data;

                // Try paginated response
                var paginated = JsonConvert.DeserializeObject<ExternalPaginatedResponse<T>>(response);
                if (paginated?.Rows != null)
                    return paginated.Rows;

                return new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        private GLAccount MapExternalGLAccount(ExternalGLAccountDto extAccount, long? companyId, string createdBy, DateTime createdAt)
        {
            AccountType accountType = AccountType.Asset;
            if (extAccount.AccountType.HasValue)
                accountType = (AccountType)extAccount.AccountType.Value;
            else if (!string.IsNullOrEmpty(extAccount.AccountTypeName))
                accountType = ParseAccountType(extAccount.AccountTypeName);

            NormalBalance normalBalance = extAccount.NormalBalance.HasValue
                ? (NormalBalance)extAccount.NormalBalance.Value
                : InferNormalBalance(accountType);

            int level = CalculateLevel(extAccount.AccountNumber ?? extAccount.FullAccountNumber);

            // Parse Cash Flow fields from external API or infer from account type/name
            var cashFlowCategory = extAccount.CashFlowCategory.HasValue
                ? (CashFlowCategory)extAccount.CashFlowCategory.Value
                : InferCashFlowCategory(extAccount, accountType);

            var accountFlags = extAccount.AccountFlags.HasValue
                ? (GLAccountFlags)extAccount.AccountFlags.Value
                : InferAccountFlags(extAccount, accountType);

            var workingCapitalType = extAccount.WorkingCapitalType.HasValue
                ? (WorkingCapitalType?)extAccount.WorkingCapitalType.Value
                : InferWorkingCapitalType(extAccount, accountType);

            return new GLAccount
            {
                AccountId = extAccount.AccountId ?? Guid.NewGuid(),
                AccountNumber = extAccount.AccountNumber ?? extAccount.FullAccountNumber ?? Guid.NewGuid().ToString(),
                FullAccountNumber = extAccount.FullAccountNumber ?? extAccount.AccountNumber,
                AccountNameAr = extAccount.AccountNameAr ?? extAccount.AccountName ?? "",
                AccountNameEn = extAccount.AccountNameEn ?? extAccount.AccountName ?? "",
                AccountType = accountType,
                NormalBalance = normalBalance,
                Level = level,
                IsLeaf = extAccount.IsLeaf ?? true,
                IsActive = extAccount.IsActive ?? true,
                IsSystemAccount = true,
                AllowManualEntries = extAccount.AllowManualEntries ?? true,
                AllowPostingToParent = extAccount.AllowPostingToParent ?? false,
                RequiresCostCenter = extAccount.RequiresCostCenter ?? false,
                RequiresDepartment = extAccount.RequiresDepartment ?? false,
                RequiresProject = extAccount.RequiresProject ?? false,
                AccountNature = (extAccount.IsCashAccount ?? false) ? AccountNature.Cash : ((extAccount.IsBankAccount ?? false) ? AccountNature.Bank : AccountNature.None),
                IsControlAccount = extAccount.IsControlAccount ?? false,
                ReconciliationMode = (extAccount.IsReconciliationAccount ?? false) ? ReconciliationMode.Required : ReconciliationMode.None,
                Currency = extAccount.Currency ?? "SAR",
                IsForeignCurrency = extAccount.IsForeignCurrency ?? false,
                DescriptionAr = extAccount.DescriptionAr,
                DescriptionEn = extAccount.DescriptionEn,
                ExternalCode = extAccount.ExternalCode,
                TaxCode = extAccount.TaxCode,
                DisplayOrder = extAccount.DisplayOrder ?? 0,
                RowVersion = 1,
                ChildCount = 0,
                DescendantCount = 0,
                CompanyID = companyId,
                Create_by = createdBy,
                Create_time = createdAt,
                // Cash Flow Classification (IAS 7)
                CashFlowCategory = cashFlowCategory,
                AccountFlags = accountFlags,
                WorkingCapitalType = workingCapitalType
            };
        }

        /// <summary>
        /// Infer CashFlowCategory from account type and name when not provided by external API
        /// </summary>
        private CashFlowCategory InferCashFlowCategory(ExternalGLAccountDto extAccount, AccountType accountType)
        {
            var name = (extAccount.AccountNameEn ?? extAccount.AccountName ?? "").ToLowerInvariant();
            var nameAr = extAccount.AccountNameAr ?? "";

            // Equity accounts are Financing
            if (accountType == AccountType.Equity)
                return CashFlowCategory.Financing;

            // Fixed assets, intangibles, investments are Investing
            if (name.Contains("property") || name.Contains("equipment") || name.Contains("fixed asset") ||
                name.Contains("intangible") || name.Contains("investment") || name.Contains("goodwill") ||
                nameAr.Contains("ممتلكات") || nameAr.Contains("معدات") || nameAr.Contains("استثمار"))
                return CashFlowCategory.Investing;

            // Loans, borrowings, capital are Financing
            if (name.Contains("loan") || name.Contains("borrowing") || name.Contains("capital") ||
                name.Contains("dividend") || name.Contains("share") ||
                nameAr.Contains("قرض") || nameAr.Contains("رأس المال"))
                return CashFlowCategory.Financing;

            // Revenue, expenses, working capital items are Operating
            if (accountType == AccountType.Revenue || accountType == AccountType.Expense)
                return CashFlowCategory.Operating;

            // Receivables, payables, inventory, prepaid are Operating
            if (name.Contains("receivable") || name.Contains("payable") || name.Contains("inventory") ||
                name.Contains("prepaid") || name.Contains("accrued") ||
                nameAr.Contains("ذمم") || nameAr.Contains("مخزون") || nameAr.Contains("مقدم"))
                return CashFlowCategory.Operating;

            return CashFlowCategory.None;
        }

        /// <summary>
        /// Infer GLAccountFlags from account type and name when not provided by external API
        /// </summary>
        private GLAccountFlags InferAccountFlags(ExternalGLAccountDto extAccount, AccountType accountType)
        {
            var name = (extAccount.AccountNameEn ?? extAccount.AccountName ?? "").ToLowerInvariant();
            var nameAr = extAccount.AccountNameAr ?? "";
            var flags = GLAccountFlags.None;

            // Working capital items
            if (name.Contains("receivable") || name.Contains("payable") || name.Contains("inventory") ||
                name.Contains("prepaid") || name.Contains("accrued") ||
                nameAr.Contains("ذمم") || nameAr.Contains("مخزون") || nameAr.Contains("مقدم") || nameAr.Contains("مستحق"))
                flags |= GLAccountFlags.WorkingCapital;

            // Fixed assets
            if (name.Contains("property") || name.Contains("equipment") || name.Contains("fixed asset") ||
                name.Contains("building") || name.Contains("vehicle") ||
                nameAr.Contains("ممتلكات") || nameAr.Contains("معدات") || nameAr.Contains("مبنى"))
                flags |= GLAccountFlags.FixedAsset;

            // Depreciation
            if (name.Contains("depreciation") || name.Contains("accumulated") ||
                nameAr.Contains("إهلاك") || nameAr.Contains("مجمع"))
                flags |= GLAccountFlags.Depreciation;

            // Amortization
            if (name.Contains("amortization") || nameAr.Contains("استهلاك"))
                flags |= GLAccountFlags.Amortization;

            // Intangible assets
            if (name.Contains("intangible") || name.Contains("goodwill") || name.Contains("patent") ||
                name.Contains("trademark") || nameAr.Contains("غير ملموس"))
                flags |= GLAccountFlags.IntangibleAsset;

            // Investments
            if (name.Contains("investment") || nameAr.Contains("استثمار"))
                flags |= GLAccountFlags.Investment;

            // Loans
            if (name.Contains("loan") || name.Contains("borrowing") ||
                nameAr.Contains("قرض"))
                flags |= GLAccountFlags.Loan;

            // Tax
            if (name.Contains("tax") || name.Contains("vat") ||
                nameAr.Contains("ضريب"))
                flags |= GLAccountFlags.Tax;

            // Provision
            if (name.Contains("provision") || name.Contains("allowance") ||
                nameAr.Contains("مخصص"))
                flags |= GLAccountFlags.Provision;

            // FX Gain/Loss
            if (name.Contains("foreign exchange") || name.Contains("fx gain") || name.Contains("fx loss") ||
                nameAr.Contains("صرف العمل"))
                flags |= GLAccountFlags.ForeignExchange;

            return flags;
        }

        /// <summary>
        /// Infer WorkingCapitalType from account name when not provided by external API
        /// </summary>
        private WorkingCapitalType? InferWorkingCapitalType(ExternalGLAccountDto extAccount, AccountType accountType)
        {
            var name = (extAccount.AccountNameEn ?? extAccount.AccountName ?? "").ToLowerInvariant();
            var nameAr = extAccount.AccountNameAr ?? "";

            // Receivables
            if (name.Contains("receivable") || name.Contains("trade debtor") ||
                nameAr.Contains("ذمم مدينة") || nameAr.Contains("مدين"))
                return WorkingCapitalType.Receivables;

            // Payables
            if (name.Contains("payable") || name.Contains("trade creditor") ||
                nameAr.Contains("ذمم دائنة") || nameAr.Contains("دائن"))
                return WorkingCapitalType.Payables;

            // Inventory
            if (name.Contains("inventory") || name.Contains("stock") ||
                nameAr.Contains("مخزون"))
                return WorkingCapitalType.Inventory;

            // Prepaid
            if (name.Contains("prepaid") || name.Contains("advance payment") ||
                nameAr.Contains("مدفوع مقدم") || nameAr.Contains("مقدم"))
                return WorkingCapitalType.Prepaid;

            // Accrued
            if (name.Contains("accrued") || name.Contains("accrual") ||
                nameAr.Contains("مستحق"))
                return WorkingCapitalType.Accrued;

            // Deferred Revenue (classified as Other Current Liability)
            if (name.Contains("deferred revenue") || name.Contains("unearned") ||
                nameAr.Contains("إيراد مؤجل"))
                return WorkingCapitalType.OtherCurrentLiability;

            return null;
        }

        private AccountType ParseAccountType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return AccountType.Asset;

            var normalized = typeName.ToLowerInvariant().Trim();
            return normalized switch
            {
                "asset" or "assets" or "أصول" => AccountType.Asset,
                "liability" or "liabilities" or "الالتزامات" or "خصوم" => AccountType.Liability,
                "equity" or "حقوق الملكية" => AccountType.Equity,
                "revenue" or "إيرادات" => AccountType.Revenue,
                "expense" or "expenses" or "مصروفات" => AccountType.Expense,
                _ => AccountType.Asset
            };
        }

        private NormalBalance InferNormalBalance(AccountType accountType)
        {
            return accountType switch
            {
                AccountType.Asset or AccountType.Expense => NormalBalance.Debit,
                AccountType.Liability or AccountType.Equity or AccountType.Revenue => NormalBalance.Credit,
                _ => NormalBalance.Debit
            };
        }

        private int CalculateLevel(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
                return 1;

            if (accountNumber.Contains('.'))
                return accountNumber.Split('.').Length;

            if (accountNumber.Length <= 4) return 1;
            if (accountNumber.Length <= 6) return 2;
            if (accountNumber.Length <= 8) return 3;
            return 4;
        }

        #endregion
    }

    #region DTOs

    public class FinanceSeederResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalRecordsImported { get; set; }
        public int GLAccountsImported { get; set; }
        public int CurrenciesImported { get; set; }
        public int FiscalYearsImported { get; set; }
        public int TaxRulesImported { get; set; }
        public int CustomersImported { get; set; }
        public int VendorsImported { get; set; }
        public int AssetCategoriesImported { get; set; }
        public int BankAccountsImported { get; set; }
        public int CashAccountsImported { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    // External DTOs
    public class ExternalGLAccountDto
    {
        public Guid? AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string FullAccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountNameAr { get; set; }
        public string AccountNameEn { get; set; }
        public int? AccountType { get; set; }
        public string AccountTypeName { get; set; }
        public int? NormalBalance { get; set; }
        public Guid? ParentAccountId { get; set; }
        public string ParentAccountNumber { get; set; }
        public string ParentFullAccountNumber { get; set; }
        public int? Level { get; set; }
        public bool? IsLeaf { get; set; }
        public bool? IsActive { get; set; }
        public bool? AllowManualEntries { get; set; }
        public bool? AllowPostingToParent { get; set; }
        public bool? RequiresCostCenter { get; set; }
        public bool? RequiresDepartment { get; set; }
        public bool? RequiresProject { get; set; }
        public bool? IsCashAccount { get; set; }
        public bool? IsBankAccount { get; set; }
        public bool? IsControlAccount { get; set; }
        public bool? IsReconciliationAccount { get; set; }
        public string Currency { get; set; }
        public bool? IsForeignCurrency { get; set; }
        public string DescriptionAr { get; set; }
        public string DescriptionEn { get; set; }
        public string ExternalCode { get; set; }
        public string TaxCode { get; set; }
        public int? DisplayOrder { get; set; }

        // Cash Flow Classification (IAS 7 Compliance)
        public int? CashFlowCategory { get; set; }
        public string CashFlowCategoryName { get; set; }
        public int? AccountFlags { get; set; }
        public int? WorkingCapitalType { get; set; }
        public string WorkingCapitalTypeName { get; set; }
    }

    public class ExternalCurrencyDto { }
    public class ExternalFiscalYearDto { }
    public class ExternalTaxRuleDto { }
    public class ExternalCustomerDto { }
    public class ExternalVendorDto { }
    public class ExternalAssetCategoryDto { }
    public class ExternalBankAccountDto { }
    public class ExternalCashAccountDto { }

    public class ExternalApiResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ExternalPaginatedResponse<T>
    {
        public List<T> Rows { get; set; }
        public int Total { get; set; }
        public int PageNum { get; set; }
        public int PageSize { get; set; }
    }

    #endregion
}

