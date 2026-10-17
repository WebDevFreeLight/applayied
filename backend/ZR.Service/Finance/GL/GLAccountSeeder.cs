using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure;
using Newtonsoft.Json;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;
using ZR.Repository.Finance.GL;
using ZR.Repository.Finance.Treasury;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// GL Account Seeder - زراعة الحسابات المالية وفق معايير IFRS مع دعم 5 مستويات
    /// </summary>
    [AppService(ServiceType = typeof(IGLAccountSeeder), ServiceLifetime = LifeTime.Transient)]
    public class GLAccountSeeder : IGLAccountSeeder
    {
        private readonly IGLAccountRepository _accountRepository;
        private readonly BankAccountRepository _bankAccountRepository;
        private readonly CashAccountRepository _cashAccountRepository;

        public GLAccountSeeder(
            IGLAccountRepository accountRepository,
            BankAccountRepository bankAccountRepository,
            CashAccountRepository cashAccountRepository)
        {
            _accountRepository = accountRepository;
            _bankAccountRepository = bankAccountRepository;
            _cashAccountRepository = cashAccountRepository;
        }

        public async Task<int> SeedDefaultAccountsAsync(long? companyId, string createdBy)
        {
            if (await DefaultAccountsExistAsync(companyId))
            {
                return 0;
            }

            var accounts = new List<GLAccount>();
            var now = DateTime.Now;

            // ==================== LEVEL 1: MAJOR CATEGORIES ====================
            var assets = CreateAccount("1000000000", "الأصول", "Assets", AccountType.Asset, NormalBalance.Debit, 1, null, companyId, createdBy, now);
            var liabilities = CreateAccount("2000000000", "الالتزامات", "Liabilities", AccountType.Liability, NormalBalance.Credit, 1, null, companyId, createdBy, now);
            var equity = CreateAccount("3000000000", "حقوق الملكية", "Equity", AccountType.Equity, NormalBalance.Credit, 1, null, companyId, createdBy, now);
            var revenue = CreateAccount("4000000000", "الإيرادات", "Revenue", AccountType.Revenue, NormalBalance.Credit, 1, null, companyId, createdBy, now);
            var expenses = CreateAccount("5000000000", "المصروفات", "Expenses", AccountType.Expense, NormalBalance.Debit, 1, null, companyId, createdBy, now);

            accounts.AddRange(new[] { assets, liabilities, equity, revenue, expenses });

            // ==================== LEVEL 2: SUB-CATEGORIES ====================
            // Assets
            var currentAssets = CreateAccount("1100000000", "الأصول المتداولة", "Current Assets", AccountType.Asset, NormalBalance.Debit, 2, assets.AccountId, companyId, createdBy, now);
            var nonCurrentAssets = CreateAccount("1200000000", "الأصول غير المتداولة", "Non-Current Assets", AccountType.Asset, NormalBalance.Debit, 2, assets.AccountId, companyId, createdBy, now);

            // Liabilities
            var currentLiabilities = CreateAccount("2100000000", "الالتزامات المتداولة", "Current Liabilities", AccountType.Liability, NormalBalance.Credit, 2, liabilities.AccountId, companyId, createdBy, now);
            var nonCurrentLiabilities = CreateAccount("2200000000", "الالتزامات غير المتداولة", "Non-Current Liabilities", AccountType.Liability, NormalBalance.Credit, 2, liabilities.AccountId, companyId, createdBy, now);

            // Revenue
            var operatingRevenue = CreateAccount("4100000000", "إيرادات التشغيل", "Operating Revenue", AccountType.Revenue, NormalBalance.Credit, 2, revenue.AccountId, companyId, createdBy, now);
            var otherRevenue = CreateAccount("4200000000", "إيرادات أخرى", "Other Income", AccountType.Revenue, NormalBalance.Credit, 2, revenue.AccountId, companyId, createdBy, now);

            // Expenses
            var cogs = CreateAccount("5100000000", "تكلفة البضاعة المباعة", "Cost of Goods Sold", AccountType.Expense, NormalBalance.Debit, 2, expenses.AccountId, companyId, createdBy, now);
            var operatingExpenses = CreateAccount("5200000000", "مصروفات التشغيل", "Operating Expenses", AccountType.Expense, NormalBalance.Debit, 2, expenses.AccountId, companyId, createdBy, now);
            var adminExpenses = CreateAccount("5300000000", "المصروفات الإدارية", "Administrative Expenses", AccountType.Expense, NormalBalance.Debit, 2, expenses.AccountId, companyId, createdBy, now);
            var financialExpenses = CreateAccount("5400000000", "المصروفات المالية", "Financial Expenses", AccountType.Expense, NormalBalance.Debit, 2, expenses.AccountId, companyId, createdBy, now);

            accounts.AddRange(new[]
            {
                currentAssets, nonCurrentAssets,
                currentLiabilities, nonCurrentLiabilities,
                operatingRevenue, otherRevenue,
                cogs, operatingExpenses, adminExpenses, financialExpenses
            });

            // ==================== LEVEL 3: DETAIL CATEGORIES ====================
            // Cash and Cash Equivalents - Operating category
            var cash = CreateAccount("1110000000", "النقد وما يعادله", "Cash and Cash Equivalents", AccountType.Asset, NormalBalance.Debit, 3, currentAssets.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Operating);

            // Accounts Receivable - Working Capital (Receivables)
            var accountsReceivable = CreateAccount("1120000000", "الذمم المدينة", "Accounts Receivable", AccountType.Asset, NormalBalance.Debit, 3, currentAssets.AccountId, companyId, createdBy, now,
                isControl: true, cashFlowCategory: CashFlowCategory.Operating, accountFlags: GLAccountFlags.WorkingCapital, workingCapitalType: WorkingCapitalType.Receivables);

            // Inventory - Working Capital (Inventory)
            var inventory = CreateAccount("1130000000", "المخزون", "Inventory", AccountType.Asset, NormalBalance.Debit, 3, currentAssets.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Operating, accountFlags: GLAccountFlags.WorkingCapital, workingCapitalType: WorkingCapitalType.Inventory);

            // Prepaid Expenses - Working Capital (Prepaid)
            var prepaidExpenses = CreateAccount("1140000000", "المصروفات المدفوعة مقدماً", "Prepaid Expenses", AccountType.Asset, NormalBalance.Debit, 3, currentAssets.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Operating, accountFlags: GLAccountFlags.WorkingCapital, workingCapitalType: WorkingCapitalType.Prepaid);

            // VAT Input - Working Capital (Other Current Asset)
            var vatInput = CreateAccount("1150000000", "ضريبة القيمة المضافة - المدخلات", "VAT Input", AccountType.Asset, NormalBalance.Debit, 3, currentAssets.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Operating, accountFlags: GLAccountFlags.WorkingCapital | GLAccountFlags.Tax, workingCapitalType: WorkingCapitalType.OtherCurrentAsset);

            // PPE - Fixed Asset (Investing)
            var ppe = CreateAccount("1210000000", "الممتلكات والمعدات", "Property, Plant & Equipment", AccountType.Asset, NormalBalance.Debit, 3, nonCurrentAssets.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Investing, accountFlags: GLAccountFlags.FixedAsset);

            // Accumulated Depreciation - Depreciation (Non-cash adjustment)
            var accumulatedDepreciation = CreateAccount("1211000000", "مجمع الإهلاك", "Accumulated Depreciation", AccountType.Asset, NormalBalance.Credit, 3, ppe.AccountId, companyId, createdBy, now,
                accountFlags: GLAccountFlags.Depreciation); // Contra-asset, non-cash

            // Intangible Assets - IntangibleAsset (Investing)
            var intangible = CreateAccount("1220000000", "الأصول غير الملموسة", "Intangible Assets", AccountType.Asset, NormalBalance.Debit, 3, nonCurrentAssets.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Investing, accountFlags: GLAccountFlags.IntangibleAsset);

            // Investments - Investment (Investing)
            var investments = CreateAccount("1230000000", "الاستثمارات", "Investments", AccountType.Asset, NormalBalance.Debit, 3, nonCurrentAssets.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Investing, accountFlags: GLAccountFlags.Investment);

            // Accounts Payable - Working Capital (Payables)
            var accountsPayable = CreateAccount("2110000000", "الذمم الدائنة", "Accounts Payable", AccountType.Liability, NormalBalance.Credit, 3, currentLiabilities.AccountId, companyId, createdBy, now,
                isControl: true, cashFlowCategory: CashFlowCategory.Operating, accountFlags: GLAccountFlags.WorkingCapital, workingCapitalType: WorkingCapitalType.Payables);

            // Accrued Expenses - Working Capital (Accrued)
            var accruedExpenses = CreateAccount("2120000000", "المصروفات المستحقة", "Accrued Expenses", AccountType.Liability, NormalBalance.Credit, 3, currentLiabilities.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Operating, accountFlags: GLAccountFlags.WorkingCapital, workingCapitalType: WorkingCapitalType.Accrued);

            // Short-term Loans - Loan (Financing)
            var shortTermLoans = CreateAccount("2130000000", "القروض قصيرة الأجل", "Short-term Loans", AccountType.Liability, NormalBalance.Credit, 3, currentLiabilities.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Financing, accountFlags: GLAccountFlags.Loan);

            // VAT Output - Working Capital (Other Current Liability)
            var vatOutput = CreateAccount("2140000000", "ضريبة القيمة المضافة - المخرجات", "VAT Output", AccountType.Liability, NormalBalance.Credit, 3, currentLiabilities.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Operating, accountFlags: GLAccountFlags.WorkingCapital | GLAccountFlags.Tax, workingCapitalType: WorkingCapitalType.OtherCurrentLiability);

            // Long-term Loans - Loan (Financing)
            var longTermLoans = CreateAccount("2210000000", "القروض طويلة الأجل", "Long-term Loans", AccountType.Liability, NormalBalance.Credit, 3, nonCurrentLiabilities.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Financing, accountFlags: GLAccountFlags.Loan);

            // Deferred Tax Liabilities - Tax
            var deferredTaxLiab = CreateAccount("2220000000", "الضرائب المؤجلة", "Deferred Tax Liabilities", AccountType.Liability, NormalBalance.Credit, 3, nonCurrentLiabilities.AccountId, companyId, createdBy, now,
                accountFlags: GLAccountFlags.Tax);

            // Equity accounts - Financing
            var shareCapital = CreateAccount("3100000000", "رأس المال", "Share Capital", AccountType.Equity, NormalBalance.Credit, 2, equity.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Financing);
            var retainedEarnings = CreateAccount("3200000000", "الأرباح المحتجزة", "Retained Earnings", AccountType.Equity, NormalBalance.Credit, 2, equity.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Financing);
            var otherReserves = CreateAccount("3300000000", "الاحتياطيات الأخرى", "Other Reserves", AccountType.Equity, NormalBalance.Credit, 2, equity.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Financing);

            // Depreciation Expense - Depreciation (Non-cash, Operating adjustment)
            var depreciationExpense = CreateAccount("5410000000", "مصروفات الإهلاك", "Depreciation Expense", AccountType.Expense, NormalBalance.Debit, 3, financialExpenses.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Operating, accountFlags: GLAccountFlags.Depreciation);

            // Gain/Loss on Asset Disposal - GainLossOnDisposal (Non-cash, Operating adjustment)
            var gainLossOnAssetDisposal = CreateAccount("5411000000", "أرباح وخسائر التخلص من الأصول", "Gain/Loss on Asset Disposal", AccountType.Expense, NormalBalance.Debit, 3, financialExpenses.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Operating, accountFlags: GLAccountFlags.GainLossOnDisposal);

            // Revaluation Reserve under Equity - Financing
            var revaluationReserve = CreateAccount("3310000000", "احتياطي إعادة التقييم", "Revaluation Reserve", AccountType.Equity, NormalBalance.Credit, 3, otherReserves.AccountId, companyId, createdBy, now,
                cashFlowCategory: CashFlowCategory.Financing);

            accounts.AddRange(new[]
            {
                cash, accountsReceivable, inventory, prepaidExpenses, vatInput,
                ppe, accumulatedDepreciation, intangible, investments,
                accountsPayable, accruedExpenses, shortTermLoans, vatOutput,
                longTermLoans, deferredTaxLiab,
                shareCapital, retainedEarnings, otherReserves, revaluationReserve,
                depreciationExpense, gainLossOnAssetDisposal
            });

            // ==================== LEVEL 4 & 5: LEAF ACCOUNTS (IsLeaf = true) ====================
            // Examples: Bank accounts, specific receivables, detailed expense items
            // ==================== LEVEL 4 & 5: LEAF ACCOUNTS (IsLeaf = true) ====================
            // النقد والبنوك - Level 4
            var cashBoxes = CreateAccount("1111000000", "صناديق النقد", "Cash Boxes", AccountType.Asset, NormalBalance.Debit, 4, cash.AccountId, companyId, createdBy, now);
            var bankAccounts = CreateAccount("1112000000", "الحسابات البنكية", "Bank Accounts", AccountType.Asset, NormalBalance.Debit, 4, cash.AccountId, companyId, createdBy, now);
            var pettyCash = CreateAccount("1113000000", "صناديق النثريات", "Petty Cash", AccountType.Asset, NormalBalance.Debit, 4, cash.AccountId, companyId, createdBy, now);

            accounts.AddRange(new[] { cashBoxes, bankAccounts, pettyCash });

            // --- Level 5: حسابات فعلية نهائية ---
            // صناديق النقد
            accounts.Add(CreateAccount("1111100000", "صندوق النقد الرئيسي", "Main Cash Box", AccountType.Asset, NormalBalance.Debit, 5, cashBoxes.AccountId, companyId, createdBy, now, isCash: true));

            // صناديق النثريات
            accounts.Add(CreateAccount("1113100000", "نثريات المكتب الرئيسي", "Main Office Petty Cash", AccountType.Asset, NormalBalance.Debit, 5, pettyCash.AccountId, companyId, createdBy, now, isCash: true));
            accounts.Add(CreateAccount("1113200000", "نثريات الفرع الشرقي", "East Branch Petty Cash", AccountType.Asset, NormalBalance.Debit, 5, pettyCash.AccountId, companyId, createdBy, now, isCash: true));

            // الحسابات البنكية
            accounts.Add(CreateAccount("1112100000", "بنك الراجحي - الحساب الجاري", "Al Rajhi Bank - Current Account", AccountType.Asset, NormalBalance.Debit, 5, bankAccounts.AccountId, companyId, createdBy, now, isCash: false, isBank: true));
            accounts.Add(CreateAccount("1112200000", "البنك الأهلي - الحساب الجاري", "Al Ahli Bank - Current Account", AccountType.Asset, NormalBalance.Debit, 5, bankAccounts.AccountId, companyId, createdBy, now, isCash: false, isBank: true));
                        // Build hierarchy
            var accountDict = new Dictionary<Guid, GLAccount>();
            foreach (var acc in accounts)
            {
                accountDict[acc.AccountId] = acc;
            }

            // Set FullAccountNumber & AccountPath
            foreach (var acc in accounts)
            {
                if (acc.ParentAccountId.HasValue && accountDict.TryGetValue(acc.ParentAccountId.Value, out var parent))
                {
                    acc.FullAccountNumber = $"{parent.FullAccountNumber}.{acc.AccountNumber}";
                    acc.AccountPath = $"{parent.AccountPath}/{acc.AccountId}";
                }
                else
                {
                    acc.FullAccountNumber = acc.AccountNumber;
                    acc.AccountPath = acc.AccountId.ToString();
                }
            }

            // Determine IsLeaf: only accounts with no children are leaves
            var allParentIds = accounts.Where(a => a.ParentAccountId.HasValue)
                                       .Select(a => a.ParentAccountId.Value)
                                       .ToHashSet();

            foreach (var acc in accounts)
            {
                acc.IsLeaf = !allParentIds.Contains(acc.AccountId);
                acc.AllowManualEntries = acc.IsLeaf; // Only leaf accounts allow entries
            }

            // Insert
            int count = 0;
            foreach (var acc in accounts)
            {
                await _accountRepository.InsertAccountAsync(acc);
                count++;
            }

            await RecalculateHierarchyAsync();
            return count;
        }

        private Guid GetAccountId(string accountNumber, List<GLAccount> accounts)
        {
            return accounts.First(a => a.AccountNumber == accountNumber).AccountId;
        }

        public async Task<bool> DefaultAccountsExistAsync(long? companyId)
        {
            return await _accountRepository.FullAccountNumberExistsInCompanyAsync("1000000000", companyId);
        }

        public async Task RecalculateHierarchyAsync()
        {
            await _accountRepository.RecalculateAllPathsAsync();
            await _accountRepository.RecalculateAllCountsAsync();
        }

        // --- External API methods remain unchanged ---
        // (You can keep the rest of your external seeding logic as-is)

        private GLAccount CreateAccount(
            string accountNumber,
            string nameAr,
            string nameEn,
            AccountType accountType,
            NormalBalance normalBalance,
            int level,
            Guid? parentAccountId,
            long? companyId,
            string createdBy,
            DateTime createdAt,
            bool isCash = false,
            bool isBank = false,
            bool isControl = false,
            CashFlowCategory cashFlowCategory = CashFlowCategory.None,
            GLAccountFlags accountFlags = GLAccountFlags.None,
            WorkingCapitalType? workingCapitalType = null)
        {
            return new GLAccount
            {
                AccountId = Guid.NewGuid(),
                AccountNumber = accountNumber,
                FullAccountNumber = accountNumber, // Will be updated later
                AccountNameAr = nameAr,
                AccountNameEn = nameEn,
                AccountType = accountType,
                NormalBalance = normalBalance,
                Level = level,
                ParentAccountId = parentAccountId,
                IsLeaf = true, // Will be updated later
                IsActive = true,
                IsSystemAccount = true,
                AllowManualEntries = true, // Will be set to false for parents
                AllowPostingToParent = false,
                RequiresCostCenter = false,
                RequiresDepartment = false,
                RequiresProject = false,
                AccountNature = isCash ? AccountNature.Cash : (isBank ? AccountNature.Bank : AccountNature.None),
                IsControlAccount = isControl,
                ReconciliationMode = ReconciliationMode.None,
                Currency = "SAR",
                IsForeignCurrency = false,
                DisplayOrder = 0,
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

        // --- External seeding methods (unchanged, omitted for brevity) ---
        // Include them as in your original code if needed

        public async Task<int> SeedAccountsFromExternalApiAsync(string apiBaseUrl, string bearerToken, long? companyId, string createdBy)
        {
            // TODO: Implement external API seeding if needed
            throw new NotImplementedException("External API seeding not yet implemented");
        }

        /// <summary>
        /// Link existing treasury accounts (bank and cash) to their corresponding GL accounts
        /// </summary>
        public async Task<int> LinkTreasuryAccountsToGLAsync(long? companyId, string updatedBy)
        {
            int updatedCount = 0;

            // Get all bank accounts without GL account link
            var bankAccounts = _bankAccountRepository.GetList(x => !x.IsDeleted && !x.GLAccountId.HasValue);

            // Get the "Bank Accounts" parent GL account
            var bankAccountsGL = await _accountRepository.GetByAccountNumberAsync("1112000000");

            if (bankAccountsGL != null)
            {
                // Get all child bank GL accounts under "Bank Accounts"
                var bankGLAccounts = await _accountRepository.GetChildAccountsAsync(bankAccountsGL.AccountId);

                // Link each bank account to a corresponding GL account (round-robin if not enough GL accounts)
                for (int i = 0; i < bankAccounts.Count; i++)
                {
                    var bankAccount = bankAccounts[i];
                    var glAccount = bankGLAccounts[i % bankGLAccounts.Count]; // Round-robin assignment

                    bankAccount.GLAccountId = glAccount.AccountId;
                    bankAccount.Update_by = updatedBy;
                    bankAccount.Update_time = DateTime.Now;

                    _bankAccountRepository.Update(bankAccount, it => new { it.GLAccountId, it.Update_by, it.Update_time }, x => x.Id == bankAccount.Id);
                    updatedCount++;
                }
            }

            // Get all cash accounts without GL account link
            var cashAccounts = _cashAccountRepository.GetList(x => !x.IsDeleted && !x.GLAccountId.HasValue);

            // Get the "Cash Boxes" and "Petty Cash" parent GL accounts
            var cashBoxesGL = await _accountRepository.GetByAccountNumberAsync("1111000000");
            var pettyCashGL = await _accountRepository.GetByAccountNumberAsync("1113000000");

            if (cashBoxesGL != null && pettyCashGL != null)
            {
                // Get all child cash GL accounts
                var cashBoxGLAccounts = await _accountRepository.GetChildAccountsAsync(cashBoxesGL.AccountId);
                var pettyCashGLAccounts = await _accountRepository.GetChildAccountsAsync(pettyCashGL.AccountId);

                var allCashGLAccounts = cashBoxGLAccounts.Concat(pettyCashGLAccounts).ToList();

                // Link each cash account to a corresponding GL account
                for (int i = 0; i < cashAccounts.Count; i++)
                {
                    var cashAccount = cashAccounts[i];
                    var glAccount = allCashGLAccounts[i % allCashGLAccounts.Count]; // Round-robin assignment

                    cashAccount.GLAccountId = glAccount.AccountId;
                    cashAccount.Update_by = updatedBy;
                    cashAccount.Update_time = DateTime.Now;

                    _cashAccountRepository.Update(cashAccount, it => new { it.GLAccountId, it.Update_by, it.Update_time }, x => x.Id == cashAccount.Id);
                    updatedCount++;
                }
            }

            return updatedCount;
        }
    }

    // Keep your DTO classes (ExternalAccountDto, etc.) as they are
}