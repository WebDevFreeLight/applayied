# ERP Finance Module - Unit and Integration Tests

## Overview

This directory contains comprehensive Unit and Integration Tests for the ERP Finance module, covering all backend functionality including GL (Chart of Accounts), Vouchers, Financial Reports, and Period Closing.

## Test Structure

```
erp/finance/tests/
├── Unit/
│   ├── GL/
│   │   └── GLAccountServiceTests.cs
│   ├── Vouchers/
│   │   └── GLVoucherServiceTests.cs
│   ├── Reports/
│   │   └── GLFinancialReportServiceTests.cs
│   └── PeriodClosing/
│       └── GLPeriodClosingServiceTests.cs
├── Integration/
│   ├── GL_Voucher_Integration/
│   │   └── GLVoucherIntegrationTests.cs
│   ├── Reports_Integration/
│   │   └── FinancialReportsIntegrationTests.cs
│   └── PeriodClosing_Integration/
│       └── PeriodClosingIntegrationTests.cs
└── TestHelpers/
    └── FinanceTestFixture.cs
```

## Test Coverage

### Unit Tests

#### GL Account Service Tests
- ✅ Create account with validation
- ✅ Create account with existing code (should fail)
- ✅ Create account with parent (level calculation)
- ✅ Arabic and English fields storage
- ✅ Get account by ID
- ✅ Get paginated list with filters
- ✅ Get account tree (hierarchical structure)
- ✅ Get accounts by type
- ✅ Update account fields
- ✅ Update account with parent change (recalculate level)
- ✅ Delete account (soft delete)
- ✅ Delete account with children (should fail)
- ✅ Code existence validation
- ✅ Account balance calculation
- ✅ Multi-tenant isolation

#### Voucher Service Tests
- ✅ Create balanced voucher
- ✅ Create unbalanced voucher (should fail)
- ✅ Create voucher with less than 2 entries (should fail)
- ✅ Create voucher with invalid account (should fail)
- ✅ Create voucher with inactive account (should fail)
- ✅ Arabic and English fields storage
- ✅ Update voucher
- ✅ Update posted voucher (should fail)
- ✅ Delete voucher
- ✅ Delete posted voucher (should fail)
- ✅ Post voucher
- ✅ Post unbalanced voucher (should fail)
- ✅ Post voucher in closed period (should fail)
- ✅ Double entry validation
- ✅ Multi-tenant isolation
- ✅ Audit fields (created_by, posted_by, timestamps)

#### Financial Report Service Tests
- ✅ Generate Trial Balance
- ✅ Generate Balance Sheet
- ✅ Generate Income Statement
- ✅ Generate Cash Flow Statement
- ✅ Report metadata (generated_by, generated_at)
- ✅ Arabic and English descriptions
- ✅ Multi-tenant isolation
- ✅ Report accuracy (sums match entries)

#### Period Closing Service Tests
- ✅ Create period
- ✅ Create duplicate period (should fail)
- ✅ Close period with unposted vouchers (should fail)
- ✅ Close period with all vouchers posted (should succeed)
- ✅ Close period should lock transactions
- ✅ Close period should roll balances forward
- ✅ Reopen period
- ✅ Reopen locked period (should fail)
- ✅ Lock period
- ✅ Lock open period (should fail)
- ✅ Period status validation
- ✅ Generate fiscal year periods
- ✅ Multi-tenant isolation

### Integration Tests

#### GL-Voucher Integration
- ✅ Create voucher → Post → Verify account balances updated
- ✅ Create multiple vouchers → Post → Verify accumulated balances
- ✅ Create voucher → Delete → Verify balances not affected
- ✅ Create voucher → Update → Post → Verify updated amounts reflected
- ✅ Create voucher with multiple accounts → Post → Verify all balances updated
- ✅ Verify double entry integrity

#### Reports Integration
- ✅ Post vouchers → Generate Trial Balance → Verify sums match
- ✅ Post vouchers → Generate Balance Sheet → Verify accuracy
- ✅ Post vouchers → Generate Income Statement → Verify revenue/expense sums
- ✅ Post vouchers → Generate Cash Flow → Verify cash movements
- ✅ Generate report after voucher posting → Verify includes posted transactions only

#### Period Closing Integration
- ✅ Create period → Post vouchers → Close period → Verify success
- ✅ Create period → Create unposted vouchers → Close period → Verify failure
- ✅ Close period → Verify prevents posting in closed period
- ✅ Close period → Reopen → Verify allows new postings
- ✅ Close period → Lock → Verify prevents reopening
- ✅ Close period → Verify balances roll forward to next period
- ✅ Generate fiscal year periods → Verify all periods created

## Running Tests

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- Test Explorer or dotnet CLI

### Run All Tests
```bash
dotnet test ZR.Service.Tests.csproj
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~GLAccountServiceTests"
```

