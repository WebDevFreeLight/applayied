# Cheque Accounting Rules Document
# قواعد المحاسبة للشيكات

**Version:** 2.0
**Date:** 2025-12-06
**Module:** Treasury - Cheque Management

---

## 1. Overview

This document defines the exact GL (General Ledger) journal entries for all cheque lifecycle events, balance update rules, and integration with PaymentHub and BalanceLedger.

---

## 2. GL Account Structure for Cheques

### 2.1 Required GL Accounts

| GL Account | Arabic | Type | Normal Balance | Purpose |
|------------|--------|------|----------------|---------|
| Cheques Issued | شيكات صادرة | Liability | Credit | Post-dated outgoing cheques |
| Cheques Received | شيكات واردة | Asset | Debit | Incoming cheques in transit |
| Bounced Cheques | شيكات مرتجعة | Asset | Debit | Bounced cheque receivables |
| Bank Account | البنك | Asset | Debit | Main bank account |
| Accounts Payable | الدائنون | Liability | Credit | Vendor payables |
| Accounts Receivable | المدينون | Asset | Debit | Customer receivables |
| Bank Charges | مصاريف بنكية | Expense | Debit | Bank fees on bounced cheques |

### 2.2 Account Mapping Configuration

```json
{
  "DefaultChequesIssuedGLId": "GL account for Cheques Issued liability",
  "DefaultChequesReceivedGLId": "GL account for Cheques Received asset",
  "DefaultBouncedChequesGLId": "GL account for Bounced Cheques",
  "DefaultAccountsPayableGLId": "GL account for AP",
  "DefaultAccountsReceivableGLId": "GL account for AR",
  "DefaultBankChargesGLId": "GL account for Bank Charges expense"
}
```

---

## 3. Outgoing Cheques (شيكات صادرة) - Journal Entries

### 3.1 Issuance (إصدار الشيك)

**When:** Cheque is created (Status: Issued)
**Accounting Treatment:** Record the payable and the cheque liability

```
┌─────────────────────────────────────────────────────────────┐
│ JOURNAL ENTRY: Outgoing Cheque Issuance                     │
├─────────────────────────────────────────────────────────────┤
│ DR  Accounts Payable / Expense      XXX                     │
│     CR  Cheques Issued (Liability)      XXX                 │
├─────────────────────────────────────────────────────────────┤
│ Reference: Cheque Number                                    │
│ Description: إصدار شيك رقم {ChequeNumber} للمستفيد {PayeeName}│
└─────────────────────────────────────────────────────────────┘
```

**Example:**
```
Date: 2025-01-15
Cheque #: CHQ-001
Amount: 10,000 SAR
Payee: ABC Supplier

DR  2110 - Accounts Payable         10,000 SAR
    CR  2150 - Cheques Issued           10,000 SAR

Description: إصدار شيك رقم CHQ-001 للمستفيد ABC Supplier
```

### 3.2 Clearing (تسديد الشيك)

**When:** Bank confirms cheque has cleared (Status: Cleared)
**Accounting Treatment:** Release the cheque liability, deduct from bank

```
┌─────────────────────────────────────────────────────────────┐
│ JOURNAL ENTRY: Outgoing Cheque Clearing                     │
├─────────────────────────────────────────────────────────────┤
│ DR  Cheques Issued (Liability)      XXX                     │
│     CR  Bank Account                    XXX                 │
├─────────────────────────────────────────────────────────────┤
│ Reference: Cheque Number                                    │
│ Description: تسديد شيك رقم {ChequeNumber}                   │
└─────────────────────────────────────────────────────────────┘
```

**Example:**
```
Date: 2025-02-01
Cheque #: CHQ-001
Amount: 10,000 SAR

DR  2150 - Cheques Issued           10,000 SAR
    CR  1110 - Bank Account             10,000 SAR

Description: تسديد شيك رقم CHQ-001
```

### 3.3 Bounce (ارتجاع الشيك)

**When:** Bank returns cheque unpaid (Status: Bounced)
**Accounting Treatment:** Create receivable from payee, release cheque liability

