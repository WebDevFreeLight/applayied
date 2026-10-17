using ZR.Model;
using ZR.Model.Inventory;
using ZR.Service;

namespace ZR.Service.Inventory.IService
{
    /// <summary>
    /// Inventory Transaction Service Interface - خدمة حركات المخزون
    /// </summary>
    public interface IErpInventoryTransactionService : IBaseService<ErpInventoryTransaction>
    {
        PagedInfo<ErpInventoryTransaction> GetList(ErpInventoryTransaction parm, PagerInfo pager);
        ErpInventoryTransaction GetInfo(long transactionId);
        ErpInventoryTransaction CreateTransaction(ErpInventoryTransaction model);
        int Delete(long[] ids);
        List<ErpInventoryTransaction> GetProductTransactions(long productId, DateTime? fromDate, DateTime? toDate);
        List<ErpInventoryTransaction> GetWarehouseTransactions(long warehouseId, DateTime? fromDate, DateTime? toDate);
        int CreateOpeningBalance(long productId, long warehouseId, decimal quantity, decimal cost, string reference);
        int CreatePurchaseReceipt(long productId, long warehouseId, decimal quantity, decimal cost, long? purchaseId);
        int CreateSalesIssue(long productId, long warehouseId, decimal quantity, long? invoiceId);
        int CreateTransferOut(long productId, long warehouseId, decimal quantity, long transferId);
        int CreateTransferIn(long productId, long warehouseId, decimal quantity, long transferId);
        int CreateAdjustment(long productId, long warehouseId, decimal quantity, string reason);
        int CreateDamaged(long productId, long warehouseId, decimal quantity, string reason);
        List<object> GetInventoryReport(DateTime asOfDate, long? warehouseId = null);
        List<object> GetMovementReport(DateTime fromDate, DateTime toDate, long? productId = null);
    }
}

