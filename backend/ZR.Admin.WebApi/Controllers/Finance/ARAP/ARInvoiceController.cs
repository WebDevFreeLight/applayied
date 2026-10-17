using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Service.IService.Finance.ARAP;

namespace ZR.Admin.WebApi.Controllers.Finance.ARAP
{
    /// <summary>
    /// AR Invoice Controller - متحكم فواتير العملاء
    /// </summary>
    [Verify]
    [Route("finance/arap/ar-invoices")]
    [ApiController]
    public class ARInvoiceController : BaseController
    {
        private readonly IARInvoiceService _invoiceService;

        public ARInvoiceController(IARInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Get list of invoices - الحصول على قائمة الفواتير
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:list")]
        public IActionResult GetList([FromQuery] ARInvoiceQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_invoiceService.GetList(query, pager));
        }

        /// <summary>
        /// Get invoice by ID - الحصول على الفاتورة بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _invoiceService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Invoice not found - الفاتورة غير موجودة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create invoice - إنشاء فاتورة
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:add")]
        [Log(Title = "AR Invoice", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateARInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var result = _invoiceService.Create(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Update invoice - تحديث فاتورة
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:edit")]
        [Log(Title = "AR Invoice", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateARInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            return ToResponse(_invoiceService.Update(dto, HttpContext.GetName()));
        }

        /// <summary>
        /// Delete invoices - حذف فواتير
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:delete")]
        [Log(Title = "AR Invoice", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idsArr = ids.Split(',').Select(Guid.Parse).ToArray();
            if (idsArr.Length <= 0)
                return ToResponse(ResultCode.FAIL);

            var result = _invoiceService.Delete(idsArr);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Cannot delete invoices with payments - لا يمكن حذف فواتير لها مدفوعات");

            return ToResponse(result);
        }

        /// <summary>
        /// Prepare GL journal data - إعداد بيانات قيد اليومية
        /// </summary>
        [HttpGet("{id}/gl-journal-data")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:query")]
        public IActionResult GetGLJournalData(Guid id)
        {
            var result = _invoiceService.PrepareGLJournalData(id);
            return SUCCESS(result);
        }

        /// <summary>
        /// Validate invoice for posting - التحقق من صحة الفاتورة للترحيل
        /// </summary>
        [HttpGet("{id}/validate-posting")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:query")]
        public IActionResult ValidateForPosting(Guid id)
        {
            var result = _invoiceService.ValidateForPosting(id);
            return SUCCESS(result);
        }

        /// <summary>
        /// Post invoice - ترحيل الفاتورة
        /// </summary>
        [HttpPost("{id}/post")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:edit")]
        [Log(Title = "AR Invoice Post", BusinessType = BusinessType.UPDATE)]
        public IActionResult PostInvoice(Guid id)
        {
            var result = _invoiceService.PostInvoice(id, HttpContext.GetName());
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        /// <summary>
        /// Cancel invoice - إلغاء الفاتورة
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:edit")]
        [Log(Title = "AR Invoice Cancel", BusinessType = BusinessType.UPDATE)]
        public IActionResult CancelInvoice(Guid id, [FromBody] ARCancelInvoiceRequest request)
        {
            var result = _invoiceService.CancelInvoice(id, request?.Reason ?? "Cancelled", HttpContext.GetName());
            return ToResponse(result);
        }

        /// <summary>
        /// Get unpaid invoices for customer - الحصول على الفواتير غير المدفوعة للعميل
        /// </summary>
        [HttpGet("unpaid")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:query")]
        public IActionResult GetUnpaidInvoices([FromQuery] Guid customerId)
        {
            var result = _invoiceService.GetUnpaidInvoices(customerId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get next invoice number (thread-safe) - الحصول على رقم الفاتورة التالي (آمن للخيوط)
        /// </summary>
        /// <param name="companyId">Optional company ID for multi-tenant</param>
        [HttpGet("next-number")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:query")]
        public IActionResult GetNextInvoiceNumber([FromQuery] long? companyId = null)
        {
            var result = _invoiceService.GetNextInvoiceNumber(companyId);
            return SUCCESS(new { InvoiceNumber = result });
        }

        /// <summary>
        /// Check if invoice number exists - التحقق إذا كان رقم الفاتورة موجود
        /// Returns: { "code": 200, "data": { "exists": true|false }, "msg": null }
        /// </summary>
        /// <param name="invoiceNumber">Invoice number to check</param>
        /// <param name="excludeId">Optional invoice ID to exclude (for updates)</param>
        [HttpGet("check-code")]
        [ActionPermissionFilter(Permission = "finance:arap:ar-invoice:query")]
        public IActionResult CheckInvoiceNumberExists([FromQuery] string invoiceNumber, [FromQuery] Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                return SUCCESS(new { Exists = false });

            var result = _invoiceService.CheckInvoiceNumberExists(invoiceNumber, excludeId);
            return SUCCESS(result);
        }
    }

    /// <summary>
    /// AR Cancel Invoice Request DTO
    /// </summary>
    public class ARCancelInvoiceRequest
    {
        public string Reason { get; set; }
    }
}