```
┌─────────────────────────────────────────────────────────────┐
│ JOURNAL ENTRY: Outgoing Cheque Bounce                       │
├─────────────────────────────────────────────────────────────┤
│ DR  Bounced Cheques (Asset)         XXX                     │
│     CR  Cheques Issued (Liability)      XXX                 │
├─────────────────────────────────────────────────────────────┤
│ Reference: Cheque Number                                    │
│ Description: شيك مرتجع رقم {ChequeNumber}                   │
└─────────────────────────────────────────────────────────────┘
```

**If Bank Fees Applied:**
```
┌─────────────────────────────────────────────────────────────┐
│ JOURNAL ENTRY: Bank Fees on Bounced Cheque                  │
├─────────────────────────────────────────────────────────────┤
│ DR  Bank Charges (Expense)          XXX                     │
│     CR  Bank Account                    XXX                 │
├─────────────────────────────────────────────────────────────┤
│ Reference: FEE-{ChequeNumber}                               │
│ Description: رسوم بنكية - شيك مرتجع {ChequeNumber}          │
└─────────────────────────────────────────────────────────────┘
```

---

## 4. Incoming Cheques (شيكات واردة) - Journal Entries

### 4.1 Receipt (استلام الشيك)

**When:** Cheque received from customer (Status: Issued)
**Accounting Treatment:** Record cheque as asset, reduce AR

```
┌─────────────────────────────────────────────────────────────┐
│ JOURNAL ENTRY: Incoming Cheque Receipt                      │
├─────────────────────────────────────────────────────────────┤
│ DR  Cheques Received (Asset)        XXX                     │
│     CR  Accounts Receivable             XXX                 │
├─────────────────────────────────────────────────────────────┤
│ Reference: Cheque Number                                    │
│ Description: استلام شيك رقم {ChequeNumber} من {DrawerName}  │
└─────────────────────────────────────────────────────────────┘
```

**Example:**
```
Date: 2025-01-20
Cheque #: CHQ-IN-001
Amount: 15,000 SAR
Drawer: XYZ Customer

DR  1140 - Cheques Received         15,000 SAR
    CR  1130 - Accounts Receivable      15,000 SAR

Description: استلام شيك رقم CHQ-IN-001 من XYZ Customer
```

### 4.2 Clearing (تصفية الشيك)

**When:** Bank credits the cheque amount (Status: Cleared)
**Accounting Treatment:** Move from cheques received to bank

```
┌─────────────────────────────────────────────────────────────┐
│ JOURNAL ENTRY: Incoming Cheque Clearing                     │
├─────────────────────────────────────────────────────────────┤
│ DR  Bank Account                    XXX                     │
│     CR  Cheques Received (Asset)        XXX                 │
├─────────────────────────────────────────────────────────────┤
│ Reference: Cheque Number                                    │
│ Description: تصفية شيك رقم {ChequeNumber}                   │
└─────────────────────────────────────────────────────────────┘
```

**Example:**
```
Date: 2025-02-05
Cheque #: CHQ-IN-001
Amount: 15,000 SAR

DR  1110 - Bank Account             15,000 SAR
    CR  1140 - Cheques Received         15,000 SAR

Description: تصفية شيك رقم CHQ-IN-001
```

### 4.3 Bounce (ارتجاع الشيك)

**When:** Bank returns cheque unpaid (Status: Bounced)
**Accounting Treatment:** Restore AR, remove from cheques received

```
┌─────────────────────────────────────────────────────────────┐
│ JOURNAL ENTRY: Incoming Cheque Bounce                       │
├─────────────────────────────────────────────────────────────┤
│ DR  Accounts Receivable             XXX                     │
│     CR  Cheques Received (Asset)        XXX                 │
├─────────────────────────────────────────────────────────────┤
│ Reference: Cheque Number                                    │
│ Description: شيك مرتجع رقم {ChequeNumber}                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. Balance Update Rules

### 5.1 BankAccount Balance Fields

```csharp
// Bank Account Balance Structure
public class BankAccount
{
    // Computed from GL: OpeningBalance + Sum(JournalEntries)
    public decimal CurrentBalance { get; } // Actual ledger balance

