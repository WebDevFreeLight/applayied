using Microsoft.AspNetCore.Mvc;
using ZR.Model.Sales;
using ZR.Service.Sales.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Sales
{
    /// <summary>
    /// Sales Return Controller - متحكم مرتجعات المبيعات
    /// </summary>
    [Verify]
    [Route("api/erp/sales-returns")]
    [ApiController]
    public class ErpSalesReturnController : BaseController
    {
        private readonly IErpSalesReturnService _returnService;

        public ErpSalesReturnController(IErpSalesReturnService returnService)
        {
            _returnService = returnService;
        }

        /// <summary>
        /// Get return list - قائمة المرتجعات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:return:list")]
        public IActionResult GetList([FromQuery] ErpSalesReturn parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _returnService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get return by ID - الحصول على مرتجع
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:return:query")]
        public IActionResult GetInfo(long id)
        {
            var returnData = _returnService.GetInfo(id);
            if (returnData == null)
                return ToResponse(ResultCode.FAIL, "Return not found - المرتجع غير موجود");

            var items = _returnService.GetReturnItems(id);
            return SUCCESS(new { returnData, items });
        }

        /// <summary>
        /// Create return - إنشاء مرتجع
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:return:add")]
        [Log(Title = "Sales Return", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateReturnRequest request)
        {
            var result = _returnService.CreateReturn(request.Return, request.Items);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update return - تعديل مرتجع
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:return:edit")]
        [Log(Title = "Sales Return", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpSalesReturn returnData)
        {
            var result = _returnService.UpdateReturn(returnData);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete returns - حذف مرتجعات
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:return:delete")]
        [Log(Title = "Sales Return", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_returnService.Delete(idsArr));
        }

        /// <summary>
        /// Confirm return - تأكيد المرتجع
        /// </summary>
        [HttpPost("{id}/confirm")]
        [ActionPermissionFilter(Permission = "erp:return:confirm")]
        [Log(Title = "Sales Return", BusinessType = BusinessType.UPDATE)]
        public IActionResult Confirm(long id)
        {
            var result = _returnService.ConfirmReturn(id, HttpContext.GetUId().ToString());
            return ToResponse(result);
        }

        /// <summary>
        /// Cancel return - إلغاء المرتجع
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ActionPermissionFilter(Permission = "erp:return:cancel")]
        [Log(Title = "Sales Return", BusinessType = BusinessType.UPDATE)]
        public IActionResult Cancel(long id, [FromBody] CancelRequest request)
        {
            var result = _returnService.CancelReturn(id, request.Reason);
            return ToResponse(result);
        }

        /// <summary>
        /// Process refund - معالجة الاسترداد
        /// </summary>
        [HttpPost("{id}/refund")]
        [ActionPermissionFilter(Permission = "erp:return:refund")]
        [Log(Title = "Sales Return Refund", BusinessType = BusinessType.UPDATE)]
        public IActionResult ProcessRefund(long id, [FromBody] RefundRequest request)
        {
            var result = _returnService.ProcessRefund(id, request.PaymentMethodId, request.TreasuryId);
            return ToResponse(result);
        }

        /// <summary>
        /// Add return item - إضافة صنف للمرتجع
        /// </summary>
        [HttpPost("{id}/items")]
        [ActionPermissionFilter(Permission = "erp:return:edit")]
        public IActionResult AddItem(long id, [FromBody] ErpSalesReturnItem item)
        {
            item.ReturnId = id;
            var result = _returnService.AddReturnItem(item);
            return ToResponse(result);
        }

        /// <summary>
        /// Update return item - تعديل صنف
        /// </summary>
        [HttpPut("items/{itemId}")]
        [ActionPermissionFilter(Permission = "erp:return:edit")]
        public IActionResult UpdateItem([FromBody] ErpSalesReturnItem item)
        {
            var result = _returnService.UpdateReturnItem(item);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete return item - حذف صنف
        /// </summary>
        [HttpDelete("items/{itemId}")]
        [ActionPermissionFilter(Permission = "erp:return:edit")]
        public IActionResult DeleteItem(long itemId)
        {
            var result = _returnService.DeleteReturnItem(itemId);
            return ToResponse(result);
        }

        /// <summary>
        /// Get customer returns - مرتجعات العميل
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ActionPermissionFilter(Permission = "erp:return:list")]
        public IActionResult GetCustomerReturns(long customerId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = _returnService.GetCustomerReturns(customerId, fromDate, toDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get returns by invoice - مرتجعات الفاتورة
        /// </summary>
        [HttpGet("invoice/{invoiceId}")]
        [ActionPermissionFilter(Permission = "erp:return:list")]
        public IActionResult GetReturnsByInvoice(long invoiceId)
        {
            var result = _returnService.GetReturnsByInvoice(invoiceId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get total returns - إجمالي المرتجعات
        /// </summary>
        [HttpGet("reports/total")]
        [ActionPermissionFilter(Permission = "erp:return:report")]
        public IActionResult GetTotalReturns([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] long? BranchID)
        {
            var total = _returnService.GetTotalReturns(fromDate, toDate, BranchID);
            return SUCCESS(new { Total = total, FromDate = fromDate, ToDate = toDate });
        }

        /// <summary>
        /// Export returns - تصدير المرتجعات
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:return:export")]
        [Log(Title = "Sales Return", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpSalesReturn parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _returnService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "SalesReturns", "Sales Returns - مرتجعات المبيعات");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class CreateReturnRequest
    {
        public ErpSalesReturn Return { get; set; }
        public List<ErpSalesReturnItem> Items { get; set; }
    }

    public class RefundRequest
    {
        public long PaymentMethodId { get; set; }
        public long? TreasuryId { get; set; }
    }
}

