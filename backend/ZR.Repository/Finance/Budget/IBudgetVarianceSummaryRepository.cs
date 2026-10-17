using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Budget;

namespace ZR.Repository.Finance.Budget
{
    /// <summary>
    /// Budget Variance Summary Repository Interface - واجهة مستودع ملخص انحراف الميزانية
    /// </summary>
    public interface IBudgetVarianceSummaryRepository : IBaseRepository<BudgetVarianceSummary>
    {
        /// <summary>
        /// Get variance summaries by budget header ID - الحصول على ملخصات الانحراف حسب رأس الميزانية
        /// </summary>
        List<BudgetVarianceSummary> GetByHeaderId(Guid budgetHeaderId);

        /// <summary>
        /// Get variance summary by ID - الحصول على ملخص الانحراف بالمعرف
        /// </summary>
        BudgetVarianceSummary GetById(Guid summaryId);

        /// <summary>
        /// Get variance summaries by GL account - الحصول على ملخصات الانحراف حسب حساب دفتر الأستاذ
        /// </summary>
        List<BudgetVarianceSummary> GetByGLAccount(Guid glAccountId);

        /// <summary>
        /// Get variance summaries by header and period - الحصول على ملخصات الانحراف حسب الرأس والفترة
        /// </summary>
        List<BudgetVarianceSummary> GetByHeaderAndPeriod(Guid budgetHeaderId, int period);

        /// <summary>
        /// Get variance summary by header, GL account and period - الحصول على ملخص الانحراف حسب الرأس والحساب والفترة
        /// </summary>
        BudgetVarianceSummary GetByHeaderAccountPeriod(Guid budgetHeaderId, Guid glAccountId, int period);

        /// <summary>
        /// Delete summaries by header ID - حذف الملخصات حسب رأس الميزانية
        /// </summary>
        int DeleteByHeaderId(Guid budgetHeaderId);

        /// <summary>
        /// Bulk insert or update variance summaries - إدراج أو تحديث ملخصات الانحراف بالجملة
        /// </summary>
        int BulkUpsert(List<BudgetVarianceSummary> summaries);

        /// <summary>
        /// Get top variances (positive or negative) - الحصول على أعلى الانحرافات
        /// </summary>
        List<BudgetVarianceSummary> GetTopVariances(Guid budgetHeaderId, int count, bool positiveOnly = false);
    }
}
