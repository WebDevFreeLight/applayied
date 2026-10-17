using System;
using System.Collections.Generic;

namespace ZR.Model.Finance.Dashboard
{
    /// <summary>
    /// Comprehensive Finance Dashboard DTO (SAP/Odoo-style)
    /// Enterprise-grade dashboard with KPIs, trends, and analytics
    /// </summary>
    public class FinanceDashboardDto
    {
        /// <summary>
        /// Top-level Financial KPIs
        /// </summary>
        public FinancialKPIsDto Kpis { get; set; }

        /// <summary>
        /// Module-specific summaries (GL, AR, AP, Treasury, Assets, Budget)
        /// </summary>
        public ModuleSummariesDto Modules { get; set; }

        /// <summary>
        /// Chart data for trend visualization
        /// </summary>
        public ChartDataDto Charts { get; set; }

        /// <summary>
        /// Alerts and notifications requiring attention
        /// </summary>
        public List<AlertDto> Alerts { get; set; }

        /// <summary>
        /// Recent financial activities
        /// </summary>
        public List<RecentActivityDto> RecentActivities { get; set; }

        /// <summary>
        /// Quick actions/shortcuts
        /// </summary>
        public List<QuickActionDto> QuickActions { get; set; }

        /// <summary>
        /// Period information
        /// </summary>
        public PeriodInfoDto PeriodInfo { get; set; }
    }

    #region Financial KPIs

    public class FinancialKPIsDto
    {
        // Balance Sheet KPIs
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal Equity { get; set; }
        public decimal CurrentAssets { get; set; }
        public decimal CurrentLiabilities { get; set; }
        public decimal CurrentRatio { get; set; }

        // Income Statement KPIs
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }

        // Cash Flow KPIs
        public decimal CashBalance { get; set; }
        public decimal CashFlowOperating { get; set; }
        public decimal CashFlowInvesting { get; set; }
        public decimal CashFlowFinancing { get; set; }

        // Working Capital
        public decimal WorkingCapital { get; set; }

