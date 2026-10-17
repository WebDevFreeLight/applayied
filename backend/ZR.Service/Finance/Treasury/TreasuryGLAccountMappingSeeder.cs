using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Enums;
using ZR.Repository.Finance.GL;
using ZR.Repository.Finance.Treasury;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Service.Finance.Treasury
{
    /// <summary>
    /// Treasury GL Account Mapping Seeder - زراعة تعيينات حسابات الخزينة
    /// Seeds default treasury GL account mappings for cheques, payments, reconciliation, etc.
    /// </summary>
    [AppService(ServiceType = typeof(ITreasuryGLAccountMappingSeeder), ServiceLifetime = LifeTime.Transient)]
    public class TreasuryGLAccountMappingSeeder : ITreasuryGLAccountMappingSeeder
    {
        private readonly TreasuryGLAccountMappingRepository _mappingRepository;
        private readonly IGLAccountRepository _accountRepository;

        public TreasuryGLAccountMappingSeeder(
            TreasuryGLAccountMappingRepository mappingRepository,
            IGLAccountRepository accountRepository)
        {
            _mappingRepository = mappingRepository;
            _accountRepository = accountRepository;
        }

        public async Task<int> SeedDefaultMappingsAsync(long? companyId, string createdBy)
        {
            if (await DefaultMappingsExistAsync(companyId))
            {
                return 0;
            }

            var mappings = new List<TreasuryGLAccountMapping>();
            var now = DateTime.Now;

            // Get GL accounts needed for treasury mappings
            var accountsReceivableGL = await _accountRepository.GetByAccountNumberAsync("1120000000");
            var accountsPayableGL = await _accountRepository.GetByAccountNumberAsync("2110000000");
            var bankAccountsGL = await _accountRepository.GetByAccountNumberAsync("1112000000");
            var cashAccountsGL = await _accountRepository.GetByAccountNumberAsync("1111000000");
            var pettyCashGL = await _accountRepository.GetByAccountNumberAsync("1113000000");
            var financialExpensesGL = await _accountRepository.GetByAccountNumberAsync("5400000000");
            var otherIncomeGL = await _accountRepository.GetByAccountNumberAsync("4200000000");

            // We need to create GL accounts for cheques if they don't exist
            // For now, we'll create mappings that point to current liabilities for cheques issued
            // and current assets for cheques received

            // Get or create Cheques Issued (Liability) account
            // Operating - Working Capital (Payables) - cheques issued are short-term payment obligations
            var chequesIssuedGL = await GetOrCreateGLAccountAsync(
                "2115000000",
                "شيكات صادرة",
                "Cheques Issued",
                Model.Finance.Enums.AccountType.Liability,
                Model.Finance.Enums.NormalBalance.Credit,
                4,
                accountsPayableGL?.ParentAccountId ?? Guid.Empty,
                companyId,
                createdBy,
                now,
                cashFlowCategory: CashFlowCategory.Operating,
                accountFlags: GLAccountFlags.WorkingCapital,
                workingCapitalType: WorkingCapitalType.Payables);

            // Get or create Cheques Received (Asset) account
            // Operating - Working Capital (Receivables) - cheques received are short-term receivables
            var chequesReceivedGL = await GetOrCreateGLAccountAsync(
                "1125000000",
                "شيكات واردة",
                "Cheques Received",
                Model.Finance.Enums.AccountType.Asset,
                Model.Finance.Enums.NormalBalance.Debit,
                4,
                accountsReceivableGL?.ParentAccountId ?? Guid.Empty,
                companyId,
                createdBy,
                now,
                cashFlowCategory: CashFlowCategory.Operating,
                accountFlags: GLAccountFlags.WorkingCapital,
                workingCapitalType: WorkingCapitalType.Receivables);

            // Get or create Bounced Cheques (Asset) account
            // Operating - Working Capital (Receivables) - bounced cheques become receivables
            var bouncedChequesGL = await GetOrCreateGLAccountAsync(
                "1126000000",
                "شيكات مرتجعة",
                "Bounced Cheques",
                Model.Finance.Enums.AccountType.Asset,
                Model.Finance.Enums.NormalBalance.Debit,
                4,
                accountsReceivableGL?.ParentAccountId ?? Guid.Empty,
                companyId,
                createdBy,
                now,
                cashFlowCategory: CashFlowCategory.Operating,
                accountFlags: GLAccountFlags.WorkingCapital,
                workingCapitalType: WorkingCapitalType.Receivables);

            // Get or create Reconciliation Difference account
            // Operating - Non-cash adjustment (differences posted during bank reconciliation)
            var reconciliationDiffGL = await GetOrCreateGLAccountAsync(
                "5405000000",
                "فروقات التسوية البنكية",
                "Bank Reconciliation Differences",
                Model.Finance.Enums.AccountType.Expense,
                Model.Finance.Enums.NormalBalance.Debit,
                4,
                financialExpensesGL?.AccountId ?? Guid.Empty,
                companyId,
                createdBy,
                now,
                cashFlowCategory: CashFlowCategory.Operating);

            // Get or create Suspense Account
            // None - Temporary holding account, not directly part of cash flow
            var suspenseAccountGL = await GetOrCreateGLAccountAsync(
                "2125000000",
                "حساب معلق",
                "Suspense Account",
                Model.Finance.Enums.AccountType.Liability,
                Model.Finance.Enums.NormalBalance.Credit,
                4,
                accountsPayableGL?.ParentAccountId ?? Guid.Empty,
                companyId,
                createdBy,
                now,
                cashFlowCategory: CashFlowCategory.None);

            // Get or create FX Gain/Loss account
            // Operating - ForeignExchange flag - realized/unrealized FX adjustments
            var fxGainLossGL = await GetOrCreateGLAccountAsync(
                "4205000000",
                "أرباح وخسائر صرف العملات",
                "Foreign Exchange Gain/Loss",
                Model.Finance.Enums.AccountType.Revenue,
                Model.Finance.Enums.NormalBalance.Credit,
                4,
                otherIncomeGL?.AccountId ?? Guid.Empty,
                companyId,
                createdBy,
                now,
                cashFlowCategory: CashFlowCategory.Operating,
                accountFlags: GLAccountFlags.ForeignExchange);

            // Create default mappings
            // Bank Account Mappings
            if (bankAccountsGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.DefaultBankAccount,
                    bankAccountsGL.AccountId,
                    bankAccountsGL.AccountNumber,
                    bankAccountsGL.AccountNameEn,
                    "Default GL account for bank accounts",
                    companyId,
                    createdBy,
                    now));
            }

            if (financialExpensesGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.BankCharges,
                    financialExpensesGL.AccountId,
                    financialExpensesGL.AccountNumber,
                    financialExpensesGL.AccountNameEn,
                    "Bank charges and fees",
                    companyId,
                    createdBy,
                    now));
            }

            if (otherIncomeGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.BankInterestIncome,
                    otherIncomeGL.AccountId,
                    otherIncomeGL.AccountNumber,
                    otherIncomeGL.AccountNameEn,
                    "Bank interest income",
                    companyId,
                    createdBy,
                    now));
            }

            // Cash Account Mappings
            if (cashAccountsGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.DefaultCashAccount,
                    cashAccountsGL.AccountId,
                    cashAccountsGL.AccountNumber,
                    cashAccountsGL.AccountNameEn,
                    "Default GL account for cash accounts",
                    companyId,
                    createdBy,
                    now));
            }

            if (pettyCashGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.PettyCash,
                    pettyCashGL.AccountId,
                    pettyCashGL.AccountNumber,
                    pettyCashGL.AccountNameEn,
                    "Petty cash accounts",
                    companyId,
                    createdBy,
                    now));
            }

            // Payment Mappings
            if (accountsPayableGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.AccountsPayable,
                    accountsPayableGL.AccountId,
                    accountsPayableGL.AccountNumber,
                    accountsPayableGL.AccountNameEn,
                    "Default accounts payable",
                    companyId,
                    createdBy,
                    now));
            }

            if (accountsReceivableGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.AccountsReceivable,
                    accountsReceivableGL.AccountId,
                    accountsReceivableGL.AccountNumber,
                    accountsReceivableGL.AccountNameEn,
                    "Default accounts receivable",
                    companyId,
                    createdBy,
                    now));
            }

            // Cheque Mappings
            if (chequesIssuedGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.ChequesIssued,
                    chequesIssuedGL.AccountId,
                    chequesIssuedGL.AccountNumber,
                    chequesIssuedGL.AccountNameEn,
                    "GL account for issued cheques (liability)",
                    companyId,
                    createdBy,
                    now,
                    priority: 10));
            }

            if (chequesReceivedGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.ChequesReceived,
                    chequesReceivedGL.AccountId,
                    chequesReceivedGL.AccountNumber,
                    chequesReceivedGL.AccountNameEn,
                    "GL account for received cheques (asset)",
                    companyId,
                    createdBy,
                    now,
                    priority: 10));
            }

            if (bouncedChequesGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.BouncedCheques,
                    bouncedChequesGL.AccountId,
                    bouncedChequesGL.AccountNumber,
                    bouncedChequesGL.AccountNameEn,
                    "GL account for bounced cheques",
                    companyId,
                    createdBy,
                    now,
                    priority: 10));
            }

            // Reconciliation Mappings
            if (reconciliationDiffGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.ReconciliationDifference,
                    reconciliationDiffGL.AccountId,
                    reconciliationDiffGL.AccountNumber,
                    reconciliationDiffGL.AccountNameEn,
                    "Bank reconciliation differences",
                    companyId,
                    createdBy,
                    now));
            }

            if (suspenseAccountGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.SuspenseAccount,
                    suspenseAccountGL.AccountId,
                    suspenseAccountGL.AccountNumber,
                    suspenseAccountGL.AccountNameEn,
                    "Temporary suspense account",
                    companyId,
                    createdBy,
                    now));
            }

            // FX Mappings
            if (fxGainLossGL != null)
            {
                mappings.Add(CreateMapping(
                    GLMappingType.FxGainLoss,
                    fxGainLossGL.AccountId,
                    fxGainLossGL.AccountNumber,
                    fxGainLossGL.AccountNameEn,
                    "Realized foreign exchange gains/losses",
                    companyId,
                    createdBy,
                    now));

                mappings.Add(CreateMapping(
                    GLMappingType.UnrealizedFxGainLoss,
                    fxGainLossGL.AccountId,
                    fxGainLossGL.AccountNumber,
                    fxGainLossGL.AccountNameEn,
                    "Unrealized foreign exchange gains/losses",
                    companyId,
                    createdBy,
                    now));
            }

            // Insert all mappings
            int count = 0;
            foreach (var mapping in mappings)
            {
                var inserted = _mappingRepository.Insert(mapping);
                if (inserted > 0)
                {
                    count++;
                }
            }

            return count;
        }

        public async Task<bool> DefaultMappingsExistAsync(long? companyId)
        {
            var existingMapping = _mappingRepository.GetFirst(x =>
                x.MappingType == GLMappingType.ChequesIssued &&
                (companyId == null || x.CompanyID == companyId));

            return existingMapping != null;
        }

        public async Task<int> UpdateMappingsWithGLInfoAsync(long? companyId, string updatedBy)
        {
            var mappings = _mappingRepository.GetList(x =>
                companyId == null || x.CompanyID == companyId);

            int updatedCount = 0;

            foreach (var mapping in mappings)
            {
                var glAccount = await _accountRepository.GetByIdAsync(mapping.GLAccountId);
                if (glAccount != null)
                {
                    mapping.GLAccountCode = glAccount.AccountNumber;
                    mapping.GLAccountName = glAccount.AccountNameEn;
                    mapping.Update_by = updatedBy;
                    mapping.Update_time = DateTime.Now;

                    _mappingRepository.Update(mapping);
                    updatedCount++;
                }
            }

            return updatedCount;
        }

        private async Task<Model.Finance.GL.GLAccount> GetOrCreateGLAccountAsync(
            string accountNumber,
            string nameAr,
            string nameEn,
            Model.Finance.Enums.AccountType accountType,
            Model.Finance.Enums.NormalBalance normalBalance,
            int level,
            Guid parentAccountId,
            long? companyId,
            string createdBy,
            DateTime createdAt,
            CashFlowCategory cashFlowCategory = CashFlowCategory.None,
            GLAccountFlags accountFlags = GLAccountFlags.None,
            WorkingCapitalType? workingCapitalType = null)
        {
            // Try to get existing account
            var existing = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (existing != null)
            {
                return existing;
            }

            // Create new account
            var account = new Model.Finance.GL.GLAccount
            {
                AccountId = Guid.NewGuid(),
                AccountNumber = accountNumber,
                FullAccountNumber = accountNumber,
                AccountNameAr = nameAr,
                AccountNameEn = nameEn,
                AccountType = accountType,
                NormalBalance = normalBalance,
                Level = level,
                ParentAccountId = parentAccountId != Guid.Empty ? parentAccountId : null,
                IsLeaf = true,
                IsActive = true,
                IsSystemAccount = true,
                AllowManualEntries = true,
                AllowPostingToParent = false,
                RequiresCostCenter = false,
                RequiresDepartment = false,
                RequiresProject = false,
                AccountNature = AccountNature.None,
                IsControlAccount = false,
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

            await _accountRepository.InsertAccountAsync(account);
            return account;
        }

        private TreasuryGLAccountMapping CreateMapping(
            string mappingType,
            Guid glAccountId,
            string glAccountCode,
            string glAccountName,
            string description,
            long? companyId,
            string createdBy,
            DateTime createdAt,
            string currency = null,
            int priority = 100)
        {
            return new TreasuryGLAccountMapping
            {
                Id = Guid.NewGuid(),
                MappingType = mappingType,
                GLAccountId = glAccountId,
                GLAccountCode = glAccountCode,
                GLAccountName = glAccountName,
                Currency = currency,
                Description = description,
                IsActive = true,
                Priority = priority,
                EffectiveFrom = createdAt,
                EffectiveTo = null,
                CompanyID = companyId,
                Create_by = createdBy,
                Create_time = createdAt
            };
        }
    }
}
