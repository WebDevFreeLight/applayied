# Test Plan - NormalBalance Refactoring

## 1. Unit Test Plan

### 1.1 NormalBalanceResolver Tests

**Test Class:** `NormalBalanceResolverTests`

#### CalculateBalance Tests

| Test Name | Input | Expected Output | Category |
|-----------|-------|-----------------|----------|
| `CalculateBalance_DebitNormal_PositiveDebit_ReturnsPositive` | Debit=100, Credit=40, Normal=Debit | 60 | Positive |
| `CalculateBalance_DebitNormal_PositiveCredit_ReturnsNegative` | Debit=40, Credit=100, Normal=Debit | -60 | Negative |
| `CalculateBalance_DebitNormal_Zero_ReturnsZero` | Debit=100, Credit=100, Normal=Debit | 0 | Zero |
| `CalculateBalance_CreditNormal_PositiveCredit_ReturnsPositive` | Debit=40, Credit=100, Normal=Credit | 60 | Positive |
| `CalculateBalance_CreditNormal_PositiveDebit_ReturnsNegative` | Debit=100, Credit=40, Normal=Credit | -60 | Negative |
| `CalculateBalance_CreditNormal_Zero_ReturnsZero` | Debit=100, Credit=100, Normal=Credit | 0 | Zero |

```csharp
[Theory]
[InlineData(100, 40, NormalBalance.Debit, 60)]
[InlineData(40, 100, NormalBalance.Debit, -60)]
[InlineData(100, 100, NormalBalance.Debit, 0)]
[InlineData(40, 100, NormalBalance.Credit, 60)]
[InlineData(100, 40, NormalBalance.Credit, -60)]
[InlineData(100, 100, NormalBalance.Credit, 0)]
public void CalculateBalance_ReturnsCorrectValue(
    decimal debit, decimal credit, NormalBalance normal, decimal expected)
{
    var resolver = new NormalBalanceResolver();
    var result = resolver.CalculateBalance(debit, credit, normal);
    Assert.Equal(expected, result);
}
```

#### GetClosingEntry Tests

| Test Name | Current Balance | Normal Balance | Expected Debit | Expected Credit |
|-----------|-----------------|----------------|----------------|-----------------|
| `GetClosingEntry_DebitNormal_Positive_ReturnsCredit` | 100 | Debit | 0 | 100 |
| `GetClosingEntry_DebitNormal_Negative_ReturnsDebit` | -100 | Debit | 100 | 0 |
| `GetClosingEntry_CreditNormal_Positive_ReturnsDebit` | 100 | Credit | 100 | 0 |
| `GetClosingEntry_CreditNormal_Negative_ReturnsCredit` | -100 | Credit | 0 | 100 |
| `GetClosingEntry_Zero_ReturnsZero` | 0 | Debit | 0 | 0 |

```csharp
[Theory]
[InlineData(100, NormalBalance.Debit, 0, 100)]
[InlineData(-100, NormalBalance.Debit, 100, 0)]
[InlineData(100, NormalBalance.Credit, 100, 0)]
[InlineData(-100, NormalBalance.Credit, 0, 100)]
[InlineData(0, NormalBalance.Debit, 0, 0)]
public void GetClosingEntry_ReturnsCorrectEntry(
    decimal balance, NormalBalance normal, decimal expectedDebit, decimal expectedCredit)
{
    var resolver = new NormalBalanceResolver();
    var (debit, credit) = resolver.GetClosingEntry(balance, normal);
    Assert.Equal(expectedDebit, debit);
    Assert.Equal(expectedCredit, credit);
}
```

#### GetDefaultNormalBalance Tests

| Account Type | Expected Normal Balance |
|--------------|------------------------|
| Asset | Debit |
| Expense | Debit |
| ContraLiability | Debit |
| ContraEquity | Debit |
| ContraRevenue | Debit |
| Liability | Credit |
| Equity | Credit |
| Revenue | Credit |
| ContraAsset | Credit |
| OtherComprehensiveIncome | Credit |