    // Tracks reserved amount for pending outgoing cheques
    public decimal PendingChequesAmount { get; set; }

    // Computed: CurrentBalance - PendingChequesAmount
    public decimal AvailableBalance => CurrentBalance - PendingChequesAmount;
}
```

### 5.2 Balance Updates by Action

#### Outgoing Cheque Actions:

| Action | CurrentBalance | PendingChequesAmount | AvailableBalance |
|--------|----------------|---------------------|------------------|
| Issue | No change | +Amount | -Amount |
| Present | No change | No change | No change |
| Clear | -Amount (via GL) | -Amount | No change |
| Bounce | No change | -Amount | +Amount |
| Cancel | No change | -Amount (if was presented) | +Amount |

#### Incoming Cheque Actions:

| Action | CurrentBalance | Notes |
|--------|----------------|-------|
| Issue | No change | Recorded in Cheques Received GL |
| Present | No change | Status change only |
| Clear | +Amount (via GL) | Bank account increases |
| Bounce | No change | AR restored in GL |

### 5.3 AvailableBalance Calculation

```csharp
// Available Balance Formula
AvailableBalance = CurrentBalance - PendingChequesAmount

// Where:
// - CurrentBalance = GLAccountOpeningBalance + Sum(DebitEntries) - Sum(CreditEntries)
// - PendingChequesAmount = Sum(OutgoingCheques where Status IN (Issued, Presented))
```

---

## 6. PaymentHub Integration

### 6.1 Linking Cheques to Payments

```csharp
// When issuing cheque for a payment
public class Cheque
{
    public Guid? RelatedPaymentId { get; set; } // Links to Payment.Id
    public Guid? GLJournalId { get; set; }       // Links to GLJournal.Id
}
```

### 6.2 Payment-Cheque Flow

```
Payment Flow:
Draft → Approved → Posted (GL Created) → Cheque Issued

Cheque Flow (if payment method = Cheque):
Payment Posted → Cheque Created → Presented → Cleared
```

### 6.3 Journal Linking

```csharp
// Payment creates initial GL
Payment.GLJournalId = journalId;

// Cheque creates additional GL for clearing
Cheque.GLJournalId = clearingJournalId;
```

---

## 7. BalanceLedger Integration

### 7.1 BalanceLedger Entry Structure

```csharp
public class BalanceLedger
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public bool IsBankAccount { get; set; }
    public BalanceLedgerTransactionType TransactionType { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? ReferenceEntityId { get; set; }        // Cheque.Id
    public TreasuryEntityType? ReferenceEntityType { get; set; } // Cheque
    public string ReferenceNumber { get; set; }        // ChequeNumber
}
```

### 7.2 Transaction Types for Cheques

```csharp
public enum BalanceLedgerTransactionType
{
    ChequeIssue = 10,      // Outgoing cheque issued
    ChequeClearing = 11,   // Cheque cleared
    ChequeDeposit = 12,    // Incoming cheque deposited
    ChequeBounce = 13,     // Cheque bounced
    ChequeCancel = 14,     // Cheque cancelled
    BankFees = 20          // Bank fees charged
}
```

---

## 8. Reversal Rules

### 8.1 When Reversal is Allowed

| Original Transaction | Reversible | Method |
|---------------------|------------|--------|
| Cheque Issuance GL | ❌ No | Cancel cheque instead |
| Cheque Clearing GL | ❌ No | Cannot reverse after clearing |
| Cheque Bounce GL | ✅ Yes | Re-present cheque |
| Bank Fees GL | ⚠️ Partial | Manual adjustment only |

### 8.2 Re-Presentation After Bounce

```
Original: Issued → Presented → Bounced
Re-present: Bounced → Presented → (Cleared or Bounced again)

