# ERP System Design - نظام تخطيط موارد المؤسسات

## Overview - نظرة عامة

This ERP system is designed for gym/fitness centers, retail businesses, and service-based companies with full accounting, inventory, HR, and AI agent integration capabilities.

---

## Module Architecture - هيكل الوحدات

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         AI AGENT ORCHESTRATION LAYER                        │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐          │
│  │ Thinking │ │ Planning │ │ Decision │ │ Learning │ │ Execution│          │
│  │  Agent   │ │  Agent   │ │  Agent   │ │  Agent   │ │  Agent   │          │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘          │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
┌─────────────────────────────────────────────────────────────────────────────┐
│                            BUSINESS MODULES                                 │
├─────────────┬─────────────┬─────────────┬─────────────┬─────────────────────┤
│   SALES     │ SUBSCRIPTIONS│ INVENTORY  │  ACCOUNTS   │    HR & PAYROLL    │
│  المبيعات   │  الإشتراكات  │   المخزن   │  الحسابات   │ الموظفين والرواتب  │
├─────────────┼─────────────┼─────────────┼─────────────┼─────────────────────┤
│  SUPPLIERS  │  MARKETING  │  BRANCHES   │  TREASURY   │     REPORTS        │
│  الموردين   │   التسويق   │   الفروع    │   الخزائن   │     التقارير       │
└─────────────┴─────────────┴─────────────┴─────────────┴─────────────────────┘
```

---

## 1. Sales Module - وحدة المبيعات

### 1.1 Models - النماذج

```
ZR.Model/ERP/Sales/
├── ErpSalesOrder.cs              # أمر البيع
├── ErpSalesOrderItem.cs          # بنود أمر البيع
├── ErpSalesInvoice.cs            # فاتورة المبيعات
├── ErpSalesInvoiceItem.cs        # بنود الفاتورة
├── ErpSalesReturn.cs             # مردودات المبيعات
├── ErpSalesReturnItem.cs         # بنود المردودات
├── ErpCustomer.cs                # العملاء
├── ErpCustomerBalance.cs         # أرصدة العملاء
├── ErpAdvancePayment.cs          # دفعات مقدمة
├── ErpQuotation.cs               # عروض الأسعار
└── Dto/
    ├── SalesOrderDto.cs
    ├── SalesInvoiceDto.cs
    └── CustomerDto.cs
```

### 1.2 Database Schema

```sql
-- Sales Order - أمر البيع
CREATE TABLE erp_sales_orders (
    order_id BIGINT PRIMARY KEY,
    order_number VARCHAR(50) NOT NULL,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    customer_id BIGINT REFERENCES erp_customers(customer_id),
    branch_id BIGINT,
    status VARCHAR(20) DEFAULT 'draft', -- draft/confirmed/delivered/cancelled
    total_amount DECIMAL(18,2),
    discount_amount DECIMAL(18,2) DEFAULT 0,
    tax_amount DECIMAL(18,2) DEFAULT 0,
    net_amount DECIMAL(18,2),
    payment_method_id BIGINT,
    salesperson_id BIGINT,
    notes TEXT,
    create_by VARCHAR(50),
    create_time TIMESTAMP,
    update_by VARCHAR(50),
    update_time TIMESTAMP,
    company_id INT,
    branch_id INT
);

-- Customer - العميل
CREATE TABLE erp_customers (
    customer_id BIGINT PRIMARY KEY,
    customer_code VARCHAR(50) UNIQUE,
    customer_name VARCHAR(200) NOT NULL,
    customer_name_ar VARCHAR(200),
    customer_type VARCHAR(20), -- individual/corporate
    phone VARCHAR(20),
    mobile VARCHAR(20),
    email VARCHAR(100),
    vat_number VARCHAR(50),
    credit_limit DECIMAL(18,2) DEFAULT 0,
    current_balance DECIMAL(18,2) DEFAULT 0,
    address TEXT,
    city VARCHAR(100),
    country VARCHAR(100) DEFAULT 'Saudi Arabia',
    is_active BOOLEAN DEFAULT TRUE,
    create_by VARCHAR(50),
    create_time TIMESTAMP,
    company_id INT
);

-- Customer Balance - رصيد العميل
CREATE TABLE erp_customer_balances (
    balance_id BIGINT PRIMARY KEY,
    customer_id BIGINT REFERENCES erp_customers(customer_id),
    transaction_date TIMESTAMP,
    transaction_type VARCHAR(20), -- invoice/payment/return/adjustment
    reference_type VARCHAR(50),
    reference_id BIGINT,
    reference_number VARCHAR(50),
    debit_amount DECIMAL(18,2) DEFAULT 0,
    credit_amount DECIMAL(18,2) DEFAULT 0,
    balance DECIMAL(18,2),
    notes TEXT,
    create_time TIMESTAMP
);

