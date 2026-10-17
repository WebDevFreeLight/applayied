using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.GL;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Account Opening Balance Repository - مستودع الأرصدة الافتتاحية
    /// </summary>
    [AppService(ServiceType = typeof(IGLAccountOpeningBalanceRepository), ServiceLifetime = LifeTime.Transient)]
    public class GLAccountOpeningBalanceRepository : BaseRepository<GLAccountOpeningBalance>, IGLAccountOpeningBalanceRepository
    {
        #region Basic CRUD

        /// <summary>
        /// Get paginated list of opening balances
        /// </summary>
        public PagedInfo<GLAccountOpeningBalance> GetList(GLAccountOpeningBalance parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLAccountOpeningBalance>();
            predicate = predicate.AndIF(parm.AccountId != Guid.Empty, it => it.AccountId == parm.AccountId);
            predicate = predicate.AndIF(parm.FiscalYear > 0, it => it.FiscalYear == parm.FiscalYear);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.FiscalYear)
                .OrderBy(x => x.Create_time)
                .ToPage(pager);
        }

        /// <summary>
        /// Get opening balance by ID
        /// </summary>
        public GLAccountOpeningBalance GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get opening balance by ID async
        /// </summary>
        public async Task<GLAccountOpeningBalance> GetByIdAsync(Guid id)
        {
            return await GetFirstAsync(x => x.Id == id);
        }

        /// <summary>
        /// Get opening balance for account and fiscal year
        /// </summary>
        public GLAccountOpeningBalance GetByAccountAndYear(Guid accountId, int fiscalYear)
        {
            return GetFirst(x => x.AccountId == accountId && x.FiscalYear == fiscalYear);
        }

        /// <summary>
        /// Get opening balance for account and fiscal year async
        /// </summary>
        public async Task<GLAccountOpeningBalance> GetByAccountAndYearAsync(Guid accountId, int fiscalYear)
        {
            return await GetFirstAsync(x => x.AccountId == accountId && x.FiscalYear == fiscalYear);
        }

        /// <summary>
        /// Insert opening balance
        /// </summary>
        public int InsertOpeningBalance(GLAccountOpeningBalance model)
        {
            return Insert(model);
        }

        /// <summary>
        /// Insert opening balance async
        /// </summary>
        public async Task<int> InsertOpeningBalanceAsync(GLAccountOpeningBalance model)
        {
            return await InsertAsync(model) ? 1 : 0;
        }

        /// <summary>
        /// Insert multiple opening balances
        /// </summary>
        public int InsertBulk(List<GLAccountOpeningBalance> models)
        {
            return InsertRange(models) ? models.Count : 0;
        }

        /// <summary>
        /// Insert multiple opening balances async
        /// </summary>
        public async Task<int> InsertBulkAsync(List<GLAccountOpeningBalance> models)
        {
            return await InsertRangeAsync(models) ? models.Count : 0;
        }

        /// <summary>
        /// Update opening balance
        /// </summary>
        public int UpdateOpeningBalance(GLAccountOpeningBalance model)
        {
            return Update(model, it => new
            {
                it.OpeningDebit,
                it.OpeningCredit,
                it.OpeningBalance,
                it.Currency,
                it.BaseOpeningDebit,
                it.BaseOpeningCredit,
                it.ExchangeRate,
                it.Notes,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Update opening balance async
        /// </summary>
        public async Task<int> UpdateOpeningBalanceAsync(GLAccountOpeningBalance model)
        {
            return await Context.Updateable(model)
                .UpdateColumns(it => new
                {
                    it.OpeningDebit,
                    it.OpeningCredit,
                    it.OpeningBalance,
                    it.Currency,
                    it.BaseOpeningDebit,
                    it.BaseOpeningCredit,
                    it.ExchangeRate,
                    it.Notes,
                    it.Update_by,
                    it.Update_time
                })
                .Where(f => f.Id == model.Id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Delete opening balance
        /// </summary>
        public int DeleteById(Guid id)
        {
            return Delete(x => x.Id == id) ? 1 : 0;
        }

        /// <summary>
        /// Delete opening balance async
        /// </summary>
        public async Task<int> DeleteByIdAsync(Guid id)
        {
            return await DeleteAsync(x => x.Id == id) ? 1 : 0;
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// Get opening balances by fiscal year
        /// </summary>
        public List<GLAccountOpeningBalance> GetByFiscalYear(int fiscalYear)
        {
            return Queryable()
                .Where(x => x.FiscalYear == fiscalYear)
                .OrderBy(x => x.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get opening balances by fiscal year async
        /// </summary>
        public async Task<List<GLAccountOpeningBalance>> GetByFiscalYearAsync(int fiscalYear)
        {
            return await Queryable()
                .Where(x => x.FiscalYear == fiscalYear)
                .OrderBy(x => x.Create_time)
                .ToListAsync();
        }

        /// <summary>
        /// Get opening balances by account ID
        /// </summary>
        public List<GLAccountOpeningBalance> GetByAccountId(Guid accountId)
        {
            return Queryable()
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.FiscalYear)
                .ToList();
        }

        /// <summary>
        /// Get opening balances by account ID async
        /// </summary>
        public async Task<List<GLAccountOpeningBalance>> GetByAccountIdAsync(Guid accountId)
        {
            return await Queryable()
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.FiscalYear)
                .ToListAsync();
        }

        /// <summary>
        /// Get all opening balances with account details for fiscal year
        /// </summary>
        public List<GLAccountOpeningBalance> GetWithAccountDetails(int fiscalYear, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLAccountOpeningBalance>();
            predicate = predicate.And(it => it.FiscalYear == fiscalYear);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return Context.Queryable<GLAccountOpeningBalance, GLAccount>((ob, a) =>
                    new JoinQueryInfos(JoinType.Left, ob.AccountId == a.AccountId))
                .Where(predicate.ToExpression())
                .Select((ob, a) => new GLAccountOpeningBalance
                {
                    Id = ob.Id,
                    AccountId = ob.AccountId,
                    FiscalYear = ob.FiscalYear,
                    PeriodStartDate = ob.PeriodStartDate,
                    PeriodEndDate = ob.PeriodEndDate,
                    OpeningDebit = ob.OpeningDebit,
                    OpeningCredit = ob.OpeningCredit,
                    OpeningBalance = ob.OpeningBalance,
                    Currency = ob.Currency,
                    BaseOpeningDebit = ob.BaseOpeningDebit,
                    BaseOpeningCredit = ob.BaseOpeningCredit,
                    ExchangeRate = ob.ExchangeRate,
                    SourceType = ob.SourceType,
                    SourceReference = ob.SourceReference,
                    IsLocked = ob.IsLocked,
                    LockedAt = ob.LockedAt,
                    LockedBy = ob.LockedBy,
                    Notes = ob.Notes,
                    Create_by = ob.Create_by,
                    Create_time = ob.Create_time,
                    Update_by = ob.Update_by,
                    Update_time = ob.Update_time,
                    CompanyID = ob.CompanyID,
                    BranchID = ob.BranchID,
                    Account = a
                })
                .ToList();
        }

        /// <summary>
        /// Get all opening balances with account details async
        /// </summary>
        public async Task<List<GLAccountOpeningBalance>> GetWithAccountDetailsAsync(int fiscalYear, long? companyId = null)
        {
            var predicate = Expressionable.Create<GLAccountOpeningBalance>();
            predicate = predicate.And(it => it.FiscalYear == fiscalYear);
            predicate = predicate.AndIF(companyId.HasValue, it => it.CompanyID == companyId);

            return await Context.Queryable<GLAccountOpeningBalance, GLAccount>((ob, a) =>
                    new JoinQueryInfos(JoinType.Left, ob.AccountId == a.AccountId))
                .Where(predicate.ToExpression())
                .Select((ob, a) => new GLAccountOpeningBalance
                {
                    Id = ob.Id,
                    AccountId = ob.AccountId,
                    FiscalYear = ob.FiscalYear,
                    PeriodStartDate = ob.PeriodStartDate,
                    PeriodEndDate = ob.PeriodEndDate,
                    OpeningDebit = ob.OpeningDebit,
                    OpeningCredit = ob.OpeningCredit,
                    OpeningBalance = ob.OpeningBalance,
                    Currency = ob.Currency,
                    BaseOpeningDebit = ob.BaseOpeningDebit,
                    BaseOpeningCredit = ob.BaseOpeningCredit,
                    ExchangeRate = ob.ExchangeRate,
                    SourceType = ob.SourceType,
                    SourceReference = ob.SourceReference,
                    IsLocked = ob.IsLocked,
                    LockedAt = ob.LockedAt,
                    LockedBy = ob.LockedBy,
                    Notes = ob.Notes,
                    Create_by = ob.Create_by,
                    Create_time = ob.Create_time,
                    Update_by = ob.Update_by,
                    Update_time = ob.Update_time,
                    CompanyID = ob.CompanyID,
                    BranchID = ob.BranchID,
                    Account = a
                })
                .ToListAsync();
        }

        #endregion

        #region Validation Methods

        /// <summary>
        /// Check if opening balance exists for account and year
        /// </summary>
        public bool Exists(Guid accountId, int fiscalYear)
        {
            return Any(x => x.AccountId == accountId && x.FiscalYear == fiscalYear);
        }

        /// <summary>
        /// Check if opening balance exists async
        /// </summary>
        public async Task<bool> ExistsAsync(Guid accountId, int fiscalYear)
        {
            return await Queryable().AnyAsync(x => x.AccountId == accountId && x.FiscalYear == fiscalYear);
        }

        /// <summary>
        /// Check if any opening balances are locked for fiscal year
        /// </summary>
        public bool HasLockedBalances(int fiscalYear)
        {
            return Any(x => x.FiscalYear == fiscalYear && x.IsLocked);
        }

        /// <summary>
        /// Check if any opening balances are locked async
        /// </summary>
        public async Task<bool> HasLockedBalancesAsync(int fiscalYear)
        {
            return await Queryable().AnyAsync(x => x.FiscalYear == fiscalYear && x.IsLocked);
        }

        #endregion

        #region Lock Operations

        /// <summary>
        /// Lock opening balance
        /// </summary>
        public int Lock(Guid id, Guid lockedBy)
        {
            return Context.Updateable<GLAccountOpeningBalance>()
                .SetColumns(it => new GLAccountOpeningBalance
                {
                    IsLocked = true,
                    LockedAt = DateTime.Now,
                    LockedBy = lockedBy
                })
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Lock opening balance async
        /// </summary>
        public async Task<int> LockAsync(Guid id, Guid lockedBy)
        {
            return await Context.Updateable<GLAccountOpeningBalance>()
                .SetColumns(it => new GLAccountOpeningBalance
                {
                    IsLocked = true,
                    LockedAt = DateTime.Now,
                    LockedBy = lockedBy
                })
                .Where(it => it.Id == id)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Lock all opening balances for fiscal year
        /// </summary>
        public int LockByFiscalYear(int fiscalYear, Guid lockedBy)
        {
            return Context.Updateable<GLAccountOpeningBalance>()
                .SetColumns(it => new GLAccountOpeningBalance
                {
                    IsLocked = true,
                    LockedAt = DateTime.Now,
                    LockedBy = lockedBy
                })
                .Where(it => it.FiscalYear == fiscalYear && !it.IsLocked)
                .ExecuteCommand();
        }

        /// <summary>
        /// Lock all opening balances for fiscal year async
        /// </summary>
        public async Task<int> LockByFiscalYearAsync(int fiscalYear, Guid lockedBy)
        {
            return await Context.Updateable<GLAccountOpeningBalance>()
                .SetColumns(it => new GLAccountOpeningBalance
                {
                    IsLocked = true,
                    LockedAt = DateTime.Now,
                    LockedBy = lockedBy
                })
                .Where(it => it.FiscalYear == fiscalYear && !it.IsLocked)
                .ExecuteCommandAsync();
        }

        /// <summary>
        /// Unlock opening balance (admin only)
        /// </summary>
        public int Unlock(Guid id)
        {
            return Context.Updateable<GLAccountOpeningBalance>()
                .SetColumns(it => new GLAccountOpeningBalance
                {
                    IsLocked = false,
                    LockedAt = null,
                    LockedBy = null
                })
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Unlock opening balance async
        /// </summary>
        public async Task<int> UnlockAsync(Guid id)
        {
            return await Context.Updateable<GLAccountOpeningBalance>()
                .SetColumns(it => new GLAccountOpeningBalance
                {
                    IsLocked = false,
                    LockedAt = null,
                    LockedBy = null
                })
                .Where(it => it.Id == id)
                .ExecuteCommandAsync();
        }

        #endregion

        #region Year-End Operations

        /// <summary>
        /// Create opening balances from previous year closing
        /// </summary>
        public async Task<int> CreateFromPreviousYearClosingAsync(int newFiscalYear, int previousFiscalYear, long? companyId, string createdBy)
        {
            // Get accounts with balance summaries from previous year
            var previousBalances = await Context.Queryable<GLAccountBalanceSummary>()
                .Where(b => b.FiscalYear == previousFiscalYear && b.FiscalPeriod == 0) // Period 0 = annual summary
                .WhereIF(companyId.HasValue, b => b.CompanyID == companyId)
                .ToListAsync();

            if (!previousBalances.Any())
                return 0;

            var newOpeningBalances = new List<GLAccountOpeningBalance>();
            var periodStartDate = new DateTime(newFiscalYear, 1, 1);
            var periodEndDate = new DateTime(newFiscalYear, 12, 31);

            foreach (var balance in previousBalances)
            {
                // Skip if opening balance already exists
                if (await ExistsAsync(balance.AccountId, newFiscalYear))
                    continue;

                newOpeningBalances.Add(new GLAccountOpeningBalance
                {
                    Id = Guid.NewGuid(),
                    AccountId = balance.AccountId,
                    FiscalYear = newFiscalYear,
                    PeriodStartDate = periodStartDate,
                    PeriodEndDate = periodEndDate,
                    OpeningDebit = balance.ClosingDebit,
                    OpeningCredit = balance.ClosingCredit,
                    OpeningBalance = balance.ClosingBalance,
                    Currency = balance.Currency,
                    BaseOpeningDebit = balance.BasePeriodDebit,
                    BaseOpeningCredit = balance.BasePeriodCredit,
                    ExchangeRate = 1,
                    SourceType = "Carryforward",
                    SourceReference = $"FY{previousFiscalYear} Closing",
                    IsLocked = false,
                    Create_by = createdBy,
                    Create_time = DateTime.Now,
                    CompanyID = companyId
                });
            }

            if (newOpeningBalances.Any())
            {
                return await InsertBulkAsync(newOpeningBalances);
            }

            return 0;
        }

        /// <summary>
        /// Get total debit and credit for fiscal year
        /// </summary>
        public (decimal TotalDebit, decimal TotalCredit) GetTotalsByFiscalYear(int fiscalYear, long? companyId = null)
        {
            var query = Queryable()
                .Where(x => x.FiscalYear == fiscalYear);

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }

            var balances = query.ToList();
            return (balances.Sum(x => x.OpeningDebit), balances.Sum(x => x.OpeningCredit));
        }

        /// <summary>
        /// Get total debit and credit for fiscal year async
        /// </summary>
        public async Task<(decimal TotalDebit, decimal TotalCredit)> GetTotalsByFiscalYearAsync(int fiscalYear, long? companyId = null)
        {
            var query = Queryable()
                .Where(x => x.FiscalYear == fiscalYear);

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyID == companyId);
            }

            var balances = await query.ToListAsync();
            return (balances.Sum(x => x.OpeningDebit), balances.Sum(x => x.OpeningCredit));
        }

        #endregion
    }
}
