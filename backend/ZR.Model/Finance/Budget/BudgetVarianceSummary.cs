using System;
using SqlSugar;
using ZR.Model.System;
using ZR.Model.Finance.GL;

namespace ZR.Model.Finance.Budget
{
    /// <summary>
    /// Budget Variance Summary - ملخص انحراف الميزانية
    /// Pre-calculated variance data updated via background job only
    /// </summary>
    [SugarTable("finance_budget_variance_summary", "Budget variance summary (updated via background job)")]
    // Unique constraint: One summary per header/account/period
    [SugarIndex("UX_variance_header_account_period", nameof(BudgetHeaderId), OrderByType.Asc, nameof(GLAccountId), OrderByType.Asc, nameof(Period), OrderByType.Asc, true)]
    // FK indexes for joins
    [SugarIndex("IX_variance_header", nameof(BudgetHeaderId), OrderByType.Asc)]
    [SugarIndex("IX_variance_gl_account", nameof(GLAccountId), OrderByType.Asc)]
    // Period-based queries
    [SugarIndex("IX_variance_period", nameof(Period), OrderByType.Asc)]
    // Refresh tracking index
    [SugarIndex("IX_variance_last_refreshed", nameof(LastRefreshed), OrderByType.Desc)]
    // Stale data identification
    [SugarIndex("IX_variance_stale", nameof(IsStale), OrderByType.Asc)]
    // Company isolation index
    [SugarIndex("IX_variance_company", nameof(CompanyID), OrderByType.Asc)]
    [SugarIndex("IX_variance_company_header", nameof(CompanyID), OrderByType.Asc, nameof(BudgetHeaderId), OrderByType.Asc)]
    public class BudgetVarianceSummary : SysBase
    {
        #region Primary Key

        /// <summary>
        /// Summary ID - معرف الملخص
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

        #region Variance Data

        /// <summary>
        /// Period - الفترة
        /// 1-12 for months, or 1-4 for quarters
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "period")]
        public int Period { get; set; }

        /// <summary>
        /// Budget Amount - مبلغ الميزانية
        /// Snapshot of the budgeted amount
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "budget_amount", DecimalDigits = 2, DefaultValue = "0")]
        public decimal BudgetAmount { get; set; }

        /// <summary>
        /// Actual Amount - المبلغ الفعلي
        /// Calculated from GL entries at refresh time
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "actual_amount", DecimalDigits = 2, DefaultValue = "0")]
        public decimal ActualAmount { get; set; }

        /// <summary>
        /// Variance - الانحراف
        /// ActualAmount - BudgetAmount (positive = over budget)
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "variance", DecimalDigits = 2, DefaultValue = "0")]
        public decimal Variance { get; set; }

        /// <summary>
        /// Variance Percentage - نسبة الانحراف
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "variance_percentage", DecimalDigits = 2, DefaultValue = "0")]
        public decimal VariancePercentage { get; set; }

        /// <summary>
        /// Committed Amount - المبلغ الملتزم به
        /// From purchase orders
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "committed_amount", DecimalDigits = 2, DefaultValue = "0")]
        public decimal CommittedAmount { get; set; }

        /// <summary>
        /// Available Amount - المبلغ المتاح
        /// BudgetAmount - ActualAmount - CommittedAmount
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "available_amount", DecimalDigits = 2, DefaultValue = "0")]
        public decimal AvailableAmount { get; set; }

        #endregion

        #region Refresh Tracking

        /// <summary>
        /// Last Refreshed - آخر تحديث
        /// Timestamp of last background job refresh
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "last_refreshed")]
        public DateTime LastRefreshed { get; set; }

        /// <summary>
        /// Refreshed By Job ID - معرف مهمة التحديث
        /// ID of the background job that performed the refresh
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnName = "refreshed_by_job_id")]
        public string RefreshedByJobId { get; set; }

        /// <summary>
        /// Is Stale - قديم
        /// Indicates if data needs to be refreshed
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnName = "is_stale", DefaultValue = "0")]
        public bool IsStale { get; set; }

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

        #region Helper Properties (Ignored in DB)

        /// <summary>
        /// Is Over Budget - تجاوز الميزانية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsOverBudget => Variance > 0;

        /// <summary>
        /// Is Under Budget - تحت الميزانية
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsUnderBudget => Variance < 0;

        /// <summary>
        /// Utilization Percentage - نسبة الاستخدام
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal UtilizationPercentage => BudgetAmount != 0
            ? Math.Round((ActualAmount / BudgetAmount) * 100, 2)
            : 0;

        #endregion

        #region Methods

        /// <summary>
        /// Recalculate variance values
        /// Should only be called by background job
        /// </summary>
        public void Recalculate()
        {
            Variance = ActualAmount - BudgetAmount;
            VariancePercentage = BudgetAmount != 0
                ? Math.Round((Variance / BudgetAmount) * 100, 2)
                : 0;
            AvailableAmount = BudgetAmount - ActualAmount - CommittedAmount;
            IsStale = false;
            LastRefreshed = DateTime.Now;
        }

        /// <summary>
        /// Mark as needing refresh
        /// </summary>
        public void MarkStale()
        {
            IsStale = true;
        }

        #endregion
    }
}