-- Advance Payments - الدفعات المقدمة
CREATE TABLE erp_advance_payments (
    advance_id BIGINT PRIMARY KEY,
    advance_number VARCHAR(50),
    customer_id BIGINT,
    payment_date TIMESTAMP,
    amount DECIMAL(18,2),
    payment_method_id BIGINT,
    treasury_id BIGINT,
    used_amount DECIMAL(18,2) DEFAULT 0,
    remaining_amount DECIMAL(18,2),
    status VARCHAR(20), -- pending/confirmed/used/cancelled
    notes TEXT,
    create_by VARCHAR(50),
    create_time TIMESTAMP
);
```

---

## 2. Subscriptions Module - وحدة الإشتراكات (Gym/Fitness)

### 2.1 Models

```
ZR.Model/ERP/Subscriptions/
├── ErpSubscriptionPlan.cs        # باقات الإشتراك
├── ErpSubscription.cs            # الإشتراكات
├── ErpSubscriptionPayment.cs     # مدفوعات الإشتراك
├── ErpMember.cs                  # المشتركين
├── ErpMemberVisit.cs             # زيارات المشتركين
├── ErpResource.cs                # الموارد والغرف
├── ErpResourceBooking.cs         # حجوزات الموارد
├── ErpServiceCategory.cs         # فئات الخدمات
├── ErpService.cs                 # الخدمات
└── Dto/
```

### 2.2 Database Schema

```sql
-- Subscription Plan - باقة الإشتراك
CREATE TABLE erp_subscription_plans (
    plan_id BIGINT PRIMARY KEY,
    plan_code VARCHAR(50),
    plan_name VARCHAR(200),
    plan_name_ar VARCHAR(200),
    duration_days INT,
    duration_months INT,
    price DECIMAL(18,2),
    discount_percent DECIMAL(5,2) DEFAULT 0,
    features JSONB, -- مميزات الباقة
    max_visits INT, -- عدد الزيارات (0 = غير محدود)
    includes_trainer BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    create_time TIMESTAMP
);

-- Subscription - الإشتراك
CREATE TABLE erp_subscriptions (
    subscription_id BIGINT PRIMARY KEY,
    subscription_number VARCHAR(50),
    member_id BIGINT REFERENCES erp_members(member_id),
    plan_id BIGINT REFERENCES erp_subscription_plans(plan_id),
    start_date DATE,
    end_date DATE,
    status VARCHAR(20), -- active/expired/suspended/cancelled
    total_amount DECIMAL(18,2),
    paid_amount DECIMAL(18,2) DEFAULT 0,
    remaining_amount DECIMAL(18,2),
    remaining_visits INT,
    trainer_id BIGINT,
    branch_id BIGINT,
    notes TEXT,
    create_time TIMESTAMP
);

-- Member - المشترك
CREATE TABLE erp_members (
    member_id BIGINT PRIMARY KEY,
    member_code VARCHAR(50) UNIQUE,
    full_name VARCHAR(200),
    full_name_ar VARCHAR(200),
    national_id VARCHAR(20),
    phone VARCHAR(20),
    email VARCHAR(100),
    gender VARCHAR(10),
    birth_date DATE,
    photo_url VARCHAR(500),
    emergency_contact VARCHAR(100),
    emergency_phone VARCHAR(20),
    health_notes TEXT,
    membership_start_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    create_time TIMESTAMP
);

-- Resource/Room - المورد/الغرفة
CREATE TABLE erp_resources (
    resource_id BIGINT PRIMARY KEY,
    resource_code VARCHAR(50),
    resource_name VARCHAR(200),
    resource_name_ar VARCHAR(200),
    resource_type VARCHAR(50), -- room/equipment/trainer
    capacity INT,
    hourly_rate DECIMAL(18,2),
    location VARCHAR(200),
    is_bookable BOOLEAN DEFAULT TRUE,
    is_active BOOLEAN DEFAULT TRUE
);
```

---

## 3. Inventory Module - وحدة المخزن

### 3.1 Models

```
ZR.Model/ERP/Inventory/
├── ErpWarehouse.cs               # المخازن
├── ErpProduct.cs                 # المنتجات
├── ErpProductCategory.cs         # فئات المنتجات
├── ErpInventoryTransaction.cs    # حركات المخزن
├── ErpPurchaseOrder.cs           # أوامر الشراء
├── ErpPurchaseInvoice.cs         # فواتير المشتريات
├── ErpPurchaseReturn.cs          # مردودات المشتريات
├── ErpStockTransfer.cs           # تحويلات المخزن
├── ErpCustodyIssuance.cs         # صرف العهدة
├── ErpCustodySettlement.cs       # تسوية العهدة
├── ErpDamagedItems.cs            # العناصر التالفة
├── ErpStockCount.cs              # جرد المخزن
├── ErpStockAdjustment.cs         # تسويات المخزن
└── Dto/
```

### 3.2 Database Schema

```sql
-- Warehouse - المخزن
CREATE TABLE erp_warehouses (
    warehouse_id BIGINT PRIMARY KEY,
    warehouse_code VARCHAR(50),
    warehouse_name VARCHAR(200),
    warehouse_name_ar VARCHAR(200),
    location VARCHAR(500),
    branch_id BIGINT,
    is_default BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE
);

-- Product - المنتج
CREATE TABLE erp_products (
    product_id BIGINT PRIMARY KEY,
    product_code VARCHAR(50) UNIQUE,
    barcode VARCHAR(50),
    product_name VARCHAR(200),
    product_name_ar VARCHAR(200),
    category_id BIGINT,
    unit_of_measure VARCHAR(50),
    cost_price DECIMAL(18,2),
    selling_price DECIMAL(18,2),
    vat_percent DECIMAL(5,2) DEFAULT 15,
    reorder_level INT DEFAULT 0,
    is_service BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE
);

