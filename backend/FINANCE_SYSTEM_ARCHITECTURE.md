# Finance System Architecture Documentation

## Overview

The Finance Module (`/ERP/Finance/`) is a comprehensive financial management system that handles General Ledger (GL) operations, Accounts Receivable/Payable (AR/AP), Assets management, and financial reporting. The module follows a **layered architecture pattern** with clear separation of concerns.

---

## 📁 Directory Structure

```
ZR.Model/ERP/Finance/
├── ARAP/                    # Accounts Receivable & Accounts Payable Module
│   ├── Entities/            # AR/AP Entity Models
│   ├── Dto/                 # AR/AP Data Transfer Objects
│   └── Enums/               # AR/AP Enumerations
├── Assets/                  # Fixed & Current Assets Module
│   ├── Dto/                 # Asset DTOs
│   └── Enums/               # Asset Enumerations
├── Configuration/           # System Configuration
│   └── Dto/                 # Configuration DTOs
├── Treasury/                # Treasury Management
│   └── Dto/                 # Treasury DTOs
├── Dto/                     # General Finance DTOs
├── Enums/                   # General Finance Enumerations
└── [Core Entities]          # GL Account, Journal, etc.

ZR.Repository/ERP/Finance/
├── ARAP/                    # AR/AP Repositories
├── Configuration/           # Configuration Repositories
└── [Core Repositories]      # GL Repositories

ZR.Service/ERP/Finance/
├── ARAP/                    # AR/AP Services
├── Assets/                  # Asset Services
├── Configuration/           # Configuration Services
├── Treasury/                 # Treasury Services
└── [Core Services]          # GL Services

ZR.Admin.WebApi/Controllers/ERP/Finance/
├── ARAP/                    # AR/AP Controllers
├── Assets/                  # Asset Controllers
├── Configuration/           # Configuration Controllers
├── Treasury/                # Treasury Controllers
└── [Core Controllers]       # GL Controllers
```

---

## 🏗️ Architecture Layers

### 1. Model Layer (`ZR.Model/ERP/Finance/`)

#### Purpose
Represents database entities and business domain objects. These are the core data structures that map directly to database tables.

#### Location
`ZR.Model/ERP/Finance/`

#### Core Finance Entities

| Entity | Description | Key Features |
|--------|-------------|--------------|
| `GLAccount` | Chart of Accounts | Hierarchical structure, parent-child relationships, account types (asset/liability/equity/revenue/expense) |
| `GLJournal` | General Journal | Journal entries, posting status |
| `GLJournalEntry` | Journal Line Items | Account transactions, debit/credit amounts |
| `GLPeriodClosing` | Period Closing | Fiscal year/period management, closing status |
| `GLFinancialReport` | Financial Reports | Report generation, report types (Balance Sheet, Income Statement, etc.) |
| `GLAccountOpeningBalance` | Opening Balances | Fiscal year opening balances per account |
| `GLAccountBalanceSummary` | Balance Summary | Computed balance summaries for performance |
| `SystemConfiguration` | System Settings | Key-value configuration storage |

#### Common Entity Features

- **Inherits from `SysBase`**: All entities inherit audit fields:
  - `Create_by` - User who created the record
  - `Create_time` - Creation timestamp
  - `Update_by` - User who last updated
  - `Update_time` - Last update timestamp
  - `CompanyID` - Multi-tenant company identifier
  - `BranchID` - Branch identifier

- **Multi-tenant Support**: All entities include `CompanyID`/`TenantId` field for tenant isolation

- **Bilingual Support**: User-facing fields have Arabic and English versions:
  - `NameAr`, `NameEn`
  - `DescriptionAr`, `DescriptionEn`

- **Primary Key**: Uses `Guid` as primary key (non-identity) for distributed systems compatibility

- **ORM Attributes**: Uses SqlSugar ORM attributes:
  - `[SugarTable("table_name")]` - Maps to database table
  - `[SugarColumn(...)]` - Column mapping and constraints
  - `[SugarIndex(...)]` - Database indexes

- **Navigation Properties**: Related entities marked with `[SugarColumn(IsIgnore = true)]` (not stored in DB)

#### Example Entity Structure

```csharp
[SugarTable("gl_account")]
[SugarIndex("UX_gl_account_company_full_number", nameof(CompanyID), OrderByType.Asc, nameof(FullAccountNumber), OrderByType.Asc, true)]
public class GLAccount : SysBase
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "account_id")]
    public Guid AccountId { get; set; }

    [SugarColumn(Length = 50, IsNullable = false, ColumnName = "account_number")]
    public string AccountNumber { get; set; }

    [SugarColumn(Length = 200, IsNullable = false, ColumnName = "full_account_number")]
    public string FullAccountNumber { get; set; }

    [SugarColumn(Length = 200, IsNullable = false, ColumnName = "account_name_ar")]
    public string AccountNameAr { get; set; }

    [SugarColumn(Length = 200, IsNullable = false, ColumnName = "account_name_en")]
    public string AccountNameEn { get; set; }

    [SugarColumn(IsNullable = false, ColumnName = "account_type")]
    public AccountType AccountType { get; set; }

    [SugarColumn(IsNullable = false, ColumnName = "normal_balance")]
    public NormalBalance NormalBalance { get; set; }

    [SugarColumn(IsNullable = true, ColumnName = "parent_account_id")]
    public Guid? ParentAccountId { get; set; }

    // Navigation properties (not stored in DB)
    [SugarColumn(IsIgnore = true)]
    public GLAccount ParentAccount { get; set; }

    [SugarColumn(IsIgnore = true)]
    public List<GLAccount> ChildAccounts { get; set; }
}
```

#### AR/AP Module Entities

**Location:** `ZR.Model/ERP/Finance/ARAP/Entities/`

