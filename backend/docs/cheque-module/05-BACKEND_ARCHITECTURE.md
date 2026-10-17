# Backend Architecture Enhancement Design
# تصميم تحسين البنية الخلفية

**Version:** 1.0
**Date:** 2025-12-06
**Module:** Treasury - Cheque Management

---

## 1. Overview

This document details the backend architecture enhancements required to implement the complete cheque lifecycle with status history tracking.

---

## 2. New Database Objects

### 2.1 ChequeStatusHistory Table

```sql
-- Migration: 20251206_Add_ChequeStatusHistory.sql

-- Create the cheque_status_history table
CREATE TABLE IF NOT EXISTS cheque_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cheque_id UUID NOT NULL,
    previous_status INT NOT NULL,
    new_status INT NOT NULL,
    changed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    changed_by VARCHAR(100) NOT NULL,
    reason VARCHAR(500),
    notes VARCHAR(1000),
    bank_fees DECIMAL(18,2),
    bank_reference VARCHAR(100),
    gl_journal_id UUID,
    ip_address VARCHAR(50),

    -- Audit fields
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),

    -- Foreign key
    CONSTRAINT fk_cheque_status_history_cheque
        FOREIGN KEY (cheque_id) REFERENCES cheque(id) ON DELETE CASCADE
);

-- Indexes for performance
CREATE INDEX idx_cheque_status_history_cheque_id
    ON cheque_status_history(cheque_id);
CREATE INDEX idx_cheque_status_history_changed_at
    ON cheque_status_history(changed_at DESC);
CREATE INDEX idx_cheque_status_history_new_status
    ON cheque_status_history(new_status);

-- Comment
COMMENT ON TABLE cheque_status_history IS 'Tracks all status changes for cheques - سجل تغييرات حالة الشيكات';
```

### 2.2 Entity Model Design

```csharp
// File: ZR.Model/ERP/Finance/Treasury/ChequeStatusHistory.cs

using System;
using System.ComponentModel.DataAnnotations;
using SqlSugar;

namespace ZR.Model.ERP.Finance.Treasury
{
    /// <summary>
    /// Cheque Status History - سجل تغييرات حالة الشيك
    /// Tracks all status transitions for audit and timeline display
    /// </summary>
    [SugarTable("cheque_status_history", "Cheque Status History")]
    public class ChequeStatusHistory
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnName = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to Cheque - مرجع الشيك
        /// </summary>
        [Required]
        [SugarColumn(ColumnName = "cheque_id")]
        public Guid ChequeId { get; set; }

        /// <summary>
        /// Previous Status - الحالة السابقة
        /// </summary>
        [SugarColumn(ColumnName = "previous_status")]
        public ChequeStatus PreviousStatus { get; set; }

        /// <summary>
        /// New Status - الحالة الجديدة
        /// </summary>
        [SugarColumn(ColumnName = "new_status")]
        public ChequeStatus NewStatus { get; set; }

        /// <summary>
        /// When the change occurred - وقت التغيير
        /// </summary>
        [SugarColumn(ColumnName = "changed_at")]
        public DateTime ChangedAt { get; set; }

        /// <summary>
        /// User who made the change - المستخدم
        /// </summary>
        [Required]
        [StringLength(100)]
        [SugarColumn(ColumnName = "changed_by")]
        public string ChangedBy { get; set; }

        /// <summary>
        /// Reason for change (bounce/cancel) - سبب التغيير
        /// </summary>
        [StringLength(500)]
        [SugarColumn(ColumnName = "reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Additional notes - ملاحظات إضافية
        /// </summary>
        [StringLength(1000)]
        [SugarColumn(ColumnName = "notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Bank fees if applicable - رسوم البنك
        /// </summary>
        [SugarColumn(ColumnName = "bank_fees")]
        public decimal? BankFees { get; set; }

        /// <summary>
        /// Bank reference number - مرجع البنك
        /// </summary>
        [StringLength(100)]
        [SugarColumn(ColumnName = "bank_reference")]
        public string BankReference { get; set; }

        /// <summary>
        /// GL Journal ID if created - معرف القيد المحاسبي
        /// </summary>
        [SugarColumn(ColumnName = "gl_journal_id")]
        public Guid? GLJournalId { get; set; }

        /// <summary>
        /// IP Address of user - عنوان IP
        /// </summary>
        [StringLength(50)]
        [SugarColumn(ColumnName = "ip_address")]
        public string IPAddress { get; set; }

        /// <summary>
        /// Created timestamp - وقت الإنشاء
        /// </summary>
        [SugarColumn(ColumnName = "created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation property (not mapped)
        [SugarColumn(IsIgnore = true)]
        public Cheque Cheque { get; set; }
    }
}
```

