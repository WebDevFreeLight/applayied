using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Service.IService.Finance.ARAP;

namespace ZR.Admin.WebApi.Controllers.Finance.ARAP
{
    /// <summary>
    /// Credit/Debit Note Controller - متحكم الإشعارات
    /// </summary>
    [Verify]
    [Route("finance/arap/notes")]
    [ApiController]
    public class CreditDebitNoteController : BaseController
    {
        private readonly IAdjustmentService _adjustmentService;

        public CreditDebitNoteController(IAdjustmentService adjustmentService)
        {
            _adjustmentService = adjustmentService;
        }

        /// <summary>
        /// Get list of notes - الحصول على قائمة الإشعارات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:arap:note:list")]
        public IActionResult GetList([FromQuery] CreditDebitNoteQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_adjustmentService.GetList(query, pager));
        }

        /// <summary>
        /// Get note by ID - الحصول على الإشعار بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:arap:note:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _adjustmentService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Note not found - الإشعار غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create note - إنشاء إشعار
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:arap:note:add")]
        [Log(Title = "Credit/Debit Note", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateCreditDebitNoteDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _adjustmentService.Create(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Apply adjustment to invoice - تطبيق التعديل على الفاتورة
        /// </summary>
        [HttpPost("{noteId}/apply-to-invoice/{invoiceId}")]
        [ActionPermissionFilter(Permission = "finance:arap:note:edit")]
        [Log(Title = "Apply Adjustment", BusinessType = BusinessType.UPDATE)]
        public IActionResult ApplyToInvoice(Guid noteId, Guid invoiceId)
        {
            try
            {
                _adjustmentService.ApplyAdjustmentToInvoice(noteId, invoiceId);
                return SUCCESS(new { Success = true, Message = "Adjustment applied successfully - تم تطبيق التعديل بنجاح" });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Prepare GL journal data - إعداد بيانات قيد اليومية
        /// </summary>
        [HttpGet("{id}/gl-journal-data")]
        [ActionPermissionFilter(Permission = "finance:arap:note:query")]
        public IActionResult GetGLJournalData(Guid id)
        {
            var result = _adjustmentService.PrepareGLJournalData(id);
            return SUCCESS(result);
        }
    }
}


