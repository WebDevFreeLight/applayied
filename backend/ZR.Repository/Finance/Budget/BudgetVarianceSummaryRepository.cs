using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Budget;

namespace ZR.Repository.Finance.Budget
{
    /// <summary>
    /// Budget Variance Summary Repository - مستودع ملخص انحراف الميزانية
    /// </summary>
    [AppService(ServiceType = typeof(IBudgetVarianceSummaryRepository), ServiceLifetime = LifeTime.Transient)]
    public class BudgetVarianceSummaryRepository : BaseRepository<BudgetVarianceSummary>, IBudgetVarianceSummaryRepository
    {
        /// <summary>
        /// Get variance summaries by budget header ID
        /// </summary>
        public List<BudgetVarianceSummary> GetByHeaderId(Guid budgetHeaderId)
        {
            return Queryable()
                .Where(x => x.BudgetHeaderId == budgetHeaderId)
                .OrderBy(x => x.Period)
                .OrderBy(x => x.GLAccountId)
                .ToList();
        }

        /// <summary>
        /// Get variance summary by ID
        /// </summary>
        public BudgetVarianceSummary GetById(Guid summaryId)
        {
            return GetFirst(x => x.Id == summaryId);
        }

        /// <summary>
        /// Get variance summaries by GL account
        /// </summary>
        public List<BudgetVarianceSummary> GetByGLAccount(Guid glAccountId)
        {
            return Queryable()
                .Where(x => x.GLAccountId == glAccountId)
                .OrderBy(x => x.Period)
                .ToList();
        }

        /// <summary>
        /// Get variance summaries by header and period
        /// </summary>
        public List<BudgetVarianceSummary> GetByHeaderAndPeriod(Guid budgetHeaderId, int period)
        {
            return Queryable()
                .Where(x => x.BudgetHeaderId == budgetHeaderId && x.Period == period)
                .OrderBy(x => x.GLAccountId)
                .ToList();
        }

        /// <summary>
        /// Get variance summary by header, GL account and period
        /// </summary>
        public BudgetVarianceSummary GetByHeaderAccountPeriod(Guid budgetHeaderId, Guid glAccountId, int period)
        {
            return GetFirst(x => x.BudgetHeaderId == budgetHeaderId &&
                                 x.GLAccountId == glAccountId &&
                                 x.Period == period);
        }

        /// <summary>
        /// Delete summaries by header ID
        /// </summary>
        public int DeleteByHeaderId(Guid budgetHeaderId)
        {
            return Context.Deleteable<BudgetVarianceSummary>()
                .Where(x => x.BudgetHeaderId == budgetHeaderId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Bulk insert or update variance summaries
        /// </summary>
        public int BulkUpsert(List<BudgetVarianceSummary> summaries)
        {
            if (summaries == null || summaries.Count == 0)
                return 0;

            // Use StorageableDataTable for upsert functionality
            return Context.Storageable(summaries)
                .WhereColumns(s => new { s.BudgetHeaderId, s.GLAccountId, s.Period })
                .ToStorage()
                .AsInsertable
                .ExecuteCommand();
        }

        /// <summary>
        /// Get top variances (positive or negative)
        /// </summary>
        public List<BudgetVarianceSummary> GetTopVariances(Guid budgetHeaderId, int count, bool positiveOnly = false)
        {
            var query = Queryable()
                .Where(x => x.BudgetHeaderId == budgetHeaderId);

            if (positiveOnly)
            {
                query = query.Where(x => x.Variance > 0);
            }

            return query
                .OrderByDescending(x => SqlFunc.Abs(x.Variance))
                .Take(count)
                .ToList();
        }
    }
}
