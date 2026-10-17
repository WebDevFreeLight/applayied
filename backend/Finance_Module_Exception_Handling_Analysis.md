# Finance Module Exception Handling Analysis

**System:** ZR.Admin.NET ERP Platform
**Module:** Finance (ARAP, Budget, Treasury, PaymentHub, Assets, Zatca)
**Analysis Date:** December 15, 2025
**Framework:** .NET 8.0 with SqlSugar ORM

---

## 1. Overview

The Finance Module implements a **multi-layered exception handling strategy** combining:

- **Controller-level** `try-catch` blocks for operation endpoints
- **Global middleware** (`GlobalExceptionMiddleware`) for unhandled exceptions
- **Transaction-based** rollback patterns in service layer
- **Limited custom exceptions** for period and treasury operations

**Current State:** The module exhibits **inconsistent exception handling patterns** across submodules. While transaction rollback is generally implemented, exception handling lacks financial system best practices such as structured error codes, comprehensive validation, and proper security controls for sensitive data exposure.

---

## 2. Exception Types Used

### 2.1 Built-in .NET Exceptions

**Primary Usage:**
- `System.Exception` - Generic exceptions thrown throughout services
- `ArgumentException` - Used in GlobalExceptionMiddleware for parameter validation
- `ArgumentNullException` - Caught by GlobalExceptionFilter
- `KeyNotFoundException` - Caught by GlobalExceptionFilter
- `InvalidOperationException` - Caught by GlobalExceptionFilter

### 2.2 Custom Finance-Specific Exceptions

#### Period-Related Exceptions
**Location:** `ZR.Model.ERP.Finance.Exceptions`

| Exception | Error Code | Purpose | Bilingual |
|-----------|-----------|---------|-----------|
| `BasePeriodException` | Abstract | Base class for fiscal period errors | ✓ |
| `PeriodClosedException` | `PERIOD_CLOSED` | Fiscal period is closed | ✓ |
| `PeriodLockedException` | `PERIOD_LOCKED` | Fiscal period is locked | ✓ |
| `PeriodNotFoundException` | `PERIOD_NOT_FOUND` | Fiscal period does not exist | ✓ |

**Example:**
```csharp
public class PeriodClosedException : BasePeriodException
{
    public override string ErrorCode => "PERIOD_CLOSED";
    public DateTime Date { get; protected set; }
    public string MessageAr { get; protected set; }
    public string MessageEn { get; protected set; }
}
```

#### Treasury Module Exception
**Location:** `ZR.Model.ERP.Finance.Treasury.TreasuryException`

**Features:**
- Structured error codes (`TreasuryErrorCode` enum with 40+ codes)
- Bilingual error messages (Arabic and English)
- Contextual metadata (EntityId, EntityType)
- Converts to structured `TreasuryErrorResponse` for API responses

**Error Categories:**
- Validation Errors (InvalidAmount, InvalidCurrency, InvalidDate, etc.)
- Business Rule Errors (InsufficientBalance, AccountClosed, PaymentAlreadyPosted, etc.)
- GL Integration Errors (GLPostingFailed, JournalNotFound, etc.)
- Data Access Errors (RecordNotFound, ConcurrencyConflict, etc.)
- External Service Errors (BankAPIError, WebhookDeliveryFailed, etc.)

**Example:**
```csharp
throw new TreasuryException(
    TreasuryErrorCode.InsufficientBalance,
    $"Insufficient balance: required {amount}, available {balance}",
    $"رصيد غير كافٍ: مطلوب {amount}، متاح {balance}",
    accountId,
    TreasuryEntityType.BankAccount
);
```

### 2.3 Custom Exception Usage Analysis

**Gap Identified:** Despite the existence of well-designed custom exceptions (`TreasuryException`, period exceptions), **most Finance services throw generic `System.Exception`** with hardcoded messages.

**Files Using Generic Exception:**
- `ARInvoiceService.cs` (lines 112, 211, 514, 517, 746)
- `BudgetService.cs` (lines 105, 150, 156, 162, 219, 242, 248, 269, 274, 424, 430, 464, 470, 476, 496, 502, 520, 569, 643)
- `PaymentHubService.cs` - Uses result objects but still throws generic exceptions in transactions

