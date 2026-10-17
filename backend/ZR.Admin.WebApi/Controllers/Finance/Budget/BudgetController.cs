using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Budget.Dto;
using ZR.Service.IService.Finance.Budget;
using ZR.Common;
using Infrastructure.Enums;

namespace ZR.Admin.WebApi.Controllers.Finance.Budget
{
    /// <summary>
    /// Budget Management - إدارة الميزانية
    /// </summary>
    [Verify]
    [Route("finance/budgets")]
    [ApiController]
    public class BudgetController : BaseController
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        #region Budget Header Endpoints

        /// <summary>
        /// Get budget list - قائمة الميزانيات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:budget:list")]
        public IActionResult GetList([FromQuery] BudgetHeaderQueryDto query, [FromQuery] PagerInfo pager)
        {
            var response = _budgetService.GetList(query, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get budget by ID - الحصول على ميزانية بالمعرف
        /// </summary>
        [HttpGet("{budgetId}")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetInfo(Guid budgetId)
        {
            var response = _budgetService.GetById(budgetId);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Budget not found - الميزانية غير موجودة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create budget - إنشاء ميزانية
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:budget:add")]
        [Log(Title = "Create Budget", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateBudgetHeaderDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var response = _budgetService.Create(dto, HttpContext.GetName());
            return SUCCESS(response);
        }

        /// <summary>
        /// Update budget - تحديث ميزانية
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:budget:edit")]
        [Log(Title = "Update Budget", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateBudgetHeaderDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var response = _budgetService.Update(dto, HttpContext.GetName());
            return ToResponse(response);
        }

        /// <summary>
        /// Delete budgets - حذف الميزانيات
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:budget:delete")]
        [Log(Title = "Delete Budgets", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idList = ids.Split(',')
                .Where(s => Guid.TryParse(s, out _))
                .Select(Guid.Parse)
                .ToArray();

            if (idList.Length == 0)
                return ToResponse(ResultCode.FAIL, "No valid IDs provided - لم يتم تقديم معرفات صالحة");

            var response = _budgetService.Delete(idList);
            return ToResponse(response);
        }

        /// <summary>
        /// Approve budget - اعتماد الميزانية
        /// </summary>
        [HttpPost("approve")]
        [ActionPermissionFilter(Permission = "finance:budget:approve")]
        [Log(Title = "Approve Budget", BusinessType = BusinessType.UPDATE)]
        public IActionResult Approve([FromBody] ApproveBudgetRequestDto request)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            // Get current user ID as Guid (you may need to adjust this based on your user system)
            var userIdStr = HttpContext.GetUId().ToString();
            if (!Guid.TryParse(userIdStr, out Guid approvedBy))
            {
                approvedBy = Guid.Empty;
            }

            var response = _budgetService.ApproveBudget(request.BudgetId, approvedBy);
            return ToResponse(response);
        }

        /// <summary>
        /// Close budget - إغلاق الميزانية
        /// </summary>
        [HttpPost("close")]
        [ActionPermissionFilter(Permission = "finance:budget:close")]
        [Log(Title = "Close Budget", BusinessType = BusinessType.UPDATE)]
        public IActionResult Close([FromBody] CloseBudgetRequestDto request)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            // Get current user ID as Guid
            var userIdStr = HttpContext.GetUId().ToString();
            if (!Guid.TryParse(userIdStr, out Guid closedBy))
            {
                closedBy = Guid.Empty;
            }

            var response = _budgetService.CloseBudget(request.BudgetId, closedBy);
            return ToResponse(response);
        }

        /// <summary>
        /// Get budgets by status - الميزانيات حسب الحالة
        /// </summary>
        [HttpGet("by-status/{status}")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetByStatus(string status)
        {
            var response = _budgetService.GetByStatus(status);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get active budgets for dropdown - الميزانيات النشطة للقائمة المنسدلة
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetActiveBudgets()
        {
            var response = _budgetService.GetActiveBudgets();
            return SUCCESS(response);
        }

        /// <summary>
        /// Get budgets by fiscal year - الميزانيات حسب السنة المالية
        /// </summary>
        [HttpGet("by-fiscal-year/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetByFiscalYear(int fiscalYear)
        {
            var response = _budgetService.GetByFiscalYear(fiscalYear);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get budgets by cost center - الميزانيات حسب مركز التكلفة
        /// </summary>
        [HttpGet("by-cost-center/{costCenterId}")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetByCostCenter(long costCenterId)
        {
            var response = _budgetService.GetByCostCenter(costCenterId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get budgets by project - الميزانيات حسب المشروع
        /// </summary>
        [HttpGet("by-project/{projectId}")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetByProject(Guid projectId)
        {
            var response = _budgetService.GetByProject(projectId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get budgets by department - الميزانيات حسب القسم
        /// </summary>
        [HttpGet("by-department/{departmentId}")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetByDepartment(long departmentId)
        {
            var response = _budgetService.GetByDepartment(departmentId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Check if code exists (with company isolation) - التحقق من وجود الكود
        /// </summary>
        [HttpGet("check-code")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult CheckCodeExists([FromQuery] string code, [FromQuery] long? companyId = null, [FromQuery] Guid? excludeId = null)
        {
            var exists = _budgetService.CodeExists(code, companyId, excludeId);
            return SUCCESS(new { Exists = exists });
        }

        /// <summary>
        /// Get next budget code (with company isolation) - الحصول على الكود التالي
        /// </summary>
        [HttpGet("next-code")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetNextCode([FromQuery] string prefix = "BUD", [FromQuery] long? companyId = null)
        {
            var nextCode = _budgetService.GetNextCode(prefix, companyId);
            return SUCCESS(new { Code = nextCode });
        }

        #endregion

        #region Budget Line Endpoints

        /// <summary>
        /// Get budget lines - بنود الميزانية
        /// </summary>
        [HttpGet("{budgetId}/lines")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetBudgetLines(Guid budgetId)
        {
            var response = _budgetService.GetBudgetLines(budgetId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Add budget line - إضافة بند ميزانية
        /// </summary>
        [HttpPost("{budgetId}/lines")]
        [ActionPermissionFilter(Permission = "finance:budget:edit")]
        [Log(Title = "Add Budget Line", BusinessType = BusinessType.INSERT)]
        public IActionResult AddBudgetLine(Guid budgetId, [FromBody] CreateBudgetLineDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var response = _budgetService.AddBudgetLine(budgetId, dto, HttpContext.GetName());
            return SUCCESS(response);
        }

        /// <summary>
        /// Update budget line - تحديث بند ميزانية
        /// </summary>
        [HttpPut("lines")]
        [ActionPermissionFilter(Permission = "finance:budget:edit")]
        [Log(Title = "Update Budget Line", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateBudgetLine([FromBody] UpdateBudgetLineDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var response = _budgetService.UpdateBudgetLine(dto, HttpContext.GetName());
            return ToResponse(response);
        }

        /// <summary>
        /// Delete budget line - حذف بند ميزانية
        /// </summary>
        [HttpDelete("lines/{lineId}")]
        [ActionPermissionFilter(Permission = "finance:budget:edit")]
        [Log(Title = "Delete Budget Line", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteBudgetLine(Guid lineId)
        {
            var response = _budgetService.DeleteBudgetLine(lineId);
            return ToResponse(response);
        }

        #endregion

        #region Actuals and Variance Endpoints

        /// <summary>
        /// Refresh actuals from GL - تحديث الفعلي من دفتر الأستاذ
        /// </summary>
        [HttpPost("refresh-actuals")]
        [ActionPermissionFilter(Permission = "finance:budget:refresh")]
        [Log(Title = "Refresh Budget Actuals", BusinessType = BusinessType.UPDATE)]
        public IActionResult RefreshActuals([FromBody] RefreshActualsRequestDto request)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            var response = _budgetService.RefreshActualsFromGL(request.BudgetId, request.FromDate, request.ToDate);
            return SUCCESS(new { UpdatedLines = response, Message = $"Updated {response} budget lines - تم تحديث {response} بند ميزانية" });
        }

        /// <summary>
        /// Recalculate variance summaries - إعادة حساب ملخصات الانحراف
        /// </summary>
        [HttpPost("{budgetId}/recalculate-variance")]
        [ActionPermissionFilter(Permission = "finance:budget:refresh")]
        [Log(Title = "Recalculate Budget Variance", BusinessType = BusinessType.UPDATE)]
        public IActionResult RecalculateVariance(Guid budgetId)
        {
            var response = _budgetService.RecalculateVarianceSummaries(budgetId);
            return SUCCESS(new { SummaryCount = response, Message = $"Recalculated {response} variance summaries - تمت إعادة حساب {response} ملخص انحراف" });
        }

        /// <summary>
        /// Get variance summaries - ملخصات الانحراف
        /// </summary>
        [HttpGet("{budgetId}/variance-summaries")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetVarianceSummaries(Guid budgetId)
        {
            var response = _budgetService.GetVarianceSummaries(budgetId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get variance summaries by period - ملخصات الانحراف حسب الفترة
        /// </summary>
        [HttpGet("{budgetId}/variance-summaries/period/{period}")]
        [ActionPermissionFilter(Permission = "finance:budget:query")]
        public IActionResult GetVarianceSummariesByPeriod(Guid budgetId, int period)
        {
            var response = _budgetService.GetVarianceSummariesByPeriod(budgetId, period);
            return SUCCESS(response);
        }

        #endregion

        #region Reporting Endpoints

        /// <summary>
        /// Get budget vs actual report - تقرير الميزانية مقابل الفعلي
        /// </summary>
        [HttpGet("{budgetId}/report")]
        [ActionPermissionFilter(Permission = "finance:budget:report")]
        public IActionResult GetBudgetVsActualReport(Guid budgetId)
        {
            var response = _budgetService.GetBudgetVsActualReport(budgetId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get budget vs actual report for fiscal year - تقرير الميزانية مقابل الفعلي للسنة المالية
        /// </summary>
        [HttpGet("report/fiscal-year/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "finance:budget:report")]
        public IActionResult GetBudgetVsActualReportByFiscalYear(int fiscalYear)
        {
            var response = _budgetService.GetBudgetVsActualReportByFiscalYear(fiscalYear);
            return SUCCESS(response);
        }

        #endregion
    }
}
