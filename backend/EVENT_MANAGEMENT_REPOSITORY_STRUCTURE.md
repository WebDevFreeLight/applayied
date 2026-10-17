# Event Management Repository Structure Documentation

## Overview

This document explains the structure of Event Management repositories and why they differ from Finance repositories in terms of method implementation.

---

## 📁 Event Management Repository Structure

### Location
`ZR.Repository/EventManagement/`

### Repository Files

| Repository | Entity | Description |
|------------|--------|-------------|
| `EventRepository` | `Event` | Main event information |
| `EventVisitorRepository` | `EventVisitor` | Event visitors/attendees |
| `EventTicketRepository` | `EventTicket` | Event tickets |
| `EventBoothRepository` | `EventBooth` | Event booths |
| `EventContractRepository` | `EventContract` | Event contracts |
| `EventPaymentRepository` | `EventPayment` | Event payments |
| `EventInvoiceRepository` | `EventInvoice` | Event invoices |
| `EventPermitRepository` | `EventPermit` | Event permits |
| `EventIncidentRepository` | `EventIncident` | Event incidents |
| `EventZoneRepository` | `EventZone` | Event zones |
| `EventGateRepository` | `EventGate` | Event gates |
| `EventLocationRepository` | `EventLocation` | Event locations |
| `EventTypeRepository` | `EventType` | Event types (lookup) |
| `EventStatusRepository` | `EventStatus` | Event statuses (lookup) |
| `BoothTypeRepository` | `BoothType` | Booth types (lookup) |
| `VisitorTypeRepository` | `VisitorType` | Visitor types (lookup) |
| `TicketTypeRepository` | `TicketType` | Ticket types (lookup) |
| `PaymentMethodRepository` | `PaymentMethod` | Payment methods (lookup) |
| `PermitTypeRepository` | `PermitType` | Permit types (lookup) |
| `EventZoneTypeRepository` | `EventZoneType` | Zone types (lookup) |
| `EventGateTypeRepository` | `EventGateType` | Gate types (lookup) |
| `IncidentTypeRepository` | `IncidentType` | Incident types (lookup) |
| `TicketCheckinRepository` | `TicketCheckin` | Ticket check-in records |
| `EventTenantRepository` | `EventTenant` | Event tenant associations |
| `EventContractHistoryRepository` | `EventContractHistory` | Contract history |
| `EventBoothHistoryRepository` | `EventBoothHistory` | Booth history |

---

## 🏗️ Repository Implementation Pattern

### Event Management Repository Example

```csharp
using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository.EventManagement
{
    /// <summary>
    /// Event Information - معلومات الفعالية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventRepository : BaseRepository<Event>
    {
        // Empty class - all functionality comes from BaseRepository<T>
    }
}
```

**Key Characteristics:**
- ✅ Inherits from `BaseRepository<Event>`
- ✅ No custom methods defined
- ✅ Uses `[AppService]` attribute for dependency injection
- ✅ All functionality inherited from base class

---

## 🔍 Why Event Management Repositories Are Simple

### 1. **Inheritance from BaseRepository<T>**

Event Management repositories inherit all functionality from `BaseRepository<T>`, which provides:

#### **Insert Operations:**
```csharp
int Add(T t, bool ignoreNull = true)
int Insert(List<T> t)
int Insert(T parm, Expression<Func<T, object>> iClumns = null, bool ignoreNull = true)
IInsertable<T> Insertable(T t)
```

#### **Update Operations:**
```csharp
int Update(T entity, bool ignoreNullColumns = false, object data = null)
int Update(T entity, Expression<Func<T, object>> expression, bool ignoreAllNull = false)
int Update(T entity, Expression<Func<T, object>> expression, Expression<Func<T, bool>> where)
int Update(Expression<Func<T, bool>> where, Expression<Func<T, T>> columns)
Task<int> UpdateAsync(Expression<Func<T, bool>> where, Expression<Func<T, T>> columns)
```

#### **Delete Operations:**
```csharp
IDeleteable<T> Deleteable()
int Delete(object id, string title = "")
int DeleteTable()
bool Truncate()
```

