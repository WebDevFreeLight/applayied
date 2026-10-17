# Finance Module Reports - Comprehensive Technical Documentation

## Table of Contents
1. [Overview](#overview)
2. [Report Types Summary](#report-types-summary)
3. [GL Reports (General Ledger)](#gl-reports-general-ledger)
   - [Trial Balance](#1-trial-balance-report)
   - [Balance Sheet](#2-balance-sheet-report)
   - [Income Statement](#3-income-statement-report)
   - [Cash Flow Statement (IAS 7)](#4-cash-flow-statement-ias-7)
   - [Cost Center P&L](#5-cost-center-pl-report)
   - [Project P&L](#6-project-pl-report)
4. [ARAP Reports (Receivables/Payables)](#arap-reports-receivablespayables)
   - [Customer Aging](#7-customer-aging-report)
   - [Vendor Aging](#8-vendor-aging-report)
5. [API Endpoints Reference](#api-endpoints-reference)
6. [Frontend Pages Reference](#frontend-pages-reference)
7. [Data Flow Diagrams](#data-flow-diagrams)

---

## Overview

The Finance Module provides comprehensive financial reporting capabilities that comply with international accounting standards. All reports support:
- **Multi-tenant isolation** via `CompanyID`
- **Bilingual support** (Arabic/English)
- **RTL/LTR layouts** based on locale
- **Currency formatting** (default: SAR)
- **Date range filtering**
- **Export capabilities** (PDF/Excel placeholders)

### Architecture Pattern
```
Frontend (Next.js) → API Service → Backend Controller → Service Layer → Repository → Database
```

---

## Report Types Summary

| Report | Type | Backend | Frontend | Status |
|--------|------|---------|----------|--------|
| Trial Balance | GL | ✅ Implemented | ✅ Implemented | Complete |
| Balance Sheet | GL | ✅ Implemented | ✅ Implemented | Complete |
| Income Statement | GL | ✅ Implemented | ✅ Implemented | Complete |
| Cash Flow (IAS 7) | GL | ✅ Implemented | ✅ Implemented | Complete |
| Cost Center P&L | GL | ✅ Implemented | ✅ Implemented | Complete |
| Project P&L | GL | ✅ Implemented | ✅ Implemented | Complete |
| Customer Aging | ARAP | ✅ Implemented | ✅ Implemented | Complete |
| Vendor Aging | ARAP | ✅ Implemented | ✅ Implemented | Complete |
| General Ledger | GL | ❌ Not Implemented | ❌ Not Implemented | Pending |
| Account Statement | GL | ❌ Not Implemented | ❌ Not Implemented | Pending |

---

## GL Reports (General Ledger)

### 1. Trial Balance Report

#### Description
The Trial Balance report lists all general ledger accounts with their debit and credit balances for a specified period. It verifies that total debits equal total credits (accounting equation balance).

#### Calculation Logic (Backend: `GLFinancialReportService.cs:343-415`)

```csharp
public TrialBalanceReportDto GetTrialBalanceData(DateTime periodStart, DateTime periodEnd, long? companyId = null)
{
    // 1. Get all active accounts
    var accounts = _accountRepository.ListAll(null);
    var lines = new List<TrialBalanceLineDto>();

    // 2. For each active account, get balance
    foreach (var account in accounts.Where(a => a.IsActive))
    {
        // Get account balance for the period
        var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);

        // Only include accounts with activity
        if (balance.TotalDebit > 0 || balance.TotalCredit > 0)
        {
            lines.Add(new TrialBalanceLineDto
            {
                AccountId = account.AccountId,
                AccountCode = account.AccountNumber,
                AccountNameAr = account.AccountNameAr,
                AccountNameEn = account.AccountNameEn,
                AccountType = account.AccountType.ToString(),
                Debit = balance.TotalDebit,
                Credit = balance.TotalCredit,
                Balance = balance.TotalDebit - balance.TotalCredit  // Net balance
            });
        }
    }

    // 3. Return report with totals
    return new TrialBalanceReportDto
    {
        PeriodStartDate = periodStart,
        PeriodEndDate = periodEnd,
        FiscalYear = periodStart.Year,
        Currency = "SAR",
        TotalDebit = lines.Sum(l => l.Debit),
        TotalCredit = lines.Sum(l => l.Credit),
        Lines = lines.OrderBy(l => l.AccountCode).ToList()
    };
}
```

#### Key Formulas
| Field | Formula |
|-------|---------|
| Account Balance | `TotalDebit - TotalCredit` |
| Total Debit | `SUM(all account debits)` |
| Total Credit | `SUM(all account credits)` |
| IsBalanced | `TotalDebit == TotalCredit` |

#### Data Displayed (Frontend)
- **Summary Cards**: Total Debit, Total Credit, Balance Status (Balanced/Unbalanced)
- **Table Columns**: Account Code, Account Name, Account Type, Debit, Credit, Balance
- **Filters**: Period Start Date, Period End Date
- **Features**: Date presets, Export button

#### DTO Structure
```typescript
interface TrialBalanceReportDto {
  periodStartDate: Date;
  periodEndDate: Date;
  fiscalYear: number;
  currency: string;
  totalDebit: number;
  totalCredit: number;
  isBalanced: boolean;  // Computed: totalDebit === totalCredit
  lines: TrialBalanceLineDto[];
}

interface TrialBalanceLineDto {
  accountId: string;
  accountCode: string;
  accountNameAr: string;
  accountNameEn: string;
  accountType: string;
  debit: number;
  credit: number;
  balance: number;
}
```

---

### 2. Balance Sheet Report

#### Description
The Balance Sheet shows the financial position of the company at a specific point in time, displaying Assets, Liabilities, and Equity. It follows the accounting equation: **Assets = Liabilities + Equity**.

#### Calculation Logic (Backend: `GLFinancialReportService.cs:419-502`)

```csharp
public BalanceSheetReportDto GetBalanceSheetData(DateTime asOfDate, long? companyId = null)
{
    // 1. Get fiscal year configuration
    var fiscalYear = _fiscalYearService.GetFiscalYearByDate(companyId, asOfDate);

    // 2. Determine balance start date
    // For permanent accounts (Assets, Liabilities, Equity),
    // we need CUMULATIVE balances from earliest fiscal year
    DateTime balanceStartDate;
    if (fiscalYear != null)
    {
        var earliestFiscalYear = _fiscalYearService.GetEarliestFiscalYear(companyId);
        balanceStartDate = earliestFiscalYear?.YearStartDate ?? fiscalYear.YearStartDate;
    }
    else
    {
        balanceStartDate = new DateTime(asOfDate.Year, 1, 1);  // Fallback
    }

    // 3. Get all accounts and classify by type
    var accounts = _accountRepository.ListAll(null);
    var assets = new List<BalanceSheetSectionDto>();
    var liabilities = new List<BalanceSheetSectionDto>();
    var equity = new List<BalanceSheetSectionDto>();

    foreach (var account in accounts.Where(a => a.IsActive))
    {
        // Get cumulative balance from earliest fiscal year to asOfDate
        var balance = _entryRepository.GetAccountBalance(account.AccountId, balanceStartDate, asOfDate);
        var netBalance = balance.TotalDebit - balance.TotalCredit;

        if (netBalance == 0) continue;

        var section = new BalanceSheetSectionDto
        {
            AccountId = account.AccountId,
            AccountCode = account.AccountNumber,
            AccountNameAr = account.AccountNameAr,
            AccountNameEn = account.AccountNameEn,
            Level = account.Level,
            Balance = Math.Abs(netBalance)  // Always positive
        };

        // 4. Classify based on account type
        switch (account.AccountType)
        {
            case AccountType.Asset:
            case AccountType.ContraAsset:
                assets.Add(section);
                break;
            case AccountType.Liability:
            case AccountType.ContraLiability:
                liabilities.Add(section);
                break;
            case AccountType.Equity:
            case AccountType.ContraEquity:
                equity.Add(section);
                break;
        }
    }

    return new BalanceSheetReportDto
    {
        AsOfDate = asOfDate,
        FiscalYear = fiscalYearNumber,
        Currency = "SAR",
        TotalAssets = assets.Sum(a => a.Balance),
        TotalLiabilities = liabilities.Sum(l => l.Balance),
        TotalEquity = equity.Sum(e => e.Balance),
        Assets = assets.OrderBy(a => a.AccountCode).ToList(),
        Liabilities = liabilities.OrderBy(l => l.AccountCode).ToList(),
        Equity = equity.OrderBy(e => e.AccountCode).ToList()
    };
}
```

#### Key Formulas
| Field | Formula |
|-------|---------|
| Account Balance | `ABS(TotalDebit - TotalCredit)` cumulative from earliest fiscal year |
| Total Assets | `SUM(all asset account balances)` |
| Total Liabilities | `SUM(all liability account balances)` |
| Total Equity | `SUM(all equity account balances)` |
| IsBalanced | `TotalAssets == (TotalLiabilities + TotalEquity)` |

#### Important Notes
- **Cumulative Balances**: Unlike Trial Balance (period-based), Balance Sheet uses cumulative balances from the earliest fiscal year
- **Contra Accounts**: Included in their parent category (e.g., ContraAsset with Assets)
- **Fiscal Year Awareness**: Respects fiscal year configuration for proper period determination

#### Data Displayed (Frontend)
- **Summary Cards**: Total Assets, Total Liabilities, Total Equity, Balance Status
- **Three Section Tables**: Assets, Liabilities, Equity
- **Balance Equation Display**: Assets = Liabilities + Equity
- **Filters**: As of Date (single date)
- **Date Presets**: Today, End of Last Month, End of Quarter, End of Last Year

#### DTO Structure
```typescript
interface BalanceSheetReportDto {
  asOfDate: Date;
  fiscalYear: number;
  currency: string;
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
  isBalanced: boolean;  // Computed: totalAssets === (totalLiabilities + totalEquity)
  assets: BalanceSheetSectionDto[];
  liabilities: BalanceSheetSectionDto[];
  equity: BalanceSheetSectionDto[];
}

interface BalanceSheetSectionDto {
  accountId: string;
  accountCode: string;
  accountNameAr: string;
  accountNameEn: string;
  level: number;
  balance: number;
  children?: BalanceSheetSectionDto[];  // For hierarchical display
}
```

---

### 3. Income Statement Report

#### Description
The Income Statement (Profit & Loss) shows revenues and expenses for a period, calculating the net income or loss. It includes only temporary accounts (Revenue and Expense).

#### Calculation Logic (Backend: `GLFinancialReportService.cs:589-638`)

```csharp
public IncomeStatementReportDto GetIncomeStatementData(DateTime periodStart, DateTime periodEnd, long? companyId = null)
{
    var accounts = _accountRepository.ListAll(null);
    var revenue = new List<IncomeStatementLineDto>();
    var expenses = new List<IncomeStatementLineDto>();

    foreach (var account in accounts.Where(a => a.IsActive))
    {
        // Get period balance
        var balance = _entryRepository.GetAccountBalance(account.AccountId, periodStart, periodEnd);

        // For Revenue: Credit - Debit (credits increase revenue)
        // For Expense: Debit - Credit (debits increase expense)
        var netBalance = balance.TotalCredit - balance.TotalDebit;

        if (netBalance == 0) continue;

        var line = new IncomeStatementLineDto
        {
            AccountId = account.AccountId,
            AccountCode = account.AccountNumber,
            AccountNameAr = account.AccountNameAr,
            AccountNameEn = account.AccountNameEn,
            Amount = Math.Abs(netBalance)
        };

        switch (account.AccountType)
        {
            case AccountType.Revenue:
            case AccountType.ContraRevenue:
                revenue.Add(line);
                break;
            case AccountType.Expense:
                expenses.Add(line);
                break;
        }
    }

    return new IncomeStatementReportDto
    {
        PeriodStartDate = periodStart,
        PeriodEndDate = periodEnd,
        FiscalYear = periodStart.Year,
        Currency = "SAR",
        TotalRevenue = revenue.Sum(r => r.Amount),
        TotalExpenses = expenses.Sum(e => e.Amount),
        Revenue = revenue.OrderBy(r => r.AccountCode).ToList(),
        Expenses = expenses.OrderBy(e => e.AccountCode).ToList()
    };
}
```

#### Key Formulas
| Field | Formula |
|-------|---------|
| Revenue Amount | `TotalCredit - TotalDebit` (positive = revenue) |
| Expense Amount | `TotalDebit - TotalCredit` (positive = expense) |
| Total Revenue | `SUM(all revenue account amounts)` |
| Total Expenses | `SUM(all expense account amounts)` |
| Net Income | `TotalRevenue - TotalExpenses` |
| Profit Margin % | `(NetIncome / TotalRevenue) * 100` |

#### Data Displayed (Frontend)
- **Summary Cards**: Total Revenue, Total Expenses, Net Profit/Loss, Profit Margin %
- **Two Section Tables**: Revenue accounts, Expense accounts
- **Color Coding**: Green for profit, Red for loss
- **Filters**: Period Start, Period End
- **Date Presets**: This Month, Last Month, This Quarter, This Year

#### DTO Structure
```typescript
interface IncomeStatementReportDto {
  periodStartDate: Date;
  periodEndDate: Date;
  fiscalYear: number;
  currency: string;
  totalRevenue: number;
  totalExpenses: number;
  netIncome: number;  // Computed: totalRevenue - totalExpenses
  revenue: IncomeStatementLineDto[];
  expenses: IncomeStatementLineDto[];
}
```

---

### 4. Cash Flow Statement (IAS 7)

#### Description
The Cash Flow Statement complies with **IAS 7 (International Accounting Standard 7)** using the **Indirect Method**. It classifies cash flows into three categories:
1. **Operating Activities** - Day-to-day business operations
2. **Investing Activities** - Long-term asset transactions
3. **Financing Activities** - Equity and debt transactions

#### Calculation Logic (Backend: `GLFinancialReportService.cs:829-922`)

```csharp
public CashFlowStatementDto GetCashFlowDataIAS7(DateTime periodStart, DateTime periodEnd, long? companyId = null)
{
    var accounts = _accountRepository.ListAll(null);

    // Calculate dates
    var openingDate = periodStart.AddDays(-1);
    var fiscalYearStart = new DateTime(periodStart.Year, 1, 1);

    // STEP 1: Get Net Income from Income Statement
    var incomeData = GetIncomeStatementData(periodStart, periodEnd, companyId);
    decimal netIncome = incomeData.TotalRevenue - incomeData.TotalExpenses;

    // STEP 2: Calculate Opening and Closing Cash Balances
    var cashAccounts = accounts.Where(a => a.IsActive && (a.IsCashAccount || a.IsBankAccount)).ToList();
    decimal openingCashBalance = CalculateCashBalance(cashAccounts, fiscalYearStart, openingDate);
    decimal closingCashBalance = CalculateCashBalance(cashAccounts, fiscalYearStart, periodEnd);

    // STEP 3: Calculate Operating Activities (Indirect Method)
    var operatingActivities = CalculateOperatingActivities(accounts, periodStart, periodEnd, netIncome, companyId);

    // STEP 4: Calculate Investing Activities
    var investingActivities = CalculateInvestingActivities(accounts, periodStart, periodEnd, companyId);

    // STEP 5: Calculate Financing Activities
    var financingActivities = CalculateFinancingActivities(accounts, periodStart, periodEnd, companyId);

    return new CashFlowStatementDto
    {
        PeriodStartDate = periodStart,
        PeriodEndDate = periodEnd,
        OpeningCashBalance = openingCashBalance,
        ActualCashBalance = closingCashBalance,
        OperatingActivities = operatingActivities,
        InvestingActivities = investingActivities,
        FinancingActivities = financingActivities
    };
}
```

#### Operating Activities Calculation (Indirect Method)
```csharp
private CashFlowOperatingActivitiesDto CalculateOperatingActivities(...)
{
    var operatingActivities = new CashFlowOperatingActivitiesDto { NetIncome = netIncome };

    // NON-CASH ADJUSTMENTS (add back to net income)

    // 1. Depreciation - identified by ContraAsset accounts with "depreciation" in name
    var depreciationAccounts = accounts.Where(a =>
        a.AccountType == AccountType.ContraAsset &&
        (a.AccountNameEn.Contains("depreciation") || a.AccountNameAr.Contains("إهلاك")));
    // Add period movement (Credit - Debit) back to net income

    // 2. Amortization - similar to depreciation

    // 3. Provisions - liability accounts with "provision" or "مخصص" in name
    // Track change in provision balance

    // WORKING CAPITAL CHANGES

    // 4. Trade Receivables (Asset)
    // Increase = cash outflow (negative), Decrease = cash inflow (positive)
    var receivablesChange = CalculateWorkingCapitalChange(receivableAccounts, ..., isAsset: true);
    operatingActivities.ChangeInTradeReceivables = -receivablesChange;

    // 5. Inventory (Asset) - same logic as receivables

    // 6. Prepaid Expenses (Asset) - same logic

    // 7. Trade Payables (Liability)
    // Increase = cash inflow (positive), Decrease = cash outflow (negative)
    var payablesChange = CalculateWorkingCapitalChange(payableAccounts, ..., isAsset: false);
    operatingActivities.ChangeInTradePayables = payablesChange;

    // 8. Accrued Expenses (Liability) - same logic as payables

    return operatingActivities;
}
```

#### Key Formulas
| Section | Formula |
|---------|---------|
| **Operating Activities** | |
| Cash Generated from Operations | `NetIncome + TotalNonCashAdjustments + TotalWorkingCapitalChanges` |
| Total Non-Cash Adjustments | `Depreciation + Amortization + Provisions + LossOnDisposal - GainOnDisposal` |
| Working Capital Changes | `ΔReceivables + ΔInventory + ΔPrepaid + ΔPayables + ΔAccrued` |
| Net Cash from Operating | `CashGeneratedFromOperations - InterestPaid - TaxesPaid` |
| **Investing Activities** | |
| Net Cash from Investing | `SaleOfPPE + SaleOfIntangibles + SaleOfInvestments - PurchaseOfPPE - PurchaseOfIntangibles - PurchaseOfInvestments` |
| **Financing Activities** | |
| Net Cash from Financing | `ProceedsFromShares + ProceedsFromBorrowings - RepurchaseOfShares - RepaymentOfBorrowings - DividendsPaid` |
| **Summary** | |
| Net Change in Cash | `NetCashFromOperating + NetCashFromInvesting + NetCashFromFinancing` |
| Closing Cash Balance | `OpeningCashBalance + NetChangeInCash` |
| IsReconciled | `ABS(ClosingCashBalance - ActualCashBalance) < 0.01` |

#### Account Classification (How accounts are identified)
| Category | Account Identification Method |
|----------|------------------------------|
| Cash/Bank | `IsCashAccount == true` OR `IsBankAccount == true` |
| Depreciation | `AccountType == ContraAsset` AND name contains "depreciation" or "إهلاك" |
| Amortization | `AccountType == ContraAsset` AND name contains "amortization" or "إطفاء" |
| Provisions | `AccountType == Liability` AND name contains "provision" or "مخصص" |
| Receivables | `AccountType == Asset` AND `IsControlAccount` AND name contains "receivable" or "مدين" |
| Inventory | `AccountType == Asset` AND name contains "inventory" or "مخزون" |
| Payables | `AccountType == Liability` AND `IsControlAccount` AND name contains "payable" or "دائن" |
| Fixed Assets | `AccountType == Asset` AND name contains "property", "plant", "equipment", "fixed asset" |
| Investments | `AccountType == Asset` AND name contains "investment" or "استثمار" |
| Capital | `AccountType == Equity` AND name contains "capital" or "share" or "رأس مال" |
| Loans | `AccountType == Liability` AND name contains "loan", "borrowing", "debt" |

#### Data Displayed (Frontend)
- **Summary Cards**: Opening Balance, Operating Cash, Investing Cash, Financing Cash, Net Change, Closing Balance
- **Operating Section**: Net Income, Non-Cash Adjustments (collapsible), Working Capital Changes (collapsible)
- **Investing Section**: PPE transactions, Intangible transactions, Investment transactions
- **Financing Section**: Share transactions, Borrowing transactions, Dividends
- **Reconciliation Status**: Shows if calculated closing matches actual cash accounts

---

### 5. Cost Center P&L Report

#### Description
Shows Profit & Loss broken down by Cost Center, allowing analysis of profitability per department, branch, or business unit.

#### Calculation Logic (Backend: `GLFinancialReportService.cs:1523-1639`)

```csharp
public CostCenterPLReportDto GetCostCenterPLReport(CostCenterPLQueryDto query)
{
    // 1. Get Revenue and Expense accounts only
    var accounts = _accountRepository.ListAll(null)
        .Where(a => a.IsActive && (a.AccountType == AccountType.Revenue || a.AccountType == AccountType.Expense));

    // 2. Get journal entries with Cost Center dimension
    var entries = _entryRepository.GetEntriesForDimensionPL(
        query.PeriodStartDate,
        query.PeriodEndDate,
        accountIds,
        query.CompanyId,
        query.CostCenterId,  // Filter by specific cost center if provided
        null);  // No project filter

    // 3. Group by Cost Center
    var groupedByCostCenter = entries.GroupBy(e => e.CostCenterId);

    foreach (var costCenterGroup in groupedByCostCenter)
    {
        var item = new CostCenterPLItemDto
        {
            CostCenterId = costCenterGroup.Key,
            RevenueAccounts = new List<PLAccountLineDto>(),
            ExpenseAccounts = new List<PLAccountLineDto>()
        };

        // 4. Group by account within each cost center
        var accountGroups = costCenterGroup.GroupBy(e => e.AccountId);

        foreach (var accountGroup in accountGroups)
        {
            var account = accountDict[accountGroup.Key];
            var totalDebit = accountGroup.Sum(e => e.BaseDebit);
            var totalCredit = accountGroup.Sum(e => e.BaseCredit);

            var accountLine = new PLAccountLineDto
            {
                AccountId = account.AccountId,
                AccountCode = account.AccountNumber,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                TransactionCount = accountGroup.Count()
            };

            // 5. Calculate amount based on account type
            if (account.AccountType == AccountType.Revenue)
            {
                // Revenue: Credit - Debit (positive = revenue)
                accountLine.Amount = totalCredit - totalDebit;
                item.RevenueAccounts.Add(accountLine);
            }
            else if (account.AccountType == AccountType.Expense)
            {
                // Expense: Debit - Credit (positive = expense)
                accountLine.Amount = totalDebit - totalCredit;
                item.ExpenseAccounts.Add(accountLine);
            }
        }

        report.Items.Add(item);
    }

    return report;
}
```

#### Key Formulas
| Field | Formula |
|-------|---------|
| Revenue Amount | `TotalCredit - TotalDebit` per account per cost center |
| Expense Amount | `TotalDebit - TotalCredit` per account per cost center |
| Total Revenue (Cost Center) | `SUM(revenue accounts)` |
| Total Expenses (Cost Center) | `SUM(expense accounts)` |
| Net Profit/Loss | `TotalRevenue - TotalExpenses` |
| Profit Margin % | `(NetProfitLoss / TotalRevenue) * 100` (if TotalRevenue > 0) |
| IsProfitable | `NetProfitLoss > 0` |

#### Data Displayed (Frontend)
- **View Modes**: Summary (totals per cost center) / Detailed (with account breakdown)
- **Summary View**: Cost Center, Total Revenue, Total Expenses, Net P/L, Margin %
- **Detailed View**: Revenue accounts list, Expense accounts list per cost center
- **Filters**: Period, Cost Center filter dropdown
- **Color Coding**: Profitable (green), Loss-making (red)

---

### 6. Project P&L Report

#### Description
Shows Profit & Loss broken down by Project, allowing analysis of profitability per project or contract.

#### Calculation Logic
Identical to Cost Center P&L but groups by `ProjectId` instead of `CostCenterId`.

#### Key Differences from Cost Center P&L
| Feature | Cost Center P&L | Project P&L |
|---------|-----------------|-------------|
| Grouping Field | `CostCenterId` | `ProjectId` |
| Additional Fields | - | Project Status, Start/End Date |
| Summary Stats | - | Profitable Projects Count, Loss Projects Count |

---

## ARAP Reports (Receivables/Payables)

### 7. Customer Aging Report

#### Description
Shows outstanding customer invoices (Accounts Receivable) categorized by age buckets: 0-30 days, 31-60 days, 61-90 days, and 90+ days.

#### Calculation Logic (Backend: `AgingService.cs:39-62`)

```csharp
public List<AgingReportDto> GetCustomerAgingReport(AgingQueryDto query)
{
    var asOfDate = query.AsOfDate ?? DateTime.Now;

    // 1. Get AR Invoices (filtered by customer if specified)
    var invoices = query.CustomerId.HasValue
        ? _arInvoiceRepository.GetByCustomerId(query.CustomerId.Value)
        : _arInvoiceRepository.GetList(searchModel, pager).Result;

    // 2. Filter to only unpaid invoices and calculate aging
    var report = invoices
        .Where(x => x.RemainingAmount > 0)  // Only unpaid
        .Select(invoice => CalculateAging(invoice, asOfDate, isCustomer: true))
        .ToList();

    return report;
}

private AgingReportDto CalculateAging(ARInvoice invoice, DateTime asOfDate, bool isCustomer)
{
    // Calculate days overdue from due date
    var daysOverdue = (asOfDate - invoice.DueDate).Days;
    var remainingAmount = invoice.RemainingAmount;

    var dto = new AgingReportDto
    {
        InvoiceNumber = invoice.InvoiceNumber,
        InvoiceDate = invoice.InvoiceDate,
        DueDate = invoice.DueDate,
        TotalAmount = invoice.TotalAmount,
        RemainingAmount = remainingAmount,
        DaysOverdue = Math.Max(0, daysOverdue)  // Never negative
    };

    // Assign to aging bucket
    if (daysOverdue <= 30)
        dto.Age0To30 = remainingAmount;
    else if (daysOverdue <= 60)
        dto.Age31To60 = remainingAmount;
    else if (daysOverdue <= 90)
        dto.Age61To90 = remainingAmount;
    else
        dto.Age90Plus = remainingAmount;

    return dto;
}
```

#### Aging Summary Calculation
```csharp
public List<AgingSummaryDto> GetCustomerAgingSummary(AgingQueryDto query)
{
    var report = GetCustomerAgingReport(query);

    // Group by customer and aggregate aging buckets
    var summary = report
        .GroupBy(x => x.CustomerVendorId)
        .Select(g => new AgingSummaryDto
        {
            CustomerVendorId = g.Key,
            CustomerVendorName = g.First().CustomerVendorName,
            TotalOutstanding = g.Sum(x => x.RemainingAmount),
            Age0To30 = g.Sum(x => x.Age0To30),
            Age31To60 = g.Sum(x => x.Age31To60),
            Age61To90 = g.Sum(x => x.Age61To90),
            Age90Plus = g.Sum(x => x.Age90Plus),
            Invoices = g.ToList()  // Include invoice details
        })
        .ToList();

    return summary;
}
```

#### Key Formulas
| Field | Formula |
|-------|---------|
| Days Overdue | `MAX(0, AsOfDate - DueDate)` |
| Aging Bucket Assignment | Based on DaysOverdue: 0-30, 31-60, 61-90, 90+ |
| Total Outstanding | `SUM(RemainingAmount)` per customer |
| Bucket Total | `SUM(RemainingAmount)` where DaysOverdue in bucket range |

#### Data Displayed (Frontend)
- **Summary Cards**: Total Outstanding, 0-30 Days, 31-60 Days, 61-90 Days, 90+ Days
- **Summary Table**: Customer Name, Total Outstanding, Age buckets
- **Detail View**: Invoice list per customer with Invoice #, Date, Due Date, Amount, Days Overdue
- **Filters**: As of Date, Customer dropdown
- **Color Coding**: Higher aging = darker red highlighting

---

### 8. Vendor Aging Report

#### Description
Shows outstanding vendor invoices (Accounts Payable) categorized by age buckets. Helps manage payment schedules and identify overdue obligations.

#### Calculation Logic
Identical to Customer Aging but uses `APInvoice` and `VendorId` instead of `ARInvoice` and `CustomerId`.

---

## API Endpoints Reference

### GL Financial Reports Controller
Base URL: `/api/erp/finance/reports`

| Endpoint | Method | Description | Parameters |
|----------|--------|-------------|------------|
| `/trial-balance-data` | GET | Get Trial Balance data | `periodStart`, `periodEnd`, `companyId?` |
| `/balance-sheet-data` | GET | Get Balance Sheet data | `asOfDate`, `companyId?` |
| `/income-statement-data` | GET | Get Income Statement data | `periodStart`, `periodEnd`, `companyId?` |
| `/cash-flow-ias7-data` | GET | Get IAS 7 Cash Flow data | `periodStart`, `periodEnd`, `companyId?` |
| `/cost-center-pl` | GET | Get Cost Center P&L | `periodStart`, `periodEnd`, `companyId?`, `costCenterId?` |
| `/cost-center-pl-summary` | GET | Get Cost Center P&L Summary | `periodStart`, `periodEnd`, `companyId?`, `topN?` |
| `/project-pl` | GET | Get Project P&L | `periodStart`, `periodEnd`, `companyId?`, `projectId?` |
| `/project-pl-summary` | GET | Get Project P&L Summary | `periodStart`, `periodEnd`, `companyId?`, `topN?` |
| `/trial-balance` | POST | Generate & save Trial Balance | `GenerateReportDto` |
| `/balance-sheet` | POST | Generate & save Balance Sheet | `GenerateReportDto` |
| `/income-statement` | POST | Generate & save Income Statement | `GenerateReportDto` |
| `/cash-flow-ias7` | POST | Generate & save IAS 7 Cash Flow | `GenerateReportDto` |

### ARAP Aging Controller
Base URL: `/api/erp/finance/arap/aging`

| Endpoint | Method | Description | Parameters |
|----------|--------|-------------|------------|
| `/customer-report` | GET | Get Customer Aging Report | `asOfDate?`, `customerId?`, `companyId?` |
| `/customer-summary` | GET | Get Customer Aging Summary | `asOfDate?`, `customerId?`, `companyId?` |
| `/vendor-report` | GET | Get Vendor Aging Report | `asOfDate?`, `vendorId?`, `companyId?` |
| `/vendor-summary` | GET | Get Vendor Aging Summary | `asOfDate?`, `vendorId?`, `companyId?` |

---

## Frontend Pages Reference

| Report | Route | File Location |
|--------|-------|---------------|
| Trial Balance | `/finance/reports/gl/trial-balance` | `app/[locale]/(hydrogen)/finance/reports/gl/trial-balance/page.tsx` |
| Balance Sheet | `/finance/reports/gl/balance-sheet` | `app/[locale]/(hydrogen)/finance/reports/gl/balance-sheet/page.tsx` |
| Income Statement | `/finance/reports/gl/income-statement` | `app/[locale]/(hydrogen)/finance/reports/gl/income-statement/page.tsx` |
| Cash Flow (IAS 7) | `/finance/reports/gl/cash-flow` | `app/[locale]/(hydrogen)/finance/reports/gl/cash-flow/page.tsx` |
| Cost Center P&L | `/finance/reports/gl/cost-center-pl` | `app/[locale]/(hydrogen)/finance/reports/gl/cost-center-pl/page.tsx` |
| Project P&L | `/finance/reports/gl/project-pl` | `app/[locale]/(hydrogen)/finance/reports/gl/project-pl/page.tsx` |
| Customer Aging | `/finance/reports/arap/customer-aging` | `app/[locale]/(hydrogen)/finance/reports/arap/customer-aging/page.tsx` |
| Vendor Aging | `/finance/reports/arap/vendor-aging` | `app/[locale]/(hydrogen)/finance/reports/arap/vendor-aging/page.tsx` |

---

## Data Flow Diagrams

### GL Report Data Flow
```
┌─────────────────┐     ┌────────────────────┐     ┌─────────────────────┐
│  Frontend Page  │ ──> │  API Service (TS)  │ ──> │  Backend Controller │
│  (React/Next)   │     │  financial-reports │     │  GLFinancialReport  │
└─────────────────┘     └────────────────────┘     └─────────────────────┘
                                                              │
                                                              ▼
┌─────────────────┐     ┌────────────────────┐     ┌─────────────────────┐
│  Display Data   │ <── │  Response DTO      │ <── │  Service Layer      │
│  (UI Components)│     │  (JSON)            │     │  GLFinancialReport  │
└─────────────────┘     └────────────────────┘     └─────────────────────┘
                                                              │
                                                              ▼
                                                   ┌─────────────────────┐
                                                   │  Repository Layer   │
                                                   │  - Account Repo     │
                                                   │  - Entry Repo       │
                                                   │  - Report Repo      │
                                                   └─────────────────────┘
                                                              │
                                                              ▼
                                                   ┌─────────────────────┐
                                                   │  Database           │
                                                   │  - GLAccount        │
                                                   │  - GLJournalEntry   │
                                                   │  - GLJournal        │
                                                   └─────────────────────┘
```

### Balance Calculation Flow
```
Account Balance Query
        │
        ▼
┌───────────────────────────────────────────────────────────────┐
│  SELECT SUM(BaseDebit) as TotalDebit,                         │
│         SUM(BaseCredit) as TotalCredit                        │
│  FROM GLJournalEntry e                                        │
│  JOIN GLJournal j ON e.JournalId = j.Id                       │
│  WHERE e.AccountId = @AccountId                               │
│    AND j.TransactionDate >= @StartDate                        │
│    AND j.TransactionDate <= @EndDate                          │
│    AND j.Status = 'Posted'                                    │
│    AND (j.CompanyID = @CompanyId OR @CompanyId IS NULL)       │
└───────────────────────────────────────────────────────────────┘
        │
        ▼
┌───────────────────────────────────────────────────────────────┐
│  AccountBalance {                                             │
│    TotalDebit: decimal,                                       │
│    TotalCredit: decimal                                       │
│  }                                                            │
└───────────────────────────────────────────────────────────────┘
```

---

## Appendix: Account Type Reference

| AccountType | Normal Balance | Used In |
|-------------|----------------|---------|
| Asset | Debit | Balance Sheet (Assets) |
| ContraAsset | Credit | Balance Sheet (Assets) |
| Liability | Credit | Balance Sheet (Liabilities) |
| ContraLiability | Debit | Balance Sheet (Liabilities) |
| Equity | Credit | Balance Sheet (Equity) |
| ContraEquity | Debit | Balance Sheet (Equity) |
| Revenue | Credit | Income Statement |
| ContraRevenue | Debit | Income Statement |
| Expense | Debit | Income Statement |

---

*Document generated: December 14, 2024*
*Version: 1.0*
*Maham ERP Finance Module*