| Entity | Description | Key Relationships |
|--------|-------------|------------------|
| `Customer` | Customer Master Data | Links to GL Account (AR Control Account), has CreditLimit |
| `Vendor` | Vendor Master Data | Links to GL Account (AP Control Account) |
| `ARInvoice` | Accounts Receivable Invoice | Links to Customer, tracks PaidAmount/RemainingAmount, status |
| `APInvoice` | Accounts Payable Invoice | Links to Vendor, tracks PaidAmount/RemainingAmount, status |
| `Payment` | Payment/Receipt | Supports both Customer Receipts and Vendor Payments |
| `PaymentAllocation` | Payment to Invoice Allocation | Links Payment to ARInvoice or APInvoice |
| `CreditDebitNote` | Credit/Debit Notes | Adjustments for AR or AP |

---

### 2. DTO Layer (`ZR.Model/ERP/Finance/Dto/`)

#### Purpose
Data Transfer Objects for API communication, validation, and data transformation. DTOs separate the internal entity structure from the API contract.

#### Location
`ZR.Model/ERP/Finance/Dto/` and sub-module `Dto/` folders

#### DTO Naming Conventions

| Pattern | Purpose | Example |
|---------|---------|---------|
| `{Entity}QueryDto` | Query/Filter DTOs (inherits `PagerInfo`) | `GLAccountQueryDto`, `ARInvoiceQueryDto` |
| `Create{Entity}Dto` | Create operation DTOs | `CreateAccountDto`, `CreateARInvoiceDto` |
| `Update{Entity}Dto` | Update operation DTOs | `UpdateAccountDto`, `UpdateARInvoiceDto` |
| `{Entity}ResponseDto` | List/Grid display DTOs | `AccountResponseDto`, `ARInvoiceListDto` |
| `{Entity}DetailDto` | Detail view DTOs | `ARInvoiceDetailDto`, `PaymentDetailDto` |

#### DTO Features

- **Validation Attributes**: 
  - `[Required]` - Required field validation
  - `[StringLength(max)]` - String length validation
  - `[Range(min, max)]` - Numeric range validation
  - `[EmailAddress]` - Email format validation

- **Bilingual Error Messages**: All validation attributes support Arabic and English error messages

- **Excel Export Support**: `[ExcelColumn(Name = "...")]` attributes for export functionality

- **Pagination**: Query DTOs inherit from `PagerInfo` for pagination support:
  ```csharp
  public class GLAccountQueryDto : PagerInfo
  {
      public Guid? AccountId { get; set; }
      public string AccountNumber { get; set; }
      // ... other filter properties
  }
  ```

#### Example DTO Structure

```csharp
/// <summary>
/// Create Account DTO - إنشاء حساب جديد
/// </summary>
public class CreateAccountDto
{
    [Required(ErrorMessage = "اسم الحساب بالعربي مطلوب")]
    [StringLength(200, ErrorMessage = "اسم الحساب بالعربي يجب ألا يتجاوز 200 حرف")]
    public string AccountNameAr { get; set; }

    [Required(ErrorMessage = "Account name in English is required")]
    [StringLength(200, ErrorMessage = "Account name must not exceed 200 characters")]
    public string AccountNameEn { get; set; }

    [Required(ErrorMessage = "رقم الحساب مطلوب")]
    [StringLength(50, ErrorMessage = "رقم الحساب يجب ألا يتجاوز 50 حرف")]
    public string AccountNumber { get; set; }

    [Required(ErrorMessage = "نوع الحساب مطلوب")]
    public AccountType AccountType { get; set; }

    [Required(ErrorMessage = "طبيعة الحساب مطلوبة")]
    public NormalBalance NormalBalance { get; set; } = Enums.NormalBalance.Debit;

    public Guid? ParentAccountId { get; set; }
    public bool IsActive { get; set; } = true;
    // ... other properties
}

/// <summary>
/// Account Response DTO - استجابة الحساب
/// </summary>
public class AccountResponseDto
{
    [ExcelColumn(Name = "Account ID")]
    public Guid AccountId { get; set; }

    [ExcelColumn(Name = "Account Number")]
    public string AccountNumber { get; set; }

    [ExcelColumn(Name = "Account Name (Arabic)")]
    public string AccountNameAr { get; set; }

    [ExcelColumn(Name = "Account Name (English)")]
    public string AccountNameEn { get; set; }

    // ... other properties
    public List<AccountResponseDto> Children { get; set; }
    public AccountBalanceDto Balance { get; set; }
}
```

---

### 3. Repository Layer (`ZR.Repository/ERP/Finance/`)

#### Purpose
Data access layer - handles all database operations. Provides abstraction over the database and implements the Repository pattern.

#### Location
`ZR.Repository/ERP/Finance/` and sub-module folders

#### Repository Pattern

**Interface:** `I{Entity}Repository : IBaseRepository<{Entity}>`

**Implementation:** `{Entity}Repository : BaseRepository<{Entity}>, I{Entity}Repository`

#### Repository Features

- **Base Functionality**: Inherits from `BaseRepository<T>` (SqlSugar SimpleClient wrapper)
  - Provides `Queryable()` for LINQ queries
  - Provides `GetFirst()`, `GetList()`, `Insert()`, `Update()`, `Delete()` methods
  - Automatic tenant filtering via `BaseRepository`

- **Multi-tenant Support**: Automatic tenant filtering via `BaseRepository` - all queries automatically filter by `CompanyID`

- **Query Methods**: 
  - `GetList(entity, pager)` - Paginated list with filtering
  - `GetById(Guid id)` - Get by primary key
  - `GetByCode(string code)` - Get by unique code
  - Custom query methods for business-specific needs

- **Pagination**: Returns `PagedInfo<T>` for paginated results

- **Expression Building**: Uses `Expressionable.Create<T>()` for dynamic query building

- **Dependency Injection**: Registered via `[AppService]` attribute

#### Example Repository Implementation

