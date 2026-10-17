# ARAP Module - Complete Documentation

## 📋 Table of Contents

1. [Overview](#overview)
2. [Module Structure](#module-structure)
3. [Entities/Models](#entitiesmodels)
4. [Enumerations](#enumerations)
5. [DTOs (Data Transfer Objects)](#dtos-data-transfer-objects)
6. [Repositories](#repositories)
7. [Services](#services)
8. [Controllers](#controllers)
9. [Business Logic & Rules](#business-logic--rules)
10. [API Endpoints](#api-endpoints)
11. [Data Flow Examples](#data-flow-examples)
12. [Entity Relationships](#entity-relationships)

---

## Overview

The **ARAP (Accounts Receivable & Accounts Payable) Module** is a comprehensive financial management system that handles:

- **Accounts Receivable (AR)**: Customer invoices, receipts, and outstanding balances
- **Accounts Payable (AP)**: Vendor invoices, payments, and outstanding balances
- **Payment Management**: Payment allocation to invoices, tracking allocated/unallocated amounts
- **Adjustments**: Credit notes and debit notes for invoice corrections
- **Aging Reports**: Analysis of outstanding invoices by aging buckets (0-30, 31-60, 61-90, 90+ days)

**Location:** `/ERP/Finance/ARAP/`

---

## Module Structure

```
ZR.Model/ERP/Finance/ARAP/
├── Customer.cs                    # Customer entity
├── Vendor.cs                      # Vendor entity
├── ARInvoice.cs                   # Accounts Receivable Invoice
├── APInvoice.cs                   # Accounts Payable Invoice
├── Payment.cs                     # Payment/Receipt entity
├── PaymentAllocation.cs           # Payment to Invoice allocation
├── CreditDebitNote.cs             # Credit/Debit Note entity
├── Dto/                           # Data Transfer Objects
│   ├── CustomerDto.cs
│   ├── VendorDto.cs
│   ├── ARInvoiceDto.cs
│   ├── APInvoiceDto.cs
│   ├── PaymentDto.cs
│   ├── PaymentAllocationDto.cs
│   ├── CreditDebitNoteDto.cs
│   └── AgingDto.cs
└── Enums/                         # Enumerations
    ├── InvoiceStatus.cs
    ├── PaymentType.cs
    ├── PaymentMethod.cs
    └── NoteType.cs

ZR.Repository/ERP/Finance/ARAP/
├── ICustomerRepository.cs / CustomerRepository.cs
├── IVendorRepository.cs / VendorRepository.cs
├── IARInvoiceRepository.cs / ARInvoiceRepository.cs
├── IAPInvoiceRepository.cs / APInvoiceRepository.cs
├── IPaymentRepository.cs / PaymentRepository.cs
├── IPaymentAllocationRepository.cs / PaymentAllocationRepository.cs
└── ICreditDebitNoteRepository.cs / CreditDebitNoteRepository.cs

ZR.Service/ERP/Finance/ARAP/
├── CustomerService.cs
├── VendorService.cs
├── ARInvoiceService.cs
├── APInvoiceService.cs
├── PaymentService.cs
├── AdjustmentService.cs
└── AgingService.cs

ZR.Admin.WebApi/Controllers/ERP/Finance/ARAP/
├── CustomerController.cs
├── VendorController.cs
├── ARInvoiceController.cs
├── APInvoiceController.cs
├── PaymentController.cs
├── CreditDebitNoteController.cs
└── AgingController.cs
```

---

## Entities/Models

### 1. Customer (العميل)

**Table:** `arap_customer`  
**Namespace:** `ZR.Model.ERP.Finance.ARAP`

#### Properties

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| `Id` | `Guid` | Primary key | ✅ |
| `CustomerCode` | `string(50)` | Unique customer code | ✅ |
| `NameAr` | `string(200)` | Name in Arabic | ✅ |
| `NameEn` | `string(200)` | Name in English | ✅ |
| `ContactPerson` | `string(200)` | Contact person name | ❌ |
| `Email` | `string(200)` | Email address | ❌ |
| `Phone` | `string(50)` | Phone number | ❌ |
| `CreditLimit` | `decimal(2)` | Credit limit amount | ✅ (default: 0) |
| `GLAccountId` | `Guid?` | AR Control Account reference | ❌ |
| `IsActive` | `bool` | Active status | ✅ (default: true) |
| `TenantId` | `Guid?` | Tenant identifier | ❌ |
| `CreatedAt` | `DateTime` | Creation timestamp | ✅ |
| `UpdatedAt` | `DateTime?` | Update timestamp | ❌ |
| `CreatedBy` | `string(64)` | Creator username | ❌ |
| `UpdatedBy` | `string(64)` | Updater username | ❌ |

#### Key Features
- Inherits from `SysBase` (audit fields)
- Credit limit validation for invoice creation
- Links to GL Account for AR control account
- Soft delete via `IsActive` flag

---

### 2. Vendor (المورد)

**Table:** `arap_vendor`  
**Namespace:** `ZR.Model.ERP.Finance.ARAP`

#### Properties

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| `Id` | `Guid` | Primary key | ✅ |
| `VendorCode` | `string(50)` | Unique vendor code | ✅ |
| `NameAr` | `string(200)` | Name in Arabic | ✅ |
| `NameEn` | `string(200)` | Name in English | ✅ |
| `ContactPerson` | `string(200)` | Contact person name | ❌ |
| `Email` | `string(200)` | Email address | ❌ |
| `Phone` | `string(50)` | Phone number | ❌ |
| `GLAccountId` | `Guid?` | AP Control Account reference | ❌ |
| `IsActive` | `bool` | Active status | ✅ (default: true) |
| `TenantId` | `Guid?` | Tenant identifier | ❌ |
| `CreatedAt` | `DateTime` | Creation timestamp | ✅ |
| `UpdatedAt` | `DateTime?` | Update timestamp | ❌ |
| `CreatedBy` | `string(64)` | Creator username | ❌ |
| `UpdatedBy` | `string(64)` | Updater username | ❌ |

#### Key Features
- Similar structure to Customer
- Links to GL Account for AP control account
- No credit limit (vendors are paid, not extended credit)

---

### 3. ARInvoice (فاتورة العملاء)

**Table:** `arap_ar_invoice`  
**Namespace:** `ZR.Model.ERP.Finance.ARAP`

#### Properties

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| `Id` | `Guid` | Primary key | ✅ |
| `InvoiceNumber` | `string(50)` | Invoice number | ✅ |
| `CustomerId` | `Guid` | Customer reference | ✅ |
| `InvoiceDate` | `DateTime` | Invoice date | ✅ |
| `DueDate` | `DateTime` | Payment due date | ✅ |
| `TotalAmount` | `decimal(2)` | Total invoice amount | ✅ |
| `PaidAmount` | `decimal(2)` | Amount paid so far | ✅ (default: 0) |
| `RemainingAmount` | `decimal(2)` | Outstanding amount | ✅ |
| `Currency` | `string(10)` | Currency code | ✅ (default: "SAR") |
| `Status` | `InvoiceStatus` | Invoice status enum | ✅ (default: Unpaid) |
| `GLVoucherId` | `Guid?` | GL Voucher reference | ❌ |
| `Notes` | `string(1000)` | Additional notes | ❌ |
| `TenantId` | `Guid?` | Tenant identifier | ❌ |
| `CreatedAt` | `DateTime` | Creation timestamp | ✅ |
| `UpdatedAt` | `DateTime?` | Update timestamp | ❌ |
| `CreatedBy` | `string(64)` | Creator username | ❌ |
| `UpdatedBy` | `string(64)` | Updater username | ❌ |

#### Key Features
- **Auto-calculation:** `RemainingAmount = TotalAmount - PaidAmount`
- **Status auto-update:** Based on `PaidAmount`, `RemainingAmount`, and `DueDate`
- **Credit limit check:** Validated before invoice creation
- **Date validation:** `DueDate >= InvoiceDate`

#### Status Logic
```csharp
if (RemainingAmount <= 0)
    Status = Paid
else if (PaidAmount > 0)
    Status = PartiallyPaid
else
    Status = Unpaid

if (RemainingAmount > 0 && DueDate < DateTime.Now)
    Status = Overdue
```

---

### 4. APInvoice (فاتورة الموردين)

**Table:** `arap_ap_invoice`  
**Namespace:** `ZR.Model.ERP.Finance.ARAP.Entities`

#### Properties

Same structure as `ARInvoice`, but:
- Links to `VendorId` instead of `CustomerId`
- No credit limit validation
- Same status logic applies

---

### 5. Payment (الدفع)

**Table:** `arap_payment`  
**Namespace:** `ZR.Model.ERP.Finance.ARAP`

#### Properties

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| `Id` | `Guid` | Primary key | ✅ |
| `PaymentNumber` | `string(50)` | Payment number | ✅ |
| `PaymentType` | `PaymentType` | CustomerReceipt or VendorPayment | ✅ |
| `CustomerId` | `Guid?` | Customer (for AR payments) | ❌ |
| `VendorId` | `Guid?` | Vendor (for AP payments) | ❌ |
| `PaymentDate` | `DateTime` | Payment date | ✅ |
| `Amount` | `decimal(2)` | Total payment amount | ✅ |
| `AllocatedAmount` | `decimal(2)` | Amount allocated to invoices | ✅ (default: 0) |
| `UnallocatedAmount` | `decimal(2)` | Unallocated amount | ✅ |
| `PaymentMethod` | `PaymentMethod` | Cash, BankTransfer, Cheque, Card, Other | ✅ |
| `ReferenceNo` | `string(100)` | Reference number | ❌ |
| `GLVoucherId` | `Guid?` | GL Voucher reference | ❌ |
| `Notes` | `string(1000)` | Additional notes | ❌ |
| `TenantId` | `Guid?` | Tenant identifier | ❌ |
| `CreatedAt` | `DateTime` | Creation timestamp | ✅ |
| `UpdatedAt` | `DateTime?` | Update timestamp | ❌ |
| `CreatedBy` | `string(64)` | Creator username | ❌ |
| `UpdatedBy` | `string(64)` | Updater username | ❌ |

#### Key Features
- **Dual purpose:** Handles both customer receipts and vendor payments
- **Allocation tracking:** `UnallocatedAmount = Amount - AllocatedAmount`
- **Validation:** CustomerId required for CustomerReceipt, VendorId for VendorPayment

---

### 6. PaymentAllocation (تخصيص الدفع)

**Table:** `arap_payment_allocation`  
**Namespace:** `ZR.Model.ERP.Finance.ARAP`

#### Properties

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| `Id` | `Guid` | Primary key | ✅ |
| `PaymentId` | `Guid` | Payment reference | ✅ |
| `InvoiceId` | `Guid` | Invoice reference (AR or AP) | ✅ |
| `AllocatedAmount` | `decimal(2)` | Allocated amount | ✅ |
| `TenantId` | `Guid?` | Tenant identifier | ❌ |
| `CreatedAt` | `DateTime` | Creation timestamp | ✅ |

#### Key Features
- Links payment to invoice (ARInvoice or APInvoice)
- Prevents over-allocation (validated in service)
- Updates invoice balances when created/deleted

---

### 7. CreditDebitNote (إشعار دائن/مدين)

**Table:** `arap_credit_debit_note`  
**Namespace:** `ZR.Model.ERP.Finance.ARAP`

#### Properties

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| `Id` | `Guid` | Primary key | ✅ |
| `NoteNumber` | `string(50)` | Note number | ✅ |
| `NoteType` | `NoteType` | Credit or Debit | ✅ |
| `CustomerId` | `Guid?` | Customer (for AR adjustments) | ❌ |
| `VendorId` | `Guid?` | Vendor (for AP adjustments) | ❌ |
| `Amount` | `decimal(2)` | Adjustment amount | ✅ |
| `NoteDate` | `DateTime` | Note date | ✅ |
| `Reason` | `string(500)` | Reason for adjustment | ❌ |
| `GLVoucherId` | `Guid?` | GL Voucher reference | ❌ |
| `TenantId` | `Guid?` | Tenant identifier | ❌ |
| `CreatedAt` | `DateTime` | Creation timestamp | ✅ |
| `CreatedBy` | `string(64)` | Creator username | ❌ |

#### Key Features
- **Credit Note:** Reduces amount owed (decreases RemainingAmount)
- **Debit Note:** Increases amount owed (increases RemainingAmount)
- Can be applied to AR or AP invoices
- Must specify either CustomerId or VendorId (not both)

---

## Enumerations

### InvoiceStatus

```csharp
public enum InvoiceStatus
{
    Unpaid = 0,        // غير مدفوعة
    PartiallyPaid = 1, // مدفوعة جزئياً
    Paid = 2,          // مدفوعة
    Overdue = 3        // متأخرة
}
```

**Usage:** Used in `ARInvoice` and `APInvoice` entities

---

### PaymentType

```csharp
public enum PaymentType
{
    CustomerReceipt = 0,  // إيصال استلام من العميل
    VendorPayment = 1      // دفع للمورد
}
```

**Usage:** Determines if payment is for AR (receipt) or AP (payment)

---

### PaymentMethod

```csharp
public enum PaymentMethod
{
    Cash = 0,           // نقدي
    BankTransfer = 1,   // تحويل بنكي
    Cheque = 2,         // شيك
    Card = 3,           // بطاقة
    Other = 4           // أخرى
}
```

**Usage:** Payment method for `Payment` entity

---

### NoteType

```csharp
public enum NoteType
{
    Credit = 0,  // إشعار دائن (reduces amount owed)
    Debit = 1    // إشعار مدين (increases amount owed)
}
```

**Usage:** Type of adjustment in `CreditDebitNote`

---

## DTOs (Data Transfer Objects)

### Customer DTOs

**Location:** `ZR.Model.ERP.Finance.ARAP.Dto.CustomerDto`

#### CustomerQueryDto
- Inherits `PagerInfo` for pagination
- Filter properties: `Id`, `CustomerCode`, `NameAr`, `NameEn`, `IsActive`, `TenantId`, date ranges

#### CreateCustomerDto
- Validation: Required fields, string lengths, email format, credit limit range
- Properties: All customer fields except `Id`, audit fields

#### UpdateCustomerDto
- Same as CreateCustomerDto + `Id` (required)

#### CustomerListDto
- Display properties for list views
- Includes `ExcelColumn` attributes for export

#### CustomerDetailDto
- Extends `CustomerListDto`
- Includes audit fields (`CreatedBy`, `UpdatedBy`)

---

### ARInvoice DTOs

**Location:** `ZR.Model.ERP.Finance.ARAP.Dto.ARInvoiceDto`

#### ARInvoiceQueryDto
- Filters: `InvoiceNumber`, `CustomerId`, `Status`, date ranges, `TenantId`

#### CreateARInvoiceDto
- Validation:
  - `InvoiceNumber`: Required, max 50 chars
  - `CustomerId`: Required
  - `InvoiceDate`: Required
  - `DueDate`: Required, must be >= InvoiceDate
  - `TotalAmount`: Required, must be > 0

#### UpdateARInvoiceDto
- Same as CreateARInvoiceDto + `Id` (required)

#### ARInvoiceListDto
- Display: Invoice details + `CustomerName` (from join)

#### ARInvoiceDetailDto
- Extends `ARInvoiceListDto` + audit fields

---

### Payment DTOs

**Location:** `ZR.Model.ERP.Finance.ARAP.Dto.PaymentDto`

#### PaymentQueryDto
- Filters: `PaymentNumber`, `PaymentType`, `CustomerId`, `VendorId`, `PaymentMethod`, date ranges

#### CreatePaymentDto
- **Special feature:** Includes `Allocations` list for immediate allocation
- Validation:
  - `PaymentType` determines required `CustomerId` or `VendorId`
  - `Amount` must be > 0

#### PaymentListDto
- Display: Payment details + `CustomerName` or `VendorName`

#### PaymentDetailDto
- Extends `PaymentListDto` + `Allocations` list

---

### Aging DTOs

**Location:** `ZR.Model.ERP.Finance.ARAP.Dto.AgingDto`

#### AgingQueryDto
- `CustomerId` or `VendorId` (optional)
- `AsOfDate` (defaults to today)
- `TenantId`

#### AgingReportDto
- Per-invoice aging data:
  - `DaysOverdue`
  - `Age0To30`, `Age31To60`, `Age61To90`, `Age90Plus` (amounts in each bucket)
  - Invoice details

#### AgingSummaryDto
- Per customer/vendor summary:
  - `TotalOutstanding`
  - Aging bucket totals
  - `Invoices` list (detailed)

---

## Repositories

### Repository Pattern

All repositories follow this structure:

```csharp
public interface I{Entity}Repository : IBaseRepository<{Entity}>
{
    PagedInfo<{Entity}> GetList({Entity} parm, PagerInfo pager);
    {Entity} GetById(Guid id);
    // Additional custom methods
}

[AppService(ServiceType = typeof(I{Entity}Repository), ServiceLifetime = LifeTime.Transient)]
public class {Entity}Repository : BaseRepository<{Entity}>, I{Entity}Repository
{
    // Implementation
}
```

### CustomerRepository

**Methods:**
- `GetList(Customer parm, PagerInfo pager)` - Paginated list with filtering
- `GetById(Guid id)` - Get by primary key
- `GetByCode(string code)` - Get by customer code
- `CodeExists(string code, Guid? excludeId)` - Check code uniqueness
- `GetActiveCustomers()` - Get all active customers

### ARInvoiceRepository

**Methods:**
- `GetList(ARInvoice parm, PagerInfo pager)` - Paginated list
- `GetById(Guid id)` - Get by ID
- `GetByInvoiceNumber(string invoiceNumber)` - Get by invoice number
- `GetByCustomerId(Guid customerId)` - Get all invoices for customer
- `GetOverdueInvoices(DateTime asOfDate)` - Get overdue invoices

### PaymentAllocationRepository

**Methods:**
- `GetByPaymentId(Guid paymentId)` - Get allocations for payment
- `GetByInvoiceId(Guid invoiceId)` - Get allocations for invoice
- `DeleteByPaymentId(Guid paymentId)` - Delete all allocations for payment

---

## Services

### Service Pattern

All services follow this structure:

```csharp
[AppService(ServiceType = typeof(I{Entity}Service), ServiceLifetime = LifeTime.Transient)]
public class {Entity}Service : BaseService<{Entity}>, I{Entity}Service
{
    private readonly I{Entity}Repository _repository;
    // Additional dependencies
    
    // CRUD methods
    // Business logic methods
}
```

### 1. CustomerService

**Interface:** `ICustomerService`

**Methods:**

| Method | Description | Business Logic |
|--------|-------------|----------------|
| `GetList(CustomerQueryDto, PagerInfo)` | Paginated customer list | Maps entity to DTO |
| `GetById(Guid)` | Get customer details | Returns `CustomerDetailDto` |
| `Create(CreateCustomerDto, string)` | Create customer | Generates `Id`, sets audit fields |
| `Update(UpdateCustomerDto, string)` | Update customer | Updates specified fields only |
| `Delete(Guid[])` | Delete customers | Soft delete (sets `IsActive = false`), prevents deletion if has invoices |
| `CodeExists(string, Guid?)` | Check code uniqueness | Validates unique customer code |
| `ValidateCreditLimit(Guid, decimal)` | Validate credit limit | Checks: `(TotalOutstanding + NewInvoiceAmount) <= CreditLimit` |

**Key Business Rules:**
- Cannot delete customer if has invoices
- Credit limit validation: `TotalOutstanding + NewInvoice <= CreditLimit`
- Code must be unique per tenant

---

### 2. VendorService

**Interface:** `IVendorService`

**Methods:** Similar to CustomerService (no credit limit validation)

**Key Business Rules:**
- Cannot delete vendor if has invoices
- Code must be unique per tenant

---

### 3. ARInvoiceService

**Interface:** `IARInvoiceService`

**Methods:**

| Method | Description | Business Logic |
|--------|-------------|----------------|
| `GetList(ARInvoiceQueryDto, PagerInfo)` | Paginated invoice list | Includes customer name |
| `GetById(Guid)` | Get invoice details | Includes customer info |
| `Create(CreateARInvoiceDto, string)` | Create invoice | Validates credit limit, dates, sets `RemainingAmount = TotalAmount` |
| `Update(UpdateARInvoiceDto, string)` | Update invoice | Recalculates `RemainingAmount` if total changed |
| `Delete(Guid[])` | Delete invoices | Prevents deletion if has payments |
| `UpdateInvoiceStatus(Guid)` | Update status | Auto-updates based on amounts and due date |
| `PrepareGLVoucherData(Guid)` | Prepare GL data | Returns GL voucher structure for external system |

**Key Business Rules:**
- **Credit Limit Check:** Before creation, validates customer credit limit
- **Date Validation:** `DueDate >= InvoiceDate`
- **Amount Calculation:** `RemainingAmount = TotalAmount - PaidAmount`
- **Status Auto-Update:**
  - `RemainingAmount = 0` → `Paid`
  - `PaidAmount > 0` → `PartiallyPaid`
  - `PaidAmount = 0` → `Unpaid`
  - `RemainingAmount > 0 && DueDate < Today` → `Overdue`
- **Deletion:** Cannot delete if `PaidAmount > 0`

---

### 4. APInvoiceService

**Interface:** `IAPInvoiceService`

**Methods:** Similar to ARInvoiceService (no credit limit check)

**Key Business Rules:**
- Same status logic as ARInvoice
- No credit limit validation
- Date validation: `DueDate >= InvoiceDate`

---

### 5. PaymentService

**Interface:** `IPaymentService`

**Methods:**

| Method | Description | Business Logic |
|--------|-------------|----------------|
| `GetList(PaymentQueryDto, PagerInfo)` | Paginated payment list | Includes customer/vendor names |
| `GetById(Guid)` | Get payment details | Includes allocations list |
| `Create(CreatePaymentDto, string)` | Create payment | Validates payment type, creates allocations if provided |
| `AllocatePayment(Guid, List<CreatePaymentAllocationDto>, string)` | Allocate to invoices | Validates allocations, updates invoice balances, prevents over-allocation |
| `UpdateInvoiceBalances(Guid)` | Revert allocations | Used when deleting/modifying allocations |

**Key Business Rules:**
- **Payment Type Validation:**
  - `CustomerReceipt` → Requires `CustomerId`
  - `VendorPayment` → Requires `VendorId`
- **Allocation Validation:**
  - `AllocatedAmount <= Invoice.RemainingAmount`
  - Cannot allocate negative amounts
  - Total allocations cannot exceed payment amount
- **Invoice Balance Update:**
  - `Invoice.PaidAmount += AllocatedAmount`
  - `Invoice.RemainingAmount -= AllocatedAmount`
  - Updates invoice status
- **Payment Tracking:**
  - `AllocatedAmount = Sum of all allocations`
  - `UnallocatedAmount = Amount - AllocatedAmount`

**Allocation Flow:**
```
1. Delete existing allocations
2. For each allocation:
   - Validate invoice exists
   - Validate allocation <= RemainingAmount
   - Update invoice: PaidAmount += allocation, RemainingAmount -= allocation
   - Update invoice status
   - Create PaymentAllocation record
3. Update payment: AllocatedAmount, UnallocatedAmount
```

---

### 6. AdjustmentService

**Interface:** `IAdjustmentService`

**Methods:**

| Method | Description | Business Logic |
|--------|-------------|----------------|
| `GetList(CreditDebitNoteQueryDto, PagerInfo)` | Paginated notes list | Includes customer/vendor names |
| `GetById(Guid)` | Get note details | Full note information |
| `Create(CreateCreditDebitNoteDto, string)` | Create note | Validates customer or vendor specified |
| `ApplyAdjustmentToInvoice(Guid noteId, Guid invoiceId)` | Apply to invoice | Updates invoice RemainingAmount based on note type |
| `PrepareGLVoucherData(Guid)` | Prepare GL data | Returns GL voucher structure |

**Key Business Rules:**
- **Validation:**
  - Must specify either `CustomerId` or `VendorId` (not both, not neither)
- **Credit Note (NoteType.Credit):**
  - Reduces amount owed: `RemainingAmount = Max(0, RemainingAmount - Amount)`
- **Debit Note (NoteType.Debit):**
  - Increases amount owed: `RemainingAmount += Amount`
- **Invoice Update:**
  - `TotalAmount = PaidAmount + RemainingAmount` (recalculated)
  - Status updated after adjustment

---

### 7. AgingService

**Interface:** `IAgingService`

**Methods:**

| Method | Description | Business Logic |
|--------|-------------|----------------|
| `GetCustomerAgingReport(AgingQueryDto)` | Customer aging report | Calculates aging for AR invoices |
| `GetVendorAgingReport(AgingQueryDto)` | Vendor aging report | Calculates aging for AP invoices |
| `GetCustomerAgingSummary(AgingQueryDto)` | Customer summary | Grouped by customer with totals |
| `GetVendorAgingSummary(AgingQueryDto)` | Vendor summary | Grouped by vendor with totals |

**Aging Calculation Logic:**
```csharp
DaysOverdue = (AsOfDate - DueDate).Days

if (DaysOverdue <= 30)
    Age0To30 = RemainingAmount
else if (DaysOverdue <= 60)
    Age31To60 = RemainingAmount
else if (DaysOverdue <= 90)
    Age61To90 = RemainingAmount
else
    Age90Plus = RemainingAmount
```

**Key Features:**
- Only includes invoices with `RemainingAmount > 0`
- Supports filtering by customer/vendor or all
- Summary reports group by customer/vendor with bucket totals

---

## Controllers

### Controller Pattern

All controllers follow this structure:

```csharp
[Verify]
[Route("finance/arap/{resource}")]
[ApiController]
public class {Entity}Controller : BaseController
{
    private readonly I{Entity}Service _service;
    
    // Standard CRUD endpoints
    // Special endpoints
}
```

### 1. CustomerController

**Route:** `/finance/arap/customers`

**Endpoints:**

| Method | Route | Permission | Description |
|--------|-------|------------|-------------|
| `GET` | `/list` | `finance:arap:customer:list` | Get paginated list |
| `GET` | `/{id}` | `finance:arap:customer:query` | Get by ID |
| `POST` | `/` | `finance:arap:customer:add` | Create customer |
| `PUT` | `/` | `finance:arap:customer:edit` | Update customer |
| `DELETE` | `/{ids}` | `finance:arap:customer:delete` | Delete customers (comma-separated IDs) |
| `GET` | `/check-code` | `finance:arap:customer:query` | Check if code exists |

**Features:**
- Model validation
- Code uniqueness check before create/update
- Soft delete validation (prevents if has invoices)

---

### 2. VendorController

**Route:** `/finance/arap/vendors`

**Endpoints:** Same as CustomerController (replace "customer" with "vendor")

---

### 3. ARInvoiceController

**Route:** `/finance/arap/ar-invoices`

**Endpoints:**

| Method | Route | Permission | Description |
|--------|-------|------------|-------------|
| `GET` | `/list` | `finance:arap:ar-invoice:list` | Get paginated list |
| `GET` | `/{id}` | `finance:arap:ar-invoice:query` | Get by ID |
| `POST` | `/` | `finance:arap:ar-invoice:add` | Create invoice |
| `PUT` | `/` | `finance:arap:ar-invoice:edit` | Update invoice |
| `DELETE` | `/{ids}` | `finance:arap:ar-invoice:delete` | Delete invoices |
| `GET` | `/{id}/gl-voucher-data` | `finance:arap:ar-invoice:query` | Get GL voucher preparation data |

**Features:**
- Try-catch for business rule exceptions
- Credit limit validation errors handled
- Date validation errors handled

---

### 4. APInvoiceController

**Route:** `/finance/arap/ap-invoices`

**Endpoints:** Same as ARInvoiceController (replace "ar-invoice" with "ap-invoice")

---

### 5. PaymentController

**Route:** `/finance/arap/payments`

**Endpoints:**

| Method | Route | Permission | Description |
|--------|-------|------------|-------------|
| `GET` | `/list` | `finance:arap:payment:list` | Get paginated list |
| `GET` | `/{id}` | `finance:arap:payment:query` | Get by ID (includes allocations) |
| `POST` | `/` | `finance:arap:payment:add` | Create payment |
| `POST` | `/{id}/allocate` | `finance:arap:payment:edit` | Allocate payment to invoices |

**Special Endpoint - Allocate:**
```http
POST /finance/arap/payments/{paymentId}/allocate
Body: [
  {
    "InvoiceId": "guid",
    "AllocatedAmount": 1000.00
  }
]
```

**Features:**
- Payment creation can include allocations in DTO
- Separate allocation endpoint for existing payments
- Over-allocation validation errors handled

---

### 6. CreditDebitNoteController

**Route:** `/finance/arap/notes`

**Endpoints:**

| Method | Route | Permission | Description |
|--------|-------|------------|-------------|
| `GET` | `/list` | `finance:arap:note:list` | Get paginated list |
| `GET` | `/{id}` | `finance:arap:note:query` | Get by ID |
| `POST` | `/` | `finance:arap:note:add` | Create note |
| `POST` | `/{noteId}/apply-to-invoice/{invoiceId}` | `finance:arap:note:edit` | Apply adjustment to invoice |
| `GET` | `/{id}/gl-voucher-data` | `finance:arap:note:query` | Get GL voucher preparation data |

**Special Endpoint - Apply Adjustment:**
```http
POST /finance/arap/notes/{noteId}/apply-to-invoice/{invoiceId}
```

---

### 7. AgingController

**Route:** `/finance/arap/aging`

**Endpoints:**

| Method | Route | Permission | Description |
|--------|-------|------------|-------------|
| `GET` | `/customers/report` | `finance:arap:aging:report` | Customer aging report (detailed) |
| `GET` | `/vendors/report` | `finance:arap:aging:report` | Vendor aging report (detailed) |
| `GET` | `/customers/summary` | `finance:arap:aging:report` | Customer aging summary (grouped) |
| `GET` | `/vendors/summary` | `finance:arap:aging:report` | Vendor aging summary (grouped) |

**Query Parameters:**
- `CustomerId` (optional) - Filter by customer
- `VendorId` (optional) - Filter by vendor
- `AsOfDate` (optional) - Aging calculation date (default: today)
- `TenantId` (optional) - Tenant filter

---

## Business Logic & Rules

### 1. Credit Limit Validation

**When:** Before creating AR Invoice

**Rule:**
```
TotalOutstanding = Sum of all customer invoices (RemainingAmount)
NewInvoiceAmount = Invoice being created

if (TotalOutstanding + NewInvoiceAmount > Customer.CreditLimit)
    → Throw Exception: "Credit limit exceeded"
```

**Implementation:** `CustomerService.ValidateCreditLimit()`

---

### 2. Invoice Status Auto-Update

**When:** 
- Invoice creation
- Invoice update
- Payment allocation
- Payment allocation removal
- Adjustment application

**Rules:**
```csharp
if (RemainingAmount <= 0)
    Status = Paid
else if (PaidAmount > 0)
    Status = PartiallyPaid
else
    Status = Unpaid

// Overdue check (after above)
if (RemainingAmount > 0 && DueDate < DateTime.Now)
    Status = Overdue
```

---

### 3. Payment Allocation Rules

**Validation:**
1. Invoice must exist
2. `AllocatedAmount > 0`
3. `AllocatedAmount <= Invoice.RemainingAmount`
4. Payment type must match invoice type (AR invoice → CustomerReceipt, AP invoice → VendorPayment)

**Update Logic:**
```csharp
Invoice.PaidAmount += AllocatedAmount
Invoice.RemainingAmount -= AllocatedAmount
UpdateInvoiceStatus(Invoice)

Payment.AllocatedAmount = Sum of all allocations
Payment.UnallocatedAmount = Payment.Amount - Payment.AllocatedAmount
```

---

### 4. Date Validation

**Rule:** `DueDate >= InvoiceDate`

**Applied to:**
- AR Invoice creation/update
- AP Invoice creation/update

**Error:** "Due date must be greater than or equal to invoice date"

---

### 5. Deletion Rules

**Customer/Vendor:**
- Cannot delete if has invoices (checks `GetByCustomerId()` or `GetByVendorId()`)

**Invoice:**
- Cannot delete if `PaidAmount > 0` (has payments)

**Payment:**
- Can be deleted (allocations can be removed first)

---

### 6. Adjustment Application

**Credit Note (reduces amount):**
```csharp
Invoice.RemainingAmount = Math.Max(0, Invoice.RemainingAmount - Note.Amount)
Invoice.TotalAmount = Invoice.PaidAmount + Invoice.RemainingAmount
```

**Debit Note (increases amount):**
```csharp
Invoice.RemainingAmount += Note.Amount
Invoice.TotalAmount = Invoice.PaidAmount + Invoice.RemainingAmount
```

---

## API Endpoints

### Base URL
```
/finance/arap/
```

### Complete Endpoint List

#### Customers
```
GET    /finance/arap/customers/list
GET    /finance/arap/customers/{id}
POST   /finance/arap/customers
PUT    /finance/arap/customers
DELETE /finance/arap/customers/{ids}
GET    /finance/arap/customers/check-code?code={code}&excludeId={id}
```

#### Vendors
```
GET    /finance/arap/vendors/list
GET    /finance/arap/vendors/{id}
POST   /finance/arap/vendors
PUT    /finance/arap/vendors
DELETE /finance/arap/vendors/{ids}
GET    /finance/arap/vendors/check-code?code={code}&excludeId={id}
```

#### AR Invoices
```
GET    /finance/arap/ar-invoices/list
GET    /finance/arap/ar-invoices/{id}
POST   /finance/arap/ar-invoices
PUT    /finance/arap/ar-invoices
DELETE /finance/arap/ar-invoices/{ids}
GET    /finance/arap/ar-invoices/{id}/gl-voucher-data
```

#### AP Invoices
```
GET    /finance/arap/ap-invoices/list
GET    /finance/arap/ap-invoices/{id}
POST   /finance/arap/ap-invoices
PUT    /finance/arap/ap-invoices
DELETE /finance/arap/ap-invoices/{ids}
GET    /finance/arap/ap-invoices/{id}/gl-voucher-data
```

#### Payments
```
GET    /finance/arap/payments/list
GET    /finance/arap/payments/{id}
POST   /finance/arap/payments
POST   /finance/arap/payments/{id}/allocate
```

#### Credit/Debit Notes
```
GET    /finance/arap/notes/list
GET    /finance/arap/notes/{id}
POST   /finance/arap/notes
POST   /finance/arap/notes/{noteId}/apply-to-invoice/{invoiceId}
GET    /finance/arap/notes/{id}/gl-voucher-data
```

#### Aging Reports
```
GET    /finance/arap/aging/customers/report
GET    /finance/arap/aging/vendors/report
GET    /finance/arap/aging/customers/summary
GET    /finance/arap/aging/vendors/summary
```

---

## Data Flow Examples

### Example 1: Create AR Invoice with Credit Limit Check

```
1. POST /finance/arap/ar-invoices
   Body: {
     "InvoiceNumber": "INV-001",
     "CustomerId": "guid",
     "InvoiceDate": "2024-01-15",
     "DueDate": "2024-02-15",
     "TotalAmount": 5000.00
   }

2. Controller validates ModelState

3. Service.ARInvoiceService.Create():
   a. Validate credit limit:
      - Get customer
      - Get all customer invoices
      - Calculate: TotalOutstanding = Sum(RemainingAmount)
      - Check: (TotalOutstanding + 5000) <= CreditLimit
      - If exceeds → Throw exception
   
   b. Validate dates:
      - Check: DueDate >= InvoiceDate
      - If invalid → Throw exception
   
   c. Create entity:
      - RemainingAmount = 5000.00
      - PaidAmount = 0
      - Status = Unpaid (or Overdue if past due)
   
   d. Update status based on due date
   
   e. Insert to database

4. Return created invoice
```

---

### Example 2: Payment Allocation Flow

```
1. POST /finance/arap/payments
   Body: {
     "PaymentNumber": "PAY-001",
     "PaymentType": "CustomerReceipt",
     "CustomerId": "guid",
     "PaymentDate": "2024-01-20",
     "Amount": 3000.00,
     "PaymentMethod": "BankTransfer",
     "Allocations": [
       {
         "InvoiceId": "invoice-guid-1",
         "AllocatedAmount": 2000.00
       },
       {
         "InvoiceId": "invoice-guid-2",
         "AllocatedAmount": 1000.00
       }
     ]
   }

2. Service.PaymentService.Create():
   a. Validate payment type matches customer
   b. Create payment:
      - UnallocatedAmount = 3000.00
      - AllocatedAmount = 0
   
   c. Call AllocatePayment():
      - For each allocation:
        * Get invoice
        * Validate: AllocatedAmount <= Invoice.RemainingAmount
        * Update invoice:
          - PaidAmount += AllocatedAmount
          - RemainingAmount -= AllocatedAmount
          - Update status
        * Create PaymentAllocation record
      
      - Update payment:
        - AllocatedAmount = 3000.00
        - UnallocatedAmount = 0.00

3. Return created payment with allocations
```

---

### Example 3: Aging Report Generation

```
1. GET /finance/arap/aging/customers/report?CustomerId={guid}&AsOfDate=2024-01-31

2. Service.AgingService.GetCustomerAgingReport():
   a. Get all customer invoices (or all if no CustomerId)
   b. Filter: RemainingAmount > 0
   
   c. For each invoice:
      - Calculate: DaysOverdue = (AsOfDate - DueDate).Days
      - Categorize:
        * if DaysOverdue <= 30: Age0To30 = RemainingAmount
        * else if DaysOverdue <= 60: Age31To60 = RemainingAmount
        * else if DaysOverdue <= 90: Age61To90 = RemainingAmount
        * else: Age90Plus = RemainingAmount
      
      - Create AgingReportDto

3. Return list of AgingReportDto
```

---

## Entity Relationships

### ER Diagram (Text Representation)

```
Customer (1) ────────< (N) ARInvoice
    │                        │
    │                        ├──> GLVoucher (via GLVoucherId)
    │                        │
    └──> GLAccount (AR Control Account)

Vendor (1) ──────────< (N) APInvoice
    │                        │
    │                        ├──> GLVoucher (via GLVoucherId)
    │                        │
    └──> GLAccount (AP Control Account)

Payment (1) ────────< (N) PaymentAllocation ───> (1) ARInvoice or APInvoice
    │
    └──> GLVoucher (via GLVoucherId)

CreditDebitNote
    ├──> (1) Customer (optional)
    ├──> (1) Vendor (optional)
    └──> GLVoucher (via GLVoucherId)
```

### Relationship Details

1. **Customer → ARInvoice** (1:N)
   - One customer can have many invoices
   - Invoice must have a customer
   - Foreign key: `ARInvoice.CustomerId`

2. **Vendor → APInvoice** (1:N)
   - One vendor can have many invoices
   - Invoice must have a vendor
   - Foreign key: `APInvoice.VendorId`

3. **Payment → PaymentAllocation** (1:N)
   - One payment can be allocated to multiple invoices
   - Foreign key: `PaymentAllocation.PaymentId`

4. **PaymentAllocation → Invoice** (N:1)
   - One allocation links to one invoice (AR or AP)
   - Foreign key: `PaymentAllocation.InvoiceId`

5. **Customer/Vendor → GLAccount** (N:1)
   - Links to GL control account
   - Foreign key: `Customer.GLAccountId` or `Vendor.GLAccountId`

6. **Invoice/Payment/Note → GLVoucher** (N:1)
   - Links to GL voucher for accounting integration
   - Foreign key: `GLVoucherId` (nullable)

---

## Summary

The ARAP Module provides:

✅ **Complete AR/AP Management**
- Customer and Vendor master data
- Invoice creation and tracking
- Payment processing and allocation
- Adjustment handling (credit/debit notes)

✅ **Business Intelligence**
- Aging reports (0-30, 31-60, 61-90, 90+ days)
- Summary reports by customer/vendor
- Outstanding balance tracking

✅ **Business Rules**
- Credit limit validation
- Automatic status updates
- Over-allocation prevention
- Date validation

✅ **GL Integration**
- GL voucher data preparation
- GL account references
- Voucher ID tracking

✅ **Multi-tenant Support**
- Tenant isolation
- Tenant-specific data

✅ **Audit Trail**
- Creation/update tracking
- User tracking
- Timestamp management

The module is production-ready and follows all established patterns and conventions.