---

## 3. Handling Strategy

### 3.1 Controller Layer (API Endpoints)

**Pattern:** Explicit `try-catch` blocks wrapping service calls

**Files Analyzed:**
- `ARInvoiceController.cs`
- `APInvoiceController.cs`
- `BudgetController.cs`
- `PaymentHubController.cs`
- `AssetTransactionController.cs`

**Implementation Example (ARInvoiceController.cs:60-68):**
```csharp
try
{
    var result = _invoiceService.Create(dto, HttpContext.GetName());
    return SUCCESS(result);
}
catch (Exception ex)
{
    return ToResponse(ResultCode.FAIL, ex.Message);
}
```

**Characteristics:**
- ✓ **Consistent:** All Create/Update/Delete/Post/Cancel operations use try-catch
- ✓ **Returns API result:** Uses `ToResponse(ResultCode.FAIL, ex.Message)`
- ✗ **Generic catch:** Catches all `Exception` types without differentiation
- ✗ **Exposes raw messages:** Returns `ex.Message` directly to client
- ✗ **No contextual logging:** Does not log before returning error

**PaymentHubController Pattern (more sophisticated):**
```csharp
if (result.Success)
{
    return SUCCESS(result);
}
return ToResponse(GetStatusCode(result.ErrorCode), result.ErrorMessage);
```

Uses structured result objects with error codes:
- `PaymentOperationResult` with `ErrorCode` (NOT_FOUND, VALIDATION_FAILED, INVALID_STATUS, etc.)
- `PostPaymentResult` with GL journal details
- HTTP status codes mapped from error codes (404, 400, 500)

### 3.2 Service Layer

**Pattern:** Transaction-based exception handling with rollback

**Transaction Handling (ARInvoiceService.cs:187-198):**
```csharp
Context.Ado.BeginTran();
try
{
    _invoiceRepository.Insert(invoice);
    _lineRepository.InsertRange(lineEntities);
    Context.Ado.CommitTran();
}
catch (Exception)
{
    Context.Ado.RollbackTran();
    throw; // Re-throws to controller
}
```

**Characteristics:**
- ✓ **Transaction safety:** BEGIN TRAN / COMMIT / ROLLBACK pattern
- ✓ **Atomic operations:** Multi-table inserts/updates in single transaction
- ✓ **Re-throw:** Preserves original exception for global handler
- ✗ **No logging before rollback:** Missing audit trail for failed transactions
- ✗ **Generic exception catch:** No differentiation between types
- ✗ **No cleanup actions:** No resource cleanup in catch block

**PaymentHubService Advanced Pattern (lines 404-528):**
```csharp
Context.Ado.BeginTran();
try
{
    // 1. Validate
    var validation = ValidateForPosting(dto.PaymentId);

    // 2. Create balance ledger entry (removed in GL refactoring)

    // 3. Create GL Journal Entry - MANDATORY
    var glJournal = _glIntegrationService.CreatePaymentJournal(payment, userId);
    if (glJournal == null || glJournal.Id == Guid.Empty)
    {
        Context.Ado.RollbackTran();
        result.ErrorCode = "GL_JOURNAL_FAILED";
        result.ErrorMessage = "Failed to create GL journal entry";
        result.ErrorMessageAr = "فشل في إنشاء قيد اليومية";
        return result;
    }

    // 4. Update payment status
    payment.Status = PaymentStatus.Posted;
    _paymentRepository.Update(payment);

    // 5. Audit event
    _eventRepository.AppendEvent(evt);

    Context.Ado.CommitTran();
    result.Success = true;
}
catch (Exception ex)
{
    Context.Ado.RollbackTran();
    result.ErrorCode = "POST_FAILED";
    result.ErrorMessage = ex.Message;
    result.ErrorMessageAr = "فشل في ترحيل الدفعة";
}
```

**Advanced Features:**
- Pre-validation before transaction
- Explicit error handling for critical steps (GL journal creation)
- Structured result objects with bilingual messages
- Audit event logging within transaction

### 3.3 Global Exception Handling

**Primary Middleware:** `GlobalExceptionMiddleware.cs`