```csharp
[Theory]
[InlineData(AccountType.Asset, NormalBalance.Debit)]
[InlineData(AccountType.Expense, NormalBalance.Debit)]
[InlineData(AccountType.Liability, NormalBalance.Credit)]
[InlineData(AccountType.Equity, NormalBalance.Credit)]
[InlineData(AccountType.Revenue, NormalBalance.Credit)]
[InlineData(AccountType.ContraAsset, NormalBalance.Credit)]
[InlineData(AccountType.ContraRevenue, NormalBalance.Debit)]
public void GetDefaultNormalBalance_ReturnsCorrectDefault(
    AccountType type, NormalBalance expected)
{
    var resolver = new NormalBalanceResolver();
    var result = resolver.GetDefaultNormalBalance(type);
    Assert.Equal(expected, result);
}
```

---

### 1.2 AccountBalanceCalculator Tests

**Test Class:** `AccountBalanceCalculatorTests`

| Test Name | Scenario | Expected Behavior |
|-----------|----------|-------------------|
| `GetAccountBalanceAsync_Asset_ReturnsPositiveForDebitBalance` | Asset account with DR>CR | Positive balance |
| `GetAccountBalanceAsync_Liability_ReturnsPositiveForCreditBalance` | Liability account with CR>DR | Positive balance |
| `GetAccountBalanceAsync_ContraAsset_ReturnsPositiveForCreditBalance` | Accumulated Depreciation | Positive balance |
| `GetAccountBalanceAsync_NonExistentAccount_ReturnsNull` | Invalid account ID | Null result |

---

## 2. Integration Test Plan

### 2.1 Trial Balance Tests

**Test Class:** `TrialBalanceIntegrationTests`

| Test Name | Setup | Assertion |
|-----------|-------|-----------|
| `TrialBalance_MixedAccounts_AllPositive` | Create Asset, Liability, Revenue, Expense accounts with transactions | All balances positive |
| `TrialBalance_ContraAsset_CorrectSign` | Create Accumulated Depreciation with credits | Balance shows correctly |
| `TrialBalance_TotalDebitsEqualsCredits` | Various transactions | Total DR = Total CR |

```csharp
[Fact]
public async Task TrialBalance_ContraAsset_ShowsPositiveBalance()
{
    // Arrange
    var accumulatedDepreciation = await CreateAccount(
        accountType: AccountType.ContraAsset,
        normalBalance: NormalBalance.Credit);

    await PostJournalEntry(
        accountId: accumulatedDepreciation.AccountId,
        debit: 0,
        credit: 50000);  // Depreciation

    // Act
    var trialBalance = await _reportService.GetTrialBalanceDataAsync(startDate, endDate);

    // Assert
    var line = trialBalance.Lines.First(l => l.AccountId == accumulatedDepreciation.AccountId);
    Assert.True(line.Balance > 0, "ContraAsset should show positive balance");
}
```

---

### 2.2 Balance Sheet Tests

**Test Class:** `BalanceSheetIntegrationTests`

| Test Name | Setup | Assertion |
|-----------|-------|-----------|
| `BalanceSheet_Assets_ShowPositive` | Asset accounts with debit balances | All asset values positive |
| `BalanceSheet_Liabilities_ShowPositive` | Liability accounts with credit balances | All liability values positive |
| `BalanceSheet_ContraAsset_ReducesTotal` | Accumulated Depreciation | Net fixed assets correct |
| `BalanceSheet_IsBalanced` | Full chart of accounts | Assets = Liabilities + Equity |

```csharp
[Fact]
public async Task BalanceSheet_AccumulatedDepreciation_ReducesNetAssets()
{
    // Arrange
    var fixedAssets = await CreateAccount(AccountType.Asset, NormalBalance.Debit);
    var accumDepr = await CreateAccount(AccountType.ContraAsset, NormalBalance.Credit);

    await PostJournalEntry(fixedAssets.AccountId, debit: 100000, credit: 0);  // Purchase asset
    await PostJournalEntry(accumDepr.AccountId, debit: 0, credit: 20000);     // Depreciation

    // Act
    var balanceSheet = await _reportService.GetBalanceSheetDataAsync(DateTime.Today);

    // Assert
    var netAssets = balanceSheet.TotalAssets;  // Should be 80,000 (100,000 - 20,000)
    Assert.Equal(80000, netAssets);
}
```