-- Inventory Transaction - حركة المخزن
CREATE TABLE erp_inventory_transactions (
    transaction_id BIGINT PRIMARY KEY,
    transaction_number VARCHAR(50),
    transaction_date TIMESTAMP,
    transaction_type VARCHAR(50),
    -- Types: opening_balance, direct_addition, direct_return,
    -- purchase_order, purchase_invoice, purchase_return,
    -- stock_transfer, custody_issue, custody_settle,
    -- damaged, stock_count, adjustment, sale
    warehouse_id BIGINT,
    product_id BIGINT,
    quantity DECIMAL(18,3),
    unit_cost DECIMAL(18,2),
    total_cost DECIMAL(18,2),
    reference_type VARCHAR(50),
    reference_id BIGINT,
    from_warehouse_id BIGINT, -- للتحويلات
    to_warehouse_id BIGINT,   -- للتحويلات
    employee_id BIGINT,       -- للعهد
    notes TEXT,
    status VARCHAR(20),
    create_by VARCHAR(50),
    create_time TIMESTAMP
);

-- Stock Balance - رصيد المخزن
CREATE TABLE erp_stock_balances (
    balance_id BIGINT PRIMARY KEY,
    warehouse_id BIGINT,
    product_id BIGINT,
    quantity_on_hand DECIMAL(18,3),
    quantity_reserved DECIMAL(18,3) DEFAULT 0,
    quantity_available DECIMAL(18,3),
    average_cost DECIMAL(18,2),
    last_cost DECIMAL(18,2),
    last_transaction_date TIMESTAMP,
    UNIQUE(warehouse_id, product_id)
);
```

### 3.3 Transaction Types - أنواع المعاملات

| Type | Arabic | Description |
|------|--------|-------------|
| opening_balance | رصيد إفتتاحي | Initial stock entry |
| direct_addition | إضافة مباشرة | Direct stock addition |
| direct_return | مردود إضافة مباشرة | Return of direct addition |
| purchase_order | أمر شراء | Purchase order |
| purchase_invoice | فاتورة مشتريات | Purchase invoice |
| purchase_return | فاتورة مردود مشتريات | Purchase return |
| stock_transfer | تحويل لمخزن أخر | Transfer to another warehouse |
| custody_issue | صرف عهدة لموظف | Employee custody issuance |
| custody_settle | تسوية عهدة مخزنية | Custody settlement |
| damaged | العناصر التالفة | Damaged items |
| stock_count | جرد مخزن | Physical inventory count |
| adjustment | تسويات مخزنية | Stock adjustments |

---

## 4. Accounts Module - وحدة الحسابات

### 4.1 Models

```
ZR.Model/ERP/Accounts/
├── ErpAccount.cs                 # دليل الحسابات
├── ErpCostCenter.cs              # مراكز التكلفة
├── ErpFinancialPeriod.cs         # الفترات المالية
├── ErpJournalEntry.cs            # القيود اليومية
├── ErpJournalEntryLine.cs        # بنود القيد
├── ErpReceipt.cs                 # المقبوضات
├── ErpPaymentVoucher.cs          # المدفوعات
├── ErpAccountStatement.cs        # كشف حساب
├── ErpExpense.cs                 # المصروفات
├── ErpExpenseCategory.cs         # فئات المصروفات
└── Dto/
```

### 4.2 Database Schema

```sql
-- Chart of Accounts - دليل الحسابات
CREATE TABLE erp_accounts (
    account_id BIGINT PRIMARY KEY,
    parent_account_id BIGINT REFERENCES erp_accounts(account_id),
    account_number VARCHAR(20) UNIQUE,
    account_name VARCHAR(200),
    account_name_ar VARCHAR(200),
    account_type VARCHAR(20), -- asset/liability/equity/revenue/expense
    account_nature VARCHAR(10), -- debit/credit
    account_level INT,
    is_parent BOOLEAN DEFAULT FALSE,
    is_leaf BOOLEAN DEFAULT TRUE,
    opening_balance DECIMAL(18,2) DEFAULT 0,
    current_balance DECIMAL(18,2) DEFAULT 0,
    is_system_account BOOLEAN DEFAULT FALSE,
    is_cash_account BOOLEAN DEFAULT FALSE,
    is_bank_account BOOLEAN DEFAULT FALSE,
    requires_cost_center BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE
);

-- Cost Center - مركز التكلفة
CREATE TABLE erp_cost_centers (
    cost_center_id BIGINT PRIMARY KEY,
    parent_id BIGINT,
    cost_center_code VARCHAR(50),
    cost_center_name VARCHAR(200),
    cost_center_name_ar VARCHAR(200),
    cost_center_type VARCHAR(50), -- branch/department/project
    budget_amount DECIMAL(18,2),
    actual_amount DECIMAL(18,2) DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE
);

-- Financial Period - الفترة المالية
CREATE TABLE erp_financial_periods (
    period_id BIGINT PRIMARY KEY,
    period_name VARCHAR(100),
    fiscal_year INT,
    period_number INT, -- 1-12 for months
    start_date DATE,
    end_date DATE,
    status VARCHAR(20), -- open/closed/locked
    closed_by VARCHAR(50),
    closed_date TIMESTAMP
);