**Registration:** `app.UseMiddleware<GlobalExceptionMiddleware>()`

**Process Flow:**
1. **Catch:** All unhandled exceptions in HTTP pipeline
2. **Categorize:** CustomException, ArgumentException, or generic Exception
3. **Log:** NLog with severity level, stack trace, request context
4. **Persist:** Insert to `SysOperLog` database table
5. **Notify:** Send WeChat notification for critical errors
6. **Respond:** Return standardized `ApiResult` JSON

**Implementation (GlobalExceptionMiddleware.cs:44-54):**
```csharp
public async Task Invoke(HttpContext context)
{
    try
    {
        await next(context);
    }
    catch (Exception ex)
    {
        await HandleExceptionAsync(context, ex);
    }
}
```

**Exception Type Handling (lines 64-88):**
```csharp
if (ex is CustomException customException)
{
    code = customException.Code;
    msg = customException.Message;
    error = customException.LogMsg;
    notice = customException.Notice;
}
else if (ex is ArgumentException)
{
    code = (int)ResultCode.PARAM_ERROR;
    msg = ex.Message;
}
else
{
    msg = "The server seems to have encountered an issue...";
    error = $"Exception reason: {ex.Message}";
    logLevel = LogLevel.Error;
    context.Response.StatusCode = 500;
}
```

**Logging Context (lines 101-136):**
```csharp
SysOperLog sysOperLog = new()
{
    Status = 1,
    OperIp = ip,
    OperUrl = HttpContextExtension.GetRequestUrl(context),
    RequestMethod = context.Request.Method,
    JsonResult = responseResult,
    ErrorMsg = error,
    OperName = HttpContextExtension.GetName(context),
    OperLocation = ip_info.Province + " " + ip_info.City,
    OperTime = DateTime.Now,
    OperParam = HttpContextExtension.GetRequestValue(context, context.Request.Method)
};

LogEventInfo ei = new(logLevel, "GlobalExceptionMiddleware", error)
{
    Exception = ex,
    Message = error
};
ei.Properties["status"] = 1;
ei.Properties["jsonResult"] = responseResult;
ei.Properties["requestParam"] = sysOperLog.OperParam;
ei.Properties["user"] = sysOperLog.OperName;

Logger.Log(ei);
```

**Alternative:** `GlobalExceptionFilter.cs` (IExceptionFilter)

Provides similar functionality as MVC filter:
- Maps exception types to HTTP status codes
- Environment-aware responses (dev vs production)
- Stack trace in development only

---

## 4. Logging Practices

### 4.1 Global Level Logging

**Logger:** NLog configured in `GlobalExceptionMiddleware`

**Logged Information:**
- ✓ Exception type and message
- ✓ Stack trace (development mode)
- ✓ HTTP request details (URL, method, IP, location)
- ✓ User identity (`OperName`)
- ✓ Request parameters
- ✓ Response JSON
- ✓ Timestamp
- ✗ **Missing:** Transaction ID, correlation ID
- ✗ **Missing:** Financial context (invoice number, payment amount, account ID)

**Log Targets:**
1. **NLog file/database** (configured externally)
2. **SysOperLog table** (operation audit log)
3. **WeChat notification** (for critical errors)

**Example Log Entry:**
```
> Operator: admin
> Operation Location: 192.168.1.100 (Riyadh City)
> Operation Module: AR Invoice
> Operation URL: /finance/arap/ar-invoices
> Error Message: Invoice not found - الفاتورة غير موجودة
> Exception reason: Invoice not found
```

### 4.2 Service Level Logging

**Current State:** **NO explicit logging in Finance services**

**Gap:** Services do not log before throwing exceptions:
- No log entry before `throw new Exception("Budget Code already exists")`
- No log entry before transaction rollback
- No audit trail for business rule violations

**Exception:** PaymentHubService uses audit events (PaymentEvent) but only for successful operations, not failures.

### 4.3 Context Logging Quality

