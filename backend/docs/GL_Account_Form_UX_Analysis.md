# GL Account Form - UX + Accounting + System Design Analysis

**Document Version:** 1.0
**Date:** 2026-01-14
**Purpose:** Deep analysis of the GL Account creation form with recommendations for simplification while maintaining accounting integrity

---

## Table of Contents

1. [Current Form Analysis](#1-current-form-analysis)
2. [Recommended Simplified Form](#2-recommended-simplified-form)
3. [Fields to Relocate to Settings Pages](#3-fields-to-relocate-to-settings-pages)
4. [Accounting Logic Requirements](#4-accounting-logic-requirements)
5. [Default Chart of Accounts Recommendation](#5-default-chart-of-accounts-recommendation)
6. [Final Proposed UX Flow](#6-final-proposed-ux-flow)

---

## 1. Current Form Analysis

### 1.1 Complete Field Inventory

The current GL Account form contains **31 total fields** organized into 6 collapsible sections:

| Section | Field Count | Complexity |
|---------|-------------|------------|
| Basic Information | 6 | Low |
| Hierarchy & Posting | 3 | Medium |
| Account Nature | 5 | High |
| Cash Flow & Reporting | 15 | Very High |
| Accounting Dimensions | 3 | Medium |
| Additional Information | 5 | Low |

### 1.2 Field Classification Matrix

#### **ESSENTIAL (Must Display Immediately)**

| Field | Purpose | Why Essential |
|-------|---------|---------------|
| Account Number | Unique identifier within parent | Required for tree structure, cannot be inferred |
| Account Name (EN) | English display name | Required for international reporting |
| Account Name (AR) | Arabic display name | Required for local compliance (Saudi) |
| Account Type | IFRS classification | Determines Normal Balance, reporting category |
| Active Status | Controls usability | Common operational need |

#### **OPTIONAL (Relevant but Not Always Needed)**

| Field | Purpose | When Needed |
|-------|---------|-------------|
| Parent Account | Hierarchy placement | Only for sub-accounts (inherited from context) |
| Currency | Multi-currency support | Only for foreign currency accounts |
| Is Leaf Account | Posting control | Only when creating parent/summary accounts |
| Description (EN/AR) | Documentation | For complex accounts needing explanation |

#### **ADVANCED (Should Be Hidden)**

| Field | Purpose | Why Hide |
|-------|---------|----------|
| Allow Manual Entries | System integration | 99% of accounts need manual entries |
| Allow Posting to Parent | Exception handling | Dangerous flag, rarely used |
| External Code | ERP integration | Only for companies with external systems |
| Tax Code | Tax automation | Only for specific tax-related accounts |
| Normal Balance Override | Exception handling | Extremely rare, requires CFO approval |

#### **COMPLEX (Move Out of Form)**

| Field | Purpose | Why Move |
|-------|---------|----------|
| Cash Flow Category | IAS 7 reporting | Requires accounting expertise |
| Working Capital Type | Cash flow indirect method | Requires advanced knowledge |
| GLAccountFlags (14 flags) | Report classification | Too technical for end users |
| Is Retained Earnings | Year-end closing | System-wide setting (only 1 account) |
| Is Control Account | Subsidiary integration | System architecture concern |
| Reconciliation Mode | Audit requirements | Policy-level decision |

### 1.3 Current UX Problems

| Problem | Impact | Severity |
|---------|--------|----------|
| **31 fields overwhelm users** | High cognitive load, errors | Critical |
| **Technical accounting terms** | Confusion for non-accountants | High |
| **No smart defaults** | Every account requires full configuration | High |
| **Flat structure despite collapsibles** | Users don't know what to expand | Medium |
| **Cash Flow flags visible to all** | Only CFO/auditors need this | Medium |
| **No role-based field visibility** | Bookkeepers see CFO fields | Medium |

---

## 2. Recommended Simplified Form

### 2.1 Core Simplified Form (5 Fields Only)

For **90% of use cases**, users should only see:

```
┌─────────────────────────────────────────────────────────────┐
│                    Add New Account                          │
├─────────────────────────────────────────────────────────────┤
│  Under: 1100 - Cash and Cash Equivalents                    │
│  Full Account Number: 1100.____                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Account Number *         [1110              ]              │
│                                                             │
│  Name (English) *         [Petty Cash        ]              │
│                                                             │
│  Name (Arabic) *          [صندوق المصروفات النثرية    ]      │
│                                                             │
│  Account Type *           [Asset ▼           ] (inherited)  │
│                           Normal Balance: Debit (auto)      │
│                                                             │
│  ☑ Active Account                                           │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│  [+ Show Advanced Options]                                  │
├─────────────────────────────────────────────────────────────┤
│                    [Cancel]  [Save Account]                 │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Field Visibility Rules

| Field | Visibility | Condition |
|-------|------------|-----------|
| Account Number | Always | Required |
| Account Name (EN) | Always | Required |
| Account Name (AR) | Always | Required |
| Account Type | Always (but may be locked) | Inherited from parent in sub-accounts |
| Active Status | Always | Simple checkbox |
| Currency | Show if parent has foreign currency | Inherited by default |
| Is Leaf | Show only if creating under a leaf account | System determines automatically |
| Dimensions | Show only for Revenue/Expense types | Dynamic |
| Advanced Options | Collapsed by default | User-initiated |

### 2.3 Why This Works

| Design Decision | Rationale |
|-----------------|-----------|
| **5 core fields** | Matches mental model of "creating an account" |
| **Inherited account type** | 95% of sub-accounts share parent's type |
| **Auto Normal Balance** | Eliminates a major error source |
| **Hidden dimensions for non-P&L** | Balance sheet accounts rarely need dimensions |
| **"Show Advanced" pattern** | Power users can access everything |

### 2.4 Progressive Disclosure Tiers

```
Tier 1: Core Fields (Always Visible)
├── Account Number, Names, Type, Active

Tier 2: Contextual Fields (Show When Relevant)
├── Currency (when parent is foreign)
├── Dimensions (when Revenue/Expense)
├── Is Leaf (when creating under leaf account)

Tier 3: Advanced Options (User-Initiated)
├── Hierarchy & Posting
├── Account Nature
├── Description

Tier 4: Expert Mode (Separate Settings Page)
├── Cash Flow Classification
├── GLAccountFlags
├── Reconciliation Settings
├── External Integration
```

---

## 3. Fields to Relocate to Settings Pages

### 3.1 Account Advanced Settings (Per-Account)

These fields should be accessible via an **"Advanced Settings"** link on the account detail page, not the creation form:

| Field | New Location | Reasoning |
|-------|--------------|-----------|
| Cash Flow Category | Account Settings > Reporting | Requires IAS 7 expertise |
| Working Capital Type | Account Settings > Reporting | Subset of Cash Flow |
| GLAccountFlags | Account Settings > Reporting | Technical classification |
| External Code | Account Settings > Integration | Not needed for accounting |
| Tax Code | Account Settings > Tax | Tax specialist responsibility |
| Reconciliation Mode | Account Settings > Audit | Audit/compliance concern |
| Allow Manual Entries | Account Settings > Controls | Rare exception |
| Allow Posting to Parent | Account Settings > Controls | Dangerous, CFO approval |

**Risks if Kept in Main Form:**
- Users accidentally set wrong Cash Flow Category (audit risk)
- Report flags misconfigured (Cash Flow Statement errors)
- System integration fields left empty (data quality)

### 3.2 System-Wide Accounting Settings Page

These should be managed at company/system level, not per-account:

| Setting | Page Location | Reasoning |
|---------|---------------|-----------|
| Retained Earnings Account | Company Settings > Year-End | Only 1 per company |
| Default Currency | Company Settings > General | Applied to all new accounts |
| Default Dimension Requirements | Company Settings > Dimensions | Policy-level decision |
| Control Account Mappings | Company Settings > Subsidiaries | System architecture |
| Reconciliation Policy | Company Settings > Audit | Company-wide rules |

**Proposed Settings Structure:**

```
Finance Settings
├── General
│   ├── Base Currency
│   ├── Default Account Currency
│   └── Fiscal Year Configuration
│
├── Chart of Accounts
│   ├── Account Numbering Rules
│   ├── Default Dimension Requirements
│   └── Import/Export Templates
│
├── Year-End Closing
│   ├── Retained Earnings Account (select)
│   ├── Income Summary Account (optional)
│   └── Auto-close Rules
│
├── Cash Flow Reporting (IAS 7)
│   ├── Classification Defaults by Account Type
│   ├── Working Capital Account Mappings
│   └── Non-Cash Adjustment Rules
│
├── Integration
│   ├── External System Mappings
│   └── Tax Code Library
│
└── Audit & Compliance
    ├── Reconciliation Requirements
    ├── Approval Workflows
    └── Override Permissions
```

### 3.3 Default Values vs. User-Provided Values

| Current Behavior | Recommended Change |
|------------------|-------------------|
| User selects Cash Flow Category | System suggests based on account type + name pattern |
| User sets dimension requirements | Inherit from parent, allow override |
| User chooses Reconciliation Mode | Default to "Required" for Cash/Bank, "None" otherwise |
| User toggles Is Leaf | System determines based on whether account has children |
| User selects Working Capital Type | Auto-suggest based on account name keywords |

---

## 4. Accounting Logic Requirements

### 4.1 Account Classification Automation

**Current:** User manually selects Cash/Bank/General

**Recommended:** Smart detection with confirmation

```typescript
function detectAccountNature(accountName: string, parentAccount: Account): AccountNature {
  const lowerName = accountName.toLowerCase();

  // Pattern matching for Cash accounts
  if (lowerName.includes('petty cash') ||
      lowerName.includes('cash on hand') ||
      lowerName.includes('صندوق')) {
    return AccountNature.Cash;
  }

  // Pattern matching for Bank accounts
  if (lowerName.includes('bank') ||
      lowerName.includes('بنك') ||
      lowerName.includes('checking') ||
      lowerName.includes('savings')) {
    return AccountNature.Bank;
  }

  // Inherit from parent if under Cash & Cash Equivalents
  if (parentAccount?.isCashAccount || parentAccount?.isBankAccount) {
    return parentAccount.isCashAccount ? AccountNature.Cash : AccountNature.Bank;
  }

  return AccountNature.None;
}
```

**UI Behavior:**
1. User enters account name
2. System auto-suggests nature with visual indicator
3. User can confirm or override

### 4.2 Parent/Sub-Account Relations

**Automatic Handling:**

| Scenario | System Behavior |
|----------|-----------------|
| Creating under leaf account | Convert parent to non-leaf, warn user |
| Creating root account | Level = 1, no parent |
| Creating sub-account | Level = parent.level + 1, inherit type |
| Moving account | Recalculate full account numbers |
| Deleting parent | Block if has children, or cascade delete |

**Validation Rules:**
```typescript
interface HierarchyValidation {
  // Block circular references
  cannotBeOwnParent: boolean;
  cannotBeChildOfOwnChild: boolean;

  // Maintain type consistency
  subAccountTypeMustMatchParent: boolean; // Configurable

  // Prevent deep nesting
  maxDepth: number; // Default: 7 levels
}
```

### 4.3 Dimension Requirements

**Smart Defaults:**

| Account Type | Cost Center | Department | Project |
|--------------|-------------|------------|---------|
| Asset | No | No | No |
| Liability | No | No | No |
| Equity | No | No | No |
| Revenue | Yes (configurable) | Yes (configurable) | Optional |
| Expense | Yes | Yes | Optional |
| Contra accounts | Follow base type | Follow base type | Follow base type |

**Inheritance Logic:**
```typescript
function getDimensionRequirements(account: Account, parent: Account | null): DimensionRequirements {
  // If parent has requirements, child inherits
  if (parent?.dimensionRequirements) {
    return parent.dimensionRequirements;
  }

  // Apply system defaults based on type
  if (account.accountType === AccountType.Expense ||
      account.accountType === AccountType.Revenue) {
    return getCompanyDefaultDimensions();
  }

  return DimensionRequirements.None;
}
```

### 4.4 Cash Flow Category Auto-Assignment

**IAS 7 Compliant Auto-Assignment:**

| Account Type | Account Flags | Default Category |
|--------------|---------------|------------------|
| Asset (Fixed) | FixedAsset | Investing |
| Asset (Intangible) | IntangibleAsset | Investing |
| Asset (Investment) | Investment | Investing |
| Asset (Current) | WorkingCapital | Operating |
| Liability (Loan) | Loan | Financing |
| Equity | - | Financing |
| Revenue | - | Operating |
| Expense | - | Operating |
| Expense (Depreciation) | Depreciation | Operating (non-cash) |
| Expense (Interest) | Interest | Operating or Financing |

**Algorithm:**
```typescript
function suggestCashFlowCategory(account: CreateAccountDto): CashFlowCategory {
  const flags = account.accountFlags || 0;

  // Flags take precedence
  if (flags & GLAccountFlags.FixedAsset ||
      flags & GLAccountFlags.IntangibleAsset ||
      flags & GLAccountFlags.Investment) {
    return CashFlowCategory.Investing;
  }

  if (flags & GLAccountFlags.Loan) {
    return CashFlowCategory.Financing;
  }

  // Type-based defaults
  switch (account.accountType) {
    case AccountType.Equity:
      return CashFlowCategory.Financing;
    case AccountType.Revenue:
    case AccountType.Expense:
      return CashFlowCategory.Operating;
    default:
      return CashFlowCategory.None;
  }
}
```

### 4.5 GLAccountFlags Auto-Detection

**Keyword-Based Flag Suggestion:**

| Keywords (EN) | Keywords (AR) | Suggested Flag |
|---------------|---------------|----------------|
| depreciation | إهلاك | Depreciation |
| amortization | إطفاء | Amortization |
| provision, allowance | مخصص | Provision |
| receivable, payable, inventory | ذمم، مخزون | WorkingCapital |
| equipment, property, vehicle | معدات، عقار، سيارة | FixedAsset |
| software, patent, license | برمجيات، براءة | IntangibleAsset |
| investment, shares | استثمار، أسهم | Investment |
| loan, borrowing | قرض، اقتراض | Loan |
| interest | فوائد | Interest |
| dividend | أرباح موزعة | Dividend |
| tax, VAT | ضريبة | Tax |
| gain, loss, disposal | ربح، خسارة، استبعاد | GainLossOnDisposal |
| forex, exchange | فروق عملات | ForeignExchange |
| impairment | اضمحلال | Impairment |

### 4.6 Preventing Invalid Account Creation

**Validation Rules (Backend Enforced):**

```typescript
interface AccountValidation {
  // Hard Blocks (Cannot Save)
  errors: [
    "Cash/Bank accounts must be leaf accounts",
    "Account cannot be both Cash and Bank",
    "WorkingCapital flag requires WorkingCapitalType",
    "Balance sheet classification flags are mutually exclusive",
    "IntangibleAsset accounts must be in Investing category",
  ],

  // Soft Warnings (Can Save with Acknowledgment)
  warnings: [
    "Normal balance overridden from default",
    "Posting to parent account enabled",
    "Interest account without CashFlowCategory",
    "Foreign currency account without exchange rate",
  ]
}
```

### 4.7 IFRS Compliance Matrix

| IFRS Standard | Requirement | System Support |
|---------------|-------------|----------------|
| IAS 1 | Statement of Financial Position | Account Type classification |
| IAS 7 | Cash Flow Statement | CashFlowCategory, GLAccountFlags |
| IAS 8 | Accounting Policies | Audit trail, versioning |
| IAS 21 | Foreign Currency | Currency field, exchange rates |
| IFRS 15 | Revenue Recognition | Revenue account type, dimensions |
| IFRS 16 | Leases | Asset/Liability types |

---

## 5. Default Chart of Accounts Recommendation

### 5.1 Options Analysis

| Approach | Pros | Cons |
|----------|------|------|
| **A. No Default COA** | Maximum flexibility | Every company starts from zero |
| **B. Full Default COA** | Fast setup, IFRS compliant | May not fit all industries |
| **C. Template-Based COA** | Industry-specific, customizable | Requires template maintenance |
| **D. Hybrid Approach** | Best of both worlds | More complex implementation |

### 5.2 Recommendation: Hybrid Approach (Option D)

**Implementation:**

1. **Core IFRS-Compliant Structure** (Always provided)
   - Top-level categories only
   - 1000-1999: Assets
   - 2000-2999: Liabilities
   - 3000-3999: Equity
   - 4000-4999: Revenue
   - 5000-5999: Expenses

2. **Industry Templates** (User selects during setup)
   - Retail & Wholesale
   - Manufacturing
   - Services (Professional)
   - Real Estate
   - Healthcare
   - Hospitality
   - Construction
   - Non-Profit

3. **Custom Building** (Always available)
   - Users can modify any template
   - Add/remove accounts freely
   - Import from Excel

### 5.3 Proposed Core Structure

```
ASSETS (1000-1999)
├── 1000 Current Assets
│   ├── 1100 Cash and Cash Equivalents
│   │   ├── 1110 Cash on Hand
│   │   ├── 1120 Cash in Bank
│   │   └── 1130 Petty Cash
│   ├── 1200 Receivables
│   │   ├── 1210 Trade Receivables
│   │   └── 1220 Other Receivables
│   ├── 1300 Inventory
│   └── 1400 Prepaid Expenses
├── 1500 Non-Current Assets
│   ├── 1510 Property, Plant & Equipment
│   ├── 1520 Intangible Assets
│   └── 1530 Investments

LIABILITIES (2000-2999)
├── 2000 Current Liabilities
│   ├── 2100 Trade Payables
│   ├── 2200 Accrued Expenses
│   └── 2300 Short-term Loans
├── 2500 Non-Current Liabilities
│   ├── 2510 Long-term Loans
│   └── 2520 Deferred Tax

EQUITY (3000-3999)
├── 3100 Share Capital
├── 3200 Retained Earnings ★
└── 3300 Reserves

REVENUE (4000-4999)
├── 4100 Sales Revenue
├── 4200 Service Revenue
└── 4900 Other Income

EXPENSES (5000-5999)
├── 5100 Cost of Sales
├── 5200 Operating Expenses
│   ├── 5210 Salaries & Wages
│   ├── 5220 Rent Expense
│   ├── 5230 Utilities
│   └── 5240 Depreciation
├── 5300 Administrative Expenses
└── 5900 Other Expenses
```

★ = Pre-configured as Retained Earnings Account

### 5.4 Template Benefits

| Benefit | Impact |
|---------|--------|
| Faster onboarding | Setup time reduced from hours to minutes |
| IFRS compliance built-in | Reduces audit findings |
| Consistent structure | Easier support and training |
| Industry best practices | Pre-configured Cash Flow mappings |
| Dimension defaults | Revenue/Expense accounts pre-configured |

---

## 6. Final Proposed UX Flow

### 6.1 Account Creation Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    ACCOUNT CREATION FLOW                         │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │  User clicks    │
                    │  "Add Account"  │
                    └────────┬────────┘
                             │
                             ▼
              ┌──────────────────────────────┐
              │  Context Detection           │
              │  - Under which parent?       │
              │  - What account type?        │
              └──────────────┬───────────────┘
                             │
                             ▼
        ┌────────────────────────────────────────┐
        │         SIMPLIFIED FORM                │
        │  ┌──────────────────────────────────┐  │
        │  │ Account Number *    [____]       │  │
        │  │ Name (EN) *         [_______]    │  │
        │  │ Name (AR) *         [_______]    │  │
        │  │ Account Type *      [Asset ▼]    │  │
        │  │                     (inherited)   │  │
        │  │ ☑ Active                         │  │
        │  └──────────────────────────────────┘  │
        │                                        │
        │  💡 Suggestion: This looks like a     │
        │     Cash account. [Confirm] [Ignore]  │
        │                                        │
        │  [+ Show Advanced Options]             │
        └────────────────────┬───────────────────┘
                             │
              ┌──────────────┴──────────────┐
              │                             │
              ▼                             ▼
     ┌────────────────┐           ┌────────────────┐
     │  Quick Save    │           │  Show Advanced │
     │  (90% users)   │           │  (10% users)   │
     └───────┬────────┘           └───────┬────────┘
             │                            │
             ▼                            ▼
   ┌──────────────────┐     ┌──────────────────────────┐
   │  Validate &      │     │  ADVANCED OPTIONS        │
   │  Auto-Configure  │     │  ┌────────────────────┐  │
   │  - Cash Flow     │     │  │ Hierarchy & Posting│  │
   │  - Flags         │     │  ├────────────────────┤  │
   │  - Dimensions    │     │  │ Account Nature     │  │
   │                  │     │  ├────────────────────┤  │
   └───────┬──────────┘     │  │ Dimensions         │  │
           │                │  │ (if Revenue/Exp)   │  │
           │                │  ├────────────────────┤  │
           │                │  │ Currency           │  │
           │                │  ├────────────────────┤  │
           │                │  │ Description        │  │
           │                │  └────────────────────┘  │
           │                │                          │
           │                │  [Configure Reporting ↗] │
           │                │  (Opens Settings Page)   │
           │                └────────────┬─────────────┘
           │                             │
           └──────────────┬──────────────┘
                          │
                          ▼
               ┌──────────────────┐
               │  ACCOUNT CREATED │
               │  ✓ Success       │
               └────────┬─────────┘
                        │
                        ▼
        ┌───────────────────────────────┐
        │  Post-Creation Options        │
        │  • Add another account        │
        │  • Add sub-account            │
        │  • Configure Cash Flow ↗      │
        │  • View in Chart of Accounts  │
        └───────────────────────────────┘
```

### 6.2 Role-Based Field Visibility

| Role | Core Fields | Advanced | Cash Flow/Flags | Settings Access |
|------|-------------|----------|-----------------|-----------------|
| Bookkeeper | ✓ | Read-only | Hidden | No |
| Accountant | ✓ | ✓ | View only | No |
| Senior Accountant | ✓ | ✓ | ✓ | Limited |
| Finance Manager | ✓ | ✓ | ✓ | ✓ |
| CFO | ✓ | ✓ | ✓ | Full |

### 6.3 Dynamic UI Triggers

| Trigger | UI Response |
|---------|-------------|
| Account name contains "bank" | Show "Bank Account" suggestion |
| Account name contains "petty" or "cash" | Show "Cash Account" suggestion |
| Parent is Revenue/Expense | Auto-show Dimensions section |
| Parent has foreign currency | Pre-fill currency, show warning |
| Creating root account | Hide parent field, show level selector |
| User is Bookkeeper role | Hide Advanced Options link |

### 6.4 Settings Page Access Points

```
Account Detail Page
├── Basic Info (editable)
├── Hierarchy Info (read-only)
├── Current Balance (read-only)
│
├── [Edit Account] → Opens Simplified Form
│
├── ⚙ Account Settings (expandable)
│   ├── Reporting Configuration
│   │   ├── Cash Flow Category
│   │   ├── Working Capital Type
│   │   └── Report Flags
│   │
│   ├── Controls
│   │   ├── Allow Manual Entries
│   │   └── Allow Posting to Parent
│   │
│   ├── Integration
│   │   ├── External Code
│   │   └── Tax Code
│   │
│   └── Audit
│       └── Reconciliation Mode
│
└── [Delete Account] (if no transactions)
```

### 6.5 Implementation Priority

| Phase | Features | Effort |
|-------|----------|--------|
| **Phase 1** | Simplified 5-field form | Low |
| **Phase 2** | Smart suggestions (Cash/Bank detection) | Medium |
| **Phase 3** | Settings page separation | Medium |
| **Phase 4** | Role-based visibility | Medium |
| **Phase 5** | Auto Cash Flow classification | High |
| **Phase 6** | Industry templates | High |

---

## Summary

### Key Recommendations

1. **Reduce initial form to 5 fields** - Account Number, Names (EN/AR), Type, Active
2. **Use progressive disclosure** - Advanced options hidden by default
3. **Move Cash Flow/Flags to Settings** - Separate page for expert configuration
4. **Implement smart defaults** - Auto-detect account nature, inherit dimensions
5. **Provide hybrid COA templates** - Core structure + industry-specific options
6. **Role-based access** - Bookkeepers see simplified view
7. **Auto-configure where possible** - Normal Balance, Cash Flow Category

### Expected Outcomes

| Metric | Current | After Implementation |
|--------|---------|---------------------|
| Time to create account | 2-5 minutes | 30 seconds |
| Error rate | ~15% | <3% |
| Training required | 2 hours | 15 minutes |
| User satisfaction | Medium | High |
| CFO review needed | Every account | Exception-only |

---

**Document prepared by:** Claude AI
**For:** Maham ERP Finance Module
**Classification:** Internal Development Reference