-- Journal Entry - القيد اليومي
CREATE TABLE erp_journal_entries (
    entry_id BIGINT PRIMARY KEY,
    entry_number VARCHAR(50) UNIQUE,
    entry_date DATE,
    entry_type VARCHAR(50), -- manual/auto/opening/closing
    period_id BIGINT,
    description TEXT,
    total_debit DECIMAL(18,2),
    total_credit DECIMAL(18,2),
    reference_type VARCHAR(50),
    reference_id BIGINT,
    status VARCHAR(20), -- draft/posted/cancelled
    posted_by VARCHAR(50),
    posted_date TIMESTAMP,
    create_by VARCHAR(50),
    create_time TIMESTAMP
);

-- Journal Entry Lines - بنود القيد
CREATE TABLE erp_journal_entry_lines (
    line_id BIGINT PRIMARY KEY,
    entry_id BIGINT REFERENCES erp_journal_entries(entry_id),
    account_id BIGINT REFERENCES erp_accounts(account_id),
    cost_center_id BIGINT,
    debit_amount DECIMAL(18,2) DEFAULT 0,
    credit_amount DECIMAL(18,2) DEFAULT 0,
    description VARCHAR(500),
    line_order INT
);

-- Receipt - المقبوضات (سند قبض)
CREATE TABLE erp_receipts (
    receipt_id BIGINT PRIMARY KEY,
    receipt_number VARCHAR(50),
    receipt_date DATE,
    customer_id BIGINT,
    amount DECIMAL(18,2),
    payment_method_id BIGINT,
    treasury_id BIGINT,
    bank_account_id BIGINT,
    reference_type VARCHAR(50), -- invoice/advance/other
    reference_id BIGINT,
    description TEXT,
    journal_entry_id BIGINT,
    status VARCHAR(20),
    create_by VARCHAR(50),
    create_time TIMESTAMP
);

-- Payment Voucher - المدفوعات (سند صرف)
CREATE TABLE erp_payment_vouchers (
    payment_id BIGINT PRIMARY KEY,
    payment_number VARCHAR(50),
    payment_date DATE,
    supplier_id BIGINT,
    employee_id BIGINT,
    amount DECIMAL(18,2),
    payment_method_id BIGINT,
    treasury_id BIGINT,
    bank_account_id BIGINT,
    payment_type VARCHAR(50), -- supplier/employee/expense
    reference_type VARCHAR(50),
    reference_id BIGINT,
    description TEXT,
    journal_entry_id BIGINT,
    status VARCHAR(20),
    create_by VARCHAR(50),
    create_time TIMESTAMP
);

-- Expense - المصروفات
CREATE TABLE erp_expenses (
    expense_id BIGINT PRIMARY KEY,
    expense_number VARCHAR(50),
    expense_date DATE,
    expense_category_id BIGINT,
    amount DECIMAL(18,2),
    vat_amount DECIMAL(18,2) DEFAULT 0,
    total_amount DECIMAL(18,2),
    payment_method_id BIGINT,
    treasury_id BIGINT,
    cost_center_id BIGINT,
    supplier_id BIGINT,
    description TEXT,
    attachment_url VARCHAR(500),
    journal_entry_id BIGINT,
    status VARCHAR(20),
    create_by VARCHAR(50),
    create_time TIMESTAMP
);
```

---

## 5. Treasury Module - وحدة الخزائن

### 5.1 Models

```
ZR.Model/ERP/Treasury/
├── ErpTreasury.cs                # الخزينة
├── ErpTreasuryTransaction.cs     # حركات الخزينة
├── ErpTreasuryTransfer.cs        # تحويلات الرصيد
├── ErpPaymentMethod.cs           # طرق الدفع
├── ErpCurrency.cs                # العملات
├── ErpCurrencyRate.cs            # أسعار الصرف
└── Dto/
```

### 5.2 Database Schema

```sql
-- Treasury - الخزينة
CREATE TABLE erp_treasuries (
    treasury_id BIGINT PRIMARY KEY,
    treasury_code VARCHAR(50),
    treasury_name VARCHAR(200),
    treasury_name_ar VARCHAR(200),
    treasury_type VARCHAR(50), -- cash/bank/pos
    currency_code VARCHAR(10) DEFAULT 'SAR',
    opening_balance DECIMAL(18,2) DEFAULT 0,
    current_balance DECIMAL(18,2) DEFAULT 0,
    branch_id BIGINT,
    responsible_employee_id BIGINT,
    is_default BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE
);

-- Treasury Transaction - حركة الخزينة
CREATE TABLE erp_treasury_transactions (
    transaction_id BIGINT PRIMARY KEY,
    treasury_id BIGINT,
    transaction_date TIMESTAMP,
    transaction_type VARCHAR(50), -- receipt/payment/transfer_in/transfer_out
    amount DECIMAL(18,2),
    balance_after DECIMAL(18,2),
    reference_type VARCHAR(50),
    reference_id BIGINT,
    reference_number VARCHAR(50),
    description TEXT,
    create_by VARCHAR(50),
    create_time TIMESTAMP
);

