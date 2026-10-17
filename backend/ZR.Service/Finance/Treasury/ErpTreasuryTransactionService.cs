using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZR.Model.Finance.Treasury;
using ZR.Repository;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Service.Finance.Treasury
{

    [AppService(ServiceType = typeof(IErpTreasuryTransactionService), ServiceLifetime = LifeTime.Transient)]
    public class ErpTreasuryTransactionService : BaseService<ErpTreasuryTransaction>, IErpTreasuryTransactionService
    {

        public PagedInfo<ErpTreasuryTransaction> GetList(ErpTreasuryTransaction parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpTreasuryTransaction>();
            predicate = predicate.AndIF(parm.TreasuryId > 0, it => it.TreasuryId == parm.TreasuryId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TransactionType), it => it.TransactionType == parm.TransactionType);
            return Queryable().Where(predicate.ToExpression()).OrderByDescending(x => x.TransactionDate).ToPage(pager);
        }

        public ErpTreasuryTransaction GetInfo(long transactionId) => GetFirst(x => x.TransactionId == transactionId);

        /// <summary>
        /// OBSOLETE: Transactions should be recorded through GL Journal Entries
        /// </summary>
        [Obsolete("Transactions should be recorded through GL Journal Entries")]
        public ErpTreasuryTransaction AddTransaction(ErpTreasuryTransaction model)
        {
            throw new Exception("OBSOLETE: Transactions should be recorded through GL Journal Entries - يجب تسجيل المعاملات من خلال قيود اليومية");
        }

        public int Delete(long[] ids) => 0; // Transactions should not be deleted

        public List<ErpTreasuryTransaction> GetByTreasury(long treasuryId, DateTime? fromDate, DateTime? toDate)
        {
            return Queryable().Where(x => x.TreasuryId == treasuryId)
                .WhereIF(fromDate.HasValue, x => x.TransactionDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.TransactionDate <= toDate.Value).ToList();
        }

        public decimal GetTotalReceipts(long treasuryId, DateTime fromDate, DateTime toDate) => Queryable().Where(x => x.TreasuryId == treasuryId && x.TransactionType == "receipt" && x.TransactionDate >= fromDate && x.TransactionDate <= toDate).Sum(x => x.Amount);
        public decimal GetTotalPayments(long treasuryId, DateTime fromDate, DateTime toDate) => Queryable().Where(x => x.TreasuryId == treasuryId && x.TransactionType == "payment" && x.TransactionDate >= fromDate && x.TransactionDate <= toDate).Sum(x => x.Amount);
    }


}

