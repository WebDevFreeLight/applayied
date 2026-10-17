using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using ZR.Common;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Repository.Finance.GL;
using ZR.ServiceCore.Model;
using ZR.Service.Finance.Configuration;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.GL
{
    /// <summary>
    /// GL Account Opening Balance Service - خدمة أرصدة الافتتاحية
    /// Single source of truth for ALL opening balances in the system
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class GLAccountOpeningBalanceService
    {
        private readonly IGLAccountOpeningBalanceRepository _repository;
        private readonly IGLAccountRepository _accountRepository;
        private readonly IFiscalYearHelper _fiscalYearHelper;
        private readonly INormalBalanceResolver _normalBalanceResolver;

        public GLAccountOpeningBalanceService(
            IGLAccountOpeningBalanceRepository repository,
            IGLAccountRepository accountRepository,
            IFiscalYearHelper fiscalYearHelper,
            INormalBalanceResolver normalBalanceResolver)
        {
            _repository = repository;
            _accountRepository = accountRepository;
            _fiscalYearHelper = fiscalYearHelper;
            _normalBalanceResolver = normalBalanceResolver;
        }

        #region Query Methods

        /// <summary>
        /// Get list of opening balances with filters
        /// </summary>
        public PagedInfo<GLAccountOpeningBalanceListDto> GetList(GLAccountOpeningBalanceQueryDto query, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLAccountOpeningBalance>();

            // Apply filters
            predicate = predicate.AndIF(query.FiscalYear.HasValue, x => x.FiscalYear == query.FiscalYear.Value);
            predicate = predicate.AndIF(query.AccountId.HasValue, x => x.AccountId == query.AccountId.Value);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.Currency), x => x.Currency == query.Currency);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(query.SourceType), x => x.SourceType == query.SourceType);
            predicate = predicate.AndIF(query.IsLocked.HasValue, x => x.IsLocked == query.IsLocked.Value);
            predicate = predicate.AndIF(query.CompanyID.HasValue, x => x.CompanyID == query.CompanyID.Value);

            // First, get opening balances with basic filters
            var totalCount = 0;
            var openingBalances = _repository.Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(ob => ob.FiscalYear, OrderByType.Desc)
                .OrderBy(ob => ob.AccountId, OrderByType.Asc)
                .ToPageList(pager.PageNum, pager.PageSize, ref totalCount);

            // Get unique account IDs from the results
            var accountIds = openingBalances.Select(ob => ob.AccountId).Distinct().ToList();

            // Fetch GL account information separately
            var accounts = accountIds.Any()
                ? _accountRepository.GetList(a => accountIds.Contains(a.AccountId))
                : new List<GLAccount>();

            // Create lookup dictionary for fast access
            var accountLookup = accounts.ToDictionary(a => a.AccountId, a => a);

            // Map to DTOs with account information
            var pagedResult = openingBalances.Select(ob =>
            {
                accountLookup.TryGetValue(ob.AccountId, out var account);
                return new GLAccountOpeningBalanceListDto
                {
                    Id = ob.Id,
                    AccountId = ob.AccountId,
                    AccountNumber = account?.AccountNumber ?? "",
                    AccountNameAr = account?.AccountNameAr ?? "",
                    AccountNameEn = account?.AccountNameEn ?? "",
                    FiscalYear = ob.FiscalYear,
                    OpeningDebit = ob.OpeningDebit,
                    OpeningCredit = ob.OpeningCredit,
                    OpeningBalance = ob.OpeningBalance,
                    Currency = ob.Currency,
                    ExchangeRate = ob.ExchangeRate,
                    SourceTypeDisplay = GetSourceTypeDisplay(ob.SourceType),
                    IsLocked = ob.IsLocked,
                    Create_time = ob.Create_time
                };
            }).ToList();

            // Apply account-based filters if specified (post-filter since we need account data)
            if (!string.IsNullOrEmpty(query.AccountNumber))
            {
                pagedResult = pagedResult.Where(x => x.AccountNumber.Contains(query.AccountNumber)).ToList();
            }
            if (!string.IsNullOrEmpty(query.AccountNameAr))
            {
                pagedResult = pagedResult.Where(x => x.AccountNameAr.Contains(query.AccountNameAr)).ToList();
            }
            if (!string.IsNullOrEmpty(query.AccountNameEn))
            {
                pagedResult = pagedResult.Where(x => x.AccountNameEn.Contains(query.AccountNameEn)).ToList();
            }

            // Return paginated result
            return new PagedInfo<GLAccountOpeningBalanceListDto>
            {
                TotalNum = totalCount,
                PageIndex = pager.PageNum,
                PageSize = pager.PageSize,
                Result = pagedResult
            };
        }

        /// <summary>
        /// Get opening balance detail by ID
        /// </summary>
        public GLAccountOpeningBalanceDetailDto GetDetail(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND, new Dictionary<string, object> { { "Id", id } });
            }

            // Load account information for display
            var account = _accountRepository.GetById(entity.AccountId);

            return new GLAccountOpeningBalanceDetailDto
            {
                Id = entity.Id,
                AccountId = entity.AccountId,
                AccountNumber = account?.AccountNumber ?? "",
                AccountNameAr = account?.AccountNameAr ?? "",
                AccountNameEn = account?.AccountNameEn ?? "",
                FiscalYear = entity.FiscalYear,
                PeriodStartDate = entity.PeriodStartDate,
                PeriodEndDate = entity.PeriodEndDate,
                OpeningDebit = entity.OpeningDebit,
                OpeningCredit = entity.OpeningCredit,
                OpeningBalance = entity.OpeningBalance,
                Currency = entity.Currency,
                ExchangeRate = entity.ExchangeRate,
                BaseOpeningDebit = entity.BaseOpeningDebit,
                BaseOpeningCredit = entity.BaseOpeningCredit,
                SourceType = entity.SourceType,
                SourceTypeDisplay = GetSourceTypeDisplay(entity.SourceType),
                SourceReference = entity.SourceReference,
                IsLocked = entity.IsLocked,
                LockedDate = entity.LockedAt,
                LockedByUserId = entity.LockedBy?.ToString() ?? "",
                Notes = entity.Notes,
                CompanyID = entity.CompanyID,
                BranchId = null, // BranchID from SysBase is long?, DTO expects Guid? - no direct conversion, set to null
                Create_time = entity.Create_time,
                Create_by = entity.Create_by,
                Update_time = entity.Update_time,
                Update_by = entity.Update_by
            };
        }

        #endregion

        #region Create/Update/Delete Methods

        /// <summary>
        /// Create new opening balance with comprehensive validation
        /// </summary>
        public int Create(CreateGLAccountOpeningBalanceDto dto, string userId)
        {
            // Validation 1: Check if account exists
            var account = _accountRepository.GetById(dto.AccountId);
            if (account == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND, new Dictionary<string, object> { { "AccountId", dto.AccountId } });
            }

            // Validation 2: Only leaf accounts can have opening balances
            if (!account.IsLeaf)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_ONLY_LEAF, new Dictionary<string, object> { { "AccountId", dto.AccountId } });
            }

            // Validation 3: Check for duplicate (one opening balance per account per fiscal year)
            var existingBalance = _repository.GetFirst(x =>
                x.AccountId == dto.AccountId &&
                x.FiscalYear == dto.FiscalYear);

            if (existingBalance != null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_EXISTS, new Dictionary<string, object> { { "AccountNumber", account.AccountNumber }, { "FiscalYear", dto.FiscalYear } });
            }

            // Validation 4: Debit and Credit cannot both be zero (unless explicitly allowed)
            if (dto.OpeningDebit == 0 && dto.OpeningCredit == 0)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_ZERO);
            }

            // Calculate period dates using fiscal year helper (respects ERP Settings)
            var periodStartDate = _fiscalYearHelper.GetFiscalYearStartDate(dto.FiscalYear, dto.CompanyID);
            var periodEndDate = _fiscalYearHelper.GetFiscalYearEndDate(dto.FiscalYear, dto.CompanyID);

            // Calculate balances using normalBalance-aware calculation
            // This ensures the opening balance is positive when in the account's normal direction
            var openingBalance = _normalBalanceResolver.CalculateBalance(
                dto.OpeningDebit,
                dto.OpeningCredit,
                account.NormalBalance);
            var baseOpeningDebit = dto.OpeningDebit * dto.ExchangeRate;
            var baseOpeningCredit = dto.OpeningCredit * dto.ExchangeRate;

            // Create entity
            var entity = new GLAccountOpeningBalance
            {
                Id = Guid.NewGuid(),
                AccountId = dto.AccountId,
                FiscalYear = dto.FiscalYear,
                PeriodStartDate = periodStartDate,
                PeriodEndDate = periodEndDate,
                OpeningDebit = dto.OpeningDebit,
                OpeningCredit = dto.OpeningCredit,
                OpeningBalance = openingBalance,
                Currency = dto.Currency,
                ExchangeRate = dto.ExchangeRate,
                BaseOpeningDebit = baseOpeningDebit,
                BaseOpeningCredit = baseOpeningCredit,
                SourceType = dto.SourceType,
                SourceReference = dto.SourceReference,
                IsLocked = false,
                Notes = dto.Notes,
                CompanyID = dto.CompanyID,
                BranchID = dto.BranchId.HasValue ? (long?)dto.BranchId.Value.GetHashCode() : null, // Convert Guid? to long? for SysBase (temporary - needs proper mapping)
                Create_time = DateTime.Now,
                Create_by = userId
            };

            return _repository.Insert(entity);
        }

        /// <summary>
        /// Update existing opening balance (only if not locked)
        /// </summary>
        public int Update(UpdateGLAccountOpeningBalanceDto dto, string userId)
        {
            var entity = _repository.GetById(dto.Id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND, new Dictionary<string, object> { { "Id", dto.Id } });
            }

            // Validation: Cannot update locked opening balance
            if (entity.IsLocked)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_LOCKED, new Dictionary<string, object> { { "Id", dto.Id } });
            }

            // Validation: Debit and Credit cannot both be zero
            if (dto.OpeningDebit == 0 && dto.OpeningCredit == 0)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_ZERO);
            }

            // Get account to access normalBalance for correct balance calculation
            var account = _accountRepository.GetById(entity.AccountId);
            if (account == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_ACCOUNT_NOT_FOUND, new Dictionary<string, object> { { "AccountId", entity.AccountId } });
            }

            // Recalculate balances using normalBalance-aware calculation
            entity.OpeningDebit = dto.OpeningDebit;
            entity.OpeningCredit = dto.OpeningCredit;
            entity.OpeningBalance = _normalBalanceResolver.CalculateBalance(
                dto.OpeningDebit,
                dto.OpeningCredit,
                account.NormalBalance);
            entity.ExchangeRate = dto.ExchangeRate;
            entity.BaseOpeningDebit = dto.OpeningDebit * dto.ExchangeRate;
            entity.BaseOpeningCredit = dto.OpeningCredit * dto.ExchangeRate;
            entity.Notes = dto.Notes;
            entity.Update_time = DateTime.Now;
            entity.Update_by = userId;

            return _repository.Update(entity);
        }

        /// <summary>
        /// Delete opening balance (only if not locked)
        /// </summary>
        public int Delete(Guid id)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND, new Dictionary<string, object> { { "Id", id } });
            }

            // Validation: Cannot delete locked opening balance
            if (entity.IsLocked)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_LOCKED, new Dictionary<string, object> { { "Id", id } });
            }

            return _repository.Delete(id);
        }

        #endregion

        #region Lock/Unlock Methods

        /// <summary>
        /// Lock opening balance to prevent modifications
        /// </summary>
        public int Lock(Guid id, string userId)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND, new Dictionary<string, object> { { "Id", id } });
            }

            if (entity.IsLocked)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_LOCKED, new Dictionary<string, object> { { "Id", id } });
            }

            entity.IsLocked = true;
            entity.LockedAt = DateTime.Now;
            
            // Try to parse userId string to Guid for LockedBy field
            if (Guid.TryParse(userId, out Guid userGuid))
            {
                entity.LockedBy = userGuid;
            }
            
            entity.Update_time = DateTime.Now;
            entity.Update_by = userId;

            return _repository.Update(entity);
        }

        /// <summary>
        /// Unlock opening balance to allow modifications
        /// </summary>
        public int Unlock(Guid id, string userId)
        {
            var entity = _repository.GetById(id);
            if (entity == null)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_OPENING_BALANCE_NOT_FOUND, new Dictionary<string, object> { { "Id", id } });
            }

            if (!entity.IsLocked)
            {
                throw new AppException(ExceptionCode.FINANCE_GL_VALIDATION_FAILED, new Dictionary<string, object> { { "Id", id }, { "Reason", "OpeningBalanceNotLocked" } });
            }

            entity.IsLocked = false;
            entity.LockedAt = null;
            entity.LockedBy = null;
            entity.Update_time = DateTime.Now;
            entity.Update_by = userId;

            return _repository.Update(entity);
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Validate that total opening balances are balanced (Total Debit = Total Credit) for a fiscal year
        /// </summary>
        public GLOpeningBalanceValidationResult ValidateBalanceForFiscalYear(int fiscalYear, long? companyId = null)
        {
            var result = new GLOpeningBalanceValidationResult();

            var predicate = Expressionable.Create<GLAccountOpeningBalance>();
            predicate = predicate.And(x => x.FiscalYear == fiscalYear);
            predicate = predicate.AndIF(companyId.HasValue, x => x.CompanyID == companyId.Value);

            var openingBalances = _repository.GetList(predicate.ToExpression());

            if (!openingBalances.Any())
            {
                result.Warnings.Add($"No opening balances found for fiscal year {fiscalYear}");
                result.IsValid = true;
                return result;
            }

            // Calculate totals
            result.TotalDebit = openingBalances.Sum(x => x.OpeningDebit);
            result.TotalCredit = openingBalances.Sum(x => x.OpeningCredit);
            result.Difference = result.TotalDebit - result.TotalCredit;

            // Check if balanced (tolerance: 0.01)
            if (Math.Abs(result.Difference) > 0.01m)
            {
                result.Errors.Add($"Opening balances are not balanced. Debit: {result.TotalDebit:N2}, Credit: {result.TotalCredit:N2}, Difference: {result.Difference:N2}");
                result.IsValid = false;
            }
            else
            {
                result.IsValid = true;
            }

            // Additional warnings
            var lockedCount = openingBalances.Count(x => x.IsLocked);
            if (lockedCount > 0 && lockedCount < openingBalances.Count)
            {
                result.Warnings.Add($"{lockedCount} out of {openingBalances.Count} opening balances are locked");
            }

            return result;
        }

        /// <summary>
        /// Get opening balance for a specific account and fiscal year
        /// </summary>
        public GLAccountOpeningBalance GetByAccountAndYear(Guid accountId, int fiscalYear)
        {
            return _repository.GetFirst(x => x.AccountId == accountId && x.FiscalYear == fiscalYear);
        }

        /// <summary>
        /// Check if account has opening balance for fiscal year
        /// </summary>
        public bool HasOpeningBalance(Guid accountId, int fiscalYear)
        {
            return _repository.GetFirst(x => x.AccountId == accountId && x.FiscalYear == fiscalYear) != null;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get display text for source type
        /// </summary>
        private string GetSourceTypeDisplay(string sourceType)
        {
            return sourceType switch
            {
                "Manual" => "Manual Entry - إدخال يدوي",
                "YearEndClosing" => "Year-End Closing - إقفال نهاية السنة",
                "DataMigration" => "Data Migration - ترحيل بيانات",
                "SystemGenerated" => "System Generated - مولد تلقائياً",
                _ => sourceType ?? "Unknown"
            };
        }

        #endregion
    }
}
