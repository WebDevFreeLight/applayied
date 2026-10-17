# Updated Code Samples - NormalBalance Implementation

## 1. New Interface: INormalBalanceResolver

**File:** `ZR.Service\IService\ERP\Finance\GL\INormalBalanceResolver.cs`

```csharp
using ZR.Model.ERP.Finance.Enums;
using ZR.Model.ERP.Finance.GL;

namespace ZR.Service.IService.ERP.Finance.GL
{
    /// <summary>
    /// Centralized resolver for normalBalance-aware balance calculations.
    /// All financial calculations MUST use this interface.
    /// جميع الحسابات المالية يجب أن تستخدم هذه الواجهة
    /// </summary>
    public interface INormalBalanceResolver
    {
        /// <summary>
        /// Calculate account balance respecting normalBalance direction.
        /// حساب رصيد الحساب مع احترام اتجاه الرصيد الطبيعي
        /// </summary>
        /// <param name="totalDebit">Sum of all debits - مجموع المدين</param>
        /// <param name="totalCredit">Sum of all credits - مجموع الدائن</param>
        /// <param name="normalBalance">Account's normal balance direction - اتجاه الرصيد الطبيعي</param>
        /// <returns>Calculated balance (positive = normal direction)</returns>
        decimal CalculateBalance(decimal totalDebit, decimal totalCredit, NormalBalance normalBalance);

        /// <summary>
        /// Calculate account balance using account entity.
        /// </summary>
        decimal CalculateBalance(decimal totalDebit, decimal totalCredit, GLAccount account);

        /// <summary>
        /// Determine if a balance is in the normal direction (positive).
        /// </summary>
        bool IsBalanceNormal(decimal balance, NormalBalance normalBalance);

        /// <summary>
        /// Get the debit/credit amounts needed to zero out an account.
        /// الحصول على مبالغ المدين/الدائن اللازمة لإقفال الحساب
        /// </summary>
        (decimal Debit, decimal Credit) GetClosingEntry(decimal currentBalance, NormalBalance normalBalance);

        /// <summary>
        /// Get default normalBalance for an account type.
        /// </summary>
        NormalBalance GetDefaultNormalBalance(AccountType accountType);

        /// <summary>
        /// Validate that normalBalance matches expected for AccountType.
        /// Returns false for non-standard configurations (e.g., ContraAsset).
        /// </summary>
        bool ValidateNormalBalance(AccountType accountType, NormalBalance normalBalance);
    }
}
```

---

## 2. New Implementation: NormalBalanceResolver

**File:** `ZR.Service\ERP\Finance\GL\NormalBalanceResolver.cs`

```csharp
using Infrastructure.Attribute;
using ZR.Model.ERP.Finance.Enums;
using ZR.Model.ERP.Finance.GL;
using ZR.Service.IService.ERP.Finance.GL;

namespace ZR.Service.ERP.Finance.GL
{
    /// <summary>
    /// Implementation of INormalBalanceResolver.
    /// Singleton lifetime for performance - no state, pure calculations.
    /// </summary>
    [AppService(ServiceType = typeof(INormalBalanceResolver), ServiceLifetime = LifeTime.Singleton)]
    public class NormalBalanceResolver : INormalBalanceResolver
    {
        /// <inheritdoc />
        public decimal CalculateBalance(decimal totalDebit, decimal totalCredit, NormalBalance normalBalance)
        {
            // Debit-normal accounts: positive when Debit > Credit
            // Credit-normal accounts: positive when Credit > Debit
            return normalBalance == NormalBalance.Debit
                ? totalDebit - totalCredit
                : totalCredit - totalDebit;
        }

        /// <inheritdoc />
        public decimal CalculateBalance(decimal totalDebit, decimal totalCredit, GLAccount account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            return CalculateBalance(totalDebit, totalCredit, account.NormalBalance);
        }

        /// <inheritdoc />
        public bool IsBalanceNormal(decimal balance, NormalBalance normalBalance)
        {
            // A positive balance indicates the account is in its normal state
            // A negative balance indicates abnormal state (e.g., overdrawn bank account)
            return balance >= 0;
        }

        /// <inheritdoc />
        public (decimal Debit, decimal Credit) GetClosingEntry(decimal currentBalance, NormalBalance normalBalance)
        {
            // To close an account, we need to bring its balance to zero.
            // The closing entry is the opposite of the current balance.

            if (currentBalance == 0)
                return (0, 0);

            if (normalBalance == NormalBalance.Debit)
            {
                // Debit-normal account with positive balance: Credit to close
                // Debit-normal account with negative balance: Debit to close
                return currentBalance > 0
                    ? (0, currentBalance)
                    : (Math.Abs(currentBalance), 0);
            }
            else
            {
                // Credit-normal account with positive balance: Debit to close
                // Credit-normal account with negative balance: Credit to close
                return currentBalance > 0
                    ? (currentBalance, 0)
                    : (0, Math.Abs(currentBalance));
            }
        }

        /// <inheritdoc />
        public NormalBalance GetDefaultNormalBalance(AccountType accountType)
        {
            return accountType switch
            {
                // Debit-normal accounts (increase with debits)
                AccountType.Asset => NormalBalance.Debit,
                AccountType.Expense => NormalBalance.Debit,
                AccountType.ContraLiability => NormalBalance.Debit,
                AccountType.ContraEquity => NormalBalance.Debit,
                AccountType.ContraRevenue => NormalBalance.Debit,

                // Credit-normal accounts (increase with credits)
                AccountType.Liability => NormalBalance.Credit,
                AccountType.Equity => NormalBalance.Credit,
                AccountType.Revenue => NormalBalance.Credit,
                AccountType.ContraAsset => NormalBalance.Credit,
                AccountType.OtherComprehensiveIncome => NormalBalance.Credit,

                // Default to Debit for unknown types
                _ => NormalBalance.Debit
            };
        }

        /// <inheritdoc />
        public bool ValidateNormalBalance(AccountType accountType, NormalBalance normalBalance)
        {
            var expected = GetDefaultNormalBalance(accountType);
            return normalBalance == expected;
        }
    }
}
```

