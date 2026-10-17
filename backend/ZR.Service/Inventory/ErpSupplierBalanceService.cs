
using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Inventory.Suppliers;
using ZR.Repository;
using ZR.Service.Inventory.IService;

namespace ZR.Service.Inventory
{
    
    
    [AppService(ServiceType = typeof(IErpSupplierBalanceService), ServiceLifetime = LifeTime.Transient)]
    public class ErpSupplierBalanceService : BaseService<ErpSupplierBalance>, IErpSupplierBalanceService
    {


        public PagedInfo<ErpSupplierBalance> GetList(ErpSupplierBalance parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpSupplierBalance>();
            predicate = predicate.AndIF(parm.SupplierId > 0, it => it.SupplierId == parm.SupplierId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TransactionType), it => it.TransactionType == parm.TransactionType);
            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.TransactionDate).ToPage(pager);
        }

        public ErpSupplierBalance GetInfo(long balanceId) => GetFirst(x => x.BalanceId == balanceId);

        public ErpSupplierBalance AddTransaction(ErpSupplierBalance model)
        {
            model.BalanceId = SnowFlakeSingle.Instance.NextId();
            var previousBalance = GetSupplierBalance(model.SupplierId);
            model.Balance = previousBalance + model.CreditAmount - model.DebitAmount;
            Insert(model);

            // Update supplier current balance
            Context.Updateable<ErpSupplier>()
                .SetColumns(it => new ErpSupplier { CurrentBalance = model.Balance, Update_time = DateTime.Now })
                .Where(it => it.SupplierId == model.SupplierId).ExecuteCommand();

            return model;
        }

        public List<ErpSupplierBalance> GetSupplierStatement(long supplierId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable()
                .Where(x => x.SupplierId == supplierId)
                .WhereIF(fromDate.HasValue, x => x.TransactionDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.TransactionDate <= toDate.Value)
                .OrderBy(x => x.TransactionDate)
                .OrderBy(x => x.BalanceId)
                .ToList();
        }

        public decimal GetSupplierBalance(long supplierId)
        {
            var totalCredit = Queryable().Where(x => x.SupplierId == supplierId).Sum(x => x.CreditAmount);
            var totalDebit = Queryable().Where(x => x.SupplierId == supplierId).Sum(x => x.DebitAmount);
            return totalCredit - totalDebit;
        }

        public decimal GetSupplierTotalDebit(long supplierId) => Queryable().Where(x => x.SupplierId == supplierId).Sum(x => x.DebitAmount);
        public decimal GetSupplierTotalCredit(long supplierId) => Queryable().Where(x => x.SupplierId == supplierId).Sum(x => x.CreditAmount);

        public List<object> GetAgingReport(long? supplierId = null)
        {
            return Context.Queryable<ErpSupplierBalance>()
                .WhereIF(supplierId.HasValue, x => x.SupplierId == supplierId)
                .GroupBy(x => x.SupplierId)
                .Select(x => new
                {
                    SupplierId = x.SupplierId,
                    TotalDebit = SqlFunc.AggregateSum(x.DebitAmount),
                    TotalCredit = SqlFunc.AggregateSum(x.CreditAmount),
                    Balance = SqlFunc.AggregateSum(x.CreditAmount) - SqlFunc.AggregateSum(x.DebitAmount)
                })
                .ToList().Cast<object>().ToList();
        }

        public int RecalculateSupplierBalance(long supplierId)
        {
            return Context.Ado.UseTran(() =>
            {
                var transactions = GetSupplierStatement(supplierId, null, null);
                decimal runningBalance = 0;

                foreach (var trans in transactions)
                {
                    runningBalance += trans.CreditAmount - trans.DebitAmount;
                    Context.Updateable<ErpSupplierBalance>()
                        .SetColumns(it => new ErpSupplierBalance { Balance = runningBalance })
                        .Where(it => it.BalanceId == trans.BalanceId)
                        .ExecuteCommand();
                }

                return Context.Updateable<ErpSupplier>()
                    .SetColumns(it => new ErpSupplier { CurrentBalance = runningBalance, Update_time = DateTime.Now })
                    .Where(it => it.SupplierId == supplierId).ExecuteCommand();
            }).Data;
        }
    }


}

