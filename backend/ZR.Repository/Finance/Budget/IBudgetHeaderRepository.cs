using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Budget;

namespace ZR.Repository.Finance.Budget
{
    /// <summary>
    /// Budget Header Repository Interface - واجهة مستودع رأس الميزانية
    /// All methods support optional companyId parameter for multi-tenant isolation
    /// </summary>
    public interface IBudgetHeaderRepository : IBaseRepository<BudgetHeader>
    {
        /// <summary>
        /// Get paginated list of budget headers - الحصول على قائمة رؤوس الميزانية بالترقيم
        /// CompanyID filtering is handled via the BudgetHeader.CompanyID property
        /// </summary>
        PagedInfo<BudgetHeader> GetList(BudgetHeader parm, PagerInfo pager);

        /// <summary>
        /// Get budget header by ID - الحصول على رأس الميزانية بالمعرف
        /// </summary>
        BudgetHeader GetById(Guid budgetId);

        /// <summary>
        /// Get budget header by code with company isolation - الحصول على رأس الميزانية بالكود
        /// </summary>
        BudgetHeader GetByCode(string budgetCode, long? companyId = null);

        /// <summary>
        /// Check if budget code exists with company isolation - التحقق من وجود كود الميزانية
        /// </summary>
        bool CodeExists(string code, long? companyId = null, Guid? excludeId = null);

        /// <summary>
        /// Get active budgets with company isolation - الحصول على الميزانيات النشطة
        /// </summary>
        List<BudgetHeader> GetActiveBudgets(long? companyId = null);

        /// <summary>
        /// Get budgets by fiscal year with company isolation - الحصول على الميزانيات حسب السنة المالية
        /// </summary>
        List<BudgetHeader> GetByFiscalYear(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Get budgets by cost center with company isolation - الحصول على الميزانيات حسب مركز التكلفة
        /// </summary>
        List<BudgetHeader> GetByCostCenter(long costCenterId, long? companyId = null);

        /// <summary>
        /// Get budgets by project with company isolation - الحصول على الميزانيات حسب المشروع
        /// </summary>
        List<BudgetHeader> GetByProject(Guid projectId, long? companyId = null);

        /// <summary>
        /// Get budgets by department with company isolation - الحصول على الميزانيات حسب القسم
        /// </summary>
        List<BudgetHeader> GetByDepartment(long departmentId, long? companyId = null);

        /// <summary>
        /// Get budgets by type with company isolation - الحصول على الميزانيات حسب النوع
        /// </summary>
        List<BudgetHeader> GetByType(string budgetType, long? companyId = null);

        /// <summary>
        /// Approve budget - اعتماد الميزانية
        /// </summary>
        int ApproveBudget(Guid budgetId, Guid approvedBy);

        /// <summary>
        /// Close budget - إغلاق الميزانية
        /// </summary>
        int CloseBudget(Guid budgetId, Guid closedBy);

        /// <summary>
        /// Get budgets by status with company isolation - الحصول على الميزانيات حسب الحالة
        /// </summary>
        List<BudgetHeader> GetByStatus(string status, long? companyId = null);

        /// <summary>
        /// Get next budget code with company isolation - الحصول على الكود التالي
        /// </summary>
        string GetNextCode(string prefix, long? companyId = null);
    }
}