---

## 3. Patch: GLFinancialReportService Constructor

**File:** `ZR.Service\ERP\Finance\GL\GLFinancialReportService.cs`

**Add field:**
```csharp
private readonly INormalBalanceResolver _normalBalanceResolver;
```

**Modify constructor:**
```csharp
public GLFinancialReportService(
    IGLFinancialReportRepository reportRepository,
    IGLAccountRepository accountRepository,
    IGLJournalEntryRepository entryRepository,
    IFiscalYearService fiscalYearService,
    INormalBalanceResolver normalBalanceResolver)  // <-- ADD THIS
{
    _reportRepository = reportRepository;
    _accountRepository = accountRepository;
    _entryRepository = entryRepository;
    _fiscalYearService = fiscalYearService;
    _normalBalanceResolver = normalBalanceResolver;  // <-- ADD THIS
}
```

---

## 4. Patch: GetTrialBalanceData (Line 368)

**File:** `ZR.Service\ERP\Finance\GL\GLFinancialReportService.cs`

**Before:**
```csharp
lines.Add(new TrialBalanceLineDto
{
    AccountId = account.AccountId,
    AccountCode = account.AccountNumber,
    AccountNameAr = account.AccountNameAr,
    AccountNameEn = account.AccountNameEn,
    AccountType = account.AccountType.ToString(),
    Debit = balance.TotalDebit,
    Credit = balance.TotalCredit,
    Balance = balance.TotalDebit - balance.TotalCredit  // <-- VIOLATION
});
```

**After:**
```csharp
lines.Add(new TrialBalanceLineDto
{
    AccountId = account.AccountId,
    AccountCode = account.AccountNumber,
    AccountNameAr = account.AccountNameAr,
    AccountNameEn = account.AccountNameEn,
    AccountType = account.AccountType.ToString(),
    Debit = balance.TotalDebit,
    Credit = balance.TotalCredit,
    Balance = _normalBalanceResolver.CalculateBalance(  // <-- FIXED
        balance.TotalDebit,
        balance.TotalCredit,
        account.NormalBalance)
});
```

---

## 5. Patch: GetBalanceSheetData (Lines 464-476)

**File:** `ZR.Service\ERP\Finance\GL\GLFinancialReportService.cs`

**Before:**
```csharp
var balance = _entryRepository.GetAccountBalance(account.AccountId, balanceStartDate, asOfDate);
var netBalance = balance.TotalDebit - balance.TotalCredit;  // <-- VIOLATION

if (netBalance == 0) continue;

var section = new BalanceSheetSectionDto
{
    AccountId = account.AccountId,
    AccountCode = account.AccountNumber,
    AccountNameAr = account.AccountNameAr,
    AccountNameEn = account.AccountNameEn,
    Level = account.Level,
    Balance = Math.Abs(netBalance)  // <-- VIOLATION: Math.Abs hides errors
};
```

