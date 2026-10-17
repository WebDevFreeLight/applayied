-- =============================================
-- Payment Hub Database Schema
-- THE CANONICAL single source of truth for all payments
-- PostgreSQL DDL
-- =============================================

-- Drop existing tables if any (clean slate)
DROP TABLE IF EXISTS hub_payment_event CASCADE;
DROP TABLE IF EXISTS hub_payment_allocation CASCADE;
DROP TABLE IF EXISTS hub_payment CASCADE;

-- =============================================
-- 1. Payment Table (hub_payment)
-- =============================================
CREATE TABLE hub_payment (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Identification
    payment_number VARCHAR(50) NOT NULL,
    idempotency_key VARCHAR(100),

    -- Classification
    payment_type INTEGER NOT NULL,  -- 0:CustomerReceipt, 1:VendorPayment, 2:InternalTransfer, etc.
    payment_method INTEGER NOT NULL, -- 0:Cash, 1:BankTransfer, 2:Cheque, etc.
    status INTEGER NOT NULL DEFAULT 0, -- 0:Draft, 1:Approved, 2:Posted, 3:Reversed, 4:Cancelled

    -- Dates
    payment_date DATE NOT NULL,
    value_date DATE,
    posted_at TIMESTAMP,
    approved_at TIMESTAMP,
    reversed_at TIMESTAMP,

    -- Amounts & Currency
    amount DECIMAL(18,4) NOT NULL CHECK (amount > 0),
    currency VARCHAR(3) NOT NULL,
    exchange_rate DECIMAL(18,6) NOT NULL DEFAULT 1,
    base_currency_amount DECIMAL(18,4) NOT NULL,
    allocated_amount DECIMAL(18,4) NOT NULL DEFAULT 0,
    unallocated_amount DECIMAL(18,4) NOT NULL,
    fx_gain_loss DECIMAL(18,4) NOT NULL DEFAULT 0,

    -- Account References
    source_account_id UUID,
    source_account_type INTEGER, -- 0:BankAccount, 1:CashAccount
    destination_account_id UUID,
    destination_account_type INTEGER,

    -- Counterparty
    counterparty_type INTEGER, -- 0:Customer, 1:Vendor, 2:Employee, 99:Other
    counterparty_id UUID,
    counterparty_name VARCHAR(200),

    -- GL Integration
    gl_journal_id UUID,
    reversal_gl_journal_id UUID,
    balance_ledger_entry_id UUID,

    -- Fiscal Period
    fiscal_year INTEGER NOT NULL,
    fiscal_period INTEGER NOT NULL,

    -- Segmentation
    cost_center_id UUID,
    department_id UUID,
    project_id UUID,

    -- Reversal Tracking
    is_reversed BOOLEAN NOT NULL DEFAULT FALSE,
    reversal_of_id UUID REFERENCES hub_payment(id),
    reversed_by_id UUID REFERENCES hub_payment(id),
    reversal_reason VARCHAR(500),

    -- Approval
    approved_by VARCHAR(100),
    posted_by VARCHAR(100),
    approval_comments VARCHAR(500),

    -- References & Notes
    reference_number VARCHAR(100),
    bank_reference VARCHAR(100),
    cheque_number VARCHAR(50),
    cheque_id UUID,
    description VARCHAR(500),
    notes VARCHAR(1000),

    -- Concurrency
    row_version INTEGER NOT NULL DEFAULT 1,

    -- Multi-tenant
    company_id BIGINT,

    -- Audit columns
    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    create_by VARCHAR(50),
    update_time TIMESTAMP,
    update_by VARCHAR(50),

    -- Constraints
    CONSTRAINT uk_hub_payment_number UNIQUE (payment_number),
    CONSTRAINT uk_hub_payment_idempotency UNIQUE (idempotency_key) WHERE idempotency_key IS NOT NULL
);

