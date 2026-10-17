-- =============================================
-- Treasury Module Database Schema
-- Cash & Bank (Treasury) Module DDL Scripts
-- =============================================

-- 1. Cash Account Table
CREATE TABLE IF NOT EXISTS cash_account (
    id UUID PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name_ar VARCHAR(200) NOT NULL,
    name_en VARCHAR(200) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    branch_id UUID,
    current_balance DECIMAL(18,4) NOT NULL DEFAULT 0,
    type INTEGER NOT NULL, -- 0: CashBox, 1: PettyCash
    status INTEGER NOT NULL DEFAULT 0, -- 0: Active, 1: Inactive, 2: Closed
    notes VARCHAR(1000),
    tenant_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP,
    updated_by UUID,
    CONSTRAINT uk_cash_account_code_tenant UNIQUE (code, tenant_id)
);

CREATE INDEX IF NOT EXISTS idx_cash_account_tenant ON cash_account(tenant_id);
CREATE INDEX IF NOT EXISTS idx_cash_account_code ON cash_account(code);
CREATE INDEX IF NOT EXISTS idx_cash_account_status ON cash_account(status);

-- 2. Bank Account Table
CREATE TABLE IF NOT EXISTS bank_account (
    id UUID PRIMARY KEY,
    bank_code VARCHAR(50) NOT NULL,
    bank_name_ar VARCHAR(200) NOT NULL,
    bank_name_en VARCHAR(200) NOT NULL,
    account_number VARCHAR(100) NOT NULL,
    iban VARCHAR(34),
    swift_code VARCHAR(11),
    currency VARCHAR(3) NOT NULL,
    branch VARCHAR(200),
    bank_contact VARCHAR(500),
    opening_balance DECIMAL(18,4) NOT NULL DEFAULT 0,
    current_balance DECIMAL(18,4) NOT NULL DEFAULT 0,
    reconciliation_date DATE,
    status INTEGER NOT NULL DEFAULT 0, -- 0: Active, 1: Inactive, 2: Closed
    notes VARCHAR(1000),
    tenant_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP,
    updated_by UUID,
    CONSTRAINT uk_bank_account_number_tenant UNIQUE (account_number, tenant_id)
);

CREATE INDEX IF NOT EXISTS idx_bank_account_tenant ON bank_account(tenant_id);
CREATE INDEX IF NOT EXISTS idx_bank_account_number ON bank_account(account_number);
CREATE INDEX IF NOT EXISTS idx_bank_account_status ON bank_account(status);

-- 3. Bank Statement Table
CREATE TABLE IF NOT EXISTS bank_statement (
    id UUID PRIMARY KEY,
    bank_account_id UUID NOT NULL,
    statement_date DATE NOT NULL,
    statement_reference VARCHAR(200) NOT NULL,
    opening_balance DECIMAL(18,4) NOT NULL,
    closing_balance DECIMAL(18,4) NOT NULL,
    imported BOOLEAN NOT NULL DEFAULT FALSE,
    import_source INTEGER NOT NULL DEFAULT 0, -- 0: Manual, 1: CSV, 2: MT940, 3: API
    notes VARCHAR(1000),
    tenant_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    CONSTRAINT fk_bank_statement_account FOREIGN KEY (bank_account_id) REFERENCES bank_account(id)
);

CREATE INDEX IF NOT EXISTS idx_bank_statement_account ON bank_statement(bank_account_id);
CREATE INDEX IF NOT EXISTS idx_bank_statement_date ON bank_statement(statement_date);
CREATE INDEX IF NOT EXISTS idx_bank_statement_tenant ON bank_statement(tenant_id);

-- 4. Bank Statement Line Table
CREATE TABLE IF NOT EXISTS bank_statement_line (
    id UUID PRIMARY KEY,
    bank_statement_id UUID NOT NULL,
    line_date DATE NOT NULL,
    value_date DATE NOT NULL,
    description VARCHAR(500) NOT NULL,
    amount DECIMAL(18,4) NOT NULL,
    running_balance DECIMAL(18,4) NOT NULL,
    reference_no VARCHAR(100),
    matched BOOLEAN NOT NULL DEFAULT FALSE,
    matched_transaction_id UUID,
    tenant_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    CONSTRAINT fk_statement_line_statement FOREIGN KEY (bank_statement_id) REFERENCES bank_statement(id),
    CONSTRAINT fk_statement_line_payment FOREIGN KEY (matched_transaction_id) REFERENCES payment(id)
);