-- Treasury Transfer - تحويل رصيد
CREATE TABLE erp_treasury_transfers (
    transfer_id BIGINT PRIMARY KEY,
    transfer_number VARCHAR(50),
    transfer_date DATE,
    from_treasury_id BIGINT,
    to_treasury_id BIGINT,
    amount DECIMAL(18,2),
    description TEXT,
    status VARCHAR(20),
    approved_by VARCHAR(50),
    approval_date TIMESTAMP,
    create_by VARCHAR(50),
    create_time TIMESTAMP
);
```

---

## 6. Shifts & Daily Operations - الورديات واليوميات

### 6.1 Models

```
ZR.Model/ERP/Operations/
├── ErpShift.cs                   # الوردية
├── ErpShiftTransaction.cs        # معاملات الوردية
├── ErpDailyReport.cs             # اليومية
├── ErpShiftClose.cs              # إغلاق الوردية
└── Dto/
```

### 6.2 Database Schema

```sql
-- Shift - الوردية
CREATE TABLE erp_shifts (
    shift_id BIGINT PRIMARY KEY,
    shift_number VARCHAR(50),
    branch_id BIGINT,
    treasury_id BIGINT,
    employee_id BIGINT,
    start_time TIMESTAMP,
    end_time TIMESTAMP,
    opening_balance DECIMAL(18,2),
    closing_balance DECIMAL(18,2),
    expected_balance DECIMAL(18,2),
    difference DECIMAL(18,2),
    total_sales DECIMAL(18,2) DEFAULT 0,
    total_returns DECIMAL(18,2) DEFAULT 0,
    total_receipts DECIMAL(18,2) DEFAULT 0,
    total_payments DECIMAL(18,2) DEFAULT 0,
    status VARCHAR(20), -- open/closed
    notes TEXT,
    closed_by VARCHAR(50),
    create_time TIMESTAMP
);

-- Daily Report - اليومية
CREATE TABLE erp_daily_reports (
    report_id BIGINT PRIMARY KEY,
    report_date DATE UNIQUE,
    branch_id BIGINT,
    total_sales DECIMAL(18,2),
    total_returns DECIMAL(18,2),
    total_purchases DECIMAL(18,2),
    total_expenses DECIMAL(18,2),
    total_receipts DECIMAL(18,2),
    total_payments DECIMAL(18,2),
    net_cash_flow DECIMAL(18,2),
    total_new_subscriptions INT,
    total_visits INT,
    top_products JSONB,
    top_services JSONB,
    is_auto_generated BOOLEAN DEFAULT TRUE,
    create_time TIMESTAMP
);
```

---

## 7. HR & Employees Module - وحدة الموظفين

### 7.1 Models

```
ZR.Model/ERP/HR/
├── ErpEmployee.cs                # الموظفين
├── ErpDepartment.cs              # الأقسام
├── ErpJobPosition.cs             # الوظائف
├── ErpBranchEmployee.cs          # موظفى الفروع
├── ErpEmployeeTransfer.cs        # سجل نقل الموظف
├── ErpFingerprint.cs             # بصمة الإصبع
├── ErpAttendanceMethod.cs        # طرق الحضور
├── ErpAttendanceLog.cs           # سجل الحضور
├── ErpAttendanceSettings.cs      # إعدادات الحضور
├── ErpIncentive.cs               # الحوافز
├── ErpCommission.cs              # العمولات
└── Dto/
```

### 7.2 Database Schema

```sql
-- Employee - الموظف
CREATE TABLE erp_employees (
    employee_id BIGINT PRIMARY KEY,
    employee_code VARCHAR(50) UNIQUE,
    full_name VARCHAR(200),
    full_name_ar VARCHAR(200),
    national_id VARCHAR(20),
    department_id BIGINT,
    job_position_id BIGINT,
    branch_id BIGINT,
    hire_date DATE,
    phone VARCHAR(20),
    email VARCHAR(100),
    basic_salary DECIMAL(18,2),
    commission_percent DECIMAL(5,2) DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE
);

-- Attendance Log - سجل الحضور
CREATE TABLE erp_attendance_logs (
    log_id BIGINT PRIMARY KEY,
    employee_id BIGINT,
    attendance_date DATE,
    check_in_time TIMESTAMP,
    check_out_time TIMESTAMP,
    attendance_method VARCHAR(50), -- fingerprint/manual/mobile
    work_hours DECIMAL(5,2),
    overtime_hours DECIMAL(5,2) DEFAULT 0,
    status VARCHAR(20), -- present/absent/late/leave
    notes TEXT
);

