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
    /// GL Journal Entry Repository - مستودع قيود اليومية
    /// </summary>
    [AppService(ServiceType = typeof(IGLJournalEntryRepository), ServiceLifetime = LifeTime.Transient)]
    public class GLJournalEntryRepository : BaseRepository<GLJournalEntry>, IGLJournalEntryRepository
    {
        /// <summary>
        /// Get paginated list of entries
        /// </summary>
        public PagedInfo<GLJournalEntry> GetList(GLJournalEntry parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLJournalEntry>();
            predicate = predicate.AndIF(parm.JournalId != Guid.Empty, it => it.JournalId == parm.JournalId);
            predicate = predicate.AndIF(parm.AccountId != Guid.Empty, it => it.AccountId == parm.AccountId);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.Create_time)
                .ToPage(pager);
        }

        /// <summary>
        /// Get entry by ID
        /// </summary>
        public GLJournalEntry GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Insert entry
        /// </summary>
        public int InsertEntry(GLJournalEntry model)
        {
            return Insert(model);
        }

        /// <summary>
        /// Insert multiple entries
        /// </summary>
        public int InsertEntries(List<GLJournalEntry> models)
        {
            return Insert(models);
        }

        /// <summary>
        /// Update entry
        /// </summary>
        public int UpdateEntry(GLJournalEntry model)
        {
            return Update(model, it => new
            {
                it.AccountId,
                it.Debit,
                it.Credit,
                it.DescriptionAr,
                it.DescriptionEn,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Delete entries by IDs
        /// </summary>
        public int DeleteByIds(Guid[] ids)
        {
            return Context.Deleteable<GLJournalEntry>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Delete entries by journal ID
        /// </summary>
        public int DeleteByJournalId(Guid journalId)
        {
            return Context.Deleteable<GLJournalEntry>()
                .Where(it => it.JournalId == journalId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Get entries by journal ID
        /// </summary>
        public List<GLJournalEntry> GetByJournalId(Guid journalId)
        {
            return Queryable()
                .Where(x => x.JournalId == journalId)
                .OrderBy(x => x.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get entries by account ID
        /// </summary>
        public List<GLJournalEntry> GetByAccountId(Guid accountId)
        {
            return Queryable()
                .Where(x => x.AccountId == accountId)
                .OrderByDescending(x => x.Create_time)
                .ToList();
        }

        /// <summary>
        /// Get total debit by journal ID
        /// </summary>
        public decimal GetTotalDebitByJournalId(Guid journalId)
        {
            return Queryable()
                .Where(x => x.JournalId == journalId)
                .Sum(x => x.Debit);
        }

        /// <summary>
        /// Get total credit by journal ID
        /// </summary>
        public decimal GetTotalCreditByJournalId(Guid journalId)
        {
            return Queryable()
                .Where(x => x.JournalId == journalId)
                .Sum(x => x.Credit);
        }

        /// <summary>
        /// Check if journal is balanced
        /// </summary>
        public bool IsJournalBalanced(Guid journalId)
        {
            var totalDebit = GetTotalDebitByJournalId(journalId);
            var totalCredit = GetTotalCreditByJournalId(journalId);
            return totalDebit == totalCredit;
        }

        /// <summary>
        /// Get account balance for a period
        /// </summary>
        public AccountBalanceResult GetAccountBalance(Guid accountId, DateTime periodStart, DateTime periodEnd)
        {
            var entries = Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => e.AccountId == accountId
                    && j.JournalDate >= periodStart
                    && j.JournalDate <= periodEnd
                    && j.Status == Model.Finance.Enums.JournalStatus.Posted)
                .ToList();

            return new AccountBalanceResult
            {
                TotalDebit = entries.Sum(e => e.BaseDebit),
                TotalCredit = entries.Sum(e => e.BaseCredit)
            };
        }

        /// <summary>
        /// Get account balance for a period async
        /// </summary>
        public async Task<AccountBalanceResult> GetAccountBalanceAsync(Guid accountId, DateTime periodStart, DateTime periodEnd)
        {
            var entries = await Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => e.AccountId == accountId
                    && j.JournalDate >= periodStart
                    && j.JournalDate <= periodEnd
                    && j.Status == Model.Finance.Enums.JournalStatus.Posted)
                .ToListAsync();

            return new AccountBalanceResult
            {
                TotalDebit = entries.Sum(e => e.BaseDebit),
                TotalCredit = entries.Sum(e => e.BaseCredit)
            };
        }

        /// <summary>
        /// Get entries for P&L report by dimension (Cost Center / Project)
        /// Returns all posted journal entries for the specified period and accounts
        /// </summary>
        public List<GLJournalEntry> GetEntriesForDimensionPL(
            DateTime periodStart,
            DateTime periodEnd,
            List<Guid> accountIds,
            long? companyId = null,
            Guid? costCenterId = null,
            Guid? projectId = null)
        {
            var query = Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => j.JournalDate >= periodStart
                              && j.JournalDate <= periodEnd
                              && j.Status == Model.Finance.Enums.JournalStatus.Posted
                              && accountIds.Contains(e.AccountId));

            // Apply company filter if specified
            if (companyId.HasValue)
            {
                query = query.Where((e, j) => j.CompanyID == companyId);
            }

            // Apply cost center filter if specified
            if (costCenterId.HasValue)
            {
                query = query.Where((e, j) => e.CostCenterId == costCenterId);
            }

            // Apply project filter if specified
            if (projectId.HasValue)
            {
                query = query.Where((e, j) => e.ProjectId == projectId);
            }

            return query.Select((e, j) => e).ToList();
        }

        #region Trial Balance Methods - طرق ميزان المراجعة

        /// <summary>
        /// Get opening balance for an account (transactions before period start)
        /// </summary>
        public AccountBalanceResult GetOpeningBalance(Guid accountId, DateTime beforeDate, long? companyId = null)
        {
            var query = Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => e.AccountId == accountId
                    && j.JournalDate < beforeDate
                    && j.Status == Model.Finance.Enums.JournalStatus.Posted);

            if (companyId.HasValue)
            {
                query = query.Where((e, j) => j.CompanyID == companyId);
            }

            var entries = query.ToList();

            return new AccountBalanceResult
            {
                TotalDebit = entries.Sum(e => e.BaseDebit),
                TotalCredit = entries.Sum(e => e.BaseCredit)
            };
        }

        /// <summary>
        /// Get opening balance for an account async
        /// </summary>
        public async Task<AccountBalanceResult> GetOpeningBalanceAsync(Guid accountId, DateTime beforeDate, long? companyId = null)
        {
            var query = Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => e.AccountId == accountId
                    && j.JournalDate < beforeDate
                    && j.Status == Model.Finance.Enums.JournalStatus.Posted);

            if (companyId.HasValue)
            {
                query = query.Where((e, j) => j.CompanyID == companyId);
            }

            var entries = await query.ToListAsync();

            return new AccountBalanceResult
            {
                TotalDebit = entries.Sum(e => e.BaseDebit),
                TotalCredit = entries.Sum(e => e.BaseCredit)
            };
        }

        /// <summary>
        /// Get period movement for an account (transactions within period)
        /// </summary>
        public AccountBalanceResult GetPeriodMovement(Guid accountId, DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            var query = Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => e.AccountId == accountId
                    && j.JournalDate >= periodStart
                    && j.JournalDate <= periodEnd
                    && j.Status == Model.Finance.Enums.JournalStatus.Posted);

            if (companyId.HasValue)
            {
                query = query.Where((e, j) => j.CompanyID == companyId);
            }

            var entries = query.ToList();

            return new AccountBalanceResult
            {
                TotalDebit = entries.Sum(e => e.BaseDebit),
                TotalCredit = entries.Sum(e => e.BaseCredit)
            };
        }

        /// <summary>
        /// Get period movement for an account async
        /// </summary>
        public async Task<AccountBalanceResult> GetPeriodMovementAsync(Guid accountId, DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            var query = Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => e.AccountId == accountId
                    && j.JournalDate >= periodStart
                    && j.JournalDate <= periodEnd
                    && j.Status == Model.Finance.Enums.JournalStatus.Posted);

            if (companyId.HasValue)
            {
                query = query.Where((e, j) => j.CompanyID == companyId);
            }

            var entries = await query.ToListAsync();

            return new AccountBalanceResult
            {
                TotalDebit = entries.Sum(e => e.BaseDebit),
                TotalCredit = entries.Sum(e => e.BaseCredit)
            };
        }

        /// <summary>
        /// Get trial balance data for all accounts efficiently
        /// Opening Balance = GL Opening Balance Table + Journal Entries before period
        /// Period Movement = Journal Entries within period
        /// </summary>
        public Dictionary<Guid, TrialBalanceAccountData> GetTrialBalanceData(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            var result = new Dictionary<Guid, TrialBalanceAccountData>();
            int fiscalYear = periodStart.Year;

            // Step 1: Get opening balances from gl_account_opening_balance table
            var glOpeningQuery = Context.Queryable<GLAccountOpeningBalance>()
                .Where(ob => ob.FiscalYear == fiscalYear);

            if (companyId.HasValue)
            {
                glOpeningQuery = glOpeningQuery.Where(ob => ob.CompanyID == companyId);
            }

            var glOpeningBalances = glOpeningQuery
                .Select(ob => new
                {
                    ob.AccountId,
                    // Use base currency amounts for consistency
                    OpeningDebit = ob.BaseOpeningDebit > 0 ? ob.BaseOpeningDebit : ob.OpeningDebit,
                    OpeningCredit = ob.BaseOpeningCredit > 0 ? ob.BaseOpeningCredit : ob.OpeningCredit
                })
                .ToList();

            foreach (var ob in glOpeningBalances)
            {
                result[ob.AccountId] = new TrialBalanceAccountData
                {
                    AccountId = ob.AccountId,
                    OpeningDebit = ob.OpeningDebit,
                    OpeningCredit = ob.OpeningCredit,
                    PeriodDebit = 0,
                    PeriodCredit = 0
                };
            }

            // Step 2: Get journal entries from fiscal year start to period start (to add to opening balance)
            // These are transactions that happened after the opening balance was set but before our report period
            var fiscalYearStart = new DateTime(fiscalYear, 1, 1);
            if (periodStart > fiscalYearStart)
            {
                var prePeriodQuery = Context.Queryable<GLJournalEntry>()
                    .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                    .Where((e, j) => j.JournalDate >= fiscalYearStart
                        && j.JournalDate < periodStart
                        && j.Status == Model.Finance.Enums.JournalStatus.Posted);

                if (companyId.HasValue)
                {
                    prePeriodQuery = prePeriodQuery.Where((e, j) => j.CompanyID == companyId);
                }

                var prePeriodEntries = prePeriodQuery
                    .GroupBy((e, j) => e.AccountId)
                    .Select((e, j) => new
                    {
                        AccountId = e.AccountId,
                        TotalDebit = SqlFunc.AggregateSum(e.BaseDebit),
                        TotalCredit = SqlFunc.AggregateSum(e.BaseCredit)
                    })
                    .ToList();

                foreach (var entry in prePeriodEntries)
                {
                    if (result.ContainsKey(entry.AccountId))
                    {
                        // Add to existing opening balance
                        result[entry.AccountId].OpeningDebit += entry.TotalDebit;
                        result[entry.AccountId].OpeningCredit += entry.TotalCredit;
                    }
                    else
                    {
                        // Create new entry with just pre-period amounts as opening
                        result[entry.AccountId] = new TrialBalanceAccountData
                        {
                            AccountId = entry.AccountId,
                            OpeningDebit = entry.TotalDebit,
                            OpeningCredit = entry.TotalCredit,
                            PeriodDebit = 0,
                            PeriodCredit = 0
                        };
                    }
                }
            }

            // Step 3: Get period movements (all posted entries within period)
            var periodQuery = Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => j.JournalDate >= periodStart
                    && j.JournalDate <= periodEnd
                    && j.Status == Model.Finance.Enums.JournalStatus.Posted);

            if (companyId.HasValue)
            {
                periodQuery = periodQuery.Where((e, j) => j.CompanyID == companyId);
            }

            var periodMovements = periodQuery
                .GroupBy((e, j) => e.AccountId)
                .Select((e, j) => new
                {
                    AccountId = e.AccountId,
                    TotalDebit = SqlFunc.AggregateSum(e.BaseDebit),
                    TotalCredit = SqlFunc.AggregateSum(e.BaseCredit)
                })
                .ToList();

            foreach (var pm in periodMovements)
            {
                if (result.ContainsKey(pm.AccountId))
                {
                    result[pm.AccountId].PeriodDebit = pm.TotalDebit;
                    result[pm.AccountId].PeriodCredit = pm.TotalCredit;
                }
                else
                {
                    result[pm.AccountId] = new TrialBalanceAccountData
                    {
                        AccountId = pm.AccountId,
                        OpeningDebit = 0,
                        OpeningCredit = 0,
                        PeriodDebit = pm.TotalDebit,
                        PeriodCredit = pm.TotalCredit
                    };
                }
            }

            return result;
        }

        /// <summary>
        /// Get trial balance data for all accounts efficiently async
        /// Opening Balance = GL Opening Balance Table + Journal Entries before period
        /// Period Movement = Journal Entries within period
        /// </summary>
        public async Task<Dictionary<Guid, TrialBalanceAccountData>> GetTrialBalanceDataAsync(DateTime periodStart, DateTime periodEnd, long? companyId = null)
        {
            var result = new Dictionary<Guid, TrialBalanceAccountData>();
            int fiscalYear = periodStart.Year;

            // Step 1: Get opening balances from gl_account_opening_balance table
            var glOpeningQuery = Context.Queryable<GLAccountOpeningBalance>()
                .Where(ob => ob.FiscalYear == fiscalYear);

            if (companyId.HasValue)
            {
                glOpeningQuery = glOpeningQuery.Where(ob => ob.CompanyID == companyId);
            }

            var glOpeningBalances = await glOpeningQuery
                .Select(ob => new
                {
                    ob.AccountId,
                    // Use base currency amounts for consistency
                    OpeningDebit = ob.BaseOpeningDebit > 0 ? ob.BaseOpeningDebit : ob.OpeningDebit,
                    OpeningCredit = ob.BaseOpeningCredit > 0 ? ob.BaseOpeningCredit : ob.OpeningCredit
                })
                .ToListAsync();

            foreach (var ob in glOpeningBalances)
            {
                result[ob.AccountId] = new TrialBalanceAccountData
                {
                    AccountId = ob.AccountId,
                    OpeningDebit = ob.OpeningDebit,
                    OpeningCredit = ob.OpeningCredit,
                    PeriodDebit = 0,
                    PeriodCredit = 0
                };
            }

            // Step 2: Get journal entries from fiscal year start to period start (to add to opening balance)
            // These are transactions that happened after the opening balance was set but before our report period
            var fiscalYearStart = new DateTime(fiscalYear, 1, 1);
            if (periodStart > fiscalYearStart)
            {
                var prePeriodQuery = Context.Queryable<GLJournalEntry>()
                    .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                    .Where((e, j) => j.JournalDate >= fiscalYearStart
                        && j.JournalDate < periodStart
                        && j.Status == Model.Finance.Enums.JournalStatus.Posted);

                if (companyId.HasValue)
                {
                    prePeriodQuery = prePeriodQuery.Where((e, j) => j.CompanyID == companyId);
                }

                var prePeriodEntries = await prePeriodQuery
                    .GroupBy((e, j) => e.AccountId)
                    .Select((e, j) => new
                    {
                        AccountId = e.AccountId,
                        TotalDebit = SqlFunc.AggregateSum(e.BaseDebit),
                        TotalCredit = SqlFunc.AggregateSum(e.BaseCredit)
                    })
                    .ToListAsync();

                foreach (var entry in prePeriodEntries)
                {
                    if (result.ContainsKey(entry.AccountId))
                    {
                        // Add to existing opening balance
                        result[entry.AccountId].OpeningDebit += entry.TotalDebit;
                        result[entry.AccountId].OpeningCredit += entry.TotalCredit;
                    }
                    else
                    {
                        // Create new entry with just pre-period amounts as opening
                        result[entry.AccountId] = new TrialBalanceAccountData
                        {
                            AccountId = entry.AccountId,
                            OpeningDebit = entry.TotalDebit,
                            OpeningCredit = entry.TotalCredit,
                            PeriodDebit = 0,
                            PeriodCredit = 0
                        };
                    }
                }
            }

            // Step 3: Get period movements (all posted entries within period)
            var periodQuery = Context.Queryable<GLJournalEntry>()
                .InnerJoin<GLJournal>((e, j) => e.JournalId == j.Id)
                .Where((e, j) => j.JournalDate >= periodStart
                    && j.JournalDate <= periodEnd
                    && j.Status == Model.Finance.Enums.JournalStatus.Posted);

            if (companyId.HasValue)
            {
                periodQuery = periodQuery.Where((e, j) => j.CompanyID == companyId);
            }

            var periodMovements = await periodQuery
                .GroupBy((e, j) => e.AccountId)
                .Select((e, j) => new
                {
                    AccountId = e.AccountId,
                    TotalDebit = SqlFunc.AggregateSum(e.BaseDebit),
                    TotalCredit = SqlFunc.AggregateSum(e.BaseCredit)
                })
                .ToListAsync();

            foreach (var pm in periodMovements)
            {
                if (result.ContainsKey(pm.AccountId))
                {
                    result[pm.AccountId].PeriodDebit = pm.TotalDebit;
                    result[pm.AccountId].PeriodCredit = pm.TotalCredit;
                }
                else
                {
                    result[pm.AccountId] = new TrialBalanceAccountData
                    {
                        AccountId = pm.AccountId,
                        OpeningDebit = 0,
                        OpeningCredit = 0,
                        PeriodDebit = pm.TotalDebit,
                        PeriodCredit = pm.TotalCredit
                    };
                }
            }

            return result;
        }

        #endregion
    }
}

