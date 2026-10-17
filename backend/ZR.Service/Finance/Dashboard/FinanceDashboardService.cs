using Infrastructure.Attribute;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.Finance.Dashboard;
using ZR.Service.IService.Finance.Dashboard;

namespace ZR.Service.Finance.Dashboard
{
    /// <summary>
    /// Finance Dashboard Service - Enterprise-grade dashboard aggregation
    /// Aggregates data from all finance modules (GL, AR, AP, Treasury, Assets, Budget)
    /// Similar to SAP Financial Cockpit or Odoo Finance Dashboard
    ///
    /// NOTE: This is a basic implementation returning mock data.
    /// TODO: Integrate actual data from finance module services (GL, AR, AP, Treasury, Assets, Budget)
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient, InterfaceServiceType = true)]
    public class FinanceDashboardService : IFinanceDashboardService
    {
        private readonly ILogger<FinanceDashboardService> _logger;

        public FinanceDashboardService(
            ILogger<FinanceDashboardService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive finance dashboard data
        /// </summary>
        public async Task<FinanceDashboardDto> GetDashboardAsync(DateTime? asOfDate = null, string fiscalYear = null, string period = null)
        {
            try
            {
                asOfDate ??= DateTime.Today;

                _logger.LogInformation("Building finance dashboard for date: {AsOfDate}", asOfDate);

                var dashboard = new FinanceDashboardDto
                {
                    Kpis = GetMockFinancialKPIs(),
                    Modules = GetMockModuleSummaries(),
                    Charts = GetMockChartData(),
                    Alerts = GetMockAlerts(),
                    RecentActivities = GetMockRecentActivities(),
                    QuickActions = GetQuickActions(),
                    PeriodInfo = GetPeriodInfo(fiscalYear, period)
                };

                _logger.LogInformation("Dashboard built successfully");
                return await Task.FromResult(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building finance dashboard");
                throw;
            }
        }

        #region Mock Data Methods

        private FinancialKPIsDto GetMockFinancialKPIs()
        {
            return new FinancialKPIsDto
            {
                // Balance Sheet KPIs
                TotalAssets = 5000000,
                TotalLiabilities = 2000000,
                Equity = 3000000,
                CurrentAssets = 2500000,
                CurrentLiabilities = 800000,
                CurrentRatio = 3.13m,

                // Income Statement KPIs
                TotalRevenue = 1500000,
                TotalExpenses = 1100000,
                GrossProfit = 600000,
                NetProfit = 400000,
                ProfitMargin = 26.67m,

                // Cash Flow KPIs
                CashBalance = 750000,
                CashFlowOperating = 350000,
                CashFlowInvesting = -100000,
                CashFlowFinancing = -50000,

                // Working Capital
                WorkingCapital = 1700000,

                // Period comparisons
                RevenueChange = new PercentageChangeDto
                {
                    CurrentValue = 1500000,
                    PreviousValue = 1400000,
                    Change = 100000,
                    ChangePercent = 7.14m,
                    Trend = "up"
                },
                ExpenseChange = new PercentageChangeDto
                {
                    CurrentValue = 1100000,
                    PreviousValue = 1050000,
                    Change = 50000,
                    ChangePercent = 4.76m,
                    Trend = "up"
                },
                ProfitChange = new PercentageChangeDto
                {
                    CurrentValue = 400000,
                    PreviousValue = 350000,
                    Change = 50000,
                    ChangePercent = 14.29m,
                    Trend = "up"
                }
            };
        }

        private ModuleSummariesDto GetMockModuleSummaries()
        {
            return new ModuleSummariesDto
            {
                GL = new GLModuleSummaryDto
                {
                    TotalAccounts = 250,
                    PostableAccounts = 180,
                    JournalsPostedToday = 12,
                    JournalsPostedThisMonth = 156,
                    TotalDebitAmount = 2500000,
                    TotalCreditAmount = 2500000,
                    PeriodsInBalance = true,
                    CurrentPeriodStatus = "Open",
                    LastClosingDate = DateTime.Today.AddMonths(-1)
                },
                AR = new ARModuleSummaryDto
                {
                    TotalCustomers = 85,
                    ActiveCustomers = 62,
                    TotalOutstanding = 450000,
                    CurrentDue = 300000,
                    Overdue = 150000,
                    OverduePercent = 33.33m,
                    InvoicesCount = 125,
                    OverdueInvoicesCount = 42,
                    AverageDSO = 35,
                    CollectionRate = 85.5m,
                    AgingAnalysis = new List<AgingBucketDto>
                    {
                        new AgingBucketDto { Range = "0-30", Amount = 300000, Count = 83, Percentage = 66.67m },
                        new AgingBucketDto { Range = "31-60", Amount = 80000, Count = 25, Percentage = 17.78m },
                        new AgingBucketDto { Range = "61-90", Amount = 40000, Count = 12, Percentage = 8.89m },
                        new AgingBucketDto { Range = "90+", Amount = 30000, Count = 5, Percentage = 6.67m }
                    }
                },
                AP = new APModuleSummaryDto
                {
                    TotalVendors = 45,
                    ActiveVendors = 32,
                    TotalOutstanding = 280000,
                    CurrentDue = 200000,
                    Overdue = 80000,
                    OverduePercent = 28.57m,
                    InvoicesCount = 78,
                    OverdueInvoicesCount = 22,
                    AverageDPO = 42,
                    AgingAnalysis = new List<AgingBucketDto>
                    {
                        new AgingBucketDto { Range = "0-30", Amount = 200000, Count = 56, Percentage = 71.43m },
                        new AgingBucketDto { Range = "31-60", Amount = 50000, Count = 15, Percentage = 17.86m },
                        new AgingBucketDto { Range = "61-90", Amount = 20000, Count = 5, Percentage = 7.14m },
                        new AgingBucketDto { Range = "90+", Amount = 10000, Count = 2, Percentage = 3.57m }
                    }
                },
                Treasury = new TreasuryModuleSummaryDto
                {
                    BankAccounts = 5,
                    CashAccounts = 2,
                    TotalBankBalance = 650000,
                    TotalCashBalance = 100000,
                    UnreconciledTransactions = 23,
                    UnreconciledAmount = 45000,
                    PendingCheques = 8,
                    PendingChequesAmount = 125000,
                    BalancesByCurrency = new List<CurrencyBalanceDto>
                    {
                        new CurrencyBalanceDto { Currency = "SAR", Balance = 750000, BalanceInBaseCurrency = 750000 }
                    },
                    LastReconciliationDate = DateTime.Today.AddDays(-3)
                },
                Assets = new AssetsModuleSummaryDto
                {
                    TotalFixedAssets = 35,
                    TotalCurrentAssets = 120,
                    FixedAssetsValue = 1800000,
                    CurrentAssetsValue = 850000,
                    TotalDepreciation = 420000,
                    NetBookValue = 1380000,
                    AssetsAddedThisMonth = 3,
                    AssetsDisposedThisMonth = 1,
                    DepreciationThisMonth = 15000
                },
                Budget = new BudgetModuleSummaryDto
                {
                    ActiveBudgets = 12,
                    TotalBudgetedAmount = 2000000,
                    ActualSpent = 1650000,
                    Remaining = 350000,
                    UtilizationPercent = 82.5m,
                    OverBudgetItems = 3,
                    AverageVariance = -5.2m,
                    TopCategories = new List<BudgetCategoryDto>
                    {
                        new BudgetCategoryDto
                        {
                            Category = "Operations",
                            Budgeted = 800000,
                            Actual = 750000,
                            Variance = -50000,
                            VariancePercent = -6.25m
                        },
                        new BudgetCategoryDto
                        {
                            Category = "Marketing",
                            Budgeted = 300000,
                            Actual = 320000,
                            Variance = 20000,
                            VariancePercent = 6.67m
                        },
                        new BudgetCategoryDto
                        {
                            Category = "Administration",
                            Budgeted = 500000,
                            Actual = 480000,
                            Variance = -20000,
                            VariancePercent = -4.0m
                        }
                    }
                },
                Tax = new TaxModuleSummaryDto
                {
                    TaxCollected = 195000,
                    TaxPaid = 143000,
                    NetTaxLiability = 52000,
                    PendingTaxReturns = 1,
                    NextFilingDate = DateTime.Today.AddDays(15)
                }
            };
        }

        private ChartDataDto GetMockChartData()
        {
            var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var revenueTrend = new List<TrendDataPoint>();
            var expenseTrend = new List<TrendDataPoint>();
            var profitTrend = new List<TrendDataPoint>();
            var cashFlowTrend = new List<TrendDataPoint>();

            var baseRevenue = 1200000;
            var baseExpense = 950000;
            var baseCashFlow = 600000;

            for (int i = 0; i < 12; i++)
            {
                var date = DateTime.Today.AddMonths(-(11 - i));
                var revenue = baseRevenue + (i * 25000) + (new Random().Next(-50000, 50000));
                var expense = baseExpense + (i * 12500) + (new Random().Next(-30000, 30000));
                var cashFlow = baseCashFlow + (i * 5000) + (new Random().Next(-20000, 20000));

                revenueTrend.Add(new TrendDataPoint
                {
                    Period = months[i],
                    Date = date,
                    Value = revenue
                });

                expenseTrend.Add(new TrendDataPoint
                {
                    Period = months[i],
                    Date = date,
                    Value = expense
                });

                profitTrend.Add(new TrendDataPoint
                {
                    Period = months[i],
                    Date = date,
                    Value = revenue - expense
                });

                cashFlowTrend.Add(new TrendDataPoint
                {
                    Period = months[i],
                    Date = date,
                    Value = cashFlow
                });
            }

            return new ChartDataDto
            {
                RevenueTrend = revenueTrend,
                ExpenseTrend = expenseTrend,
                ProfitTrend = profitTrend,
                CashFlowTrend = cashFlowTrend,
                RevenueByCategory = new List<CategoryDataPoint>
                {
                    new CategoryDataPoint { Category = "Product Sales", Value = 800000, Percentage = 53.33m, Color = "#10b981" },
                    new CategoryDataPoint { Category = "Services", Value = 500000, Percentage = 33.33m, Color = "#3b82f6" },
                    new CategoryDataPoint { Category = "Other Income", Value = 200000, Percentage = 13.33m, Color = "#8b5cf6" }
                },
                ExpenseByCategory = new List<CategoryDataPoint>
                {
                    new CategoryDataPoint { Category = "Salaries", Value = 500000, Percentage = 45.45m, Color = "#ef4444" },
                    new CategoryDataPoint { Category = "Operations", Value = 350000, Percentage = 31.82m, Color = "#f59e0b" },
                    new CategoryDataPoint { Category = "Marketing", Value = 150000, Percentage = 13.64m, Color = "#ec4899" },
                    new CategoryDataPoint { Category = "Other", Value = 100000, Percentage = 9.09m, Color = "#6b7280" }
                },
                AgingDistribution = new List<CategoryDataPoint>
                {
                    new CategoryDataPoint { Category = "0-30 Days", Value = 300000, Percentage = 66.67m, Color = "#10b981" },
                    new CategoryDataPoint { Category = "31-60 Days", Value = 80000, Percentage = 17.78m, Color = "#f59e0b" },
                    new CategoryDataPoint { Category = "61-90 Days", Value = 40000, Percentage = 8.89m, Color = "#ef4444" },
                    new CategoryDataPoint { Category = "90+ Days", Value = 30000, Percentage = 6.67m, Color = "#dc2626" }
                }
            };
        }

        private List<AlertDto> GetMockAlerts()
        {
            return new List<AlertDto>
            {
                new AlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "warning",
                    Severity = "high",
                    Title = "Overdue Customer Invoices",
                    Message = "42 customer invoices are overdue totaling 150,000 SAR",
                    Module = "AR",
                    ActionUrl = "/finance/arap/ar-invoices?status=overdue",
                    CreatedAt = DateTime.Now.AddHours(-2),
                    IsRead = false
                },
                new AlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "info",
                    Severity = "medium",
                    Title = "Period Closing Reminder",
                    Message = "Current period closes in 5 days. Ensure all transactions are posted.",
                    Module = "GL",
                    ActionUrl = "/finance/gl/period-closing",
                    CreatedAt = DateTime.Now.AddHours(-5),
                    IsRead = false
                },
                new AlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "warning",
                    Severity = "medium",
                    Title = "Pending Bank Reconciliations",
                    Message = "23 bank transactions need reconciliation",
                    Module = "Treasury",
                    ActionUrl = "/finance/treasury/reconciliation",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    IsRead = false
                },
                new AlertDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "success",
                    Severity = "low",
                    Title = "Budget Performance",
                    Message = "Operations department is 6.25% under budget this month",
                    Module = "Budget",
                    ActionUrl = "/finance/budget",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    IsRead = true
                }
            };
        }

        private List<RecentActivityDto> GetMockRecentActivities()
        {
            return new List<RecentActivityDto>
            {
                new RecentActivityDto
                {
                    Id = "1",
                    Type = "journal",
                    Module = "GL",
                    Title = "Journal Entry #JV-2025-0543",
                    Description = "Salary payment posting",
                    Amount = 125000,
                    Currency = "SAR",
                    UserName = "Ahmed Al-Mansour",
                    CreatedAt = DateTime.Now.AddHours(-1),
                    Status = "Posted",
                    DetailUrl = "/finance/gl/journals/543"
                },
                new RecentActivityDto
                {
                    Id = "2",
                    Type = "invoice",
                    Module = "AR",
                    Title = "Customer Invoice #INV-2025-1245",
                    Description = "ABC Company - Product sales",
                    Amount = 45000,
                    Currency = "SAR",
                    UserName = "Sara Al-Qahtani",
                    CreatedAt = DateTime.Now.AddHours(-3),
                    Status = "Pending",
                    DetailUrl = "/finance/arap/ar-invoices/1245"
                },
                new RecentActivityDto
                {
                    Id = "3",
                    Type = "payment",
                    Module = "Treasury",
                    Title = "Payment Received #PMT-2025-0789",
                    Description = "Customer payment - XYZ Corp",
                    Amount = 78000,
                    Currency = "SAR",
                    UserName = "Mohammed Al-Harbi",
                    CreatedAt = DateTime.Now.AddHours(-5),
                    Status = "Cleared",
                    DetailUrl = "/finance/treasury/payments/789"
                },
                new RecentActivityDto
                {
                    Id = "4",
                    Type = "invoice",
                    Module = "AP",
                    Title = "Vendor Invoice #APINV-2025-0456",
                    Description = "Office supplies purchase",
                    Amount = 12500,
                    Currency = "SAR",
                    UserName = "Fatima Al-Dosari",
                    CreatedAt = DateTime.Now.AddHours(-7),
                    Status = "Approved",
                    DetailUrl = "/finance/arap/ap-invoices/456"
                },
                new RecentActivityDto
                {
                    Id = "5",
                    Type = "reconciliation",
                    Module = "Treasury",
                    Title = "Bank Reconciliation #BR-2025-023",
                    Description = "Al Rajhi Bank - Account ending 4521",
                    Amount = 0,
                    Currency = "SAR",
                    UserName = "Omar Al-Shehri",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Status = "Completed",
                    DetailUrl = "/finance/treasury/reconciliation/23"
                }
            };
        }

        private List<QuickActionDto> GetQuickActions()
        {
            return new List<QuickActionDto>
            {
                new QuickActionDto
                {
                    Id = "gl-journal",
                    Title = "New Journal Entry",
                    Description = "Create a new GL journal entry",
                    Icon = "PiNotebookBold",
                    Url = "/finance/gl/journals/create",
                    Category = "GL",
                    IsEnabled = true,
                    RequiredPermission = "finance:gl:journal:create"
                },
                new QuickActionDto
                {
                    Id = "ar-invoice",
                    Title = "New AR Invoice",
                    Description = "Create customer invoice",
                    Icon = "PiReceiptBold",
                    Url = "/finance/arap/ar-invoices/create",
                    Category = "AR",
                    IsEnabled = true,
                    RequiredPermission = "finance:ar:invoice:create"
                },
                new QuickActionDto
                {
                    Id = "ap-invoice",
                    Title = "New AP Invoice",
                    Description = "Create vendor invoice",
                    Icon = "PiReceiptBold",
                    Url = "/finance/arap/ap-invoices/create",
                    Category = "AP",
                    IsEnabled = true,
                    RequiredPermission = "finance:ap:invoice:create"
                },
                new QuickActionDto
                {
                    Id = "treasury-payment",
                    Title = "Record Payment",
                    Description = "Record customer/vendor payment",
                    Icon = "PiCurrencyCircleDollarBold",
                    Url = "/finance/treasury/payments/create",
                    Category = "Treasury",
                    IsEnabled = true,
                    RequiredPermission = "finance:treasury:payment:create"
                },
                new QuickActionDto
                {
                    Id = "treasury-cheque",
                    Title = "Issue Cheque",
                    Description = "Issue a new cheque",
                    Icon = "PiMoneyBold",
                    Url = "/finance/treasury/cheques/create",
                    Category = "Treasury",
                    IsEnabled = true,
                    RequiredPermission = "finance:treasury:cheque:create"
                },
                new QuickActionDto
                {
                    Id = "treasury-reconciliation",
                    Title = "Bank Reconciliation",
                    Description = "Reconcile bank statements",
                    Icon = "PiBankBold",
                    Url = "/finance/treasury/reconciliation",
                    Category = "Treasury",
                    IsEnabled = true,
                    RequiredPermission = "finance:treasury:reconciliation"
                },
                new QuickActionDto
                {
                    Id = "reports-trial-balance",
                    Title = "Trial Balance",
                    Description = "View trial balance report",
                    Icon = "PiChartLineBold",
                    Url = "/finance/reports/trial-balance",
                    Category = "Reports",
                    IsEnabled = true,
                    RequiredPermission = "finance:reports:view"
                },
                new QuickActionDto
                {
                    Id = "reports-balance-sheet",
                    Title = "Balance Sheet",
                    Description = "View balance sheet",
                    Icon = "PiChartBarBold",
                    Url = "/finance/reports/balance-sheet",
                    Category = "Reports",
                    IsEnabled = true,
                    RequiredPermission = "finance:reports:view"
                },
                new QuickActionDto
                {
                    Id = "reports-income-statement",
                    Title = "Income Statement",
                    Description = "View P&L report",
                    Icon = "PiTrendUpBold",
                    Url = "/finance/reports/income-statement",
                    Category = "Reports",
                    IsEnabled = true,
                    RequiredPermission = "finance:reports:view"
                },
                new QuickActionDto
                {
                    Id = "reports-cash-flow",
                    Title = "Cash Flow Statement",
                    Description = "View cash flow (IAS 7)",
                    Icon = "PiFlowArrowBold",
                    Url = "/finance/reports/cash-flow",
                    Category = "Reports",
                    IsEnabled = true,
                    RequiredPermission = "finance:reports:view"
                }
            };
        }

        private PeriodInfoDto GetPeriodInfo(string fiscalYear, string period)
        {
            var today = DateTime.Today;
            var currentYear = fiscalYear ?? today.Year.ToString();
            var currentPeriod = period ?? today.Month.ToString("D2");
            var periodEnd = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

            return new PeriodInfoDto
            {
                CurrentFiscalYear = currentYear,
                CurrentPeriod = currentPeriod,
                PeriodStartDate = new DateTime(today.Year, today.Month, 1),
                PeriodEndDate = periodEnd,
                Status = "Open",
                DaysRemaining = (periodEnd - today).Days,
                NextClosingDate = periodEnd.AddDays(1)
            };
        }

        #endregion
    }
}
