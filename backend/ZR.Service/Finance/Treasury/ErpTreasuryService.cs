using System;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Treasury;
using ZR.Repository;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Service.Finance.Treasury
{
    [AppService(ServiceType = typeof(IErpTreasuryService), ServiceLifetime = LifeTime.Transient)]
    public class ErpTreasuryService : BaseService<ErpTreasury>, IErpTreasuryService
    {
      

        public PagedInfo<ErpTreasury> GetList(ErpTreasury parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<ErpTreasury>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.TreasuryName), it => it.TreasuryName.Contains(parm.TreasuryName));
            predicate = predicate.AndIF(parm.BranchID > 0, it => it.BranchID == parm.BranchID);
            predicate = predicate.And(it => it.IsActive == true);
            return Queryable().Where(predicate.ToExpression()).OrderBy(x => x.TreasuryName).ToPage(pager);
        }

        public ErpTreasury GetInfo(long treasuryId) => GetFirst(x => x.TreasuryId == treasuryId);

        public ErpTreasury AddTreasury(ErpTreasury model)
        {
            model.TreasuryId = SnowFlakeSingle.Instance.NextId();
            Insert(model);
            return model;
        }

        public int UpdateTreasury(ErpTreasury model) => Update(model, it => new { it.TreasuryName, it.TreasuryNameAr, it.ResponsibleEmployeeId, it.Update_by, it.Update_time }, f => f.TreasuryId == model.TreasuryId);

        public int Delete(long[] ids) => Context.Updateable<ErpTreasury>().SetColumns(it => it.IsActive == false).Where(it => ids.Contains(it.TreasuryId)).ExecuteCommand();

        public List<ErpTreasury> GetActiveTreasuries(long? BranchID = null) => Queryable().Where(x => x.IsActive == true).WhereIF(BranchID.HasValue, x => x.BranchID == BranchID).ToList();

        /// <summary>
        /// OBSOLETE: Balance should be retrieved from GLAccountOpeningBalance and computed from journal entries
        /// </summary>
        [Obsolete("Balance should be retrieved from GLAccountOpeningBalance and computed from journal entries")]
        public decimal GetTreasuryBalance(long treasuryId)
        {
            throw new Exception("OBSOLETE: Use GLAccountOpeningBalance to retrieve treasury balance - استخدم GLAccountOpeningBalance لاسترجاع رصيد الخزينة");
        }

        public List<ErpTreasuryTransaction> GetTransactions(long treasuryId, DateTime? fromDate, DateTime? toDate)
        {
            return Context.Queryable<ErpTreasuryTransaction>().Where(x => x.TreasuryId == treasuryId)
                .WhereIF(fromDate.HasValue, x => x.TransactionDate >= fromDate.Value)
                .WhereIF(toDate.HasValue, x => x.TransactionDate <= toDate.Value)
                .OrderByDescending(x => x.TransactionDate).ToList();
        }

        /// <summary>
        /// OBSOLETE: Transactions should be recorded through GL Journal Entries
        /// </summary>
        [Obsolete("Transactions should be recorded through GL Journal Entries")]
        public int AddTransaction(ErpTreasuryTransaction transaction)
        {
            throw new Exception("OBSOLETE: Transactions should be recorded through GL Journal Entries - يجب تسجيل المعاملات من خلال قيود اليومية");
        }

        /// <summary>
        /// OBSOLETE: Transfers should be recorded through GL Journal Entries
        /// </summary>
        [Obsolete("Transfers should be recorded through GL Journal Entries")]
        public int TransferBetweenTreasuries(long fromTreasuryId, long toTreasuryId, decimal amount, string description)
        {
            throw new Exception("OBSOLETE: Transfers should be recorded through GL Journal Entries - يجب تسجيل التحويلات من خلال قيود اليومية");
        }

        public List<object> GetDailyBalances(long treasuryId, DateTime fromDate, DateTime toDate)
        {
            return Context.Queryable<ErpTreasuryTransaction>().Where(x => x.TreasuryId == treasuryId && x.TransactionDate >= fromDate && x.TransactionDate <= toDate)
                .GroupBy(x => x.TransactionDate.Date)
                .Select(x => new { Date = x.TransactionDate.Date, Receipts = SqlFunc.AggregateSum(SqlFunc.IIF(x.TransactionType == "receipt", x.Amount, 0)), Payments = SqlFunc.AggregateSum(SqlFunc.IIF(x.TransactionType == "payment", x.Amount, 0)) })
                .ToList().Cast<object>().ToList();
        }
    }



}

