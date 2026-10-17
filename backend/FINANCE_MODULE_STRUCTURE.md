# Finance Module Structure Documentation

## Overview

The Finance Module (`/ERP/Finance/`) is a comprehensive financial management system that handles General Ledger (GL) operations, Accounts Receivable/Payable (AR/AP), Assets management, and financial reporting. The module follows a layered architecture pattern with clear separation of concerns.

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
├── Dto/                     # General Finance DTOs
├── Enums/                   # General Finance Enumerations
└── [Core Entities]          # GL Account, Voucher, Journal, etc.

ZR.Repository/ERP/Finance/
├── ARAP/                    # AR/AP Repositories
└── [Core Repositories]      # GL Repositories

ZR.Service/ERP/Finance/
├── ARAP/                    # AR/AP Services
└── [Core Services]          # GL Services

ZR.Admin.WebApi/Controllers/ERP/Finance/
├── ARAP/                    # AR/AP Controllers
└── [Core Controllers]       # GL Controllers
```

---

## 🏗️ Architecture Layers

### 1. Model Layer (`ZR.Model/ERP/Finance/`)

#### Core Finance Entities

**Location:** `ZR.Model/ERP/Finance/`

| Entity | Description | Key Features |
|--------|-------------|--------------|
| `GLAccount` | Chart of Accounts | Hierarchical structure, parent-child relationships, account types (asset/liability/equity/revenue/expense) |
| `GLVoucher` | Accounting Voucher | Voucher entries, status workflow (Draft → Submitted → Approved → Posted), fiscal period tracking |
| `GLVoucherEntry` | Voucher Line Items | Debit/Credit entries, account references, line numbers |
| `GLJournal` | General Journal | Journal entries, posting status |
| `GLJournalEntry` | Journal Line Items | Account transactions, debit/credit amounts |
| `GLPeriodClosing` | Period Closing | Fiscal year/period management, closing status |
| `GLFinancialReport` | Financial Reports | Report generation, report types (Balance Sheet, Income Statement, etc.) |
| `SystemConfiguration` | System Settings | Key-value configuration storage |

**Common Entity Features:**
- Inherits from `SysBase` (audit fields: Create_by, Create_time, Update_by, Update_time)
- Multi-tenant support via `TenantId`
- Arabic & English field support (`NameAr`, `NameEn`, `DescriptionAr`, `DescriptionEn`)
- Uses `Guid` as primary key (non-identity)
- SqlSugar ORM attributes (`[SugarTable]`, `[SugarColumn]`)

#### AR/AP Module Entities

**Location:** `ZR.Model/ERP/Finance/ARAP/Entities/`

| Entity | Description | Key Relationships |
|--------|-------------|------------------|
| `Customer` | Customer Master Data | Links to GL Account (AR Control Account), has CreditLimit |
| `Vendor` | Vendor Master Data | Links to GL Account (AP Control Account) |
| `ARInvoice` | Accounts Receivable Invoice | Links to Customer, tracks PaidAmount/RemainingAmount, status (Unpaid/PartiallyPaid/Paid/Overdue) |
| `APInvoice` | Accounts Payable Invoice | Links to Vendor, tracks PaidAmount/RemainingAmount, status |
| `Payment` | Payment/Receipt | Supports both Customer Receipts and Vendor Payments, tracks AllocatedAmount/UnallocatedAmount |
| `PaymentAllocation` | Payment to Invoice Allocation | Links Payment to ARInvoice or APInvoice |
| `CreditDebitNote` | Credit/Debit Notes | Adjustments for AR or AP, supports both Customer and Vendor |

**AR/AP Entity Features:**
- Invoice status auto-calculation based on DueDate and RemainingAmount
- Payment allocation prevents over-allocation
- Credit limit validation for customers
- GL voucher integration (GLVoucherId field)

#### Assets Module Entities

**Location:** `ZR.Model/ERP/Finance/Assets/`

| Entity | Description |
|--------|-------------|
| `FixedAsset` | Fixed Assets (Property, Plant, Equipment) |
| `CurrentAsset` | Current Assets (Inventory, Cash, etc.) |
| `AssetCategory` | Asset Classification |
| `AssetDepreciation` | Depreciation Calculations |
| `AssetTransaction` | Asset Transactions |

#### Enumerations

**Location:** `ZR.Model/ERP/Finance/Enums/` and `ZR.Model/ERP/Finance/ARAP/Enums/`

**Core Finance Enums:**
- `VoucherStatus`: Draft, Submitted, Approved, Posted, Rejected, Cancelled
- `PeriodStatus`: Open, Closed, Locked
- `ReportType`: BalanceSheet, IncomeStatement, CashFlow, TrialBalance, etc.

**AR/AP Enums:**
- `InvoiceStatus`: Unpaid, PartiallyPaid, Paid, Overdue
- `PaymentType`: CustomerReceipt, VendorPayment
- `PaymentMethod`: Cash, BankTransfer, Cheque, Card, Other
- `NoteType`: Credit, Debit

---

### 2. DTO Layer (`ZR.Model/ERP/Finance/Dto/`)

#### DTO Naming Convention

All DTOs follow a consistent naming pattern:

| Pattern | Purpose | Example |
|---------|---------|---------|
| `{Entity}QueryDto` | Query/Filter DTOs (inherits `PagerInfo`) | `GLAccountQueryDto`, `ARInvoiceQueryDto` |
| `Create{Entity}Dto` | Create operation DTOs | `CreateAccountDto`, `CreateARInvoiceDto` |
| `Update{Entity}Dto` | Update operation DTOs | `UpdateAccountDto`, `UpdateARInvoiceDto` |
| `{Entity}ListDto` | List/Grid display DTOs | `AccountResponseDto`, `ARInvoiceListDto` |
| `{Entity}DetailDto` | Detail view DTOs | `ARInvoiceDetailDto`, `PaymentDetailDto` |

#### DTO Features

- **Validation Attributes:** `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`
- **Excel Export Support:** `[ExcelColumn]` attributes for export functionality
- **Bilingual Support:** Arabic and English error messages
- **Pagination:** Query DTOs inherit from `PagerInfo` for pagination support

**Example DTO Structure:**
```csharp
public class ARInvoiceQueryDto : PagerInfo
{
    public Guid? Id { get; set; }
    public string InvoiceNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public InvoiceStatus? Status { get; set; }
    public DateTime? BeginInvoiceDate { get; set; }
    public DateTime? EndInvoiceDate { get; set; }
    public Guid? TenantId { get; set; }
}
```

---

### 3. Repository Layer (`ZR.Repository/ERP/Finance/`)

#### Repository Pattern

All repositories follow the standard pattern:

**Interface:** `I{Entity}Repository : IBaseRepository<{Entity}>`
**Implementation:** `{Entity}Repository : BaseRepository<{Entity}>, I{Entity}Repository`

#### Repository Features

- **Base Functionality:** Inherits from `BaseRepository<T>` (SqlSugar SimpleClient)
- **Multi-tenant Support:** Automatic tenant filtering via `BaseRepository`
- **Query Methods:** `GetList()`, `GetById()`, `GetByCode()`, custom query methods
- **Pagination:** Returns `PagedInfo<T>` for paginated results
- **Expression Building:** Uses `Expressionable.Create<T>()` for dynamic queries

**Example Repository:**
```csharp
[AppService(ServiceType = typeof(ICustomerRepository), ServiceLifetime = LifeTime.Transient)]
public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public PagedInfo<Customer> GetList(Customer parm, PagerInfo pager)
    {
        var predicate = Expressionable.Create<Customer>();
        predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.CustomerCode), 
            it => it.CustomerCode.Contains(parm.CustomerCode));
        // ... more filters
        
        return Queryable()
            .Where(predicate.ToExpression())
            .OrderBy(x => x.CustomerCode)
            .ToPage(pager);
    }
}
```

#### Repository Methods

**Standard Methods:**
- `GetList(entity, pager)` - Paginated list with filtering
- `GetById(Guid id)` - Get by primary key
- `GetByCode(string code)` - Get by unique code
- `CodeExists(string code, Guid? excludeId)` - Check code uniqueness

**AR/AP Specific Methods:**
- `GetByCustomerId(Guid customerId)` - Get invoices by customer
- `GetByVendorId(Guid vendorId)` - Get invoices by vendor
- `GetOverdueInvoices(DateTime asOfDate)` - Get overdue invoices
- `GetByPaymentId(Guid paymentId)` - Get payment allocations
- `GetByInvoiceId(Guid invoiceId)` - Get allocations for invoice

---

### 4. Service Layer (`ZR.Service/ERP/Finance/`)

#### Service Pattern

All services follow the standard pattern:

**Interface:** `I{Entity}Service : IBaseService<{Entity}>`
**Implementation:** `{Entity}Service : BaseService<{Entity}>, I{Entity}Service`

#### Service Features

- **Dependency Injection:** Uses `[AppService]` attribute for DI registration
- **Business Logic:** Contains all business rules and validations
- **DTO Mapping:** Uses Mapster (`Adapt<T>()`) for entity-DTO conversion
- **Transaction Management:** Uses repository transactions for complex operations
- **Error Handling:** Throws exceptions with bilingual messages

#### Core Finance Services

| Service | Responsibilities |
|---------|------------------|
| `GLAccountService` | Account CRUD, account tree, balance calculation, code validation |
| `GLVoucherService` | Voucher creation, status workflow, entry management, balance validation |
| `GLVoucherEntryService` | Entry CRUD, debit/credit validation |
| `GLJournalService` | Journal management, entry posting |
| `GLPeriodClosingService` | Period closing, status management |
| `GLFinancialReportService` | Report generation, data aggregation |
| `SystemConfigurationService` | Configuration management |

#### AR/AP Services

| Service | Responsibilities |
|---------|------------------|
| `CustomerService` | Customer CRUD, credit limit validation, code uniqueness |
| `VendorService` | Vendor CRUD, code uniqueness |
| `ARInvoiceService` | AR invoice CRUD, status auto-update, credit limit check, GL voucher prep |
| `APInvoiceService` | AP invoice CRUD, status auto-update, GL voucher prep |
| `PaymentService` | Payment creation, allocation to invoices, invoice balance updates, over-allocation prevention |
| `AdjustmentService` | Credit/debit note creation, invoice adjustment application, GL voucher prep |
| `AgingService` | Aging report generation (0-30, 31-60, 61-90, 90+ days), summary reports |

#### Service Business Rules

**AR Invoice Service:**
- Validates credit limit before invoice creation
- Auto-calculates `RemainingAmount = TotalAmount - PaidAmount`
- Auto-updates status: Unpaid → PartiallyPaid → Paid → Overdue (based on DueDate)
- Prevents invoice deletion if payments exist

**Payment Service:**
- Validates payment type matches customer/vendor
- Allocates payments to invoices
- Updates invoice `PaidAmount` and `RemainingAmount`
- Prevents over-allocation (allocation > RemainingAmount)
- Tracks `AllocatedAmount` and `UnallocatedAmount` on payment

**Aging Service:**
- Calculates days overdue: `(AsOfDate - DueDate).Days`
- Categorizes into buckets: 0-30, 31-60, 61-90, 90+ days
- Supports both customer and vendor aging
- Generates summary reports grouped by customer/vendor

---

### 5. Controller Layer (`ZR.Admin.WebApi/Controllers/ERP/Finance/`)

#### Controller Pattern

All controllers follow the standard pattern:

```csharp
[Verify]
[Route("finance/{resource}")]
[ApiController]
public class {Entity}Controller : BaseController
{
    private readonly I{Entity}Service _service;
    
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

- **Authorization:** `[Verify]` attribute for authentication
- **Permission Control:** `[ActionPermissionFilter]` for role-based access
- **Audit Logging:** `[Log]` attribute for operation tracking
- **Standardized Responses:** Uses `SUCCESS()` and `ToResponse()` methods
- **Error Handling:** Try-catch blocks with bilingual error messages
- **Model Validation:** `ModelState.IsValid` checks

#### AR/AP Controllers

**Standard Controllers:**
- `CustomerController` - `/finance/arap/customers`
- `VendorController` - `/finance/arap/vendors`
- `ARInvoiceController` - `/finance/arap/ar-invoices`
- `APInvoiceController` - `/finance/arap/ap-invoices`
- `PaymentController` - `/finance/arap/payments`
- `CreditDebitNoteController` - `/finance/arap/notes`
- `AgingController` - `/finance/arap/aging`

**Special Endpoints:**
- `POST /payments/{id}/allocate` - Allocate payment to invoices
- `POST /notes/{noteId}/apply-to-invoice/{invoiceId}` - Apply adjustment
- `GET /ar-invoices/{id}/gl-voucher-data` - Get GL voucher preparation data
- `GET /aging/customers/report` - Customer aging report
- `GET /aging/vendors/summary` - Vendor aging summary

---

## 🔄 Data Flow

### Create Invoice Flow

```
1. Controller receives CreateARInvoiceDto
   ↓
2. Service validates:
   - Credit limit check
   - Date validation (DueDate >= InvoiceDate)
   ↓
3. Entity creation:
   - RemainingAmount = TotalAmount
   - Status = Unpaid (or Overdue if past due)
   ↓
4. Repository inserts entity
   ↓
5. Service prepares GL voucher data (for external GL system)
   ↓
6. Controller returns created entity
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
   - Update invoice: PaidAmount += allocation, RemainingAmount -= allocation
   - Update invoice status
   - Create PaymentAllocation record
   ↓
4. Update payment: AllocatedAmount, UnallocatedAmount
   ↓
5. Return success
```

### Aging Report Flow

```
1. Controller receives AgingQueryDto (CustomerId/VendorId, AsOfDate)
   ↓
2. Service queries invoices with RemainingAmount > 0
   ↓
3. For each invoice:
   - Calculate days overdue: (AsOfDate - DueDate).Days
   - Categorize into bucket (0-30, 31-60, 61-90, 90+)
   ↓
4. Group by customer/vendor for summary
   ↓
5. Return AgingReportDto list
```

---

## 🔗 Entity Relationships

### AR/AP Module Relationships

```
Customer (1) ──→ (N) ARInvoice
    │
    └──→ GLAccount (AR Control Account)

Vendor (1) ──→ (N) APInvoice
    │
    └──→ GLAccount (AP Control Account)

Payment (1) ──→ (N) PaymentAllocation ──→ (1) ARInvoice or APInvoice
    │
    └──→ GLVoucher (via GLVoucherId)

CreditDebitNote ──→ (1) Customer or Vendor
    │
    └──→ GLVoucher (via GLVoucherId)

ARInvoice ──→ (1) GLVoucher (via GLVoucherId)
APInvoice ──→ (1) GLVoucher (via GLVoucherId)
```

### Core Finance Relationships

```
GLAccount (1) ──→ (N) GLAccount (Parent-Child hierarchy)
GLAccount (1) ──→ (N) GLVoucherEntry
GLAccount (1) ──→ (N) GLJournalEntry

GLVoucher (1) ──→ (N) GLVoucherEntry
GLJournal (1) ──→ (N) GLJournalEntry
```

---

## 📊 Key Features

### 1. Multi-Tenant Support
- All entities include `TenantId` field
- `BaseRepository` automatically filters by tenant
- Tenant isolation at database level

### 2. Audit Trail
- All entities inherit from `SysBase`
- Tracks: `Create_by`, `Create_time`, `Update_by`, `Update_time`
- Automatic timestamp management via SqlSugar

### 3. Bilingual Support
- All user-facing fields have Arabic and English versions
- Error messages in both languages
- DTOs support bilingual validation messages

### 4. Status Management
- Invoice status auto-updates based on payments and due dates
- Voucher status workflow (Draft → Posted)
- Period status (Open → Closed)

### 5. GL Integration
- All financial transactions prepare GL voucher data
- `GLVoucherId` stored on invoices, payments, notes
- GL account references via `GLAccountId`

### 6. Validation Rules
- Credit limit validation for customers
- Over-allocation prevention
- Date validation (DueDate >= InvoiceDate)
- Code uniqueness validation
- Amount validation (must be positive)

---

## 🛠️ Technology Stack

- **ORM:** SqlSugar
- **Mapping:** Mapster
- **Validation:** Data Annotations + FluentValidation (optional)
- **DI:** Custom `[AppService]` attribute
- **Multi-tenancy:** SqlSugar tenant support
- **Pagination:** Custom `PagerInfo` class
- **Excel Export:** MiniExcel

---

## 📝 Naming Conventions

### Files
- Entities: `{EntityName}.cs`
- DTOs: `{EntityName}Dto.cs`
- Repositories: `{EntityName}Repository.cs`, `I{EntityName}Repository.cs`
- Services: `{EntityName}Service.cs`, `I{EntityName}Service.cs`
- Controllers: `{EntityName}Controller.cs`

### Database Tables
- Prefix: `gl_` for GL entities, `arap_` for AR/AP entities
- Snake_case: `gl_account`, `arap_customer`, `arap_ar_invoice`

### API Routes
- Pattern: `/finance/{module}/{resource}`
- Examples: `/finance/accounts`, `/finance/arap/customers`, `/finance/vouchers`

---

## 🔐 Security & Permissions

### Permission Naming Convention
```
finance:{module}:{resource}:{action}
```

**Examples:**
- `finance:account:list`
- `finance:arap:customer:add`
- `finance:voucher:edit`
- `finance:arap:payment:delete`

### Authorization Flow
1. `[Verify]` - Checks authentication
2. `[ActionPermissionFilter]` - Checks permission
3. Service validates business rules
4. Repository enforces tenant isolation

---

## 📈 Performance Considerations

1. **Pagination:** All list endpoints support pagination
2. **Indexing:** Database indexes on `TenantId`, foreign keys, codes
3. **Lazy Loading:** Navigation properties use `[SugarColumn(IsIgnore = true)]`
4. **Query Optimization:** Uses `Expressionable` for efficient SQL generation
5. **Caching:** Repository methods support optional caching

---

## 🧪 Testing Considerations

### Unit Testing
- Test services with mocked repositories
- Test business logic (credit limits, allocations, status updates)
- Test validation rules

### Integration Testing
- Test repository queries
- Test controller endpoints
- Test multi-tenant isolation

### Test Data
- Use test fixtures for common entities
- Isolate test data by tenant
- Clean up after tests

---

## 📚 Additional Resources

### Related Documentation
- `BACKEND_STRUCTURE_DOCUMENTATION.md` - Overall backend architecture
- `SysBase.cs` - Base entity structure
- `BaseRepository.cs` - Repository base implementation

### Key Classes to Review
- `BaseService<T>` - Service base class
- `BaseController` - Controller base class
- `PagerInfo` - Pagination helper
- `Expressionable` - Dynamic query builder

---

## 🎯 Summary

The Finance Module is a comprehensive, well-structured financial management system that:

✅ Follows clean architecture principles  
✅ Supports multi-tenancy  
✅ Provides bilingual support  
✅ Implements robust business rules  
✅ Integrates with GL system  
✅ Maintains audit trails  
✅ Enforces security and permissions  
✅ Optimizes for performance  

The module is production-ready and follows all established patterns and conventions of the system.