---

### 2.3 Income Statement Tests

**Test Class:** `IncomeStatementIntegrationTests`

| Test Name | Setup | Assertion |
|-----------|-------|-----------|
| `IncomeStatement_Revenue_ShowsPositive` | Revenue with credit balance | Positive revenue |
| `IncomeStatement_Expenses_ShowsPositive` | Expenses with debit balance | Positive expenses |
| `IncomeStatement_ContraRevenue_ReducesTotal` | Sales Returns | Net revenue correct |
| `IncomeStatement_NetIncome_Calculated` | Various R&E accounts | Correct net income |

```csharp
[Fact]
public async Task IncomeStatement_SalesReturns_ReducesRevenue()
{
    // Arrange
    var salesRevenue = await CreateAccount(AccountType.Revenue, NormalBalance.Credit);
    var salesReturns = await CreateAccount(AccountType.ContraRevenue, NormalBalance.Debit);

    await PostJournalEntry(salesRevenue.AccountId, debit: 0, credit: 100000);  // Sales
    await PostJournalEntry(salesReturns.AccountId, debit: 5000, credit: 0);     // Returns

    // Act
    var incomeStatement = await _reportService.GetIncomeStatementDataAsync(startDate, endDate);

    // Assert
    Assert.Equal(95000, incomeStatement.TotalRevenue);  // 100,000 - 5,000
}
```

---

### 2.4 Year-End Closing Tests

**Test Class:** `YearEndClosingIntegrationTests`

| Test Name | Setup | Assertion |
|-----------|-------|-----------|
| `YearEndClosing_RevenueAccounts_CloseToZero` | Revenue accounts with balances | Post-close balance = 0 |
| `YearEndClosing_ExpenseAccounts_CloseToZero` | Expense accounts with balances | Post-close balance = 0 |
| `YearEndClosing_NetProfit_TransfersToRetainedEarnings` | Net profit scenario | RE increases |
| `YearEndClosing_NetLoss_TransfersToRetainedEarnings` | Net loss scenario | RE decreases |
| `YearEndClosing_ClosingJournal_IsBalanced` | Any scenario | DR = CR |

```csharp
[Fact]
public async Task YearEndClosing_ClosingEntries_AreBalanced()
{
    // Arrange
    var revenue = await CreateRevenueAccountWithBalance(100000);
    var expense = await CreateExpenseAccountWithBalance(60000);

    // Act
    var result = await _yearEndService.CloseFiscalYearAndTransferProfitLossAsync(
        new CloseFiscalYearDto { FiscalYear = 2025, AutoPost = true }, "TestUser");

    // Assert
    Assert.True(result.Success);
    var closingJournal = await _journalService.GetWithEntries(result.ClosingJournalId);
    var totalDebit = closingJournal.Entries.Sum(e => e.Debit);
    var totalCredit = closingJournal.Entries.Sum(e => e.Credit);
    Assert.Equal(totalDebit, totalCredit);
}
```

---

### 2.5 Opening Balance Tests

**Test Class:** `OpeningBalanceIntegrationTests`

| Test Name | Setup | Assertion |
|-----------|-------|-----------|
| `OpeningBalance_Asset_StoresPositive` | Asset account with DR opening | Positive stored value |
| `OpeningBalance_Liability_StoresPositive` | Liability account with CR opening | Positive stored value |
| `OpeningBalance_ImportBulk_AllCorrect` | Multiple accounts | All balances correct |
| `OpeningBalance_Carryforward_PreservesBalances` | Year-end to new year | Balances match |

```csharp
[Fact]
public async Task OpeningBalance_LiabilityAccount_CalculatesCorrectly()
{
    // Arrange
    var apAccount = await CreateAccount(AccountType.Liability, NormalBalance.Credit);
    var dto = new CreateOpeningBalanceDto
    {
        AccountId = apAccount.AccountId,
        FiscalYear = 2026,
        OpeningDebit = 0,
        OpeningCredit = 50000
    };

    // Act
    var result = await _openingBalanceService.CreateAsync(dto);

    // Assert
    Assert.Equal(50000, result.OpeningBalance);  // Should be positive
}
```