        // Period comparisons
        public PercentageChangeDto RevenueChange { get; set; }
        public PercentageChangeDto ExpenseChange { get; set; }
        public PercentageChangeDto ProfitChange { get; set; }
    }

    public class PercentageChangeDto
    {
        public decimal CurrentValue { get; set; }
        public decimal PreviousValue { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public string Trend { get; set; } // "up", "down", "stable"
    }

    #endregion

    #region Module Summaries

    public class ModuleSummariesDto
    {
        public GLModuleSummaryDto GL { get; set; }
        public ARModuleSummaryDto AR { get; set; }
        public APModuleSummaryDto AP { get; set; }
        public TreasuryModuleSummaryDto Treasury { get; set; }
        public AssetsModuleSummaryDto Assets { get; set; }
        public BudgetModuleSummaryDto Budget { get; set; }
        public TaxModuleSummaryDto Tax { get; set; }
    }

    public class GLModuleSummaryDto
    {
        public int TotalAccounts { get; set; }
        public int PostableAccounts { get; set; }
        public int JournalsPostedToday { get; set; }
        public int JournalsPostedThisMonth { get; set; }
        public decimal TotalDebitAmount { get; set; }
        public decimal TotalCreditAmount { get; set; }
        public bool PeriodsInBalance { get; set; }
        public string CurrentPeriodStatus { get; set; } // "Open", "Closed", "Locked"
        public DateTime? LastClosingDate { get; set; }
    }

    public class ARModuleSummaryDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal CurrentDue { get; set; } // 0-30 days
        public decimal Overdue { get; set; } // 30+ days
        public decimal OverduePercent { get; set; }
        public int InvoicesCount { get; set; }
        public int OverdueInvoicesCount { get; set; }
        public decimal AverageDSO { get; set; } // Days Sales Outstanding
        public decimal CollectionRate { get; set; }
        public List<AgingBucketDto> AgingAnalysis { get; set; }
    }

    public class APModuleSummaryDto
    {
        public int TotalVendors { get; set; }
        public int ActiveVendors { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal CurrentDue { get; set; }
        public decimal Overdue { get; set; }
        public decimal OverduePercent { get; set; }
        public int InvoicesCount { get; set; }
        public int OverdueInvoicesCount { get; set; }
        public decimal AverageDPO { get; set; } // Days Payable Outstanding
        public List<AgingBucketDto> AgingAnalysis { get; set; }
    }

    public class AgingBucketDto
    {
        public string Range { get; set; } // "0-30", "31-60", "61-90", "90+"
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TreasuryModuleSummaryDto
    {
        public int BankAccounts { get; set; }
        public int CashAccounts { get; set; }
        public decimal TotalBankBalance { get; set; }
        public decimal TotalCashBalance { get; set; }
        public int UnreconciledTransactions { get; set; }
        public decimal UnreconciledAmount { get; set; }
        public int PendingCheques { get; set; }
        public decimal PendingChequesAmount { get; set; }
        public List<CurrencyBalanceDto> BalancesByCurrency { get; set; }
        public DateTime? LastReconciliationDate { get; set; }
    }

    public class CurrencyBalanceDto
    {
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public decimal BalanceInBaseCurrency { get; set; }
    }

    public class AssetsModuleSummaryDto
    {
        public int TotalFixedAssets { get; set; }
        public int TotalCurrentAssets { get; set; }
        public decimal FixedAssetsValue { get; set; }
        public decimal CurrentAssetsValue { get; set; }
        public decimal TotalDepreciation { get; set; }
        public decimal NetBookValue { get; set; }
        public int AssetsAddedThisMonth { get; set; }
        public int AssetsDisposedThisMonth { get; set; }
        public decimal DepreciationThisMonth { get; set; }
    }

    public class BudgetModuleSummaryDto
    {
        public int ActiveBudgets { get; set; }
        public decimal TotalBudgetedAmount { get; set; }
        public decimal ActualSpent { get; set; }
        public decimal Remaining { get; set; }
        public decimal UtilizationPercent { get; set; }
        public int OverBudgetItems { get; set; }
        public decimal AverageVariance { get; set; }
        public List<BudgetCategoryDto> TopCategories { get; set; }
    }

    public class BudgetCategoryDto
    {
        public string Category { get; set; }
        public decimal Budgeted { get; set; }
        public decimal Actual { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercent { get; set; }
    }

    public class TaxModuleSummaryDto
    {
        public decimal TaxCollected { get; set; }
        public decimal TaxPaid { get; set; }
        public decimal NetTaxLiability { get; set; }
        public int PendingTaxReturns { get; set; }
        public DateTime? NextFilingDate { get; set; }
    }

    #endregion

    #region Chart Data

    public class ChartDataDto
    {
        public List<TrendDataPoint> RevenueTrend { get; set; }
        public List<TrendDataPoint> ExpenseTrend { get; set; }
        public List<TrendDataPoint> ProfitTrend { get; set; }
        public List<TrendDataPoint> CashFlowTrend { get; set; }
        public List<CategoryDataPoint> RevenueByCategory { get; set; }
        public List<CategoryDataPoint> ExpenseByCategory { get; set; }
        public List<CategoryDataPoint> AgingDistribution { get; set; }
    }

    public class TrendDataPoint
    {
        public string Period { get; set; } // "Jan 2025", "Feb 2025", etc.
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
    }

    public class CategoryDataPoint
    {
        public string Category { get; set; }
        public decimal Value { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; }
    }

    #endregion

    #region Alerts

    public class AlertDto
    {
        public string Id { get; set; }
        public string Type { get; set; } // "warning", "error", "info", "success"
        public string Severity { get; set; } // "high", "medium", "low"
        public string Title { get; set; }
        public string Message { get; set; }
        public string Module { get; set; } // "GL", "AR", "AP", "Treasury", etc.
        public string ActionUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    #endregion

    #region Recent Activities

    public class RecentActivityDto
    {
        public string Id { get; set; }
        public string Type { get; set; } // "journal", "invoice", "payment", "reconciliation"
        public string Module { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string DetailUrl { get; set; }
    }

    #endregion

    #region Quick Actions

    public class QuickActionDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public string Category { get; set; } // "GL", "AR", "AP", "Treasury", "Reports"
        public bool IsEnabled { get; set; }
        public string RequiredPermission { get; set; }
    }

    #endregion

    #region Period Info

    public class PeriodInfoDto
    {
        public string CurrentFiscalYear { get; set; }
        public string CurrentPeriod { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string Status { get; set; } // "Open", "Closed", "Locked"
        public int DaysRemaining { get; set; }
        public DateTime? NextClosingDate { get; set; }
    }

    #endregion
}
