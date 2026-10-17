using System;
using System.Linq;
using Infrastructure.Model;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Exceptions;
using ZR.Service.IService.Finance.GL;
using ZR.Service.Finance.Validation;

namespace ZR.Admin.WebApi.Controllers.Finance.GL
{
    /// <summary>
    /// GL Journal Controller - متحكم دفتر اليومية
    /// </summary>
    [Verify]
    [Route("finance/journals")]
    [ApiController]
    public class GLJournalController : BaseController
    {
        private readonly IGLJournalService _journalService;
        private readonly IJournalValidationService _validationService;

        public GLJournalController(
            IGLJournalService journalService,
            IJournalValidationService validationService)
        {
            _journalService = journalService;
            _validationService = validationService;
        }

        /// <summary>
        /// Helper method to create error response with period exception details
        /// </summary>
        private IActionResult ToPeriodErrorResponse(BasePeriodException ex)
        {
            return ToResponse(new ApiResult((int)ResultCode.FAIL, ex.MessageEn, new
            {
                errorCode = ex.ErrorCode,
                messageAr = ex.MessageAr,
                messageEn = ex.MessageEn,
                date = ex.Date
            }));
        }

        /// <summary>
        /// Get list of journals - الحصول على قائمة دفاتر اليومية
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:journal:list")]
        public IActionResult GetList([FromQuery] GLJournalQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_journalService.GetList(query, pager));
        }

