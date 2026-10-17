-- Treasury Module Database Indexes (Requirement #54)
-- Optimized indexes for improved query performance on Treasury tables
-- Execute this script after the tables are created

-- =============================================================================
-- Bank Account Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_bank_account_status
ON bank_account(status)
WHERE is_deleted = false;

CREATE INDEX IF NOT EXISTS idx_bank_account_company
ON bank_account(company_id)
WHERE is_deleted = false;

CREATE INDEX IF NOT EXISTS idx_bank_account_currency
ON bank_account(currency);

CREATE INDEX IF NOT EXISTS idx_bank_account_gl_account
ON bank_account(gl_account_id)
WHERE gl_account_id IS NOT NULL;

-- =============================================================================
-- Cash Account Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_cash_account_status
ON cash_account(status)
WHERE is_deleted = false;

CREATE INDEX IF NOT EXISTS idx_cash_account_company
ON cash_account(company_id)
WHERE is_deleted = false;

CREATE INDEX IF NOT EXISTS idx_cash_account_type
ON cash_account(type);

-- =============================================================================
-- Payment Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_payment_number
ON payment(payment_number);

CREATE INDEX IF NOT EXISTS idx_payment_status
ON payment(status);

CREATE INDEX IF NOT EXISTS idx_payment_type
ON payment(payment_type);

CREATE INDEX IF NOT EXISTS idx_payment_date
ON payment(payment_date);

CREATE INDEX IF NOT EXISTS idx_payment_date_status
ON payment(payment_date, status);

CREATE INDEX IF NOT EXISTS idx_payment_from_account
ON payment(from_account_id)
WHERE from_account_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_payment_to_account
ON payment(to_account_id)
WHERE to_account_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_payment_counterparty
ON payment(counterparty_id)
WHERE counterparty_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_payment_idempotency
ON payment(idempotency_key)
WHERE idempotency_key IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_payment_gl_journal
ON payment(gl_journal_id)
WHERE gl_journal_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_payment_company
ON payment(company_id)
WHERE company_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_payment_fiscal_period
ON payment(fiscal_year, fiscal_period);

-- =============================================================================
-- Cheque Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_cheque_number
ON cheque(cheque_number);

CREATE INDEX IF NOT EXISTS idx_cheque_status
ON cheque(status);

CREATE INDEX IF NOT EXISTS idx_cheque_type
ON cheque(cheque_type);

CREATE INDEX IF NOT EXISTS idx_cheque_issue_date
ON cheque(issue_date);

CREATE INDEX IF NOT EXISTS idx_cheque_due_date
ON cheque(due_date);

CREATE INDEX IF NOT EXISTS idx_cheque_bank_account
ON cheque(bank_account_id);

CREATE INDEX IF NOT EXISTS idx_cheque_status_due_date
ON cheque(status, due_date);

CREATE INDEX IF NOT EXISTS idx_cheque_impacts_available
ON cheque(bank_account_id, impacts_available_balance)
WHERE impacts_available_balance = true;

CREATE INDEX IF NOT EXISTS idx_cheque_idempotency
ON cheque(idempotency_key)
WHERE idempotency_key IS NOT NULL;

-- =============================================================================
-- Payment Allocation Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_payment_allocation_payment
ON payment_allocation(payment_id);

CREATE INDEX IF NOT EXISTS idx_payment_allocation_invoice
ON payment_allocation(invoice_id);

CREATE INDEX IF NOT EXISTS idx_payment_allocation_date
ON payment_allocation(allocation_date);

CREATE INDEX IF NOT EXISTS idx_payment_allocation_not_reversed
ON payment_allocation(payment_id)
WHERE is_reversed = false;

-- =============================================================================
-- Bank Reconciliation Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_bank_reconciliation_number
ON bank_reconciliation(reconciliation_number);

CREATE INDEX IF NOT EXISTS idx_bank_reconciliation_bank_account
ON bank_reconciliation(bank_account_id);

CREATE INDEX IF NOT EXISTS idx_bank_reconciliation_status
ON bank_reconciliation(status);

CREATE INDEX IF NOT EXISTS idx_bank_reconciliation_period
ON bank_reconciliation(period_start, period_end);

CREATE INDEX IF NOT EXISTS idx_bank_reconciliation_fiscal
ON bank_reconciliation(fiscal_year, fiscal_period);

-- =============================================================================
-- Bank Reconciliation Line Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_recon_line_reconciliation
ON bank_reconciliation_line(reconciliation_id);

CREATE INDEX IF NOT EXISTS idx_recon_line_status
ON bank_reconciliation_line(line_status);

CREATE INDEX IF NOT EXISTS idx_recon_line_needs_review
ON bank_reconciliation_line(reconciliation_id)
WHERE requires_manual_review = true;

CREATE INDEX IF NOT EXISTS idx_recon_line_statement
ON bank_reconciliation_line(bank_statement_line_id)
WHERE bank_statement_line_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_recon_line_payment
ON bank_reconciliation_line(matched_payment_id)
WHERE matched_payment_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_recon_line_cheque
ON bank_reconciliation_line(matched_cheque_id)
WHERE matched_cheque_id IS NOT NULL;

-- =============================================================================
-- Bank Statement Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_bank_statement_account
ON bank_statement(bank_account_id);

CREATE INDEX IF NOT EXISTS idx_bank_statement_period
ON bank_statement(statement_date);

-- =============================================================================
-- Bank Statement Line Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_statement_line_statement
ON bank_statement_line(bank_statement_id);

CREATE INDEX IF NOT EXISTS idx_statement_line_date
ON bank_statement_line(transaction_date);

CREATE INDEX IF NOT EXISTS idx_statement_line_reference
ON bank_statement_line(reference)
WHERE reference IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_statement_line_reconciled
ON bank_statement_line(is_reconciled);

-- =============================================================================
-- Treasury Audit Log Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_audit_log_entity
ON treasury_audit_log(entity_type, entity_id);

CREATE INDEX IF NOT EXISTS idx_audit_log_timestamp
ON treasury_audit_log(timestamp);

CREATE INDEX IF NOT EXISTS idx_audit_log_action
ON treasury_audit_log(action);

CREATE INDEX IF NOT EXISTS idx_audit_log_user
ON treasury_audit_log(user_id);

CREATE INDEX IF NOT EXISTS idx_audit_log_entity_number
ON treasury_audit_log(entity_number)
WHERE entity_number IS NOT NULL;

-- Composite index for common audit queries
CREATE INDEX IF NOT EXISTS idx_audit_log_entity_timestamp
ON treasury_audit_log(entity_type, entity_id, timestamp DESC);

-- =============================================================================
-- Treasury Number Sequence Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_number_seq_type_year
ON treasury_number_sequence(sequence_type, fiscal_year);

CREATE INDEX IF NOT EXISTS idx_number_seq_active
ON treasury_number_sequence(sequence_type, is_active)
WHERE is_active = true;

-- =============================================================================
-- Bank Parsing Profile Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_parsing_profile_bank
ON bank_parsing_profile(bank_code);

CREATE INDEX IF NOT EXISTS idx_parsing_profile_format
ON bank_parsing_profile(statement_format);

CREATE INDEX IF NOT EXISTS idx_parsing_profile_active
ON bank_parsing_profile(is_active)
WHERE is_active = true;

-- =============================================================================
-- Treasury Config Indexes
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_treasury_config_company
ON treasury_config(company_id);

CREATE INDEX IF NOT EXISTS idx_treasury_config_active
ON treasury_config(is_active)
WHERE is_active = true;

-- =============================================================================
-- Comments
-- =============================================================================
-- These indexes are optimized for:
-- 1. List queries with status filtering
-- 2. Date range queries for reporting
-- 3. Foreign key lookups
-- 4. Idempotency key checks
-- 5. Reconciliation workflows
-- 6. Audit log queries
--
-- Partial indexes (WHERE clauses) are used to reduce index size and improve performance
-- for the most common query patterns.
--
-- Review and adjust based on actual query patterns after deployment.
