# System Settings Page - Complete UX/UI Design Specification

## Document Information
| Property | Value |
|----------|-------|
| Version | 1.0 |
| Created | January 2026 |
| Purpose | Unified System Settings Page Design |
| Target | React/Next.js + Tailwind CSS |

---

## Table of Contents

1. [Executive Overview](#1-executive-overview)
2. [Page Layout Architecture](#2-page-layout-architecture)
3. [Internal Sidebar Design](#3-internal-sidebar-design)
4. [Settings Categories - Full Specification](#4-settings-categories---full-specification)
5. [Component Breakdown](#5-component-breakdown)
6. [JSON Schema Structure](#6-json-schema-structure)
7. [Implementation Guidelines](#7-implementation-guidelines)
8. [Best Practices & Recommendations](#8-best-practices--recommendations)

---

## 1. Executive Overview

### 1.1 Design Philosophy

This unified System Settings page follows enterprise ERP patterns (Odoo, SAP, NetSuite) with:

- **Single Entry Point**: One `/settings` route for all configurations
- **Persistent Sidebar**: Always-visible category navigation
- **Progressive Disclosure**: Settings organized from general to specific
- **Contextual Grouping**: Related settings grouped in collapsible sections
- **Real-time Validation**: Immediate feedback on invalid inputs
- **Auto-save with Confirmation**: Changes saved on blur with toast notifications

### 1.2 Key UX Principles

| Principle | Implementation |
|-----------|----------------|
| Discoverability | Searchable settings with keyboard shortcut (Cmd/Ctrl + K) |
| Consistency | Same field patterns across all categories |
| Feedback | Loading states, success/error toasts, validation messages |
| Efficiency | Bulk actions, copy settings, import/export |
| Accessibility | WCAG 2.1 AA compliant, keyboard navigation |

---

## 2. Page Layout Architecture

### 2.1 High-Level Structure

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  HEADER: System Settings                                    [Search] [Save] │
├────────────────────┬────────────────────────────────────────────────────────┤
│                    │                                                        │
│  INTERNAL SIDEBAR  │              CONTENT AREA                              │
│  (240px fixed)     │              (Fluid width)                             │
│                    │                                                        │
│  ┌──────────────┐  │  ┌──────────────────────────────────────────────────┐  │
│  │ Search...    │  │  │  SECTION HEADER                                  │  │
│  └──────────────┘  │  │  Category Title + Description                    │  │
│                    │  └──────────────────────────────────────────────────┘  │
│  ○ General         │                                                        │
│  ○ Company         │  ┌──────────────────────────────────────────────────┐  │
│  ○ Financial       │  │  SETTINGS GROUP 1                         [▼]   │  │
│    ├─ Currency     │  │  ┌────────────────┐  ┌────────────────┐         │  │
│    ├─ GL Accounts  │  │  │ Field 1        │  │ Field 2        │         │  │
│    └─ Numbering    │  │  └────────────────┘  └────────────────┘         │  │
│  ○ Treasury        │  └──────────────────────────────────────────────────┘  │
│    ├─ Cash         │                                                        │
│    ├─ Bank         │  ┌──────────────────────────────────────────────────┐  │
│    └─ Methods      │  │  SETTINGS GROUP 2                         [▼]   │  │
│  ○ Tax             │  │  ┌────────────────┐  ┌────────────────┐         │  │
│    ├─ Codes        │  │  │ Field 3        │  │ Field 4        │         │  │
│    ├─ Accounts     │  │  └────────────────┘  └────────────────┘         │  │
│    └─ Rules        │  └──────────────────────────────────────────────────┘  │
│  ○ Fiscal Year     │                                                        │
│  ○ Users & Access  │  ┌──────────────────────────────────────────────────┐  │
│  ○ Integrations    │  │  [Reset to Defaults]            [Save Changes]  │  │
│                    │  └──────────────────────────────────────────────────┘  │
│                    │                                                        │
└────────────────────┴────────────────────────────────────────────────────────┘
```

### 2.2 Responsive Behavior

| Breakpoint | Sidebar | Content |
|------------|---------|---------|
| Desktop (≥1280px) | Fixed 240px | Fluid, max-width 1200px |
| Tablet (768-1279px) | Collapsible 240px | Full width when collapsed |
| Mobile (<768px) | Bottom sheet / Drawer | Full width |

### 2.3 Page Header

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ⚙️ System Settings                                                         │
│  Configure your system preferences, financial settings, and access control │
│                                                                             │
│  [🔍 Search settings... (Ctrl+K)]                    [↻ Reset] [💾 Save]   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Internal Sidebar Design

### 3.1 Sidebar Structure

```tsx
const sidebarCategories = [
  {
    id: 'general',
    icon: 'PiGearDuotone',
    label: 'General',
    labelAr: 'عام',
    badge: null,
    children: null
  },
  {
    id: 'company',
    icon: 'PiBuildingOfficeDuotone',
    label: 'Company Profile',
    labelAr: 'ملف الشركة',
    badge: null,
    children: null
  },
  {
    id: 'financial',
    icon: 'PiCurrencyDollarDuotone',
    label: 'Financial',
    labelAr: 'المالية',
    badge: null,
    children: [
      { id: 'financial-currency', label: 'Currency', labelAr: 'العملات' },
      { id: 'financial-gl', label: 'GL Accounts', labelAr: 'حسابات الأستاذ' },
      { id: 'financial-numbering', label: 'Numbering', labelAr: 'الترقيم' },
      { id: 'financial-posting', label: 'Posting Rules', labelAr: 'قواعد الترحيل' }
    ]
  },
  {
    id: 'treasury',
    icon: 'PiVaultDuotone',
    label: 'Treasury',
    labelAr: 'الخزينة',
    badge: null,
    children: [
      { id: 'treasury-cash', label: 'Cash & Petty Cash', labelAr: 'النقدية' },
      { id: 'treasury-bank', label: 'Bank Accounts', labelAr: 'الحسابات البنكية' },
      { id: 'treasury-methods', label: 'Payment Methods', labelAr: 'طرق الدفع' },
      { id: 'treasury-rules', label: 'Auto-Posting', labelAr: 'الترحيل التلقائي' }
    ]
  },
  {
    id: 'tax',
    icon: 'PiReceiptDuotone',
    label: 'Tax',
    labelAr: 'الضرائب',
    badge: null,
    children: [
      { id: 'tax-codes', label: 'Tax Codes', labelAr: 'رموز الضرائب' },
      { id: 'tax-accounts', label: 'Tax Accounts', labelAr: 'حسابات الضرائب' },
      { id: 'tax-rules', label: 'Calculation Rules', labelAr: 'قواعد الحساب' },
      { id: 'tax-exemptions', label: 'Exemptions', labelAr: 'الإعفاءات' }
    ]
  },
  {
    id: 'fiscal',
    icon: 'PiCalendarDuotone',
    label: 'Fiscal Year',
    labelAr: 'السنة المالية',
    badge: null,
    children: [
      { id: 'fiscal-years', label: 'Years', labelAr: 'السنوات' },
      { id: 'fiscal-periods', label: 'Periods', labelAr: 'الفترات' },
      { id: 'fiscal-closing', label: 'Closing Rules', labelAr: 'قواعد الإغلاق' }
    ]
  },
  {
    id: 'users',
    icon: 'PiUsersDuotone',
    label: 'Users & Access',
    labelAr: 'المستخدمين والصلاحيات',
    badge: null,
    children: [
      { id: 'users-roles', label: 'Roles', labelAr: 'الأدوار' },
      { id: 'users-permissions', label: 'Permissions', labelAr: 'الصلاحيات' },
      { id: 'users-workflows', label: 'Approval Workflows', labelAr: 'سير الموافقات' }
    ]
  },
  {
    id: 'integrations',
    icon: 'PiPlugDuotone',
    label: 'Integrations',
    labelAr: 'التكاملات',
    badge: 'NEW',
    children: [
      { id: 'integrations-zatca', label: 'ZATCA', labelAr: 'زاتكا' },
      { id: 'integrations-api', label: 'API Keys', labelAr: 'مفاتيح API' },
      { id: 'integrations-webhooks', label: 'Webhooks', labelAr: 'ويب هوكس' }
    ]
  }
];
```

### 3.2 Sidebar Visual Design

```
┌────────────────────┐
│ 🔍 Search...       │  ← Search box with icon
├────────────────────┤
│                    │
│ ⚙️  General        │  ← Active state: bg-primary/10, text-primary, left border
│ 🏢  Company        │
│ 💰  Financial    ▼ │  ← Expandable with chevron
│     ├─ Currency    │  ← Indented children
│     ├─ GL Accounts │
│     ├─ Numbering   │
│     └─ Posting     │
│ 🏦  Treasury     ▶ │  ← Collapsed
│ 🧾  Tax          ▶ │
│ 📅  Fiscal Year  ▶ │
│ 👥  Users        ▶ │
│ 🔌  Integrations ● │  ← Badge indicator
│                    │
├────────────────────┤
│ ℹ️  v2.5.0        │  ← Version info
└────────────────────┘
```

---

## 4. Settings Categories - Full Specification

### 4.1 General System Settings

**Route:** `/settings/general`
**Description:** Core system preferences that affect the entire application

#### Section: System Identity

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| System Name | Text Input | Required, max 100 chars | Display name for the system |
| System Tagline | Text Input | Optional, max 200 chars | Short description shown in header |
| Primary Logo | File Upload | PNG/SVG, max 2MB, 200x60px | Main logo for headers |
| Favicon | File Upload | ICO/PNG, max 100KB, 32x32px | Browser tab icon |
| Login Background | File Upload | JPG/PNG, max 5MB | Login page background |

#### Section: Localization

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Default Language | Dropdown | English, Arabic, French... | System default language |
| Default Timezone | Dropdown | UTC offsets list | Server timezone |
| Date Format | Dropdown | DD/MM/YYYY, MM/DD/YYYY, YYYY-MM-DD | Date display format |
| Time Format | Radio | 12-hour, 24-hour | Time display format |
| First Day of Week | Dropdown | Sunday, Monday, Saturday | Calendar start day |
| Default Country | Dropdown | ISO country list | Default for addresses |

#### Section: Regional Settings

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Number Format | Dropdown | 1,234.56 / 1.234,56 / 1 234,56 | Number display format |
| Negative Numbers | Radio | -100, (100), 100- | Negative number display |
| Thousand Separator | Text | , / . / space | Thousand separator character |
| Decimal Separator | Text | . / , | Decimal separator character |
| Decimal Places | Number | 0-6 | Default decimal precision |

---

### 4.2 Company Profile Settings

**Route:** `/settings/company`
**Description:** Legal entity and business information

#### Section: Basic Information

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Legal Name | Text Input | Required, max 200 chars | Official registered name |
| Trade Name | Text Input | Optional, max 200 chars | Business trading name |
| Legal Name (Arabic) | Text Input | Optional, RTL | Arabic legal name |
| Company Type | Dropdown | LLC, Corporation, Partnership, Sole Prop | Legal structure |
| Registration Number | Text Input | Required, alphanumeric | Commercial registration |
| Establishment Date | Date Picker | Required, past date | Company founding date |

#### Section: Tax Information

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| VAT Registration Number | Text Input | Pattern: 15 digits for SA | Tax registration ID |
| Tax Registration Date | Date Picker | Optional | When registered for VAT |
| Tax Group | Dropdown | Standard, Simplified, Exempt | VAT scheme |
| Economic Activity Code | Text Input | Optional | ISIC activity code |

#### Section: Contact Information

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Phone Number | Phone Input | Valid phone format | Primary phone |
| Fax Number | Phone Input | Optional | Fax number |
| Email Address | Email Input | Valid email | Primary email |
| Website | URL Input | Valid URL | Company website |

#### Section: Address

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Street Address | Text Input | Required | Street and number |
| Building Number | Text Input | Required for SA | Building number |
| Additional Number | Text Input | Optional | Secondary number |
| District | Text Input | Required | Neighborhood/district |
| City | Text Input | Required | City name |
| State/Province | Dropdown | Based on country | Region |
| Postal Code | Text Input | Required | ZIP/Postal code |
| Country | Dropdown | Required | Country |

---

### 4.3 Financial Settings

**Route:** `/settings/financial`

#### 4.3.1 Currency Settings (`/settings/financial/currency`)

##### Section: Base Currency

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Base Currency | Dropdown | Required | System accounting currency |
| Currency Symbol Position | Radio | Before ($100), After (100$) | Symbol placement |
| Enable Multi-Currency | Toggle | - | Allow foreign currencies |
| Exchange Rate Source | Dropdown | Manual, SAR API, ECB | Rate source |
| Auto-Update Rates | Toggle | - | Daily rate updates |
| Rate Update Time | Time Picker | If auto-update | When to fetch rates |

##### Section: Currency Display

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Show Currency Code | Toggle | - | Display "SAR" vs "ر.س" |
| Currency Code Position | Radio | Before, After | Code placement |
| Monetary Decimal Places | Number | 0-4 | Currency decimals |

##### Section: Rounding

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Rounding Method | Dropdown | None, Round, Round Up, Round Down, Bankers | How to round |
| Rounding Precision | Dropdown | 0.01, 0.05, 0.10, 0.25, 0.50, 1.00 | Round to nearest |
| Rounding Account | GL Account Selector | Required if rounding | Rounding differences GL |

---

#### 4.3.2 GL Account Defaults (`/settings/financial/gl`)

##### Section: Receivables & Payables

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Default AR Account | GL Account Selector | Required, type=Asset | Accounts Receivable |
| Default AP Account | GL Account Selector | Required, type=Liability | Accounts Payable |
| Customer Advance Account | GL Account Selector | Required | Customer prepayments |
| Vendor Advance Account | GL Account Selector | Required | Vendor prepayments |

##### Section: Revenue & Expense

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Default Sales Account | GL Account Selector | Required, type=Revenue | Sales revenue |
| Default COGS Account | GL Account Selector | Required, type=Expense | Cost of goods sold |
| Default Purchase Account | GL Account Selector | Required, type=Expense | Purchase expense |
| Default Discount Given | GL Account Selector | Required | Sales discounts |
| Default Discount Received | GL Account Selector | Required | Purchase discounts |

##### Section: Other Defaults

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Retained Earnings | GL Account Selector | Required, type=Equity | Year-end closing |
| Suspense Account | GL Account Selector | Required | Unallocated entries |
| Exchange Gain/Loss | GL Account Selector | Required | Currency differences |
| Write-off Account | GL Account Selector | Required | Bad debt write-offs |

---

#### 4.3.3 Document Numbering (`/settings/financial/numbering`)

##### Section: Invoice Numbering

| Field | Type | Example | Description |
|-------|------|---------|-------------|
| Sales Invoice Prefix | Text | INV- | Invoice number prefix |
| Sales Invoice Pattern | Text | {PREFIX}{YYYY}{MM}-{####} | Number pattern |
| Sales Invoice Start | Number | 1 | Starting number |
| Sales Invoice Digits | Number | 4-8 | Padding zeros |
| Reset Annually | Toggle | - | Reset counter yearly |

##### Section: Receipt Numbering

| Field | Type | Example | Description |
|-------|------|---------|-------------|
| Receipt Prefix | Text | REC- | Receipt prefix |
| Receipt Pattern | Text | {PREFIX}{####} | Pattern |
| Receipt Start | Number | 1 | Starting number |

##### Section: Payment Numbering

| Field | Type | Example | Description |
|-------|------|---------|-------------|
| Payment Prefix | Text | PAY- | Payment prefix |
| Payment Pattern | Text | {PREFIX}{YYYY}-{####} | Pattern |
| Payment Start | Number | 1 | Starting number |

##### Section: Journal Numbering

| Field | Type | Example | Description |
|-------|------|---------|-------------|
| Journal Prefix | Text | JV- | Journal voucher prefix |
| Journal Pattern | Text | {PREFIX}{####} | Pattern |
| Separate by Type | Toggle | - | Different series per journal type |

---

#### 4.3.4 Posting Rules (`/settings/financial/posting`)

##### Section: Auto-Posting

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Auto-Post Invoices | Toggle | - | Post on creation |
| Auto-Post Receipts | Toggle | - | Post on creation |
| Auto-Post Payments | Toggle | - | Post on creation |
| Require Approval Before Post | Toggle | - | Approval workflow |
| Approval Threshold | Currency | If approval enabled | Amount requiring approval |

##### Section: Backdating Controls

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Allow Backdating | Toggle | - | Permit past dates |
| Max Backdate Days | Number | 0-365 | Days allowed |
| Require Reason for Backdate | Toggle | - | Mandatory explanation |
| Backdate Approval Required | Toggle | - | Manager approval |

##### Section: Validation Rules

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Require Cost Center | Toggle | - | Mandatory cost center |
| Require Project | Toggle | - | Mandatory project |
| Allow Unbalanced Journals | Toggle | - | For adjustment periods only |
| Warn on Duplicate Reference | Toggle | - | Check for duplicates |

---

### 4.4 Treasury Settings

**Route:** `/settings/treasury`

#### 4.4.1 Cash Settings (`/settings/treasury/cash`)

##### Section: Petty Cash Configuration

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Enable Petty Cash | Toggle | - | Use petty cash module |
| Petty Cash Limit | Currency | Min 0 | Maximum balance |
| Low Balance Alert | Currency | < Limit | Alert threshold |
| Require Receipt | Toggle | - | Mandatory receipt |
| Auto-Replenishment | Toggle | - | Auto-refill |
| Replenishment Threshold | Currency | If auto enabled | When to replenish |

##### Section: Cash GL Accounts

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Default Cash Account | GL Account Selector | Required | Main cash GL |
| Petty Cash Account | GL Account Selector | If petty enabled | Petty cash GL |
| Cash Short/Over Account | GL Account Selector | Required | Discrepancies GL |

---

#### 4.4.2 Bank Settings (`/settings/treasury/bank`)

##### Section: Bank Statement Import

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Enable Auto-Import | Toggle | - | Automatic import |
| Import Format | Dropdown | CSV, MT940, OFX, QFX, CAMT | File format |
| Import Schedule | Dropdown | Daily, Weekly, Manual | Frequency |
| Import Time | Time Picker | If scheduled | When to import |

##### Section: Reconciliation

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Auto-Reconcile | Toggle | - | Automatic matching |
| Match Tolerance | Currency | If auto | Amount tolerance |
| Match by Reference | Toggle | - | Use reference number |
| Reconciliation Account | GL Account Selector | Required | Differences GL |

##### Section: Bank GL Accounts

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Default Bank Account | GL Account Selector | Required | Main bank GL |
| Bank Charges Account | GL Account Selector | Required | Fee expense GL |
| Bank Interest Income | GL Account Selector | Required | Interest revenue GL |
| Bank Interest Expense | GL Account Selector | Required | Interest expense GL |

---

#### 4.4.3 Payment Methods (`/settings/treasury/methods`)

##### Section: Payment Methods Table

| Column | Type | Description |
|--------|------|-------------|
| Code | Text | Method identifier |
| Name (EN) | Text | English name |
| Name (AR) | Text | Arabic name |
| Type | Dropdown | Cash, Bank Transfer, Cheque, Credit Card, Online |
| GL Account | GL Account Selector | Linked account |
| Is Active | Toggle | Enable/disable |
| Is Default | Radio | Default method |
| Sort Order | Number | Display order |

**Actions:** Add, Edit, Delete, Reorder

---

#### 4.4.4 Treasury Auto-Posting (`/settings/treasury/rules`)

##### Section: Auto-Posting Rules

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Auto-Post Deposits | Toggle | - | Post on receipt |
| Auto-Post Withdrawals | Toggle | - | Post on payment |
| Auto-Post Transfers | Toggle | - | Post on transfer |
| Post Cheques on Issue | Toggle | - | Immediate or on clear |
| Post Cheques on Clear | Toggle | - | When cheque clears |

##### Section: Cheque Accounts

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Cheques Issued Account | GL Account Selector | Required | Issued cheques GL |
| Cheques Received Account | GL Account Selector | Required | Received cheques GL |
| Bounced Cheque Account | GL Account Selector | Required | Returned cheques GL |

---

### 4.5 Tax Settings

**Route:** `/settings/tax`

#### 4.5.1 Tax Codes (`/settings/tax/codes`)

##### Section: Tax Codes Table

| Column | Type | Description |
|--------|------|-------------|
| Code | Text | Unique identifier (e.g., VAT15) |
| Name (EN) | Text | English name |
| Name (AR) | Text | Arabic name |
| Tax Type | Dropdown | Standard, Reduced, Zero, Exempt, Reverse Charge |
| Rate (%) | Number | Tax percentage |
| Input Account | GL Account Selector | Input VAT GL |
| Output Account | GL Account Selector | Output VAT GL |
| Is Active | Toggle | Enable/disable |
| Is Default | Toggle | Default for new items |
| ZATCA Category | Dropdown | S, Z, E, O | ZATCA tax category |

**Actions:** Add, Edit, Delete, Set Default

---

#### 4.5.2 Tax Accounts (`/settings/tax/accounts`)

##### Section: VAT Accounts

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| VAT Input Account | GL Account Selector | Required | Recoverable VAT |
| VAT Output Account | GL Account Selector | Required | Payable VAT |
| VAT Payable Account | GL Account Selector | Required | Net VAT liability |
| VAT Receivable Account | GL Account Selector | Optional | Net VAT asset |

##### Section: Withholding Tax

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Enable Withholding Tax | Toggle | - | Use WHT |
| WHT Payable Account | GL Account Selector | If enabled | WHT liability GL |
| WHT Receivable Account | GL Account Selector | If enabled | WHT asset GL |

---

#### 4.5.3 Tax Calculation Rules (`/settings/tax/rules`)

##### Section: Calculation Method

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Tax Calculation | Radio | Exclusive, Inclusive | Tax in/out of price |
| Calculate on | Radio | Line Item, Document Total | When to calculate |
| Round Tax | Dropdown | Per Line, Per Document | Rounding level |

##### Section: Default Behavior

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Default Sales Tax | Dropdown | Tax codes list | New invoice default |
| Default Purchase Tax | Dropdown | Tax codes list | New bill default |
| Apply Tax on Services | Toggle | - | Service items taxable |
| Apply Tax on Shipping | Toggle | - | Delivery taxable |
| Apply Tax on Discounts | Radio | Before Discount, After Discount | Discount timing |

---

#### 4.5.4 Tax Exemptions (`/settings/tax/exemptions`)

##### Section: Exemption Rules Table

| Column | Type | Description |
|--------|------|-------------|
| Name | Text | Rule name |
| Applies To | Dropdown | Customer, Product, Category, Country |
| Condition | Text | Rule condition |
| Tax Rate Override | Number or "Exempt" | Rate to apply |
| Valid From | Date | Start date |
| Valid To | Date | End date |
| Is Active | Toggle | Enable/disable |

---

### 4.6 Fiscal Year Settings

**Route:** `/settings/fiscal`

#### 4.6.1 Fiscal Years (`/settings/fiscal/years`)

##### Section: Current Fiscal Year

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Fiscal Year Name | Text | Required | Display name (e.g., "FY 2025") |
| Start Date | Date | Required | Year start |
| End Date | Date | Required, > Start | Year end |
| Status | Badge | Read-only | Open, Closed, Locked |
| Is Current | Toggle | Only one active | Current year flag |

##### Section: Fiscal Year Defaults

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Fiscal Year Start Month | Dropdown | January-December | Default start month |
| Year Duration | Radio | 12 months, 52 weeks, Custom | Year length |
| Auto-Create Next Year | Toggle | - | Create year automatically |
| Days Before Year End | Number | If auto-create | When to create |

##### Section: Fiscal Years Table

| Column | Type | Description |
|--------|------|-------------|
| Year | Text | Fiscal year name |
| Start Date | Date | Period start |
| End Date | Date | Period end |
| Periods | Number | Count of periods |
| Status | Badge | Open/Closed/Locked |
| Actions | Buttons | Open, Close, Lock, Delete |

---

#### 4.6.2 Fiscal Periods (`/settings/fiscal/periods`)

##### Section: Period Configuration

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Period Type | Radio | Monthly, Quarterly, Custom | Period frequency |
| Number of Periods | Number | 4, 12, 13, 52 | Periods per year |
| Adjustment Period | Toggle | - | Period 13 for adjustments |
| Auto-Generate Periods | Toggle | - | Create periods automatically |

##### Section: Periods Table

| Column | Type | Description |
|--------|------|-------------|
| Period | Number | Period number |
| Name | Text | Period name |
| Start Date | Date | Period start |
| End Date | Date | Period end |
| Status | Badge | Open/Closed/Adjusted/Locked |
| AR Open | Toggle | AR transactions allowed |
| AP Open | Toggle | AP transactions allowed |
| GL Open | Toggle | GL transactions allowed |
| Actions | Buttons | Close, Reopen, Lock |

**Actions:** Generate Periods, Bulk Close, Bulk Open

---

#### 4.6.3 Closing Rules (`/settings/fiscal/closing`)

##### Section: Period Closing

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Auto-Close Periods | Toggle | - | Close after N days |
| Days After Period End | Number | If auto-close | Grace period |
| Require All Modules Closed | Toggle | - | AR, AP, GL all closed |
| Allow Reopen Closed Period | Toggle | - | Permit reopening |
| Require Approval to Reopen | Toggle | If reopen allowed | Manager approval |

##### Section: Year-End Closing

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Closing Method | Radio | Direct, Through Closing Account | How to close |
| Income Summary Account | GL Account Selector | If through account | Temporary closing GL |
| Retained Earnings Account | GL Account Selector | Required | Final equity account |
| Auto-Generate Closing Entry | Toggle | - | Create JV automatically |
| Carry Forward Balances | Multi-select | Which account types | Balance sheet items |

---

### 4.7 Users & Access Settings

**Route:** `/settings/users`

#### 4.7.1 Roles Management (`/settings/users/roles`)

##### Section: Roles Table

| Column | Type | Description |
|--------|------|-------------|
| Role Name | Text | Role identifier |
| Description | Text | Role purpose |
| Users Count | Number | Assigned users |
| Permissions | Badge | Permission count |
| Is System | Badge | Built-in role |
| Is Active | Toggle | Enable/disable |

**Actions:** Add Role, Edit, Clone, Delete

---

#### 4.7.2 Permission Settings (`/settings/users/permissions`)

##### Section: Module Permissions Matrix

| Module | View | Create | Edit | Delete | Approve | Export |
|--------|------|--------|------|--------|---------|--------|
| General Ledger | ☑️ | ☑️ | ☑️ | ☐ | ☑️ | ☑️ |
| Accounts Receivable | ☑️ | ☑️ | ☑️ | ☐ | ☐ | ☑️ |
| Accounts Payable | ☑️ | ☐ | ☐ | ☐ | ☐ | ☐ |
| Treasury | ☑️ | ☑️ | ☑️ | ☐ | ☑️ | ☑️ |
| Reports | ☑️ | ☐ | ☐ | ☐ | ☐ | ☑️ |
| Settings | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |

---

#### 4.7.3 Approval Workflows (`/settings/users/workflows`)

##### Section: Workflow Rules Table

| Column | Type | Description |
|--------|------|-------------|
| Document Type | Dropdown | Invoice, Payment, Journal, etc. |
| Condition | Text | When rule applies |
| Threshold | Currency | Amount trigger |
| Approver Level 1 | User/Role | First approver |
| Approver Level 2 | User/Role | Second approver |
| Timeout (hours) | Number | Auto-escalate after |
| Is Active | Toggle | Enable/disable |

---

### 4.8 Integrations Settings

**Route:** `/settings/integrations`

#### 4.8.1 ZATCA Integration (`/settings/integrations/zatca`)

##### Section: ZATCA Configuration

| Field | Type | Validation | Description |
|-------|------|------------|-------------|
| Enable ZATCA | Toggle | - | Activate integration |
| Environment | Radio | Sandbox, Production | API environment |
| Seller Name | Text | Required | As per ZATCA |
| Seller VAT | Text | Required, 15 digits | VAT number |
| CCSID | Text | Required | Compliance certificate ID |
| PCSID | Text | Required | Production certificate ID |
| Private Key | File Upload | .pem file | Signing key |
| Certificate | File Upload | .pem file | ZATCA certificate |

##### Section: E-Invoice Settings

| Field | Type | Options | Description |
|-------|------|---------|-------------|
| Invoice Type | Multi-select | Standard, Simplified | Invoice types |
| Auto-Submit | Toggle | - | Submit on creation |
| Retry Failed | Toggle | - | Auto-retry failures |
| Max Retries | Number | If retry enabled | Retry attempts |

---

#### 4.8.2 API Keys (`/settings/integrations/api`)

##### Section: API Keys Table

| Column | Type | Description |
|--------|------|-------------|
| Name | Text | Key identifier |
| Key | Masked Text | API key (click to reveal) |
| Created | Date | Creation date |
| Last Used | Date | Last activity |
| Permissions | Badges | Allowed actions |
| Is Active | Toggle | Enable/disable |

**Actions:** Generate Key, Revoke, View Logs

---

#### 4.8.3 Webhooks (`/settings/integrations/webhooks`)

##### Section: Webhooks Table

| Column | Type | Description |
|--------|------|-------------|
| Name | Text | Webhook name |
| URL | URL | Endpoint URL |
| Events | Multi-select | Triggered events |
| Status | Badge | Active/Inactive/Failed |
| Last Triggered | Date | Last execution |
| Success Rate | Percentage | Delivery success |

**Events:** invoice.created, payment.received, journal.posted, etc.

---

## 5. Component Breakdown

### 5.1 Field Components Library

#### Text Input Component

```tsx
interface TextInputProps {
  name: string;
  label: string;
  labelAr?: string;
  placeholder?: string;
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
  disabled?: boolean;
  maxLength?: number;
  pattern?: RegExp;
  errorMessage?: string;
  helpText?: string;
  dir?: 'ltr' | 'rtl';
}
```

#### Dropdown/Select Component

```tsx
interface SelectProps {
  name: string;
  label: string;
  labelAr?: string;
  options: Array<{ value: string; label: string; labelAr?: string }>;
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
  disabled?: boolean;
  searchable?: boolean;
  clearable?: boolean;
  placeholder?: string;
  errorMessage?: string;
}
```

#### Toggle Component

```tsx
interface ToggleProps {
  name: string;
  label: string;
  labelAr?: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
  helpText?: string;
}
```

#### GL Account Selector Component

```tsx
interface GLAccountSelectorProps {
  name: string;
  label: string;
  labelAr?: string;
  value: string | null;
  onChange: (accountId: string | null) => void;
  required?: boolean;
  accountType?: 'Asset' | 'Liability' | 'Equity' | 'Revenue' | 'Expense';
  excludeParents?: boolean;
  placeholder?: string;
  errorMessage?: string;
}
```

#### Currency Input Component

```tsx
interface CurrencyInputProps {
  name: string;
  label: string;
  labelAr?: string;
  value: number;
  onChange: (value: number) => void;
  currency?: string;
  min?: number;
  max?: number;
  required?: boolean;
  disabled?: boolean;
  errorMessage?: string;
}
```

#### File Upload Component

```tsx
interface FileUploadProps {
  name: string;
  label: string;
  labelAr?: string;
  accept: string; // e.g., "image/png,image/jpeg"
  maxSize: number; // bytes
  value: File | string | null;
  onChange: (file: File | null) => void;
  preview?: boolean;
  required?: boolean;
  errorMessage?: string;
}
```

---

### 5.2 Section Component

```tsx
interface SettingsSectionProps {
  title: string;
  titleAr?: string;
  description?: string;
  descriptionAr?: string;
  collapsible?: boolean;
  defaultExpanded?: boolean;
  children: React.ReactNode;
}

// Usage
<SettingsSection
  title="Base Currency"
  titleAr="العملة الأساسية"
  description="Configure your system's primary currency"
  collapsible={true}
  defaultExpanded={true}
>
  <SettingsField ... />
  <SettingsField ... />
</SettingsSection>
```

---

### 5.3 Settings Page Layout Component

```tsx
interface SettingsPageProps {
  title: string;
  titleAr?: string;
  description?: string;
  descriptionAr?: string;
  sections: SettingsSectionProps[];
  onSave: () => Promise<void>;
  onReset: () => void;
  isDirty: boolean;
  isLoading: boolean;
}
```

---

## 6. JSON Schema Structure

### 6.1 Complete Settings Schema

```json
{
  "settings": {
    "general": {
      "system": {
        "name": "string",
        "tagline": "string",
        "logo": "url",
        "favicon": "url",
        "loginBackground": "url"
      },
      "localization": {
        "defaultLanguage": "en | ar | fr",
        "defaultTimezone": "string (IANA)",
        "dateFormat": "DD/MM/YYYY | MM/DD/YYYY | YYYY-MM-DD",
        "timeFormat": "12h | 24h",
        "firstDayOfWeek": "0-6",
        "defaultCountry": "ISO-3166-1"
      },
      "regional": {
        "numberFormat": "string",
        "negativeFormat": "minus | parenthesis | suffix",
        "thousandSeparator": "string",
        "decimalSeparator": "string",
        "decimalPlaces": "number (0-6)"
      }
    },
    "company": {
      "basic": {
        "legalName": "string",
        "tradeName": "string",
        "legalNameAr": "string",
        "companyType": "LLC | Corporation | Partnership | SoleProp",
        "registrationNumber": "string",
        "establishmentDate": "date"
      },
      "tax": {
        "vatNumber": "string",
        "vatRegistrationDate": "date",
        "taxGroup": "Standard | Simplified | Exempt",
        "economicActivityCode": "string"
      },
      "contact": {
        "phone": "string",
        "fax": "string",
        "email": "email",
        "website": "url"
      },
      "address": {
        "street": "string",
        "buildingNumber": "string",
        "additionalNumber": "string",
        "district": "string",
        "city": "string",
        "state": "string",
        "postalCode": "string",
        "country": "ISO-3166-1"
      }
    },
    "financial": {
      "currency": {
        "baseCurrency": "uuid (currency)",
        "symbolPosition": "before | after",
        "enableMultiCurrency": "boolean",
        "exchangeRateSource": "manual | api",
        "autoUpdateRates": "boolean",
        "rateUpdateTime": "time",
        "showCurrencyCode": "boolean",
        "codePosition": "before | after",
        "monetaryDecimals": "number (0-4)",
        "roundingMethod": "none | round | up | down | bankers",
        "roundingPrecision": "number",
        "roundingAccount": "uuid (gl_account)"
      },
      "glAccounts": {
        "defaultAR": "uuid (gl_account)",
        "defaultAP": "uuid (gl_account)",
        "customerAdvance": "uuid (gl_account)",
        "vendorAdvance": "uuid (gl_account)",
        "defaultSales": "uuid (gl_account)",
        "defaultCOGS": "uuid (gl_account)",
        "defaultPurchase": "uuid (gl_account)",
        "discountGiven": "uuid (gl_account)",
        "discountReceived": "uuid (gl_account)",
        "retainedEarnings": "uuid (gl_account)",
        "suspense": "uuid (gl_account)",
        "exchangeGainLoss": "uuid (gl_account)",
        "writeOff": "uuid (gl_account)"
      },
      "numbering": {
        "salesInvoice": {
          "prefix": "string",
          "pattern": "string",
          "startNumber": "number",
          "digits": "number",
          "resetAnnually": "boolean"
        },
        "receipt": {
          "prefix": "string",
          "pattern": "string",
          "startNumber": "number"
        },
        "payment": {
          "prefix": "string",
          "pattern": "string",
          "startNumber": "number"
        },
        "journal": {
          "prefix": "string",
          "pattern": "string",
          "separateByType": "boolean"
        }
      },
      "posting": {
        "autoPostInvoices": "boolean",
        "autoPostReceipts": "boolean",
        "autoPostPayments": "boolean",
        "requireApproval": "boolean",
        "approvalThreshold": "number",
        "allowBackdating": "boolean",
        "maxBackdateDays": "number",
        "requireBackdateReason": "boolean",
        "backdateApproval": "boolean",
        "requireCostCenter": "boolean",
        "requireProject": "boolean",
        "allowUnbalanced": "boolean",
        "warnDuplicateRef": "boolean"
      }
    },
    "treasury": {
      "cash": {
        "enablePettyCash": "boolean",
        "pettyCashLimit": "number",
        "lowBalanceAlert": "number",
        "requireReceipt": "boolean",
        "autoReplenishment": "boolean",
        "replenishmentThreshold": "number",
        "defaultCashAccount": "uuid (gl_account)",
        "pettyCashAccount": "uuid (gl_account)",
        "cashShortOverAccount": "uuid (gl_account)"
      },
      "bank": {
        "enableAutoImport": "boolean",
        "importFormat": "CSV | MT940 | OFX | QFX | CAMT",
        "importSchedule": "daily | weekly | manual",
        "importTime": "time",
        "autoReconcile": "boolean",
        "matchTolerance": "number",
        "matchByReference": "boolean",
        "reconciliationAccount": "uuid (gl_account)",
        "defaultBankAccount": "uuid (gl_account)",
        "bankChargesAccount": "uuid (gl_account)",
        "bankInterestIncome": "uuid (gl_account)",
        "bankInterestExpense": "uuid (gl_account)"
      },
      "paymentMethods": [
        {
          "id": "uuid",
          "code": "string",
          "nameEn": "string",
          "nameAr": "string",
          "type": "cash | bank | cheque | card | online",
          "glAccount": "uuid (gl_account)",
          "isActive": "boolean",
          "isDefault": "boolean",
          "sortOrder": "number"
        }
      ],
      "autoPosting": {
        "autoPostDeposits": "boolean",
        "autoPostWithdrawals": "boolean",
        "autoPostTransfers": "boolean",
        "postChequesOnIssue": "boolean",
        "postChequesOnClear": "boolean",
        "chequesIssuedAccount": "uuid (gl_account)",
        "chequesReceivedAccount": "uuid (gl_account)",
        "bouncedChequeAccount": "uuid (gl_account)"
      }
    },
    "tax": {
      "codes": [
        {
          "id": "uuid",
          "code": "string",
          "nameEn": "string",
          "nameAr": "string",
          "taxType": "standard | reduced | zero | exempt | reverseCharge",
          "rate": "number",
          "inputAccount": "uuid (gl_account)",
          "outputAccount": "uuid (gl_account)",
          "isActive": "boolean",
          "isDefault": "boolean",
          "zatcaCategory": "S | Z | E | O"
        }
      ],
      "accounts": {
        "vatInput": "uuid (gl_account)",
        "vatOutput": "uuid (gl_account)",
        "vatPayable": "uuid (gl_account)",
        "vatReceivable": "uuid (gl_account)",
        "enableWithholding": "boolean",
        "whtPayable": "uuid (gl_account)",
        "whtReceivable": "uuid (gl_account)"
      },
      "rules": {
        "calculationMethod": "exclusive | inclusive",
        "calculateOn": "line | document",
        "roundTax": "perLine | perDocument",
        "defaultSalesTax": "uuid (tax_code)",
        "defaultPurchaseTax": "uuid (tax_code)",
        "taxOnServices": "boolean",
        "taxOnShipping": "boolean",
        "taxOnDiscounts": "before | after"
      },
      "exemptions": [
        {
          "id": "uuid",
          "name": "string",
          "appliesTo": "customer | product | category | country",
          "condition": "string",
          "taxOverride": "number | exempt",
          "validFrom": "date",
          "validTo": "date",
          "isActive": "boolean"
        }
      ]
    },
    "fiscal": {
      "years": {
        "currentYear": "uuid",
        "startMonth": "number (1-12)",
        "yearDuration": "12months | 52weeks | custom",
        "autoCreateNext": "boolean",
        "daysBeforeCreate": "number",
        "list": [
          {
            "id": "uuid",
            "name": "string",
            "startDate": "date",
            "endDate": "date",
            "status": "open | closed | locked",
            "isCurrent": "boolean",
            "periodsCount": "number"
          }
        ]
      },
      "periods": {
        "periodType": "monthly | quarterly | custom",
        "numberOfPeriods": "number",
        "hasAdjustmentPeriod": "boolean",
        "autoGenerate": "boolean"
      },
      "closing": {
        "autoClosePeriods": "boolean",
        "daysAfterPeriodEnd": "number",
        "requireAllModulesClosed": "boolean",
        "allowReopenClosed": "boolean",
        "requireApprovalToReopen": "boolean",
        "closingMethod": "direct | throughAccount",
        "incomeSummaryAccount": "uuid (gl_account)",
        "retainedEarningsAccount": "uuid (gl_account)",
        "autoGenerateClosingEntry": "boolean",
        "carryForwardBalances": ["asset", "liability", "equity"]
      }
    },
    "users": {
      "roles": [
        {
          "id": "uuid",
          "name": "string",
          "description": "string",
          "isSystem": "boolean",
          "isActive": "boolean",
          "permissions": ["string"]
        }
      ],
      "permissions": {
        "modules": {
          "gl": { "view": "boolean", "create": "boolean", "edit": "boolean", "delete": "boolean", "approve": "boolean", "export": "boolean" },
          "ar": { "view": "boolean", "create": "boolean", "edit": "boolean", "delete": "boolean", "approve": "boolean", "export": "boolean" },
          "ap": { "view": "boolean", "create": "boolean", "edit": "boolean", "delete": "boolean", "approve": "boolean", "export": "boolean" },
          "treasury": { "view": "boolean", "create": "boolean", "edit": "boolean", "delete": "boolean", "approve": "boolean", "export": "boolean" },
          "reports": { "view": "boolean", "export": "boolean" },
          "settings": { "view": "boolean", "edit": "boolean" }
        }
      },
      "workflows": [
        {
          "id": "uuid",
          "documentType": "invoice | payment | journal | expense",
          "condition": "string",
          "threshold": "number",
          "approverLevel1": "uuid (user | role)",
          "approverLevel2": "uuid (user | role)",
          "timeoutHours": "number",
          "isActive": "boolean"
        }
      ]
    },
    "integrations": {
      "zatca": {
        "enabled": "boolean",
        "environment": "sandbox | production",
        "sellerName": "string",
        "sellerVat": "string",
        "ccsid": "string",
        "pcsid": "string",
        "privateKey": "string (encrypted)",
        "certificate": "string (encrypted)",
        "invoiceTypes": ["standard", "simplified"],
        "autoSubmit": "boolean",
        "retryFailed": "boolean",
        "maxRetries": "number"
      },
      "apiKeys": [
        {
          "id": "uuid",
          "name": "string",
          "key": "string (hashed)",
          "createdAt": "datetime",
          "lastUsed": "datetime",
          "permissions": ["string"],
          "isActive": "boolean"
        }
      ],
      "webhooks": [
        {
          "id": "uuid",
          "name": "string",
          "url": "url",
          "events": ["string"],
          "status": "active | inactive | failed",
          "lastTriggered": "datetime",
          "successRate": "number"
        }
      ]
    }
  }
}
```

---

## 7. Implementation Guidelines

### 7.1 File Structure

```
src/
├── app/
│   └── [locale]/
│       └── (hydrogen)/
│           └── settings/
│               ├── page.tsx                    # Redirect to /settings/general
│               ├── layout.tsx                  # Settings layout with sidebar
│               ├── general/
│               │   └── page.tsx
│               ├── company/
│               │   └── page.tsx
│               ├── financial/
│               │   ├── page.tsx               # Redirect to currency
│               │   ├── currency/
│               │   │   └── page.tsx
│               │   ├── gl/
│               │   │   └── page.tsx
│               │   ├── numbering/
│               │   │   └── page.tsx
│               │   └── posting/
│               │       └── page.tsx
│               ├── treasury/
│               │   ├── page.tsx
│               │   ├── cash/
│               │   │   └── page.tsx
│               │   ├── bank/
│               │   │   └── page.tsx
│               │   ├── methods/
│               │   │   └── page.tsx
│               │   └── rules/
│               │       └── page.tsx
│               ├── tax/
│               │   ├── page.tsx
│               │   ├── codes/
│               │   │   └── page.tsx
│               │   ├── accounts/
│               │   │   └── page.tsx
│               │   ├── rules/
│               │   │   └── page.tsx
│               │   └── exemptions/
│               │       └── page.tsx
│               ├── fiscal/
│               │   ├── page.tsx
│               │   ├── years/
│               │   │   └── page.tsx
│               │   ├── periods/
│               │   │   └── page.tsx
│               │   └── closing/
│               │       └── page.tsx
│               ├── users/
│               │   ├── page.tsx
│               │   ├── roles/
│               │   │   └── page.tsx
│               │   ├── permissions/
│               │   │   └── page.tsx
│               │   └── workflows/
│               │       └── page.tsx
│               └── integrations/
│                   ├── page.tsx
│                   ├── zatca/
│                   │   └── page.tsx
│                   ├── api/
│                   │   └── page.tsx
│                   └── webhooks/
│                       └── page.tsx
├── components/
│   └── settings/
│       ├── SettingsLayout.tsx
│       ├── SettingsSidebar.tsx
│       ├── SettingsSection.tsx
│       ├── SettingsField.tsx
│       ├── SettingsTable.tsx
│       ├── GLAccountSelector.tsx
│       └── fields/
│           ├── TextInput.tsx
│           ├── SelectInput.tsx
│           ├── ToggleInput.tsx
│           ├── NumberInput.tsx
│           ├── CurrencyInput.tsx
│           ├── DateInput.tsx
│           ├── FileUpload.tsx
│           └── PatternInput.tsx
├── services/
│   └── settings/
│       ├── settings.service.ts
│       ├── general.service.ts
│       ├── company.service.ts
│       ├── financial.service.ts
│       ├── treasury.service.ts
│       ├── tax.service.ts
│       ├── fiscal.service.ts
│       ├── users.service.ts
│       └── integrations.service.ts
├── types/
│   └── settings.types.ts
└── hooks/
    └── useSettings.ts
```

### 7.2 Settings Layout Component

```tsx
// components/settings/SettingsLayout.tsx
'use client';

import { useState } from 'react';
import { usePathname } from 'next/navigation';
import { useLocale } from 'next-intl';
import SettingsSidebar from './SettingsSidebar';
import { PiGearBold, PiMagnifyingGlassBold, PiFloppyDiskBold } from 'react-icons/pi';

interface SettingsLayoutProps {
  children: React.ReactNode;
}

export default function SettingsLayout({ children }: SettingsLayoutProps) {
  const locale = useLocale();
  const isArabic = locale === 'ar';
  const pathname = usePathname();
  const [searchQuery, setSearchQuery] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <header className="sticky top-0 z-40 border-b bg-white dark:bg-gray-800 px-6 py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
              <PiGearBold className="h-5 w-5 text-primary" />
            </div>
            <div>
              <h1 className="text-xl font-semibold text-gray-900 dark:text-white">
                {isArabic ? 'إعدادات النظام' : 'System Settings'}
              </h1>
              <p className="text-sm text-gray-500">
                {isArabic
                  ? 'تكوين تفضيلات النظام والإعدادات المالية'
                  : 'Configure system preferences and financial settings'}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            {/* Search */}
            <div className="relative">
              <PiMagnifyingGlassBold className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
              <input
                type="text"
                placeholder={isArabic ? 'بحث في الإعدادات... (Ctrl+K)' : 'Search settings... (Ctrl+K)'}
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-64 rounded-lg border border-gray-300 bg-gray-50 py-2 pl-10 pr-4 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
              />
            </div>

            {/* Save Button */}
            <button
              disabled={isSaving}
              className="flex items-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white hover:bg-primary/90 disabled:opacity-50"
            >
              <PiFloppyDiskBold className="h-4 w-4" />
              {isArabic ? 'حفظ التغييرات' : 'Save Changes'}
            </button>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <div className="flex">
        {/* Sidebar */}
        <SettingsSidebar searchQuery={searchQuery} />

        {/* Content Area */}
        <main className="flex-1 p-6">
          <div className="mx-auto max-w-4xl">
            {children}
          </div>
        </main>
      </div>
    </div>
  );
}
```

### 7.3 Settings Sidebar Component

```tsx
// components/settings/SettingsSidebar.tsx
'use client';

import { useState } from 'react';
import { usePathname } from 'next/navigation';
import { useLocale } from 'next-intl';
import Link from 'next/link';
import {
  PiGearDuotone,
  PiBuildingOfficeDuotone,
  PiCurrencyDollarDuotone,
  PiVaultDuotone,
  PiReceiptDuotone,
  PiCalendarDuotone,
  PiUsersDuotone,
  PiPlugDuotone,
  PiCaretDownBold,
  PiCaretRightBold
} from 'react-icons/pi';

const sidebarItems = [
  {
    id: 'general',
    href: '/settings/general',
    icon: PiGearDuotone,
    label: 'General',
    labelAr: 'عام'
  },
  {
    id: 'company',
    href: '/settings/company',
    icon: PiBuildingOfficeDuotone,
    label: 'Company Profile',
    labelAr: 'ملف الشركة'
  },
  {
    id: 'financial',
    icon: PiCurrencyDollarDuotone,
    label: 'Financial',
    labelAr: 'المالية',
    children: [
      { id: 'financial-currency', href: '/settings/financial/currency', label: 'Currency', labelAr: 'العملات' },
      { id: 'financial-gl', href: '/settings/financial/gl', label: 'GL Accounts', labelAr: 'حسابات الأستاذ' },
      { id: 'financial-numbering', href: '/settings/financial/numbering', label: 'Numbering', labelAr: 'الترقيم' },
      { id: 'financial-posting', href: '/settings/financial/posting', label: 'Posting Rules', labelAr: 'قواعد الترحيل' }
    ]
  },
  {
    id: 'treasury',
    icon: PiVaultDuotone,
    label: 'Treasury',
    labelAr: 'الخزينة',
    children: [
      { id: 'treasury-cash', href: '/settings/treasury/cash', label: 'Cash & Petty Cash', labelAr: 'النقدية' },
      { id: 'treasury-bank', href: '/settings/treasury/bank', label: 'Bank Accounts', labelAr: 'الحسابات البنكية' },
      { id: 'treasury-methods', href: '/settings/treasury/methods', label: 'Payment Methods', labelAr: 'طرق الدفع' },
      { id: 'treasury-rules', href: '/settings/treasury/rules', label: 'Auto-Posting', labelAr: 'الترحيل التلقائي' }
    ]
  },
  {
    id: 'tax',
    icon: PiReceiptDuotone,
    label: 'Tax',
    labelAr: 'الضرائب',
    children: [
      { id: 'tax-codes', href: '/settings/tax/codes', label: 'Tax Codes', labelAr: 'رموز الضرائب' },
      { id: 'tax-accounts', href: '/settings/tax/accounts', label: 'Tax Accounts', labelAr: 'حسابات الضرائب' },
      { id: 'tax-rules', href: '/settings/tax/rules', label: 'Calculation Rules', labelAr: 'قواعد الحساب' },
      { id: 'tax-exemptions', href: '/settings/tax/exemptions', label: 'Exemptions', labelAr: 'الإعفاءات' }
    ]
  },
  {
    id: 'fiscal',
    icon: PiCalendarDuotone,
    label: 'Fiscal Year',
    labelAr: 'السنة المالية',
    children: [
      { id: 'fiscal-years', href: '/settings/fiscal/years', label: 'Years', labelAr: 'السنوات' },
      { id: 'fiscal-periods', href: '/settings/fiscal/periods', label: 'Periods', labelAr: 'الفترات' },
      { id: 'fiscal-closing', href: '/settings/fiscal/closing', label: 'Closing Rules', labelAr: 'قواعد الإغلاق' }
    ]
  },
  {
    id: 'users',
    icon: PiUsersDuotone,
    label: 'Users & Access',
    labelAr: 'المستخدمين والصلاحيات',
    children: [
      { id: 'users-roles', href: '/settings/users/roles', label: 'Roles', labelAr: 'الأدوار' },
      { id: 'users-permissions', href: '/settings/users/permissions', label: 'Permissions', labelAr: 'الصلاحيات' },
      { id: 'users-workflows', href: '/settings/users/workflows', label: 'Approval Workflows', labelAr: 'سير الموافقات' }
    ]
  },
  {
    id: 'integrations',
    icon: PiPlugDuotone,
    label: 'Integrations',
    labelAr: 'التكاملات',
    badge: 'NEW',
    children: [
      { id: 'integrations-zatca', href: '/settings/integrations/zatca', label: 'ZATCA', labelAr: 'زاتكا' },
      { id: 'integrations-api', href: '/settings/integrations/api', label: 'API Keys', labelAr: 'مفاتيح API' },
      { id: 'integrations-webhooks', href: '/settings/integrations/webhooks', label: 'Webhooks', labelAr: 'ويب هوكس' }
    ]
  }
];

interface SettingsSidebarProps {
  searchQuery: string;
}

export default function SettingsSidebar({ searchQuery }: SettingsSidebarProps) {
  const locale = useLocale();
  const isArabic = locale === 'ar';
  const pathname = usePathname();
  const [expandedItems, setExpandedItems] = useState<string[]>(['financial', 'treasury', 'tax', 'fiscal']);

  const toggleExpand = (id: string) => {
    setExpandedItems(prev =>
      prev.includes(id)
        ? prev.filter(item => item !== id)
        : [...prev, id]
    );
  };

  const isActive = (href: string) => pathname?.includes(href);
  const isParentActive = (children: any[]) => children?.some(child => pathname?.includes(child.href));

  // Filter items based on search
  const filteredItems = searchQuery
    ? sidebarItems.filter(item => {
        const matchesParent = item.label.toLowerCase().includes(searchQuery.toLowerCase()) ||
                              item.labelAr?.includes(searchQuery);
        const matchesChild = item.children?.some(child =>
          child.label.toLowerCase().includes(searchQuery.toLowerCase()) ||
          child.labelAr?.includes(searchQuery)
        );
        return matchesParent || matchesChild;
      })
    : sidebarItems;

  return (
    <aside className="sticky top-[73px] h-[calc(100vh-73px)] w-60 shrink-0 overflow-y-auto border-r bg-white dark:bg-gray-800 p-4">
      <nav className="space-y-1">
        {filteredItems.map(item => (
          <div key={item.id}>
            {item.children ? (
              // Parent with children
              <>
                <button
                  onClick={() => toggleExpand(item.id)}
                  className={`w-full flex items-center justify-between rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                    isParentActive(item.children)
                      ? 'bg-primary/10 text-primary'
                      : 'text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <item.icon className="h-5 w-5" />
                    <span>{isArabic ? item.labelAr : item.label}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    {item.badge && (
                      <span className="rounded-full bg-green-100 px-2 py-0.5 text-xs font-medium text-green-700">
                        {item.badge}
                      </span>
                    )}
                    {expandedItems.includes(item.id) ? (
                      <PiCaretDownBold className="h-4 w-4" />
                    ) : (
                      <PiCaretRightBold className="h-4 w-4" />
                    )}
                  </div>
                </button>

                {expandedItems.includes(item.id) && (
                  <div className="ml-4 mt-1 space-y-1 border-l pl-4">
                    {item.children.map(child => (
                      <Link
                        key={child.id}
                        href={`/${locale}${child.href}`}
                        className={`block rounded-lg px-3 py-2 text-sm transition-colors ${
                          isActive(child.href)
                            ? 'bg-primary/10 text-primary font-medium'
                            : 'text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-700'
                        }`}
                      >
                        {isArabic ? child.labelAr : child.label}
                      </Link>
                    ))}
                  </div>
                )}
              </>
            ) : (
              // Simple link
              <Link
                href={`/${locale}${item.href}`}
                className={`flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                  isActive(item.href!)
                    ? 'bg-primary/10 text-primary'
                    : 'text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700'
                }`}
              >
                <item.icon className="h-5 w-5" />
                <span>{isArabic ? item.labelAr : item.label}</span>
              </Link>
            )}
          </div>
        ))}
      </nav>

      {/* Version info */}
      <div className="mt-8 border-t pt-4">
        <p className="text-xs text-gray-400 text-center">
          v2.5.0
        </p>
      </div>
    </aside>
  );
}
```

---

## 8. Best Practices & Recommendations

### 8.1 UX Best Practices

| Practice | Implementation |
|----------|----------------|
| **Progressive Disclosure** | Show advanced settings collapsed by default |
| **Immediate Feedback** | Validate on blur, show errors inline |
| **Undo Support** | Allow reverting to last saved state |
| **Confirmation Dialogs** | For destructive actions only |
| **Loading States** | Skeleton loaders during data fetch |
| **Empty States** | Helpful messages when no data |
| **Keyboard Navigation** | Full keyboard accessibility |
| **Search Everywhere** | Global search with Ctrl+K shortcut |

### 8.2 Performance Recommendations

| Area | Recommendation |
|------|----------------|
| **Lazy Loading** | Load settings sections on navigation |
| **Debounced Save** | Auto-save with 500ms debounce |
| **Optimistic Updates** | Update UI before server confirmation |
| **Caching** | Cache settings locally with SWR/React Query |
| **Bundle Splitting** | Separate chunks per settings category |

### 8.3 Security Recommendations

| Area | Implementation |
|------|----------------|
| **Permission Checks** | Check permissions before showing settings |
| **Audit Logging** | Log all settings changes with user/timestamp |
| **Sensitive Data** | Mask API keys, certificates |
| **CSRF Protection** | Include tokens on all mutations |
| **Rate Limiting** | Limit settings API calls |

### 8.4 Scalability Recommendations

| Area | Approach |
|------|----------|
| **Multi-Tenant** | Settings scoped to company/tenant |
| **Versioning** | Track settings schema versions |
| **Migration** | Automated settings migration scripts |
| **Export/Import** | Allow settings backup/restore |
| **Templates** | Predefined settings templates (Industry defaults) |

### 8.5 Accessibility Requirements

| Requirement | Implementation |
|-------------|----------------|
| **WCAG 2.1 AA** | All form controls labeled |
| **Color Contrast** | 4.5:1 minimum ratio |
| **Focus Indicators** | Visible focus rings |
| **Screen Readers** | ARIA labels, live regions |
| **Keyboard Only** | Full functionality without mouse |
| **RTL Support** | Complete Arabic/RTL layout |

---

## 9. API Endpoints Summary

```
GET    /api/settings                       # Get all settings
GET    /api/settings/{category}            # Get category settings
PUT    /api/settings/{category}            # Update category settings
POST   /api/settings/reset/{category}      # Reset to defaults

GET    /api/settings/financial/gl-accounts # Get GL account options
GET    /api/settings/tax/codes             # Get tax codes list
POST   /api/settings/tax/codes             # Create tax code
PUT    /api/settings/tax/codes/{id}        # Update tax code
DELETE /api/settings/tax/codes/{id}        # Delete tax code

GET    /api/settings/treasury/methods      # Get payment methods
POST   /api/settings/treasury/methods      # Create payment method
PUT    /api/settings/treasury/methods/{id} # Update payment method
DELETE /api/settings/treasury/methods/{id} # Delete payment method

GET    /api/settings/fiscal/years          # Get fiscal years
POST   /api/settings/fiscal/years          # Create fiscal year
PUT    /api/settings/fiscal/years/{id}     # Update fiscal year
POST   /api/settings/fiscal/years/{id}/close   # Close fiscal year
POST   /api/settings/fiscal/years/{id}/reopen  # Reopen fiscal year

GET    /api/settings/fiscal/periods        # Get fiscal periods
POST   /api/settings/fiscal/periods/generate   # Generate periods
POST   /api/settings/fiscal/periods/{id}/close # Close period
POST   /api/settings/fiscal/periods/{id}/reopen # Reopen period

GET    /api/settings/users/roles           # Get roles
POST   /api/settings/users/roles           # Create role
PUT    /api/settings/users/roles/{id}      # Update role
DELETE /api/settings/users/roles/{id}      # Delete role

GET    /api/settings/integrations/zatca/status  # ZATCA connection status
POST   /api/settings/integrations/zatca/test    # Test ZATCA connection
POST   /api/settings/integrations/api-keys      # Generate API key
DELETE /api/settings/integrations/api-keys/{id} # Revoke API key
```

---

## 10. Migration Path

### From Current to New Structure

| Current Page | New Location |
|--------------|--------------|
| `/finance/treasury/config` (operational) | `/settings/treasury/bank` |
| `/finance/treasury/config` (GL mappings) | `/settings/financial/gl` |
| `/finance/configuration/settings` | `/settings/financial/posting` + `/settings/company` |
| `/finance/configuration/taxes/settings` | `/settings/tax/accounts` |
| `/finance/configuration/taxes/codes` | `/settings/tax/codes` |
| `/finance/configuration/taxes/groups` | `/settings/tax/codes` (merged) |
| `/finance/configuration/taxes/rates` | Deprecated (merged into codes) |
| `/finance/configuration/fiscal-years` | `/settings/fiscal/years` |
| `/finance/configuration/fiscal-periods` | `/settings/fiscal/periods` |
| `/finance/configuration/currencies` | `/settings/financial/currency` |
| `/finance/configuration/exchange-rates` | `/settings/financial/currency` (tab) |
| `/finance/configuration/cost-centers` | Remains separate (operational data) |

---

*End of Design Specification*
