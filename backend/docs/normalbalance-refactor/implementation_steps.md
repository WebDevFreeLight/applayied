# Implementation Steps - NormalBalance Refactoring

## Overview

This document provides a step-by-step implementation guide for refactoring the normalBalance handling. Each step is designed to be atomic and testable, minimizing risk of breaking existing functionality.

---

## Step 1: Create Interface Definitions

### 1.1 Create INormalBalanceResolver Interface

**File:** `ZR.Service\IService\ERP\Finance\GL\INormalBalanceResolver.cs`

**Action:** Create new file with interface definition.

**Verification:**
- [ ] File compiles without errors
- [ ] No other files affected

---

### 1.2 Create IAccountBalanceCalculator Interface

**File:** `ZR.Service\IService\ERP\Finance\GL\IAccountBalanceCalculator.cs`

**Action:** Create new file with interface definition.

**Verification:**
- [ ] File compiles without errors
- [ ] No other files affected

---

## Step 2: Create Implementation Classes

### 2.1 Create NormalBalanceResolver Implementation

**File:** `ZR.Service\ERP\Finance\GL\NormalBalanceResolver.cs`

**Action:** Create implementation class with `[AppService]` attribute.

**Verification:**
- [ ] File compiles without errors
- [ ] Unit tests pass for all methods:
  - `CalculateBalance()` with Debit-normal account
  - `CalculateBalance()` with Credit-normal account
  - `GetClosingEntry()` for positive debit-normal
  - `GetClosingEntry()` for negative debit-normal
  - `GetClosingEntry()` for positive credit-normal
  - `GetClosingEntry()` for negative credit-normal
  - `GetDefaultNormalBalance()` for all AccountTypes
  - `ValidateNormalBalance()` returns correct results

---

### 2.2 Create AccountBalanceCalculator Implementation

**File:** `ZR.Service\ERP\Finance\GL\AccountBalanceCalculator.cs`

**Action:** Create implementation class.

**Verification:**
- [ ] File compiles without errors
- [ ] Integration tests pass

---

## Step 3: Refactor GLFinancialReportService

### 3.1 Add Constructor Dependency

**File:** `ZR.Service\ERP\Finance\GL\GLFinancialReportService.cs`

**Action:**
1. Add `INormalBalanceResolver` to constructor parameters
2. Store as private readonly field

**Code Change:**
```csharp
// Before
public GLFinancialReportService(
    IGLFinancialReportRepository reportRepository,
    IGLAccountRepository accountRepository,
    IGLJournalEntryRepository entryRepository,
    IFiscalYearService fiscalYearService)

// After
public GLFinancialReportService(
    IGLFinancialReportRepository reportRepository,
    IGLAccountRepository accountRepository,
    IGLJournalEntryRepository entryRepository,
    IFiscalYearService fiscalYearService,
    INormalBalanceResolver normalBalanceResolver)
{
    // ... existing assignments ...
    _normalBalanceResolver = normalBalanceResolver;
}
```

**Verification:**
- [ ] Service resolves correctly from DI container
- [ ] Existing functionality unchanged

---

### 3.2 Refactor GetTrialBalanceData() - Line 368

**Action:** Replace hardcoded formula with resolver call.

**Code Change:**
```csharp
// Before (Line 368)
Balance = balance.TotalDebit - balance.TotalCredit

// After
Balance = _normalBalanceResolver.CalculateBalance(
    balance.TotalDebit,
    balance.TotalCredit,
    account.NormalBalance)
```

**Verification:**
- [ ] Trial Balance report generates correctly
- [ ] Credit-normal accounts show positive balances
- [ ] Debit-normal accounts show positive balances

---

### 3.3 Refactor GetTrialBalanceDataAsync() - Line 405

**Action:** Same as 3.2 for async method.

**Verification:**
- [ ] Async Trial Balance works correctly

---

### 3.4 Refactor GetBalanceSheetData() - Lines 464, 475

**Action:**
1. Replace hardcoded formula
2. Remove `Math.Abs()` workaround