-- Commission - العمولة
CREATE TABLE erp_commissions (
    commission_id BIGINT PRIMARY KEY,
    employee_id BIGINT,
    period_start DATE,
    period_end DATE,
    total_sales DECIMAL(18,2),
    commission_percent DECIMAL(5,2),
    commission_amount DECIMAL(18,2),
    bonus_amount DECIMAL(18,2) DEFAULT 0,
    total_amount DECIMAL(18,2),
    status VARCHAR(20), -- calculated/approved/paid
    paid_date DATE
);
```

---

## 8. Suppliers Module - وحدة الموردين

### 8.1 Models

```
ZR.Model/ERP/Suppliers/
├── ErpSupplier.cs                # المورد
├── ErpSupplierBalance.cs         # رصيد المورد
├── ErpSupplierPayment.cs         # مدفوعات المورد
└── Dto/
```

### 8.2 Database Schema

```sql
-- Supplier - المورد
CREATE TABLE erp_suppliers (
    supplier_id BIGINT PRIMARY KEY,
    supplier_code VARCHAR(50) UNIQUE,
    supplier_name VARCHAR(200),
    supplier_name_ar VARCHAR(200),
    contact_person VARCHAR(100),
    phone VARCHAR(20),
    email VARCHAR(100),
    vat_number VARCHAR(50),
    credit_limit DECIMAL(18,2) DEFAULT 0,
    current_balance DECIMAL(18,2) DEFAULT 0,
    payment_terms INT DEFAULT 30, -- days
    address TEXT,
    is_active BOOLEAN DEFAULT TRUE
);
```

---

## 9. Branches & Settings - الفروع والإعدادات

### 9.1 Models

```
ZR.Model/ERP/Settings/
├── ErpCompanySettings.cs         # إعدادات الشركة
├── ErpBranch.cs                  # الفروع
├── ErpBranchSettings.cs          # إعدادات الفرع
├── ErpAutoJournalSettings.cs     # تشغيل اليوميات الآلي
├── ErpAutoShiftSettings.cs       # تشغيل الورديات الآلي
├── ErpServiceGroup.cs            # مجموعات الخدمات
├── ErpVatSettings.cs             # إعدادات الضريبة
├── ErpZakatSettings.cs           # إعدادات هيئة الزكاة
└── Dto/
```

---

## 10. Reports Module - وحدة التقارير

### 10.1 Report Types

```
├── Sales Reports - تقارير المبيعات
│   ├── عمليات البيع
│   ├── أرصدة العملاء
│   ├── مبيعات العملاء
│   └── مبيعات الباقات
│
├── Inventory Reports - تقارير المخزن
│   ├── أرصدة المخزن
│   ├── منتجات حد الطلب
│   └── حركات المخزن
│
├── Financial Reports - التقارير المالية
│   ├── كشف حساب
│   ├── ميزان المراجعة
│   ├── قائمة الدخل
│   └── الميزانية العمومية
│
├── HR Reports - تقارير الموظفين
│   ├── سجل الحضور
│   ├── أصناف الموظف الحالي
│   └── العمولات والحوافز
│
├── Treasury Reports - تقارير الخزائن
│   ├── أرصدة وحركات الخزائن
│   └── تحويلات الرصيد
│
└── Operational Reports - التقارير التشغيلية
    ├── الورديات
    ├── اليوميات
    └── أفضل الأصناف
```

---

## 11. AI Agent Integration Layer - طبقة تكامل الوكيل الذكي

### 11.1 Agent Architecture

```
ZR.Model/ERP/AI/
├── ErpAiAgent.cs                 # وكيل الذكاء الاصطناعي
├── ErpAiAgentTask.cs             # مهام الوكيل
├── ErpAiAgentDecision.cs         # قرارات الوكيل
├── ErpAiAgentLearning.cs         # تعلم الوكيل
├── ErpAiThinkingLog.cs           # سجل التفكير
├── ErpAiPlanningContext.cs       # سياق التخطيط
└── Dto/
```

### 11.2 AI Agent Database Schema

```sql
-- AI Agent - وكيل الذكاء الاصطناعي
CREATE TABLE erp_ai_agents (
    agent_id BIGINT PRIMARY KEY,
    agent_name VARCHAR(200),
    agent_type VARCHAR(50), -- thinking/planning/decision/learning/execution
    agent_mode_id BIGINT, -- links to ai_agent_mode
    description TEXT,
    capabilities JSONB, -- القدرات
    permissions JSONB, -- الصلاحيات
    is_active BOOLEAN DEFAULT TRUE,
    config JSONB,
    create_time TIMESTAMP
);

-- AI Agent Task - مهمة الوكيل
CREATE TABLE erp_ai_agent_tasks (
    task_id BIGINT PRIMARY KEY,
    agent_id BIGINT,
    task_type VARCHAR(100),
    task_context JSONB, -- سياق المهمة
    input_data JSONB,
    output_data JSONB,
    status VARCHAR(20), -- pending/processing/completed/failed
    started_at TIMESTAMP,
    completed_at TIMESTAMP,
    error_message TEXT,
    processing_time_ms INT
);

-- AI Thinking Log - سجل التفكير
CREATE TABLE erp_ai_thinking_logs (
    log_id BIGINT PRIMARY KEY,
    agent_id BIGINT,
    task_id BIGINT,
    thinking_step INT,
    thought_type VARCHAR(50), -- analysis/reasoning/conclusion
    thought_content TEXT,
    confidence_score DECIMAL(5,2),
    data_sources JSONB, -- مصادر البيانات المستخدمة
    create_time TIMESTAMP
);

-- AI Decision - قرار الوكيل
CREATE TABLE erp_ai_decisions (
    decision_id BIGINT PRIMARY KEY,
    agent_id BIGINT,
    task_id BIGINT,
    decision_type VARCHAR(100),
    decision_context JSONB,
    options_considered JSONB, -- الخيارات المدروسة
    selected_option TEXT,
    reasoning TEXT, -- التبرير
    confidence_score DECIMAL(5,2),
    auto_executed BOOLEAN DEFAULT FALSE,
    user_approved BOOLEAN,
    approved_by VARCHAR(50),
    approval_time TIMESTAMP,
    executed_at TIMESTAMP,
    result JSONB,
    create_time TIMESTAMP
);

-- AI Planning Context - سياق التخطيط
CREATE TABLE erp_ai_planning_contexts (
    context_id BIGINT PRIMARY KEY,
    agent_id BIGINT,
    context_type VARCHAR(50), -- sales/inventory/financial/hr
    context_data JSONB,
    goals JSONB, -- الأهداف
    constraints JSONB, -- القيود
    plan_steps JSONB, -- خطوات الخطة
    status VARCHAR(20),
    create_time TIMESTAMP,
    update_time TIMESTAMP
);