```csharp
/// <summary>
/// GL Account Repository - مستودع الحسابات
/// </summary>
[AppService(ServiceType = typeof(IGLAccountRepository), ServiceLifetime = LifeTime.Transient)]
public class GLAccountRepository : BaseRepository<GLAccount>, IGLAccountRepository
{
    /// <summary>
    /// Get paginated list of accounts
    /// </summary>
    public PagedInfo<GLAccount> GetList(GLAccount parm, PagerInfo pager)
    {
        var predicate = Expressionable.Create<GLAccount>();
        predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.AccountNumber), 
            it => it.AccountNumber.Contains(parm.AccountNumber));
        predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.AccountNameAr), 
            it => it.AccountNameAr.Contains(parm.AccountNameAr));
        predicate = predicate.AndIF(parm.AccountType != default, 
            it => it.AccountType == parm.AccountType);
        predicate = predicate.AndIF(parm.ParentAccountId.HasValue, 
            it => it.ParentAccountId == parm.ParentAccountId);
        predicate = predicate.AndIF(parm.CompanyID.HasValue, 
            it => it.CompanyID == parm.CompanyID);

        return Queryable()
            .Where(predicate.ToExpression())
            .OrderBy(x => x.AccountNumber)
            .ToPage(pager);
    }

    /// <summary>
    /// Get account by ID
    /// </summary>
    public GLAccount GetById(Guid id)
    {
        return GetFirst(x => x.AccountId == id);
    }

    /// <summary>
    /// Check if account code exists
    /// </summary>
    public bool CodeExists(string accountNumber, Guid? excludeAccountId = null)
    {
        var predicate = Expressionable.Create<GLAccount>();
        predicate = predicate.And(it => it.AccountNumber == accountNumber);
        predicate = predicate.AndIF(excludeAccountId.HasValue, 
            it => it.AccountId != excludeAccountId.Value);

        return Any(predicate.ToExpression());
    }

    /// <summary>
    /// Get child accounts
    /// </summary>
    public List<GLAccount> GetChildAccounts(Guid parentAccountId)
    {
        return Queryable()
            .Where(x => x.ParentAccountId == parentAccountId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .OrderBy(x => x.AccountNumber)
            .ToList();
    }
}
```

#### Standard Repository Methods

**Basic CRUD:**
- `GetList(entity, pager)` - Paginated list with filtering
- `GetById(Guid id)` - Get by primary key
- `GetByCode(string code)` - Get by unique code
- `Insert(entity)` - Insert new entity
- `Update(entity)` - Update existing entity
- `Delete(ids)` - Delete entities (soft delete)

**Validation Methods:**
- `CodeExists(string code, Guid? excludeId)` - Check code uniqueness
- `HasChildren(Guid id)` - Check if entity has child records
- `HasTransactions(Guid id)` - Check if entity has transactions

**Custom Query Methods:**
- `GetByType(AccountType type)` - Get by account type
- `GetChildAccounts(Guid parentId)` - Get child accounts
- `GetRootAccounts()` - Get root-level accounts
- `GetLeafAccounts()` - Get leaf accounts only

---

### 4. Service Layer (`ZR.Service/ERP/Finance/`)

#### Purpose
Business logic layer - contains all business rules, validations, and orchestration. This is where the core business logic lives.

#### Location
`ZR.Service/ERP/Finance/` and sub-module folders

#### Service Pattern

**Interface:** `I{Entity}Service : IBaseService<{Entity}>`

**Implementation:** `{Entity}Service : BaseService<{Entity}>, I{Entity}Service`

#### Service Features

- **Dependency Injection**: Uses `[AppService]` attribute for DI registration
  ```csharp
  [AppService(ServiceType = typeof(IGLAccountService), ServiceLifetime = LifeTime.Transient)]
  public class GLAccountService : BaseService<GLAccount>, IGLAccountService
  ```

- **Business Logic**: Contains all business rules and validations:
  - Credit limit validation
  - Account hierarchy validation
  - Circular reference prevention
  - Status workflow management
  - Balance calculations

- **DTO Mapping**: Uses Mapster (`Adapt<T>()`) for entity-DTO conversion
  ```csharp
  var model = dto.Adapt<GLAccount>();
  var dto = entity.Adapt<AccountResponseDto>();
  ```

- **Transaction Management**: Uses repository transactions for complex operations
  ```csharp
  using (var transaction = Context.AsTenant().BeginTransaction())
  {
      // Multiple operations
      transaction.Commit();
  }
  ```

- **Error Handling**: Throws exceptions with bilingual messages
  ```csharp
  throw new Exception("الحساب الأب غير موجود - Parent account not found");
  ```

- **Repository Coordination**: Coordinates multiple repositories for complex operations

#### Example Service Implementation