| Context Data | Global Middleware | Service Layer | Finance-Specific |
|--------------|-------------------|---------------|------------------|
| User ID | ✓ | ✗ | ✗ |
| IP Address | ✓ | ✗ | ✗ |
| HTTP Method | ✓ | ✗ | ✗ |
| Request URL | ✓ | ✗ | ✗ |
| Stack Trace | ✓ (dev) | ✗ | ✗ |
| **Invoice Number** | ✗ | ✗ | ✗ |
| **Transaction ID** | ✗ | ✗ | ✗ |
| **Payment Amount** | ✗ | ✗ | ✗ |
| **Account Balance** | ✗ | ✗ | ✗ |
| **GL Journal ID** | ✗ | ✗ | ✗ |

---

## 5. User Feedback

### 5.1 Error Message Exposure

**Critical Security Issue:** Raw exception messages are **directly exposed to end users**

**Examples:**

**Controller (ARInvoiceController.cs:67):**
```csharp
catch (Exception ex)
{
    return ToResponse(ResultCode.FAIL, ex.Message);
}
```

**Service (BudgetService.cs:105):**
```csharp
throw new Exception("Budget Code already exists - كود الميزانية موجود مسبقاً");
```

**Result:** Technical database errors, stack traces, or internal system details could be exposed.

### 5.2 Localization

**Positive:** Many error messages include **bilingual text** (English and Arabic)

**Examples:**
- `"Invoice not found - الفاتورة غير موجودة"`
- `"Cannot delete non-draft budget: {code} - لا يمكن حذف ميزانية غير مسودة"`
- `"Only draft invoices can be edited - يمكن تعديل المسودات فقط"`

**Pattern:** Hardcoded bilingual strings in exception messages

**Gap:** No centralized localization system, no language selection based on user preference

### 5.3 User-Friendly vs Technical Messages

**Current State:** **Mixed quality**

**User-Friendly Messages (Good):**
```csharp
"Cannot delete invoice with payments - لا يمكن حذف فاتورة لها مدفوعات"
"Only draft budgets can be approved - يمكن اعتماد الميزانيات المسودة فقط"
"Payment must be approved before posting"
```

**Technical Messages (Exposed):**
```csharp
"Failed to generate invoice number after 5 attempts: {ex.Message}"
"Exception reason: Object reference not set to an instance of an object"
context.Response.StatusCode = 500
```

**Best Practice in PaymentHub:**
```csharp
result.ErrorMessage = "Failed to create GL journal entry";
result.ErrorMessageAr = "فشل في إنشاء قيد اليومية";
// Internal: ex.Message logged separately
```

### 5.4 HTTP Status Codes

**Controller-Level:** Generic `ResultCode.FAIL` returned (typically 200 OK with error in body)

**PaymentHubController:** Proper HTTP status codes:
```csharp
private int GetStatusCode(string errorCode)
{
    return errorCode switch
    {
        "NOT_FOUND" => 404,
        "VALIDATION_FAILED" => 400,
        "INVALID_STATUS" => 400,
        "INSUFFICIENT_BALANCE" => 400,
        "GL_JOURNAL_FAILED" => 400,
        "CREATE_POST_FAILED" => 500,
        _ => 500
    };
}
```

**Global Middleware:** Sets HTTP 500 for unhandled exceptions

---

## 6. Security & Compliance

### 6.1 Sensitive Data Exposure

**CRITICAL RISK:** Exception messages may expose sensitive financial data

**Potential Leaks:**

**Account Numbers:**
```csharp
// ARInvoiceService.cs - Could expose account IDs in GL preparation
$"AR Invoice {invoice.InvoiceNumber} - {customer?.NameEn}"
```

**Balance Information:**
```csharp
// PaymentHubService.cs:757
$"Requested {dto.AllocatedAmount} but only {payment.UnallocatedAmount} available"
```

**Customer/Vendor Names:**
```csharp
// Always included in error context
payment.CounterpartyName
customer.NameEn
```

**Database Schema Information:**
```csharp
// Generic exceptions could expose table names, column names
SqlException messages
```

### 6.2 Stack Trace Exposure

**Development Mode:**
```csharp
// GlobalExceptionFilter.cs:126-129
if (_env.IsDevelopment())
{
    response.StackTrace = exception.StackTrace;
    response.InnerException = exception.InnerException?.Message;
}
```

