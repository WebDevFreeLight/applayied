# Architecture Refactoring Plan - NormalBalance Implementation

## 1. Design Principles

### 1.1 Constraints
- **Minimal changes** - Only modify what's necessary
- **No heavy frameworks** - Use simple interfaces and dependency injection
- **Backward compatible** - Existing APIs should not break
- **Single responsibility** - Centralize normalBalance logic in one component
- **No hardcoded assumptions** - Never assume "assets are debit"

### 1.2 Goals
- All balance calculations respect account's `normalBalance`
- All financial reports produce correct signs
- Contra accounts work correctly
- Opening balances are calculated correctly

---

## 2. New Interface Definitions

### 2.1 INormalBalanceResolver
**Purpose:** Centralized service for all normalBalance-related calculations.

**Location:** `ZR.Service\IService\ERP\Finance\GL\INormalBalanceResolver.cs`

```csharp
namespace ZR.Service.IService.ERP.Finance.GL
{
    /// <summary>
    /// Centralized resolver for normalBalance-aware balance calculations
    /// All financial calculations MUST use this interface
    /// </summary>
    public interface INormalBalanceResolver
    {
        /// <summary>
        /// Calculate account balance respecting normalBalance direction
        /// </summary>
        /// <param name="totalDebit">Sum of all debits</param>
        /// <param name="totalCredit">Sum of all credits</param>
        /// <param name="normalBalance">Account's normal balance direction</param>
        /// <returns>Calculated balance (positive = normal direction)</returns>
        decimal CalculateBalance(decimal totalDebit, decimal totalCredit, NormalBalance normalBalance);

        /// <summary>
        /// Calculate account balance using account entity
        /// </summary>
        decimal CalculateBalance(decimal totalDebit, decimal totalCredit, GLAccount account);

        /// <summary>
        /// Determine if a balance is in the normal direction
        /// </summary>
        bool IsBalanceNormal(decimal balance, NormalBalance normalBalance);

        /// <summary>
        /// Get the debit/credit amounts needed to zero out an account
        /// </summary>
        (decimal Debit, decimal Credit) GetClosingEntry(decimal currentBalance, NormalBalance normalBalance);

        /// <summary>
        /// Get default normalBalance for an account type
        /// </summary>
        NormalBalance GetDefaultNormalBalance(AccountType accountType);

        /// <summary>
        /// Validate that normalBalance matches expected for AccountType
        /// </summary>
        bool ValidateNormalBalance(AccountType accountType, NormalBalance normalBalance);
    }
}
```

### 2.2 IAccountBalanceCalculator
**Purpose:** Higher-level interface for account balance operations.

**Location:** `ZR.Service\IService\ERP\Finance\GL\IAccountBalanceCalculator.cs`

```csharp
namespace ZR.Service.IService.ERP.Finance.GL
{
    /// <summary>
    /// High-level balance calculation service
    /// </summary>
    public interface IAccountBalanceCalculator
    {
        /// <summary>
        /// Get account balance for a date range with normalBalance applied
        /// </summary>
        Task<AccountBalanceDto> GetAccountBalanceAsync(
            Guid accountId,
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// Get multiple account balances efficiently
        /// </summary>
        Task<List<AccountBalanceDto>> GetAccountBalancesAsync(
            IEnumerable<Guid> accountIds,
            DateTime startDate,
            DateTime endDate);

        /// <summary>
        /// Get balance for a specific account type
        /// </summary>
        Task<decimal> GetTotalBalanceByTypeAsync(
            AccountType accountType,
            DateTime startDate,
            DateTime endDate,
            long? companyId = null);
    }

    public class AccountBalanceDto
    {
        public Guid AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public AccountType AccountType { get; set; }
        public NormalBalance NormalBalance { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Balance { get; set; }  // Already normalized
        public bool IsNormalDirection { get; set; }
    }
}
```

---

## 3. Implementation Classes