### 2.3 DTO Design

```csharp
// File: ZR.Model/ERP/Finance/Treasury/Dto/ChequeStatusHistoryDto.cs

namespace ZR.Model.ERP.Finance.Treasury.Dto
{
    /// <summary>
    /// Cheque Status History DTO for API responses
    /// </summary>
    public class ChequeStatusHistoryDto
    {
        public Guid Id { get; set; }
        public Guid ChequeId { get; set; }
        public int PreviousStatus { get; set; }
        public string PreviousStatusName { get; set; }
        public string PreviousStatusNameAr { get; set; }
        public int NewStatus { get; set; }
        public string NewStatusName { get; set; }
        public string NewStatusNameAr { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
        public decimal? BankFees { get; set; }
        public string BankReference { get; set; }
        public Guid? GLJournalId { get; set; }
    }

    /// <summary>
    /// Timeline item for frontend display
    /// </summary>
    public class ChequeTimelineItemDto
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public string StatusLabel { get; set; }
        public string StatusLabelAr { get; set; }
        public string StatusColor { get; set; }
        public string StatusIcon { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public decimal? BankFees { get; set; }
        public string BankReference { get; set; }
        public Guid? GLJournalId { get; set; }
    }
}
```

---

## 3. Repository Design

### 3.1 ChequeStatusHistoryRepository

```csharp
// File: ZR.Repository/ERP/Finance/Treasury/ChequeStatusHistoryRepository.cs

using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.ERP.Finance.Treasury;
using ZR.Repository.ERP.Finance.Treasury.Enums;

namespace ZR.Repository.ERP.Finance.Treasury
{
    /// <summary>
    /// Cheque Status History Repository
    /// مستودع سجل تغييرات حالة الشيك
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class ChequeStatusHistoryRepository : BaseRepository<ChequeStatusHistory>
    {
        /// <summary>
        /// Get history for a specific cheque
        /// الحصول على سجل شيك معين
        /// </summary>
        public List<ChequeStatusHistory> GetByChequeId(Guid chequeId)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.ChequeId == chequeId)
                .OrderByDescending(x => x.ChangedAt)
                .ToList();
        }

        /// <summary>
        /// Get latest status change for a cheque
        /// الحصول على آخر تغيير حالة
        /// </summary>
        public ChequeStatusHistory GetLatestByChequeId(Guid chequeId)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.ChequeId == chequeId)
                .OrderByDescending(x => x.ChangedAt)
                .First();
        }

        /// <summary>
        /// Get all history entries for status
        /// الحصول على جميع السجلات لحالة معينة
        /// </summary>
        public List<ChequeStatusHistory> GetByStatus(ChequeStatus status)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.NewStatus == status)
                .OrderByDescending(x => x.ChangedAt)
                .ToList();
        }

        /// <summary>
        /// Get history in date range
        /// الحصول على السجلات في فترة زمنية
        /// </summary>
        public List<ChequeStatusHistory> GetByDateRange(DateTime from, DateTime to)
        {
            return Context.Queryable<ChequeStatusHistory>()
                .Where(x => x.ChangedAt >= from && x.ChangedAt <= to)
                .OrderByDescending(x => x.ChangedAt)
                .ToList();
        }
    }
}
```

---

## 4. Service Layer Enhancement

### 4.1 IChequeService Interface Updates

```csharp
// Add to: ZR.Service/IService/ERP/Finance/Treasury/IChequeService.cs

/// <summary>
/// Get cheque status history - الحصول على سجل تغييرات الحالة
/// </summary>
/// <param name="chequeId">Cheque ID</param>
/// <returns>List of status history entries</returns>
List<ChequeStatusHistoryDto> GetChequeHistory(Guid chequeId);

/// <summary>
/// Get cheque timeline for UI display - الحصول على الجدول الزمني للعرض
/// </summary>
/// <param name="chequeId">Cheque ID</param>
/// <returns>Formatted timeline items</returns>
List<ChequeTimelineItemDto> GetChequeTimeline(Guid chequeId);
```

### 4.2 ChequeService Enhancement