**Global Middleware:**
```csharp
// GlobalExceptionMiddleware.cs:92-95
#if DEBUG
apiResult.Add("error", "Please find answers in issues...");
#endif
```

**Risk:** Stack traces reveal internal code structure, file paths, method names

### 6.3 Data Sanitization

**Current State:** **NO sanitization of error messages or context data**

**Missing Controls:**
- No redaction of account numbers
- No masking of sensitive amounts
- No filtering of PII (Personally Identifiable Information)
- No removal of technical details before client response

### 6.4 Audit Trail

**Positive:** `SysOperLog` captures exception details for compliance

**Logged Data:**
- User identity
- IP address and location
- Timestamp
- Full request parameters
- Full response
- Error message

**Compliance Value:**
- ✓ **User accountability:** Who triggered the error
- ✓ **Non-repudiation:** Immutable log record
- ✓ **Forensics:** Complete request/response for investigation
- ✗ **Financial context:** Missing transaction details (invoice number, amount, etc.)

---

## 7. Gaps & Risks

### 7.1 Inconsistent Exception Handling

**Issue:** Custom exceptions exist but are rarely used

| Module | Custom Exceptions Available | Actual Usage |
|--------|---------------------------|--------------|
| ARAP | None defined | Generic `Exception` |
| Budget | None defined | Generic `Exception` |
| Treasury | `TreasuryException` (40+ codes) | **Partial usage** |
| Period Validation | Period exceptions (3 types) | **Rarely used** |
| PaymentHub | Result objects with codes | **Good usage** |

**Example of Gap:**
```csharp
// ARInvoiceService.cs:112 - Should use custom exception
throw new Exception("Credit limit exceeded - تجاوز الحد الائتماني");

// Better approach (not implemented):
throw new InsufficientCreditException(customerId, requiredAmount, creditLimit);
```

### 7.2 Missing Financial-Specific Exceptions

**Required Exception Types (Not Implemented):**

- `InsufficientBalanceException` (for payments)
- `InvalidAmountException` (for negative or zero amounts)
- `CreditLimitExceededException` (for AR invoices)
- `FiscalPeriodClosedException` (exists but not used)
- `DuplicateInvoiceNumberException` (for unique constraint violations)
- `InvalidGLAccountException` (for chart of accounts validation)
- `UnbalancedJournalException` (for GL entry validation)
- `PaymentAllocationException` (for payment application errors)

### 7.3 Exception Control Flow Anti-Pattern

**Issue:** Using exceptions for business logic flow

**Example (ARInvoiceService.cs:209-211):**
```csharp
var existing = _invoiceRepository.GetById(dto.Id);
if (existing == null)
    throw new Exception("Invoice not found - الفاتورة غير موجودة");
```

**Better Approach:**
```csharp
var existing = _invoiceRepository.GetById(dto.Id);
if (existing == null)
    return UpdateResult.Failure("INVOICE_NOT_FOUND", "Invoice not found");
```

**Impact:**
- Performance overhead (exception creation is expensive)
- Obscures actual error conditions
- Difficult to unit test

### 7.4 Swallowed Exceptions

**Example (GlobalExceptionMiddleware.cs:146-155):**
```csharp
try
{
    SysOperLogService.InsertOperlog(sysOperLog);
}
catch (Exception exH)
{
    Console.WriteLine(exH.Message); // Swallowed!
}
```

**Risk:** If logging fails, the error is silently ignored

### 7.5 Missing Transaction Rollback Logging

**Issue:** No audit trail when transactions fail

**Example (ARInvoiceService.cs:194-198):**
```csharp
catch (Exception)
{
    Context.Ado.RollbackTran();
    throw; // No log before rollback
}
```

**Impact:**
- Cannot determine which transactions failed
- No forensic data for debugging
- Compliance gap for financial systems

### 7.6 No Retry Logic

**Issue:** Transient failures (network, deadlock) are not retried

**Example Scenario:**
- Database deadlock during invoice posting
- Network timeout during GL integration
- Temporary unavailability of external service

**Current Behavior:** Immediate failure, no automatic retry

**Required:** Retry with exponential backoff for transient errors

### 7.7 Insufficient Validation

**Issue:** Exception handling is reactive, not proactive

