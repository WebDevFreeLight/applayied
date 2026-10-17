using Microsoft.AspNetCore.Mvc;
using ZR.Model.Sales;
using ZR.Service.Sales.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Sales
{
    /// <summary>
    /// Advance Payment Controller - متحكم الدفعات المقدمة
    /// </summary>
    [Verify]
    [Route("api/erp/advance-payments")]
    [ApiController]
    public class ErpAdvancePaymentController : BaseController
    {
        private readonly IErpAdvancePaymentService _advanceService;

        public ErpAdvancePaymentController(IErpAdvancePaymentService advanceService)
        {
            _advanceService = advanceService;
        }

        /// <summary>
        /// Get advance payment list - قائمة الدفعات المقدمة
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:advance:list")]
        public IActionResult GetList([FromQuery] ErpAdvancePayment parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _advanceService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get advance payment by ID - الحصول على دفعة مقدمة
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:advance:query")]
        public IActionResult GetInfo(long id)
        {
            var response = _advanceService.GetInfo(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Advance payment not found - الدفعة المقدمة غير موجودة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create advance payment - إنشاء دفعة مقدمة
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:advance:add")]
        [Log(Title = "Advance Payment", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] ErpAdvancePayment model)
        {
            var result = _advanceService.AddAdvancePayment(model);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update advance payment - تعديل دفعة مقدمة
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:advance:edit")]
        [Log(Title = "Advance Payment", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpAdvancePayment model)
        {
            var result = _advanceService.UpdateAdvancePayment(model);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete advance payments - حذف دفعات مقدمة
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:advance:delete")]
        [Log(Title = "Advance Payment", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_advanceService.Delete(idsArr));
        }

        /// <summary>
        /// Use advance payment - استخدام الدفعة المقدمة
        /// </summary>
        [HttpPost("{id}/use")]
        [ActionPermissionFilter(Permission = "erp:advance:apply")]
        [Log(Title = "Use Advance Payment", BusinessType = BusinessType.UPDATE)]
        public IActionResult UseAdvance(long id, [FromBody] UseAdvanceRequest request)
        {
            var result = _advanceService.UseAdvance(id, request.Amount, request.ReferenceId, request.ReferenceType);
            return ToResponse(result);
        }

        /// <summary>
        /// Refund advance payment - استرداد الدفعة المقدمة
        /// </summary>
        [HttpPost("{id}/refund")]
        [ActionPermissionFilter(Permission = "erp:advance:refund")]
        [Log(Title = "Refund Advance Payment", BusinessType = BusinessType.UPDATE)]
        public IActionResult Refund(long id, [FromBody] RefundAdvanceRequest request)
        {
            var result = _advanceService.RefundAdvance(id, request.Amount, request.Reason);
            return ToResponse(result);
        }

        /// <summary>
        /// Get customer advances - الدفعات المقدمة للعميل
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [ActionPermissionFilter(Permission = "erp:advance:list")]
        public IActionResult GetCustomerAdvances(long customerId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var result = _advanceService.GetCustomerAdvances(customerId, fromDate, toDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get unused advances - الدفعات المقدمة غير المستخدمة
        /// </summary>
        [HttpGet("unused")]
        [ActionPermissionFilter(Permission = "erp:advance:list")]
        public IActionResult GetUnusedAdvances([FromQuery] long? customerId)
        {
            var result = _advanceService.GetUnusedAdvances(customerId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get customer balance - رصيد العميل من الدفعات المقدمة
        /// </summary>
        [HttpGet("customer/{customerId}/balance")]
        [ActionPermissionFilter(Permission = "erp:advance:list")]
        public IActionResult GetCustomerBalance(long customerId)
        {
            var balance = _advanceService.GetCustomerAdvanceBalance(customerId);
            return SUCCESS(new { CustomerId = customerId, Balance = balance });
        }

        /// <summary>
        /// Export advance payments - تصدير الدفعات المقدمة
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:advance:export")]
        [Log(Title = "Advance Payment", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpAdvancePayment parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _advanceService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "AdvancePayments", "Advance Payments - الدفعات المقدمة");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class UseAdvanceRequest
    {
        public decimal Amount { get; set; }
        public long ReferenceId { get; set; }
        public string ReferenceType { get; set; }
    }

    public class RefundAdvanceRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; }
    }
}

