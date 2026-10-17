using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Budget;
using ZR.Model.Finance.Budget.Dto;

namespace ZR.Service.IService.Finance.Budget
{
    /// <summary>
    /// Budget Service Interface - واجهة خدمة الميزانية
    /// </summary>
    public interface IBudgetService : IBaseService<BudgetHeader>
    {
        #region Budget Header Operations

        /// <summary>
        /// Get paginated list of budgets - الحصول على قائمة الميزانيات بالترقيم
        /// </summary>
        PagedInfo<BudgetHeaderListDto> GetList(BudgetHeaderQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get budget by ID with lines - الحصول على الميزانية بالمعرف مع البنود
        /// </summary>
        BudgetHeaderDetailDto GetById(Guid budgetId);

        /// <summary>
        /// Create budget with lines - إنشاء ميزانية مع البنود
        /// </summary>
        BudgetHeader Create(CreateBudgetHeaderDto dto, string userName);

        /// <summary>
        /// Update budget with lines - تحديث ميزانية مع البنود
        /// </summary>
        int Update(UpdateBudgetHeaderDto dto, string userName);

        /// <summary>
        /// Delete budgets - حذف الميزانيات
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Approve budget - اعتماد الميزانية
        /// </summary>
        int ApproveBudget(Guid budgetId, Guid approvedBy);

        /// <summary>
        /// Close budget - إغلاق الميزانية
        /// </summary>
        int CloseBudget(Guid budgetId, Guid closedBy);

        /// <summary>
        /// Get budgets by status - الميزانيات حسب الحالة
        /// </summary>
        List<BudgetSelectItemDto> GetByStatus(string status);

        /// <summary>
        /// Get active budgets for dropdown - الميزانيات النشطة للقائمة المنسدلة
        /// </summary>
        List<BudgetSelectItemDto> GetActiveBudgets();

        /// <summary>
        /// Get budgets by fiscal year - الميزانيات حسب السنة المالية
        /// </summary>
        List<BudgetSelectItemDto> GetByFiscalYear(int fiscalYear);

        /// <summary>
        /// Get budgets by cost center - الميزانيات حسب مركز التكلفة
        /// </summary>
        List<BudgetSelectItemDto> GetByCostCenter(long costCenterId);

        /// <summary>
        /// Get budgets by project - الميزانيات حسب المشروع
        /// </summary>
        List<BudgetSelectItemDto> GetByProject(Guid projectId);

        /// <summary>
        /// Get budgets by department - الميزانيات حسب القسم
        /// </summary>
        List<BudgetSelectItemDto> GetByDepartment(long departmentId);

        /// <summary>
        /// Check if budget code exists (with company isolation) - التحقق من وجود كود الميزانية
        /// </summary>
        bool CodeExists(string code, long? companyId = null, Guid? excludeId = null);

        /// <summary>
        /// Get next budget code (with company isolation) - الحصول على الكود التالي
        /// </summary>
        string GetNextCode(string prefix, long? companyId = null);

        #endregion

        #region Budget Line Operations

        /// <summary>
        /// Get budget lines by header ID - بنود الميزانية حسب رأس الميزانية
        /// </summary>
        List<BudgetLineDto> GetBudgetLines(Guid budgetHeaderId);

        /// <summary>
        /// Add budget line - إضافة بند ميزانية
        /// </summary>
        BudgetLine AddBudgetLine(Guid budgetHeaderId, CreateBudgetLineDto dto, string userName);

        /// <summary>
        /// Update budget line - تحديث بند ميزانية
        /// </summary>
        int UpdateBudgetLine(UpdateBudgetLineDto dto, string userName);

        /// <summary>
        /// Delete budget line - حذف بند ميزانية
        /// </summary>
        int DeleteBudgetLine(Guid lineId);

        #endregion

        #region Actuals and Variance Operations

        /// <summary>
        /// Refresh actuals from GL - تحديث الفعلي من دفتر الأستاذ
        /// </summary>
        int RefreshActualsFromGL(Guid budgetId, DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Recalculate variance summaries - إعادة حساب ملخصات الانحراف
        /// </summary>
        int RecalculateVarianceSummaries(Guid budgetId);

        /// <summary>
        /// Get variance summaries by budget - ملخصات الانحراف حسب الميزانية
        /// </summary>
        List<BudgetVarianceSummaryDto> GetVarianceSummaries(Guid budgetId);

        /// <summary>
        /// Get variance summaries by period - ملخصات الانحراف حسب الفترة
        /// </summary>
        List<BudgetVarianceSummaryDto> GetVarianceSummariesByPeriod(Guid budgetId, int period);

        #endregion

        #region Reporting Operations

        /// <summary>
        /// Get budget vs actual report - تقرير الميزانية مقابل الفعلي
        /// </summary>
        BudgetVsActualReportDto GetBudgetVsActualReport(Guid budgetId);

        /// <summary>
        /// Get budget vs actual report for fiscal year - تقرير الميزانية مقابل الفعلي للسنة المالية
        /// </summary>
        List<BudgetVsActualReportDto> GetBudgetVsActualReportByFiscalYear(int fiscalYear);

        #endregion
    }
}