**Code Change:**
```csharp
// Before (Lines 464, 475)
var netBalance = balance.TotalDebit - balance.TotalCredit;
Balance = Math.Abs(netBalance)

// After
Balance = _normalBalanceResolver.CalculateBalance(
    balance.TotalDebit,
    balance.TotalCredit,
    account.NormalBalance)
```

**Verification:**
- [ ] Balance Sheet generates correctly
- [ ] ContraAsset accounts (e.g., Accumulated Depreciation) show correctly
- [ ] Assets, Liabilities, Equity sections balanced

---

### 3.5 Refactor GetBalanceSheetDataAsync() - Lines 549, 560

**Action:** Same as 3.4 for async method.

**Verification:**
- [ ] Async Balance Sheet works correctly

---

### 3.6 Refactor Income Statement Methods - Lines 607, 617, 668

**Action:**
1. Replace `AccountType` checks with `normalBalance` check
2. Remove `Math.Abs()` workaround

**Code Change:**
```csharp
// Before (conceptual)
if (account.AccountType == AccountType.Revenue)
    Amount = totalCredit - totalDebit;
else if (account.AccountType == AccountType.Expense)
    Amount = totalDebit - totalCredit;

// After
Amount = _normalBalanceResolver.CalculateBalance(
    balance.TotalDebit,
    balance.TotalCredit,
    account.NormalBalance)
```

**Verification:**
- [ ] Income Statement generates correctly
- [ ] Revenue accounts show positive amounts
- [ ] Expense accounts show positive amounts
- [ ] Net Income calculates correctly

---

### 3.7 Refactor GetIncomeStatementByPeriodData() - Lines 1716-1725

**Action:** Replace `AccountType` checks.

**Verification:**
- [ ] Period Income Statement works correctly

---

### 3.8 Refactor GetIncomeStatementByPeriodDataAsync() - Lines 1846-1855

**Action:** Same as 3.7 for async method.

**Verification:**
- [ ] Async Period Income Statement works correctly

---

### 3.9 Refactor Cash Flow Statement Methods - Lines 1068-1529

**Action:** Replace `AccountType` checks with `normalBalance` lookups.

**Verification:**
- [ ] Cash Flow Statement generates correctly
- [ ] Operating, Investing, Financing sections accurate

---

## Step 4: Refactor GLAccountOpeningBalanceService

### 4.1 Add Constructor Dependency

**File:** `ZR.Service\ERP\Finance\GL\GLAccountOpeningBalanceService.cs`

**Action:** Add `INormalBalanceResolver` to constructor.

**Verification:**
- [ ] Service resolves correctly

---

### 4.2 Refactor CreateAsync() - Line 192

**Action:** Use resolver for opening balance calculation.

**Code Change:**
```csharp
// Before
entity.OpeningBalance = dto.OpeningDebit - dto.OpeningCredit;

// After
var account = await _accountRepository.GetByIdAsync(dto.AccountId);
entity.OpeningBalance = _normalBalanceResolver.CalculateBalance(
    dto.OpeningDebit,
    dto.OpeningCredit,
    account.NormalBalance);
```

**Verification:**
- [ ] Opening balance creates correctly
- [ ] Credit-normal accounts store positive values
- [ ] Debit-normal accounts store positive values

---

### 4.3 Refactor UpdateAsync() - Line 250

**Action:** Same as 4.2 for update method.

**Verification:**
- [ ] Opening balance updates correctly

---

## Step 5: Refactor YearEndClosingService

### 5.1 Add Constructor Dependency

**File:** `ZR.Service\ERP\Finance\GL\YearEndClosingService.cs`

**Action:** Add `INormalBalanceResolver` to constructor.

---

### 5.2 Refactor CloseFiscalYearAndTransferProfitLossAsync() - Lines 121-136

**Action:** Use `GetClosingEntry()` method for closing entries.

**Code Change:**
```csharp
// Before (Lines 121-122)
Debit = account.Balance > 0 ? account.Balance : 0,
Credit = account.Balance < 0 ? Math.Abs(account.Balance) : 0,

// After
var (debit, credit) = _normalBalanceResolver.GetClosingEntry(
    account.Balance,
    account.NormalBalance);
```

