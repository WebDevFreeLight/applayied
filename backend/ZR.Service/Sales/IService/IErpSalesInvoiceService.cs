using ZR.Model;
using ZR.Model.Sales;
using ZR.Service;

namespace ZR.Service.Sales.IService
{
    /// <summary>
    /// Sales Invoice Service Interface - خدمة فواتير المبيعات
    /// </summary>
    public interface IErpSalesInvoiceService : IBaseService<ErpSalesInvoice>
    {
        PagedInfo<ErpSalesInvoice> GetList(ErpSalesInvoice parm, PagerInfo pager);
        ErpSalesInvoice GetInfo(long invoiceId);
        ErpSalesInvoice CreateInvoice(ErpSalesInvoice invoice, List<ErpSalesInvoiceItem> items);
        int UpdateInvoice(ErpSalesInvoice invoice);
        int Delete(long[] ids);
        int ConfirmInvoice(long invoiceId, string userId);
        int CancelInvoice(long invoiceId, string reason);
        string GenerateInvoiceNumber(long? BranchID);
        List<ErpSalesInvoiceItem> GetInvoiceItems(long invoiceId);
        int AddInvoiceItem(ErpSalesInvoiceItem item);
        int UpdateInvoiceItem(ErpSalesInvoiceItem item);
        int DeleteInvoiceItem(long itemId);
        decimal CalculateInvoiceTotal(long invoiceId);
        decimal CalculateInvoiceTax(long invoiceId);
        List<ErpSalesInvoice> GetCustomerInvoices(long customerId, DateTime? fromDate, DateTime? toDate);
        List<ErpSalesInvoice> GetUnpaidInvoices(long? customerId = null);
        int ApplyPayment(long invoiceId, decimal amount, long paymentMethodId);
        decimal GetTotalSales(DateTime fromDate, DateTime toDate, long? BranchID = null);
        List<object> GetSalesByProduct(DateTime fromDate, DateTime toDate, long? BranchID = null);
        List<object> GetSalesByEmployee(DateTime fromDate, DateTime toDate, long? BranchID = null);
        Task<bool> SubmitToZatcaAsync(long invoiceId);
    }
}

