# Finance Module - Detailed Implementation Plan

## Overview
This document provides a comprehensive implementation plan for the Finance module, covering accounting, budgeting, financial reporting, expense management, and financial analytics dashboard capabilities.

## Finance Module Architecture

### Core Components
1. **Accounting Management**
2. **Budget Planning & Control**
3. **Expense Management**
4. **Financial Reporting**
5. **Cash Flow Management**
6. **Asset Management**
7. **Financial Analytics Dashboard**
8. **Audit Trail**

## 1. Backend Implementation

### Controllers

**FinanceDashboardController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Finance,Admin,SuperAdmin")]
public class FinanceDashboardController : BaseController
{
    private readonly IFinanceDashboardService _financeDashboardService;
    private readonly IFinancialAnalyticsService _analyticsService;
    
    public FinanceDashboardController(
        IFinanceDashboardService financeDashboardService,
        IFinancialAnalyticsService analyticsService)
    {
        _financeDashboardService = financeDashboardService;
        _analyticsService = analyticsService;
    }
    
    [HttpGet("overview")]
    public async Task<IActionResult> GetDashboardOverview([FromQuery] string period = "month")
    {
        var overview = await _financeDashboardService.GetDashboardOverviewAsync(period);
        return Ok(overview);
    }
    
    [HttpGet("financial-summary")]
    public async Task<IActionResult> GetFinancialSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var summary = await _financeDashboardService.GetFinancialSummaryAsync(startDate, endDate);
        return Ok(summary);
    }
    
    [HttpGet("cash-flow")]
    public async Task<IActionResult> GetCashFlow([FromQuery] string period = "month")
    {
        var cashFlow = await _financeDashboardService.GetCashFlowAsync(period);
        return Ok(cashFlow);
    }
    
    [HttpGet("budget-vs-actual")]
    public async Task<IActionResult> GetBudgetVsActual([FromQuery] int year, [FromQuery] int? month = null)
    {
        var comparison = await _financeDashboardService.GetBudgetVsActualAsync(year, month);
        return Ok(comparison);
    }
    
    [HttpGet("expense-analysis")]
    public async Task<IActionResult> GetExpenseAnalysis([FromQuery] string period = "month")
    {
        var analysis = await _analyticsService.GetExpenseAnalysisAsync(period);
        return Ok(analysis);
    }
    
    [HttpGet("revenue-trends")]
    public async Task<IActionResult> GetRevenueTrends([FromQuery] int months = 12)
    {
        var trends = await _analyticsService.GetRevenueTrendsAsync(months);
        return Ok(trends);
    }
    
    [HttpGet("financial-ratios")]
    public async Task<IActionResult> GetFinancialRatios([FromQuery] string period = "quarter")
    {
        var ratios = await _analyticsService.GetFinancialRatiosAsync(period);
        return Ok(ratios);
    }
    
    [HttpGet("profit-loss")]
    public async Task<IActionResult> GetProfitLoss([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var profitLoss = await _financeDashboardService.GetProfitLossAsync(startDate, endDate);
        return Ok(profitLoss);
    }
}
```

**AccountingController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Finance,Accountant,Admin")]
public class AccountingController : BaseController
{
    private readonly IAccountingService _accountingService;
    private readonly IChartOfAccountsService _chartOfAccountsService;
    private readonly IJournalEntryService _journalEntryService;
    
    public AccountingController(
        IAccountingService accountingService,
        IChartOfAccountsService chartOfAccountsService,
        IJournalEntryService journalEntryService)
    {
        _accountingService = accountingService;
        _chartOfAccountsService = chartOfAccountsService;
        _journalEntryService = journalEntryService;
    }
    
    [HttpGet("chart-of-accounts")]
    public async Task<IActionResult> GetChartOfAccounts([FromQuery] AccountQueryDto query)
    {
        var accounts = await _chartOfAccountsService.GetAccountsAsync(query);
        return Ok(accounts);
    }
    
    [HttpPost("chart-of-accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto account)
    {
        var result = await _chartOfAccountsService.CreateAccountAsync(account);
        return Ok(result);
    }
    
    [HttpPut("chart-of-accounts/{id}")]
    public async Task<IActionResult> UpdateAccount(long id, [FromBody] UpdateAccountDto account)
    {
        await _chartOfAccountsService.UpdateAccountAsync(id, account);
        return Ok(new { message = "Account updated successfully" });
    }
    
    [HttpGet("journal-entries")]
    public async Task<IActionResult> GetJournalEntries([FromQuery] JournalEntryQueryDto query)
    {
        var entries = await _journalEntryService.GetJournalEntriesAsync(query);
        return Ok(entries);
    }
    
    [HttpPost("journal-entries")]
    public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryDto entry)
    {
        var result = await _journalEntryService.CreateJournalEntryAsync(entry);
        return Ok(result);
    }
    
    [HttpPut("journal-entries/{id}")]
    public async Task<IActionResult> UpdateJournalEntry(long id, [FromBody] UpdateJournalEntryDto entry)
    {
        await _journalEntryService.UpdateJournalEntryAsync(id, entry);
        return Ok(new { message = "Journal entry updated successfully" });
    }
    
    [HttpPost("journal-entries/{id}/post")]
    public async Task<IActionResult> PostJournalEntry(long id)
    {
        await _journalEntryService.PostJournalEntryAsync(id);
        return Ok(new { message = "Journal entry posted successfully" });
    }
    
    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance([FromQuery] DateTime asOfDate)
    {
        var trialBalance = await _accountingService.GetTrialBalanceAsync(asOfDate);
        return Ok(trialBalance);
    }
    
    [HttpGet("general-ledger")]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] GeneralLedgerQueryDto query)
    {
        var ledger = await _accountingService.GetGeneralLedgerAsync(query);
        return Ok(ledger);
    }
}
```

