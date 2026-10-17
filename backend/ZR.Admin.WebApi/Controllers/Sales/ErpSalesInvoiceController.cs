using Microsoft.AspNetCore.Mvc;
using ZR.Model.Sales;
using ZR.Service.Sales.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Sales
{
    /// <summary>
    /// Sales Invoice Controller - متحكم فواتير المبيعات
    /// </summary>
    [Verify]
    [Route("api/erp/sales-invoices")]
    [ApiController]
    public class ErpSalesInvoiceController : BaseController
    {
        private readonly IErpSalesInvoiceService _invoiceService;

        public ErpSalesInvoiceController(IErpSalesInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Get invoice list - قائمة الفواتير
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:invoice:list")]
        public IActionResult GetList([FromQuery] ErpSalesInvoice parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _invoiceService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get invoice by ID - الحصول على فاتورة
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:invoice:query")]
        public IActionResult GetInfo(long id)
        {
            var invoice = _invoiceService.GetInfo(id);
            if (invoice == null)
                return ToResponse(ResultCode.FAIL, "Invoice not found - الفاتورة غير موجودة");

            var items = _invoiceService.GetInvoiceItems(id);
            return SUCCESS(new { invoice, items });
        }

        /// <summary>
        /// Create invoice - إنشاء فاتورة
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:invoice:add")]
        [Log(Title = "Sales Invoice", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateInvoiceRequest request)
        {
            var result = _invoiceService.CreateInvoice(request.Invoice, request.Items);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update invoice - تعديل فاتورة
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:invoice:edit")]
        [Log(Title = "Sales Invoice", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpSalesInvoice invoice)
        {
            var result = _invoiceService.UpdateInvoice(invoice);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete invoices - حذف فواتير
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:invoice:delete")]
        [Log(Title = "Sales Invoice", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_invoiceService.Delete(idsArr));
        }

        /// <summary>
        /// Confirm invoice - تأكيد الفاتورة
        /// </summary>
        [HttpPost("{id}/confirm")]
        [ActionPermissionFilter(Permission = "erp:invoice:confirm")]
        [Log(Title = "Sales Invoice", BusinessType = BusinessType.UPDATE)]
        public IActionResult Confirm(long id)
        {
            var result = _invoiceService.ConfirmInvoice(id, HttpContext.GetUId().ToString());
            return ToResponse(result);
        }

        /// <summary>
        /// Cancel invoice - إلغاء الفاتورة
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ActionPermissionFilter(Permission = "erp:invoice:cancel")]
        [Log(Title = "Sales Invoice", BusinessType = BusinessType.UPDATE)]
        public IActionResult Cancel(long id, [FromBody] CancelRequest request)
        {
            var result = _invoiceService.CancelInvoice(id, request.Reason);
            return ToResponse(result);
        }

        /// <summary>
        /// Add invoice item - إضافة صنف للفاتورة
        /// </summary>
        [HttpPost("{id}/items")]
        [ActionPermissionFilter(Permission = "erp:invoice:edit")]
        public IActionResult AddItem(long id, [FromBody] ErpSalesInvoiceItem item)
        {
            item.InvoiceId = id;
            var result = _invoiceService.AddInvoiceItem(item);
            return ToResponse(result);
        }

        /// <summary>
        /// Update invoice item - تعديل صنف
        /// </summary>
        [HttpPut("items/{itemId}")]
        [ActionPermissionFilter(Permission = "erp:invoice:edit")]
        public IActionResult UpdateItem([FromBody] ErpSalesInvoiceItem item)
        {
            var result = _invoiceService.UpdateInvoiceItem(item);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete invoice item - حذف صنف
        /// </summary>
        [HttpDelete("items/{itemId}")]
        [ActionPermissionFilter(Permission = "erp:invoice:edit")]
        public IActionResult DeleteItem(long itemId)
        {
            var result = _invoiceService.DeleteInvoiceItem(itemId);
            return ToResponse(result);
        }

        /// <summary>
        /// Apply payment - تطبيق دفعة
        /// </summary>
        [HttpPost("{id}/payment")]
        [ActionPermissionFilter(Permission = "erp:invoice:payment")]
        [Log(Title = "Sales Invoice Payment", BusinessType = BusinessType.UPDATE)]
        public IActionResult ApplyPayment(long id, [FromBody] PaymentRequest request)
        {
            var result = _invoiceService.ApplyPayment(id, request.Amount, request.PaymentMethodId);
            return ToResponse(result);
        }

        /// <summary>
        /// Get unpaid invoices - الفواتير غير المدفوعة
        /// </summary>
        [HttpGet("unpaid")]
        [ActionPermissionFilter(Permission = "erp:invoice:list")]
        public IActionResult GetUnpaidInvoices([FromQuery] long? customerId)
        {
            var result = _invoiceService.GetUnpaidInvoices(customerId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get customer invoices - فواتير العميل
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ActionPermissionFilter(Permission = "erp:invoice:list")]
        public IActionResult GetCustomerInvoices(long customerId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = _invoiceService.GetCustomerInvoices(customerId, fromDate, toDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get total sales - إجمالي المبيعات
        /// </summary>
        [HttpGet("reports/total-sales")]
        [ActionPermissionFilter(Permission = "erp:invoice:report")]
        public IActionResult GetTotalSales([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] long? BranchID)
        {
            var total = _invoiceService.GetTotalSales(fromDate, toDate, BranchID);
            return SUCCESS(new { Total = total, FromDate = fromDate, ToDate = toDate });
        }

        /// <summary>
        /// Get sales by product - المبيعات حسب المنتج
        /// </summary>
        [HttpGet("reports/by-product")]
        [ActionPermissionFilter(Permission = "erp:invoice:report")]
        public IActionResult GetSalesByProduct([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] long? BranchID)
        {
            var result = _invoiceService.GetSalesByProduct(fromDate, toDate, BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get sales by employee - المبيعات حسب الموظف
        /// </summary>
        [HttpGet("reports/by-employee")]
        [ActionPermissionFilter(Permission = "erp:invoice:report")]
        public IActionResult GetSalesByEmployee([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] long? BranchID)
        {
            var result = _invoiceService.GetSalesByEmployee(fromDate, toDate, BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Submit to ZATCA - إرسال لهيئة الزكاة
        /// </summary>
        [HttpPost("{id}/zatca")]
        [ActionPermissionFilter(Permission = "erp:invoice:zatca")]
        [Log(Title = "ZATCA Submit", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> SubmitToZatca(long id)
        {
            var result = await _invoiceService.SubmitToZatcaAsync(id);
            return SUCCESS(new { Success = result });
        }

        /// <summary>
        /// Export invoices - تصدير الفواتير
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:invoice:export")]
        [Log(Title = "Sales Invoice", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpSalesInvoice parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _invoiceService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "SalesInvoices", "Sales Invoices - فواتير المبيعات");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class CreateInvoiceRequest
    {
        public ErpSalesInvoice Invoice { get; set; }
        public List<ErpSalesInvoiceItem> Items { get; set; }
    }

    public class CancelRequest
    {
        public string Reason { get; set; }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public long PaymentMethodId { get; set; }
    }
}

