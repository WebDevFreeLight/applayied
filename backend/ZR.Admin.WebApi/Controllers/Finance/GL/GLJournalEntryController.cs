using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Service.IService.Finance.GL;

namespace ZR.Admin.WebApi.Controllers.Finance.GL
{
    /// <summary>
    /// GL Journal Entry Controller - متحكم قيود اليومية
    /// </summary>
    [Verify]
    [Route("finance/journal-entries")]
    [ApiController]
    public class GLJournalEntryController : BaseController
    {
        private readonly IGLJournalEntryService _entryService;

        public GLJournalEntryController(IGLJournalEntryService entryService)
        {
            _entryService = entryService;
        }

        /// <summary>
        /// Get list of entries - الحصول على قائمة القيود
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:entry:list")]
        public IActionResult GetList([FromQuery] GLJournalEntryQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_entryService.GetList(query, pager));
        }

        /// <summary>
        /// Get entry by ID - الحصول على القيد بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:entry:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _entryService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Entry not found - القيد غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create entry - إنشاء قيد
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:entry:add")]
        [Log(Title = "GL Journal Entry", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateJournalEntryDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var result = _entryService.Create(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Update entry - تحديث قيد
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:entry:edit")]
        [Log(Title = "GL Journal Entry", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateJournalEntryDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var result = _entryService.Update(dto, HttpContext.GetName());
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Cannot update entry of posted journal - لا يمكن تحديث قيد لدفتر يومية مرحّل");

            return ToResponse(result);
        }

        /// <summary>
        /// Delete entries - حذف قيود
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:entry:delete")]
        [Log(Title = "GL Journal Entry", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idsArr = ids.Split(',').Select(Guid.Parse).ToArray();
            if (idsArr.Length <= 0)
                return ToResponse(ResultCode.FAIL);

            var result = _entryService.Delete(idsArr);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Cannot delete entries of posted journals - لا يمكن حذف قيود دفاتر اليومية المرحّلة");

            return ToResponse(result);
        }

        /// <summary>
        /// Get entries by journal ID - الحصول على القيود بمعرف دفتر اليومية
        /// </summary>
        [HttpGet("by-journal/{journalId}")]
        [ActionPermissionFilter(Permission = "finance:entry:list")]
        public IActionResult GetByJournalId(Guid journalId)
        {
            return SUCCESS(_entryService.GetByJournalId(journalId));
        }

        /// <summary>
        /// Get entries by account ID - الحصول على القيود بمعرف الحساب
        /// </summary>
        [HttpGet("by-account/{accountId}")]
        [ActionPermissionFilter(Permission = "finance:entry:list")]
        public IActionResult GetByAccountId(Guid accountId)
        {
            return SUCCESS(_entryService.GetByAccountId(accountId));
        }

        /// <summary>
        /// Get journal totals - الحصول على إجماليات دفتر اليومية
        /// </summary>
        [HttpGet("totals/{journalId}")]
        [ActionPermissionFilter(Permission = "finance:entry:query")]
        public IActionResult GetJournalTotals(Guid journalId)
        {
            var (totalDebit, totalCredit) = _entryService.GetJournalTotals(journalId);
            return SUCCESS(new
            {
                JournalId = journalId,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                IsBalanced = totalDebit == totalCredit && totalDebit > 0
            });
        }

        /// <summary>
        /// Export entries - تصدير القيود
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "finance:entry:export")]
        [Log(Title = "GL Journal Entry", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] GLJournalEntryQueryDto query, [FromQuery] PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _entryService.GetList(query, pager).Result;
            var result = ExportExcelMini(list, "GL_JournalEntries", "Journal Entry Lines - قيود دفتر اليومية");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}

