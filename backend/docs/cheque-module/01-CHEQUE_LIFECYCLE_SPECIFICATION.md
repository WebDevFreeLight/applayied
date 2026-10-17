# Cheque Lifecycle Specification Document
# مواصفات دورة حياة الشيك

**Version:** 2.0
**Date:** 2025-12-06
**Module:** Treasury - Cheque Management

---

## 1. Overview

This document specifies the complete cheque lifecycle for both **Outgoing** (صادر) and **Incoming** (وارد) cheques in the Maham ERP Treasury Module.

---

## 2. Cheque States (حالات الشيك)

| Status Code | Status Name | Arabic | Description |
|-------------|-------------|--------|-------------|
| 0 | Issued | صادر | Cheque created/received but not yet deposited to bank |
| 1 | Presented | مقدم | Cheque submitted to bank for clearing |
| 2 | Cleared | مقاص/مسدد | Cheque successfully cleared by bank |
| 3 | Bounced | مرتجع | Cheque returned by bank (NSF, etc.) |
| 4 | Cancelled | ملغي | Cheque cancelled before clearing |

---

## 3. State Transition Diagram

### 3.1 Outgoing Cheques (شيكات صادرة)
```
                    ┌──────────────┐
                    │   ISSUED     │ (Cheque created)
                    │     (0)      │
                    └──────┬───────┘
                           │
              ┌────────────┼────────────┐
              │            │            │
              ▼            ▼            │
     ┌────────────┐  ┌──────────┐       │
     │ PRESENTED  │  │CANCELLED │       │
     │    (1)     │  │   (4)    │◄──────┘
     └──────┬─────┘  └──────────┘
            │              ▲
    ┌───────┼───────┐      │
    │       │       │      │
    ▼       ▼       │      │
┌───────┐ ┌────────┐│      │
│CLEARED│ │BOUNCED ├┼──────┘
│  (2)  │ │  (3)   ││
└───────┘ └────┬───┘│
   [END]       │    │
               │    │ (Re-present)
               └────┘
```

### 3.2 Incoming Cheques (شيكات واردة)
```
                    ┌──────────────┐
                    │   ISSUED     │ (Cheque received from customer)
                    │     (0)      │
                    └──────┬───────┘
                           │
              ┌────────────┼────────────┐
              │            │            │
              ▼            ▼            │
     ┌────────────┐  ┌──────────┐       │
     │ PRESENTED  │  │CANCELLED │       │
     │    (1)     │  │   (4)    │◄──────┘
     └──────┬─────┘  └──────────┘
            │              ▲
    ┌───────┼───────┐      │
    │       │       │      │
    ▼       ▼       │      │
┌───────┐ ┌────────┐│      │
│CLEARED│ │BOUNCED ├┼──────┘
│  (2)  │ │  (3)   ││
└───────┘ └────┬───┘│
   [END]       │    │ (Re-present)
               └────┘
```

---

## 4. Valid State Transitions

| From Status | To Status | Allowed | Action Required |
|-------------|-----------|---------|-----------------|
| Issued (0) | Presented (1) | ✅ Yes | Present() |
| Issued (0) | Cancelled (4) | ✅ Yes | Cancel(reason) |
| Presented (1) | Cleared (2) | ✅ Yes | Clear() |
| Presented (1) | Bounced (3) | ✅ Yes | Bounce(reason, fees) |
| Presented (1) | Cancelled (4) | ✅ Yes | Cancel(reason) |
| Bounced (3) | Presented (1) | ✅ Yes | Present() - Re-present |
| Bounced (3) | Cancelled (4) | ✅ Yes | Cancel(reason) |
| Cleared (2) | Any | ❌ No | Terminal State |
| Cancelled (4) | Any | ❌ No | Terminal State |

---

## 5. Required Dates per Status

| Date Field | When Set | Description |
|------------|----------|-------------|
| IssueDate | On Create | تاريخ الإصدار - When cheque was issued/received |
| DueDate | On Create | تاريخ الاستحقاق - When cheque becomes due |
| PresentedDate | On Present | تاريخ التقديم - When submitted to bank |
| ClearedDate | On Clear | تاريخ التصفية - When bank confirms clearing |
| BouncedDate | On Bounce | تاريخ الارتجاع - When bank returns cheque |
| CancelledDate | On Cancel | تاريخ الإلغاء - When cheque is cancelled |

---

## 6. Validations per Transition

### 6.1 Issue (Create) Validations
```
- ChequeNumber: Required, unique per BankAccount
- BankAccountId: Required, must exist and be Active
- PayeeName: Required
- Amount: Required, > 0
- Currency: Required, 3-char ISO code
- IssueDate: Required
- DueDate: Required, >= IssueDate
- FiscalPeriod: Must be open
- For Incoming: DrawerName recommended
```

### 6.2 Present Validations
```
- Current Status: Must be Issued (0) or Bounced (3)
- BankAccount: Must be Active
- DueDate: Typically should be reached (post-dated handling)
```

### 6.3 Clear Validations
```
- Current Status: Must be Presented (1)
- For Outgoing: Available balance already reserved
- For Incoming: Bank confirmed deposit
```

### 6.4 Bounce Validations
```
- Current Status: Must be Presented (1)
- Reason: Required
- BankFees: Optional (>= 0)
```

