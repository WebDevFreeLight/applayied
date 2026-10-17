using System;
using SqlSugar;
using ZR.Model.System;
using ZR.Model.Finance.GL;

namespace ZR.Model.Finance.Budget
{
    /// <summary>
    /// Budget Line - بند الميزانية
    /// Budget line items per GL account and period
    /// </summary>
    [SugarTable("finance_budget_lines", "Budget line items per GL account and period")]
    // Unique constraint: One line per header/account/period combination
    [SugarIndex("UX_budget_line_header_account_period", nameof(BudgetHeaderId), OrderByType.Asc, nameof(GLAccountId), OrderByType.Asc, nameof(Period), OrderByType.Asc, true)]
    // FK indexes for joins
    [SugarIndex("IX_budget_line_header", nameof(BudgetHeaderId), OrderByType.Asc)]
    [SugarIndex("IX_budget_line_gl_account", nameof(GLAccountId), OrderByType.Asc)]
    // Period-based queries
    [SugarIndex("IX_budget_line_period", nameof(Period), OrderByType.Asc)]
    // Company isolation index
    [SugarIndex("IX_budget_line_company", nameof(CompanyID), OrderByType.Asc)]
    // Company + Header for filtered queries
    [SugarIndex("IX_budget_line_company_header", nameof(CompanyID), OrderByType.Asc, nameof(BudgetHeaderId), OrderByType.Asc)]
    public class BudgetLine : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Line ID - معرف البند
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "id")]
        public Guid Id { get; set; }

        #endregion

        #region Foreign Keys

        /// <summary>
        /// Budget Header ID - معرف رأس الميزانية
        /// FK to finance_budget_header
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "budget_header_id")]
        public Guid BudgetHeaderId { get; set; }

        /// <summary>
        /// GL Account ID - معرف حساب دفتر الأستاذ
        /// FK to gl_account
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "gl_account_id")]
        public Guid GLAccountId { get; set; }

        #endregion

        #region Budget Data

        /// <summary>
        /// Period - الفترة
        /// 1-12 for months, or 1-4 for quarters
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period")]
        public int Period { get; set; }

        /// <summary>
        /// Budgeted Amount - المبلغ المخطط
        /// Original budgeted amount
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "budgeted_amount", DecimalDigits = 2, DefaultValue = "0")]
        public decimal BudgetedAmount { get; set; }

        /// <summary>
        /// Revised Amount - المبلغ المعدل
        /// Revised/adjusted budget amount
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "revised_amount", DecimalDigits = 2, DefaultValue = "0")]
        public decimal RevisedAmount { get; set; }

        /// <summary>
        /// Actual Amount - المبلغ الفعلي
        /// Updated via background job integration with GL
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "actual_amount", DecimalDigits = 2, DefaultValue = "0")]
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Committed Amount - المبلغ الملتزم به
        /// Amounts committed via purchase orders
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "committed_amount", DecimalDigits = 2, DefaultValue = "0")]
        public decimal CommittedAmount { get; set; }

        #endregion

        #region Audit Information

        /// <summary>
        /// Last Actual Update - آخر تحديث للفعلي
        /// Timestamp of last actual amount update from GL
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnName = "last_actual_update")]
        public DateTime? LastActualUpdate { get; set; }

        /// <summary>
        /// Notes - ملاحظات
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnName = "notes")]
        public string Notes { get; set; }

        #endregion

        #region Navigation Properties (Ignored in DB)

        /// <summary>
        /// Budget Header - رأس الميزانية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public BudgetHeader Header { get; set; }

        /// <summary>
        /// GL Account - حساب دفتر الأستاذ
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public GLAccount GLAccount { get; set; }

        #endregion

        #region Calculated Properties (Ignored in DB)

        /// <summary>
        /// Variance - الانحراف
        /// ActualAmount - BudgetedAmount (positive = over budget, negative = under budget)
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal Variance => ActualAmount - BudgetedAmount;

        /// <summary>
        /// Variance Percentage - نسبة الانحراف
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal VariancePercentage => BudgetedAmount != 0
            ? Math.Round((Variance / BudgetedAmount) * 100, 2)
            : 0;

        /// <summary>
        /// Effective Budget - الميزانية الفعالة
        /// Returns RevisedAmount if > 0, otherwise BudgetedAmount
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal EffectiveBudget => RevisedAmount > 0 ? RevisedAmount : BudgetedAmount;

        /// <summary>
        /// Available Amount - المبلغ المتاح
        /// EffectiveBudget - ActualAmount - CommittedAmount
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal AvailableAmount => EffectiveBudget - ActualAmount - CommittedAmount;

        /// <summary>
        /// Utilization Percentage - نسبة الاستخدام
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal UtilizationPercentage => EffectiveBudget != 0
            ? Math.Round((ActualAmount / EffectiveBudget) * 100, 2)
            : 0;

        /// <summary>
        /// Is Over Budget - تجاوز الميزانية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsOverBudget => ActualAmount > EffectiveBudget;

        #endregion
    }
}
