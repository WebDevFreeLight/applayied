using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.GL;
using SqlSugar;
using FinanceEnums = ZR.Model.Finance.Enums;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// GL Account Auto Creator Service
    /// Handles automatic creation of GL accounts for Treasury module
    /// خدمة إنشاء حسابات دفتر الأستاذ تلقائياً
    /// </summary>
    [AppService(ServiceType = typeof(IGLAccountAutoCreator), ServiceLifetime = LifeTime.Transient)]
    public class GLAccountAutoCreator : IGLAccountAutoCreator
    {
        private readonly IGLAccountRepository _accountRepository;
        private readonly IGLAccountService _glAccountService;

        // Standard account numbers for hierarchy
        private const string ASSETS_ACCOUNT_NUMBER = "1000000000";
        private const string CURRENT_ASSETS_ACCOUNT_NUMBER = "1100000000";
        private const string CASH_EQUIVALENTS_ACCOUNT_NUMBER = "1110000000";
        private const string CASH_BOXES_ACCOUNT_NUMBER = "1111000000";
        private const string BANK_ACCOUNTS_ACCOUNT_NUMBER = "1112000000";

        public GLAccountAutoCreator(
            IGLAccountRepository accountRepository,
            IGLAccountService glAccountService)
        {
            _accountRepository = accountRepository;
            _glAccountService = glAccountService;
        }

        /// <summary>
        /// Creates a GL Account for a new bank account
        /// </summary>
        public async Task<GLAccount> CreateBankGLAccountAsync(
            string bankNameAr,
            string bankNameEn,
            string currency,
            long? companyId,
            string createdBy,
            Guid? parentAccountId = null,
            string customAccountCode = null)
        {
            // 1. Determine parent account
            GLAccount parentAccount;
            if (parentAccountId.HasValue && parentAccountId.Value != Guid.Empty)
            {
                parentAccount = await _accountRepository.GetByIdAsync(parentAccountId.Value);
                if (parentAccount == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND,
                        new Dictionary<string, object> { { "AccountId", parentAccountId } });
                }

                // Validate parent is suitable for bank accounts
                if (!await ValidateBankParentAccountAsync(parentAccountId.Value))
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED,
                        new Dictionary<string, object> { { "Reason", "Invalid parent account for bank GL account" } });
                }
            }
            else
            {
                // Use default Bank Accounts parent
                parentAccount = await GetOrCreateBankParentAccountAsync(companyId, createdBy);
            }

            // 2. Generate account code if not provided
            string accountCode = customAccountCode;
            if (string.IsNullOrEmpty(accountCode))
            {
                accountCode = await GenerateNextAccountCodeAsync(parentAccount.AccountId, companyId);
            }
            else
            {
                // Validate custom code doesn't exist
                if (_glAccountService.CodeExists(accountCode))
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED,
                        new Dictionary<string, object> { { "Reason", $"Account code {accountCode} already exists" } });
                }
            }

            // 3. Create the GL Account
            var createDto = new CreateAccountDto
            {
                AccountNumber = accountCode,
                AccountNameAr = bankNameAr,
                AccountNameEn = bankNameEn,
                AccountType = AccountType.Asset,
                NormalBalance = NormalBalance.Debit,
                Level = parentAccount.Level + 1,
                ParentAccountId = parentAccount.AccountId,
                IsActive = true,
                IsLeaf = true,
                AllowManualEntries = true,
                AllowPostingToParent = false,
                AccountNature = AccountNature.Bank,
                ReconciliationMode = ReconciliationMode.Required,
                CashFlowCategory = FinanceEnums.CashFlowCategory.Operating,
                DimensionRequirements = DimensionRequirements.None,
                AccountFlags = GLAccountFlags.None
            };

            var glAccount = await _glAccountService.CreateAsync(createDto, createdBy);

            Console.WriteLine($"[GLAccountAutoCreator] Created bank GL account: {accountCode} - {bankNameEn} (ID: {glAccount.AccountId})");

            return glAccount;
        }

        /// <summary>
        /// Creates a GL Account for a new cash account
        /// </summary>
        public async Task<GLAccount> CreateCashGLAccountAsync(
            string cashAccountNameAr,
            string cashAccountNameEn,
            string currency,
            long? companyId,
            string createdBy,
            Guid? parentAccountId = null,
            string customAccountCode = null)
        {
            // 1. Determine parent account
            GLAccount parentAccount;
            if (parentAccountId.HasValue && parentAccountId.Value != Guid.Empty)
            {
                parentAccount = await _accountRepository.GetByIdAsync(parentAccountId.Value);
                if (parentAccount == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND,
                        new Dictionary<string, object> { { "AccountId", parentAccountId } });
                }
            }
            else
            {
                // Use default Cash Boxes parent
                parentAccount = await GetOrCreateCashParentAccountAsync(companyId, createdBy);
            }

            // 2. Generate account code if not provided
            string accountCode = customAccountCode;
            if (string.IsNullOrEmpty(accountCode))
            {
                accountCode = await GenerateNextAccountCodeAsync(parentAccount.AccountId, companyId);
            }

            // 3. Create the GL Account
            var createDto = new CreateAccountDto
            {
                AccountNumber = accountCode,
                AccountNameAr = cashAccountNameAr,
                AccountNameEn = cashAccountNameEn,
                AccountType = AccountType.Asset,
                NormalBalance = NormalBalance.Debit,
                Level = parentAccount.Level + 1,
                ParentAccountId = parentAccount.AccountId,
                IsActive = true,
                IsLeaf = true,
                AllowManualEntries = true,
                AllowPostingToParent = false,
                AccountNature = AccountNature.Cash,
                ReconciliationMode = ReconciliationMode.None,
                CashFlowCategory = FinanceEnums.CashFlowCategory.Operating,
                DimensionRequirements = DimensionRequirements.None,
                AccountFlags = GLAccountFlags.None
            };

            var glAccount = await _glAccountService.CreateAsync(createDto, createdBy);

            Console.WriteLine($"[GLAccountAutoCreator] Created cash GL account: {accountCode} - {cashAccountNameEn} (ID: {glAccount.AccountId})");

            return glAccount;
        }

        /// <summary>
        /// Generates the next available account code under a parent
        /// </summary>
        public async Task<string> GenerateNextAccountCodeAsync(Guid parentAccountId, long? companyId)
        {
            var parentAccount = await _accountRepository.GetByIdAsync(parentAccountId);
            if (parentAccount == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND,
                    new Dictionary<string, object> { { "AccountId", parentAccountId } });
            }

            // Get all children of the parent
            var children = await _accountRepository.GetChildAccountsAsync(parentAccountId);

            // Extract the parent's prefix (first 4 digits for standard 10-digit codes)
            var parentCode = parentAccount.AccountNumber;
            var prefix = parentCode.Length >= 4 ? parentCode.Substring(0, 4) : parentCode;

            // Find the max sequence number from existing children
            int maxSequence = 0;
            foreach (var child in children)
            {
                if (child.AccountNumber.Length >= 6 && child.AccountNumber.StartsWith(prefix))
                {
                    // Extract digits 5-6 (0-indexed positions 4-5) as sequence
                    if (int.TryParse(child.AccountNumber.Substring(4, 2), out int seq))
                    {
                        if (seq > maxSequence)
                            maxSequence = seq;
                    }
                }
            }

            // Increment by 10 for nice spacing (or by 1 if near limit)
            int newSequence = maxSequence < 90 ? maxSequence + 10 : maxSequence + 1;

            if (newSequence > 99)
            {
                // Fallback: use timestamp-based suffix
                newSequence = (int)(DateTime.Now.Ticks % 100);
            }

            // Build new code: prefix + sequence + padding
            string newCode;
            if (parentCode.Length == 10)
            {
                // Standard 10-digit format: XXXX + YY + 0000
                newCode = $"{prefix}{newSequence:D2}0000";
            }
            else
            {
                // Non-standard format: append sequence
                newCode = $"{parentCode}{newSequence:D2}";
            }

            // Verify uniqueness, retry if needed
            int retryCount = 0;
            while (_glAccountService.CodeExists(newCode) && retryCount < 10)
            {
                newSequence++;
                if (parentCode.Length == 10)
                {
                    newCode = $"{prefix}{newSequence:D2}0000";
                }
                else
                {
                    newCode = $"{parentCode}{newSequence:D2}";
                }
                retryCount++;
            }

            if (_glAccountService.CodeExists(newCode))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED,
                    new Dictionary<string, object> { { "Reason", "Unable to generate unique account code after multiple attempts" } });
            }

            Console.WriteLine($"[GLAccountAutoCreator] Generated account code: {newCode} (Parent: {parentCode}, Sequence: {newSequence})");

            return newCode;
        }

        /// <summary>
        /// Gets or creates the default parent account for bank GL accounts
        /// </summary>
        public async Task<GLAccount> GetOrCreateBankParentAccountAsync(long? companyId, string createdBy)
        {
            // Try to find existing Bank Accounts parent (1112000000)
            var bankAccountsParent = await _accountRepository.GetByAccountNumberAsync(BANK_ACCOUNTS_ACCOUNT_NUMBER);

            if (bankAccountsParent != null)
            {
                // Ensure it's not a leaf (can have children)
                if (bankAccountsParent.IsLeaf)
                {
                    bankAccountsParent.IsLeaf = false;
                    await _accountRepository.UpdateAsync(bankAccountsParent);
                }
                return bankAccountsParent;
            }

            // Bank Accounts parent not found - need to create the hierarchy
            Console.WriteLine($"[GLAccountAutoCreator] Bank Accounts parent not found, creating hierarchy...");

            // This should rarely happen if Chart of Accounts was properly seeded
            throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED,
                new Dictionary<string, object>
                {
                    { "Reason", "Bank Accounts parent GL account (1112000000) not found. Please run Chart of Accounts seeder." },
                    { "AccountNumber", BANK_ACCOUNTS_ACCOUNT_NUMBER }
                });
        }

        /// <summary>
        /// Gets or creates the default parent account for cash GL accounts
        /// </summary>
        public async Task<GLAccount> GetOrCreateCashParentAccountAsync(long? companyId, string createdBy)
        {
            // Try to find existing Cash Boxes parent (1111000000)
            var cashBoxesParent = await _accountRepository.GetByAccountNumberAsync(CASH_BOXES_ACCOUNT_NUMBER);

            if (cashBoxesParent != null)
            {
                // Ensure it's not a leaf (can have children)
                if (cashBoxesParent.IsLeaf)
                {
                    cashBoxesParent.IsLeaf = false;
                    await _accountRepository.UpdateAsync(cashBoxesParent);
                }
                return cashBoxesParent;
            }

            // Cash Boxes parent not found
            throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED,
                new Dictionary<string, object>
                {
                    { "Reason", "Cash Boxes parent GL account (1111000000) not found. Please run Chart of Accounts seeder." },
                    { "AccountNumber", CASH_BOXES_ACCOUNT_NUMBER }
                });
        }

        /// <summary>
        /// Gets parent GL accounts suitable for bank account placement
        /// </summary>
        public async Task<List<GLAccount>> GetBankParentAccountOptionsAsync(long? companyId)
        {
            // Get Asset accounts that could be parents for bank accounts
            // Note: We don't filter by IsLeaf because accounts like 1111000000 (Cash Boxes) and
            // 1112000000 (Bank Accounts) start as leaf accounts until children are added.
            // They are still valid parent targets based on their position in the hierarchy.
            var predicate = Expressionable.Create<GLAccount>();

            predicate = predicate.And(x => x.AccountType == AccountType.Asset);
            predicate = predicate.And(x => x.IsActive == true);
            predicate = predicate.And(x => x.Level >= 3 && x.Level <= 5); // Cash & Equivalents (L3), Cash Boxes/Bank Accounts (L4), sub-accounts (L5)

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var accounts = await _accountRepository.GetListAsync(predicate.ToExpression());

            Console.WriteLine($"[GLAccountAutoCreator] GetBankParentAccountOptionsAsync: Found {accounts.Count} asset accounts before filtering");

            // Filter to accounts under Cash & Equivalents hierarchy
            // Supports both short codes (121, 1211, 1212) and 10-digit codes (1110000000, 1111000000, 1112000000)
            var result = accounts
                .Where(a => !string.IsNullOrEmpty(a.AccountNumber) &&
                            (a.AccountNumber.StartsWith("121") ||   // Cash & Equivalents (short code: 121, 1211, 1212)
                             a.AccountNumber.StartsWith("1110") ||  // Cash & Equivalents (10-digit)
                             a.AccountNumber.StartsWith("1111") ||  // Cash Boxes (10-digit)
                             a.AccountNumber.StartsWith("1112")))   // Bank Accounts (10-digit)
                .OrderBy(a => a.AccountNumber)
                .ToList();

            Console.WriteLine($"[GLAccountAutoCreator] GetBankParentAccountOptionsAsync: Found {result.Count} accounts under Cash & Equivalents hierarchy");

            // If no accounts found, log warning
            if (!result.Any())
            {
                Console.WriteLine("[GLAccountAutoCreator] Warning: No suitable parent accounts found. Ensure Chart of Accounts has Cash & Equivalents hierarchy (121/1211/1212 or 1110000000/1111000000/1112000000)");
            }

            return result;
        }

        /// <summary>
        /// Validates if a GL account can be used as a parent for bank accounts
        /// </summary>
        public async Task<bool> ValidateBankParentAccountAsync(Guid parentAccountId)
        {
            var account = await _accountRepository.GetByIdAsync(parentAccountId);

            if (account == null)
                return false;

            // Must be an Asset account
            if (account.AccountType != AccountType.Asset)
                return false;

            // Must be active
            if (!account.IsActive)
                return false;

            // Must not exceed max level (allow up to level 9, leaving room for children at level 10)
            if (account.Level >= 10)
                return false;

            // Should be under Cash & Equivalents hierarchy
            // Supports both short codes (121x) and 10-digit codes (111x)
            if (!account.AccountNumber.StartsWith("121") && !account.AccountNumber.StartsWith("111"))
            {
                Console.WriteLine($"[GLAccountAutoCreator] Warning: Parent account {account.AccountNumber} is not under Cash & Equivalents (121x or 111x)");
                // Allow but log warning - user might have custom CoA structure
            }

            return true;
        }

        /// <summary>
        /// Gets parent GL accounts suitable for cash account placement
        /// </summary>
        public async Task<List<GLAccount>> GetCashParentAccountOptionsAsync(long? companyId)
        {
            // Get Asset accounts that could be parents for cash accounts
            // Note: We don't filter by IsLeaf because accounts like 1211 (Cash Box)
            // start as leaf accounts until children are added.
            var predicate = Expressionable.Create<GLAccount>();

            predicate = predicate.And(x => x.AccountType == AccountType.Asset);
            predicate = predicate.And(x => x.IsActive == true);
            predicate = predicate.And(x => x.Level >= 3 && x.Level <= 5); // Cash & Equivalents (L3), Cash Boxes (L4), sub-accounts (L5)

            if (companyId.HasValue)
            {
                predicate = predicate.And(x => x.CompanyID == companyId.Value);
            }

            var accounts = await _accountRepository.GetListAsync(predicate.ToExpression());

            Console.WriteLine($"[GLAccountAutoCreator] GetCashParentAccountOptionsAsync: Found {accounts.Count} asset accounts before filtering");

            // Filter to accounts under Cash Box hierarchy
            // Supports both short codes (1211 for Cash Box) and 10-digit codes (1111 for Cash Boxes)
            var result = accounts
                .Where(a => !string.IsNullOrEmpty(a.AccountNumber) &&
                            (a.AccountNumber.StartsWith("121") ||   // Cash & Equivalents hierarchy (short code)
                             a.AccountNumber.StartsWith("1110") ||  // Cash & Equivalents (10-digit)
                             a.AccountNumber.StartsWith("1111")))   // Cash Boxes (10-digit)
                .OrderBy(a => a.AccountNumber)
                .ToList();

            Console.WriteLine($"[GLAccountAutoCreator] GetCashParentAccountOptionsAsync: Found {result.Count} accounts under Cash hierarchy");

            // If no accounts found, log warning
            if (!result.Any())
            {
                Console.WriteLine("[GLAccountAutoCreator] Warning: No suitable parent accounts found for Cash. Ensure Chart of Accounts has Cash hierarchy (121/1211 or 1110000000/1111000000)");
            }

            return result;
        }
    }
}