#### **Query Operations:**
```csharp
ISugarQueryable<T> Queryable()                    // LINQ query builder
T GetId(object pkValue)                           // Get by primary key
PagedInfo<T> GetPages(Expression<Func<T, bool>> where, PagerInfo parm)  // Paginated list
List<T> GetAll(bool useCache = false, int cacheSecond = 3600)           // Get all
bool Any(Expression<Func<T, bool>> expression)    // Check existence
List<T> SqlQueryToList(string sql, object obj = null)  // Raw SQL query
```

#### **Transaction Operations:**
```csharp
DbResult<bool> UseTran(Action action)
DbResult<bool> UseTran(ISqlSugarClient client, Action action)
bool UseTran2(Action action)
Task<DbResult<bool>> UseTranAsync(Func<Task> asyncAction)
```

#### **Stored Procedure Operations:**
```csharp
DataTable UseStoredProcedureToDataTable(string procedureName, List<SugarParameter> parameters)
(DataTable, List<SugarParameter>) UseStoredProcedureToTuple(string procedureName, List<SugarParameter> parameters)
```

### 2. **Simple Entity Structure**

Event Management entities are typically:
- **Flat structures** - No complex hierarchies
- **Standard CRUD operations** - Create, Read, Update, Delete
- **No complex business logic** - Simple data storage and retrieval
- **No special validations** - Basic data integrity only

**Example Usage:**
```csharp
// In Service Layer
var eventRepository = new EventRepository();

// Get by ID (from BaseRepository)
var event = eventRepository.GetId(eventId);

// Get paginated list (from BaseRepository)
var events = eventRepository.GetPages(
    e => e.Status == "Active", 
    new PagerInfo { PageNum = 1, PageSize = 10 }
);

// Insert (from BaseRepository)
eventRepository.Add(newEvent);

// Update (from BaseRepository)
eventRepository.Update(existingEvent);

// Delete (from BaseRepository)
eventRepository.Delete(eventId);
```

---

## 🆚 Comparison: Event Management vs Finance Repositories

### Event Management Repository (Simple)

```csharp
[AppService(ServiceLifetime = LifeTime.Transient)]
public class EventRepository : BaseRepository<Event>
{
    // Empty - uses BaseRepository methods only
}
```

**Available Methods (from BaseRepository):**
- `GetId(id)` - Get by primary key
- `GetPages(where, pager)` - Paginated list
- `Queryable()` - LINQ query builder
- `Add(entity)` - Insert
- `Update(entity)` - Update
- `Delete(id)` - Delete
- `GetAll()` - Get all records

### Finance Repository (Complex)

```csharp
[AppService(ServiceType = typeof(IGLAccountRepository), ServiceLifetime = LifeTime.Transient)]
public class GLAccountRepository : BaseRepository<GLAccount>, IGLAccountRepository
{
    // Custom methods for business logic
    public PagedInfo<GLAccount> GetList(GLAccount parm, PagerInfo pager) { ... }
    public GLAccount GetById(Guid id) { ... }
    public GLAccount GetByCode(string accountNumber) { ... }
    public bool CodeExists(string accountNumber, Guid? excludeAccountId = null) { ... }
    public List<GLAccount> GetChildAccounts(Guid parentAccountId) { ... }
    public List<GLAccount> GetRootAccounts() { ... }
    public List<GLAccount> GetLeafAccounts() { ... }
    public bool HasChildren(Guid accountId) { ... }
    public bool WouldCreateCircularReference(Guid accountId, Guid? newParentId) { ... }
    // ... many more custom methods
}
```

**Why Finance Needs Custom Methods:**

1. **Hierarchical Structure**
   - Accounts have parent-child relationships
   - Need methods like `GetChildAccounts()`, `GetRootAccounts()`, `GetAllDescendants()`
   - Must prevent circular references

2. **Complex Validations**
   - `CodeExists()` - Check account number uniqueness
   - `FullAccountNumberExists()` - Check full account number uniqueness
   - `WouldCreateCircularReference()` - Validate hierarchy