CREATE INDEX IF NOT EXISTS idx_statement_line_statement ON bank_statement_line(bank_statement_id);
CREATE INDEX IF NOT EXISTS idx_statement_line_matched ON bank_statement_line(matched);
CREATE INDEX IF NOT EXISTS idx_statement_line_reference ON bank_statement_line(reference_no);
CREATE INDEX IF NOT EXISTS idx_statement_line_tenant ON bank_statement_line(tenant_id);

-- 5. Payment Table
CREATE TABLE IF NOT EXISTS payment (
    id UUID PRIMARY KEY,
    payment_number VARCHAR(50) NOT NULL,
    payment_type INTEGER NOT NULL, -- 0: Receipt, 1: Payment, 2: Transfer
    payment_date DATE NOT NULL,
    amount DECIMAL(18,4) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    from_account_id UUID,
    to_account_id UUID,
    counterparty_id UUID, -- Customer or Vendor ID
    method INTEGER NOT NULL, -- 0: Cash, 1: BankTransfer, 2: Cheque, 3: Card, 4: Other
    reference_no VARCHAR(100),
    status INTEGER NOT NULL DEFAULT 0, -- 0: Draft, 1: Submitted, 2: Approved, 3: Posted, 4: Cancelled
    allocated BOOLEAN NOT NULL DEFAULT FALSE,
    gl_voucher_id UUID,
    notes VARCHAR(1000),
    tenant_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP,
    updated_by UUID,
    CONSTRAINT uk_payment_number_tenant UNIQUE (payment_number, tenant_id)
);

CREATE INDEX IF NOT EXISTS idx_payment_tenant ON payment(tenant_id);
CREATE INDEX IF NOT EXISTS idx_payment_number ON payment(payment_number);
CREATE INDEX IF NOT EXISTS idx_payment_date ON payment(payment_date);
CREATE INDEX IF NOT EXISTS idx_payment_status ON payment(status);
CREATE INDEX IF NOT EXISTS idx_payment_from_account ON payment(from_account_id);
CREATE INDEX IF NOT EXISTS idx_payment_to_account ON payment(to_account_id);
CREATE INDEX IF NOT EXISTS idx_payment_counterparty ON payment(counterparty_id);
CREATE INDEX IF NOT EXISTS idx_payment_type ON payment(payment_type);

-- 6. Payment Allocation Table
CREATE TABLE IF NOT EXISTS payment_allocation (
    id UUID PRIMARY KEY,
    payment_id UUID NOT NULL,
    invoice_id UUID NOT NULL, -- AR or AP Invoice ID
    allocated_amount DECIMAL(18,4) NOT NULL,
    tenant_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    CONSTRAINT fk_allocation_payment FOREIGN KEY (payment_id) REFERENCES payment(id)
);

CREATE INDEX IF NOT EXISTS idx_allocation_payment ON payment_allocation(payment_id);
CREATE INDEX IF NOT EXISTS idx_allocation_invoice ON payment_allocation(invoice_id);
CREATE INDEX IF NOT EXISTS idx_allocation_tenant ON payment_allocation(tenant_id);

-- 7. Cheque Table
CREATE TABLE IF NOT EXISTS cheque (
    id UUID PRIMARY KEY,
    cheque_number VARCHAR(50) NOT NULL,
    bank_account_id UUID NOT NULL,
    payee_name VARCHAR(200) NOT NULL,
    issue_date DATE NOT NULL,
    due_date DATE NOT NULL,
    amount DECIMAL(18,4) NOT NULL,
    status INTEGER NOT NULL DEFAULT 0, -- 0: Issued, 1: Presented, 2: Cleared, 3: Bounced, 4: Cancelled
    related_payment_id UUID,
    notes VARCHAR(1000),
    tenant_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP,
    updated_by UUID,
    CONSTRAINT fk_cheque_bank_account FOREIGN KEY (bank_account_id) REFERENCES bank_account(id),
    CONSTRAINT fk_cheque_payment FOREIGN KEY (related_payment_id) REFERENCES payment(id),
    CONSTRAINT uk_cheque_number_tenant UNIQUE (cheque_number, tenant_id)
);

CREATE INDEX IF NOT EXISTS idx_cheque_bank_account ON cheque(bank_account_id);
CREATE INDEX IF NOT EXISTS idx_cheque_number ON cheque(cheque_number);
CREATE INDEX IF NOT EXISTS idx_cheque_status ON cheque(status);
CREATE INDEX IF NOT EXISTS idx_cheque_due_date ON cheque(due_date);
CREATE INDEX IF NOT EXISTS idx_cheque_tenant ON cheque(tenant_id);