```csharp
/// <summary>
/// GL Account Service - خدمة الحسابات المالية
/// </summary>
[AppService(ServiceType = typeof(IGLAccountService), ServiceLifetime = LifeTime.Transient)]
public class GLAccountService : BaseService<GLAccount>, IGLAccountService
{
    private readonly IGLAccountRepository _accountRepository;
    private readonly IGLAccountOpeningBalanceRepository _openingBalanceRepository;
    private readonly IGLJournalEntryRepository _journalEntryRepository;

    public GLAccountService(
        IGLAccountRepository accountRepository,
        IGLAccountOpeningBalanceRepository openingBalanceRepository,
        IGLJournalEntryRepository journalEntryRepository)
    {
        _accountRepository = accountRepository;
        _openingBalanceRepository = openingBalanceRepository;
        _journalEntryRepository = journalEntryRepository;
    }

    public PagedInfo<AccountResponseDto> GetList(GLAccountQueryDto query, PagerInfo pager)
    {
        var predicate = Expressionable.Create<GLAccount>();
        predicate = predicate.AndIF(query.AccountId.HasValue, 
            it => it.AccountId == query.AccountId.Value);
        predicate = predicate.AndIF(!string.IsNullOrEmpty(query.AccountNumber), 
            it => it.AccountNumber.Contains(query.AccountNumber));
        // ... more filters

        var result = Queryable()
            .Where(predicate.ToExpression())
            .OrderBy(x => x.AccountNumber)
            .ToPage(pager);

        var dtoList = result.Result.Select(MapToResponseDto).ToList();

        return new PagedInfo<AccountResponseDto>
        {
            PageIndex = result.PageIndex,
            PageSize = result.PageSize,
            TotalNum = result.TotalNum,
            Result = dtoList
        };
    }

    public GLAccount Create(CreateAccountDto dto, string userName)
    {
        // Business validation
        if (dto.ParentAccountId.HasValue)
        {
            var parent = _accountRepository.GetById(dto.ParentAccountId.Value);
            if (parent == null)
            {
                throw new Exception("الحساب الأب غير موجود - Parent account not found");
            }

            // Check for circular reference
            if (_accountRepository.WouldCreateCircularReference(
                dto.ParentAccountId.Value, dto.ParentAccountId))
            {
                throw new Exception("Circular reference detected");
            }

            dto.Level = parent.Level + 1;
        }

        // Validate FullAccountNumber uniqueness
        var fullAccountNumber = BuildFullAccountNumber(dto.AccountNumber, dto.ParentAccountId);
        if (_accountRepository.FullAccountNumberExistsInCompany(fullAccountNumber, dto.CompanyID))
        {
            throw new Exception("Full account number already exists");
        }

        // Map DTO to Entity
        var model = dto.Adapt<GLAccount>();
        model.AccountId = Guid.NewGuid();
        model.FullAccountNumber = fullAccountNumber;
        model.Create_by = userName;
        model.Create_time = DateTime.Now;

        // Insert entity
        _accountRepository.InsertAccount(model);

        // Update parent IsLeaf flag
        if (dto.ParentAccountId.HasValue)
        {
            _accountRepository.UpdateParentIsLeaf(dto.ParentAccountId.Value, false);
        }

        return model;
    }

    private AccountResponseDto MapToResponseDto(GLAccount account)
    {
        var dto = account.Adapt<AccountResponseDto>();
        // Add computed properties
        dto.Balance = CalculateBalance(account.AccountId);
        return dto;
    }
}
```

#### Core Finance Services

| Service | Responsibilities |
|---------|------------------|
| `GLAccountService` | Account CRUD, account tree, balance calculation, code validation |
| `GLJournalService` | Journal management, entry posting |
| `GLJournalEntryService` | Journal entry CRUD, debit/credit validation |
| `GLPeriodClosingService` | Period closing, status management |
| `GLFinancialReportService` | Report generation, data aggregation |
| `SystemConfigurationService` | Configuration management |

#### AR/AP Services

| Service | Responsibilities |
|---------|------------------|
| `CustomerService` | Customer CRUD, credit limit validation, code uniqueness |
| `VendorService` | Vendor CRUD, code uniqueness |
| `ARInvoiceService` | AR invoice CRUD, status auto-update, credit limit check |
| `APInvoiceService` | AP invoice CRUD, status auto-update |
| `PaymentService` | Payment creation, allocation to invoices, invoice balance updates |
| `AgingService` | Aging report generation (0-30, 31-60, 61-90, 90+ days) |

#### Service Business Rules Examples

**Account Service:**
- Validates parent account exists
- Prevents circular references in hierarchy
- Validates FullAccountNumber uniqueness per company
- Updates parent IsLeaf flag when children are added
- Prevents deletion of accounts with transactions

**AR Invoice Service:**
- Validates credit limit before invoice creation
- Auto-calculates `RemainingAmount = TotalAmount - PaidAmount`
- Auto-updates status: Unpaid → PartiallyPaid → Paid → Overdue
- Prevents invoice deletion if payments exist

**Payment Service:**
- Validates payment type matches customer/vendor
- Allocates payments to invoices
- Updates invoice `PaidAmount` and `RemainingAmount`
- Prevents over-allocation (allocation > RemainingAmount)

---

### 5. Controller Layer (`ZR.Admin.WebApi/Controllers/ERP/Finance/`)

#### Purpose
API endpoints - handles HTTP requests and responses. This is the entry point for all API calls.

#### Location
`ZR.Admin.WebApi/Controllers/ERP/Finance/` and sub-module folders

#### Controller Pattern

All controllers follow the standard pattern:

```csharp
[Verify]
[Route("finance/{resource}")]
[ApiController]
public class {Entity}Controller : BaseController
{
    private readonly I{Entity}Service _service;

    public {Entity}Controller(I{Entity}Service service)
    {
        _service = service;
    }

    // Standard CRUD endpoints
    [HttpGet("list")]
    [ActionPermissionFilter(Permission = "finance:{resource}:list")]
    public IActionResult GetList([FromQuery] QueryDto query, [FromQuery] PagerInfo pager)

    [HttpGet("{id}")]
    [ActionPermissionFilter(Permission = "finance:{resource}:query")]
    public IActionResult GetInfo(Guid id)

    [HttpPost]
    [ActionPermissionFilter(Permission = "finance:{resource}:add")]
    [Log(Title = "{Entity}", BusinessType = BusinessType.INSERT)]
    public IActionResult Create([FromBody] CreateDto dto)

    [HttpPut]
    [ActionPermissionFilter(Permission = "finance:{resource}:edit")]
    [Log(Title = "{Entity}", BusinessType = BusinessType.UPDATE)]
    public IActionResult Update([FromBody] UpdateDto dto)

    [HttpDelete("{ids}")]
    [ActionPermissionFilter(Permission = "finance:{resource}:delete")]
    [Log(Title = "{Entity}", BusinessType = BusinessType.DELETE)]
    public IActionResult Delete(string ids)
}
```

#### Controller Features

- **Authorization**: `[Verify]` attribute for authentication - ensures user is logged in

- **Permission Control**: `[ActionPermissionFilter]` for role-based access control
  ```csharp
  [ActionPermissionFilter(Permission = "finance:account:list")]
  ```

- **Audit Logging**: `[Log]` attribute for operation tracking
  ```csharp
  [Log(Title = "GL Account", BusinessType = BusinessType.INSERT)]
  ```