**Example (ARInvoiceService.cs:107-113):**
```csharp
public ARInvoice Create(CreateARInvoiceDto dto, string userName)
{
    // Validation happens inline, throws exception
    if (!_customerService.ValidateCreditLimit(dto.CustomerId, dto.TotalAmount))
    {
        throw new Exception("Credit limit exceeded - تجاوز الحد الائتماني");
    }
    // ...
}
```

**Better Approach:**
```csharp
// Validate before entering service method
var validationResult = ValidateCreateInvoice(dto);
if (!validationResult.IsValid)
{
    return CreateResult.Failure(validationResult.Errors);
}
```

**Note:** `InvoiceValidator.cs` exists with comprehensive validation methods but is **not consistently used** in services.

### 7.8 Missing Correlation IDs

**Issue:** Cannot track related operations across distributed logs

**Example:**
- Invoice created → GL journal posted → Payment applied
- If any step fails, no way to correlate logs

**Required:**
- Generate correlation ID at API entry point
- Pass through all service calls
- Include in all log entries

---

## 8. Recommendations

### 8.1 Implement Financial Exception Hierarchy

**Priority:** HIGH

**Action:** Create comprehensive exception hierarchy for Finance module

**Proposed Structure:**
```csharp
namespace ZR.Model.ERP.Finance.Exceptions
{
    // Base exception
    public abstract class FinanceException : Exception
    {
        public abstract string ErrorCode { get; }
        public string MessageAr { get; protected set; }
        public string MessageEn { get; protected set; }
        public object Context { get; protected set; }
        public Guid? TransactionId { get; protected set; }
    }

    // Domain-specific exceptions
    public class InsufficientBalanceException : FinanceException
    {
        public override string ErrorCode => "INSUFFICIENT_BALANCE";
        public Guid AccountId { get; }
        public decimal Required { get; }
        public decimal Available { get; }
    }

    public class FiscalPeriodClosedException : FinanceException
    {
        public override string ErrorCode => "FISCAL_PERIOD_CLOSED";
        public DateTime Date { get; }
    }

    public class InvalidAmountException : FinanceException
    {
        public override string ErrorCode => "INVALID_AMOUNT";
        public decimal Amount { get; }
        public string FieldName { get; }
    }

    public class DuplicateInvoiceException : FinanceException
    {
        public override string ErrorCode => "DUPLICATE_INVOICE";
        public string InvoiceNumber { get; }
    }
}
```

### 8.2 Adopt Result Pattern (Not Exceptions)

**Priority:** HIGH

**Action:** Replace exception-based flow control with Result objects

**Pattern:**
```csharp
public class OperationResult<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string ErrorCode { get; set; }
    public string MessageEn { get; set; }
    public string MessageAr { get; set; }
    public List<string> ValidationErrors { get; set; }

    public static OperationResult<T> SuccessResult(T data) => new() { Success = true, Data = data };
    public static OperationResult<T> FailResult(string code, string msgEn, string msgAr = null) => new()
    {
        Success = false,
        ErrorCode = code,
        MessageEn = msgEn,
        MessageAr = msgAr
    };
}
```

**Usage:**
```csharp
public OperationResult<ARInvoice> Create(CreateARInvoiceDto dto, string userName)
{
    // Validate
    var validation = ValidateInvoice(dto);
    if (!validation.IsValid)
        return OperationResult<ARInvoice>.FailResult("VALIDATION_FAILED", validation.ErrorMessage);

    // Check credit limit
    if (!_customerService.ValidateCreditLimit(dto.CustomerId, dto.TotalAmount))
        return OperationResult<ARInvoice>.FailResult("CREDIT_LIMIT_EXCEEDED", "...");

    // Business logic
    Context.Ado.BeginTran();
    try
    {
        var invoice = // create invoice
        Context.Ado.CommitTran();
        return OperationResult<ARInvoice>.SuccessResult(invoice);
    }
    catch (Exception ex)
    {
        Context.Ado.RollbackTran();
        return OperationResult<ARInvoice>.FailResult("CREATE_FAILED", ex.Message);
    }
}
```

**Benefits:**
- Explicit success/failure handling
- No performance overhead
- Easier to unit test
- Better API design

