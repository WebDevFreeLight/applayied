# Finance Module - Exception Handling Phase 1 Implementation Guide

**Project:** ZR.Admin.NET
**Module:** Finance (ARAP, Budget, Treasury, PaymentHub, GL, Assets, Zatca)
**Phase:** 1 - Initial Implementation
**Timeline:** 2-3 weeks
**Date:** December 15, 2025

---

## Table of Contents

1. [Overview](#overview)
2. [Pre-Implementation Checklist](#pre-implementation-checklist)
3. [Step-by-Step Implementation](#step-by-step-implementation)
4. [Code Changes Required](#code-changes-required)
5. [Testing Strategy](#testing-strategy)
6. [Rollback Plan](#rollback-plan)
7. [Phase 2+ Roadmap](#phase-2-roadmap)

---

## Overview

### Goals

✅ Implement universal exception handling framework
✅ Migrate Finance module to new exception system
✅ Eliminate raw exception message exposure
✅ Add bilingual error messages (English/Arabic)
✅ Implement structured logging
✅ Remove try-catch blocks from controllers

### Scope

**In Scope (Phase 1):**
- Core exception infrastructure (AppException, ExceptionCode, etc.)
- Global exception middleware
- Localization service with English and Arabic
- Finance module exception codes (ARAP, Budget, Treasury, PaymentHub, GL)
- Migration of Finance services to use AppException
- Clean up Finance controllers (remove try-catch)

**Out of Scope (Phase 1):**
- Other modules (HR, Inventory, Sales, etc.)
- Advanced features (metrics, alerting)
- Database migration for old logs

### Success Criteria

✅ All Finance API endpoints return standardized error responses
✅ No raw exception messages exposed to clients
✅ 100% of Finance services use AppException
✅ All error messages available in English and Arabic
✅ Structured logging operational
✅ Zero breaking changes to API contracts (backward compatible)

---

## Pre-Implementation Checklist

### 1. Development Environment Setup

- [ ] Create feature branch: `feature/finance-exception-handling-phase1`
- [ ] Backup current codebase
- [ ] Set up local testing environment
- [ ] Install NuGet packages (if needed):
  ```bash
  dotnet add package Microsoft.Extensions.Localization
  ```

### 2. Team Preparation

- [ ] Review universal exception handling document
- [ ] Identify Finance module owners
- [ ] Schedule code review sessions
- [ ] Prepare rollback plan

### 3. Code Analysis

- [ ] Review existing Finance exception patterns (see analysis report)
- [ ] Identify all Finance controllers and services
- [ ] Document current error response formats
- [ ] List all unique error scenarios in Finance module

---

## Step-by-Step Implementation

### Week 1: Infrastructure Setup

#### Day 1-2: Core Exception Infrastructure

**Task:** Create exception framework classes

**Files to Create:**

1. **Infrastructure/Exceptions/ExceptionSeverity.cs**
   ```bash
   Location: backend/Infrastructure/Exceptions/ExceptionSeverity.cs
   ```

2. **Infrastructure/Exceptions/ExceptionCode.cs**
   ```bash
   Location: backend/Infrastructure/Exceptions/ExceptionCode.cs
   ```

3. **Infrastructure/Exceptions/ExceptionMetadata.cs**
   ```bash
   Location: backend/Infrastructure/Exceptions/ExceptionMetadata.cs
   ```

4. **Infrastructure/Exceptions/AppException.cs**
   ```bash
   Location: backend/Infrastructure/Exceptions/AppException.cs
   ```

**Action Steps:**

```bash
# Create folder structure
cd backend/Infrastructure
mkdir Exceptions
cd Exceptions

# Copy code from Universal_Exception_Handling_System.md
# Create the 4 files listed above
```

**Validation:**
- [ ] All 4 files created
- [ ] Project compiles successfully
- [ ] No namespace conflicts

---

#### Day 3: Response Models

**Files to Create:**

1. **Infrastructure/Models/ApiResponse.cs**
2. **Infrastructure/Models/ErrorResponse.cs**

**Action Steps:**

```bash
cd backend/Infrastructure
mkdir Models
cd Models

# Create ApiResponse.cs and ErrorResponse.cs
```

**Important Note:**

Your project already has `Infrastructure/Model/ApiResult.cs`. We need to decide:

**Option A (Recommended):** Keep both, use `ApiResponse<T>` for new code, `ApiResult` for existing code
**Option B:** Migrate all code to `ApiResponse<T>` (breaking change, not recommended for Phase 1)

**For Phase 1, choose Option A:**

```csharp
// Keep existing ApiResult.cs
// Add new ApiResponse.cs alongside it
// Controllers can use either (gradual migration)
```

**Validation:**
- [ ] Both response models created
- [ ] No conflicts with existing `ApiResult`
- [ ] Project compiles

---

#### Day 4: Localization Service

**Files to Create:**

1. **Infrastructure/Localization/ILocalizationService.cs**
2. **Infrastructure/Localization/LocalizationService.cs**

**Action Steps:**

```bash
cd backend/Infrastructure
mkdir Localization
cd Localization

# Create interface and implementation
```

**Validation:**
- [ ] Interface and implementation created
- [ ] Project compiles

---

#### Day 5: Resource Files for Finance Module

**Files to Create:**

1. **Resources/ExceptionMessages.resx** (English)
2. **Resources/ExceptionMessages.ar.resx** (Arabic)

**Action Steps:**

```bash
cd backend
mkdir Resources
cd Resources

# Create .resx files using Visual Studio or VS Code
```

**ExceptionMessages.resx (English) - Finance Section:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <!-- Common Exceptions -->
  <data name="COMMON_VALIDATION_FAILED" xml:space="preserve">
    <value>Validation failed. Please check your input.</value>
  </data>
  <data name="COMMON_NOT_FOUND" xml:space="preserve">
    <value>Resource not found.</value>
  </data>
  <data name="COMMON_INTERNAL_ERROR" xml:space="preserve">
    <value>An internal error occurred. Please contact support.</value>
  </data>
  <data name="COMMON_DATABASE_ERROR" xml:space="preserve">
    <value>Database operation failed. Please try again.</value>
  </data>

  <!-- Finance - AR Invoices -->
  <data name="FINANCE_INVOICE_NOT_FOUND" xml:space="preserve">
    <value>Invoice not found.</value>
  </data>
  <data name="FINANCE_INVOICE_DUPLICATE" xml:space="preserve">
    <value>Invoice number {InvoiceNumber} already exists.</value>
  </data>
  <data name="FINANCE_INVOICE_ALREADY_POSTED" xml:space="preserve">
    <value>Invoice is already posted and cannot be modified.</value>
  </data>
  <data name="FINANCE_INVOICE_CANNOT_DELETE" xml:space="preserve">
    <value>Cannot delete invoice with payments.</value>
  </data>
  <data name="FINANCE_INVOICE_INVALID_STATUS" xml:space="preserve">
    <value>Only draft invoices can be modified.</value>
  </data>
  <data name="FINANCE_INVOICE_INVALID_AMOUNT" xml:space="preserve">
    <value>Invoice amount must be greater than zero.</value>
  </data>

  <!-- Finance - Payments -->
  <data name="FINANCE_PAYMENT_NOT_FOUND" xml:space="preserve">
    <value>Payment not found.</value>
  </data>
  <data name="FINANCE_PAYMENT_INSUFFICIENT_BALANCE" xml:space="preserve">
    <value>Insufficient balance. Required: {RequiredAmount}, Available: {AvailableAmount}</value>
  </data>
  <data name="FINANCE_PAYMENT_ALREADY_POSTED" xml:space="preserve">
    <value>Payment is already posted.</value>
  </data>
  <data name="FINANCE_PAYMENT_INVALID_STATUS" xml:space="preserve">
    <value>Payment must be approved before posting.</value>
  </data>

  <!-- Finance - Budget -->
  <data name="FINANCE_BUDGET_NOT_FOUND" xml:space="preserve">
    <value>Budget not found.</value>
  </data>
  <data name="FINANCE_BUDGET_DUPLICATE_CODE" xml:space="preserve">
    <value>Budget code {BudgetCode} already exists.</value>
  </data>
  <data name="FINANCE_BUDGET_CANNOT_MODIFY" xml:space="preserve">
    <value>Only draft budgets can be modified.</value>
  </data>
  <data name="FINANCE_BUDGET_INSUFFICIENT_FUNDS" xml:space="preserve">
    <value>Insufficient budget funds. Available: {AvailableAmount}, Required: {RequiredAmount}</value>
  </data>

  <!-- Finance - GL -->
  <data name="FINANCE_GL_PERIOD_CLOSED" xml:space="preserve">
    <value>Fiscal period {PeriodName} is closed for date {Date}.</value>
  </data>
  <data name="FINANCE_GL_PERIOD_LOCKED" xml:space="preserve">
    <value>Fiscal period {PeriodName} is locked.</value>
  </data>
  <data name="FINANCE_GL_UNBALANCED_ENTRY" xml:space="preserve">
    <value>Journal entry is not balanced. Difference: {Difference}</value>
  </data>
  <data name="FINANCE_GL_POSTING_FAILED" xml:space="preserve">
    <value>Failed to post journal entry.</value>
  </data>

  <!-- Finance - Treasury -->
  <data name="FINANCE_TREASURY_ACCOUNT_NOT_FOUND" xml:space="preserve">
    <value>Treasury account not found.</value>
  </data>
  <data name="FINANCE_TREASURY_ACCOUNT_INACTIVE" xml:space="preserve">
    <value>Treasury account is inactive.</value>
  </data>
  <data name="FINANCE_TREASURY_INSUFFICIENT_BALANCE" xml:space="preserve">
    <value>Insufficient treasury balance.</value>
  </data>
</root>
```

**ExceptionMessages.ar.resx (Arabic):**

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <!-- Common Exceptions -->
  <data name="COMMON_VALIDATION_FAILED" xml:space="preserve">
    <value>فشل التحقق من الصحة. يرجى التحقق من المدخلات.</value>
  </data>
  <data name="COMMON_NOT_FOUND" xml:space="preserve">
    <value>المورد غير موجود.</value>
  </data>
  <data name="COMMON_INTERNAL_ERROR" xml:space="preserve">
    <value>حدث خطأ داخلي. يرجى الاتصال بالدعم.</value>
  </data>
  <data name="COMMON_DATABASE_ERROR" xml:space="preserve">
    <value>فشلت عملية قاعدة البيانات. يرجى المحاولة مرة أخرى.</value>
  </data>

  <!-- Finance - AR Invoices -->
  <data name="FINANCE_INVOICE_NOT_FOUND" xml:space="preserve">
    <value>الفاتورة غير موجودة.</value>
  </data>
  <data name="FINANCE_INVOICE_DUPLICATE" xml:space="preserve">
    <value>رقم الفاتورة {InvoiceNumber} موجود مسبقاً.</value>
  </data>
  <data name="FINANCE_INVOICE_ALREADY_POSTED" xml:space="preserve">
    <value>الفاتورة مرحّلة بالفعل ولا يمكن تعديلها.</value>
  </data>
  <data name="FINANCE_INVOICE_CANNOT_DELETE" xml:space="preserve">
    <value>لا يمكن حذف فاتورة لها مدفوعات.</value>
  </data>
  <data name="FINANCE_INVOICE_INVALID_STATUS" xml:space="preserve">
    <value>يمكن تعديل الفواتير المسودة فقط.</value>
  </data>
  <data name="FINANCE_INVOICE_INVALID_AMOUNT" xml:space="preserve">
    <value>مبلغ الفاتورة يجب أن يكون أكبر من صفر.</value>
  </data>

  <!-- Finance - Payments -->
  <data name="FINANCE_PAYMENT_NOT_FOUND" xml:space="preserve">
    <value>الدفعة غير موجودة.</value>
  </data>
  <data name="FINANCE_PAYMENT_INSUFFICIENT_BALANCE" xml:space="preserve">
    <value>رصيد غير كافٍ. مطلوب: {RequiredAmount}، متاح: {AvailableAmount}</value>
  </data>
  <data name="FINANCE_PAYMENT_ALREADY_POSTED" xml:space="preserve">
    <value>الدفعة مرحّلة بالفعل.</value>
  </data>
  <data name="FINANCE_PAYMENT_INVALID_STATUS" xml:space="preserve">
    <value>يجب اعتماد الدفعة قبل الترحيل.</value>
  </data>

  <!-- Finance - Budget -->
  <data name="FINANCE_BUDGET_NOT_FOUND" xml:space="preserve">
    <value>الميزانية غير موجودة.</value>
  </data>
  <data name="FINANCE_BUDGET_DUPLICATE_CODE" xml:space="preserve">
    <value>كود الميزانية {BudgetCode} موجود مسبقاً.</value>
  </data>
  <data name="FINANCE_BUDGET_CANNOT_MODIFY" xml:space="preserve">
    <value>يمكن تعديل الميزانيات المسودة فقط.</value>
  </data>
  <data name="FINANCE_BUDGET_INSUFFICIENT_FUNDS" xml:space="preserve">
    <value>أموال الميزانية غير كافية. متاح: {AvailableAmount}، مطلوب: {RequiredAmount}</value>
  </data>

  <!-- Finance - GL -->
  <data name="FINANCE_GL_PERIOD_CLOSED" xml:space="preserve">
    <value>الفترة المالية {PeriodName} مغلقة للتاريخ {Date}.</value>
  </data>
  <data name="FINANCE_GL_PERIOD_LOCKED" xml:space="preserve">
    <value>الفترة المالية {PeriodName} مقفلة.</value>
  </data>
  <data name="FINANCE_GL_UNBALANCED_ENTRY" xml:space="preserve">
    <value>قيد اليومية غير متوازن. الفرق: {Difference}</value>
  </data>
  <data name="FINANCE_GL_POSTING_FAILED" xml:space="preserve">
    <value>فشل ترحيل قيد اليومية.</value>
  </data>

  <!-- Finance - Treasury -->
  <data name="FINANCE_TREASURY_ACCOUNT_NOT_FOUND" xml:space="preserve">
    <value>حساب الخزينة غير موجود.</value>
  </data>
  <data name="FINANCE_TREASURY_ACCOUNT_INACTIVE" xml:space="preserve">
    <value>حساب الخزينة غير نشط.</value>
  </data>
  <data name="FINANCE_TREASURY_INSUFFICIENT_BALANCE" xml:space="preserve">
    <value>رصيد الخزينة غير كافٍ.</value>
  </data>
</root>
```

**Validation:**
- [ ] Both .resx files created in Resources folder
- [ ] Files are set as embedded resources in project
- [ ] Build action: EmbeddedResource

**Update .csproj to include resources:**

```xml
<ItemGroup>
  <EmbeddedResource Update="Resources\ExceptionMessages.resx">
    <Generator>ResXFileCodeGenerator</Generator>
  </EmbeddedResource>
  <EmbeddedResource Update="Resources\ExceptionMessages.ar.resx">
    <DependentUpon>ExceptionMessages.resx</DependentUpon>
  </EmbeddedResource>
</ItemGroup>
```

---

### Week 2: Middleware and Service Integration

#### Day 6-7: Global Exception Middleware

**File to Create:**

**Infrastructure/Middleware/GlobalExceptionMiddleware.cs**

**Important Consideration:**

Your project already has `ZR.ServiceCore/Middleware/GlobalExceptionMiddleware.cs`. We have two options:

**Option A (Recommended):** Replace existing middleware
- Backup old middleware
- Replace with new implementation
- Test thoroughly

**Option B:** Create new middleware with different name
- Name it `UniversalExceptionMiddleware.cs`
- Register both temporarily
- Migrate gradually

**For Phase 1, use Option A:**

**Action Steps:**

```bash
# 1. Backup existing middleware
cd backend/ZR.ServiceCore/Middleware
cp GlobalExceptionMiddleware.cs GlobalExceptionMiddleware.cs.backup

# 2. Replace with new implementation
# Copy code from Universal_Exception_Handling_System.md

# 3. Update namespace if needed
# Change: namespace Infrastructure.Middleware
# To: namespace ZR.ServiceCore.Middleware
```

**Updated GlobalExceptionMiddleware.cs (with correct namespace):**

```csharp
using Infrastructure.Exceptions;
using Infrastructure.Localization;
using Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ZR.ServiceCore.Middleware // <-- Use your project namespace
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IHostEnvironment _environment;

        // ... rest of implementation from universal doc
    }
}
```

**Validation:**
- [ ] Old middleware backed up
- [ ] New middleware implemented
- [ ] Namespace matches project structure
- [ ] Project compiles

---

#### Day 8: Dependency Injection Setup

**File to Create/Modify:**

**Infrastructure/Extensions/ServiceCollectionExtensions.cs**

**Action Steps:**

```bash
cd backend/Infrastructure
mkdir Extensions
cd Extensions

# Create ServiceCollectionExtensions.cs
```

**ServiceCollectionExtensions.cs:**

```csharp
using Infrastructure.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using ZR.ServiceCore.Middleware; // Use correct namespace

namespace Infrastructure.Extensions
{
    public static class ExceptionHandlingExtensions
    {
        public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
        {
            // Register localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Register localization service
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddScoped<IStringLocalizer<ExceptionMessages>, StringLocalizer<ExceptionMessages>>();

            return services;
        }

        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalExceptionMiddleware>();
            return app;
        }
    }
}
```

**Validation:**
- [ ] Extension methods created
- [ ] Namespaces correct
- [ ] Project compiles

---

#### Day 9: Update Program.cs

**File to Modify:**

**backend/ZR.Admin.WebApi/Program.cs**

**Current Program.cs (approximation):**

```csharp
var builder = WebApplication.CreateBuilder(args);

// ... existing services

var app = builder.Build();

// Existing middleware
app.UseMiddleware<GlobalExceptionMiddleware>(); // OLD
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

**Updated Program.cs:**

```csharp
using Infrastructure.Extensions; // ADD THIS

var builder = WebApplication.CreateBuilder(args);

// ... existing services

// ADD: Register exception handling
builder.Services.AddExceptionHandling();

var app = builder.Build();

// REPLACE: Use new global exception handler
// OLD: app.UseMiddleware<GlobalExceptionMiddleware>();
// NEW:
app.UseGlobalExceptionHandler(); // MUST BE FIRST MIDDLEWARE

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Critical Notes:**

1. `UseGlobalExceptionHandler()` MUST be the FIRST middleware
2. This ensures ALL exceptions are caught
3. Place before routing, authentication, authorization

**Validation:**
- [ ] Exception handling registered in DI
- [ ] Middleware registered as first in pipeline
- [ ] Application starts successfully
- [ ] Test endpoint returns proper error format

---

### Week 3: Migrate Finance Services

#### Day 10-12: Migrate AR Invoice Service

**File to Modify:**

**backend/ZR.Service/ERP/Finance/ARAP/AR/ARInvoiceService.cs**

**Before (Current Code):**

```csharp
public ARInvoice GetById(Guid id)
{
    var invoice = _repository.GetById(id);
    if (invoice == null)
    {
        throw new Exception("Invoice not found - الفاتورة غير موجودة");
    }
    return invoice;
}

public ARInvoice Create(CreateARInvoiceDto dto, string userName)
{
    // Check duplicate
    if (_repository.ExistsByNumber(dto.InvoiceNumber))
    {
        throw new Exception($"Invoice number {dto.InvoiceNumber} already exists");
    }

    Context.Ado.BeginTran();
    try
    {
        // ... create invoice
        Context.Ado.CommitTran();
        return invoice;
    }
    catch (Exception ex)
    {
        Context.Ado.RollbackTran();
        throw; // Generic exception
    }
}
```

**After (New Code):**

```csharp
using Infrastructure.Exceptions; // ADD THIS

public ARInvoice GetById(Guid id)
{
    var invoice = _repository.GetById(id);
    if (invoice == null)
    {
        // Use AppException with code
        throw new AppException(ExceptionCode.FINANCE_INVOICE_NOT_FOUND);
    }
    return invoice;
}

public ARInvoice Create(CreateARInvoiceDto dto, string userName)
{
    // Validation
    if (dto.TotalAmount <= 0)
    {
        throw new AppException(ExceptionCode.FINANCE_INVOICE_INVALID_AMOUNT);
    }

    // Check duplicate with parameters
    if (_repository.ExistsByNumber(dto.InvoiceNumber))
    {
        throw new AppException(
            ExceptionCode.FINANCE_INVOICE_DUPLICATE,
            new Dictionary<string, object>
            {
                { "InvoiceNumber", dto.InvoiceNumber }
            });
    }

    Context.Ado.BeginTran();
    try
    {
        var invoice = new ARInvoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = dto.InvoiceNumber,
            TotalAmount = dto.TotalAmount,
            Create_by = userName,
            Create_time = DateTime.Now
        };

        _repository.Insert(invoice);
        Context.Ado.CommitTran();

        return invoice;
    }
    catch (AppException)
    {
        // Re-throw AppException as-is
        Context.Ado.RollbackTran();
        throw;
    }
    catch (Exception ex)
    {
        // Wrap generic exception
        Context.Ado.RollbackTran();
        throw new AppException(
            ExceptionCode.COMMON_DATABASE_ERROR,
            "Failed to create invoice",
            details: new { InvoiceNumber = dto.InvoiceNumber },
            severity: ExceptionSeverity.Critical,
            httpStatusCode: 500,
            innerException: ex);
    }
}

public void Delete(Guid id)
{
    var invoice = GetById(id); // Throws if not found

    // Business rule validation
    if (invoice.PaidAmount > 0)
    {
        throw new AppException(ExceptionCode.FINANCE_INVOICE_CANNOT_DELETE);
    }

    if (invoice.Status != InvoiceStatus.Draft)
    {
        throw new AppException(ExceptionCode.FINANCE_INVOICE_INVALID_STATUS);
    }

    _repository.Delete(id);
}

public void PostInvoice(Guid id)
{
    var invoice = GetById(id);

    if (invoice.Status != InvoiceStatus.Draft)
    {
        throw new AppException(ExceptionCode.FINANCE_INVOICE_INVALID_STATUS);
    }

    if (invoice.GLJournalId.HasValue)
    {
        throw new AppException(ExceptionCode.FINANCE_INVOICE_ALREADY_POSTED);
    }

    // Check fiscal period
    var fiscalPeriod = _fiscalPeriodService.GetForDate(invoice.InvoiceDate);
    if (fiscalPeriod.IsClosed)
    {
        throw new AppException(
            ExceptionCode.FINANCE_GL_PERIOD_CLOSED,
            new Dictionary<string, object>
            {
                { "PeriodName", fiscalPeriod.Name },
                { "Date", invoice.InvoiceDate.ToString("yyyy-MM-dd") }
            });
    }

    // GL posting...
    var glJournal = CreateGLJournal(invoice);
    if (!glJournal.IsBalanced)
    {
        throw new AppException(
            ExceptionCode.FINANCE_GL_UNBALANCED_ENTRY,
            new Dictionary<string, object>
            {
                { "Difference", Math.Abs(glJournal.TotalDebit - glJournal.TotalCredit) }
            })
        {
            Details = new
            {
                TotalDebit = glJournal.TotalDebit,
                TotalCredit = glJournal.TotalCredit
            }
        };
    }

    invoice.Status = InvoiceStatus.Posted;
    invoice.GLJournalId = glJournal.Id;
    _repository.Update(invoice);
}
```

**Migration Checklist for ARInvoiceService:**

- [ ] Add `using Infrastructure.Exceptions;`
- [ ] Replace all `throw new Exception(...)` with `throw new AppException(...)`
- [ ] Use appropriate exception codes
- [ ] Add parameters for dynamic messages
- [ ] Keep transaction rollback logic
- [ ] Test all methods

---

#### Day 13-14: Migrate Budget Service

**File to Modify:**

**backend/ZR.Service/ERP/Finance/Budget/BudgetService.cs**

**Key Changes:**

```csharp
using Infrastructure.Exceptions;

// Old:
throw new Exception("Budget Code already exists - كود الميزانية موجود مسبقاً");

// New:
throw new AppException(
    ExceptionCode.FINANCE_BUDGET_DUPLICATE_CODE,
    new Dictionary<string, object>
    {
        { "BudgetCode", dto.BudgetCode }
    });

// Old:
throw new Exception($"Cannot delete non-draft budget: {code}");

// New:
throw new AppException(ExceptionCode.FINANCE_BUDGET_CANNOT_MODIFY);

// Old:
throw new Exception("Budget not found");

// New:
throw new AppException(ExceptionCode.FINANCE_BUDGET_NOT_FOUND);
```

**Validation:**
- [ ] All exceptions migrated
- [ ] Service compiles
- [ ] Unit tests pass (if exist)

---

#### Day 15: Migrate Payment Hub Service

**File to Modify:**

**backend/ZR.Service/ERP/Finance/PaymentHub/PaymentHubService.cs**

**Good News:** This service already uses result objects (`PaymentOperationResult`). We can enhance it:

**Current Pattern:**

```csharp
result.Success = false;
result.ErrorCode = "NOT_FOUND";
result.ErrorMessage = "Payment not found";
result.ErrorMessageAr = "الدفعة غير موجودة";
return result;
```

**Enhanced Pattern (Option 1 - Keep Result Objects):**

Keep the result pattern but throw AppException for critical errors:

```csharp
// For critical errors during transactions - throw
if (glJournal == null)
{
    Context.Ado.RollbackTran();
    throw new AppException(ExceptionCode.FINANCE_GL_POSTING_FAILED);
}

// For business rule violations - return result
if (payment.Status != PaymentStatus.Posted)
{
    result.Success = false;
    result.ErrorCode = ExceptionCode.FINANCE_PAYMENT_INVALID_STATUS;
    result.ErrorMessage = "Payment must be posted";
    return result;
}
```

**Enhanced Pattern (Option 2 - Full Migration):**

```csharp
// Replace result returns with exceptions
if (payment == null)
{
    throw new AppException(ExceptionCode.FINANCE_PAYMENT_NOT_FOUND);
}

if (payment.Status != PaymentStatus.Posted)
{
    throw new AppException(ExceptionCode.FINANCE_PAYMENT_INVALID_STATUS);
}
```

**Recommendation:** Use Option 1 for Phase 1 (minimal changes), Option 2 for Phase 2.

---

### Week 3 (Continued): Update Controllers

#### Day 16-17: Clean Up Finance Controllers

**Files to Modify:**

1. ARInvoiceController.cs
2. APInvoiceController.cs
3. BudgetController.cs
4. PaymentHubController.cs
5. AssetTransactionController.cs

**Before (Current Code):**

```csharp
[HttpGet("{id}")]
public IActionResult GetInvoice(Guid id)
{
    try
    {
        var invoice = _invoiceService.GetById(id);
        return SUCCESS(invoice);
    }
    catch (Exception ex)
    {
        return ToResponse(ResultCode.FAIL, ex.Message); // Exposes raw message!
    }
}
```

**After (New Code):**

```csharp
[HttpGet("{id}")]
public IActionResult GetInvoice(Guid id)
{
    // No try-catch needed - middleware handles it!
    var invoice = _invoiceService.GetById(id);
    return Ok(ApiResponse<ARInvoice>.SuccessResponse(invoice));
}
```

**OR (if keeping existing ApiResult):**

```csharp
[HttpGet("{id}")]
public IActionResult GetInvoice(Guid id)
{
    // No try-catch - middleware handles exceptions
    var invoice = _invoiceService.GetById(id);
    return SUCCESS(invoice); // Keep existing SUCCESS helper
}
```

**Migration Pattern for All Controllers:**

1. **Remove all try-catch blocks**
2. **Let middleware handle exceptions**
3. **Optionally update to use new ApiResponse** (or keep existing ApiResult)

**Example - Complete Controller Migration:**

**ARInvoiceController.cs:**

```csharp
using Infrastructure.Models; // Optional
using Microsoft.AspNetCore.Mvc;

namespace ZR.Admin.WebApi.Controllers.ERP.Finance.ARAP
{
    [ApiController]
    [Route("api/finance/ar-invoices")]
    public class ARInvoiceController : BaseController
    {
        private readonly IARInvoiceService _invoiceService;

        public ARInvoiceController(IARInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetInvoice(Guid id)
        {
            var invoice = _invoiceService.GetById(id);
            return SUCCESS(invoice); // Keep existing helper
        }

        /// <summary>
        /// Create invoice
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateARInvoiceDto dto)
        {
            var invoice = _invoiceService.Create(dto, HttpContext.GetName());
            return SUCCESS(invoice, "Invoice created successfully");
        }

        /// <summary>
        /// Update invoice
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UpdateARInvoiceDto dto)
        {
            dto.Id = id;
            var invoice = _invoiceService.Update(dto, HttpContext.GetName());
            return SUCCESS(invoice, "Invoice updated successfully");
        }

        /// <summary>
        /// Delete invoice
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _invoiceService.Delete(id);
            return SUCCESS("Invoice deleted successfully");
        }

        /// <summary>
        /// Post invoice to GL
        /// </summary>
        [HttpPost("{id}/post")]
        public IActionResult Post(Guid id)
        {
            _invoiceService.PostInvoice(id);
            return SUCCESS("Invoice posted successfully");
        }

        /// <summary>
        /// Cancel invoice
        /// </summary>
        [HttpPost("{id}/cancel")]
        public IActionResult Cancel(Guid id)
        {
            _invoiceService.Cancel(id);
            return SUCCESS("Invoice cancelled successfully");
        }
    }
}
```

**Validation:**
- [ ] All try-catch blocks removed
- [ ] Controllers are clean and simple
- [ ] All methods work as before
- [ ] Error responses are properly formatted

---

#### Day 18: Testing & Validation

**Test Plan:**

**1. Unit Tests (if exist):**

```bash
cd backend
dotnet test
```

**2. API Tests:**

**Test Scenario 1: Not Found**

```bash
# Request
GET /api/finance/ar-invoices/00000000-0000-0000-0000-000000000000
Accept-Language: en

# Expected Response (English)
{
  "success": false,
  "code": "FINANCE_INVOICE_NOT_FOUND",
  "message": "Invoice not found.",
  "severity": "Info",
  "timestamp": "2025-12-15T10:30:00Z",
  "traceId": "...",
  "path": "/api/finance/ar-invoices/00000000-0000-0000-0000-000000000000"
}
```

**Test Scenario 2: Arabic Response**

```bash
# Request
GET /api/finance/ar-invoices/00000000-0000-0000-0000-000000000000
Accept-Language: ar

# Expected Response (Arabic)
{
  "success": false,
  "code": "FINANCE_INVOICE_NOT_FOUND",
  "message": "الفاتورة غير موجودة.",
  "severity": "Info",
  ...
}
```

**Test Scenario 3: Validation Error with Parameters**

```bash
# Request
POST /api/finance/ar-invoices
{
  "invoiceNumber": "INV-001", // Already exists
  "totalAmount": 1000
}

# Expected Response
{
  "success": false,
  "code": "FINANCE_INVOICE_DUPLICATE",
  "message": "Invoice number INV-001 already exists.",
  "severity": "Warning",
  ...
}
```

**Test Scenario 4: Business Rule Violation**

```bash
# Request
DELETE /api/finance/ar-invoices/{id}
# Invoice has payments

# Expected Response
{
  "success": false,
  "code": "FINANCE_INVOICE_CANNOT_DELETE",
  "message": "Cannot delete invoice with payments.",
  "severity": "Warning",
  ...
}
```

**Test Scenario 5: GL Period Closed**

```bash
# Request
POST /api/finance/ar-invoices/{id}/post
# Invoice date is in closed period

# Expected Response
{
  "success": false,
  "code": "FINANCE_GL_PERIOD_CLOSED",
  "message": "Fiscal period December 2024 is closed for date 2024-12-01.",
  "severity": "Warning",
  ...
}
```

**Test Scenario 6: Unbalanced Journal**

```bash
# Request
POST /api/finance/ar-invoices/{id}/post

# Expected Response
{
  "success": false,
  "code": "FINANCE_GL_UNBALANCED_ENTRY",
  "message": "Journal entry is not balanced. Difference: 0.05",
  "details": {
    "totalDebit": 1500.05,
    "totalCredit": 1500.00
  },
  "severity": "Error",
  ...
}
```

**3. Logging Validation:**

Check NLog output to ensure structured logging:

```
2025-12-15 10:30:00.123 [Info] AppException occurred. Code: FINANCE_INVOICE_NOT_FOUND, Severity: Info, TraceId: 0HN7QKQVQJ3K1:00000001, Path: /api/finance/ar-invoices/..., Method: GET, User: admin, Details: null

2025-12-15 10:31:00.456 [Warning] AppException occurred. Code: FINANCE_INVOICE_DUPLICATE, Severity: Warning, TraceId: 0HN7QKQVQJ3K1:00000002, Path: /api/finance/ar-invoices, Method: POST, User: admin, Details: {"InvoiceNumber":"INV-001"}
```

**Validation Checklist:**

- [ ] All error responses use new format
- [ ] English messages work correctly
- [ ] Arabic messages work correctly
- [ ] Parameters are replaced in messages
- [ ] Details object is included when present
- [ ] Severity levels are correct
- [ ] HTTP status codes are correct
- [ ] Trace IDs are unique per request
- [ ] Stack traces only in Development mode
- [ ] Logs contain structured data

---

## Code Changes Required

### Summary of Files

**New Files (13 total):**

1. Infrastructure/Exceptions/ExceptionSeverity.cs
2. Infrastructure/Exceptions/ExceptionCode.cs
3. Infrastructure/Exceptions/ExceptionMetadata.cs
4. Infrastructure/Exceptions/AppException.cs
5. Infrastructure/Models/ApiResponse.cs
6. Infrastructure/Models/ErrorResponse.cs
7. Infrastructure/Localization/ILocalizationService.cs
8. Infrastructure/Localization/LocalizationService.cs
9. Infrastructure/Extensions/ServiceCollectionExtensions.cs
10. Resources/ExceptionMessages.resx
11. Resources/ExceptionMessages.ar.resx
12. Resources/ExceptionMessages.Designer.cs (auto-generated)
13. Infrastructure/Localization/ExceptionMessages.cs

**Modified Files (~15 total):**

1. ZR.ServiceCore/Middleware/GlobalExceptionMiddleware.cs (replaced)
2. ZR.Admin.WebApi/Program.cs
3. ZR.Service/ERP/Finance/ARAP/AR/ARInvoiceService.cs
4. ZR.Service/ERP/Finance/ARAP/AP/APInvoiceService.cs
5. ZR.Service/ERP/Finance/Budget/BudgetService.cs
6. ZR.Service/ERP/Finance/PaymentHub/PaymentHubService.cs
7. ZR.Service/ERP/Finance/Treasury/TreasuryBalanceEngine.cs (if exists)
8. ZR.Admin.WebApi/Controllers/ERP/Finance/ARAP/ARInvoiceController.cs
9. ZR.Admin.WebApi/Controllers/ERP/Finance/ARAP/APInvoiceController.cs
10. ZR.Admin.WebApi/Controllers/ERP/Finance/Budget/BudgetController.cs
11. ZR.Admin.WebApi/Controllers/ERP/Finance/PaymentHub/PaymentHubController.cs
12. ZR.Admin.WebApi/Controllers/ERP/Finance/Assets/AssetTransactionController.cs
13. Infrastructure/Infrastructure.csproj (add embedded resources)
14. ZR.Admin.WebApi/appsettings.json (optional - add localization settings)
15. ZR.Admin.WebApi/appsettings.Development.json (optional)

**Total Effort:** ~15 new files, ~15 modified files

---

## Testing Strategy

### 1. Development Testing

**Phase 1.1: Infrastructure Testing (Day 1-5)**

- [ ] Create test controller with intentional exceptions
- [ ] Verify middleware catches exceptions
- [ ] Verify localization works (en/ar)
- [ ] Verify logging outputs correctly

**Test Controller:**

```csharp
[ApiController]
[Route("api/test/exceptions")]
public class ExceptionTestController : ControllerBase
{
    [HttpGet("not-found")]
    public IActionResult TestNotFound()
    {
        throw new AppException(ExceptionCode.FINANCE_INVOICE_NOT_FOUND);
    }

    [HttpGet("duplicate")]
    public IActionResult TestDuplicate()
    {
        throw new AppException(
            ExceptionCode.FINANCE_INVOICE_DUPLICATE,
            new Dictionary<string, object> { { "InvoiceNumber", "TEST-001" } });
    }

    [HttpGet("generic")]
    public IActionResult TestGeneric()
    {
        throw new Exception("This is a generic exception");
    }
}
```

**Phase 1.2: Service Testing (Day 10-15)**

- [ ] Test each migrated service method
- [ ] Verify exceptions are thrown correctly
- [ ] Verify transaction rollback works
- [ ] Verify parameters are passed correctly

**Phase 1.3: Integration Testing (Day 16-18)**

- [ ] Test complete workflows (Create → Post → Delete)
- [ ] Test error scenarios (duplicate, invalid status, etc.)
- [ ] Test language switching
- [ ] Test with real data

### 2. Postman/API Testing

**Create Postman Collection:**

```json
{
  "info": {
    "name": "Finance Exception Testing",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get Non-Existent Invoice (EN)",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Accept-Language",
            "value": "en"
          }
        ],
        "url": "{{baseUrl}}/api/finance/ar-invoices/00000000-0000-0000-0000-000000000000"
      }
    },
    {
      "name": "Get Non-Existent Invoice (AR)",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Accept-Language",
            "value": "ar"
          }
        ],
        "url": "{{baseUrl}}/api/finance/ar-invoices/00000000-0000-0000-0000-000000000000"
      }
    },
    {
      "name": "Create Duplicate Invoice",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"invoiceNumber\": \"EXISTING-001\",\n  \"totalAmount\": 1000\n}"
        },
        "url": "{{baseUrl}}/api/finance/ar-invoices"
      }
    }
  ]
}
```

### 3. Automated Testing (Optional)

**Create xUnit tests:**

```csharp
public class ExceptionHandlingTests
{
    [Fact]
    public void GetById_NonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        var service = CreateService();
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<AppException>(() => service.GetById(nonExistentId));
        Assert.Equal(ExceptionCode.FINANCE_INVOICE_NOT_FOUND, exception.Code);
    }

    [Fact]
    public void Create_DuplicateNumber_ThrowsDuplicateException()
    {
        // Arrange
        var service = CreateService();
        var dto = new CreateARInvoiceDto { InvoiceNumber = "EXISTING" };

        // Act & Assert
        var exception = Assert.Throws<AppException>(() => service.Create(dto, "user"));
        Assert.Equal(ExceptionCode.FINANCE_INVOICE_DUPLICATE, exception.Code);
        Assert.Contains("EXISTING", exception.Parameters["InvoiceNumber"].ToString());
    }
}
```

---

## Rollback Plan

### If Issues Are Found

**Step 1: Identify Scope**

- Is it a single service/controller?
- Is it the middleware?
- Is it localization?

**Step 2: Selective Rollback**

**Option A: Rollback Middleware Only**

```bash
cd backend/ZR.ServiceCore/Middleware
cp GlobalExceptionMiddleware.cs.backup GlobalExceptionMiddleware.cs
```

Update Program.cs:

```csharp
// Remove:
app.UseGlobalExceptionHandler();

// Restore:
app.UseMiddleware<GlobalExceptionMiddleware>();
```

**Option B: Rollback Single Service**

```bash
git checkout HEAD -- ZR.Service/ERP/Finance/ARAP/AR/ARInvoiceService.cs
```

**Option C: Full Rollback**

```bash
git checkout main
```

### Backup Strategy

**Before each major change:**

```bash
# Day 6: Before middleware replacement
cp -r ZR.ServiceCore/Middleware ZR.ServiceCore/Middleware.backup

# Day 9: Before Program.cs changes
cp ZR.Admin.WebApi/Program.cs ZR.Admin.WebApi/Program.cs.backup

# Day 10: Before service migration
cp -r ZR.Service/ERP/Finance ZR.Service/ERP/Finance.backup
```

---

## Phase 2+ Roadmap

### Phase 2: Other Modules

- [ ] Migrate HR module
- [ ] Migrate Inventory module
- [ ] Migrate Sales module
- [ ] Migrate AI module

### Phase 3: Advanced Features

- [ ] Add exception metrics dashboard
- [ ] Implement alerting for critical exceptions
- [ ] Add retry policies (Polly)
- [ ] Add circuit breaker for external services
- [ ] Implement correlation IDs across microservices

### Phase 4: Optimization

- [ ] Performance testing
- [ ] Exception analytics
- [ ] Fine-tune severity levels
- [ ] Optimize localization caching

---

## Final Checklist

### Pre-Deployment

- [ ] All Finance services migrated
- [ ] All Finance controllers cleaned up
- [ ] Resource files complete (EN + AR)
- [ ] Middleware registered correctly
- [ ] Program.cs updated
- [ ] All tests passing
- [ ] Code review completed
- [ ] Documentation updated

### Deployment

- [ ] Deploy to staging environment
- [ ] Run smoke tests
- [ ] Test language switching
- [ ] Verify logging
- [ ] Monitor for 24 hours
- [ ] Deploy to production
- [ ] Monitor production logs

### Post-Deployment

- [ ] Collect metrics on exception frequency
- [ ] Review log patterns
- [ ] Gather user feedback
- [ ] Plan Phase 2

---

## Support & Resources

**Documentation:**
- Universal Exception Handling System.md
- Finance Module Exception Handling Analysis.md

**Code Examples:**
- See `Universal_Exception_Handling_System.md` section 8 (Usage Examples)

**Questions?**
- Review existing TreasuryException.cs for patterns
- Check PaymentHubService.cs for result object patterns

---

**End of Phase 1 Implementation Guide**

**Estimated Timeline:** 2-3 weeks
**Team Size:** 2-3 developers
**Risk Level:** Low (backward compatible)
**Impact:** High (improved error handling, security, UX)

---

**Next Steps:**

1. Review this guide with the team
2. Create feature branch
3. Start Week 1 implementation
4. Daily standups to track progress
5. Weekly demos to stakeholders

Good luck! 🚀