### 3.1 NormalBalanceResolver
**Location:** `ZR.Service\ERP\Finance\GL\NormalBalanceResolver.cs`

```csharp
namespace ZR.Service.ERP.Finance.GL
{
    [AppService(ServiceType = typeof(INormalBalanceResolver), ServiceLifetime = LifeTime.Singleton)]
    public class NormalBalanceResolver : INormalBalanceResolver
    {
        public decimal CalculateBalance(decimal totalDebit, decimal totalCredit, NormalBalance normalBalance)
        {
            return normalBalance == NormalBalance.Debit
                ? totalDebit - totalCredit
                : totalCredit - totalDebit;
        }

        public decimal CalculateBalance(decimal totalDebit, decimal totalCredit, GLAccount account)
        {
            return CalculateBalance(totalDebit, totalCredit, account.NormalBalance);
        }

        public bool IsBalanceNormal(decimal balance, NormalBalance normalBalance)
        {
            // A positive balance means the account is in its normal state
            return balance >= 0;
        }

        public (decimal Debit, decimal Credit) GetClosingEntry(decimal currentBalance, NormalBalance normalBalance)
        {
            // To close an account, we need to bring it to zero
            // If normalBalance is Debit and balance is positive: Credit to close
            // If normalBalance is Debit and balance is negative: Debit to close
            // If normalBalance is Credit and balance is positive: Debit to close
            // If normalBalance is Credit and balance is negative: Credit to close

            if (normalBalance == NormalBalance.Debit)
            {
                return currentBalance >= 0
                    ? (0, currentBalance)    // Credit to close positive debit-normal
                    : (Math.Abs(currentBalance), 0);  // Debit to close negative debit-normal
            }
            else
            {
                return currentBalance >= 0
                    ? (currentBalance, 0)    // Debit to close positive credit-normal
                    : (0, Math.Abs(currentBalance));  // Credit to close negative credit-normal
            }
        }

        public NormalBalance GetDefaultNormalBalance(AccountType accountType)
        {
            return accountType switch
            {
                AccountType.Asset => NormalBalance.Debit,
                AccountType.Expense => NormalBalance.Debit,
                AccountType.ContraLiability => NormalBalance.Debit,
                AccountType.ContraEquity => NormalBalance.Debit,
                AccountType.ContraRevenue => NormalBalance.Debit,
                AccountType.Liability => NormalBalance.Credit,
                AccountType.Equity => NormalBalance.Credit,
                AccountType.Revenue => NormalBalance.Credit,
                AccountType.ContraAsset => NormalBalance.Credit,
                AccountType.OtherComprehensiveIncome => NormalBalance.Credit,
                _ => NormalBalance.Debit
            };
        }

        public bool ValidateNormalBalance(AccountType accountType, NormalBalance normalBalance)
        {
            var expected = GetDefaultNormalBalance(accountType);
            return normalBalance == expected;
        }
    }
}
```

### 3.2 AccountBalanceCalculator
**Location:** `ZR.Service\ERP\Finance\GL\AccountBalanceCalculator.cs`