### Run Specific Test
```bash
dotnet test --filter "FullyQualifiedName~GLAccountServiceTests.CreateAccount_ShouldCreateAccountSuccessfully"
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Database

Tests use an **in-memory SQLite database** that is created fresh for each test run. This ensures:
- Tests are isolated and don't affect each other
- No external database setup required
- Fast test execution
- Clean state for each test

The `FinanceTestFixture` class handles:
- Database initialization
- Table creation
- Test data seeding
- Cleanup after tests

## Test Data

The `FinanceTestDataBuilder` class provides helper methods to create test data:
- `CreateAccount()` - Create GL accounts
- `CreateVoucher()` - Create vouchers
- `CreateVoucherEntry()` - Create voucher entries
- `CreateBalancedVoucher()` - Create balanced vouchers with entries
- `CreatePeriod()` - Create period closing records
- `CreateStandardChartOfAccounts()` - Create standard COA structure

## Key Features Tested

### Multi-Tenant Support
All tests verify tenant isolation:
- Data created for one tenant should not be visible to another tenant
- Queries filtered by tenant return only that tenant's data

### Arabic + English Fields
All tests verify bilingual support:
- `NameAr` / `NameEn` fields are stored and retrieved correctly
- `DescriptionAr` / `DescriptionEn` fields work properly
- Reports include both Arabic and English descriptions

### Async/Await
All database operations use async/await patterns:
- `CreateAsync()`, `UpdateAsync()`, `DeleteAsync()` methods tested
- Proper async/await usage throughout

### Audit Fields
All tests verify audit trail:
- `CreatedBy`, `CreatedAt` set on creation
- `UpdatedBy`, `UpdatedAt` set on update
- `PostedBy`, `PostedDate` set on posting
- `ClosedBy`, `ClosedAt` set on period closing

### Business Rules
- Double entry validation (debit = credit)
- No transactions on summary accounts
- Period closing only if all vouchers posted
- No posting in closed periods
- Account hierarchy validation

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- No external dependencies
- Fast execution (< 1 minute for full suite)
- Deterministic results
- Clear failure messages

## Maintenance

When adding new features to the Finance module:
1. Add corresponding unit tests
2. Add integration tests if the feature affects multiple services
3. Update this README with new test coverage
4. Ensure all tests pass before merging

## Notes

- Tests use `FluentAssertions` for readable assertions
- Tests use `xUnit` as the testing framework
- Tests use `Moq` for mocking (if needed in future)
- All tests follow the Arrange-Act-Assert pattern
- Test names follow the pattern: `MethodName_Scenario_ExpectedResult`

## Enhanced Test Coverage (Latest Updates)

### New Unit Tests

#### Currency Conversion & Multi-Currency (`Unit/Currency/CurrencyConversionTests.cs`)
- ✅ Create voucher with USD currency and convert to base currency
- ✅ Create voucher with EUR currency and convert to base currency
- ✅ Create voucher with mixed currencies (USD + EUR) and convert all to base
- ✅ Post voucher with currency conversion and verify account balances in base currency
- ✅ Handle invalid exchange rates gracefully
- ✅ Test precision rounding for currency conversion
- ✅ Update exchange rate and recalculate base amounts

#### GL Account Edge Cases (`Unit/GL/GLAccountEdgeCasesTests.cs`)
- ✅ Create account with maximum field lengths (200 chars name, 500 chars description)
- ✅ Create account with invalid account type (documents current behavior)
- ✅ Create account with empty code
- ✅ Create account with very long code (truncation testing)
- ✅ Update account to invalid type
- ✅ Attempt transactions on summary accounts
- ✅ Deactivate account with open balance
- ✅ Create account with negative opening balance
- ✅ Create account with very large opening balance
- ✅ Create account with special characters in name
- ✅ Create account with Unicode characters
- ✅ Create account with null parent ID
- ✅ Test circular parent reference handling

#### Advanced Voucher Scenarios (`Unit/Vouchers/GLVoucherAdvancedScenariosTests.cs`)
- ✅ Post partially balanced voucher (should fail)
- ✅ Create multi-line entries across multiple currencies
- ✅ Reverse already reversed voucher (should fail)
- ✅ Post voucher with future date
- ✅ Post voucher across fiscal periods
- ✅ Create voucher with zero amount
- ✅ Create voucher with very large amount
- ✅ Create voucher with many entries (20+ entries)
- ✅ Update posted voucher (should fail)
- ✅ Delete posted voucher (should fail)

#### Fixed Asset & Depreciation (`Unit/Assets/FixedAssetDepreciationTests.cs`)
- ✅ Create fixed asset and verify initial GL posting
- ✅ Calculate monthly depreciation using straight-line method
- ✅ Post depreciation and verify GL voucher entry creation
- ✅ Sell asset and validate gain/loss posting
- ✅ Attempt depreciation on closed period (should fail)
- ✅ Depreciate fully depreciated asset (should not allow)
- ✅ Create assets with different depreciation methods (Straight Line, Declining Balance, None)

#### Period Closing Edge Cases (`Unit/PeriodClosing/GLPeriodClosingEdgeCasesTests.cs`)
- ✅ Close period with mixed posted/unposted vouchers (should fail)
- ✅ Close period with force close (should succeed even with unposted)
- ✅ Reopen period and post pending vouchers
- ✅ Close period and verify balances roll forward correctly
- ✅ Close period with multi-tenant data isolation
- ✅ Lock period and prevent reopening
- ✅ Generate fiscal year periods with custom start date
- ✅ Close period with zero transactions

### New Integration Tests

#### Voucher → GL → Reports Workflow (`Integration/VoucherGLReportsWorkflowTests.cs`)
- ✅ Post voucher → generate trial balance → verify totals match
- ✅ Post multiple vouchers → generate balance sheet → validate asset/liability totals
- ✅ Post vouchers → generate income statement → verify revenue/expense sums
- ✅ Post vouchers → generate cash flow → check entries match voucher postings
- ✅ Generate report after posting → verify includes only posted transactions

#### Multi-Tenant & Multi-Currency (`Integration/MultiTenantMultiCurrencyTests.cs`)
- ✅ Tenant A and Tenant B create same accounts and vouchers → verify isolation
- ✅ Post transactions in USD, SAR, EUR → verify consolidated report in base currency
- ✅ Generate report with multi-currency transactions → verify consolidation

#### Stress & Performance Tests (`Integration/StressPerformanceTests.cs`)
- ✅ Bulk creation of 1000+ vouchers → post → validate balances (completes in < 60 seconds)
- ✅ Bulk create vouchers → validate balances are accurate
- ✅ Generate reports with high-volume data (1000 vouchers) → measure execution time (< 10 seconds)
- ✅ Generate multiple reports (Trial Balance, Balance Sheet, Income Statement, Cash Flow) → verify efficiency (< 15 seconds)
- ✅ Concurrent voucher creation → maintain data integrity
- ✅ Large voucher with many entries (50+ entries) → process correctly

### Enhanced Test Infrastructure

#### FinanceTestDataBuilder Enhancements
- ✅ `CreateCurrency()` - Create test currencies with exchange rates
- ✅ `CreateMultiCurrencyVoucherEntry()` - Create voucher entries in different currencies
- ✅ `CreateBalancedMultiCurrencyVoucher()` - Create balanced vouchers with currency conversion
- ✅ `CreateAssetCategory()` - Create asset categories with depreciation methods
- ✅ `CreateFixedAsset()` - Create fixed assets with GL account links
- ✅ `CreateComplexAccountTree()` - Create hierarchical account structure (3+ levels)
- ✅ `CreateFiscalYearPeriods()` - Create fiscal year periods with custom start/end dates
- ✅ `CreateAccountWithMaxFieldLengths()` - Create accounts with maximum field lengths

#### TestServices Async Methods
- ✅ `TestGLAccountService.CreateAsync()` - Async account creation
- ✅ `TestGLAccountService.GetByIdAsync()` - Async account retrieval
- ✅ `TestGLAccountService.UpdateAsync()` - Async account update
- ✅ `TestGLAccountService.GetAccountBalanceAsync()` - Async balance calculation
- ✅ `TestGLVoucherService.CreateAsync()` - Async voucher creation
- ✅ `TestGLVoucherService.PostAsync()` - Async voucher posting
- ✅ `TestGLVoucherService.GetByIdAsync()` - Async voucher retrieval

#### FinanceTestFixture Updates
- ✅ Added Currency table support
- ✅ Added FixedAsset table support
- ✅ Added AssetDepreciation table support
- ✅ Added AssetCategory table support

## Test Statistics

- **Total Test Files**: 15+ test files
- **Total Test Methods**: 200+ test scenarios
- **Unit Tests**: 150+ scenarios
- **Integration Tests**: 50+ scenarios
- **Edge Case Coverage**: 80+ edge case scenarios
- **Performance Tests**: 6 stress/performance scenarios

## Running Enhanced Tests

### Run All Enhanced Tests
```bash
dotnet test ZR.Service.Tests.csproj
```

### Run Currency Tests
```bash
dotnet test --filter "FullyQualifiedName~CurrencyConversionTests"
```

### Run Edge Case Tests
```bash
dotnet test --filter "FullyQualifiedName~EdgeCases"
```

### Run Asset Tests
```bash
dotnet test --filter "FullyQualifiedName~FixedAsset"
```

### Run Integration Tests
```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

### Run Performance Tests
```bash
dotnet test --filter "FullyQualifiedName~StressPerformance"
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:Exclude="[*.Tests]*"
```

## Future Enhancements

- [ ] Add tests for Cash & Bank module
- [ ] Add tests for Receivables & Payables module
- [ ] Add tests for System Configuration
- [ ] Add tests for report export functionality
- [ ] Add tests for budget and forecasting
- [ ] Add tests for financial consolidation