**BudgetController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Finance,BudgetManager,Admin")]
public class BudgetController : BaseController
{
    private readonly IBudgetService _budgetService;
    private readonly IBudgetAnalysisService _budgetAnalysisService;
    
    public BudgetController(
        IBudgetService budgetService,
        IBudgetAnalysisService budgetAnalysisService)
    {
        _budgetService = budgetService;
        _budgetAnalysisService = budgetAnalysisService;
    }
    
    [HttpGet("budgets")]
    public async Task<IActionResult> GetBudgets([FromQuery] BudgetQueryDto query)
    {
        var budgets = await _budgetService.GetBudgetsAsync(query);
        return Ok(budgets);
    }
    
    [HttpGet("budgets/{id}")]
    public async Task<IActionResult> GetBudget(long id)
    {
        var budget = await _budgetService.GetBudgetByIdAsync(id);
        return Ok(budget);
    }
    
    [HttpPost("budgets")]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto budget)
    {
        var result = await _budgetService.CreateBudgetAsync(budget);
        return Ok(result);
    }
    
    [HttpPut("budgets/{id}")]
    public async Task<IActionResult> UpdateBudget(long id, [FromBody] UpdateBudgetDto budget)
    {
        await _budgetService.UpdateBudgetAsync(id, budget);
        return Ok(new { message = "Budget updated successfully" });
    }
    
    [HttpPost("budgets/{id}/approve")]
    public async Task<IActionResult> ApproveBudget(long id, [FromBody] ApproveBudgetDto approval)
    {
        await _budgetService.ApproveBudgetAsync(id, approval);
        return Ok(new { message = "Budget approved successfully" });
    }
    
    [HttpGet("budgets/{id}/variance-analysis")]
    public async Task<IActionResult> GetVarianceAnalysis(long id, [FromQuery] DateTime? asOfDate)
    {
        var analysis = await _budgetAnalysisService.GetVarianceAnalysisAsync(id, asOfDate);
        return Ok(analysis);
    }
    
    [HttpGet("budget-templates")]
    public async Task<IActionResult> GetBudgetTemplates()
    {
        var templates = await _budgetService.GetBudgetTemplatesAsync();
        return Ok(templates);
    }
    
    [HttpPost("budgets/{id}/copy")]
    public async Task<IActionResult> CopyBudget(long id, [FromBody] CopyBudgetDto copyRequest)
    {
        var result = await _budgetService.CopyBudgetAsync(id, copyRequest);
        return Ok(result);
    }
}
```

**ExpenseController.cs**
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExpenseController : BaseController
{
    private readonly IExpenseService _expenseService;
    private readonly IExpenseApprovalService _approvalService;
    
    public ExpenseController(
        IExpenseService expenseService,
        IExpenseApprovalService approvalService)
    {
        _expenseService = expenseService;
        _approvalService = approvalService;
    }
    
    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpenses([FromQuery] ExpenseQueryDto query)
    {
        var expenses = await _expenseService.GetExpensesAsync(query);
        return Ok(expenses);
    }
    
    [HttpGet("expenses/{id}")]
    public async Task<IActionResult> GetExpense(long id)
    {
        var expense = await _expenseService.GetExpenseByIdAsync(id);
        return Ok(expense);
    }
    
    [HttpPost("expenses")]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto expense)
    {
        var result = await _expenseService.CreateExpenseAsync(expense);
        return Ok(result);
    }
    
    [HttpPut("expenses/{id}")]
    public async Task<IActionResult> UpdateExpense(long id, [FromBody] UpdateExpenseDto expense)
    {
        await _expenseService.UpdateExpenseAsync(id, expense);
        return Ok(new { message = "Expense updated successfully" });
    }
    
    [HttpPost("expenses/{id}/submit")]
    public async Task<IActionResult> SubmitExpense(long id)
    {
        await _expenseService.SubmitExpenseAsync(id);
        return Ok(new { message = "Expense submitted for approval" });
    }
    
    [HttpPost("expenses/{id}/approve")]
    [Authorize(Roles = "Finance,ExpenseApprover,Admin")]
    public async Task<IActionResult> ApproveExpense(long id, [FromBody] ApproveExpenseDto approval)
    {
        await _approvalService.ApproveExpenseAsync(id, approval);
        return Ok(new { message = "Expense approved successfully" });
    }
    
    [HttpPost("expenses/{id}/reject")]
    [Authorize(Roles = "Finance,ExpenseApprover,Admin")]
    public async Task<IActionResult> RejectExpense(long id, [FromBody] RejectExpenseDto rejection)
    {
        await _approvalService.RejectExpenseAsync(id, rejection);
        return Ok(new { message = "Expense rejected" });
    }
    
    [HttpGet("expense-categories")]
    public async Task<IActionResult> GetExpenseCategories()
    {
        var categories = await _expenseService.GetExpenseCategoriesAsync();
        return Ok(categories);
    }
    
    [HttpGet("expense-reports")]
    public async Task<IActionResult> GetExpenseReports([FromQuery] ExpenseReportQueryDto query)
    {
        var reports = await _expenseService.GetExpenseReportsAsync(query);
        return Ok(reports);
    }
}
```

