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
    /// AP Invoice Controller - متحكم فواتير الموردين
    /// </summary>
    [Verify]
    [Route("finance/arap/ap-invoices")]
    [ApiController]
    public class APInvoiceController : BaseController
    {
        private readonly IAPInvoiceService _invoiceService;

        public APInvoiceController(IAPInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Get list of invoices - الحصول على قائمة الفواتير
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:list")]
        public IActionResult GetList([FromQuery] APInvoiceQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_invoiceService.GetList(query, pager));
        }

        /// <summary>
        /// Get invoice by ID - الحصول على الفاتورة بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:query")]
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
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:add")]
        [Log(Title = "AP Invoice", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateAPInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _invoiceService.Create(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update invoice - تحديث فاتورة
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:edit")]
        [Log(Title = "AP Invoice", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateAPInvoiceDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                return ToResponse(_invoiceService.Update(dto, HttpContext.GetName()));
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete invoices - حذف فواتير
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:delete")]
        [Log(Title = "AP Invoice", BusinessType = BusinessType.DELETE)]
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
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:query")]
        public IActionResult GetGLJournalData(Guid id)
        {
            var result = _invoiceService.PrepareGLJournalData(id);
            return SUCCESS(result);
        }

        /// <summary>
        /// Validate invoice for posting - التحقق من صحة الفاتورة للترحيل
        /// </summary>
        [HttpGet("{id}/validate-posting")]
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:query")]
        public IActionResult ValidateForPosting(Guid id)
        {
            var result = _invoiceService.ValidateForPosting(id);
            return SUCCESS(result);
        }

        /// <summary>
        /// Post invoice - ترحيل الفاتورة
        /// </summary>
        [HttpPost("{id}/post")]
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:edit")]
        [Log(Title = "AP Invoice Post", BusinessType = BusinessType.UPDATE)]
        public IActionResult PostInvoice(Guid id)
        {
            try
            {
                var result = _invoiceService.PostInvoice(id, HttpContext.GetName());
                if (!result.Success)
                    return ToResponse(ResultCode.FAIL, result.Message);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Cancel invoice - إلغاء الفاتورة
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:edit")]
        [Log(Title = "AP Invoice Cancel", BusinessType = BusinessType.UPDATE)]
        public IActionResult CancelInvoice(Guid id, [FromBody] CancelInvoiceRequest request)
        {
            try
            {
                var result = _invoiceService.CancelInvoice(id, request?.Reason ?? "Cancelled", HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get unpaid invoices for vendor - الحصول على الفواتير غير المدفوعة للمورد
        /// </summary>
        [HttpGet("unpaid")]
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:query")]
        public IActionResult GetUnpaidInvoices([FromQuery] Guid vendorId)
        {
            var result = _invoiceService.GetUnpaidInvoices(vendorId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get next invoice number - الحصول على رقم الفاتورة التالي
        /// </summary>
        [HttpGet("next-number")]
        [ActionPermissionFilter(Permission = "finance:arap:ap-invoice:query")]
        public IActionResult GetNextInvoiceNumber()
        {
            var result = _invoiceService.GetNextInvoiceNumber();
            return SUCCESS(new { InvoiceNumber = result });
        }
    }

    /// <summary>
    /// Cancel Invoice Request DTO
    /// </summary>
    public class CancelInvoiceRequest
    {
        public string Reason { get; set; }
    }
}


