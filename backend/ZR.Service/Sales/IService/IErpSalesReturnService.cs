using ZR.Model;
using ZR.Model.Sales;
using ZR.Service;

namespace ZR.Service.Sales.IService
{
    /// <summary>
    /// Sales Return Service Interface - خدمة مرتجعات المبيعات
    /// </summary>
    public interface IErpSalesReturnService : IBaseService<ErpSalesReturn>
    {
        PagedInfo<ErpSalesReturn> GetList(ErpSalesReturn parm, PagerInfo pager);
        ErpSalesReturn GetInfo(long returnId);
        ErpSalesReturn CreateReturn(ErpSalesReturn returnData, List<ErpSalesReturnItem> items);
        int UpdateReturn(ErpSalesReturn returnData);
        int Delete(long[] ids);
        int ConfirmReturn(long returnId, string userId);
        int CancelReturn(long returnId, string reason);
        string GenerateReturnNumber(long? BranchID);
        List<ErpSalesReturnItem> GetReturnItems(long returnId);
        int AddReturnItem(ErpSalesReturnItem item);
        int UpdateReturnItem(ErpSalesReturnItem item);
        int DeleteReturnItem(long itemId);
        decimal CalculateReturnTotal(long returnId);
        List<ErpSalesReturn> GetCustomerReturns(long customerId, DateTime? fromDate, DateTime? toDate);
        List<ErpSalesReturn> GetReturnsByInvoice(long invoiceId);
        decimal GetTotalReturns(DateTime fromDate, DateTime toDate, long? BranchID = null);
        int ProcessRefund(long returnId, long paymentMethodId, long? treasuryId);
    }
}