**After:**
```csharp
var balance = _entryRepository.GetAccountBalance(account.AccountId, balanceStartDate, asOfDate);
var netBalance = _normalBalanceResolver.CalculateBalance(  // <-- FIXED
    balance.TotalDebit,
    balance.TotalCredit,
    account.NormalBalance);

if (netBalance == 0) continue;

var section = new BalanceSheetSectionDto
{
    AccountId = account.AccountId,
    AccountCode = account.AccountNumber,
    AccountNameAr = account.AccountNameAr,
    AccountNameEn = account.AccountNameEn,
    Level = account.Level,
    Balance = netBalance  // <-- FIXED: No Math.Abs - correct sign preserved
};
```

---

## 6. Patch: GetIncomeStatementByPeriodData (Lines 1716-1726)

**File:** `ZR.Service\ERP\Finance\GL\GLFinancialReportService.cs`

**Before:**
```csharp
if (account.AccountType == AccountType.Revenue)
{
    // Revenue: Credit - Debit (positive = revenue)
    accountLine.Amount = totalCredit - totalDebit;  // <-- HARDCODED
    item.RevenueAccounts.Add(accountLine);
    item.TotalRevenue += accountLine.Amount;
}
else if (account.AccountType == AccountType.Expense)
{
    // Expense: Debit - Credit (positive = expense)
    accountLine.Amount = totalDebit - totalCredit;  // <-- HARDCODED
    item.ExpenseAccounts.Add(accountLine);
    item.TotalExpenses += accountLine.Amount;
}
```

**After:**
```csharp
// Use normalBalance instead of AccountType assumption
accountLine.Amount = _normalBalanceResolver.CalculateBalance(
    totalDebit,
    totalCredit,
    account.NormalBalance);

if (account.AccountType == AccountType.Revenue ||
    account.AccountType == AccountType.ContraRevenue)
{
    item.RevenueAccounts.Add(accountLine);
    // ContraRevenue will have negative amount, correctly reducing revenue
    item.TotalRevenue += accountLine.Amount;
}
else if (account.AccountType == AccountType.Expense)
{
    item.ExpenseAccounts.Add(accountLine);
    item.TotalExpenses += accountLine.Amount;
}
```

---

## 7. Patch: GLAccountOpeningBalanceService Constructor

**File:** `ZR.Service\ERP\Finance\GL\GLAccountOpeningBalanceService.cs`

**Add field:**
```csharp
private readonly INormalBalanceResolver _normalBalanceResolver;
private readonly IGLAccountRepository _accountRepository;
```

**Modify constructor:**
```csharp
public GLAccountOpeningBalanceService(
    IGLAccountOpeningBalanceRepository repository,
    IGLAccountRepository accountRepository,  // <-- ADD THIS
    INormalBalanceResolver normalBalanceResolver)  // <-- ADD THIS
{
    _repository = repository;
    _accountRepository = accountRepository;  // <-- ADD THIS
    _normalBalanceResolver = normalBalanceResolver;  // <-- ADD THIS
}
```

---

## 8. Patch: GLAccountOpeningBalanceService CreateAsync (Line 192)

**File:** `ZR.Service\ERP\Finance\GL\GLAccountOpeningBalanceService.cs`

**Before:**
```csharp
var entity = dto.Adapt<GLAccountOpeningBalance>();
entity.Id = Guid.NewGuid();
entity.OpeningBalance = dto.OpeningDebit - dto.OpeningCredit;  // <-- VIOLATION
```

**After:**
```csharp
var entity = dto.Adapt<GLAccountOpeningBalance>();
entity.Id = Guid.NewGuid();

// Get account to determine normalBalance
var account = await _accountRepository.GetByIdAsync(dto.AccountId);
if (account == null)
    throw new InvalidOperationException($"Account {dto.AccountId} not found");

entity.OpeningBalance = _normalBalanceResolver.CalculateBalance(  // <-- FIXED
    dto.OpeningDebit,
    dto.OpeningCredit,
    account.NormalBalance);
```

---

## 9. Patch: YearEndClosingService Constructor

**File:** `ZR.Service\ERP\Finance\GL\YearEndClosingService.cs`

**Add field:**
```csharp
private readonly INormalBalanceResolver _normalBalanceResolver;
```

**Modify constructor:**
```csharp
public YearEndClosingService(
    IGLJournalService journalService,
    IGLPeriodClosingService periodClosingService,
    IFiscalYearHelper fiscalYearHelper,
    INormalBalanceResolver normalBalanceResolver)  // <-- ADD THIS
{
    _journalService = journalService;
    _periodClosingService = periodClosingService;
    _fiscalYearHelper = fiscalYearHelper;
    _normalBalanceResolver = normalBalanceResolver;  // <-- ADD THIS
}
```