- **Standardized Responses**: Uses `SUCCESS()` and `ToResponse()` methods
  ```csharp
  return SUCCESS(data);
  return ToResponse(ResultCode.FAIL, "Error message");
  ```

- **Error Handling**: Try-catch blocks with bilingual error messages
  ```csharp
  try
  {
      var result = _service.Create(dto, HttpContext.GetName());
      return SUCCESS(result);
  }
  catch (Exception ex)
  {
      return ToResponse(ResultCode.FAIL, ex.Message);
  }
  ```

- **Model Validation**: `ModelState.IsValid` checks
  ```csharp
  if (!ModelState.IsValid)
      return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters");
  ```

#### Example Controller Implementation

```csharp
/// <summary>
/// GL Account Controller - متحكم الحسابات المالية
/// </summary>
[Verify]
[Route("finance/accounts")]
[ApiController]
public class GLAccountController : BaseController
{
    private readonly IGLAccountService _accountService;

    public GLAccountController(IGLAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Get list of accounts - الحصول على قائمة الحسابات
    /// </summary>
    [HttpGet("list")]
    [ActionPermissionFilter(Permission = "finance:account:list")]
    public IActionResult GetList([FromQuery] GLAccountQueryDto query, [FromQuery] PagerInfo pager)
    {
        return SUCCESS(_accountService.GetList(query, pager));
    }

    /// <summary>
    /// Get account by ID - الحصول على الحساب بالمعرف
    /// </summary>
    [HttpGet("{id:guid}")]
    [ActionPermissionFilter(Permission = "finance:account:query")]
    public IActionResult GetInfo(Guid id)
    {
        var response = _accountService.GetById(id);
        if (response == null)
            return ToResponse(ResultCode.FAIL, "Account not found - الحساب غير موجود");
        return SUCCESS(response);
    }

    /// <summary>
    /// Create account - إنشاء حساب
    /// </summary>
    [HttpPost]
    [ActionPermissionFilter(Permission = "finance:account:add")]
    [Log(Title = "GL Account", BusinessType = BusinessType.INSERT)]
    public IActionResult Create([FromBody] CreateAccountDto dto)
    {
        if (!ModelState.IsValid)
            return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

        try
        {
            var result = _accountService.Create(dto, HttpContext.GetName());
            return SUCCESS(result);
        }
        catch (Exception ex)
        {
            return ToResponse(ResultCode.FAIL, ex.Message);
        }
    }

    /// <summary>
    /// Update account - تحديث حساب
    /// </summary>
    [HttpPut]
    [ActionPermissionFilter(Permission = "finance:account:edit")]
    [Log(Title = "GL Account", BusinessType = BusinessType.UPDATE)]
    public IActionResult Update([FromBody] UpdateAccountDto dto)
    {
        if (!ModelState.IsValid)
            return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

        try
        {
            return ToResponse(_accountService.Update(dto, HttpContext.GetName()));
        }
        catch (Exception ex)
        {
            return ToResponse(ResultCode.FAIL, ex.Message);
        }
    }

    /// <summary>
    /// Delete accounts - حذف حسابات
    /// </summary>
    [HttpDelete("{ids}")]
    [ActionPermissionFilter(Permission = "finance:account:delete")]
    [Log(Title = "GL Account", BusinessType = BusinessType.DELETE)]
    public IActionResult Delete(string ids)
    {
        var idsArr = ids.Split(',').Select(Guid.Parse).ToArray();
        try
        {
            return ToResponse(_accountService.Delete(idsArr, HttpContext.GetName()));
        }
        catch (Exception ex)
        {
            return ToResponse(ResultCode.FAIL, ex.Message);
        }
    }
}
```

#### Standard Controller Endpoints

**CRUD Operations:**
- `GET /finance/{resource}/list` - Get paginated list
- `GET /finance/{resource}/{id}` - Get by ID
- `GET /finance/{resource}/code/{code}` - Get by code
- `POST /finance/{resource}` - Create new entity
- `PUT /finance/{resource}` - Update entity
- `DELETE /finance/{resource}/{ids}` - Delete entities

**Special Endpoints:**
- `GET /finance/{resource}/tree` - Get hierarchical tree
- `POST /finance/{resource}/bulk` - Bulk operations
- `GET /finance/{resource}/export` - Export to Excel
- `POST /finance/{resource}/import` - Import from Excel

---

## 🔄 Data Flow

### Create Account Flow

```
1. Client sends POST /finance/accounts
   ↓
2. Controller receives CreateAccountDto
   ↓
3. Controller validates ModelState.IsValid
   ↓
4. Controller calls Service.Create(dto, userName)
   ↓
5. Service validates business rules:
   - Parent account exists
   - FullAccountNumber uniqueness
   - Circular reference check
   ↓
6. Service creates GLAccount entity
   - Maps DTO to Entity (Mapster)
   - Sets audit fields
   - Builds FullAccountNumber
   ↓
7. Repository inserts into database
   ↓
8. Service updates parent IsLeaf flag
   ↓
9. Service returns GLAccount entity
   ↓
10. Controller returns AccountResponseDto
```

### Payment Allocation Flow

```
1. Controller receives payment + allocations
   ↓
2. Service creates Payment entity
   ↓
3. For each allocation:
   - Validate invoice exists
   - Validate allocation <= RemainingAmount
   - Update invoice: PaidAmount += allocation
   - Update invoice: RemainingAmount -= allocation
   - Update invoice status (Unpaid → PartiallyPaid → Paid)
   - Create PaymentAllocation record
   ↓
4. Update payment: AllocatedAmount, UnallocatedAmount
   ↓
5. Return success response
```

---

## 🔗 Entity Relationships

### Core Finance Relationships

```
GLAccount (1) ──→ (N) GLAccount (Parent-Child hierarchy)
GLAccount (1) ──→ (N) GLJournalEntry
GLAccount (1) ──→ (N) GLAccountOpeningBalance

GLJournal (1) ──→ (N) GLJournalEntry
```

### AR/AP Module Relationships