### 8.3 Sanitize Error Messages

**Priority:** CRITICAL

**Action:** Implement message sanitization layer

**Pattern:**
```csharp
public static class ErrorMessageSanitizer
{
    public static string SanitizeForClient(string message, bool isDevelopment)
    {
        if (isDevelopment) return message; // Full detail in dev

        // Remove SQL error details
        message = Regex.Replace(message, @"SqlException.*", "Database error");

        // Remove stack trace references
        message = Regex.Replace(message, @"at [^\n]+", "");

        // Remove file paths
        message = Regex.Replace(message, @"[A-Z]:\\[^\s]+", "[path]");

        // Mask GUIDs
        message = Regex.Replace(message,
            @"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}",
            "[id]");

        return message;
    }

    public static decimal MaskAmount(decimal amount, bool showFull)
    {
        return showFull ? amount : Math.Round(amount / 1000, 0) * 1000; // Round to thousands
    }
}
```

**Usage in Controller:**
```csharp
catch (Exception ex)
{
    var safeMessage = ErrorMessageSanitizer.SanitizeForClient(ex.Message, _env.IsDevelopment());
    return ToResponse(ResultCode.FAIL, safeMessage);
}
```

### 8.4 Add Transaction-Level Logging

**Priority:** HIGH

**Action:** Log before rollback with financial context

**Pattern:**
```csharp
Context.Ado.BeginTran();
try
{
    // Operations
    Context.Ado.CommitTran();
}
catch (Exception ex)
{
    // Log BEFORE rollback
    _logger.LogError(ex,
        "Transaction rollback for invoice {InvoiceNumber}. User: {User}, Amount: {Amount}, Lines: {LineCount}",
        invoice.InvoiceNumber, userName, invoice.TotalAmount, lineEntities.Count);

    Context.Ado.RollbackTran();
    throw;
}
```

### 8.5 Implement Correlation IDs

**Priority:** MEDIUM

**Action:** Add correlation tracking across services

**Implementation:**
```csharp
// Middleware to generate correlation ID
public class CorrelationIdMiddleware
{
    public async Task Invoke(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        await _next(context);
    }
}

// Service method
public ARInvoice Create(CreateARInvoiceDto dto, string userName, string correlationId)
{
    _logger.LogInformation("Creating invoice. CorrelationId: {CorrelationId}", correlationId);
    // ...
}
```

### 8.6 Utilize InvoiceValidator Consistently

**Priority:** MEDIUM

**Action:** Enforce validation before service operations

**Current State:** `InvoiceValidator.cs` contains comprehensive validation but is rarely called

**Recommended Usage:**
```csharp
public ARInvoice Create(CreateARInvoiceDto dto, string userName)
{
    // ALWAYS validate first
    var errors = InvoiceValidator.ValidateInvoice(
        dto.InvoiceNumber, dto.InvoiceDate, dto.DueDate,
        dto.Currency, dto.ExchangeRate, dto.TotalAmount, dto.Lines.Count);

    if (errors.Any())
        throw new ValidationException(string.Join("; ", errors));

    // Validate lines
    int lineNum = 1;
    foreach (var line in dto.Lines)
    {
        var lineErrors = InvoiceValidator.ValidateInvoiceLine(
            lineNum++, line.AccountId, line.Quantity, line.UnitPrice);
        if (lineErrors.Any())
            throw new ValidationException(string.Join("; ", lineErrors));
    }

    // Proceed with creation
    // ...
}
```

### 8.7 Add Retry Logic for Transient Failures

**Priority:** MEDIUM

**Action:** Implement Polly retry policies

**NuGet:** `Polly`

**Pattern:**
```csharp
using Polly;
using Polly.Retry;

private static readonly AsyncRetryPolicy _retryPolicy = Policy
    .Handle<SqlException>(ex => IsTransient(ex))
    .Or<TimeoutException>()
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timespan, retryCount, context) =>
        {
            _logger.LogWarning("Retry {RetryCount} after {Timespan}s due to {Exception}",
                retryCount, timespan.TotalSeconds, exception.GetType().Name);
        });

public async Task<ARInvoice> CreateAsync(CreateARInvoiceDto dto, string userName)
{
    return await _retryPolicy.ExecuteAsync(async () =>
    {
        // Database operation
        return await CreateInvoiceInternal(dto, userName);
    });
}

private static bool IsTransient(SqlException ex)
{
    // SQL Server transient error codes
    int[] transientErrors = { 1205, 49918, 49919, 49920, 4060, 40197, 40501, 40613 };
    return transientErrors.Contains(ex.Number);
}
```