3. **Business-Specific Queries**
   - `GetByType()` - Get accounts by account type
   - `GetCashAccounts()` - Get only cash accounts
   - `GetBankAccounts()` - Get only bank accounts
   - `GetLeafAccounts()` - Get only postable accounts

4. **Hierarchy Management**
   - `UpdateParentIsLeaf()` - Update parent's leaf status
   - `UpdateAccountPath()` - Update materialized path
   - `RecalculateAllPaths()` - Recalculate all paths
   - `IncrementChildCount()` - Update child counts

5. **Performance Optimization**
   - Materialized paths for efficient tree queries
   - Cached counts (ChildCount, DescendantCount)
   - Optimized queries for specific use cases

---

## 📊 Where Methods Come From

### Inheritance Chain

```
BaseRepository<T>
    ↓
EventRepository : BaseRepository<Event>
    ↓
All BaseRepository methods are available
```

### Method Source Breakdown

| Method Category | Source | Available In |
|----------------|--------|--------------|
| **Basic CRUD** | `BaseRepository<T>` | ✅ Event Management<br>✅ Finance |
| **Pagination** | `BaseRepository<T>` | ✅ Event Management<br>✅ Finance |
| **LINQ Queries** | `BaseRepository<T>` | ✅ Event Management<br>✅ Finance |
| **Transactions** | `BaseRepository<T>` | ✅ Event Management<br>✅ Finance |
| **Custom Business Logic** | Custom Implementation | ❌ Event Management<br>✅ Finance |

### BaseRepository Methods Available to All Repositories

**From `BaseRepository<T>` (inherited by all):**
```csharp
// Query
ISugarQueryable<T> Queryable()
T GetId(object pkValue)
PagedInfo<T> GetPages(Expression<Func<T, bool>> where, PagerInfo parm)
List<T> GetAll(bool useCache = false, int cacheSecond = 3600)
bool Any(Expression<Func<T, bool>> expression)

// Insert
int Add(T t, bool ignoreNull = true)
int Insert(List<T> t)
IInsertable<T> Insertable(T t)

// Update
int Update(T entity, bool ignoreNullColumns = false, object data = null)
int Update(T entity, Expression<Func<T, object>> expression, bool ignoreAllNull = false)

// Delete
int Delete(object id, string title = "")
IDeleteable<T> Deleteable()

// Transactions
DbResult<bool> UseTran(Action action)
Task<DbResult<bool>> UseTranAsync(Func<Task> asyncAction)
```

**From `ISimpleClient<T>` (SqlSugar base class):**
```csharp
T GetById(object id)
T GetFirst(Expression<Func<T, bool>> where)
List<T> GetList(Expression<Func<T, bool>> where)
bool InsertRange(List<T> entities)
bool UpdateRange(List<T> entities)
bool DeleteRange(List<T> entities)
// ... and many more SqlSugar methods
```

---

## 🎯 When to Add Custom Methods

### ✅ Add Custom Methods When:

1. **Complex Business Logic Required**
   - Hierarchy management (parent-child relationships)
   - Circular reference prevention
   - Complex validations

2. **Performance Optimization Needed**
   - Materialized paths
   - Cached calculations
   - Optimized queries for specific use cases

3. **Domain-Specific Queries**
   - Get by business code (not just primary key)
   - Filter by multiple complex conditions
   - Aggregate queries

4. **Reusable Business Operations**
   - Methods used across multiple services
   - Standardized query patterns
   - Common validations

### ❌ Don't Add Custom Methods When:

1. **Simple CRUD Operations**
   - Standard insert, update, delete
   - Basic queries by primary key
   - Simple pagination

2. **BaseRepository Methods Sufficient**
   - `GetId()` for primary key lookup
   - `GetPages()` for pagination
   - `Queryable()` for LINQ queries

3. **One-Time Queries**
   - Specific queries used only once
   - Better handled in service layer

---

## 💡 Usage Examples

### Event Management Repository Usage