---

## 3. Regression Test Plan

### 3.1 Existing API Endpoint Tests

| Endpoint | Method | Test Scenario |
|----------|--------|---------------|
| `/api/finance/accounts/balance` | GET | Returns correct balance |
| `/api/finance/reports/trial-balance` | GET | Returns correct report |
| `/api/finance/reports/balance-sheet` | GET | Returns correct report |
| `/api/finance/reports/income-statement` | GET | Returns correct report |
| `/api/finance/reports/cash-flow` | GET | Returns correct report |
| `/api/finance/year-end/close` | POST | Creates correct entries |
| `/api/finance/opening-balances` | POST | Stores correct values |

### 3.2 Performance Regression Tests

| Test | Baseline | Acceptable Variance |
|------|----------|---------------------|
| Trial Balance (1000 accounts) | < 2 seconds | ± 10% |
| Balance Sheet generation | < 3 seconds | ± 10% |
| Year-End Closing | < 5 seconds | ± 10% |
| Opening Balance import (100 accounts) | < 10 seconds | ± 10% |

---

## 4. Scenarios Where Accounting Balance May Break

### 4.1 Misconfigured normalBalance

**Scenario:** Asset account configured with Credit normalBalance.

**Impact:**
- Trial Balance shows negative balance
- Balance Sheet shows negative assets
- Aging reports incorrect

**Test:**
```csharp
[Fact]
public async Task MisconfiguredAsset_ShowsWarning()
{
    // Arrange
    var misconfiguredAsset = await CreateAccount(
        accountType: AccountType.Asset,
        normalBalance: NormalBalance.Credit);  // WRONG!

    // Act
    var validation = _normalBalanceResolver.ValidateNormalBalance(
        AccountType.Asset,
        NormalBalance.Credit);

    // Assert
    Assert.False(validation, "Should return false for non-standard config");
}
```

### 4.2 ContraAsset Handling

**Scenario:** Accumulated Depreciation not reducing asset total.

**Impact:**
- Net book value overstated
- Balance Sheet incorrect

**Test:**
```csharp
[Fact]
public async Task ContraAsset_ReducesNetAssets()
{
    // ... test as shown in 2.2
}
```

### 4.3 Year-End Closing Edge Cases

**Scenario:** Account with abnormal balance (e.g., negative expense).

**Impact:**
- Closing entry reversed
- Net income incorrect

**Test:**
```csharp
[Fact]
public async Task YearEndClosing_NegativeExpense_HandlesCorrectly()
{
    // Arrange - Expense account with credit balance (abnormal)
    var expense = await CreateAccount(AccountType.Expense, NormalBalance.Debit);
    await PostJournalEntry(expense.AccountId, debit: 0, credit: 5000);  // Abnormal

    // Act
    var preview = await _yearEndService.PreviewYearEndClosingAsync(2025);

    // Assert
    var expenseEntry = preview.ExpenseAccounts.First(a => a.AccountId == expense.AccountId);
    Assert.Equal(0, expenseEntry.ClosingDebit);   // Should be 0
    Assert.Equal(5000, expenseEntry.ClosingCredit);  // Credit to bring to zero (wrong!)

    // Actual expected with resolver:
    // Debit = 5000 to bring negative debit-normal back to zero
}
```

### 4.4 Opening Balance Import Misalignment

**Scenario:** Opening balance imported with wrong DR/CR direction.

**Impact:**
- Account starts fiscal year with incorrect balance
- All subsequent calculations wrong

