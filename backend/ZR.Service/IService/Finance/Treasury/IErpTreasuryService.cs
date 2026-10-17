using ZR.Model;
using ZR.Model.Finance.Treasury;
using ZR.Service;

namespace ZR.Service.IService.Finance.Treasury
{
    public interface IErpTreasuryService : IBaseService<ErpTreasury>
    {
        PagedInfo<ErpTreasury> GetList(ErpTreasury parm, PagerInfo pager);
        ErpTreasury GetInfo(long treasuryId);
        ErpTreasury AddTreasury(ErpTreasury model);
        int UpdateTreasury(ErpTreasury model);
        int Delete(long[] ids);
        List<ErpTreasury> GetActiveTreasuries(long? BranchID = null);
        decimal GetTreasuryBalance(long treasuryId);
        List<ErpTreasuryTransaction> GetTransactions(long treasuryId, DateTime? fromDate, DateTime? toDate);
        int AddTransaction(ErpTreasuryTransaction transaction);
        int TransferBetweenTreasuries(long fromTreasuryId, long toTreasuryId, decimal amount, string description);
        List<object> GetDailyBalances(long treasuryId, DateTime fromDate, DateTime toDate);
    }

    public interface IErpTreasuryTransactionService : IBaseService<ErpTreasuryTransaction>
    {
        PagedInfo<ErpTreasuryTransaction> GetList(ErpTreasuryTransaction parm, PagerInfo pager);
        ErpTreasuryTransaction GetInfo(long transactionId);
        ErpTreasuryTransaction AddTransaction(ErpTreasuryTransaction model);
        int Delete(long[] ids);
        List<ErpTreasuryTransaction> GetByTreasury(long treasuryId, DateTime? fromDate, DateTime? toDate);
        decimal GetTotalReceipts(long treasuryId, DateTime fromDate, DateTime toDate);
        decimal GetTotalPayments(long treasuryId, DateTime fromDate, DateTime toDate);
    }

    public interface IErpTreasuryTransferService : IBaseService<ErpTreasuryTransfer>
    {
        PagedInfo<ErpTreasuryTransfer> GetList(ErpTreasuryTransfer parm, PagerInfo pager);
        ErpTreasuryTransfer GetInfo(long transferId);
        ErpTreasuryTransfer CreateTransfer(ErpTreasuryTransfer model);
        int ApproveTransfer(long transferId, string userId);
        int CancelTransfer(long transferId, string reason);
        List<ErpTreasuryTransfer> GetPendingTransfers(long? BranchID = null);
    }
}

