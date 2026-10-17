using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Budget;

namespace ZR.Repository.Finance.Budget
{
    /// <summary>
    /// Budget Line Repository Interface - واجهة مستودع بند الميزانية
    /// </summary>
    public interface IBudgetLineRepository : IBaseRepository<BudgetLine>
    {
        /// <summary>
        /// Get budget lines by header ID - الحصول على بنود الميزانية حسب رأس الميزانية
        /// </summary>
        List<BudgetLine> GetByHeaderId(Guid budgetHeaderId);

        /// <summary>
        /// Get budget line by ID - الحصول على بند الميزانية بالمعرف
        /// </summary>
        BudgetLine GetById(Guid lineId);

        /// <summary>
        /// Get budget lines by GL account - الحصول على بنود الميزانية حسب حساب دفتر الأستاذ
        /// </summary>
        List<BudgetLine> GetByGLAccount(Guid glAccountId);

        /// <summary>
        /// Get budget lines by header and period - الحصول على بنود الميزانية حسب الرأس والفترة
        /// </summary>
        List<BudgetLine> GetByHeaderAndPeriod(Guid budgetHeaderId, int period);

        /// <summary>
        /// Get budget line by header, GL account and period - الحصول على بند الميزانية حسب الرأس والحساب والفترة
        /// </summary>
        BudgetLine GetByHeaderAccountPeriod(Guid budgetHeaderId, Guid glAccountId, int period);

        /// <summary>
        /// Delete lines by header ID - حذف البنود حسب رأس الميزانية
        /// </summary>
        int DeleteByHeaderId(Guid budgetHeaderId);

        /// <summary>
        /// Update actual amount - تحديث المبلغ الفعلي
        /// </summary>
        int UpdateActualAmount(Guid lineId, decimal actualAmount);

        /// <summary>
        /// Bulk update actual amounts - تحديث المبالغ الفعلية بالجملة
        /// </summary>
        int BulkUpdateActualAmounts(List<BudgetLine> lines);

        /// <summary>
        /// Get total budgeted amount by header - الحصول على إجمالي المبلغ المخطط حسب الرأس
        /// </summary>
        decimal GetTotalBudgetedByHeader(Guid budgetHeaderId);

        /// <summary>
        /// Get total actual amount by header - الحصول على إجمالي المبلغ الفعلي حسب الرأس
        /// </summary>
        decimal GetTotalActualByHeader(Guid budgetHeaderId);
    }
}