        /// <summary>
        /// Get journal by ID - الحصول على دفتر اليومية بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:journal:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _journalService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Journal not found - دفتر اليومية غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Get journal with entries - الحصول على دفتر اليومية مع القيود
        /// </summary>
        [HttpGet("{id}/with-entries")]
        [ActionPermissionFilter(Permission = "finance:journal:query")]
        public IActionResult GetWithEntries(Guid id)
        {
            var response = _journalService.GetWithEntries(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Journal not found - دفتر اليومية غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Validate journal before creation - التحقق من القيد قبل الإنشاء
        /// </summary>
        [HttpPost("validate")]
        [ActionPermissionFilter(Permission = "finance:journal:add")]
        public IActionResult ValidateJournal([FromBody] CreateJournalDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var validation = _validationService.ValidateForCreate(dto);
            return SUCCESS(new
            {
                validation.IsValid,
                validation.Errors,
                validation.Warnings,
                validation.TotalDebit,
                validation.TotalCredit,
                validation.IsBalanced,
                validation.Difference
            });
        }

        /// <summary>
        /// Create journal - إنشاء دفتر يومية
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:journal:add")]
        [Log(Title = "GL Journal", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateJournalDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _journalService.Create(dto, HttpContext.GetName());
                if (result == null)
                    return ToResponse(ResultCode.FAIL, "Failed to create journal - فشل في إنشاء دفتر اليومية");

                return SUCCESS(result);
            }
            catch (BasePeriodException ex)
            {
                return ToPeriodErrorResponse(ex);
            }
            catch (Exception ex)
            {
                // Return validation errors with details
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update journal - تحديث دفتر يومية
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:journal:edit")]
        [Log(Title = "GL Journal", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateJournalDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _journalService.Update(dto, HttpContext.GetName());
                if (result == 0)
                    return ToResponse(ResultCode.FAIL, "Cannot update posted journal - لا يمكن تحديث دفتر يومية مرحّل");

                return ToResponse(result);
            }
            catch (BasePeriodException ex)
            {
                return ToPeriodErrorResponse(ex);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete journals - حذف دفاتر يومية
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:journal:delete")]
        [Log(Title = "GL Journal", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idsArr = ids.Split(',').Select(Guid.Parse).ToArray();
            if (idsArr.Length <= 0)
                return ToResponse(ResultCode.FAIL);

            try
            {
                var result = _journalService.Delete(idsArr);
                if (result == 0)
                    return ToResponse(ResultCode.FAIL, "Only draft journals can be deleted - يمكن حذف دفاتر اليومية بحالة مسودة فقط");

                return ToResponse(result);
            }
            catch (BasePeriodException ex)
            {
                return ToPeriodErrorResponse(ex);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Post journal - ترحيل دفتر يومية
        /// </summary>
        [HttpPost("{id}/post")]
        [ActionPermissionFilter(Permission = "finance:journal:post")]
        [Log(Title = "GL Journal Post", BusinessType = BusinessType.UPDATE)]
        public IActionResult PostJournal(Guid id)
        {
            try
            {
                var result = _journalService.PostJournal(id, HttpContext.GetName());
                if (result == 0)
                    return ToResponse(ResultCode.FAIL, "Cannot post journal. Journal must be draft and balanced - لا يمكن ترحيل دفتر اليومية. يجب أن يكون بحالة مسودة ومتوازن");

                return ToResponse(result);
            }
            catch (BasePeriodException ex)
            {
                return ToPeriodErrorResponse(ex);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Cancel journal - إلغاء دفتر يومية
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ActionPermissionFilter(Permission = "finance:journal:cancel")]
        [Log(Title = "GL Journal Cancel", BusinessType = BusinessType.UPDATE)]
        public IActionResult CancelJournal(Guid id)
        {
            try
            {
                var result = _journalService.CancelJournal(id, HttpContext.GetName());
                if (result == 0)
                    return ToResponse(ResultCode.FAIL, "Cannot cancel journal - لا يمكن إلغاء دفتر اليومية");

                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Reverse journal - عكس دفتر يومية مرحّل
        /// </summary>
        [HttpPost("{id}/reverse")]
        [ActionPermissionFilter(Permission = "finance:journal:reverse")]
        [Log(Title = "GL Journal Reverse", BusinessType = BusinessType.UPDATE)]
        public IActionResult ReverseJournal(Guid id, [FromBody] ReverseJournalDto dto)
        {
            try
            {
                dto.Id = id;
                var result = _journalService.ReverseJournal(dto, HttpContext.GetName());
                if (result == null)
                    return ToResponse(ResultCode.FAIL, "Cannot reverse journal - لا يمكن عكس دفتر اليومية");

                return SUCCESS(result);
            }
            catch (BasePeriodException ex)
            {
                return ToPeriodErrorResponse(ex);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Submit journal for approval - تقديم دفتر يومية للاعتماد
        /// </summary>
        [HttpPost("{id}/submit")]
        [ActionPermissionFilter(Permission = "finance:journal:submit")]
        [Log(Title = "GL Journal Submit", BusinessType = BusinessType.UPDATE)]
        public IActionResult SubmitJournal(Guid id)
        {
            try
            {
                var result = _journalService.SubmitJournal(id, HttpContext.GetName());
                if (result == 0)
                    return ToResponse(ResultCode.FAIL, "Cannot submit journal - لا يمكن تقديم دفتر اليومية");

                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Approve journal - اعتماد دفتر يومية
        /// </summary>
        [HttpPost("{id}/approve")]
        [ActionPermissionFilter(Permission = "finance:journal:approve")]
        [Log(Title = "GL Journal Approve", BusinessType = BusinessType.UPDATE)]
        public IActionResult ApproveJournal(Guid id)
        {
            try
            {
                var result = _journalService.ApproveJournal(id, HttpContext.GetName());
                if (result == 0)
                    return ToResponse(ResultCode.FAIL, "Cannot approve journal - لا يمكن اعتماد دفتر اليومية");

                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Reject journal - رفض دفتر يومية
        /// </summary>
        [HttpPost("{id}/reject")]
        [ActionPermissionFilter(Permission = "finance:journal:reject")]
        [Log(Title = "GL Journal Reject", BusinessType = BusinessType.UPDATE)]
        public IActionResult RejectJournal(Guid id, [FromBody] RejectJournalDto dto)
        {
            try
            {
                var result = _journalService.RejectJournal(id, dto?.Reason ?? "", HttpContext.GetName());
                if (result == 0)
                    return ToResponse(ResultCode.FAIL, "Cannot reject journal - لا يمكن رفض دفتر اليومية");

                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Validate journal for posting - التحقق من القيد للترحيل
        /// </summary>
        [HttpGet("{id}/validate-post")]
        [ActionPermissionFilter(Permission = "finance:journal:query")]
        public IActionResult ValidateForPosting(Guid id)
        {
            var validation = _journalService.ValidateJournal(id);
            return SUCCESS(new
            {
                validation.IsValid,
                validation.Errors,
                validation.Warnings,
                validation.TotalDebit,
                validation.TotalCredit,
                validation.IsBalanced,
                validation.Difference
            });
        }

        /// <summary>
        /// Generate next journal number - توليد رقم دفتر اليومية التالي
        /// </summary>
        [HttpGet("next-number")]
        [ActionPermissionFilter(Permission = "finance:journal:query")]
        public IActionResult GetNextJournalNumber()
        {
            return SUCCESS(new { JournalNumber = _journalService.GenerateNextJournalNumber() });
        }

        /// <summary>
        /// Check if journal is balanced - التحقق من توازن دفتر اليومية
        /// </summary>
        [HttpGet("{id}/is-balanced")]
        [ActionPermissionFilter(Permission = "finance:journal:query")]
        public IActionResult CheckIsBalanced(Guid id)
        {
            return SUCCESS(new { IsBalanced = _journalService.IsJournalBalanced(id) });
        }

        /// <summary>
        /// Export journals - تصدير دفاتر اليومية
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "finance:journal:export")]
        [Log(Title = "GL Journal", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] GLJournalQueryDto query, [FromQuery] PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _journalService.GetList(query, pager).Result;
            var result = ExportExcelMini(list, "GL_Journals", "Journal Entries - دفتر اليومية");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    /// <summary>
    /// DTO for rejecting a journal
    /// </summary>
    public class RejectJournalDto
    {
        public string Reason { get; set; }
    }
}