**Test:**
```csharp
[Fact]
public async Task OpeningBalance_ValidationRejectsWrongDirection()
{
    // Arrange
    var liability = await CreateAccount(AccountType.Liability, NormalBalance.Credit);

    // Act & Assert - Should warn or reject debit opening for liability
    var dto = new CreateOpeningBalanceDto
    {
        AccountId = liability.AccountId,
        OpeningDebit = 50000,  // Wrong direction for liability!
        OpeningCredit = 0
    };

    // With resolver, this should still calculate correctly
    // OpeningBalance = CalculateBalance(50000, 0, Credit) = 0 - 50000 = -50000
    // This indicates abnormal balance (debit balance for liability)
}
```

---

## 5. Test Data Setup

### 5.1 Account Test Data

```csharp
public static class TestAccounts
{
    // Debit-Normal Accounts
    public static readonly TestAccount CashAccount = new()
    {
        Name = "Cash",
        AccountType = AccountType.Asset,
        NormalBalance = NormalBalance.Debit
    };

    public static readonly TestAccount OfficeSupplies = new()
    {
        Name = "Office Supplies Expense",
        AccountType = AccountType.Expense,
        NormalBalance = NormalBalance.Debit
    };

    // Credit-Normal Accounts
    public static readonly TestAccount AccountsPayable = new()
    {
        Name = "Accounts Payable",
        AccountType = AccountType.Liability,
        NormalBalance = NormalBalance.Credit
    };

    public static readonly TestAccount SalesRevenue = new()
    {
        Name = "Sales Revenue",
        AccountType = AccountType.Revenue,
        NormalBalance = NormalBalance.Credit
    };

    // Contra Accounts
    public static readonly TestAccount AccumulatedDepreciation = new()
    {
        Name = "Accumulated Depreciation",
        AccountType = AccountType.ContraAsset,
        NormalBalance = NormalBalance.Credit
    };

    public static readonly TestAccount SalesReturns = new()
    {
        Name = "Sales Returns",
        AccountType = AccountType.ContraRevenue,
        NormalBalance = NormalBalance.Debit
    };
}
```

### 5.2 Journal Entry Test Data

```csharp
public static class TestJournalEntries
{
    public static CreateJournalDto SaleTransaction(Guid arAccountId, Guid revenueAccountId, decimal amount)
    {
        return new CreateJournalDto
        {
            JournalDate = DateTime.Today,
            DescriptionEn = "Test Sale",
            Entries = new List<CreateJournalEntryDto>
            {
                new() { AccountId = arAccountId, Debit = amount, Credit = 0 },
                new() { AccountId = revenueAccountId, Debit = 0, Credit = amount }
            }
        };
    }

    public static CreateJournalDto DepreciationEntry(Guid expenseAccountId, Guid accumDeprAccountId, decimal amount)
    {
        return new CreateJournalDto
        {
            JournalDate = DateTime.Today,
            DescriptionEn = "Monthly Depreciation",
            Entries = new List<CreateJournalEntryDto>
            {
                new() { AccountId = expenseAccountId, Debit = amount, Credit = 0 },
                new() { AccountId = accumDeprAccountId, Debit = 0, Credit = amount }
            }
        };
    }
}
```

---

## 6. Test Execution Checklist

### 6.1 Pre-Deployment

- [ ] All unit tests pass (NormalBalanceResolver, AccountBalanceCalculator)
- [ ] All integration tests pass
- [ ] All regression tests pass
- [ ] Performance benchmarks within acceptable range
- [ ] Code review completed
- [ ] Documentation updated

### 6.2 Post-Deployment

- [ ] Smoke test: Generate Trial Balance
- [ ] Smoke test: Generate Balance Sheet
- [ ] Smoke test: Generate Income Statement
- [ ] Smoke test: Create Opening Balance
- [ ] Verify: ContraAsset accounts display correctly
- [ ] Verify: Year-End preview shows correct entries

---

## 7. Test Coverage Targets

| Component | Target Coverage |
|-----------|-----------------|
| NormalBalanceResolver | 100% |
| AccountBalanceCalculator | 90% |
| GLFinancialReportService (balance calcs) | 80% |
| YearEndClosingService (closing entries) | 80% |
| GLAccountOpeningBalanceService | 80% |

---

**Document Date:** 2026-01-08
**Author:** Claude Code - Finance Systems Architect