### 8.8 Implement Structured Logging

**Priority:** MEDIUM

**Action:** Use structured logging with financial context

**Pattern:**
```csharp
// Use structured log properties instead of string interpolation
_logger.LogError(ex,
    "Invoice creation failed. " +
    "InvoiceNumber: {InvoiceNumber}, " +
    "Customer: {CustomerId}, " +
    "Amount: {Amount}, " +
    "Currency: {Currency}, " +
    "User: {User}, " +
    "CorrelationId: {CorrelationId}",
    dto.InvoiceNumber, dto.CustomerId, dto.TotalAmount, dto.Currency,
    userName, correlationId);

// Avoid:
_logger.LogError(ex, $"Invoice {dto.InvoiceNumber} creation failed for customer {dto.CustomerId}");
```

**Benefits:**
- Searchable log properties
- Better log aggregation (e.g., in ELK, Seq, Application Insights)
- Easier debugging

### 8.9 Implement Circuit Breaker for External Services

**Priority:** LOW

**Action:** Protect against cascading failures from external systems (Bank API, ZATCA, etc.)

**Pattern:**
```csharp
using Polly.CircuitBreaker;

private static readonly AsyncCircuitBreakerPolicy _circuitBreaker = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromMinutes(1),
        onBreak: (ex, duration) => _logger.LogWarning("Circuit breaker opened for {Duration}", duration),
        onReset: () => _logger.LogInformation("Circuit breaker reset"));

public async Task<ZatcaResponse> SubmitInvoiceToZatca(Guid invoiceId)
{
    return await _circuitBreaker.ExecuteAsync(async () =>
    {
        return await _zatcaClient.SubmitInvoice(invoiceId);
    });
}
```

---

## 9. Summary Table

| Aspect | Current State | Risk Level | Recommendation |
|--------|---------------|-----------|----------------|
| **Custom Exceptions** | Partial (Treasury only) | Medium | Implement finance hierarchy |
| **Generic Exceptions** | Widely used | High | Adopt Result pattern |
| **Error Message Exposure** | Raw messages to clients | Critical | Sanitize messages |
| **Transaction Rollback** | Implemented | Low | Add rollback logging |
| **Logging Context** | Global only, no finance context | Medium | Add structured logging |
| **Validation** | Validator exists, rarely used | Medium | Enforce pre-validation |
| **Retry Logic** | None | Medium | Add Polly policies |
| **Correlation Tracking** | None | Medium | Implement correlation IDs |
| **Sensitive Data Leakage** | Possible in logs/errors | Critical | Implement data masking |
| **Bilingual Messages** | Hardcoded | Low | Centralize localization |

---

## 10. Conclusion

The Finance Module exhibits **functional but immature exception handling** practices:

**Strengths:**
- Transaction safety with rollback
- Global exception handling infrastructure
- Bilingual error messages
- Comprehensive logging to audit tables
- PaymentHub shows good practices with result objects

**Critical Weaknesses:**
- **Security:** Raw exception messages expose sensitive data
- **Consistency:** Generic exceptions instead of domain-specific types
- **Observability:** Insufficient financial context in logs
- **Resilience:** No retry logic for transient failures

**Priority Actions:**
1. **Sanitize error messages** (prevent data leakage)
2. **Implement OperationResult pattern** (eliminate exception-based flow control)
3. **Add transaction-level logging** (audit failed operations)
4. **Create finance exception hierarchy** (type-safe error handling)

**Compliance Note:** For financial systems handling regulated transactions (ZATCA, SOX, IFRS), the current exception handling poses **audit and security risks** that should be addressed before production deployment.

---

**Document Version:** 1.0
**Generated By:** Claude Code Analysis
**Review Status:** Pending technical leadership review