-- AI Learning Data - بيانات التعلم
CREATE TABLE erp_ai_learning_data (
    learning_id BIGINT PRIMARY KEY,
    agent_id BIGINT,
    learning_type VARCHAR(50), -- pattern/preference/optimization
    source_data JSONB,
    learned_pattern JSONB,
    accuracy_score DECIMAL(5,2),
    usage_count INT DEFAULT 0,
    last_used TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    create_time TIMESTAMP
);
```

### 11.3 AI Agent Capabilities Matrix

| Agent Type | Arabic | Capabilities |
|------------|--------|--------------|
| **ThinkingAgent** | وكيل التفكير | Analyze data, identify patterns, reason about situations |
| **PlanningAgent** | وكيل التخطيط | Create plans, schedule tasks, resource allocation |
| **DecisionAgent** | وكيل القرارات | Make recommendations, evaluate options, approve/reject |
| **LearningAgent** | وكيل التعلم | Learn from data, improve predictions, adapt behavior |
| **ExecutionAgent** | وكيل التنفيذ | Execute tasks, automate processes, integrate systems |

### 11.4 AI Agent Use Cases

#### 11.4.1 Sales Intelligence
```json
{
  "agent_type": "ThinkingAgent",
  "capabilities": [
    "Analyze customer purchasing patterns",
    "Predict customer churn",
    "Recommend upselling opportunities",
    "Identify best-selling products"
  ],
  "integration_points": [
    "ErpSalesOrder",
    "ErpCustomer",
    "ErpSalesInvoice"
  ]
}
```

#### 11.4.2 Inventory Optimization
```json
{
  "agent_type": "PlanningAgent",
  "capabilities": [
    "Predict stock requirements",
    "Optimize reorder points",
    "Plan purchase orders",
    "Prevent stockouts"
  ],
  "integration_points": [
    "ErpStockBalance",
    "ErpPurchaseOrder",
    "ErpInventoryTransaction"
  ]
}
```

#### 11.4.3 Financial Analysis
```json
{
  "agent_type": "DecisionAgent",
  "capabilities": [
    "Approve expense requests",
    "Recommend credit limits",
    "Detect anomalies in transactions",
    "Suggest cost reductions"
  ],
  "integration_points": [
    "ErpExpense",
    "ErpJournalEntry",
    "ErpCustomerBalance"
  ]
}
```

#### 11.4.4 HR & Scheduling
```json
{
  "agent_type": "ExecutionAgent",
  "capabilities": [
    "Auto-generate shift schedules",
    "Calculate commissions",
    "Process attendance",
    "Send notifications"
  ],
  "integration_points": [
    "ErpShift",
    "ErpAttendanceLog",
    "ErpCommission"
  ]
}
```

---

## 12. ZATCA Integration - تكامل هيئة الزكاة

### 12.1 Models

```
ZR.Model/ERP/ZATCA/
├── ErpZatcaInvoice.cs            # فاتورة الزكاة
├── ErpZatcaSubmission.cs         # إرسال الفواتير
├── ErpZatcaResponse.cs           # رد الهيئة
├── ErpZatcaSettings.cs           # إعدادات الربط
└── Dto/
```

### 12.2 Database Schema

```sql
-- ZATCA Submission - إرسال الفواتير
CREATE TABLE erp_zatca_submissions (
    submission_id BIGINT PRIMARY KEY,
    invoice_id BIGINT,
    invoice_number VARCHAR(50),
    invoice_type VARCHAR(20), -- simplified/standard
    submission_date TIMESTAMP,
    xml_content TEXT,
    qr_code TEXT,
    signed_invoice TEXT,
    zatca_status VARCHAR(50), -- pending/submitted/accepted/rejected
    zatca_response JSONB,
    error_messages JSONB,
    retry_count INT DEFAULT 0,
    last_retry TIMESTAMP,
    create_time TIMESTAMP
);
```

---

## 13. Service Layer Structure - هيكل طبقة الخدمات

```
ZR.Service/ERP/
├── Sales/
│   ├── ISalesService/
│   │   ├── ISalesOrderService.cs
│   │   ├── ISalesInvoiceService.cs
│   │   └── ICustomerService.cs
│   ├── SalesOrderService.cs
│   ├── SalesInvoiceService.cs
│   └── CustomerService.cs
│
├── Subscriptions/
│   ├── ISubscriptionService/
│   │   ├── ISubscriptionService.cs
│   │   ├── IMemberService.cs
│   │   └── IResourceService.cs
│   ├── SubscriptionService.cs
│   ├── MemberService.cs
│   └── ResourceService.cs
│
├── Inventory/
│   ├── IInventoryService/
│   │   ├── IInventoryTransactionService.cs
│   │   ├── IProductService.cs
│   │   └── IWarehouseService.cs
│   ├── InventoryTransactionService.cs
│   ├── ProductService.cs
│   └── WarehouseService.cs
│
├── Accounts/
│   ├── IAccountService/
│   │   ├── IAccountService.cs
│   │   ├── IJournalEntryService.cs
│   │   └── ICostCenterService.cs
│   ├── AccountService.cs
│   ├── JournalEntryService.cs
│   └── CostCenterService.cs
│
├── Treasury/
│   ├── ITreasuryService/
│   └── TreasuryService.cs
│
├── HR/
│   ├── IHRService/
│   └── EmployeeService.cs
│
├── AI/
│   ├── IAIService/
│   │   ├── IThinkingAgentService.cs
│   │   ├── IPlanningAgentService.cs
│   │   ├── IDecisionAgentService.cs
│   │   ├── ILearningAgentService.cs
│   │   └── IExecutionAgentService.cs
│   ├── ThinkingAgentService.cs
│   ├── PlanningAgentService.cs
│   ├── DecisionAgentService.cs
│   ├── LearningAgentService.cs
│   └── ExecutionAgentService.cs
│
└── ZATCA/
    ├── IZATCAService/
    └── ZATCASubmissionService.cs