```
Customer (1) ──→ (N) ARInvoice
    │
    └──→ GLAccount (AR Control Account)

Vendor (1) ──→ (N) APInvoice
    │
    └──→ GLAccount (AP Control Account)

Payment (1) ──→ (N) PaymentAllocation ──→ (1) ARInvoice or APInvoice
```

---

## 📊 Key Features

### 1. Multi-Tenant Support
- All entities include `CompanyID`/`TenantId` field
- `BaseRepository` automatically filters by tenant
- Tenant isolation at database level
- No cross-tenant data leakage

### 2. Audit Trail
- All entities inherit from `SysBase`
- Tracks: `Create_by`, `Create_time`, `Update_by`, `Update_time`
- Automatic timestamp management via SqlSugar
- Full audit history maintained

### 3. Bilingual Support
- All user-facing fields have Arabic and English versions
- Error messages in both languages
- DTOs support bilingual validation messages
- UI can switch between languages

### 4. Status Management
- Invoice status auto-updates based on payments and due dates
- Journal posting status management
- Period status (Open → Closed)
- Automatic status transitions

### 5. GL Integration
- All financial transactions integrate with General Ledger
- GL account references via `GLAccountId`
- Journal entries track all GL transactions
- Seamless integration with General Ledger

### 6. Validation Rules
- Credit limit validation for customers
- Over-allocation prevention
- Date validation (DueDate >= InvoiceDate)
- Code uniqueness validation
- Amount validation (must be positive)
- Circular reference prevention

---

## 🛠️ Technology Stack

- **ORM**: SqlSugar - Lightweight, high-performance ORM
- **Mapping**: Mapster - Fast object-to-object mapping
- **Validation**: Data Annotations + FluentValidation (optional)
- **DI**: Custom `[AppService]` attribute for dependency injection
- **Multi-tenancy**: SqlSugar tenant support
- **Pagination**: Custom `PagerInfo` class
- **Excel Export**: MiniExcel - Fast Excel import/export
- **Logging**: Custom `[Log]` attribute for audit logging

---

## 📝 Naming Conventions

### Files
- **Entities**: `{EntityName}.cs` (e.g., `GLAccount.cs`)
- **DTOs**: `{EntityName}Dto.cs` (e.g., `GLAccountDto.cs`)
- **Repositories**: `{EntityName}Repository.cs`, `I{EntityName}Repository.cs`
- **Services**: `{EntityName}Service.cs`, `I{EntityName}Service.cs`
- **Controllers**: `{EntityName}Controller.cs`

### Database Tables
- **Prefix**: `gl_` for GL entities, `arap_` for AR/AP entities
- **Naming**: Snake_case (e.g., `gl_account`, `arap_customer`, `arap_ar_invoice`)

### API Routes
- **Pattern**: `/finance/{module}/{resource}`
- **Examples**: 
  - `/finance/accounts`
  - `/finance/arap/customers`
  - `/finance/journals`
  - `/finance/arap/payments`

---

## 🔐 Security & Permissions

### Permission Naming Convention
```
finance:{module}:{resource}:{action}
```

**Examples:**
- `finance:account:list` - List accounts
- `finance:arap:customer:add` - Add customer
- `finance:journal:edit` - Edit journal
- `finance:arap:payment:delete` - Delete payment

### Authorization Flow
1. `[Verify]` - Checks authentication (user must be logged in)
2. `[ActionPermissionFilter]` - Checks permission (user must have required permission)
3. Service validates business rules
4. Repository enforces tenant isolation

---

## 📈 Performance Considerations

1. **Pagination**: All list endpoints support pagination to limit data transfer
2. **Indexing**: Database indexes on `CompanyID`, foreign keys, codes for fast lookups
3. **Lazy Loading**: Navigation properties use `[SugarColumn(IsIgnore = true)]` to avoid unnecessary joins
4. **Query Optimization**: Uses `Expressionable` for efficient SQL generation
5. **Caching**: Repository methods support optional caching for frequently accessed data
6. **Materialized Paths**: Account hierarchy uses materialized paths for efficient tree queries

---

## 🧪 Testing Considerations

### Unit Testing
- Test services with mocked repositories
- Test business logic (credit limits, allocations, status updates)
- Test validation rules
- Test DTO mapping

### Integration Testing
- Test repository queries
- Test controller endpoints
- Test multi-tenant isolation
- Test transaction rollback scenarios

### Test Data
- Use test fixtures for common entities
- Isolate test data by tenant
- Clean up after tests
- Use in-memory database for fast tests

---

## 🖥️ Frontend Architecture (`Internal_Event_Management_System/apps/isomorphic-intl/`)

### Overview

The frontend is built using **Next.js** with **React** and **TypeScript**, following a modern component-based architecture. It communicates with the backend API through service layers and uses TypeScript interfaces that match the backend DTOs.

### Frontend Directory Structure

```
apps/isomorphic-intl/src/
├── app/
│   └── [locale]/(hydrogen)/finance/
│       ├── accounts/
│       │   └── page.tsx              # Accounts management page
│       └── dashboard/
│           └── page.tsx              # Finance dashboard page
├── shared/
│   ├── services/finance/
│   │   ├── accounts.service.ts      # Account API service
│   │   ├── Journals.service.ts      # Journal API service
│   │   └── dashboard.service.ts     # Dashboard API service
│   └── types/
│       └── finance.types.ts          # Finance TypeScript interfaces
```

### Frontend Service Layer (`shared/services/finance/`)

#### Purpose
Service layer that handles all API communication with the backend. Provides typed functions for CRUD operations and business logic.

#### Service Pattern

All services follow a consistent pattern:
- Use `get`, `post`, `put`, `del` utility functions from `@/app/shared/utils/request`
- Map frontend types to backend DTOs
- Handle data transformation and validation
- Return typed responses

#### Accounts Service (`accounts.service.ts`)

**Location:** `apps/isomorphic-intl/src/app/shared/services/finance/accounts.service.ts`