```csharp
namespace ZR.Service.ERP.Finance.GL
{
    [AppService(ServiceType = typeof(IAccountBalanceCalculator), ServiceLifetime = LifeTime.Transient)]
    public class AccountBalanceCalculator : IAccountBalanceCalculator
    {
        private readonly IGLAccountRepository _accountRepository;
        private readonly IGLJournalEntryRepository _entryRepository;
        private readonly INormalBalanceResolver _normalBalanceResolver;

        public AccountBalanceCalculator(
            IGLAccountRepository accountRepository,
            IGLJournalEntryRepository entryRepository,
            INormalBalanceResolver normalBalanceResolver)
        {
            _accountRepository = accountRepository;
            _entryRepository = entryRepository;
            _normalBalanceResolver = normalBalanceResolver;
        }

        public async Task<AccountBalanceDto> GetAccountBalanceAsync(
            Guid accountId,
            DateTime startDate,
            DateTime endDate)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) return null;

            var rawBalance = await _entryRepository.GetAccountBalanceAsync(accountId, startDate, endDate);

            var balance = _normalBalanceResolver.CalculateBalance(
                rawBalance.TotalDebit,
                rawBalance.TotalCredit,
                account.NormalBalance);

            return new AccountBalanceDto
            {
                AccountId = account.AccountId,
                AccountCode = account.FullAccountNumber,
                AccountName = account.AccountNameEn,
                AccountType = account.AccountType,
                NormalBalance = account.NormalBalance,
                TotalDebit = rawBalance.TotalDebit,
                TotalCredit = rawBalance.TotalCredit,
                Balance = balance,
                IsNormalDirection = _normalBalanceResolver.IsBalanceNormal(balance, account.NormalBalance)
            };
        }

        // ... other implementations
    }
}
```

---

## 4. Dependency Injection Points

### 4.1 Services to Inject INormalBalanceResolver

| Service | Constructor Change |
|---------|-------------------|
| GLFinancialReportService | Add `INormalBalanceResolver _normalBalanceResolver` |
| GLAccountOpeningBalanceService | Add `INormalBalanceResolver _normalBalanceResolver` |
| YearEndClosingService | Add `INormalBalanceResolver _normalBalanceResolver` |
| BudgetService | Add `INormalBalanceResolver _normalBalanceResolver` |
| GLAccountService | Add `INormalBalanceResolver _normalBalanceResolver` |

### 4.2 Service Registration
**Location:** `Program.cs` or service registration

```csharp
// Already handled by [AppService] attribute
// services.AddSingleton<INormalBalanceResolver, NormalBalanceResolver>();
// services.AddTransient<IAccountBalanceCalculator, AccountBalanceCalculator>();
```

---

## 5. Service Modifications

### 5.1 GLFinancialReportService

**Current State:**
```csharp
Balance = balance.TotalDebit - balance.TotalCredit
```

**Refactored State:**
```csharp
Balance = _normalBalanceResolver.CalculateBalance(
    balance.TotalDebit,
    balance.TotalCredit,
    account.NormalBalance)
```

**Changes Required:**
1. Add constructor parameter: `INormalBalanceResolver normalBalanceResolver`
2. Store as private field: `private readonly INormalBalanceResolver _normalBalanceResolver`
3. Replace all hardcoded balance calculations with resolver calls
4. Remove `Math.Abs()` workarounds

### 5.2 GLAccountOpeningBalanceService

**Current State:**
```csharp
entity.OpeningBalance = dto.OpeningDebit - dto.OpeningCredit;
```

**Refactored State:**
```csharp
var account = await _accountRepository.GetByIdAsync(dto.AccountId);
entity.OpeningBalance = _normalBalanceResolver.CalculateBalance(
    dto.OpeningDebit,
    dto.OpeningCredit,
    account.NormalBalance);
```

### 5.3 YearEndClosingService

**Current State (Partial):**
```csharp
Debit = account.Balance > 0 ? account.Balance : 0,
Credit = account.Balance < 0 ? Math.Abs(account.Balance) : 0,
```

**Refactored State:**
```csharp
var (debit, credit) = _normalBalanceResolver.GetClosingEntry(
    account.Balance,
    account.NormalBalance);
```

---