### Services

**IFinanceDashboardService.cs**
```csharp
public interface IFinanceDashboardService
{
    Task<FinanceDashboardOverviewDto> GetDashboardOverviewAsync(string period);
    Task<FinancialSummaryDto> GetFinancialSummaryAsync(DateTime? startDate, DateTime? endDate);
    Task<CashFlowDto> GetCashFlowAsync(string period);
    Task<BudgetVsActualDto> GetBudgetVsActualAsync(int year, int? month);
    Task<ProfitLossDto> GetProfitLossAsync(DateTime startDate, DateTime endDate);
    Task<List<FinancialKpiDto>> GetFinancialKpisAsync(string period);
}
```

**FinanceDashboardService.cs**
```csharp
[AppService(ServiceLifetime = LifeTime.Transient)]
public class FinanceDashboardService : BaseService<Account>, IFinanceDashboardService
{
    private readonly IAccountingService _accountingService;
    private readonly IBudgetService _budgetService;
    private readonly IExpenseService _expenseService;
    private readonly IRevenueService _revenueService;
    
    public FinanceDashboardService(
        IAccountingService accountingService,
        IBudgetService budgetService,
        IExpenseService expenseService,
        IRevenueService revenueService)
    {
        _accountingService = accountingService;
        _budgetService = budgetService;
        _expenseService = expenseService;
        _revenueService = revenueService;
    }
    
    public async Task<FinanceDashboardOverviewDto> GetDashboardOverviewAsync(string period)
    {
        var startDate = GetStartDateForPeriod(period);
        var endDate = DateTime.UtcNow;
        
        var totalRevenue = await _revenueService.GetTotalRevenueAsync(startDate, endDate);
        var totalExpenses = await _expenseService.GetTotalExpensesAsync(startDate, endDate);
        var netIncome = totalRevenue - totalExpenses;
        var cashBalance = await _accountingService.GetCashBalanceAsync();
        var accountsReceivable = await _accountingService.GetAccountsReceivableAsync();
        var accountsPayable = await _accountingService.GetAccountsPayableAsync();
        var budgetUtilization = await _budgetService.GetBudgetUtilizationAsync(startDate, endDate);
        
        return new FinanceDashboardOverviewDto
        {
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            NetIncome = netIncome,
            CashBalance = cashBalance,
            AccountsReceivable = accountsReceivable,
            AccountsPayable = accountsPayable,
            BudgetUtilization = budgetUtilization,
            Period = period,
            StartDate = startDate,
            EndDate = endDate,
            LastUpdated = DateTime.UtcNow
        };
    }
    
    public async Task<FinancialSummaryDto> GetFinancialSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;
        
        var revenueByCategory = await _revenueService.GetRevenueByCategoryAsync(start, end);
        var expensesByCategory = await _expenseService.GetExpensesByCategoryAsync(start, end);
        var monthlyTrends = await GetMonthlyTrendsAsync(start, end);
        var topExpenseCategories = await _expenseService.GetTopExpenseCategoriesAsync(start, end, 10);
        
        return new FinancialSummaryDto
        {
            RevenueByCategory = revenueByCategory,
            ExpensesByCategory = expensesByCategory,
            MonthlyTrends = monthlyTrends,
            TopExpenseCategories = topExpenseCategories,
            StartDate = start,
            EndDate = end
        };
    }
    
    public async Task<CashFlowDto> GetCashFlowAsync(string period)
    {
        var startDate = GetStartDateForPeriod(period);
        var endDate = DateTime.UtcNow;
        
        var operatingCashFlow = await _accountingService.GetOperatingCashFlowAsync(startDate, endDate);
        var investingCashFlow = await _accountingService.GetInvestingCashFlowAsync(startDate, endDate);
        var financingCashFlow = await _accountingService.GetFinancingCashFlowAsync(startDate, endDate);
        var netCashFlow = operatingCashFlow + investingCashFlow + financingCashFlow;
        var cashFlowTrend = await GetCashFlowTrendAsync(startDate, endDate);
        
        return new CashFlowDto
        {
            OperatingCashFlow = operatingCashFlow,
            InvestingCashFlow = investingCashFlow,
            FinancingCashFlow = financingCashFlow,
            NetCashFlow = netCashFlow,
            CashFlowTrend = cashFlowTrend,
            Period = period,
            StartDate = startDate,
            EndDate = endDate
        };
    }
    
    // Additional method implementations...
}
```