```csharp
// Add to: ZR.Service/ERP/Finance/Treasury/ChequeService.cs

// Inject repository
private readonly ChequeStatusHistoryRepository _historyRepository;

// Constructor update
public ChequeService(
    ChequeRepository chequeRepository,
    ChequeStatusHistoryRepository historyRepository, // Add this
    BankAccountRepository bankAccountRepository,
    // ... other dependencies
)
{
    _chequeRepository = chequeRepository;
    _historyRepository = historyRepository;
    // ...
}

/// <summary>
/// Record status change to history table
/// تسجيل تغيير الحالة في جدول السجل
/// </summary>
private void RecordStatusChange(
    Cheque cheque,
    ChequeStatus previousStatus,
    ChequeStatus newStatus,
    string userId,
    string reason = null,
    string notes = null,
    decimal? bankFees = null,
    string bankReference = null,
    Guid? glJournalId = null)
{
    var history = new ChequeStatusHistory
    {
        Id = Guid.NewGuid(),
        ChequeId = cheque.Id,
        PreviousStatus = previousStatus,
        NewStatus = newStatus,
        ChangedAt = DateTime.UtcNow,
        ChangedBy = userId,
        Reason = reason,
        Notes = notes,
        BankFees = bankFees,
        BankReference = bankReference,
        GLJournalId = glJournalId,
        IPAddress = null, // Could be passed from controller
        CreatedAt = DateTime.UtcNow
    };

    _historyRepository.Insert(history);
}

/// <summary>
/// Get cheque history
/// </summary>
public List<ChequeStatusHistoryDto> GetChequeHistory(Guid chequeId)
{
    var history = _historyRepository.GetByChequeId(chequeId);
    return history.Select(h => new ChequeStatusHistoryDto
    {
        Id = h.Id,
        ChequeId = h.ChequeId,
        PreviousStatus = (int)h.PreviousStatus,
        PreviousStatusName = GetStatusName(h.PreviousStatus, false),
        PreviousStatusNameAr = GetStatusName(h.PreviousStatus, true),
        NewStatus = (int)h.NewStatus,
        NewStatusName = GetStatusName(h.NewStatus, false),
        NewStatusNameAr = GetStatusName(h.NewStatus, true),
        ChangedAt = h.ChangedAt,
        ChangedBy = h.ChangedBy,
        Reason = h.Reason,
        Notes = h.Notes,
        BankFees = h.BankFees,
        BankReference = h.BankReference,
        GLJournalId = h.GLJournalId
    }).ToList();
}

/// <summary>
/// Get timeline for UI
/// </summary>
public List<ChequeTimelineItemDto> GetChequeTimeline(Guid chequeId)
{
    var history = _historyRepository.GetByChequeId(chequeId);
    return history.Select(h => new ChequeTimelineItemDto
    {
        Id = h.Id,
        Status = (int)h.NewStatus,
        StatusLabel = GetStatusName(h.NewStatus, false),
        StatusLabelAr = GetStatusName(h.NewStatus, true),
        StatusColor = GetStatusColor(h.NewStatus),
        StatusIcon = GetStatusIcon(h.NewStatus),
        Description = GetStatusDescription(h, false),
        DescriptionAr = GetStatusDescription(h, true),
        Timestamp = h.ChangedAt,
        UserName = h.ChangedBy,
        Reason = h.Reason,
        BankFees = h.BankFees,
        BankReference = h.BankReference,
        GLJournalId = h.GLJournalId
    }).ToList();
}

// Helper methods
private string GetStatusName(ChequeStatus status, bool arabic)
{
    return status switch
    {
        ChequeStatus.Issued => arabic ? "صادر" : "Issued",
        ChequeStatus.Presented => arabic ? "مقدم" : "Presented",
        ChequeStatus.Cleared => arabic ? "مقاص" : "Cleared",
        ChequeStatus.Bounced => arabic ? "مرتجع" : "Bounced",
        ChequeStatus.Cancelled => arabic ? "ملغي" : "Cancelled",
        _ => arabic ? "غير معروف" : "Unknown"
    };
}

private string GetStatusColor(ChequeStatus status)
{
    return status switch
    {
        ChequeStatus.Issued => "blue",
        ChequeStatus.Presented => "orange",
        ChequeStatus.Cleared => "green",
        ChequeStatus.Bounced => "red",
        ChequeStatus.Cancelled => "gray",
        _ => "gray"
    };
}

private string GetStatusIcon(ChequeStatus status)
{
    return status switch
    {
        ChequeStatus.Issued => "DocumentTextIcon",
        ChequeStatus.Presented => "BuildingLibraryIcon",
        ChequeStatus.Cleared => "CheckCircleIcon",
        ChequeStatus.Bounced => "ExclamationTriangleIcon",
        ChequeStatus.Cancelled => "XCircleIcon",
        _ => "QuestionMarkCircleIcon"
    };
}

private string GetStatusDescription(ChequeStatusHistory h, bool arabic)
{
    return h.NewStatus switch
    {
        ChequeStatus.Issued => arabic
            ? "تم إصدار الشيك"
            : "Cheque issued",
        ChequeStatus.Presented => arabic
            ? "تم تقديم الشيك للبنك"
            : "Cheque presented to bank",
        ChequeStatus.Cleared => arabic
            ? "تمت تصفية الشيك بنجاح"
            : "Cheque cleared successfully",
        ChequeStatus.Bounced => arabic
            ? $"تم ارتجاع الشيك: {h.Reason}"
            : $"Cheque bounced: {h.Reason}",
        ChequeStatus.Cancelled => arabic
            ? $"تم إلغاء الشيك: {h.Reason}"
            : $"Cheque cancelled: {h.Reason}",
        _ => arabic ? "تغيير الحالة" : "Status changed"
    };
}
```

