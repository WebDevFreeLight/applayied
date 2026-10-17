using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Sales;
using ZR.Repository;
using ZR.Service.Sales.IService;

namespace ZR.Service.Sales
{
    /// <summary>
    /// Customer Balance Service - خدمة أرصدة العملاء
    /// </summary>
    [AppService(ServiceType = typeof(IErpCustomerBalanceService), ServiceLifetime = LifeTime.Transient)]
    public class ErpCustomerBalanceService : BaseService<ErpCustomerBalance>, IErpCustomerBalanceService
    { 



        public PagedInfo<ErpCustomerBalance> GetList(ErpCustomerBalance parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpCustomerBalance>();

            predicate = predicate.AndIF(parm.CustomerId > 0, it => it.CustomerId == parm.CustomerId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TransactionType), it => it.TransactionType == parm.TransactionType);

            var response = Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.TransactionDate)
                .ToPage(pager);

            return response;
        }

        public ErpCustomerBalance GetInfo(long balanceId)
        {
            return GetFirst(x => x.BalanceId == balanceId);
        }

        public ErpCustomerBalance AddTransaction(ErpCustomerBalance model)
        {
            model.BalanceId = SnowFlakeSingle.Instance.NextId();

            // Calculate running balance
            var previousBalance = GetCustomerBalance(model.CustomerId);
            model.Balance = previousBalance + model.DebitAmount - model.CreditAmount;

            Insert(model);
            return model;
        }

        public int Delete(long[] ids)
        {
            // Soft delete - transactions should not be deleted
            return 0;
        }

        public List<ErpCustomerBalance> GetCustomerStatement(long customerId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId)
                .WhereIF(fromDate.HasValue, x => x.TransactionDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.TransactionDate <= toDate.Value)
                .OrderBy(x => x.TransactionDate)
                .OrderBy(x => x.BalanceId)
                .ToList();
        }

        public decimal GetCustomerBalance(long customerId)
        {
            var totalDebit = Queryable()
                .Where(x => x.CustomerId == customerId)
                .Sum(x => x.DebitAmount);

            var totalCredit = Queryable()
                .Where(x => x.CustomerId == customerId)
                .Sum(x => x.CreditAmount);

            return totalDebit - totalCredit;
        }

        public decimal GetCustomerTotalDebit(long customerId)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId)
                .Sum(x => x.DebitAmount);
        }

        public decimal GetCustomerTotalCredit(long customerId)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId)
                .Sum(x => x.CreditAmount);
        }

        public List<object> GetAgingReport(long? customerId = null)
        {
            var today = DateTime.Today;

            var query = Context.Queryable<ErpCustomerBalance>()
                .GroupBy(x => x.CustomerId)
                .WhereIF(customerId.HasValue, x => x.CustomerId == customerId);

            // This would need to be expanded to properly calculate aging buckets
            // (0-30 days, 31-60 days, 61-90 days, 90+ days)
            var result = query.Select(x => new
            {
                CustomerId = x.CustomerId,
                TotalDebit = SqlFunc.AggregateSum(x.DebitAmount),
                TotalCredit = SqlFunc.AggregateSum(x.CreditAmount),
                Balance = SqlFunc.AggregateSum(x.DebitAmount) - SqlFunc.AggregateSum(x.CreditAmount)
            })
            .ToList()
            .Cast<object>()
            .ToList();

            return result;
        }

        public List<object> GetOverdueBalances(int daysOverdue = 30)
        {
            var cutoffDate = DateTime.Today.AddDays(-daysOverdue);

            // Get customers with balances from transactions older than cutoff date
            var result = Context.Queryable<ErpCustomerBalance>()
                .Where(x => x.TransactionDate <= cutoffDate)
                .GroupBy(x => x.CustomerId)
                .Having(x => SqlFunc.AggregateSum(x.DebitAmount) - SqlFunc.AggregateSum(x.CreditAmount) > 0)
                .Select(x => new
                {
                    CustomerId = x.CustomerId,
                    OverdueBalance = SqlFunc.AggregateSum(x.DebitAmount) - SqlFunc.AggregateSum(x.CreditAmount),
                    OldestTransactionDate = SqlFunc.AggregateMin(x.TransactionDate)
                })
                .ToList()
                .Cast<object>()
                .ToList();

            return result;
        }

        public int RecalculateCustomerBalance(long customerId)
        {
            return Context.Ado.UseTran(() =>
            {
                var transactions = GetCustomerStatement(customerId, null, null);
                decimal runningBalance = 0;

                foreach (var trans in transactions)
                {
                    runningBalance += trans.DebitAmount - trans.CreditAmount;

                    Context.Updateable<ErpCustomerBalance>()
                        .SetColumns(it => new ErpCustomerBalance { Balance = runningBalance })
                        .Where(it => it.BalanceId == trans.BalanceId)
                        .ExecuteCommand();
                }

                // Update customer's current balance
                Context.Updateable<ErpCustomer>()
                    .SetColumns(it => new ErpCustomer { CurrentBalance = runningBalance })
                    .Where(it => it.CustomerId == customerId)
                    .ExecuteCommand();

                return 1;
            }).Data;
        }
    }
}