**Key Functions:**

```typescript
// Account CRUD Operations
listAccounts(query?: AccountQueryParams)
getAccount(accountId: string)
getAccountByCode(accountNumber: string)
addAccount(data: AccountForm | CreateAccountForm)
updateAccount(data: AccountForm | UpdateAccountForm)
deleteAccount(accountIds: string | string[])

// Account Query Operations
getAccountTree()
getAccountsByType(accountType: AccountType)
getLeafAccounts()
getChildAccounts(parentId: string)
getCashAccounts()
getBankAccounts()
searchAccounts(searchTerm: string, maxResults?: number)
getAccountHierarchy(accountId: string)

// Account Balance Operations
getAccountBalance(accountId: string, fiscalYear: number, asOfDate?: string)
getAggregatedBalance(accountId: string, fiscalYear: number, asOfDate?: string)
getBalanceSummary(query: BalanceSummaryQuery)
recalculateBalance(accountId: string, fiscalYear: number, fiscalPeriod: number)

// Opening Balance Operations
listOpeningBalances(query?: OpeningBalanceQueryParams)
createOpeningBalance(data: CreateOpeningBalanceForm)
updateOpeningBalance(data: UpdateOpeningBalanceForm)
deleteOpeningBalance(id: string)
importOpeningBalances(data: BulkOpeningBalanceForm)
lockOpeningBalances(fiscalYear: number)
carryforwardOpeningBalances(newFiscalYear: number, previousFiscalYear: number)
validateOpeningBalanceTotals(fiscalYear: number)

// Validation Operations
validateForPosting(accountId: string, isManualEntry?: boolean)
validateForDeletion(accountId: string)
checkCodeExists(accountNumber: string, excludeAccountId?: string)
validateHierarchy(accountId: string, newParentId?: string)
```

**Example Implementation:**

```typescript
/**
 * Get paginated list of accounts
 */
export function listAccounts(query?: AccountQueryParams) {
  return get('/finance/accounts/list', query);
}

/**
 * Create a new account
 */
export function addAccount(data: AccountForm | CreateAccountForm) {
  // Map frontend form to backend CreateAccountDto
  const accountType = parseAccountType(data.accountType);
  const normalBalance = data.normalBalance
    ? parseNormalBalance(data.normalBalance)
    : getDefaultNormalBalance(accountType);

  const payload: CreateAccountForm = {
    accountNameAr: data.accountNameAr || '',
    accountNameEn: data.accountNameEn || '',
    accountNumber: data.accountNumber || '',
    accountType: accountType,
    normalBalance: normalBalance,
    level: data.level || 1,
    parentAccountId: data.parentAccountId || null,
    isActive: data.isActive !== undefined ? data.isActive : true,
    // ... other fields
  };
  return post('/finance/accounts', payload);
}
```

#### Journals Service (`Journals.service.ts`)

**Location:** `apps/isomorphic-intl/src/app/shared/services/finance/Journals.service.ts`

**Key Functions:**

```typescript
// Journal/Voucher CRUD Operations
listVouchers(query?: VoucherQueryParams)
getVoucher(voucherId: string)
createVoucher(data: VoucherForm)
updateVoucher(data: VoucherForm)
deleteVoucher(voucherIds: string | string[])
postVoucher(voucherId: string)
reverseVoucher(voucherId: string, reason?: string)
```

**Features:**
- Maps frontend `VoucherForm` to backend DTOs
- Handles voucher entries transformation
- Supports bilingual descriptions (Arabic/English)

#### Dashboard Service (`dashboard.service.ts`)

**Location:** `apps/isomorphic-intl/src/app/shared/services/finance/dashboard.service.ts`

**Key Functions:**

```typescript
getFinanceDashboard(query?: FinanceDashboardQueryParams)
```

**Features:**
- Fetches dashboard summary data
- Supports date range filtering
- Returns aggregated financial metrics

### Frontend Types (`shared/types/finance.types.ts`)

#### Purpose
TypeScript type definitions that match backend DTOs. Ensures type safety between frontend and backend.

#### Key Interfaces

**Account Types:**
```typescript
export interface Account {
  accountId: string;
  accountNumber: string;
  accountNameEn: string;
  accountNameAr?: string;
  accountType: AccountType;
  normalBalance: NormalBalance;
  level: number;
  parentAccountId?: string | null;
  isActive: boolean;
  isLeaf: boolean;
  // ... other properties
  children?: Account[];
  balance?: number;
}

export interface CreateAccountForm {
  accountNameAr: string;
  accountNameEn: string;
  accountNumber: string;
  accountType: AccountType;
  normalBalance?: NormalBalance;
  parentAccountId?: string | null;
  // ... other properties
}

export interface AccountQueryParams extends BaseQueryParams {
  accountId?: string;
  accountNumber?: string;
  accountType?: AccountType;
  isActive?: boolean;
  // ... other filters
}
```

**Enums:**
```typescript
export enum AccountType {
  Asset = 1,
  Liability = 2,
  Equity = 3,
  Revenue = 4,
  Expense = 5,
  ContraAsset = 6,
  ContraLiability = 7,
  ContraEquity = 8,
  ContraRevenue = 9,
  OtherComprehensiveIncome = 10
}

export enum NormalBalance {
  Debit = 1,
  Credit = 2
}
```

**Utility Functions:**
```typescript
getAccountTypeLabel(type: AccountType, lang: 'en' | 'ar'): string
getNormalBalanceLabel(balance: NormalBalance, lang: 'en' | 'ar'): string
getDefaultNormalBalance(accountType: AccountType): NormalBalance
parseAccountType(value: number | string | AccountType): AccountType
parseNormalBalance(value: number | string | NormalBalance): NormalBalance
```

**Features:**
- Matches backend DTO structure exactly
- Provides bilingual labels for enums
- Includes utility functions for type conversion
- Supports legacy field aliases for backward compatibility

### Frontend Pages

#### Accounts Page (`accounts/page.tsx`)