### 4.3 Update Existing Methods to Record History

```csharp
// Update Create() method - add at end before return
RecordStatusChange(
    cheque,
    ChequeStatus.Issued, // Previous is same as new for creation
    ChequeStatus.Issued,
    userId,
    notes: "Cheque created"
);

// Update Present() method - add after status change
var previousStatus = cheque.Status;
cheque.Status = ChequeStatus.Presented;
// ... existing code ...
RecordStatusChange(
    cheque,
    previousStatus,
    ChequeStatus.Presented,
    userId
);

// Update Clear() method - add after status change
var previousStatus = cheque.Status;
cheque.Status = ChequeStatus.Cleared;
// ... existing code ...
RecordStatusChange(
    cheque,
    previousStatus,
    ChequeStatus.Cleared,
    userId,
    glJournalId: journal?.Id
);

// Update Bounce() method - add after status change
var previousStatus = cheque.Status;
cheque.Status = ChequeStatus.Bounced;
// ... existing code ...
RecordStatusChange(
    cheque,
    previousStatus,
    ChequeStatus.Bounced,
    userId,
    reason: reason,
    bankFees: bankFees,
    glJournalId: bounceJournalId != Guid.Empty ? bounceJournalId : null
);

// Update Cancel() method - add after status change
var previousStatus = cheque.Status;
cheque.Status = ChequeStatus.Cancelled;
// ... existing code ...
RecordStatusChange(
    cheque,
    previousStatus,
    ChequeStatus.Cancelled,
    userId,
    reason: reason
);
```

---

## 5. Validation Service Updates

### 5.1 Fix Status Transitions

```csharp
// Update in TreasuryValidationService.cs - ValidateChequeStatusChange()

var validTransitions = new Dictionary<ChequeStatus, List<ChequeStatus>>
{
    { ChequeStatus.Issued, new List<ChequeStatus> {
        ChequeStatus.Presented,
        ChequeStatus.Cancelled
    }},
    { ChequeStatus.Presented, new List<ChequeStatus> {
        ChequeStatus.Cleared,
        ChequeStatus.Bounced,
        ChequeStatus.Cancelled
    }},
    { ChequeStatus.Bounced, new List<ChequeStatus> {
        ChequeStatus.Presented,  // Re-present
        ChequeStatus.Cancelled   // ADD THIS
    }},
    { ChequeStatus.Cleared, new List<ChequeStatus>() }, // Terminal
    { ChequeStatus.Cancelled, new List<ChequeStatus>() } // Terminal
};
```

---

## 6. Controller Updates

### 6.1 Add History Endpoints