**Verification:**
- [ ] Year-end closing creates correct entries
- [ ] Revenue accounts close to zero
- [ ] Expense accounts close to zero
- [ ] Net Income transfers to Retained Earnings

---

### 5.3 Refactor PreviewYearEndClosingAsync() - Lines 247-264

**Action:** Use resolver for preview closing entries.

**Verification:**
- [ ] Preview shows correct closing entries

---

## Step 6: Refactor BudgetService

### 6.1 Add Constructor Dependency

**File:** `ZR.Service\ERP\Finance\Budget\BudgetService.cs`

**Action:** Add `INormalBalanceResolver` to constructor.

---

### 6.2 Refactor Variance Calculation - Line 552

**Code Change:**
```csharp
// Before
var actualAmount = Math.Abs(balance.Balance);

// After
var actualAmount = _normalBalanceResolver.CalculateBalance(
    balance.TotalDebit,
    balance.TotalCredit,
    account.NormalBalance);
```

**Verification:**
- [ ] Budget variance calculates correctly
- [ ] Expense budgets show correct actuals

---

## Step 7: Refactor GLAccountService

### 7.1 Add Constructor Dependency

**File:** `ZR.Service\ERP\Finance\GL\GLAccountService.cs`

**Action:** Add `INormalBalanceResolver` to constructor.

---

### 7.2 Refactor GetBalanceSummary Methods

**Action:** Use resolver for balance calculations in summary methods.

**Verification:**
- [ ] Account balance summaries correct
- [ ] API responses show correct balances

---

## Step 8: Validation & Cleanup

### 8.1 Remove Unused GetDefaultNormalBalance from GLAccountService

**Action:** Remove duplicate method since it's now in resolver.

**Note:** Keep for backward compatibility or mark as `[Obsolete]`.

---

### 8.2 Add Validation Warnings

**Action:** Add logging when account normalBalance doesn't match expected for AccountType.

```csharp
if (!_normalBalanceResolver.ValidateNormalBalance(account.AccountType, account.NormalBalance))
{
    _logger.LogWarning(
        "Account {AccountId} has non-standard normalBalance. Type: {Type}, NormalBalance: {NormalBalance}",
        account.AccountId, account.AccountType, account.NormalBalance);
}
```

---

## Step 9: Final Testing

### 9.1 Unit Test Coverage

- [ ] NormalBalanceResolver: 100% method coverage
- [ ] AccountBalanceCalculator: 100% method coverage
- [ ] Each refactored service method: Positive and negative scenarios

### 9.2 Integration Tests

- [ ] Trial Balance with mixed accounts
- [ ] Balance Sheet with ContraAsset accounts
- [ ] Income Statement with all account types
- [ ] Year-End Closing complete workflow
- [ ] Opening Balance import and carryforward

### 9.3 Regression Tests

- [ ] All existing financial report tests pass
- [ ] All existing API endpoint tests pass
- [ ] Performance benchmarks unchanged

---

## Execution Order Summary

| Phase | Steps | Duration Est. | Risk |
|-------|-------|---------------|------|
| 1 | Steps 1.1, 1.2 | 1 hour | None |
| 2 | Steps 2.1, 2.2 | 2 hours | Low |
| 3 | Steps 3.1-3.9 | 4 hours | Medium |
| 4 | Steps 4.1-4.3 | 1 hour | Low |
| 5 | Steps 5.1-5.3 | 1 hour | Low |
| 6 | Steps 6.1-6.2 | 30 min | Low |
| 7 | Steps 7.1-7.2 | 1 hour | Low |
| 8 | Steps 8.1-8.2 | 30 min | None |
| 9 | Step 9.1-9.3 | 4 hours | None |

**Total Estimated Time:** 15 hours (2 days)

---

## Rollback Checkpoints

After each step, create a Git commit with a descriptive message:

```bash
git commit -m "refactor(normalBalance): Step X.Y - Description"
```

If issues are found:
```bash
git revert <commit-hash>
```

---

**Document Date:** 2026-01-08
**Author:** Claude Code - Finance Systems Architect