```csharp
// In Service Layer
public class EventService
{
    private readonly EventRepository _eventRepository;
    
    public EventService(EventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }
    
    // Get by ID - uses BaseRepository.GetId()
    public Event GetEventById(int eventId)
    {
        return _eventRepository.GetId(eventId);
    }
    
    // Get paginated list - uses BaseRepository.GetPages()
    public PagedInfo<Event> GetEvents(EventQueryDto query, PagerInfo pager)
    {
        var predicate = Expressionable.Create<Event>();
        predicate = predicate.AndIF(!string.IsNullOrEmpty(query.EventName), 
            e => e.EventName.Contains(query.EventName));
        predicate = predicate.AndIF(query.Status.HasValue, 
            e => e.Status == query.Status.Value);
        
        return _eventRepository.GetPages(predicate.ToExpression(), pager);
    }
    
    // Create - uses BaseRepository.Add()
    public int CreateEvent(Event event)
    {
        return _eventRepository.Add(event);
    }
    
    // Update - uses BaseRepository.Update()
    public int UpdateEvent(Event event)
    {
        return _eventRepository.Update(event);
    }
    
    // Delete - uses BaseRepository.Delete()
    public int DeleteEvent(int eventId)
    {
        return _eventRepository.Delete(eventId);
    }
    
    // Custom query using Queryable()
    public List<Event> GetActiveEvents()
    {
        return _eventRepository.Queryable()
            .Where(e => e.Status == "Active" && e.StartDate >= DateTime.Now)
            .OrderBy(e => e.StartDate)
            .ToList();
    }
}
```

### Finance Repository Usage (with Custom Methods)

```csharp
// In Service Layer
public class GLAccountService
{
    private readonly IGLAccountRepository _accountRepository;
    
    public GLAccountService(IGLAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }
    
    // Uses custom GetList() method
    public PagedInfo<AccountResponseDto> GetList(GLAccountQueryDto query, PagerInfo pager)
    {
        return _accountRepository.GetList(query, pager);
    }
    
    // Uses custom GetByCode() method
    public AccountResponseDto GetByCode(string accountNumber)
    {
        var account = _accountRepository.GetByCode(accountNumber);
        return account?.Adapt<AccountResponseDto>();
    }
    
    // Uses custom validation methods
    public void ValidateAccount(CreateAccountDto dto)
    {
        if (_accountRepository.CodeExists(dto.AccountNumber))
        {
            throw new Exception("Account number already exists");
        }
        
        if (dto.ParentAccountId.HasValue)
        {
            if (_accountRepository.WouldCreateCircularReference(
                dto.ParentAccountId.Value, dto.ParentAccountId))
            {
                throw new Exception("Circular reference detected");
            }
        }
    }
    
    // Uses custom hierarchy methods
    public List<AccountResponseDto> GetAccountTree()
    {
        var rootAccounts = _accountRepository.GetRootAccounts();
        return BuildTree(rootAccounts);
    }
}
```

---

## 📝 Summary

### Event Management Repositories

✅ **Simple Structure:**
- Inherit from `BaseRepository<T>`
- No custom methods
- Use inherited CRUD operations
- Suitable for simple entities

✅ **Methods Source:**
- All methods from `BaseRepository<T>`
- All methods from `ISimpleClient<T>` (SqlSugar)
- LINQ queries via `Queryable()`

### Finance Repositories

✅ **Complex Structure:**
- Inherit from `BaseRepository<T>`
- Implement custom interface `I{Entity}Repository`
- Add custom business logic methods
- Suitable for complex entities with hierarchies

✅ **Methods Source:**
- Base methods from `BaseRepository<T>`
- Custom methods for business logic
- Hierarchy management methods
- Validation methods
- Performance optimization methods

### Key Takeaway

**Event Management repositories are simple because:**
1. Entities don't require complex business logic
2. Standard CRUD operations are sufficient
3. BaseRepository provides all needed functionality
4. No hierarchies or complex validations needed

**Finance repositories are complex because:**
1. Entities have hierarchical structures
2. Complex business rules and validations
3. Performance optimizations needed
4. Domain-specific queries required

Both approaches are valid and follow the **YAGNI principle** (You Aren't Gonna Need It) - only add complexity when needed!

---

**Last Updated**: 2024  
**Version**: 1.0





