## 6. Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER                          │
│  (Controllers, API Endpoints)                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      SERVICE LAYER                              │
│                                                                 │
│  ┌─────────────────────┐    ┌─────────────────────┐           │
│  │ GLFinancialReport   │    │ YearEndClosing      │           │
│  │ Service             │    │ Service             │           │
│  └─────────┬───────────┘    └─────────┬───────────┘           │
│            │                          │                        │
│            │    ┌────────────────────┐│                        │
│            └───►│ INormalBalance     ││◄───────────────────────┘
│                 │ Resolver           ││                        │
│  ┌─────────────►│                    │◄──────────────────┐     │
│  │              └────────────────────┘                   │     │
│  │                                                       │     │
│  │  ┌─────────────────────┐    ┌─────────────────────┐  │     │
│  │  │ GLAccountOpening    │    │ BudgetService       │  │     │
│  │  │ BalanceService      │    │                     │──┘     │
│  │  └─────────────────────┘    └─────────────────────┘        │
│  │                                                             │
│  │  ┌─────────────────────┐    ┌─────────────────────┐        │
│  │  │ GLAccountService    │    │ AgingService        │        │
│  │  │                     │────│ (No changes needed) │        │
│  │  └─────────────────────┘    └─────────────────────┘        │
│  │                                                             │
└──┼─────────────────────────────────────────────────────────────┘
   │
   ▼
┌─────────────────────────────────────────────────────────────────┐
│                    REPOSITORY LAYER                             │
│                                                                 │
│  ┌─────────────────────┐    ┌─────────────────────┐           │
│  │ GLAccountRepository │    │ GLJournalEntry      │           │
│  │                     │    │ Repository          │           │
│  └─────────────────────┘    └─────────────────────┘           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      DOMAIN LAYER                               │
│                                                                 │
│  ┌─────────────────────┐    ┌─────────────────────┐           │
│  │ GLAccount           │    │ NormalBalance       │           │
│  │ (Model)             │    │ (Enum)              │           │
│  └─────────────────────┘    └─────────────────────┘           │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 7. Migration Strategy

### 7.1 Phase 1: Add New Components (No Breaking Changes)
1. Create `INormalBalanceResolver` interface
2. Create `NormalBalanceResolver` implementation
3. Create `IAccountBalanceCalculator` interface
4. Create `AccountBalanceCalculator` implementation
5. Unit test all new components

### 7.2 Phase 2: Inject Dependencies
1. Add constructor parameters to affected services
2. Store as private readonly fields
3. No logic changes yet - just injection

### 7.3 Phase 3: Refactor Balance Calculations
1. Replace hardcoded formulas one service at a time
2. Remove `Math.Abs()` workarounds
3. Test each service after changes

### 7.4 Phase 4: Validation & Testing
1. Run full regression suite
2. Test with Contra accounts
3. Verify financial reports
4. Validate opening balances

---

## 8. Rollback Plan

If issues are discovered:

1. **Immediate:** Revert to previous calculation method using feature flag
2. **Short-term:** Fix identified issues
3. **Long-term:** Complete migration

**Feature Flag Implementation:**
```csharp
public decimal CalculateBalance(...)
{
    if (_featureFlags.UseNormalBalanceResolver)
    {
        return _normalBalanceResolver.CalculateBalance(...);
    }
    else
    {
        // Legacy: hardcoded formula
        return totalDebit - totalCredit;
    }
}
```

---

## 9. Summary

### Files to Create (New)
| File | Type |
|------|------|
| `INormalBalanceResolver.cs` | Interface |
| `NormalBalanceResolver.cs` | Implementation |
| `IAccountBalanceCalculator.cs` | Interface |
| `AccountBalanceCalculator.cs` | Implementation |

### Files to Modify
| File | Change Type |
|------|-------------|
| `GLFinancialReportService.cs` | Inject resolver, refactor calculations |
| `GLAccountOpeningBalanceService.cs` | Inject resolver, refactor calculations |
| `YearEndClosingService.cs` | Inject resolver, use for closing entries |
| `BudgetService.cs` | Inject resolver, fix variance calculation |
| `GLAccountService.cs` | Inject resolver, use in balance methods |

### Estimated Impact
- **New Files:** 4
- **Modified Files:** 5
- **Lines Changed:** ~200
- **Risk Level:** Low (additive changes, centralized logic)

---

**Plan Date:** 2026-01-08
**Architect:** Claude Code - Finance Systems Architect
