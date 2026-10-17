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
    /// Budget Line Repository - مستودع بند الميزانية
    /// </summary>
    [AppService(ServiceType = typeof(IBudgetLineRepository), ServiceLifetime = LifeTime.Transient)]
    public class BudgetLineRepository : BaseRepository<BudgetLine>, IBudgetLineRepository
    {
        /// <summary>
        /// Get budget lines by header ID
        /// </summary>
        public List<BudgetLine> GetByHeaderId(Guid budgetHeaderId)
        {
            return Queryable()
                .Where(x => x.BudgetHeaderId == budgetHeaderId)
                .OrderBy(x => x.Period)
                .OrderBy(x => x.GLAccountId)
                .ToList();
        }

        /// <summary>
        /// Get budget line by ID
        /// </summary>
        public BudgetLine GetById(Guid lineId)
        {
            return GetFirst(x => x.Id == lineId);
        }

        /// <summary>
        /// Get budget lines by GL account
        /// </summary>
        public List<BudgetLine> GetByGLAccount(Guid glAccountId)
        {
            return Queryable()
                .Where(x => x.GLAccountId == glAccountId)
                .OrderBy(x => x.Period)
                .ToList();
        }

        /// <summary>
        /// Get budget lines by header and period
        /// </summary>
        public List<BudgetLine> GetByHeaderAndPeriod(Guid budgetHeaderId, int period)
        {
            return Queryable()
                .Where(x => x.BudgetHeaderId == budgetHeaderId && x.Period == period)
                .OrderBy(x => x.GLAccountId)
                .ToList();
        }

        /// <summary>
        /// Get budget line by header, GL account and period
        /// </summary>
        public BudgetLine GetByHeaderAccountPeriod(Guid budgetHeaderId, Guid glAccountId, int period)
        {
            return GetFirst(x => x.BudgetHeaderId == budgetHeaderId &&
                                 x.GLAccountId == glAccountId &&
                                 x.Period == period);
        }

        /// <summary>
        /// Delete lines by header ID
        /// </summary>
        public int DeleteByHeaderId(Guid budgetHeaderId)
        {
            return Context.Deleteable<BudgetLine>()
                .Where(x => x.BudgetHeaderId == budgetHeaderId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Update actual amount
        /// </summary>
        public int UpdateActualAmount(Guid lineId, decimal actualAmount)
        {
            return Context.Updateable<BudgetLine>()
                .SetColumns(it => new BudgetLine
                {
                    ActualAmount = actualAmount,
                    LastActualUpdate = DateTime.Now
                })
                .Where(it => it.Id == lineId)
                .ExecuteCommand();
        }

        /// <summary>
        /// Bulk update actual amounts
        /// </summary>
        public int BulkUpdateActualAmounts(List<BudgetLine> lines)
        {
            if (lines == null || lines.Count == 0)
                return 0;

            var count = 0;
            foreach (var line in lines)
            {
                count += Context.Updateable<BudgetLine>()
                    .SetColumns(it => new BudgetLine
                    {
                        ActualAmount = line.ActualAmount,
                        LastActualUpdate = DateTime.Now
                    })
                    .Where(it => it.Id == line.Id)
                    .ExecuteCommand();
            }
            return count;
        }

        /// <summary>
        /// Get total budgeted amount by header
        /// </summary>
        public decimal GetTotalBudgetedByHeader(Guid budgetHeaderId)
        {
            return Queryable()
                .Where(x => x.BudgetHeaderId == budgetHeaderId)
                .Sum(x => x.BudgetedAmount);
        }

        /// <summary>
        /// Get total actual amount by header
        /// </summary>
        public decimal GetTotalActualByHeader(Guid budgetHeaderId)
        {
            return Queryable()
                .Where(x => x.BudgetHeaderId == budgetHeaderId)
                .Sum(x => x.ActualAmount);
        }
    }
}
