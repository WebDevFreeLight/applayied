using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using MiniExcelLibs;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Repository;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.GL;
using ZR.Service.IService.Finance.Configuration;
using ZR.Service.Finance.Configuration;
using FinanceEnums = ZR.Model.Finance.Enums;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// GL Account Service - خدمة الحسابات المالية
    /// IFRS Compliant Chart of Accounts management
    /// </summary>
    [AppService(ServiceType = typeof(IGLAccountService), ServiceLifetime = LifeTime.Transient)]
    public class GLAccountService : BaseService<GLAccount>, IGLAccountService
    {
        private readonly IGLAccountRepository _accountRepository;
        private readonly IGLAccountOpeningBalanceRepository _openingBalanceRepository;
        private readonly IGLJournalEntryRepository _journalEntryRepository;
        private readonly IFiscalYearHelper _fiscalYearHelper;

        public GLAccountService(
            IGLAccountRepository accountRepository,
            IGLAccountOpeningBalanceRepository openingBalanceRepository,
            IGLJournalEntryRepository journalEntryRepository,
            IFiscalYearHelper fiscalYearHelper)
        {
            _accountRepository = accountRepository;
            _openingBalanceRepository = openingBalanceRepository;
            _journalEntryRepository = journalEntryRepository;
            _fiscalYearHelper = fiscalYearHelper;
        }

        #region Account CRUD Operations

        public PagedInfo<AccountResponseDto> GetList(GLAccountQueryDto query, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLAccount>();
            predicate = predicate.AndIF(query.AccountId.HasValue, it => it.AccountId == query.AccountId.Value);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.AccountNumber), it => it.AccountNumber.Contains(query.AccountNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.AccountNameAr), it => it.AccountNameAr.Contains(query.AccountNameAr));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.AccountNameEn), it => it.AccountNameEn.Contains(query.AccountNameEn));
            predicate = predicate.AndIF(query.AccountType.HasValue, it => it.AccountType == query.AccountType.Value);
            predicate = predicate.AndIF(query.NormalBalance.HasValue, it => it.NormalBalance == query.NormalBalance.Value);
            predicate = predicate.AndIF(query.ParentAccountId.HasValue, it => it.ParentAccountId == query.ParentAccountId);
            predicate = predicate.AndIF(query.IsActive.HasValue, it => it.IsActive == query.IsActive.Value);
            predicate = predicate.AndIF(query.IsLeaf.HasValue, it => it.IsLeaf == query.IsLeaf.Value);
            predicate = predicate.AndIF(query.AllowManualEntries.HasValue, it => it.AllowManualEntries == query.AllowManualEntries.Value);
            predicate = predicate.AndIF(query.DimensionRequirements.HasValue, it => (it.DimensionRequirements & query.DimensionRequirements.Value) == query.DimensionRequirements.Value);
            predicate = predicate.AndIF(query.AccountNature.HasValue, it => it.AccountNature == query.AccountNature.Value);
            predicate = predicate.AndIF(query.IsControlAccount.HasValue, it => it.IsControlAccount == query.IsControlAccount.Value);
            predicate = predicate.AndIF(query.ReconciliationMode.HasValue, it => it.ReconciliationMode == query.ReconciliationMode.Value);
            predicate = predicate.AndIF(query.Level.HasValue, it => it.Level == query.Level.Value);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Currency), it => it.Currency == query.Currency);
            predicate = predicate.AndIF(query.BeginCreateTime.HasValue, it => it.Create_time >= query.BeginCreateTime.Value);
            predicate = predicate.AndIF(query.EndCreateTime.HasValue, it => it.Create_time <= query.EndCreateTime.Value);

            // Cash Flow Classification Filters
            predicate = predicate.AndIF(query.CashFlowCategory.HasValue, it => it.CashFlowCategory == query.CashFlowCategory.Value);
            predicate = predicate.AndIF(query.AccountFlags.HasValue, it => (it.AccountFlags & query.AccountFlags.Value) != 0);
            predicate = predicate.AndIF(query.WorkingCapitalType.HasValue, it => it.WorkingCapitalType == query.WorkingCapitalType.Value);

            var result = Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.AccountNumber)
                .ToPage(pager);

            var dtoList = result.Result.Select(MapToResponseDto).ToList();

            return new PagedInfo<AccountResponseDto>
            {
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalNum = result.TotalNum,
                Result = dtoList
            };
        }

        public AccountResponseDto GetById(Guid id)
        {
            var account = _accountRepository.GetById(id);
            return account == null ? null : MapToResponseDto(account);
        }

        public async Task<AccountResponseDto> GetByIdAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            return account == null ? null : MapToResponseDto(account);
        }

        public AccountResponseDto GetByAccountNumber(string accountNumber)
        {
            var account = _accountRepository.GetByAccountNumber(accountNumber);
            return account == null ? null : MapToResponseDto(account);
        }

        public async Task<AccountResponseDto> GetByAccountNumberAsync(string accountNumber)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            return account == null ? null : MapToResponseDto(account);
        }

        public GLAccount Create(CreateAccountDto dto, string userName)
        {
            // Note: AccountNumber uniqueness is NOT checked globally
            // The same AccountNumber can exist under different parents
            // Only FullAccountNumber must be unique (validated below after building it)

            GLAccount parent = null;
            string parentFullAccountNumber = null;

            // Validate parent exists and hierarchy
            if (dto.ParentAccountId.HasValue)
            {
                parent = _accountRepository.GetById(dto.ParentAccountId.Value);
                if (parent == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_PARENT_NOT_FOUND, new Dictionary<string, object> { { "ParentId", dto.ParentAccountId.Value } });
                }

                // Prevent adding children to leaf accounts
                if (parent.IsLeaf)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_PARENT_IS_LEAF, new Dictionary<string, object> { { "ParentId", dto.ParentAccountId.Value } });
                }

                // Set level based on parent
                dto.Level = parent.Level + 1;
                parentFullAccountNumber = parent.FullAccountNumber;
            }
            else
            {
                dto.Level = 1; // Root account
            }

            // Set default normal balance based on account type
            if (dto.NormalBalance == default)
            {
                dto.NormalBalance = GetDefaultNormalBalance(dto.AccountType);
            }

            // Validate Cash Flow Classification
            ValidateCashFlowClassification(dto.AccountFlags, dto.WorkingCapitalType, dto.CashFlowCategory);

            var model = dto.Adapt<GLAccount>();
            model.AccountId = Guid.NewGuid();
            model.IsLeaf = dto.IsLeaf; // Respect the IsLeaf value from DTO (allows creating parent accounts)
            model.Create_by = userName;
            model.Create_time = DateTime.Now;
            model.RowVersion = 1;

            // Build FullAccountNumber
            model.FullAccountNumber = model.BuildFullAccountNumber(parentFullAccountNumber);

            // Validate FullAccountNumber uniqueness
            if (_accountRepository.FullAccountNumberExists(model.FullAccountNumber))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NUMBER_DUPLICATE, new Dictionary<string, object> { { "AccountNumber", model.FullAccountNumber } });
            }

            // Validate IsRetainedEarningsAccount uniqueness - only ONE account per company can have this flag
            if (dto.IsRetainedEarningsAccount)
            {
                var existingRetainedEarnings = Context.Queryable<GLAccount>()
                    .Where(a => a.IsRetainedEarningsAccount && a.CompanyID == model.CompanyID)
                    .First();
                if (existingRetainedEarnings != null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "AccountNumber", existingRetainedEarnings.FullAccountNumber }, { "Reason", "RetainedEarningsAccountAlreadyExists" } });
                }
            }

            // Build account path (for hierarchy queries using GUIDs)
            if (dto.ParentAccountId.HasValue)
            {
                model.AccountPath = string.IsNullOrEmpty(parent.AccountPath)
                    ? $"{parent.AccountId}/{model.AccountId}"
                    : $"{parent.AccountPath}/{model.AccountId}";
            }
            else
            {
                model.AccountPath = model.AccountId.ToString();
            }

            _accountRepository.InsertAccount(model);

            // Update parent's child count and all ancestors' descendant counts
            if (dto.ParentAccountId.HasValue)
            {
                _accountRepository.IncrementChildCount(dto.ParentAccountId.Value);
                _accountRepository.IncrementAncestorDescendantCounts(model.AccountId);
            }

            return model;
        }

        public async Task<GLAccount> CreateAsync(CreateAccountDto dto, string userName)
        {
            // Note: AccountNumber uniqueness is NOT checked globally
            // The same AccountNumber can exist under different parents
            // Only FullAccountNumber must be unique (validated below after building it)

            GLAccount parent = null;
            string parentFullAccountNumber = null;

            // Validate parent exists and hierarchy
            if (dto.ParentAccountId.HasValue)
            {
                parent = await _accountRepository.GetByIdAsync(dto.ParentAccountId.Value);
                if (parent == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_PARENT_NOT_FOUND, new Dictionary<string, object> { { "ParentId", dto.ParentAccountId.Value } });
                }

                // Prevent adding children to leaf accounts
                if (parent.IsLeaf)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_PARENT_IS_LEAF, new Dictionary<string, object> { { "ParentId", dto.ParentAccountId.Value } });
                }

                dto.Level = parent.Level + 1;
                parentFullAccountNumber = parent.FullAccountNumber;
            }
            else
            {
                dto.Level = 1;
            }

            if (dto.NormalBalance == default)
            {
                dto.NormalBalance = GetDefaultNormalBalance(dto.AccountType);
            }

            // Validate Cash Flow Classification
            ValidateCashFlowClassification(dto.AccountFlags, dto.WorkingCapitalType, dto.CashFlowCategory);

            var model = dto.Adapt<GLAccount>();
            model.AccountId = Guid.NewGuid();
            model.IsLeaf = dto.IsLeaf; // Respect the IsLeaf value from DTO (allows creating parent accounts)
            model.Create_by = userName;
            model.Create_time = DateTime.Now;
            model.RowVersion = 1;

            // Build FullAccountNumber
            model.FullAccountNumber = model.BuildFullAccountNumber(parentFullAccountNumber);

            // Validate FullAccountNumber uniqueness
            if (await _accountRepository.FullAccountNumberExistsAsync(model.FullAccountNumber))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NUMBER_DUPLICATE, new Dictionary<string, object> { { "AccountNumber", model.FullAccountNumber } });
            }

            // Validate IsRetainedEarningsAccount uniqueness - only ONE account per company can have this flag
            if (dto.IsRetainedEarningsAccount)
            {
                var existingRetainedEarnings = await Context.Queryable<GLAccount>()
                    .Where(a => a.IsRetainedEarningsAccount && a.CompanyID == model.CompanyID)
                    .FirstAsync();
                if (existingRetainedEarnings != null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "AccountNumber", existingRetainedEarnings.FullAccountNumber }, { "Reason", "RetainedEarningsAccountAlreadyExists" } });
                }
            }

            // Build account path (for hierarchy queries using GUIDs)
            if (dto.ParentAccountId.HasValue)
            {
                model.AccountPath = string.IsNullOrEmpty(parent.AccountPath)
                    ? $"{parent.AccountId}/{model.AccountId}"
                    : $"{parent.AccountPath}/{model.AccountId}";
            }
            else
            {
                model.AccountPath = model.AccountId.ToString();
            }

            await _accountRepository.InsertAccountAsync(model);

            // Update parent's child count and all ancestors' descendant counts
            if (dto.ParentAccountId.HasValue)
            {
                await _accountRepository.IncrementChildCountAsync(dto.ParentAccountId.Value);
                await _accountRepository.IncrementAncestorDescendantCountsAsync(model.AccountId);
            }

            return model;
        }

        public int Update(UpdateAccountDto dto, string userName)
        {
            var existing = _accountRepository.GetById(dto.AccountId);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND, new Dictionary<string, object> { { "AccountId", dto.AccountId } });
            }

            // Check for system account - System accounts are protected from modification
            // They are used for critical system operations (year-end closing, suspense accounts, etc.)
            if (existing.IsSystemAccount)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "AccountNumber", existing.FullAccountNumber }, { "Reason", "SystemAccountProtected" } });
            }

            // Check for structural changes after transactions exist
            if (_accountRepository.HasTransactions(dto.AccountId))
            {
                bool hasStructuralChanges =
                    dto.AccountNumber != existing.AccountNumber ||
                    dto.ParentAccountId != existing.ParentAccountId ||
                    dto.AccountType != existing.AccountType ||
                    dto.NormalBalance != existing.NormalBalance ||
                    dto.IsLeaf != existing.IsLeaf;

                if (hasStructuralChanges)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_HAS_TRANSACTIONS, new Dictionary<string, object> { { "AccountId", dto.AccountId } });
                }
            }

            // Validate parent change for circular reference
            if (dto.ParentAccountId != existing.ParentAccountId)
            {
                if (_accountRepository.WouldCreateCircularReference(dto.AccountId, dto.ParentAccountId))
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_CIRCULAR_REFERENCE);
                }
            }

            // Optimistic concurrency check
            if (existing.RowVersion != dto.RowVersion)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_CONCURRENCY_ERROR);
            }

            // Validate Cash Flow Classification
            ValidateCashFlowClassification(dto.AccountFlags, dto.WorkingCapitalType, dto.CashFlowCategory);

            // Validate IsRetainedEarningsAccount uniqueness when being changed to true
            if (dto.IsRetainedEarningsAccount && !existing.IsRetainedEarningsAccount)
            {
                var existingRetainedEarnings = Context.Queryable<GLAccount>()
                    .Where(a => a.IsRetainedEarningsAccount && a.CompanyID == existing.CompanyID && a.AccountId != dto.AccountId)
                    .First();
                if (existingRetainedEarnings != null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "AccountNumber", existingRetainedEarnings.FullAccountNumber }, { "Reason", "RetainedEarningsAccountAlreadyExists" } });
                }
            }

            var model = dto.Adapt<GLAccount>();
            model.Update_by = userName;
            model.Update_time = DateTime.Now;
            model.RowVersion = existing.RowVersion + 1;

            // Rebuild FullAccountNumber if AccountNumber or ParentAccountId changed
            bool needsFullAccountNumberRebuild = dto.AccountNumber != existing.AccountNumber ||
                                                  dto.ParentAccountId != existing.ParentAccountId;

            if (needsFullAccountNumberRebuild)
            {
                string parentFullAccountNumber = null;
                if (dto.ParentAccountId.HasValue)
                {
                    var parent = _accountRepository.GetById(dto.ParentAccountId.Value);
                    parentFullAccountNumber = parent?.FullAccountNumber;
                }
                model.FullAccountNumber = model.BuildFullAccountNumber(parentFullAccountNumber);

                // Validate new FullAccountNumber uniqueness
                if (_accountRepository.FullAccountNumberExists(model.FullAccountNumber, dto.AccountId))
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NUMBER_DUPLICATE, new Dictionary<string, object> { { "AccountNumber", model.FullAccountNumber } });
                }
            }
            else
            {
                // Keep the existing FullAccountNumber
                model.FullAccountNumber = existing.FullAccountNumber;
            }

            return _accountRepository.UpdateAccount(model);
        }

        public async Task<int> UpdateAsync(UpdateAccountDto dto, string userName)
        {
            var existing = await _accountRepository.GetByIdAsync(dto.AccountId);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND, new Dictionary<string, object> { { "AccountId", dto.AccountId } });
            }

            // Check for system account - System accounts are protected from modification
            if (existing.IsSystemAccount)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "AccountNumber", existing.FullAccountNumber }, { "Reason", "SystemAccountProtected" } });
            }

            // Check for structural changes after transactions exist
            if (await _accountRepository.HasTransactionsAsync(dto.AccountId))
            {
                bool hasStructuralChanges =
                    dto.AccountNumber != existing.AccountNumber ||
                    dto.ParentAccountId != existing.ParentAccountId ||
                    dto.AccountType != existing.AccountType ||
                    dto.NormalBalance != existing.NormalBalance ||
                    dto.IsLeaf != existing.IsLeaf;

                if (hasStructuralChanges)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_HAS_TRANSACTIONS, new Dictionary<string, object> { { "AccountId", dto.AccountId } });
                }
            }

            if (dto.ParentAccountId != existing.ParentAccountId)
            {
                if (await _accountRepository.WouldCreateCircularReferenceAsync(dto.AccountId, dto.ParentAccountId))
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_CIRCULAR_REFERENCE);
                }
            }

            if (existing.RowVersion != dto.RowVersion)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_CONCURRENCY_ERROR);
            }

            // Validate Cash Flow Classification
            ValidateCashFlowClassification(dto.AccountFlags, dto.WorkingCapitalType, dto.CashFlowCategory);

            // Validate IsRetainedEarningsAccount uniqueness when being changed to true
            if (dto.IsRetainedEarningsAccount && !existing.IsRetainedEarningsAccount)
            {
                var existingRetainedEarnings = await Context.Queryable<GLAccount>()
                    .Where(a => a.IsRetainedEarningsAccount && a.CompanyID == existing.CompanyID && a.AccountId != dto.AccountId)
                    .FirstAsync();
                if (existingRetainedEarnings != null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "AccountNumber", existingRetainedEarnings.FullAccountNumber }, { "Reason", "RetainedEarningsAccountAlreadyExists" } });
                }
            }

            var model = dto.Adapt<GLAccount>();
            model.Update_by = userName;
            model.Update_time = DateTime.Now;
            model.RowVersion = existing.RowVersion + 1;

            // Rebuild FullAccountNumber if AccountNumber or ParentAccountId changed
            bool needsFullAccountNumberRebuild = dto.AccountNumber != existing.AccountNumber ||
                                                  dto.ParentAccountId != existing.ParentAccountId;

            if (needsFullAccountNumberRebuild)
            {
                string parentFullAccountNumber = null;
                if (dto.ParentAccountId.HasValue)
                {
                    var parent = await _accountRepository.GetByIdAsync(dto.ParentAccountId.Value);
                    parentFullAccountNumber = parent?.FullAccountNumber;
                }
                model.FullAccountNumber = model.BuildFullAccountNumber(parentFullAccountNumber);

                // Validate new FullAccountNumber uniqueness
                if (await _accountRepository.FullAccountNumberExistsAsync(model.FullAccountNumber, dto.AccountId))
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NUMBER_DUPLICATE, new Dictionary<string, object> { { "AccountNumber", model.FullAccountNumber } });
                }
            }
            else
            {
                // Keep the existing FullAccountNumber
                model.FullAccountNumber = existing.FullAccountNumber;
            }

            return await _accountRepository.UpdateAccountAsync(model);
        }

        public int Delete(Guid[] ids)
        {
            foreach (var id in ids)
            {
                var validation = ValidateForDeletion(id);
                if (!validation.IsValid)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "Errors", string.Join("; ", validation.Errors) } });
                }
            }

            // Update counts before deleting
            foreach (var id in ids)
            {
                var account = _accountRepository.GetById(id);
                if (account != null && account.ParentAccountId.HasValue)
                {
                    _accountRepository.DecrementChildCount(account.ParentAccountId.Value);
                    _accountRepository.DecrementAncestorDescendantCounts(id, account.DescendantCount);
                }
            }

            return _accountRepository.DeleteByIds(ids);
        }

        public async Task<int> DeleteAsync(Guid[] ids)
        {
            foreach (var id in ids)
            {
                var validation = await ValidateForDeletionAsync(id);
                if (!validation.IsValid)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "Errors", string.Join("; ", validation.Errors) } });
                }
            }

            // Update counts before deleting
            foreach (var id in ids)
            {
                var account = await _accountRepository.GetByIdAsync(id);
                if (account != null && account.ParentAccountId.HasValue)
                {
                    await _accountRepository.DecrementChildCountAsync(account.ParentAccountId.Value);
                    await _accountRepository.DecrementAncestorDescendantCountsAsync(id, account.DescendantCount);
                }
            }

            return await _accountRepository.DeleteByIdsAsync(ids);
        }

        #endregion

        #region Account Query Operations

        public List<AccountTreeNodeDto> GetAccountTree()
        {
            var accounts = _accountRepository.GetActiveAccounts();
            return BuildTree(accounts, null);
        }

        public async Task<List<AccountTreeNodeDto>> GetAccountTreeAsync()
        {
            var accounts = await _accountRepository.GetActiveAccountsAsync();
            return BuildTree(accounts, null);
        }

        private List<AccountTreeNodeDto> BuildTree(List<GLAccount> accounts, Guid? parentId)
        {
            return accounts
                .Where(a => a.ParentAccountId == parentId)
                .OrderBy(a => a.DisplayOrder)
                .ThenBy(a => a.AccountNumber)
                .Select(a => new AccountTreeNodeDto
                {
                    AccountId = a.AccountId,
                    AccountNumber = a.AccountNumber,
                    FullAccountNumber = a.FullAccountNumber,
                    AccountNameAr = a.AccountNameAr,
                    AccountNameEn = a.AccountNameEn,
                    AccountType = a.AccountType,
                    Level = a.Level,
                    ParentAccountId = a.ParentAccountId,
                    IsLeaf = a.IsLeaf,
                    IsActive = a.IsActive,
                    HasChildren = !a.IsLeaf,
                    Children = BuildTree(accounts, a.AccountId)
                })
                .ToList();
        }

        public List<AccountResponseDto> GetByType(AccountType accountType)
        {
            var accounts = _accountRepository.GetByType(accountType);
            return accounts.Select(MapToResponseDto).ToList();
        }

        public async Task<List<AccountResponseDto>> GetByTypeAsync(AccountType accountType)
        {
            var accounts = await _accountRepository.GetByTypeAsync(accountType);
            return accounts.Select(MapToResponseDto).ToList();
        }

        public List<AccountResponseDto> GetLeafAccounts()
        {
            var accounts = _accountRepository.GetLeafAccounts();
            return accounts.Select(MapToResponseDto).ToList();
        }

        public async Task<List<AccountResponseDto>> GetLeafAccountsAsync()
        {
            var accounts = await _accountRepository.GetLeafAccountsAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        public List<AccountResponseDto> GetChildAccounts(Guid parentAccountId)
        {
            var accounts = _accountRepository.GetChildAccounts(parentAccountId);
            return accounts.Select(MapToResponseDto).ToList();
        }

        public async Task<List<AccountResponseDto>> GetChildAccountsAsync(Guid parentAccountId)
        {
            var accounts = await _accountRepository.GetChildAccountsAsync(parentAccountId);
            return accounts.Select(MapToResponseDto).ToList();
        }

        public List<AccountResponseDto> GetCashAccounts()
        {
            var accounts = _accountRepository.GetCashAccounts();
            return accounts.Select(MapToResponseDto).ToList();
        }

        public List<AccountResponseDto> GetBankAccounts()
        {
            var accounts = _accountRepository.GetBankAccounts();
            return accounts.Select(MapToResponseDto).ToList();
        }

        public List<AccountResponseDto> Search(string searchTerm, int maxResults = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AccountResponseDto>();

            var accounts = Context.Queryable<GLAccount>()
                .Where(a => a.IsActive &&
                    (a.AccountNumber.Contains(searchTerm) ||
                     a.AccountNameAr.Contains(searchTerm) ||
                     a.AccountNameEn.Contains(searchTerm)))
                .OrderBy(a => a.AccountNumber)
                .Take(maxResults)
                .ToList();

            return accounts.Select(MapToResponseDto).ToList();
        }

        public async Task<List<AccountResponseDto>> SearchAsync(string searchTerm, int maxResults = 20)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AccountResponseDto>();

            var accounts = await Context.Queryable<GLAccount>()
                .Where(a => a.IsActive &&
                    (a.AccountNumber.Contains(searchTerm) ||
                     a.AccountNameAr.Contains(searchTerm) ||
                     a.AccountNameEn.Contains(searchTerm)))
                .OrderBy(a => a.AccountNumber)
                .Take(maxResults)
                .ToListAsync();

            return accounts.Select(MapToResponseDto).ToList();
        }

        #endregion

        #region Account Balance Operations

        public AccountBalanceDto GetAccountBalance(Guid accountId, int fiscalYear, DateTime? asOfDate = null)
        {
            var account = _accountRepository.GetById(accountId);
            if (account == null) return null;

            // Get opening balance for fiscal year
            var openingBalance = _openingBalanceRepository.GetByAccountAndYear(accountId, fiscalYear);

            // Get period movements from voucher entries
            var periodStart = _fiscalYearHelper.GetFiscalYearStartDate(fiscalYear, account.CompanyID);
            var periodEnd = asOfDate ?? DateTime.Now;

            var balance = _journalEntryRepository.GetAccountBalance(accountId, periodStart, periodEnd);

            return new AccountBalanceDto
            {
                FiscalYear = fiscalYear,
                OpeningDebit = openingBalance?.OpeningDebit ?? 0,
                OpeningCredit = openingBalance?.OpeningCredit ?? 0,
                OpeningBalance = openingBalance?.OpeningBalance ?? 0,
                PeriodDebit = balance.TotalDebit,
                PeriodCredit = balance.TotalCredit,
                CurrentBalance = (openingBalance?.OpeningBalance ?? 0) + (balance.TotalDebit - balance.TotalCredit),
                AsOfDate = periodEnd
            };
        }

        public async Task<AccountBalanceDto> GetAccountBalanceAsync(Guid accountId, int fiscalYear, DateTime? asOfDate = null)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) return null;

            var openingBalance = await _openingBalanceRepository.GetByAccountAndYearAsync(accountId, fiscalYear);

            var periodStart = _fiscalYearHelper.GetFiscalYearStartDate(fiscalYear, account.CompanyID);
            var periodEnd = asOfDate ?? DateTime.Now;

            var balance = await _journalEntryRepository.GetAccountBalanceAsync(accountId, periodStart, periodEnd);

            return new AccountBalanceDto
            {
                FiscalYear = fiscalYear,
                OpeningDebit = openingBalance?.OpeningDebit ?? 0,
                OpeningCredit = openingBalance?.OpeningCredit ?? 0,
                OpeningBalance = openingBalance?.OpeningBalance ?? 0,
                PeriodDebit = balance.TotalDebit,
                PeriodCredit = balance.TotalCredit,
                CurrentBalance = (openingBalance?.OpeningBalance ?? 0) + (balance.TotalDebit - balance.TotalCredit),
                AsOfDate = periodEnd
            };
        }

        public AccountBalanceDto GetAggregatedBalance(Guid accountId, int fiscalYear, DateTime? asOfDate = null)
        {
            var result = new AccountBalanceDto { FiscalYear = fiscalYear, AsOfDate = asOfDate ?? DateTime.Now };

            // Get own balance
            var ownBalance = GetAccountBalance(accountId, fiscalYear, asOfDate);
            if (ownBalance != null)
            {
                result.OpeningDebit = ownBalance.OpeningDebit;
                result.OpeningCredit = ownBalance.OpeningCredit;
                result.OpeningBalance = ownBalance.OpeningBalance;
                result.PeriodDebit = ownBalance.PeriodDebit;
                result.PeriodCredit = ownBalance.PeriodCredit;
                result.CurrentBalance = ownBalance.CurrentBalance;
            }

            // Get descendants balances
            var descendants = _accountRepository.GetAllDescendants(accountId);
            foreach (var descendant in descendants)
            {
                var descBalance = GetAccountBalance(descendant.AccountId, fiscalYear, asOfDate);
                if (descBalance != null)
                {
                    result.OpeningDebit += descBalance.OpeningDebit;
                    result.OpeningCredit += descBalance.OpeningCredit;
                    result.OpeningBalance += descBalance.OpeningBalance;
                    result.PeriodDebit += descBalance.PeriodDebit;
                    result.PeriodCredit += descBalance.PeriodCredit;
                    result.CurrentBalance += descBalance.CurrentBalance;
                }
            }

            return result;
        }

        public async Task<AccountBalanceDto> GetAggregatedBalanceAsync(Guid accountId, int fiscalYear, DateTime? asOfDate = null)
        {
            var result = new AccountBalanceDto { FiscalYear = fiscalYear, AsOfDate = asOfDate ?? DateTime.Now };

            var ownBalance = await GetAccountBalanceAsync(accountId, fiscalYear, asOfDate);
            if (ownBalance != null)
            {
                result.OpeningDebit = ownBalance.OpeningDebit;
                result.OpeningCredit = ownBalance.OpeningCredit;
                result.OpeningBalance = ownBalance.OpeningBalance;
                result.PeriodDebit = ownBalance.PeriodDebit;
                result.PeriodCredit = ownBalance.PeriodCredit;
                result.CurrentBalance = ownBalance.CurrentBalance;
            }

            var descendants = await _accountRepository.GetAllDescendantsAsync(accountId);
            foreach (var descendant in descendants)
            {
                var descBalance = await GetAccountBalanceAsync(descendant.AccountId, fiscalYear, asOfDate);
                if (descBalance != null)
                {
                    result.OpeningDebit += descBalance.OpeningDebit;
                    result.OpeningCredit += descBalance.OpeningCredit;
                    result.OpeningBalance += descBalance.OpeningBalance;
                    result.PeriodDebit += descBalance.PeriodDebit;
                    result.PeriodCredit += descBalance.PeriodCredit;
                    result.CurrentBalance += descBalance.CurrentBalance;
                }
            }

            return result;
        }

        public BalanceSummaryResponseDto GetBalanceSummary(BalanceSummaryQueryDto query)
        {
            if (!query.AccountId.HasValue)
                return null;

            var account = _accountRepository.GetById(query.AccountId.Value);
            if (account == null) return null;

            var balance = query.IncludeSubAccounts
                ? GetAggregatedBalance(query.AccountId.Value, query.FiscalYear ?? DateTime.Now.Year, query.AsOfDate)
                : GetAccountBalance(query.AccountId.Value, query.FiscalYear ?? DateTime.Now.Year, query.AsOfDate);

            if (balance == null) return null;

            return new BalanceSummaryResponseDto
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                AccountNameAr = account.AccountNameAr,
                AccountNameEn = account.AccountNameEn,
                AccountType = account.AccountType,
                NormalBalance = account.NormalBalance,
                FiscalYear = balance.FiscalYear,
                FiscalPeriod = query.FiscalPeriod ?? 0,
                OpeningDebit = balance.OpeningDebit,
                OpeningCredit = balance.OpeningCredit,
                OpeningBalance = balance.OpeningBalance,
                PeriodDebit = balance.PeriodDebit,
                PeriodCredit = balance.PeriodCredit,
                PeriodNet = balance.PeriodDebit - balance.PeriodCredit,
                ClosingDebit = balance.OpeningDebit + balance.PeriodDebit,
                ClosingCredit = balance.OpeningCredit + balance.PeriodCredit,
                ClosingBalance = balance.CurrentBalance,
                YtdDebit = balance.OpeningDebit + balance.PeriodDebit,
                YtdCredit = balance.OpeningCredit + balance.PeriodCredit,
                YtdBalance = balance.CurrentBalance,
                LastTransactionDate = account.LastTransactionDate,
                LastCalculatedAt = DateTime.Now
            };
        }

        public async Task<BalanceSummaryResponseDto> GetBalanceSummaryAsync(BalanceSummaryQueryDto query)
        {
            if (!query.AccountId.HasValue)
                return null;

            var account = await _accountRepository.GetByIdAsync(query.AccountId.Value);
            if (account == null) return null;

            var balance = query.IncludeSubAccounts
                ? await GetAggregatedBalanceAsync(query.AccountId.Value, query.FiscalYear ?? DateTime.Now.Year, query.AsOfDate)
                : await GetAccountBalanceAsync(query.AccountId.Value, query.FiscalYear ?? DateTime.Now.Year, query.AsOfDate);

            if (balance == null) return null;

            return new BalanceSummaryResponseDto
            {
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber,
                AccountNameAr = account.AccountNameAr,
                AccountNameEn = account.AccountNameEn,
                AccountType = account.AccountType,
                NormalBalance = account.NormalBalance,
                FiscalYear = balance.FiscalYear,
                FiscalPeriod = query.FiscalPeriod ?? 0,
                OpeningDebit = balance.OpeningDebit,
                OpeningCredit = balance.OpeningCredit,
                OpeningBalance = balance.OpeningBalance,
                PeriodDebit = balance.PeriodDebit,
                PeriodCredit = balance.PeriodCredit,
                PeriodNet = balance.PeriodDebit - balance.PeriodCredit,
                ClosingDebit = balance.OpeningDebit + balance.PeriodDebit,
                ClosingCredit = balance.OpeningCredit + balance.PeriodCredit,
                ClosingBalance = balance.CurrentBalance,
                YtdDebit = balance.OpeningDebit + balance.PeriodDebit,
                YtdCredit = balance.OpeningCredit + balance.PeriodCredit,
                YtdBalance = balance.CurrentBalance,
                LastTransactionDate = account.LastTransactionDate,
                LastCalculatedAt = DateTime.Now
            };
        }

        public async Task RecalculateBalanceAsync(Guid accountId, int fiscalYear, int fiscalPeriod)
        {
            // Placeholder for balance recalculation logic
            await Task.CompletedTask;
        }

        #endregion

        #region Opening Balance Operations

        public PagedInfo<OpeningBalanceResponseDto> GetOpeningBalances(OpeningBalanceQueryDto query, PagerInfo pager)
        {
            var searchModel = new GLAccountOpeningBalance
            {
                AccountId = query.AccountId ?? Guid.Empty,
                FiscalYear = query.FiscalYear ?? 0
            };

            var result = _openingBalanceRepository.GetList(searchModel, pager);
            var dtoList = result.Result.Select(MapToOpeningBalanceDto).ToList();

            return new PagedInfo<OpeningBalanceResponseDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        public OpeningBalanceResponseDto GetOpeningBalanceById(Guid id)
        {
            var balance = _openingBalanceRepository.GetById(id);
            return balance == null ? null : MapToOpeningBalanceDto(balance);
        }

        public async Task<OpeningBalanceResponseDto> GetOpeningBalanceByIdAsync(Guid id)
        {
            var balance = await _openingBalanceRepository.GetByIdAsync(id);
            return balance == null ? null : MapToOpeningBalanceDto(balance);
        }

        public GLAccountOpeningBalance CreateOpeningBalance(CreateOpeningBalanceDto dto, string userName)
        {
            var account = _accountRepository.GetById(dto.AccountId);
            if (account == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND, new Dictionary<string, object> { { "AccountId", dto.AccountId } });
            }

            if (_openingBalanceRepository.Exists(dto.AccountId, dto.FiscalYear))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_EXISTS, new Dictionary<string, object> { { "AccountId", dto.AccountId }, { "FiscalYear", dto.FiscalYear } });
            }

            var model = dto.Adapt<GLAccountOpeningBalance>();
            model.Id = Guid.NewGuid();
            model.OpeningBalance = dto.OpeningDebit - dto.OpeningCredit;
            model.BaseOpeningDebit = dto.OpeningDebit * dto.ExchangeRate;
            model.BaseOpeningCredit = dto.OpeningCredit * dto.ExchangeRate;
            model.SourceType = "Manual";
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            _openingBalanceRepository.InsertOpeningBalance(model);
            return model;
        }

        public async Task<GLAccountOpeningBalance> CreateOpeningBalanceAsync(CreateOpeningBalanceDto dto, string userName)
        {
            var account = await _accountRepository.GetByIdAsync(dto.AccountId);
            if (account == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND, new Dictionary<string, object> { { "AccountId", dto.AccountId } });
            }

            if (await _openingBalanceRepository.ExistsAsync(dto.AccountId, dto.FiscalYear))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_EXISTS, new Dictionary<string, object> { { "AccountId", dto.AccountId }, { "FiscalYear", dto.FiscalYear } });
            }

            var model = dto.Adapt<GLAccountOpeningBalance>();
            model.Id = Guid.NewGuid();
            model.OpeningBalance = dto.OpeningDebit - dto.OpeningCredit;
            model.BaseOpeningDebit = dto.OpeningDebit * dto.ExchangeRate;
            model.BaseOpeningCredit = dto.OpeningCredit * dto.ExchangeRate;
            model.SourceType = "Manual";
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            await _openingBalanceRepository.InsertOpeningBalanceAsync(model);
            return model;
        }

        public int UpdateOpeningBalance(UpdateOpeningBalanceDto dto, string userName)
        {
            var existing = _openingBalanceRepository.GetById(dto.Id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND, new Dictionary<string, object> { { "Id", dto.Id } });
            }

            if (existing.IsLocked)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_LOCKED, new Dictionary<string, object> { { "Id", dto.Id } });
            }

            var model = dto.Adapt<GLAccountOpeningBalance>();
            model.OpeningBalance = dto.OpeningDebit - dto.OpeningCredit;
            model.BaseOpeningDebit = dto.OpeningDebit * dto.ExchangeRate;
            model.BaseOpeningCredit = dto.OpeningCredit * dto.ExchangeRate;
            model.Update_by = userName;
            model.Update_time = DateTime.Now;

            return _openingBalanceRepository.UpdateOpeningBalance(model);
        }

        public async Task<int> UpdateOpeningBalanceAsync(UpdateOpeningBalanceDto dto, string userName)
        {
            var existing = await _openingBalanceRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND, new Dictionary<string, object> { { "Id", dto.Id } });
            }

            if (existing.IsLocked)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_LOCKED, new Dictionary<string, object> { { "Id", dto.Id } });
            }

            var model = dto.Adapt<GLAccountOpeningBalance>();
            model.OpeningBalance = dto.OpeningDebit - dto.OpeningCredit;
            model.BaseOpeningDebit = dto.OpeningDebit * dto.ExchangeRate;
            model.BaseOpeningCredit = dto.OpeningCredit * dto.ExchangeRate;
            model.Update_by = userName;
            model.Update_time = DateTime.Now;

            return await _openingBalanceRepository.UpdateOpeningBalanceAsync(model);
        }

        public int DeleteOpeningBalance(Guid id)
        {
            var existing = _openingBalanceRepository.GetById(id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND, new Dictionary<string, object> { { "Id", id } });
            }

            if (existing.IsLocked)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_LOCKED, new Dictionary<string, object> { { "Id", id } });
            }

            return _openingBalanceRepository.DeleteById(id);
        }

        public async Task<int> DeleteOpeningBalanceAsync(Guid id)
        {
            var existing = await _openingBalanceRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND, new Dictionary<string, object> { { "Id", id } });
            }

            if (existing.IsLocked)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_LOCKED, new Dictionary<string, object> { { "Id", id } });
            }

            return await _openingBalanceRepository.DeleteByIdAsync(id);
        }

        public async Task<int> ImportOpeningBalancesAsync(BulkOpeningBalanceDto dto, string userName)
        {
            var balancesToInsert = new List<GLAccountOpeningBalance>();

            foreach (var line in dto.Balances)
            {
                var account = await _accountRepository.GetByAccountNumberAsync(line.AccountNumber);
                if (account == null)
                {
                    throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND, new Dictionary<string, object> { { "AccountNumber", line.AccountNumber } });
                }

                if (await _openingBalanceRepository.ExistsAsync(account.AccountId, dto.FiscalYear))
                    continue;

                balancesToInsert.Add(new GLAccountOpeningBalance
                {
                    Id = Guid.NewGuid(),
                    AccountId = account.AccountId,
                    FiscalYear = dto.FiscalYear,
                    PeriodStartDate = dto.PeriodStartDate,
                    PeriodEndDate = dto.PeriodEndDate,
                    OpeningDebit = line.OpeningDebit,
                    OpeningCredit = line.OpeningCredit,
                    OpeningBalance = line.OpeningDebit - line.OpeningCredit,
                    Currency = line.Currency,
                    BaseOpeningDebit = line.OpeningDebit,
                    BaseOpeningCredit = line.OpeningCredit,
                    ExchangeRate = 1,
                    SourceType = "Migration",
                    Create_by = userName,
                    Create_time = DateTime.Now
                });
            }

            if (balancesToInsert.Any())
            {
                return await _openingBalanceRepository.InsertBulkAsync(balancesToInsert);
            }

            return 0;
        }

        public async Task<int> LockOpeningBalancesAsync(int fiscalYear, Guid lockedBy)
        {
            return await _openingBalanceRepository.LockByFiscalYearAsync(fiscalYear, lockedBy);
        }

        public async Task<int> CarryforwardOpeningBalancesAsync(int newFiscalYear, int previousFiscalYear, string createdBy)
        {
            return await _openingBalanceRepository.CreateFromPreviousYearClosingAsync(newFiscalYear, previousFiscalYear, null, createdBy);
        }

        #endregion

        #region Validation Operations

        public AccountValidationResultDto ValidateForPosting(Guid accountId, bool isManualEntry = true)
        {
            var result = new AccountValidationResultDto { IsValid = true };

            var account = _accountRepository.GetById(accountId);
            if (account == null)
            {
                result.AddError("الحساب غير موجود - Account not found");
                return result;
            }

            if (!account.IsActive)
            {
                result.AddError("الحساب غير نشط - Account is not active");
            }

            if (!account.IsLeaf && !account.AllowPostingToParent)
            {
                result.AddError("لا يمكن الترحيل للحسابات الأب - Cannot post to parent accounts");
            }

            if (isManualEntry && !account.AllowManualEntries)
            {
                result.AddError("الحساب لا يسمح بالقيود اليدوية - Account does not allow manual entries");
            }

            // Control accounts cannot accept manual postings
            if (isManualEntry && account.IsControlAccount)
            {
                result.AddError(
                    $"الحساب {account.FullAccountNumber} هو حساب مراقبة ولا يقبل قيود يدوية. " +
                    $"يجب استخدام الوحدات المخصصة (الذمم المدينة/الدائنة/المخزون) للترحيل لهذا الحساب. - " +
                    $"Account {account.FullAccountNumber} is a control account and does not accept manual entries. " +
                    $"Use designated modules (AR/AP/Inventory) to post to this account.");
            }

            // System accounts cannot accept manual postings
            if (isManualEntry && account.IsSystemAccount)
            {
                result.AddError(
                    $"الحساب {account.FullAccountNumber} هو حساب نظام محجوز ولا يقبل قيود يدوية. " +
                    $"حسابات النظام مخصصة لعمليات النظام الداخلية فقط. - " +
                    $"Account {account.FullAccountNumber} is a reserved system account and does not accept manual entries. " +
                    $"System accounts are designated for internal system operations only.");
            }

            return result;
        }

        public async Task<AccountValidationResultDto> ValidateForPostingAsync(Guid accountId, bool isManualEntry = true)
        {
            var result = new AccountValidationResultDto { IsValid = true };

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                result.AddError("الحساب غير موجود - Account not found");
                return result;
            }

            if (!account.IsActive)
            {
                result.AddError("الحساب غير نشط - Account is not active");
            }

            if (!account.IsLeaf && !account.AllowPostingToParent)
            {
                result.AddError("لا يمكن الترحيل للحسابات الأب - Cannot post to parent accounts");
            }

            if (isManualEntry && !account.AllowManualEntries)
            {
                result.AddError("الحساب لا يسمح بالقيود اليدوية - Account does not allow manual entries");
            }

            // Control accounts cannot accept manual postings
            if (isManualEntry && account.IsControlAccount)
            {
                result.AddError(
                    $"الحساب {account.FullAccountNumber} هو حساب مراقبة ولا يقبل قيود يدوية. " +
                    $"يجب استخدام الوحدات المخصصة (الذمم المدينة/الدائنة/المخزون) للترحيل لهذا الحساب. - " +
                    $"Account {account.FullAccountNumber} is a control account and does not accept manual entries. " +
                    $"Use designated modules (AR/AP/Inventory) to post to this account.");
            }

            // System accounts cannot accept manual postings
            if (isManualEntry && account.IsSystemAccount)
            {
                result.AddError(
                    $"الحساب {account.FullAccountNumber} هو حساب نظام محجوز ولا يقبل قيود يدوية. " +
                    $"حسابات النظام مخصصة لعمليات النظام الداخلية فقط. - " +
                    $"Account {account.FullAccountNumber} is a reserved system account and does not accept manual entries. " +
                    $"System accounts are designated for internal system operations only.");
            }

            return result;
        }

        public bool IsAccountNumberUnique(string accountNumber, Guid? excludeAccountId = null)
        {
            return !_accountRepository.CodeExists(accountNumber, excludeAccountId);
        }

        public async Task<bool> IsAccountNumberUniqueAsync(string accountNumber, Guid? excludeAccountId = null)
        {
            return !(await _accountRepository.CodeExistsAsync(accountNumber, excludeAccountId));
        }

        public bool ValidateHierarchy(Guid accountId, Guid? newParentId)
        {
            return !_accountRepository.WouldCreateCircularReference(accountId, newParentId);
        }

        public async Task<bool> ValidateHierarchyAsync(Guid accountId, Guid? newParentId)
        {
            return !(await _accountRepository.WouldCreateCircularReferenceAsync(accountId, newParentId));
        }

        public AccountValidationResultDto ValidateForDeletion(Guid accountId)
        {
            var result = new AccountValidationResultDto { IsValid = true };

            var account = _accountRepository.GetById(accountId);
            if (account == null)
            {
                result.AddError("الحساب غير موجود - Account not found");
                return result;
            }

            if (account.IsSystemAccount)
            {
                result.AddError(
                    $"لا يمكن حذف حساب النظام {account.FullAccountNumber}. " +
                    $"حسابات النظام محمية ولا يمكن حذفها لأنها ضرورية لعمليات النظام الداخلية. - " +
                    $"Cannot delete system account {account.FullAccountNumber}. " +
                    $"System accounts are protected and cannot be deleted as they are essential for internal system operations.");
            }

            if (_accountRepository.HasChildren(accountId))
            {
                result.AddError("لا يمكن حذف حساب له حسابات فرعية - Cannot delete account with children");
            }

            if (_accountRepository.HasTransactions(accountId))
            {
                result.AddError("لا يمكن حذف حساب له حركات - Cannot delete account with transactions");
            }

            return result;
        }

        public async Task<AccountValidationResultDto> ValidateForDeletionAsync(Guid accountId)
        {
            var result = new AccountValidationResultDto { IsValid = true };

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                result.AddError("الحساب غير موجود - Account not found");
                return result;
            }

            if (account.IsSystemAccount)
            {
                result.AddError(
                    $"لا يمكن حذف حساب النظام {account.FullAccountNumber}. " +
                    $"حسابات النظام محمية ولا يمكن حذفها لأنها ضرورية لعمليات النظام الداخلية. - " +
                    $"Cannot delete system account {account.FullAccountNumber}. " +
                    $"System accounts are protected and cannot be deleted as they are essential for internal system operations.");
            }

            if (await _accountRepository.HasChildrenAsync(accountId))
            {
                result.AddError("لا يمكن حذف حساب له حسابات فرعية - Cannot delete account with children");
            }

            if (await _accountRepository.HasTransactionsAsync(accountId))
            {
                result.AddError("لا يمكن حذف حساب له حركات - Cannot delete account with transactions");
            }

            return result;
        }

        public bool ValidateOpeningBalanceTotals(int fiscalYear)
        {
            var totals = _openingBalanceRepository.GetTotalsByFiscalYear(fiscalYear);
            return totals.TotalDebit == totals.TotalCredit;
        }

        public async Task<bool> ValidateOpeningBalanceTotalsAsync(int fiscalYear)
        {
            var totals = await _openingBalanceRepository.GetTotalsByFiscalYearAsync(fiscalYear);
            return totals.TotalDebit == totals.TotalCredit;
        }

        #endregion

        #region Hierarchy Operations

        public List<AccountResponseDto> GetHierarchyPath(Guid accountId)
        {
            var path = _accountRepository.GetHierarchyPath(accountId);
            return path.Select(MapToResponseDto).ToList();
        }

        public async Task RecalculateAllPathsAsync()
        {
            await _accountRepository.RecalculateAllPathsAsync();
        }

        public NormalBalance GetDefaultNormalBalance(AccountType accountType)
        {
            return accountType switch
            {
                AccountType.Asset => NormalBalance.Debit,
                AccountType.Expense => NormalBalance.Debit,
                AccountType.ContraLiability => NormalBalance.Debit,
                AccountType.ContraEquity => NormalBalance.Debit,
                AccountType.ContraRevenue => NormalBalance.Debit,
                AccountType.Liability => NormalBalance.Credit,
                AccountType.Equity => NormalBalance.Credit,
                AccountType.Revenue => NormalBalance.Credit,
                AccountType.ContraAsset => NormalBalance.Credit,
                AccountType.OtherComprehensiveIncome => NormalBalance.Credit,
                _ => NormalBalance.Debit
            };
        }

        #endregion

        #region Legacy Compatibility

        public bool CodeExists(string accountNumber, Guid? excludeAccountId = null)
        {
            return _accountRepository.CodeExists(accountNumber, excludeAccountId);
        }

        #endregion

        #region Private Helper Methods

        private AccountResponseDto MapToResponseDto(GLAccount account)
        {
            var dto = account.Adapt<AccountResponseDto>();

            if (account.ParentAccountId.HasValue)
            {
                var parent = _accountRepository.GetById(account.ParentAccountId.Value);
                dto.ParentName = parent?.AccountNameEn;
            }

            return dto;
        }

        private OpeningBalanceResponseDto MapToOpeningBalanceDto(GLAccountOpeningBalance balance)
        {
            var dto = balance.Adapt<OpeningBalanceResponseDto>();

            var account = _accountRepository.GetById(balance.AccountId);
            if (account != null)
            {
                dto.AccountNumber = account.AccountNumber;
                dto.AccountNameAr = account.AccountNameAr;
                dto.AccountNameEn = account.AccountNameEn;
            }

            return dto;
        }

        /// <summary>
        /// Validate Cash Flow Classification to prevent conflicting flags
        /// Returns a list of warnings (non-blocking) for IAS 7 compliance recommendations
        /// </summary>
        private List<string> ValidateCashFlowClassification(GLAccountFlags flags, WorkingCapitalType? workingCapitalType, ZR.Model.Finance.Enums.CashFlowCategory cashFlowCategory)
        {
            var warnings = new List<string>();

            // Rule 1: WorkingCapital flag requires WorkingCapitalType
            if (flags.HasFlag(GLAccountFlags.WorkingCapital) && !workingCapitalType.HasValue)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "WorkingCapitalTypeRequired" } });
            }

            // Rule 2: WorkingCapitalType without WorkingCapital flag is invalid
            if (workingCapitalType.HasValue && !flags.HasFlag(GLAccountFlags.WorkingCapital))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "WorkingCapitalFlagRequired" } });
            }

            // Rule 3: Depreciation and WorkingCapital are mutually exclusive
            if (flags.HasFlag(GLAccountFlags.Depreciation) && flags.HasFlag(GLAccountFlags.WorkingCapital))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "DepreciationAndWorkingCapitalMutuallyExclusive" } });
            }

            // Rule 4: Amortization and WorkingCapital are mutually exclusive
            if (flags.HasFlag(GLAccountFlags.Amortization) && flags.HasFlag(GLAccountFlags.WorkingCapital))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "AmortizationAndWorkingCapitalMutuallyExclusive" } });
            }

            // Rule 5: FixedAsset and WorkingCapital are mutually exclusive
            if (flags.HasFlag(GLAccountFlags.FixedAsset) && flags.HasFlag(GLAccountFlags.WorkingCapital))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "FixedAssetAndWorkingCapitalMutuallyExclusive" } });
            }

            // Rule 6: If FixedAsset is set, CashFlowCategory should be Investing
            if (flags.HasFlag(GLAccountFlags.FixedAsset) && cashFlowCategory != ZR.Model.Finance.Enums.CashFlowCategory.None && cashFlowCategory != ZR.Model.Finance.Enums.CashFlowCategory.Investing)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "FixedAssetRequiresInvestingCategory" } });
            }

            // Rule 7: If Loan is set, CashFlowCategory should be Financing or Investing
            if (flags.HasFlag(GLAccountFlags.Loan) && cashFlowCategory != ZR.Model.Finance.Enums.CashFlowCategory.None &&
                cashFlowCategory != ZR.Model.Finance.Enums.CashFlowCategory.Financing && cashFlowCategory != ZR.Model.Finance.Enums.CashFlowCategory.Investing)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "LoanRequiresFinancingOrInvestingCategory" } });
            }

            // Rule 8 (Warning): Interest flag - IAS 7.33 allows flexibility
            if (flags.HasFlag(GLAccountFlags.Interest) && cashFlowCategory == ZR.Model.Finance.Enums.CashFlowCategory.None)
            {
                warnings.Add(
                    "تحذير: حساب الفوائد لا يحتوي على تصنيف تدفق نقدي. " +
                    "وفقاً لـ IAS 7.33، يمكن تصنيف الفوائد كتشغيلي (الأكثر شيوعاً)، استثماري، أو تمويلي. " +
                    "يرجى تحديد فئة التدفق النقدي المناسبة. - " +
                    "Warning: Interest account has no Cash Flow Category. " +
                    "Per IAS 7.33, interest can be classified as Operating (most common), Investing, or Financing. " +
                    "Please set an appropriate Cash Flow Category.");
            }

            // Rule 9 (Warning): Dividend flag - IAS 7.33-34 allows flexibility
            if (flags.HasFlag(GLAccountFlags.Dividend) && cashFlowCategory == ZR.Model.Finance.Enums.CashFlowCategory.None)
            {
                warnings.Add(
                    "تحذير: حساب الأرباح الموزعة لا يحتوي على تصنيف تدفق نقدي. " +
                    "وفقاً لـ IAS 7.33-34، يمكن تصنيف الأرباح الموزعة المدفوعة كتمويلي، " +
                    "والأرباح الموزعة المستلمة كتشغيلي أو استثماري. " +
                    "يرجى تحديد فئة التدفق النقدي المناسبة. - " +
                    "Warning: Dividend account has no Cash Flow Category. " +
                    "Per IAS 7.33-34, dividends paid are typically Financing, " +
                    "dividends received can be Operating or Investing. " +
                    "Please set an appropriate Cash Flow Category.");
            }

            // Rule 10: IntangibleAsset and WorkingCapital are mutually exclusive
            if (flags.HasFlag(GLAccountFlags.IntangibleAsset) && flags.HasFlag(GLAccountFlags.WorkingCapital))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "IntangibleAssetAndWorkingCapitalMutuallyExclusive" } });
            }

            // Rule 11: Investment and WorkingCapital are mutually exclusive
            if (flags.HasFlag(GLAccountFlags.Investment) && flags.HasFlag(GLAccountFlags.WorkingCapital))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "InvestmentAndWorkingCapitalMutuallyExclusive" } });
            }

            // Rule 12: If IntangibleAsset is set, CashFlowCategory should be Investing
            if (flags.HasFlag(GLAccountFlags.IntangibleAsset) && cashFlowCategory != ZR.Model.Finance.Enums.CashFlowCategory.None && cashFlowCategory != ZR.Model.Finance.Enums.CashFlowCategory.Investing)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "IntangibleAssetRequiresInvestingCategory" } });
            }

            // Rule 13: If Investment is set, CashFlowCategory should be Investing
            if (flags.HasFlag(GLAccountFlags.Investment) && cashFlowCategory != ZR.Model.Finance.Enums.CashFlowCategory.None && cashFlowCategory != ZR.Model.Finance.Enums.CashFlowCategory.Investing)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "InvestmentRequiresInvestingCategory" } });
            }

            // Rule 14: Impairment and WorkingCapital are mutually exclusive
            if (flags.HasFlag(GLAccountFlags.Impairment) && flags.HasFlag(GLAccountFlags.WorkingCapital))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "ImpairmentAndWorkingCapitalMutuallyExclusive" } });
            }

            // Rule 15: ForeignExchange and FixedAsset are mutually exclusive
            if (flags.HasFlag(GLAccountFlags.ForeignExchange) && flags.HasFlag(GLAccountFlags.FixedAsset))
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "ForeignExchangeAndFixedAssetMutuallyExclusive" } });
            }

            // Rule 16: Only one balance sheet classification allowed (FixedAsset, IntangibleAsset, or Investment)
            if (flags.CountBalanceSheetClassifications() > 1)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_FLAG_VALIDATION_FAILED, new Dictionary<string, object> { { "Rule", "OnlyOneBalanceSheetClassificationAllowed" } });
            }

            // Rule 17 (Warning): Tax flag - IAS 7.35 recommends Operating
            if (flags.HasFlag(GLAccountFlags.Tax) && cashFlowCategory == ZR.Model.Finance.Enums.CashFlowCategory.None)
            {
                warnings.Add(
                    "تحذير: حساب الضريبة لا يحتوي على تصنيف تدفق نقدي. " +
                    "وفقاً لـ IAS 7.35، يجب تصنيف الضرائب عادة كتشغيلي. " +
                    "يرجى تحديد فئة التدفق النقدي المناسبة. - " +
                    "Warning: Tax account has no Cash Flow Category. " +
                    "Per IAS 7.35, taxes should generally be classified as Operating. " +
                    "Please set an appropriate Cash Flow Category.");
            }

            return warnings;
        }

        /// <summary>
        /// Get validation warnings for Cash Flow Classification
        /// Public method for API to retrieve warnings without blocking
        /// </summary>
        public List<string> GetCashFlowValidationWarnings(GLAccountFlags flags, WorkingCapitalType? workingCapitalType, ZR.Model.Finance.Enums.CashFlowCategory cashFlowCategory)
        {
            try
            {
                return ValidateCashFlowClassification(flags, workingCapitalType, cashFlowCategory);
            }
            catch (AppException)
            {
                // If validation throws an error, return empty warnings (error will be handled separately)
                return new List<string>();
            }
        }

        #endregion

        #region Cash Flow Classification Query Methods

        /// <summary>
        /// Get accounts by Cash Flow Category
        /// </summary>
        public List<AccountResponseDto> GetByCashFlowCategory(ZR.Model.Finance.Enums.CashFlowCategory category)
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && a.CashFlowCategory == category)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts by Cash Flow Category async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetByCashFlowCategoryAsync(ZR.Model.Finance.Enums.CashFlowCategory category)
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && a.CashFlowCategory == category)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with specific flags (bitwise AND)
        /// </summary>
        public List<AccountResponseDto> GetByAccountFlags(GLAccountFlags flags)
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & flags) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with specific flags async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetByAccountFlagsAsync(GLAccountFlags flags)
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & flags) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get all depreciation and amortization accounts for Cash Flow
        /// </summary>
        public List<AccountResponseDto> GetDepreciationAmortizationAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive &&
                    ((a.AccountFlags & GLAccountFlags.Depreciation) != 0 ||
                     (a.AccountFlags & GLAccountFlags.Amortization) != 0))
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get all working capital accounts for Cash Flow
        /// </summary>
        public List<AccountResponseDto> GetWorkingCapitalAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.WorkingCapital) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get working capital accounts by type
        /// </summary>
        public List<AccountResponseDto> GetWorkingCapitalAccountsByType(WorkingCapitalType type)
        {
            var accounts = Queryable()
                .Where(a => a.IsActive &&
                    (a.AccountFlags & GLAccountFlags.WorkingCapital) != 0 &&
                    a.WorkingCapitalType == type)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get all non-cash adjustment accounts for Cash Flow Statement
        /// </summary>
        public List<AccountResponseDto> GetNonCashAdjustmentAccounts()
        {
            var nonCashFlags = GLAccountFlags.Depreciation | GLAccountFlags.Amortization |
                              GLAccountFlags.Provision | GLAccountFlags.Impairment |
                              GLAccountFlags.GainLossOnDisposal | GLAccountFlags.ForeignExchange;

            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & nonCashFlags) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Investment flag
        /// </summary>
        public List<AccountResponseDto> GetInvestmentAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Investment) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Investment flag async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetInvestmentAccountsAsync()
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Investment) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Loan flag
        /// </summary>
        public List<AccountResponseDto> GetLoanAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Loan) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Loan flag async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetLoanAccountsAsync()
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Loan) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Interest flag
        /// </summary>
        public List<AccountResponseDto> GetInterestAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Interest) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Interest flag async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetInterestAccountsAsync()
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Interest) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Tax flag
        /// </summary>
        public List<AccountResponseDto> GetTaxAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Tax) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Tax flag async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetTaxAccountsAsync()
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Tax) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Impairment flag
        /// </summary>
        public List<AccountResponseDto> GetImpairmentAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Impairment) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Impairment flag async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetImpairmentAccountsAsync()
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Impairment) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with ForeignExchange flag
        /// </summary>
        public List<AccountResponseDto> GetForeignExchangeAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.ForeignExchange) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with ForeignExchange flag async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetForeignExchangeAccountsAsync()
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.ForeignExchange) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Dividend flag
        /// </summary>
        public List<AccountResponseDto> GetDividendAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Dividend) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with Dividend flag async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetDividendAccountsAsync()
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.Dividend) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with GainLossOnDisposal flag
        /// </summary>
        public List<AccountResponseDto> GetGainLossOnDisposalAccounts()
        {
            var accounts = Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.GainLossOnDisposal) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToList();
            return accounts.Select(MapToResponseDto).ToList();
        }

        /// <summary>
        /// Get accounts with GainLossOnDisposal flag async
        /// </summary>
        public async Task<List<AccountResponseDto>> GetGainLossOnDisposalAccountsAsync()
        {
            var accounts = await Queryable()
                .Where(a => a.IsActive && (a.AccountFlags & GLAccountFlags.GainLossOnDisposal) != 0)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
            return accounts.Select(MapToResponseDto).ToList();
        }

        #endregion

        #region Excel Import Operations

        /// <summary>
        /// Import accounts from Excel file stream
        /// </summary>
        public async Task<ExcelImportResultDto> ImportFromExcelAsync(Stream fileStream, bool updateExisting, string userName)
        {
            var result = new ExcelImportResultDto();

            try
            {
                // Read Excel file using MiniExcel - read as dynamic to handle flexible column names
                var rows = fileStream.Query(useHeaderRow: true).ToList();
                result.TotalRows = rows.Count;

                if (rows.Count == 0)
                {
                    result.Errors.Add("Excel file is empty - الملف فارغ");
                    return result;
                }

                // First pass: collect all accounts and build a lookup by AccountNumber for parent resolution
                var accountsToProcess = new List<(int rowIndex, dynamic row, CreateAccountDto dto)>();
                var existingAccountsByNumber = await Context.Queryable<GLAccount>()
                    .Select(a => new { a.AccountId, a.AccountNumber, a.FullAccountNumber })
                    .ToListAsync();
                var existingLookup = existingAccountsByNumber.ToDictionary(a => a.AccountNumber, a => a);

                // Parse all rows first to build DTOs
                int rowIndex = 1;
                foreach (var row in rows)
                {
                    rowIndex++;

                    // Debug: Log column names from first row
                    if (rowIndex == 2)
                    {
                        var rowDict = row as IDictionary<string, object>;
                        if (rowDict != null)
                        {
                            var columnNames = string.Join(", ", rowDict.Keys);
                            result.Warnings.Add($"DEBUG: Excel columns found: [{columnNames}]");
                        }
                    }

                    try
                    {
                        var dto = ParseExcelRow(row, rowIndex, result);
                        if (dto != null)
                        {
                            accountsToProcess.Add((rowIndex, row, dto));
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {rowIndex}: {ex.Message}");
                        result.FailedCount++;
                    }
                }

                // Sort by Level to ensure parents are created before children
                accountsToProcess = accountsToProcess.OrderBy(x => x.dto.Level).ToList();

                // Track newly created accounts for parent resolution
                var newAccountsLookup = new Dictionary<string, Guid>();

                // Second pass: create/update accounts
                foreach (var (rIndex, row, dto) in accountsToProcess)
                {
                    try
                    {
                        // Resolve parent account if ParentAccountNumber is provided
                        string parentAccountNumber = GetDynamicValue(row, "Parent Account Number", "رقم الحساب الأب");
                        if (!string.IsNullOrWhiteSpace(parentAccountNumber))
                        {
                            // Check in existing accounts
                            if (existingLookup.TryGetValue(parentAccountNumber, out var existingParent))
                            {
                                dto.ParentAccountId = existingParent.AccountId;
                            }
                            // Check in newly created accounts
                            else if (newAccountsLookup.TryGetValue(parentAccountNumber, out var newParentId))
                            {
                                dto.ParentAccountId = newParentId;
                            }
                            else
                            {
                                result.Warnings.Add($"Row {rIndex}: Parent account '{parentAccountNumber}' not found, will be created as root - الحساب الأب غير موجود");
                            }
                        }

                        // Check if account already exists
                        var existingAccount = existingLookup.TryGetValue(dto.AccountNumber, out var existing) ? existing : null;

                        if (existingAccount != null)
                        {
                            if (updateExisting)
                            {
                                // Update existing account
                                var updateDto = new UpdateAccountDto
                                {
                                    AccountId = existingAccount.AccountId,
                                    AccountNumber = dto.AccountNumber,
                                    AccountNameAr = dto.AccountNameAr,
                                    AccountNameEn = dto.AccountNameEn,
                                    AccountType = dto.AccountType,
                                    NormalBalance = dto.NormalBalance,
                                    Level = dto.Level,
                                    ParentAccountId = dto.ParentAccountId,
                                    IsActive = dto.IsActive,
                                    IsLeaf = dto.IsLeaf,
                                    AllowManualEntries = dto.AllowManualEntries,
                                    AllowPostingToParent = dto.AllowPostingToParent,
                                    DimensionRequirements = dto.DimensionRequirements,
                                    AccountNature = dto.AccountNature,
                                    IsControlAccount = dto.IsControlAccount,
                                    ReconciliationMode = dto.ReconciliationMode,
                                    Currency = dto.Currency,
                                    DescriptionAr = dto.DescriptionAr,
                                    DescriptionEn = dto.DescriptionEn,
                                    ExternalCode = dto.ExternalCode,
                                    TaxCode = dto.TaxCode,
                                    CashFlowCategory = dto.CashFlowCategory,
                                    AccountFlags = dto.AccountFlags,
                                    WorkingCapitalType = dto.WorkingCapitalType,
                                    RowVersion = 1 // Will be fetched in Update method
                                };
                                await UpdateAsync(updateDto, userName);
                                result.UpdatedCount++;
                            }
                            else
                            {
                                result.Warnings.Add($"Row {rIndex}: Account '{dto.AccountNumber}' already exists, skipped - الحساب موجود مسبقاً");
                            }
                        }
                        else
                        {
                            // Create new account
                            var createdAccount = await CreateAsync(dto, userName);
                            newAccountsLookup[dto.AccountNumber] = createdAccount.AccountId;
                            result.SuccessCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {rIndex} ({dto.AccountNumber}): {ex.Message}");
                        result.FailedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to process Excel file: {ex.Message} - فشل معالجة الملف");
            }

            return result;
        }

        /// <summary>
        /// Parse a single Excel row into CreateAccountDto
        /// </summary>
        private CreateAccountDto ParseExcelRow(dynamic row, int rowIndex, ExcelImportResultDto result)
        {
            var accountNumber = GetDynamicValue(row, "Account Number", "رقم الحساب");
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                result.Warnings.Add($"Row {rowIndex}: Skipped - no account number");
                return null;
            }

            var accountNameEn = GetDynamicValue(row, "Account Name (EN)", "اسم الحساب (إنجليزي)");
            var accountNameAr = GetDynamicValue(row, "Account Name (AR)", "اسم الحساب (عربي)");

            if (string.IsNullOrWhiteSpace(accountNameEn) && string.IsNullOrWhiteSpace(accountNameAr))
            {
                result.Errors.Add($"Row {rowIndex}: Account name is required - اسم الحساب مطلوب");
                return null;
            }

            // Use one name for both if only one is provided
            if (string.IsNullOrWhiteSpace(accountNameEn)) accountNameEn = accountNameAr;
            if (string.IsNullOrWhiteSpace(accountNameAr)) accountNameAr = accountNameEn;

            // Debug: Log raw Account Type value for first 10 rows
            var rawAccountType = GetDynamicValue(row, "Account Type", "AccountType", "Type", "نوع الحساب", "نوع", "النوع", "الفئة");
            if (rowIndex <= 10)
            {
                result.Warnings.Add($"DEBUG Row {rowIndex} [{accountNumber}]: RawAccountType='{rawAccountType ?? "NULL"}' -> Parsed={ParseAccountType(rawAccountType)}");
            }

            var dto = new CreateAccountDto
            {
                AccountNumber = accountNumber,
                AccountNameEn = accountNameEn,
                AccountNameAr = accountNameAr,
                AccountType = ParseAccountType(rawAccountType),
                NormalBalance = ParseNormalBalance(GetDynamicValue(row, "Normal Balance", "NormalBalance", "Balance Type", "الرصيد الطبيعي", "نوع الرصيد")),
                Level = ParseInt(GetDynamicValue(row, "Level", "المستوى"), 1),
                Currency = GetDynamicValue(row, "Currency", "العملة") ?? "SAR",
                IsLeaf = ParseBool(GetDynamicValue(row, "Is Leaf", "حساب فرعي"), true),
                IsActive = ParseBool(GetDynamicValue(row, "Is Active", "نشط"), true),
                AllowManualEntries = ParseBool(GetDynamicValue(row, "Allow Manual Entries", "السماح بالقيود اليدوية"), true),
                AllowPostingToParent = ParseBool(GetDynamicValue(row, "Allow Posting To Parent", "السماح بالترحيل للأب"), false),
                IsControlAccount = ParseBool(GetDynamicValue(row, "Is Control Account", "حساب مراقبة"), false),
                IsForeignCurrency = ParseBool(GetDynamicValue(row, "Is Foreign Currency", "حساب عملة أجنبية"), false),
                ExternalCode = GetDynamicValue(row, "External Code", "الرمز الخارجي"),
                TaxCode = GetDynamicValue(row, "Tax Code", "رمز الضريبة"),
                DescriptionEn = GetDynamicValue(row, "Description (EN)", "الوصف (إنجليزي)"),
                DescriptionAr = GetDynamicValue(row, "Description (AR)", "الوصف (عربي)")
            };

            // Set default normal balance if not specified
            if (dto.NormalBalance == default)
            {
                dto.NormalBalance = GetDefaultNormalBalance(dto.AccountType);
            }

            // Parse AccountNature from Is Cash Account / Is Bank Account
            bool isCashAccount = ParseBool(GetDynamicValue(row, "Is Cash Account", "حساب نقدي"), false);
            bool isBankAccount = ParseBool(GetDynamicValue(row, "Is Bank Account", "حساب بنكي"), false);
            dto.AccountNature = isCashAccount ? AccountNature.Cash : (isBankAccount ? AccountNature.Bank : AccountNature.None);

            // Parse ReconciliationMode from Is Reconciliation Account
            bool isReconciliationAccount = ParseBool(GetDynamicValue(row, "Is Reconciliation Account", "حساب تسوية"), false);
            dto.ReconciliationMode = isReconciliationAccount ? ReconciliationMode.Required : ReconciliationMode.None;

            // Parse dimension requirements
            bool requiresCostCenter = ParseBool(GetDynamicValue(row, "Requires Cost Center", "يتطلب مركز تكلفة"), false);
            bool requiresDepartment = ParseBool(GetDynamicValue(row, "Requires Department", "يتطلب قسم"), false);
            bool requiresProject = ParseBool(GetDynamicValue(row, "Requires Project", "يتطلب مشروع"), false);
            if (requiresCostCenter) dto.DimensionRequirements |= DimensionRequirements.CostCenter;
            if (requiresDepartment) dto.DimensionRequirements |= DimensionRequirements.Department;
            if (requiresProject) dto.DimensionRequirements |= DimensionRequirements.Project;

            // Parse Cash Flow Category
            dto.CashFlowCategory = ParseCashFlowCategory(GetDynamicValue(row, "Cash Flow Category", "فئة التدفق النقدي"));

            // Parse Working Capital Type
            dto.WorkingCapitalType = ParseWorkingCapitalType(GetDynamicValue(row, "Working Capital Type", "نوع رأس المال العامل"));

            // Parse Account Flags
            dto.AccountFlags = ParseAccountFlags(GetDynamicValue(row, "Account Flags", "علامات الحساب"));

            return dto;
        }

        /// <summary>
        /// Get value from dynamic row with multiple possible column names
        /// </summary>
        private string GetDynamicValue(dynamic row, params string[] columnNames)
        {
            var rowDict = row as IDictionary<string, object>;
            if (rowDict == null) return null;

            foreach (var name in columnNames)
            {
                // Try exact match
                if (rowDict.TryGetValue(name, out var value) && value != null)
                {
                    return value.ToString().Trim();
                }

                // Try case-insensitive match
                var key = rowDict.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (key != null && rowDict.TryGetValue(key, out var val) && val != null)
                {
                    return val.ToString().Trim();
                }
            }

            return null;
        }

        private AccountType ParseAccountType(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return AccountType.Asset;

            var lower = value.ToLower().Trim();

            // For values like "Liability - التزام", get only the FIRST part before any separator
            var firstPart = lower.Split(new[] { '-', '–', '—', '/', '\\', '|', ':', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 .FirstOrDefault()?.Trim() ?? lower;

            // Check using StartsWith for the first part (more precise than Contains)
            // Contra accounts first (more specific)
            if (firstPart.StartsWith("contra asset") || firstPart.StartsWith("مقابل الأصول") || firstPart.StartsWith("مقابل الاصول")) return AccountType.ContraAsset;
            if (firstPart.StartsWith("contra liability") || firstPart.StartsWith("مقابل الالتزام") || firstPart.StartsWith("مقابل الخصوم")) return AccountType.ContraLiability;
            if (firstPart.StartsWith("contra equity") || firstPart.StartsWith("مقابل حقوق")) return AccountType.ContraEquity;
            if (firstPart.StartsWith("contra revenue") || firstPart.StartsWith("مقابل الإيراد") || firstPart.StartsWith("مقابل الايراد")) return AccountType.ContraRevenue;

            // Main account types - check first part with StartsWith
            if (firstPart.StartsWith("asset") || firstPart.StartsWith("أصول") || firstPart.StartsWith("اصول") || firstPart == "assets") return AccountType.Asset;
            if (firstPart.StartsWith("liabilit") || firstPart.StartsWith("التزام") || firstPart.StartsWith("الالتزام") || firstPart.StartsWith("خصوم")) return AccountType.Liability;
            if (firstPart.StartsWith("equity") || firstPart.StartsWith("حقوق") || firstPart.StartsWith("ملكية")) return AccountType.Equity;
            if (firstPart.StartsWith("revenue") || firstPart.StartsWith("إيراد") || firstPart.StartsWith("ايراد") || firstPart.StartsWith("الإيراد") || firstPart.StartsWith("الايراد")) return AccountType.Revenue;
            if (firstPart.StartsWith("expense") || firstPart.StartsWith("مصروف") || firstPart.StartsWith("مصاريف") || firstPart.StartsWith("المصروف") || firstPart.StartsWith("المصاريف")) return AccountType.Expense;
            if (firstPart.Contains("comprehensive") || firstPart.Contains("شامل")) return AccountType.OtherComprehensiveIncome;

            // Fallback: check full value with Contains (for edge cases)
            if (lower.Contains("liability") || lower.Contains("liabilities")) return AccountType.Liability;
            if (lower.Contains("equity")) return AccountType.Equity;
            if (lower.Contains("revenue")) return AccountType.Revenue;
            if (lower.Contains("expense")) return AccountType.Expense;

            // Handle numeric values - use 0-based enum values
            // 0=Asset, 1=Liability, 2=Equity, 3=Revenue, 4=Expense
            if (int.TryParse(value.Trim(), out int numericValue))
            {
                if (numericValue >= 0 && numericValue <= 10)
                {
                    return (AccountType)numericValue;
                }
            }

            // Try parsing as enum name
            if (Enum.TryParse<AccountType>(value, true, out var parsed)) return parsed;

            return AccountType.Asset;
        }

        private NormalBalance ParseNormalBalance(string value)
        {
            // Return default (0) when empty so GetDefaultNormalBalance kicks in based on AccountType
            if (string.IsNullOrWhiteSpace(value)) return default;

            var lower = value.ToLower();
            if (lower.Contains("debit") || lower.Contains("مدين")) return NormalBalance.Debit;
            if (lower.Contains("credit") || lower.Contains("دائن")) return NormalBalance.Credit;

            if (Enum.TryParse<NormalBalance>(value, true, out var parsed)) return parsed;

            // Return default (0) for unknown values so GetDefaultNormalBalance kicks in
            return default;
        }

        private FinanceEnums.CashFlowCategory ParseCashFlowCategory(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return FinanceEnums.CashFlowCategory.None;

            var lower = value.ToLower();
            if (lower.Contains("operating") || lower.Contains("تشغيل")) return FinanceEnums.CashFlowCategory.Operating;
            if (lower.Contains("investing") || lower.Contains("استثمار")) return FinanceEnums.CashFlowCategory.Investing;
            if (lower.Contains("financing") || lower.Contains("تمويل")) return FinanceEnums.CashFlowCategory.Financing;

            if (Enum.TryParse<FinanceEnums.CashFlowCategory>(value, true, out var parsed)) return parsed;

            return FinanceEnums.CashFlowCategory.None;
        }

        private WorkingCapitalType? ParseWorkingCapitalType(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var lower = value.ToLower();
            if (lower.Contains("receivable") || lower.Contains("ذمم مدينة")) return WorkingCapitalType.Receivables;
            if (lower.Contains("inventory") || lower.Contains("مخزون")) return WorkingCapitalType.Inventory;
            if (lower.Contains("prepaid") || lower.Contains("مقدم")) return WorkingCapitalType.Prepaid;
            if (lower.Contains("payable") || lower.Contains("موردين")) return WorkingCapitalType.Payables;
            if (lower.Contains("accrued") || lower.Contains("مستحق")) return WorkingCapitalType.Accrued;

            if (Enum.TryParse<WorkingCapitalType>(value, true, out var parsed)) return parsed;

            return null;
        }

        private GLAccountFlags ParseAccountFlags(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return GLAccountFlags.None;

            var lower = value.ToLower();
            var flags = GLAccountFlags.None;

            if (lower.Contains("depreciation") || lower.Contains("إهلاك")) flags |= GLAccountFlags.Depreciation;
            if (lower.Contains("amortization") || lower.Contains("إطفاء")) flags |= GLAccountFlags.Amortization;
            if (lower.Contains("provision") || lower.Contains("مخصص")) flags |= GLAccountFlags.Provision;
            if (lower.Contains("working capital") || lower.Contains("رأس مال عامل")) flags |= GLAccountFlags.WorkingCapital;
            if (lower.Contains("fixed asset") || lower.Contains("أصل ثابت")) flags |= GLAccountFlags.FixedAsset;
            if (lower.Contains("investment") || lower.Contains("استثمار")) flags |= GLAccountFlags.Investment;
            if (lower.Contains("loan") || lower.Contains("قرض")) flags |= GLAccountFlags.Loan;
            if (lower.Contains("interest") || lower.Contains("فائدة")) flags |= GLAccountFlags.Interest;
            if (lower.Contains("dividend") || lower.Contains("أرباح موزعة")) flags |= GLAccountFlags.Dividend;
            if (lower.Contains("tax") || lower.Contains("ضريبة")) flags |= GLAccountFlags.Tax;

            return flags;
        }

        private bool ParseBool(string value, bool defaultValue = false)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;

            var lower = value.ToLower().Trim();
            if (lower == "yes" || lower == "true" || lower == "1" || lower == "نعم" || lower == "صحيح") return true;
            if (lower == "no" || lower == "false" || lower == "0" || lower == "لا" || lower == "خطأ") return false;

            return defaultValue;
        }

        private int ParseInt(string value, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Rebuild account hierarchy - fixes ParentAccountId and IsLeaf based on Level structure
        /// </summary>
        public async Task RebuildAccountHierarchyAsync()
        {
            // Get all accounts ordered by FullAccountNumber (which reflects hierarchy)
            var allAccounts = await Context.Queryable<GLAccount>()
                .OrderBy(a => a.FullAccountNumber)
                .ToListAsync();

            // Build lookup by FullAccountNumber for parent resolution
            var accountsByFullNumber = allAccounts.ToDictionary(a => a.FullAccountNumber ?? a.AccountNumber);

            foreach (var account in allAccounts)
            {
                // Determine parent from FullAccountNumber structure
                if (!string.IsNullOrEmpty(account.FullAccountNumber) && account.FullAccountNumber.Contains("-"))
                {
                    var parts = account.FullAccountNumber.Split('-');
                    if (parts.Length > 1)
                    {
                        // Parent's FullAccountNumber is everything except the last part
                        var parentFullNumber = string.Join("-", parts.Take(parts.Length - 1));
                        if (accountsByFullNumber.TryGetValue(parentFullNumber, out var parent))
                        {
                            if (account.ParentAccountId != parent.AccountId)
                            {
                                account.ParentAccountId = parent.AccountId;
                                account.Level = parent.Level + 1;
                            }
                        }
                    }
                }
            }

            // Determine IsLeaf based on whether account has children
            var accountsWithChildren = allAccounts
                .Where(a => a.ParentAccountId.HasValue)
                .Select(a => a.ParentAccountId.Value)
                .ToHashSet();

            foreach (var account in allAccounts)
            {
                bool hasChildren = accountsWithChildren.Contains(account.AccountId);
                if (account.IsLeaf == hasChildren) // IsLeaf should be opposite of hasChildren
                {
                    account.IsLeaf = !hasChildren;
                }
            }

            // Update all modified accounts
            await Context.Updateable(allAccounts)
                .UpdateColumns(a => new { a.ParentAccountId, a.Level, a.IsLeaf })
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Fix NormalBalance for all accounts based on their AccountType
        /// This corrects accounts that were imported with incorrect NormalBalance values
        /// إصلاح الرصيد الطبيعي لجميع الحسابات بناءً على نوع الحساب
        /// </summary>
        public async Task<int> FixAccountNormalBalancesAsync()
        {
            var allAccounts = await Context.Queryable<GLAccount>().ToListAsync();
            var fixedCount = 0;

            foreach (var account in allAccounts)
            {
                var expectedNormalBalance = GetDefaultNormalBalance(account.AccountType);
                if (account.NormalBalance != expectedNormalBalance)
                {
                    account.NormalBalance = expectedNormalBalance;
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                await Context.Updateable(allAccounts)
                    .UpdateColumns(a => new { a.NormalBalance })
                    .ExecuteCommandAsync();
            }

            return fixedCount;
        }

        #endregion
    }
}
