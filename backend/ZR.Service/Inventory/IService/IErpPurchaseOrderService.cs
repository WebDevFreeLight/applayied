using ZR.Model;
using ZR.Model.Inventory;
using ZR.Service;

namespace ZR.Service.Inventory.IService
{
    /// <summary>
    /// Purchase Order Service Interface - خدمة أوامر الشراء
    /// </summary>
    public interface IErpPurchaseOrderService : IBaseService<ErpPurchaseOrder>
    {
        PagedInfo<ErpPurchaseOrder> GetList(ErpPurchaseOrder parm, PagerInfo pager);
        ErpPurchaseOrder GetInfo(long purchaseOrderId);
        ErpPurchaseOrder AddPurchaseOrder(ErpPurchaseOrder model);
        int UpdatePurchaseOrder(ErpPurchaseOrder model);
        int Delete(long[] ids);
        string GeneratePurchaseOrderNumber(long? BranchID);
        int ApprovePurchaseOrder(long purchaseOrderId, string userId);
        int RejectPurchaseOrder(long purchaseOrderId, string reason);
        int CancelPurchaseOrder(long purchaseOrderId, string reason);
        int ReceivePurchaseOrder(long purchaseOrderId, List<ErpPurchaseOrderItem> receivedItems);
        List<ErpPurchaseOrder> GetSupplierPurchaseOrders(long supplierId, DateTime? fromDate, DateTime? toDate);
        List<ErpPurchaseOrder> GetPendingPurchaseOrders(long? BranchID = null);
        List<ErpPurchaseOrderItem> GetPurchaseOrderItems(long purchaseOrderId);
        decimal GetTotalPurchases(DateTime fromDate, DateTime toDate, long? supplierId = null);
        List<object> GetPurchaseReport(DateTime fromDate, DateTime toDate, long? BranchID = null);
    }
}

