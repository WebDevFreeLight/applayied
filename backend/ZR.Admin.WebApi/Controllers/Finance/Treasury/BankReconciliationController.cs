using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Admin.WebApi.Controllers.Finance.Treasury
{
    /// <summary>
    /// Bank Reconciliation Controller - تحكم التسوية البنكية
    /// </summary>
    [Verify]
    [Route("erp/finance/treasury/[controller]")]
    [ApiController]
    public class BankReconciliationController : BaseController
    {
        private readonly IBankReconciliationService _reconciliationService;

        public BankReconciliationController(IBankReconciliationService reconciliationService)
        {
            _reconciliationService = reconciliationService;
        }

        /// <summary>
        /// Get reconciliation list with paging - الحصول على قائمة التسويات مع ترقيم الصفحات
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] BankReconciliationQueryDto parm)
        {
            var response = _reconciliationService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get reconciliation by ID - الحصول على تسوية بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _reconciliationService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Reconciliation not found - التسوية غير موجودة");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Get reconciliation report - الحصول على تقرير التسوية
        /// </summary>
        [HttpGet("{id}/report")]
        public IActionResult GetReport(Guid id)
        {
            var response = _reconciliationService.GetReport(id);
            return SUCCESS(response);
        }

        /// <summary>
        /// Start new reconciliation - بدء تسوية جديدة
        /// </summary>
        [HttpPost]
        [Log(Title = "Start Bank Reconciliation", BusinessType = BusinessType.INSERT)]
        public IActionResult StartReconciliation([FromBody] CreateReconciliationDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _reconciliationService.StartReconciliation(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Auto-match reconciliation items - مطابقة تلقائية لعناصر التسوية
        /// </summary>
        [HttpPost("{id}/auto-match")]
        [Log(Title = "Auto Match Reconciliation", BusinessType = BusinessType.UPDATE)]
        public IActionResult AutoMatch(Guid id)
        {
            var result = _reconciliationService.AutoMatch(id);
            return SUCCESS(new { MatchedCount = result });
        }

        /// <summary>
        /// Match reconciliation line manually - مطابقة بند تسوية يدوياً
        /// </summary>
        [HttpPost("match")]
        [Log(Title = "Match Reconciliation Line", BusinessType = BusinessType.UPDATE)]
        public IActionResult MatchLine([FromBody] MatchReconciliationLineDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _reconciliationService.MatchLine(dto, userId);
            return ToResponse(result);
        }

        /// <summary>
        /// Complete reconciliation - إكمال التسوية
        /// </summary>
        [HttpPost("complete")]
        [Log(Title = "Complete Reconciliation", BusinessType = BusinessType.UPDATE)]
        public IActionResult Complete([FromBody] CompleteReconciliationDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _reconciliationService.Complete(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete reconciliation - حذف تسوية
        /// </summary>
        [HttpDelete("{id}")]
        [Log(Title = "Delete Reconciliation", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid id)
        {
            var result = _reconciliationService.Delete(new[] { id });
            return ToResponse(result);
        }
    }
}