-- Indexes for Payment
CREATE INDEX idx_hub_payment_number ON hub_payment(payment_number);
CREATE INDEX idx_hub_payment_status ON hub_payment(status);
CREATE INDEX idx_hub_payment_date ON hub_payment(payment_date);
CREATE INDEX idx_hub_payment_type ON hub_payment(payment_type);
CREATE INDEX idx_hub_payment_counterparty ON hub_payment(counterparty_id) WHERE counterparty_id IS NOT NULL;
CREATE INDEX idx_hub_payment_source_account ON hub_payment(source_account_id) WHERE source_account_id IS NOT NULL;
CREATE INDEX idx_hub_payment_dest_account ON hub_payment(destination_account_id) WHERE destination_account_id IS NOT NULL;
CREATE INDEX idx_hub_payment_fiscal ON hub_payment(fiscal_year, fiscal_period);
CREATE INDEX idx_hub_payment_unallocated ON hub_payment(unallocated_amount) WHERE unallocated_amount > 0.0001;
CREATE INDEX idx_hub_payment_company ON hub_payment(company_id) WHERE company_id IS NOT NULL;
CREATE INDEX idx_hub_payment_idempotency ON hub_payment(idempotency_key) WHERE idempotency_key IS NOT NULL;

-- =============================================
-- 2. Payment Allocation Table (hub_payment_allocation)
-- =============================================
CREATE TABLE hub_payment_allocation (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Payment Reference
    payment_id UUID NOT NULL REFERENCES hub_payment(id),
    payment_number VARCHAR(50),

    -- Document Reference
    document_type INTEGER NOT NULL, -- 0:ARInvoice, 1:APInvoice, 2:CreditNote, 3:DebitNote
    document_id UUID NOT NULL,
    document_number VARCHAR(50),

    -- Amounts in Payment Currency
    allocated_amount DECIMAL(18,4) NOT NULL CHECK (allocated_amount > 0),
    payment_currency VARCHAR(3) NOT NULL,

    -- Amounts in Document Currency
    allocated_amount_doc_currency DECIMAL(18,4) NOT NULL,
    document_currency VARCHAR(3) NOT NULL,
    exchange_rate DECIMAL(18,6) NOT NULL DEFAULT 1,

    -- FX Gain/Loss
    fx_gain_loss DECIMAL(18,4) NOT NULL DEFAULT 0,
    fx_gl_journal_id UUID,

    -- Document State Snapshot
    document_balance_before DECIMAL(18,4) NOT NULL,
    document_balance_after DECIMAL(18,4) NOT NULL,

    -- Status & Reversal
    status INTEGER NOT NULL DEFAULT 0, -- 0:Active, 1:Reversed
    is_reversed BOOLEAN NOT NULL DEFAULT FALSE,
    reversal_date TIMESTAMP,
    reversal_reason VARCHAR(500),
    reversed_by VARCHAR(100),

    -- Dates
    allocation_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Notes
    notes VARCHAR(500),

    -- Concurrency
    row_version INTEGER NOT NULL DEFAULT 1,

    -- Multi-tenant
    company_id BIGINT,

    -- Audit columns
    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    create_by VARCHAR(50),
    update_time TIMESTAMP,
    update_by VARCHAR(50)
);

-- Indexes for Allocation
CREATE INDEX idx_hub_allocation_payment ON hub_payment_allocation(payment_id);
CREATE INDEX idx_hub_allocation_document ON hub_payment_allocation(document_id, document_type);
CREATE INDEX idx_hub_allocation_active ON hub_payment_allocation(payment_id) WHERE NOT is_reversed;
CREATE INDEX idx_hub_allocation_date ON hub_payment_allocation(allocation_date);
CREATE INDEX idx_hub_allocation_company ON hub_payment_allocation(company_id) WHERE company_id IS NOT NULL;

-- =============================================
-- 3. Payment Event Table (hub_payment_event)
-- APPEND-ONLY AUDIT TRAIL - NEVER UPDATE/DELETE
-- =============================================
CREATE TABLE hub_payment_event (
    -- Primary Key
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Payment Reference
    payment_id UUID NOT NULL REFERENCES hub_payment(id),
    payment_number VARCHAR(50) NOT NULL,

    -- Event Details
    event_type INTEGER NOT NULL, -- 0:Created, 1:Updated, 2:Approved, 3:Posted, etc.
    event_timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sequence_number INTEGER NOT NULL,

    -- State Snapshot
    previous_status INTEGER,
    new_status INTEGER,
    amount DECIMAL(18,4),
    currency VARCHAR(3),

    -- Related Entities
    allocation_id UUID,
    document_id UUID,
    gl_journal_id UUID,
    balance_ledger_entry_id UUID,

    -- User & Context
    user_id VARCHAR(100) NOT NULL,
    user_name VARCHAR(200),
    ip_address VARCHAR(50),
    user_agent VARCHAR(500),

    -- Tracing
    correlation_id VARCHAR(100),
    request_id VARCHAR(100),

    -- Details
    description VARCHAR(500),
    event_data TEXT, -- JSON for additional context
    error_message VARCHAR(1000),

    -- Multi-tenant
    company_id BIGINT
);