```csharp
// Add to: ZR.Admin.WebApi/Controllers/ERP/Finance/Treasury/ChequeController.cs

/// <summary>
/// Get cheque status history
/// الحصول على سجل تغييرات حالة الشيك
/// </summary>
/// <param name="id">Cheque ID</param>
/// <returns>List of history entries</returns>
[HttpGet("{id}/history")]
public IActionResult GetHistory(Guid id)
{
    var history = _chequeService.GetChequeHistory(id);
    return SUCCESS(history);
}

/// <summary>
/// Get cheque timeline for UI display
/// الحصول على الجدول الزمني للعرض
/// </summary>
/// <param name="id">Cheque ID</param>
/// <returns>Formatted timeline items</returns>
[HttpGet("{id}/timeline")]
public IActionResult GetTimeline(Guid id)
{
    var timeline = _chequeService.GetChequeTimeline(id);
    return SUCCESS(timeline);
}
```

---

## 7. Access Control Rules

### 7.1 Permission Matrix

| Action | Permission Required | Notes |
|--------|---------------------|-------|
| Create | treasury:cheque:create | Basic create |
| Update | treasury:cheque:update | Issued only |
| Delete | treasury:cheque:delete | Issued only |
| Present | treasury:cheque:present | Status change |
| Clear | treasury:cheque:clear | Financial action |
| Bounce | treasury:cheque:bounce | Requires reason |
| Cancel | treasury:cheque:cancel | Requires reason |
| View History | treasury:cheque:read | Read-only |

### 7.2 Authorization Attributes

```csharp
// Example controller attributes
[HttpPost]
[ActionPermissionFilter(Permission = "treasury:cheque:create")]
public IActionResult Create([FromBody] CreateChequeDto dto)

[HttpPut("{id}/clear")]
[ActionPermissionFilter(Permission = "treasury:cheque:clear")]
public IActionResult Clear(Guid id)
```

---

## 8. Error Handling

### 8.1 Custom Exceptions

```csharp
// Existing TreasuryException covers these error codes:
public enum TreasuryErrorCode
{
    // Status Transitions
    InvalidStatusTransition = 1001,
    ChequeAlreadyCleared = 1002,
    ChequeAlreadyCancelled = 1003,

    // Balance Errors
    InsufficientBalance = 2001,
    BalanceUpdateFailed = 2002,

    // Validation Errors
    ChequeNotFound = 3001,
    BankAccountNotFound = 3002,
    DuplicateChequeNumber = 3003,

    // Period Errors
    FiscalPeriodClosed = 4001
}
```

---

## 9. Transaction Management

### 9.1 TransactionScope Usage

```csharp
// Example: Clear() method with proper transaction handling
public Cheque Clear(Guid chequeId, string userId)
{
    using (var scope = new TransactionScope(
        TransactionScopeOption.Required,
        new TransactionOptions
        {
            IsolationLevel = IsolationLevel.ReadCommitted
        }))
    {
        // 1. Get and validate cheque
        var cheque = _chequeRepository.GetById(chequeId);
        ValidateStatusTransition(cheque, ChequeStatus.Cleared);

        // 2. Update cheque status
        var previousStatus = cheque.Status;
        cheque.Status = ChequeStatus.Cleared;
        cheque.ClearedDate = DateTime.UtcNow;
        _chequeRepository.Update(cheque);

        // 3. Update bank account
        UpdateBankAccountPendingAmount(cheque);

        // 4. Create GL Journal
        var journal = CreateClearingJournal(cheque, userId);

        // 5. Record history
        RecordStatusChange(cheque, previousStatus, ChequeStatus.Cleared, userId);

        // 6. Commit all changes
        scope.Complete();

        return cheque;
    }
}
```

---

## 10. Implementation Sequence

### Step 1: Database Migration
1. Create SQL migration file
2. Run migration against database
3. Verify table creation

### Step 2: Model & Repository
1. Create ChequeStatusHistory.cs
2. Create ChequeStatusHistoryDto.cs
3. Create ChequeStatusHistoryRepository.cs
4. Update dependency injection

### Step 3: Service Layer
1. Add repository injection to ChequeService
2. Add RecordStatusChange() private method
3. Add GetChequeHistory() public method
4. Add GetChequeTimeline() public method
5. Update IChequeService interface
6. Update Create/Present/Clear/Bounce/Cancel methods

### Step 4: Controller Layer
1. Add /history endpoint
2. Add /timeline endpoint
3. Add authorization attributes

### Step 5: Validation Updates
1. Fix Bounced → Cancelled transition
2. Add validation tests

### Step 6: Testing
1. Unit tests for history recording
2. Integration tests for full lifecycle
3. API tests for endpoints

---

## 11. Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-06 | Claude | Initial architecture design |