-- 8. Bank Reconciliation Table
CREATE TABLE IF NOT EXISTS bank_reconciliation (
    id UUID PRIMARY KEY,
    bank_account_id UUID NOT NULL,
    period_start DATE NOT NULL,
    period_end DATE NOT NULL,
    statement_opening_balance DECIMAL(18,4) NOT NULL,
    statement_closing_balance DECIMAL(18,4) NOT NULL,
    ledger_opening_balance DECIMAL(18,4) NOT NULL,
    ledger_closing_balance DECIMAL(18,4) NOT NULL,
    discrepancies_summary VARCHAR(4000),
    status INTEGER NOT NULL DEFAULT 0, -- 0: InProgress, 1: Reconciled, 2: PendingInvestigation
    reconciled_at TIMESTAMP,
    reconciled_by UUID,
    notes VARCHAR(1000),
    tenant_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    CONSTRAINT fk_reconciliation_account FOREIGN KEY (bank_account_id) REFERENCES bank_account(id)
);

CREATE INDEX IF NOT EXISTS idx_reconciliation_account ON bank_reconciliation(bank_account_id);
CREATE INDEX IF NOT EXISTS idx_reconciliation_status ON bank_reconciliation(status);
CREATE INDEX IF NOT EXISTS idx_reconciliation_period ON bank_reconciliation(period_start, period_end);
CREATE INDEX IF NOT EXISTS idx_reconciliation_tenant ON bank_reconciliation(tenant_id);

-- 9. Bank Reconciliation Line Table
CREATE TABLE IF NOT EXISTS bank_reconciliation_line (
    id UUID PRIMARY KEY,
    reconciliation_id UUID NOT NULL,
    bank_statement_line_id UUID,
    matched_payment_id UUID,
    difference_amount DECIMAL(18,4) NOT NULL DEFAULT 0,
    notes VARCHAR(1000),
    tenant_id UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    CONSTRAINT fk_reconciliation_line_reconciliation FOREIGN KEY (reconciliation_id) REFERENCES bank_reconciliation(id),
    CONSTRAINT fk_reconciliation_line_statement FOREIGN KEY (bank_statement_line_id) REFERENCES bank_statement_line(id),
    CONSTRAINT fk_reconciliation_line_payment FOREIGN KEY (matched_payment_id) REFERENCES payment(id)
);

CREATE INDEX IF NOT EXISTS idx_reconciliation_line_reconciliation ON bank_reconciliation_line(reconciliation_id);
CREATE INDEX IF NOT EXISTS idx_reconciliation_line_tenant ON bank_reconciliation_line(tenant_id);

-- 10. Treasury Config Table
CREATE TABLE IF NOT EXISTS treasury_config (
    id UUID PRIMARY KEY,
    tenant_id UUID,
    auto_import_bank_statements BOOLEAN NOT NULL DEFAULT FALSE,
    bank_statement_import_format INTEGER NOT NULL DEFAULT 1, -- 0: Manual, 1: CSV, 2: MT940, 3: API
    auto_reconcile BOOLEAN NOT NULL DEFAULT FALSE,
    default_payment_approval_threshold DECIMAL(18,4) NOT NULL DEFAULT 0,
    petty_cash_limit DECIMAL(18,4) NOT NULL DEFAULT 0,
    approval_workflow_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by UUID,
    updated_at TIMESTAMP,
    updated_by UUID,
    CONSTRAINT uk_treasury_config_tenant UNIQUE (tenant_id)
);

CREATE INDEX IF NOT EXISTS idx_treasury_config_tenant ON treasury_config(tenant_id);

-- =============================================
-- Comments
-- =============================================

COMMENT ON TABLE cash_account IS 'Cash accounts (cashboxes and petty cash)';
COMMENT ON TABLE bank_account IS 'Bank accounts';
COMMENT ON TABLE bank_statement IS 'Bank statements';
COMMENT ON TABLE bank_statement_line IS 'Bank statement line items';
COMMENT ON TABLE payment IS 'Payments, receipts, and transfers';
COMMENT ON TABLE payment_allocation IS 'Payment allocations to invoices';
COMMENT ON TABLE cheque IS 'Cheque management';
COMMENT ON TABLE bank_reconciliation IS 'Bank reconciliation records';
COMMENT ON TABLE bank_reconciliation_line IS 'Bank reconciliation line items';
COMMENT ON TABLE treasury_config IS 'Treasury module configuration';





