---

## 10. Patch: YearEndClosingService Closing Entries (Lines 117-140)

**File:** `ZR.Service\ERP\Finance\GL\YearEndClosingService.cs`

**Before:**
```csharp
// Close Revenue accounts (debit revenue to zero it out)
foreach (var account in revenueAccounts.Where(a => a.Balance != 0))
{
    journalEntries.Add(new CreateJournalEntryDto
    {
        LineNumber = lineNumber++,
        AccountId = account.AccountId,
        Debit = account.Balance > 0 ? account.Balance : 0,  // <-- HARDCODED
        Credit = account.Balance < 0 ? Math.Abs(account.Balance) : 0,  // <-- HARDCODED
        DescriptionEn = $"Year-End Closing - Close Revenue {account.AccountCode}",
        DescriptionAr = $"قيد إقفال نهاية السنة - إقفال إيراد {account.AccountCode}"
    });
}
```

**After:**
```csharp
// Close Revenue accounts (debit revenue to zero it out)
foreach (var account in revenueAccounts.Where(a => a.Balance != 0))
{
    var (debit, credit) = _normalBalanceResolver.GetClosingEntry(  // <-- FIXED
        account.Balance,
        account.NormalBalance);

    journalEntries.Add(new CreateJournalEntryDto
    {
        LineNumber = lineNumber++,
        AccountId = account.AccountId,
        Debit = debit,
        Credit = credit,
        DescriptionEn = $"Year-End Closing - Close Revenue {account.AccountCode}",
        DescriptionAr = $"قيد إقفال نهاية السنة - إقفال إيراد {account.AccountCode}"
    });
}
```

---

## 11. Patch: BudgetService Constructor

**File:** `ZR.Service\ERP\Finance\Budget\BudgetService.cs`

**Add field:**
```csharp
private readonly INormalBalanceResolver _normalBalanceResolver;
```

**Add to constructor parameters and assignment.**

---

## 12. Patch: BudgetService Variance Calculation (Line 552)

**File:** `ZR.Service\ERP\Finance\Budget\BudgetService.cs`

**Before:**
```csharp
var actualAmount = Math.Abs(balance.Balance);  // <-- VIOLATION
```

**After:**
```csharp
// Get account to use normalBalance
var account = await _accountRepository.GetByIdAsync(line.AccountId);
var actualAmount = _normalBalanceResolver.CalculateBalance(
    balance.TotalDebit,
    balance.TotalCredit,
    account?.NormalBalance ?? NormalBalance.Debit);
```

---

## 13. DTO Enhancement: AccountBalanceDto

**File:** `ZR.Model\ERP\Finance\GL\Dto\GLAccountDto.cs` (or new file)

```csharp
/// <summary>
/// Account balance DTO with normalBalance-aware calculation
/// </summary>
public class AccountBalanceDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountNameEn { get; set; }
    public string AccountNameAr { get; set; }
    public AccountType AccountType { get; set; }
    public NormalBalance NormalBalance { get; set; }

    /// <summary>
    /// Raw debit total from journal entries
    /// </summary>
    public decimal TotalDebit { get; set; }

    /// <summary>
    /// Raw credit total from journal entries
    /// </summary>
    public decimal TotalCredit { get; set; }

    /// <summary>
    /// Normalized balance (calculated using normalBalance direction)
    /// Positive = normal direction, Negative = abnormal direction
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// True if balance is in normal direction (positive)
    /// </summary>
    public bool IsNormalDirection { get; set; }
}
```

---

## Summary of Files

### New Files (4)
| File | Purpose |
|------|---------|
| `INormalBalanceResolver.cs` | Interface definition |
| `NormalBalanceResolver.cs` | Implementation |
| `IAccountBalanceCalculator.cs` | High-level interface |
| `AccountBalanceCalculator.cs` | High-level implementation |

### Modified Files (5)
| File | Changes |
|------|---------|
| `GLFinancialReportService.cs` | Constructor + 14 calculation fixes |
| `GLAccountOpeningBalanceService.cs` | Constructor + 2 calculation fixes |
| `YearEndClosingService.cs` | Constructor + closing entry fixes |
| `BudgetService.cs` | Constructor + variance calculation fix |
| `GLAccountService.cs` | Constructor + optional balance summary fixes |

---

**Document Date:** 2026-01-08
**Author:** Claude Code - Finance Systems Architect