GL Impact:
1. Bounce GL already created (DR Bounced Cheques, CR Cheques Issued)
2. Re-present: Status change only, no GL
3. If Clears: DR Cheques Issued, CR Bank (normal clearing)
4. If Bounces again: Another bounce GL entry
```

---

## 9. Multi-Currency Handling

### 9.1 Foreign Currency Cheques

```
Cheque Currency: USD
Bank Account Currency: SAR
Exchange Rate at Issue: 3.75

Issuance Entry (in SAR):
DR  Accounts Payable         37,500 SAR (10,000 USD × 3.75)
    CR  Cheques Issued           37,500 SAR

Clearing Entry (at rate 3.80):
DR  Cheques Issued           37,500 SAR
DR  FX Loss                     500 SAR
    CR  Bank Account             38,000 SAR
```

### 9.2 Exchange Rate Recording

```csharp
public class Cheque
{
    public string Currency { get; set; }      // Cheque currency
    public decimal ExchangeRate { get; set; } // Rate at issue
    public decimal AmountFC { get; set; }     // Amount in foreign currency
    public decimal AmountLC { get; set; }     // Amount in local currency
}
```

---

## 10. Reconciliation Mapping

### 10.1 Bank Statement Matching

```
Bank Statement Line:
- Date: 2025-02-01
- Description: CHQ 001
- Debit: 10,000 SAR

Matched To:
- Cheque #: CHQ-001
- Status: Presented → Cleared
```

### 10.2 Reconciliation Status Update

```csharp
// When bank statement line is matched to cheque
cheque.Status = ChequeStatus.Cleared;
cheque.ClearedDate = statementLine.TransactionDate;
cheque.ReconciliationId = reconciliation.Id;
```

---

## 11. Audit Journal Requirements

### 11.1 Journal Entry Metadata

Every GL journal entry for cheques must include:

```csharp
public class CreateJournalDto
{
    public DateTime JournalDate { get; set; }
    public string Source { get; set; }           // "Treasury"
    public string SourceDocumentType { get; set; } // "Cheque"
    public Guid SourceDocumentId { get; set; }   // Cheque.Id
    public string ReferenceNumber { get; set; }  // ChequeNumber
    public string DescriptionAr { get; set; }
    public string DescriptionEn { get; set; }
    public int FiscalYear { get; set; }
    public int FiscalPeriod { get; set; }
}
```

### 11.2 Auto-Posting Rules

| GL Entry Type | Auto-Post? | Reason |
|---------------|------------|--------|
| Cheque Issuance | ✅ Yes | Immediate recording |
| Cheque Clearing | ✅ Yes | Bank confirmed |
| Cheque Bounce | ✅ Yes | Bank confirmed |
| Bank Fees | ✅ Yes | Bank confirmed |

---

## 12. Error Handling

### 12.1 Missing GL Mappings

```csharp
// If GL account not configured, log warning and skip GL creation
if (config.DefaultChequesIssuedGLId == Guid.Empty)
{
    Console.WriteLine($"Warning: GL account mappings not configured");
    return null; // Allow operation to continue without GL
}
```

### 12.2 Balance Validation Failures

```csharp
// Before clearing outgoing cheque
if (bankAccount.AvailableBalance < cheque.Amount)
{
    throw new TreasuryException(
        TreasuryErrorCode.InsufficientBalance,
        "Insufficient available balance to clear cheque",
        "رصيد غير كافي لتسديد الشيك");
}
```

---

## 13. Summary: Complete GL Entry Matrix

| Cheque Type | Action | Debit Account | Credit Account |
|-------------|--------|---------------|----------------|
| Outgoing | Issue | AP/Expense | Cheques Issued |
| Outgoing | Clear | Cheques Issued | Bank |
| Outgoing | Bounce | Bounced Cheques | Cheques Issued |
| Outgoing | Bounce Fees | Bank Charges | Bank |
| Incoming | Issue | Cheques Received | AR |
| Incoming | Clear | Bank | Cheques Received |
| Incoming | Bounce | AR | Cheques Received |

---

## 14. Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-01 | System | Initial document |
| 2.0 | 2025-12-06 | Claude | Complete accounting rules |