```

---

## 14. Controller Structure - هيكل المتحكمات

```
ZR.Admin.WebApi/Controllers/ERP/
├── Sales/
│   ├── SalesOrderController.cs
│   ├── SalesInvoiceController.cs
│   ├── CustomerController.cs
│   └── CustomerBalanceController.cs
│
├── Subscriptions/
│   ├── SubscriptionController.cs
│   ├── MemberController.cs
│   ├── ResourceController.cs
│   └── BookingController.cs
│
├── Inventory/
│   ├── InventoryTransactionController.cs
│   ├── ProductController.cs
│   ├── WarehouseController.cs
│   └── StockBalanceController.cs
│
├── Accounts/
│   ├── AccountController.cs
│   ├── JournalEntryController.cs
│   ├── CostCenterController.cs
│   ├── ReceiptController.cs
│   ├── PaymentVoucherController.cs
│   └── ExpenseController.cs
│
├── Treasury/
│   ├── TreasuryController.cs
│   ├── TreasuryTransactionController.cs
│   └── TreasuryTransferController.cs
│
├── HR/
│   ├── EmployeeController.cs
│   ├── AttendanceController.cs
│   └── CommissionController.cs
│
├── Operations/
│   ├── ShiftController.cs
│   └── DailyReportController.cs
│
├── Reports/
│   ├── SalesReportController.cs
│   ├── InventoryReportController.cs
│   ├── FinancialReportController.cs
│   └── HRReportController.cs
│
├── AI/
│   ├── AIAgentController.cs
│   ├── AIThinkingController.cs
│   ├── AIPlanningController.cs
│   └── AIDecisionController.cs
│
└── ZATCA/
    └── ZATCAController.cs
```

---

## 15. API Endpoints Summary

### 15.1 Sales APIs
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/erp/sales/orders | قائمة أوامر البيع |
| POST | /api/erp/sales/orders | إنشاء أمر بيع |
| GET | /api/erp/sales/invoices | قائمة الفواتير |
| POST | /api/erp/sales/invoices | إنشاء فاتورة |
| GET | /api/erp/customers | قائمة العملاء |
| GET | /api/erp/customers/{id}/balance | رصيد العميل |

### 15.2 Inventory APIs
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/erp/inventory/products | قائمة المنتجات |
| GET | /api/erp/inventory/stock | أرصدة المخزن |
| POST | /api/erp/inventory/transactions | إضافة حركة مخزن |
| POST | /api/erp/inventory/transfer | تحويل لمخزن |
| POST | /api/erp/inventory/count | جرد مخزن |

### 15.3 Accounts APIs
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/erp/accounts | دليل الحسابات |
| GET | /api/erp/accounts/statement/{id} | كشف حساب |
| POST | /api/erp/journal-entries | قيد يومي |
| POST | /api/erp/receipts | سند قبض |
| POST | /api/erp/payments | سند صرف |

### 15.4 AI Agent APIs
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/erp/ai/think | طلب تحليل ذكي |
| POST | /api/erp/ai/plan | طلب خطة |
| POST | /api/erp/ai/decide | طلب قرار |
| GET | /api/erp/ai/decisions/{id} | عرض قرار |
| POST | /api/erp/ai/decisions/{id}/approve | اعتماد قرار |

---

## 16. Integration Patterns

### 16.1 Automatic Journal Entry Generation

When a sales invoice is created:
```
SalesInvoice → AI Agent → JournalEntry
    ↓
    ├── Debit: Customer Account (أرصدة العملاء)
    └── Credit: Sales Revenue Account (إيرادات المبيعات)
```

### 16.2 Inventory Transaction Flow

```
InventoryTransaction → Stock Balance Update → AI Analysis
    ↓
    ├── Update warehouse quantity
    ├── Calculate new average cost
    ├── Check reorder level
    └── AI: Suggest reorder if needed
```

### 16.3 Treasury Integration

```
Receipt/Payment → Treasury Transaction → Balance Update → Journal Entry
    ↓
    └── AI: Detect anomalies, predict cash flow
```

---

## 17. Security & Permissions

Each module implements:
- Role-based access control (RBAC)
- Branch-level data filtering
- Operation-level permissions
- Audit logging

AI Agents require special permissions:
- `ai:think` - Ability to analyze data
- `ai:plan` - Ability to create plans
- `ai:decide` - Ability to make recommendations
- `ai:execute` - Ability to auto-execute approved decisions

---

## 18. Next Steps

1. Create model files for each module
2. Implement services with AI integration points
3. Create controllers with REST APIs
4. Implement AI agent services
5. Add ZATCA integration
6. Create Vue.js frontend components
7. Test and deploy

---

**Document Version:** 1.0
**Created:** 2025-01-22
**Author:** AI Design Assistant