### Models and DTOs

**FinanceDashboardOverviewDto.cs**
```csharp
public class FinanceDashboardOverviewDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome { get; set; }
    public decimal CashBalance { get; set; }
    public decimal AccountsReceivable { get; set; }
    public decimal AccountsPayable { get; set; }
    public decimal BudgetUtilization { get; set; }
    public string Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

**Account.cs**
```csharp
[SugarTable("finance_accounts")]
public class Account : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long AccountId { get; set; }
    
    [SugarColumn(Length = 20)]
    public string AccountCode { get; set; }
    
    [SugarColumn(Length = 200)]
    public string AccountName { get; set; }
    
    [SugarColumn(Length = 50)]
    public string AccountType { get; set; } // Asset, Liability, Equity, Revenue, Expense
    
    [SugarColumn(Length = 50)]
    public string AccountSubType { get; set; }
    
    public long? ParentAccountId { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal Balance { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal DebitBalance { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal CreditBalance { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsSystem { get; set; } = false;
    
    [SugarColumn(Length = 500)]
    public string Description { get; set; }
    
    [SugarColumn(Length = 10)]
    public string CurrencyCode { get; set; } = "USD";
    
    public int Level { get; set; } = 1;
    
    [Navigate(NavigateType.OneToMany, nameof(JournalEntryLine.AccountId))]
    public List<JournalEntryLine> JournalEntryLines { get; set; }
}
```

**JournalEntry.cs**
```csharp
[SugarTable("finance_journal_entries")]
public class JournalEntry : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long JournalEntryId { get; set; }
    
    [SugarColumn(Length = 50)]
    public string EntryNumber { get; set; }
    
    public DateTime EntryDate { get; set; }
    
    [SugarColumn(Length = 500)]
    public string Description { get; set; }
    
    [SugarColumn(Length = 50)]
    public string Reference { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal TotalDebit { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal TotalCredit { get; set; }
    
    [SugarColumn(Length = 20)]
    public string Status { get; set; } = "Draft"; // Draft, Posted, Reversed
    
    public DateTime? PostedDate { get; set; }
    
    public long? PostedBy { get; set; }
    
    [SugarColumn(Length = 50)]
    public string Source { get; set; } // Manual, System, Import
    
    public long? SourceId { get; set; }
    
    [Navigate(NavigateType.OneToMany, nameof(JournalEntryLine.JournalEntryId))]
    public List<JournalEntryLine> Lines { get; set; }
}
```

**Budget.cs**
```csharp
[SugarTable("finance_budgets")]
public class Budget : BaseEntity
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long BudgetId { get; set; }
    
    [SugarColumn(Length = 200)]
    public string BudgetName { get; set; }
    
    [SugarColumn(Length = 500)]
    public string Description { get; set; }
    
    public int BudgetYear { get; set; }
    
    [SugarColumn(Length = 20)]
    public string BudgetType { get; set; } // Annual, Quarterly, Monthly, Project
    
    [SugarColumn(Length = 20)]
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Active, Closed
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal TotalBudgetAmount { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal ActualAmount { get; set; }
    
    [SugarColumn(DecimalDigits = 2)]
    public decimal VarianceAmount { get; set; }
    
    public decimal VariancePercentage { get; set; }
    
    public long? ApprovedBy { get; set; }
    
    public DateTime? ApprovedDate { get; set; }
    
    public long? DepartmentId { get; set; }
    
    [Navigate(NavigateType.OneToMany, nameof(BudgetLine.BudgetId))]
    public List<BudgetLine> BudgetLines { get; set; }
}
```

## 2. Frontend Implementation

### Finance Dashboard Vue Component

**views/finance/Dashboard.vue**
```vue
<template>
  <div class="finance-dashboard">
    <!-- Dashboard Header -->
    <div class="dashboard-header">
      <h1>{{ $t('finance.dashboard.title') }}</h1>
      <div class="header-actions">
        <el-select v-model="selectedPeriod" @change="changePeriod">
          <el-option
            v-for="period in periods"
            :key="period.value"
            :label="period.label"
            :value="period.value"
          />
        </el-select>
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          :range-separator="$t('common.to')"
          :start-placeholder="$t('common.startDate')"
          :end-placeholder="$t('common.endDate')"
          @change="handleDateRangeChange"
        />
        <el-button type="primary" @click="refreshDashboard">
          <i class="el-icon-refresh"></i>
          {{ $t('common.refresh') }}
        </el-button>
      </div>
    </div>

    <!-- Financial KPIs -->
    <div class="kpi-grid">
      <FinanceKpiCard
        v-for="kpi in financialKpis"
        :key="kpi.key"
        :title="kpi.title"
        :value="kpi.value"
        :change="kpi.change"
        :trend="kpi.trend"
        :icon="kpi.icon"
        :color="kpi.color"
        :format="kpi.format"
      />
    </div>

    <!-- Financial Overview Charts -->
    <div class="charts-section">
      <el-row :gutter="20">
        <el-col :span="12">
          <div class="chart-container">
            <div class="chart-header">
              <h3>{{ $t('finance.dashboard.revenueVsExpenses') }}</h3>
              <el-button-group size="small">
                <el-button 
                  v-for="view in chartViews" 
                  :key="view.value"
                  :type="revenueExpenseView === view.value ? 'primary' : 'default'"
                  @click="revenueExpenseView = view.value"
                >
                  {{ view.label }}
                </el-button>
              </el-button-group>
            </div>
            <RevenueExpenseChart 
              :data="revenueExpenseData" 
              :view="revenueExpenseView"
            />
          </div>
        </el-col>
        <el-col :span="12">
          <div class="chart-container">
            <h3>{{ $t('finance.dashboard.cashFlow') }}</h3>
            <CashFlowChart :data="cashFlowData" />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="20" style="margin-top: 20px;">
        <el-col :span="8">
          <div class="chart-container">
            <h3>{{ $t('finance.dashboard.expensesByCategory') }}</h3>
            <ExpenseCategoryChart :data="expensesByCategory" />
          </div>
        </el-col>
        <el-col :span="8">
          <div class="chart-container">
            <h3>{{ $t('finance.dashboard.budgetVsActual') }}</h3>
            <BudgetVsActualChart :data="budgetVsActual" />
          </div>
        </el-col>
        <el-col :span="8">
          <div class="chart-container">
            <h3>{{ $t('finance.dashboard.profitMargin') }}</h3>
            <ProfitMarginChart :data="profitMarginData" />
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- Financial Ratios -->
    <div class="ratios-section">
      <h3>{{ $t('finance.dashboard.financialRatios') }}</h3>
      <FinancialRatiosGrid :ratios="financialRatios" />
    </div>

    <!-- Quick Actions -->
    <div class="quick-actions">
      <h3>{{ $t('finance.dashboard.quickActions') }}</h3>
      <div class="actions-grid">
        <FinanceActionCard
          v-for="action in quickActions"
          :key="action.key"
          :title="action.title"
          :description="action.description"
          :icon="action.icon"
          :route="action.route"
          :permission="action.permission"
        />
      </div>
    </div>

    <!-- Recent Transactions -->
    <div class="recent-transactions">
      <div class="section-header">
        <h3>{{ $t('finance.dashboard.recentTransactions') }}</h3>
        <el-button type="text" @click="viewAllTransactions">
          {{ $t('common.viewAll') }}
        </el-button>
      </div>
      <RecentTransactionsList :transactions="recentTransactions" />
    </div>

    <!-- Pending Approvals -->
    <div v-if="pendingApprovals.length > 0" class="pending-approvals">
      <h3>{{ $t('finance.dashboard.pendingApprovals') }}</h3>
      <PendingApprovalsList :approvals="pendingApprovals" />
    </div>
  </div>
</template>

<script>
import { ref, reactive, onMounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { 
  getDashboardOverview, 
  getFinancialSummary,
  getCashFlow,
  getBudgetVsActual,
  getFinancialRatios
} from '@/api/finance/dashboard'
import FinanceKpiCard from '@/components/Finance/FinanceKpiCard.vue'
import RevenueExpenseChart from '@/components/Finance/RevenueExpenseChart.vue'
import CashFlowChart from '@/components/Finance/CashFlowChart.vue'
import ExpenseCategoryChart from '@/components/Finance/ExpenseCategoryChart.vue'
import BudgetVsActualChart from '@/components/Finance/BudgetVsActualChart.vue'
import ProfitMarginChart from '@/components/Finance/ProfitMarginChart.vue'
import FinancialRatiosGrid from '@/components/Finance/FinancialRatiosGrid.vue'
import FinanceActionCard from '@/components/Finance/FinanceActionCard.vue'
import RecentTransactionsList from '@/components/Finance/RecentTransactionsList.vue'
import PendingApprovalsList from '@/components/Finance/PendingApprovalsList.vue'

export default {
  name: 'FinanceDashboard',
  components: {
    FinanceKpiCard,
    RevenueExpenseChart,
    CashFlowChart,
    ExpenseCategoryChart,
    BudgetVsActualChart,
    ProfitMarginChart,
    FinancialRatiosGrid,
    FinanceActionCard,
    RecentTransactionsList,
    PendingApprovalsList
  },
  setup() {
    const { t } = useI18n()
    const router = useRouter()
    
    const loading = ref(false)
    const selectedPeriod = ref('month')
    const dateRange = ref([])
    const revenueExpenseView = ref('monthly')
    
    const periods = [
      { label: t('common.week'), value: 'week' },
      { label: t('common.month'), value: 'month' },
      { label: t('common.quarter'), value: 'quarter' },
      { label: t('common.year'), value: 'year' }
    ]
    
    const chartViews = [
      { label: t('finance.views.monthly'), value: 'monthly' },
      { label: t('finance.views.quarterly'), value: 'quarterly' },
      { label: t('finance.views.yearly'), value: 'yearly' }
    ]
    
    const dashboardData = reactive({
      overview: {},
      financialSummary: {},
      cashFlow: {},
      budgetVsActual: {},
      financialRatios: {},
      recentTransactions: [],
      pendingApprovals: []
    })
    
    const financialKpis = computed(() => [
      {
        key: 'totalRevenue',
        title: t('finance.kpis.totalRevenue'),
        value: dashboardData.overview.totalRevenue || 0,
        change: '+12.5%',
        trend: 'up',
        icon: 'el-icon-money',
        color: 'success',
        format: 'currency'
      },
      {
        key: 'totalExpenses',
        title: t('finance.kpis.totalExpenses'),
        value: dashboardData.overview.totalExpenses || 0,
        change: '+8.3%',
        trend: 'up',
        icon: 'el-icon-shopping-cart-2',
        color: 'warning',
        format: 'currency'
      },
      {
        key: 'netIncome',
        title: t('finance.kpis.netIncome'),
        value: dashboardData.overview.netIncome || 0,
        change: '+15.7%',
        trend: 'up',
        icon: 'el-icon-trophy',
        color: 'primary',
        format: 'currency'
      },
      {
        key: 'cashBalance',
        title: t('finance.kpis.cashBalance'),
        value: dashboardData.overview.cashBalance || 0,
        change: '-2.1%',
        trend: 'down',
        icon: 'el-icon-wallet',
        color: 'info',
        format: 'currency'
      },
      {
        key: 'accountsReceivable',
        title: t('finance.kpis.accountsReceivable'),
        value: dashboardData.overview.accountsReceivable || 0,
        change: '+5.4%',
        trend: 'up',
        icon: 'el-icon-document-add',
        color: 'success',
        format: 'currency'
      },
      {
        key: 'budgetUtilization',
        title: t('finance.kpis.budgetUtilization'),
        value: dashboardData.overview.budgetUtilization || 0,
        change: '85%',
        trend: 'stable',
        icon: 'el-icon-pie-chart',
        color: 'primary',
        format: 'percentage'
      }
    ])
    
    const quickActions = computed(() => [
      {
        key: 'createExpense',
        title: t('finance.actions.createExpense'),
        description: t('finance.actions.createExpenseDesc'),
        icon: 'el-icon-plus',
        route: '/finance/expenses/create',
        permission: 'finance:expense:create'
      },
      {
        key: 'createBudget',
        title: t('finance.actions.createBudget'),
        description: t('finance.actions.createBudgetDesc'),
        icon: 'el-icon-document',
        route: '/finance/budgets/create',
        permission: 'finance:budget:create'
      },
      {
        key: 'journalEntry',
        title: t('finance.actions.journalEntry'),
        description: t('finance.actions.journalEntryDesc'),
        icon: 'el-icon-edit',
        route: '/finance/accounting/journal-entries/create',
        permission: 'finance:journal:create'
      },
      {
        key: 'financialReports',
        title: t('finance.actions.financialReports'),
        description: t('finance.actions.financialReportsDesc'),
        icon: 'el-icon-data-analysis',
        route: '/finance/reports',
        permission: 'finance:reports:view'
      },
      {
        key: 'reconciliation',
        title: t('finance.actions.reconciliation'),
        description: t('finance.actions.reconciliationDesc'),
        icon: 'el-icon-finished',
        route: '/finance/reconciliation',
        permission: 'finance:reconciliation:manage'
      },
      {
        key: 'payrollProcessing',
        title: t('finance.actions.payrollProcessing'),
        description: t('finance.actions.payrollProcessingDesc'),
        icon: 'el-icon-user',
        route: '/finance/payroll',
        permission: 'finance:payroll:process'
      }
    ])
    
    const loadDashboardData = async () => {
      loading.value = true
      try {
        const [overview, summary, cashFlow, budgetVsActual, ratios] = await Promise.all([
          getDashboardOverview(selectedPeriod.value),
          getFinancialSummary(dateRange.value?.[0], dateRange.value?.[1]),
          getCashFlow(selectedPeriod.value),
          getBudgetVsActual(new Date().getFullYear()),
          getFinancialRatios(selectedPeriod.value)
        ])
        
        dashboardData.overview = overview.data
        dashboardData.financialSummary = summary.data
        dashboardData.cashFlow = cashFlow.data
        dashboardData.budgetVsActual = budgetVsActual.data
        dashboardData.financialRatios = ratios.data
        
      } catch (error) {
        ElMessage.error(t('common.loadError'))
        console.error('Finance dashboard load error:', error)
      } finally {
        loading.value = false
      }
    }
    
    const refreshDashboard = () => {
      loadDashboardData()
    }
    
    const changePeriod = (period) => {
      selectedPeriod.value = period
      loadDashboardData()
    }
    
    const handleDateRangeChange = (dates) => {
      loadDashboardData()
    }
    
    const viewAllTransactions = () => {
      router.push('/finance/transactions')
    }
    
    onMounted(() => {
      loadDashboardData()
      // Set up auto-refresh every 10 minutes
      setInterval(loadDashboardData, 600000)
    })
    
    return {
      loading,
      selectedPeriod,
      dateRange,
      revenueExpenseView,
      periods,
      chartViews,
      financialKpis,
      quickActions,
      revenueExpenseData: computed(() => dashboardData.financialSummary.monthlyTrends || []),
      cashFlowData: computed(() => dashboardData.cashFlow || {}),
      expensesByCategory: computed(() => dashboardData.financialSummary.expensesByCategory || []),
      budgetVsActual: computed(() => dashboardData.budgetVsActual || {}),
      profitMarginData: computed(() => dashboardData.financialSummary.profitMarginTrend || []),
      financialRatios: computed(() => dashboardData.financialRatios || {}),
      recentTransactions: computed(() => dashboardData.recentTransactions || []),
      pendingApprovals: computed(() => dashboardData.pendingApprovals || []),
      refreshDashboard,
      changePeriod,
      handleDateRangeChange,
      viewAllTransactions
    }
  }
}
</script>

<style scoped>
.finance-dashboard {
  padding: 20px;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.header-actions {
  display: flex;
  gap: 15px;
  align-items: center;
}

.kpi-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.charts-section {
  margin-bottom: 30px;
}

.chart-container {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  height: 400px;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.chart-container h3 {
  margin-bottom: 20px;
  color: #333;
  font-size: 16px;
  font-weight: 600;
}

.ratios-section {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 30px;
}

.quick-actions {
  margin-bottom: 30px;
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 15px;
  margin-top: 15px;
}

.recent-transactions,
.pending-approvals {
  background: white;
  padding: 20px;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}
</style>
```

## 3. Database Schema

```sql
-- Chart of Accounts
CREATE TABLE finance_accounts (
    account_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    account_code VARCHAR(20) NOT NULL UNIQUE,
    account_name VARCHAR(200) NOT NULL,
    account_type VARCHAR(50) NOT NULL, -- Asset, Liability, Equity, Revenue, Expense
    account_sub_type VARCHAR(50),
    parent_account_id BIGINT,
    balance DECIMAL(15,2) DEFAULT 0,
    debit_balance DECIMAL(15,2) DEFAULT 0,
    credit_balance DECIMAL(15,2) DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    is_system BOOLEAN DEFAULT FALSE,
    description VARCHAR(500),
    currency_code VARCHAR(10) DEFAULT 'USD',
    level INTEGER DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by BIGINT,
    updated_by BIGINT,
    FOREIGN KEY (parent_account_id) REFERENCES finance_accounts(account_id)
);

-- Journal Entries
CREATE TABLE finance_journal_entries (
    journal_entry_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    entry_number VARCHAR(50) NOT NULL UNIQUE,
    entry_date DATE NOT NULL,
    description VARCHAR(500),
    reference VARCHAR(50),
    total_debit DECIMAL(15,2) NOT NULL,
    total_credit DECIMAL(15,2) NOT NULL,
    status VARCHAR(20) DEFAULT 'Draft', -- Draft, Posted, Reversed
    posted_date TIMESTAMP,
    posted_by BIGINT,
    source VARCHAR(50) DEFAULT 'Manual', -- Manual, System, Import
    source_id BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by BIGINT,
    updated_by BIGINT
);

-- Journal Entry Lines
CREATE TABLE finance_journal_entry_lines (
    line_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    journal_entry_id BIGINT NOT NULL,
    account_id BIGINT NOT NULL,
    description VARCHAR(500),
    debit_amount DECIMAL(15,2) DEFAULT 0,
    credit_amount DECIMAL(15,2) DEFAULT 0,
    line_number INTEGER,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (journal_entry_id) REFERENCES finance_journal_entries(journal_entry_id),
    FOREIGN KEY (account_id) REFERENCES finance_accounts(account_id)
);

-- Budgets
CREATE TABLE finance_budgets (
    budget_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    budget_name VARCHAR(200) NOT NULL,
    description VARCHAR(500),
    budget_year INTEGER NOT NULL,
    budget_type VARCHAR(20) DEFAULT 'Annual', -- Annual, Quarterly, Monthly, Project
    status VARCHAR(20) DEFAULT 'Draft', -- Draft, Submitted, Approved, Active, Closed
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_budget_amount DECIMAL(15,2) DEFAULT 0,
    actual_amount DECIMAL(15,2) DEFAULT 0,
    variance_amount DECIMAL(15,2) DEFAULT 0,
    variance_percentage DECIMAL(5,2) DEFAULT 0,
    approved_by BIGINT,
    approved_date TIMESTAMP,
    department_id BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by BIGINT,
    updated_by BIGINT
);

-- Budget Lines
CREATE TABLE finance_budget_lines (
    budget_line_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    budget_id BIGINT NOT NULL,
    account_id BIGINT NOT NULL,
    line_description VARCHAR(500),
    budget_amount DECIMAL(15,2) NOT NULL,
    actual_amount DECIMAL(15,2) DEFAULT 0,
    variance_amount DECIMAL(15,2) DEFAULT 0,
    variance_percentage DECIMAL(5,2) DEFAULT 0,
    period_type VARCHAR(20) DEFAULT 'Monthly', -- Monthly, Quarterly, Annual
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (budget_id) REFERENCES finance_budgets(budget_id),
    FOREIGN KEY (account_id) REFERENCES finance_accounts(account_id)
);

-- Expenses
CREATE TABLE finance_expenses (
    expense_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    expense_number VARCHAR(50) NOT NULL UNIQUE,
    expense_date DATE NOT NULL,
    description VARCHAR(500),
    amount DECIMAL(15,2) NOT NULL,
    currency_code VARCHAR(10) DEFAULT 'USD',
    category_id BIGINT,
    account_id BIGINT,
    vendor_id BIGINT,
    project_id BIGINT,
    department_id BIGINT,
    status VARCHAR(20) DEFAULT 'Draft', -- Draft, Submitted, Approved, Paid, Rejected
    submitted_date TIMESTAMP,
    approved_date TIMESTAMP,
    approved_by BIGINT,
    paid_date TIMESTAMP,
    payment_method VARCHAR(50),
    receipt_url VARCHAR(500),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by BIGINT,
    updated_by BIGINT,
    FOREIGN KEY (account_id) REFERENCES finance_accounts(account_id)
);

-- Expense Categories
CREATE TABLE finance_expense_categories (
    category_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    category_name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    parent_category_id BIGINT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (parent_category_id) REFERENCES finance_expense_categories(category_id)
);

-- Financial Periods
CREATE TABLE finance_periods (
    period_id BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    period_name VARCHAR(50) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    period_type VARCHAR(20) NOT NULL, -- Monthly, Quarterly, Annual
    fiscal_year INTEGER NOT NULL,
    is_closed BOOLEAN DEFAULT FALSE,
    closed_date TIMESTAMP,
    closed_by BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for performance
CREATE INDEX idx_finance_accounts_type ON finance_accounts(account_type, is_active);
CREATE INDEX idx_finance_journal_entries_date ON finance_journal_entries(entry_date, status);
CREATE INDEX idx_finance_journal_entry_lines_account ON finance_journal_entry_lines(account_id);
CREATE INDEX idx_finance_budgets_year_status ON finance_budgets(budget_year, status);
CREATE INDEX idx_finance_expenses_date_status ON finance_expenses(expense_date, status);
CREATE INDEX idx_finance_expenses_category ON finance_expenses(category_id);
```

## 4. Implementation Timeline

### Phase 1 (Weeks 1-2): Foundation
- Database schema setup
- Core accounting models and services
- Basic CRUD operations for accounts and journal entries

### Phase 2 (Weeks 3-4): Budget Management
- Budget creation and management
- Budget approval workflow
- Budget vs actual reporting

### Phase 3 (Weeks 5-6): Expense Management
- Expense tracking and approval
- Expense categories and reporting
- Integration with accounting

### Phase 4 (Weeks 7-8): Financial Dashboard
- Dashboard components and charts
- Financial KPIs and metrics
- Real-time data updates

### Phase 5 (Weeks 9-10): Advanced Features
- Financial reporting
- Cash flow management
- Financial analytics

### Phase 6 (Weeks 11-12): Testing & Optimization
- Unit and integration testing
- Performance optimization
- User acceptance testing

## 5. Key Features Summary

1. **Comprehensive Accounting**
   - Chart of accounts management
   - Journal entries and general ledger
   - Trial balance and financial statements

2. **Budget Management**
   - Budget creation and approval
   - Budget vs actual analysis
   - Variance reporting

3. **Expense Management**
   - Expense tracking and approval
   - Category-based organization
   - Receipt management

4. **Financial Dashboard**
   - Real-time financial KPIs
   - Interactive charts and analytics
   - Cash flow visualization

5. **Financial Reporting**
   - Profit & loss statements
   - Balance sheets
   - Cash flow statements
   - Custom financial reports

6. **Integration Capabilities**
   - HR payroll integration
   - Project cost tracking
   - Vendor management

This Finance module provides comprehensive financial management capabilities with modern dashboard interfaces and robust reporting features.