**Location:** `apps/isomorphic-intl/src/app/[locale]/(hydrogen)/finance/accounts/page.tsx`

**Features:**
- Account tree view with expand/collapse
- Account list with pagination and filtering
- Create/Update/Delete account operations
- Account form with validation
- Search and filter functionality
- Bilingual support (Arabic/English)
- Real-time account status updates

**Key Components:**
- Account tree display
- Account list table
- Account form dialog
- Search and filter panel
- Action buttons (Add, Edit, Delete, Toggle Status)

**State Management:**
- Uses React hooks (`useState`, `useEffect`)
- Manages account list, tree structure, form data
- Handles loading states and error messages

**Example Usage:**
```typescript
const [accounts, setAccounts] = useState<Account[]>([]);
const [loadingAccounts, setLoadingAccounts] = useState(false);

const loadAccounts = async () => {
  setLoadingAccounts(true);
  try {
    const response = await listAccounts(queryParams);
    if (response.code === 200) {
      setAccounts(response.data.Result);
      setAccountsTotal(response.data.TotalNum);
    }
  } catch (error) {
    toast.error(error.message);
  } finally {
    setLoadingAccounts(false);
  }
};
```

#### Dashboard Page (`dashboard/page.tsx`)

**Location:** `apps/isomorphic-intl/src/app/[locale]/(hydrogen)/finance/dashboard/page.tsx`

**Features:**
- Financial overview cards (Total Accounts, Total Vouchers, Total Assets)
- Recent transactions list
- Search and filter functionality
- Date range filtering
- Refresh functionality

**Key Components:**
- Stat cards for summary metrics
- Transaction list table
- Search panel
- Date range picker

**State Management:**
- Manages dashboard data state
- Handles query parameters
- Loading and error states

### Frontend-Backend Integration

#### API Communication Flow

```
1. Frontend Component (React)
   ↓
2. Service Layer (accounts.service.ts)
   - Maps frontend types to backend DTOs
   - Transforms data format
   ↓
3. HTTP Request (get/post/put/del)
   - Uses axios/fetch wrapper
   - Adds authentication headers
   - Handles errors
   ↓
4. Backend Controller
   - Receives DTO
   - Validates request
   ↓
5. Backend Service
   - Business logic
   - Data transformation
   ↓
6. Backend Repository
   - Database operations
   ↓
7. Response flows back through layers
   ↓
8. Frontend updates UI
```

#### Data Mapping

**Frontend to Backend:**
- `AccountForm` → `CreateAccountDto` / `UpdateAccountDto`
- `AccountQueryParams` → `GLAccountQueryDto`
- `VoucherForm` → `CreateVoucherDto` / `UpdateVoucherDto`

**Backend to Frontend:**
- `AccountResponseDto` → `Account`
- `PagedInfo<AccountResponseDto>` → `{ Result: Account[], TotalNum: number }`
- `AccountTreeNodeDto` → `AccountTreeNode`

#### Error Handling

Frontend services handle errors consistently:
```typescript
try {
  const response = await listAccounts(query);
  if (response.code === 200) {
    // Success handling
  } else {
    toast.error(response.msg || 'Operation failed');
  }
} catch (error: any) {
  toast.error(error.message || 'An error occurred');
}
```

#### Bilingual Support

- All user-facing text uses `next-intl` for translations
- Enum labels support both Arabic and English
- Form validation messages are bilingual
- API error messages are displayed in user's language

**Example:**
```typescript
const t = useTranslations('finance.accounts');
const locale = useLocale();

// Get account type label in current language
const typeLabel = getAccountTypeLabel(account.accountType, locale);
```

### Frontend Technology Stack

- **Framework**: Next.js 14+ (App Router)
- **Language**: TypeScript
- **UI Library**: React with custom components
- **State Management**: React Hooks (useState, useEffect)
- **HTTP Client**: Custom request utility (axios wrapper)
- **Internationalization**: next-intl
- **Icons**: react-icons (Phosphor Icons)
- **Notifications**: react-hot-toast
- **Styling**: Tailwind CSS

### Frontend Best Practices

1. **Type Safety**: All API calls are typed with TypeScript interfaces
2. **Service Layer**: All API communication goes through service layer
3. **Error Handling**: Consistent error handling with user-friendly messages
4. **Loading States**: All async operations show loading indicators
5. **Form Validation**: Client-side validation before API calls
6. **Bilingual Support**: All text is internationalized
7. **Responsive Design**: Mobile-friendly layouts
8. **Accessibility**: ARIA labels and keyboard navigation support

---

## 📚 Additional Resources

### Related Documentation
- `FINANCE_MODULE_STRUCTURE.md` - Detailed module structure
- `BACKEND_STRUCTURE_DOCUMENTATION.md` - Overall backend architecture
- `SysBase.cs` - Base entity structure
- `BaseRepository.cs` - Repository base implementation

### Key Classes to Review
- `BaseService<T>` - Service base class
- `BaseController` - Controller base class
- `PagerInfo` - Pagination helper
- `Expressionable` - Dynamic query builder
- `SysBase` - Base entity with audit fields

---

## 🎯 Summary

The Finance Module is a comprehensive, well-structured financial management system that:

✅ **Follows clean architecture principles** - Clear separation of concerns  
✅ **Supports multi-tenancy** - Automatic tenant isolation  
✅ **Provides bilingual support** - Arabic and English throughout  
✅ **Implements robust business rules** - Comprehensive validation  
✅ **Integrates with GL system** - Seamless General Ledger integration  
✅ **Maintains audit trails** - Full history tracking  
✅ **Enforces security and permissions** - Role-based access control  
✅ **Optimizes for performance** - Efficient queries and pagination  

The module is production-ready and follows all established patterns and conventions of the system.

---

## 📞 Support

For questions or issues related to the Finance Module architecture, please refer to:
- Code documentation in XML comments
- This architecture documentation
- Related module documentation files
- Development team lead

---

**Last Updated**: 2024  
**Version**: 1.0  
**Author**: Development Team