-- Indexes for Event
CREATE INDEX idx_hub_event_payment ON hub_payment_event(payment_id);
CREATE INDEX idx_hub_event_timestamp ON hub_payment_event(event_timestamp);
CREATE INDEX idx_hub_event_type ON hub_payment_event(event_type);
CREATE INDEX idx_hub_event_sequence ON hub_payment_event(payment_id, sequence_number);
CREATE INDEX idx_hub_event_user ON hub_payment_event(user_id);
CREATE INDEX idx_hub_event_company ON hub_payment_event(company_id) WHERE company_id IS NOT NULL;

-- =============================================
-- Comments
-- =============================================
COMMENT ON TABLE hub_payment IS 'Payment Hub - THE canonical single source of truth for all payments';
COMMENT ON TABLE hub_payment_allocation IS 'Payment allocations to invoices/documents - immutable once created';
COMMENT ON TABLE hub_payment_event IS 'Append-only audit trail for payment lifecycle events';

COMMENT ON COLUMN hub_payment.idempotency_key IS 'Client-provided key to prevent duplicate submissions';
COMMENT ON COLUMN hub_payment.payment_type IS '0:CustomerReceipt, 1:VendorPayment, 2:InternalTransfer, 3:IntercompanyTransfer, 4:CustomerRefund, 5:VendorRefund';
COMMENT ON COLUMN hub_payment.status IS '0:Draft, 1:Approved, 2:Posted, 3:Reversed, 4:Cancelled';
COMMENT ON COLUMN hub_payment.row_version IS 'Optimistic concurrency control';

COMMENT ON COLUMN hub_payment_allocation.document_type IS '0:ARInvoice, 1:APInvoice, 2:CreditNote, 3:DebitNote';
COMMENT ON COLUMN hub_payment_allocation.document_balance_before IS 'Snapshot of document balance before allocation';

COMMENT ON COLUMN hub_payment_event.event_type IS '0:Created, 1:Updated, 2:Approved, 3:Posted, 4:Allocated, 5:AllocationReversed, 6:Reversed, 7:Cancelled, 8:BalanceUpdated, 9:GLJournalCreated, 10:GLJournalReversed';
COMMENT ON COLUMN hub_payment_event.sequence_number IS 'Monotonically increasing within payment for ordering';

-- =============================================
-- Trigger to prevent updates/deletes on events
-- =============================================
CREATE OR REPLACE FUNCTION prevent_event_modification()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'hub_payment_event is append-only. UPDATE and DELETE operations are not allowed.';
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_hub_payment_event_immutable
BEFORE UPDATE OR DELETE ON hub_payment_event
FOR EACH ROW
EXECUTE FUNCTION prevent_event_modification();

-- =============================================
-- View for easy querying
-- =============================================
CREATE OR REPLACE VIEW vw_payment_summary AS
SELECT
    p.id,
    p.payment_number,
    p.payment_type,
    CASE p.payment_type
        WHEN 0 THEN 'Customer Receipt'
        WHEN 1 THEN 'Vendor Payment'
        WHEN 2 THEN 'Internal Transfer'
        WHEN 3 THEN 'Intercompany Transfer'
        WHEN 4 THEN 'Customer Refund'
        WHEN 5 THEN 'Vendor Refund'
    END as payment_type_name,
    p.status,
    CASE p.status
        WHEN 0 THEN 'Draft'
        WHEN 1 THEN 'Approved'
        WHEN 2 THEN 'Posted'
        WHEN 3 THEN 'Reversed'
        WHEN 4 THEN 'Cancelled'
    END as status_name,
    p.payment_date,
    p.amount,
    p.currency,
    p.allocated_amount,
    p.unallocated_amount,
    p.counterparty_name,
    p.is_reversed,
    p.create_time,
    p.company_id
FROM hub_payment p;

-- =============================================
-- End of PaymentHub DDL
-- =============================================