### 6.5 Cancel Validations
```
- Current Status: Must be Issued (0), Presented (1), or Bounced (3)
- Reason: Required
- Cannot cancel if Cleared
```

---

## 7. Available Balance Impact Rules

### 7.1 Outgoing Cheques (شيكات صادرة)

| Action | PendingChequesAmount | AvailableBalance | CurrentBalance |
|--------|---------------------|------------------|----------------|
| Issue | +Amount | -Amount (implicit) | No change |
| Present | No change | No change | No change |
| Clear | -Amount | +Amount (implicit) | -Amount via GL |
| Bounce | -Amount | +Amount (implicit) | No change |
| Cancel | -Amount (if presented) | +Amount (implicit) | No change |

**AvailableBalance = CurrentBalance - PendingChequesAmount**

### 7.2 Incoming Cheques (شيكات واردة)

| Action | Impact | Notes |
|--------|--------|-------|
| Issue | No balance impact | Recorded as receivable in GL |
| Present | No balance impact | Awaiting bank clearing |
| Clear | +Amount to Bank | GL: DR Bank, CR Cheques Received |
| Bounce | No balance impact | Receivable restored in GL |
| Cancel | No balance impact | GL reversal if needed |

---

## 8. GL Journal Impact Points

| Action | Creates GL Journal? | For Outgoing | For Incoming |
|--------|---------------------|--------------|--------------|
| Issue | ✅ Yes | DR AP/Expense, CR Cheques Issued | DR Cheques Received, CR AR |
| Present | ❌ No | Status change only | Status change only |
| Clear | ✅ Yes | DR Cheques Issued, CR Bank | DR Bank, CR Cheques Received |
| Bounce | ✅ Yes | DR Bounced Cheques, CR Cheques Issued | DR AR, CR Cheques Received |
| Cancel | ❌ No* | No GL (pre-clearing) | No GL (pre-clearing) |

*Cancel only happens before Cleared state, so no GL reversal needed.

---

## 9. Reversibility Matrix

| Transition | Reversible? | How to Reverse |
|------------|-------------|----------------|
| Issue → Presented | ⚠️ Partial | Cancel (if bank allows) |
| Presented → Cleared | ❌ No | Cannot reverse cleared cheque |
| Presented → Bounced | ✅ Yes | Re-present the cheque |
| Bounced → Presented | ✅ Yes | Re-present again |
| Any → Cancelled | ❌ No | Terminal state |

---

## 10. Post-Dated Cheque Handling

### Definition
A post-dated cheque has `DueDate > CurrentDate`.

### Rules
1. Post-dated cheques can be created (Issued)
2. Warning shown if presenting before DueDate
3. GL Journal for issuance is immediate
4. GL Journal for clearing uses actual ClearedDate

### Status Flow
```
IssueDate: 2025-01-01
DueDate: 2025-02-01

Day 1: Create → Status: Issued
Day 30: Present → Status: Presented (with warning)
Day 31: Clear → Status: Cleared
```

---

## 11. Bounce Handling

### Bounce Count
- `BounceCount` increments each time cheque bounces
- Multiple re-presents allowed (configurable limit)

### Bounce Reasons (Common)
- NSF (Insufficient Funds) - رصيد غير كافي
- Account Closed - حساب مغلق
- Signature Mismatch - توقيع غير مطابق
- Date Error - خطأ في التاريخ
- Amount Mismatch - عدم تطابق المبلغ
- Drawer Stopped Payment - إيقاف الدفع

### Bank Fees
- Captured in `BankFees` field
- Separate GL Journal created for fees
- DR Bank Charges Expense, CR Bank Account

---

## 12. Audit Trail Requirements

Each status change must record:
1. Previous Status
2. New Status
3. Timestamp (UTC)
4. User ID
5. Reason (for Bounce/Cancel)
6. IP Address (optional)

---

## 13. API Endpoints Summary

| Endpoint | Method | Action |
|----------|--------|--------|
| `/cheque` | POST | Create cheque |
| `/cheque/{id}` | PUT | Update cheque (Issued only) |
| `/cheque/{id}` | DELETE | Delete cheque (Issued only) |
| `/cheque/{id}/present` | PUT | Present cheque |
| `/cheque/{id}/clear` | PUT | Clear cheque |
| `/cheque/{id}/bounce` | PUT | Bounce cheque |
| `/cheque/{id}/cancel` | PUT | Cancel cheque |
| `/cheque/pending` | GET | Get pending cheques |
| `/cheque/due` | GET | Get cheques due in period |

---

## 14. Status History Table Schema

```sql
CREATE TABLE cheque_status_history (
    id UUID PRIMARY KEY,
    cheque_id UUID NOT NULL REFERENCES cheque(id),
    previous_status INT NOT NULL,
    new_status INT NOT NULL,
    changed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    changed_by VARCHAR(100) NOT NULL,
    reason VARCHAR(500),
    notes VARCHAR(1000),
    ip_address VARCHAR(50),
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_cheque_status_history_cheque_id ON cheque_status_history(cheque_id);
```

---

## 15. Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-01 | System | Initial specification |
| 2.0 | 2025-12-06 | Claude | Enhanced with full lifecycle, GL integration |
