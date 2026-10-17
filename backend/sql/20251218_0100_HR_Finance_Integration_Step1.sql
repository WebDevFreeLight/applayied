-- =============================================
-- HR-Finance Integration - Step 1 (Schema Only)
-- Date: 2025-12-18
-- Notes:
-- - Idempotent DDL where possible
-- - HR tables use SqlSugar default naming (lowercase property name) in existing datasets
-- =============================================

-- =============================================
-- 1) hr_employee
-- Remove redundant salary columns; add GLAccountId FK -> gl_account.account_id
-- =============================================
ALTER TABLE hr_employee
    DROP COLUMN IF EXISTS basicsalary,
    DROP COLUMN IF EXISTS housingallowance,
    DROP COLUMN IF EXISTS transportallowance,
    DROP COLUMN IF EXISTS otherallowances,
    DROP COLUMN IF EXISTS foodallowance;

ALTER TABLE hr_employee
    ADD COLUMN IF NOT EXISTS glaccountid UUID NULL;

ALTER TABLE hr_employee
    DROP CONSTRAINT IF EXISTS fk_hr_employee_gl_account;

ALTER TABLE hr_employee
    ADD CONSTRAINT fk_hr_employee_gl_account
        FOREIGN KEY (glaccountid)
        REFERENCES gl_account(account_id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

-- =============================================
-- 2) hr_payroll
-- Add APInvoiceId FK -> arap_ap_invoice.id
-- Add TreasuryTransactionId FK -> erp_treasury_transactions.transactionid (BIGINT)
-- Add CurrencyId FK -> currencies.id
-- Add ExchangeRate
-- =============================================
ALTER TABLE hr_payroll
    ADD COLUMN IF NOT EXISTS apinvoiceid UUID NULL,
    ADD COLUMN IF NOT EXISTS treasurytransactionid BIGINT NULL,
    ADD COLUMN IF NOT EXISTS currencyid UUID NULL,
    ADD COLUMN IF NOT EXISTS exchangerate NUMERIC(18,10) NOT NULL DEFAULT 1.0;

ALTER TABLE hr_payroll
    DROP CONSTRAINT IF EXISTS fk_hr_payroll_ap_invoice;
ALTER TABLE hr_payroll
    ADD CONSTRAINT fk_hr_payroll_ap_invoice
        FOREIGN KEY (apinvoiceid)
        REFERENCES arap_ap_invoice(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE hr_payroll
    DROP CONSTRAINT IF EXISTS fk_hr_payroll_treasury_tx;
DO $$
BEGIN
    IF to_regclass('public.erp_treasury_transactions') IS NULL THEN
        RAISE NOTICE 'Skipping FK fk_hr_payroll_treasury_tx: table erp_treasury_transactions not found';
        RETURN;
    END IF;

    -- Support either transactionid or transaction_id as the PK column name
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'erp_treasury_transactions'
          AND column_name = 'transactionid'
    ) THEN
        EXECUTE 'ALTER TABLE hr_payroll DROP CONSTRAINT IF EXISTS fk_hr_payroll_treasury_tx';
        EXECUTE 'ALTER TABLE hr_payroll ADD CONSTRAINT fk_hr_payroll_treasury_tx
                 FOREIGN KEY (treasurytransactionid)
                 REFERENCES erp_treasury_transactions(transactionid)
                 ON DELETE SET NULL
                 ON UPDATE CASCADE';
        RETURN;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'erp_treasury_transactions'
          AND column_name = 'transaction_id'
    ) THEN
        EXECUTE 'ALTER TABLE hr_payroll DROP CONSTRAINT IF EXISTS fk_hr_payroll_treasury_tx';
        EXECUTE 'ALTER TABLE hr_payroll ADD CONSTRAINT fk_hr_payroll_treasury_tx
                 FOREIGN KEY (treasurytransactionid)
                 REFERENCES erp_treasury_transactions(transaction_id)
                 ON DELETE SET NULL
                 ON UPDATE CASCADE';
        RETURN;
    END IF;

    RAISE NOTICE 'Skipping FK fk_hr_payroll_treasury_tx: PK column not found (expected transactionid or transaction_id)';
END $$;

-- =============================================
-- 3) hr_payroll_detail
-- Add tax & ZATCA linkages
-- =============================================
ALTER TABLE hr_payroll_detail
    ADD COLUMN IF NOT EXISTS taxcodeid UUID NULL,
    ADD COLUMN IF NOT EXISTS zatcahashid UUID NULL,
    ADD COLUMN IF NOT EXISTS istaxinclusive BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE hr_payroll_detail
    DROP CONSTRAINT IF EXISTS fk_hr_payroll_detail_tax_code;
ALTER TABLE hr_payroll_detail
    ADD CONSTRAINT fk_hr_payroll_detail_tax_code
        FOREIGN KEY (taxcodeid)
        REFERENCES tax_code(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE hr_payroll_detail
    DROP CONSTRAINT IF EXISTS fk_hr_payroll_detail_zatca_hash;
ALTER TABLE hr_payroll_detail
    ADD CONSTRAINT fk_hr_payroll_detail_zatca_hash
        FOREIGN KEY (zatcahashid)
        REFERENCES zatca_invoice_hash(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

-- =============================================
-- 4) hr_loan + hr_loan_repayment
-- Add GL journal + payment hub linkage
-- =============================================
ALTER TABLE hr_loan
    ADD COLUMN IF NOT EXISTS gljournalid UUID NULL,
    ADD COLUMN IF NOT EXISTS paymenthubtransactionid UUID NULL;

ALTER TABLE hr_loan
    DROP CONSTRAINT IF EXISTS fk_hr_loan_gl_journal;
ALTER TABLE hr_loan
    ADD CONSTRAINT fk_hr_loan_gl_journal
        FOREIGN KEY (gljournalid)
        REFERENCES gl_journal(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

-- payment_hub_transaction table is expected by HR linkages; create if missing (minimal SSOT)
CREATE TABLE IF NOT EXISTS payment_hub_transaction (
    id UUID PRIMARY KEY,
    transaction_type VARCHAR(50) NULL,
    amount NUMERIC(18,4) NOT NULL DEFAULT 0,
    currency_code VARCHAR(10) NULL,
    status VARCHAR(20) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE hr_loan
    DROP CONSTRAINT IF EXISTS fk_hr_loan_payment_hub_tx;
ALTER TABLE hr_loan
    ADD CONSTRAINT fk_hr_loan_payment_hub_tx
        FOREIGN KEY (paymenthubtransactionid)
        REFERENCES payment_hub_transaction(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE hr_loan_repayment
    ADD COLUMN IF NOT EXISTS gljournalid UUID NULL,
    ADD COLUMN IF NOT EXISTS paymenthubtransactionid UUID NULL;

ALTER TABLE hr_loan_repayment
    DROP CONSTRAINT IF EXISTS fk_hr_loan_repayment_gl_journal;
ALTER TABLE hr_loan_repayment
    ADD CONSTRAINT fk_hr_loan_repayment_gl_journal
        FOREIGN KEY (gljournalid)
        REFERENCES gl_journal(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE hr_loan_repayment
    DROP CONSTRAINT IF EXISTS fk_hr_loan_repayment_payment_hub_tx;
ALTER TABLE hr_loan_repayment
    ADD CONSTRAINT fk_hr_loan_repayment_payment_hub_tx
        FOREIGN KEY (paymenthubtransactionid)
        REFERENCES payment_hub_transaction(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

-- =============================================
-- 5) Benefits
-- BudgetAllocationId -> finance_budget_lines.id
-- ARInvoiceId -> arap_ar_invoice.id
-- Claims -> TreasuryPaymentId + ZATCA status
-- =============================================
ALTER TABLE hr_benefit_plan
    ADD COLUMN IF NOT EXISTS budgetallocationid UUID NULL,
    ADD COLUMN IF NOT EXISTS arinvoiceid UUID NULL;

ALTER TABLE hr_benefit_plan
    DROP CONSTRAINT IF EXISTS fk_hr_benefit_plan_budget_line;
ALTER TABLE hr_benefit_plan
    ADD CONSTRAINT fk_hr_benefit_plan_budget_line
        FOREIGN KEY (budgetallocationid)
        REFERENCES finance_budget_lines(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE hr_benefit_plan
    DROP CONSTRAINT IF EXISTS fk_hr_benefit_plan_ar_invoice;
ALTER TABLE hr_benefit_plan
    ADD CONSTRAINT fk_hr_benefit_plan_ar_invoice
        FOREIGN KEY (arinvoiceid)
        REFERENCES arap_ar_invoice(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE hr_benefit_enrollment
    ADD COLUMN IF NOT EXISTS budgetallocationid UUID NULL,
    ADD COLUMN IF NOT EXISTS arinvoiceid UUID NULL;

ALTER TABLE hr_benefit_enrollment
    DROP CONSTRAINT IF EXISTS fk_hr_benefit_enrollment_budget_line;
ALTER TABLE hr_benefit_enrollment
    ADD CONSTRAINT fk_hr_benefit_enrollment_budget_line
        FOREIGN KEY (budgetallocationid)
        REFERENCES finance_budget_lines(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE hr_benefit_enrollment
    DROP CONSTRAINT IF EXISTS fk_hr_benefit_enrollment_ar_invoice;
ALTER TABLE hr_benefit_enrollment
    ADD CONSTRAINT fk_hr_benefit_enrollment_ar_invoice
        FOREIGN KEY (arinvoiceid)
        REFERENCES arap_ar_invoice(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

-- treasury_payment table expected by HR claims; create if missing (minimal SSOT)
CREATE TABLE IF NOT EXISTS treasury_payment (
    id UUID PRIMARY KEY,
    payment_hub_transaction_id UUID NULL,
    amount NUMERIC(18,4) NOT NULL DEFAULT 0,
    currency_code VARCHAR(10) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_treasury_payment_payment_hub_tx
        FOREIGN KEY (payment_hub_transaction_id)
        REFERENCES payment_hub_transaction(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

ALTER TABLE hr_benefit_claim
    ADD COLUMN IF NOT EXISTS treasurypaymentid UUID NULL,
    ADD COLUMN IF NOT EXISTS zatcastatus INT NOT NULL DEFAULT 0;

ALTER TABLE hr_benefit_claim
    DROP CONSTRAINT IF EXISTS fk_hr_benefit_claim_treasury_payment;
ALTER TABLE hr_benefit_claim
    ADD CONSTRAINT fk_hr_benefit_claim_treasury_payment
        FOREIGN KEY (treasurypaymentid)
        REFERENCES treasury_payment(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

-- =============================================
-- 6) hr_retirement_plan -> GL account
-- =============================================
ALTER TABLE hr_retirement_plan
    ADD COLUMN IF NOT EXISTS glaccountid UUID NULL;

ALTER TABLE hr_retirement_plan
    DROP CONSTRAINT IF EXISTS fk_hr_retirement_plan_gl_account;
ALTER TABLE hr_retirement_plan
    ADD CONSTRAINT fk_hr_retirement_plan_gl_account
        FOREIGN KEY (glaccountid)
        REFERENCES gl_account(account_id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

-- =============================================
-- 7) hr_salary_compliance -> tax configuration + ZATCA environment
-- =============================================
ALTER TABLE hr_salary_compliance
    ADD COLUMN IF NOT EXISTS taxconfigid UUID NULL,
    ADD COLUMN IF NOT EXISTS zatcaenvironment INT NOT NULL DEFAULT 0;

ALTER TABLE hr_salary_compliance
    DROP CONSTRAINT IF EXISTS fk_hr_salary_compliance_tax_config;
ALTER TABLE hr_salary_compliance
    ADD CONSTRAINT fk_hr_salary_compliance_tax_config
        FOREIGN KEY (taxconfigid)
        REFERENCES tax_configuration(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

-- =============================================
-- 8) hr_compensation_metrics -> GL journal
-- =============================================
ALTER TABLE hr_compensation_metrics
    ADD COLUMN IF NOT EXISTS gljournalid UUID NULL;

ALTER TABLE hr_compensation_metrics
    DROP CONSTRAINT IF EXISTS fk_hr_compensation_metrics_gl_journal;
ALTER TABLE hr_compensation_metrics
    ADD CONSTRAINT fk_hr_compensation_metrics_gl_journal
        FOREIGN KEY (gljournalid)
        REFERENCES gl_journal(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

ALTER TABLE hr_payroll
    DROP CONSTRAINT IF EXISTS fk_hr_payroll_currency;
ALTER TABLE hr_payroll
    ADD CONSTRAINT fk_hr_payroll_currency
        FOREIGN KEY (currencyid)
        REFERENCES currencies(id)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

-- =============================================
-- 10) New HR tables
-- =============================================
CREATE TABLE IF NOT EXISTS hr_finance_reconciliation (
    reconciliationid UUID PRIMARY KEY,
    payrollid BIGINT NOT NULL,
    apinvoiceid UUID NOT NULL,
    reconciledamount NUMERIC(18,4) NOT NULL DEFAULT 0,
    status INT NOT NULL DEFAULT 0,
    aiinsight TEXT NULL,
    create_time TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE hr_finance_reconciliation
    DROP CONSTRAINT IF EXISTS fk_hr_finance_recon_payroll;
ALTER TABLE hr_finance_reconciliation
    ADD CONSTRAINT fk_hr_finance_recon_payroll
        FOREIGN KEY (payrollid)
        REFERENCES hr_payroll(payrollid)
        ON DELETE RESTRICT
        ON UPDATE CASCADE;

ALTER TABLE hr_finance_reconciliation
    DROP CONSTRAINT IF EXISTS fk_hr_finance_recon_ap_invoice;
ALTER TABLE hr_finance_reconciliation
    ADD CONSTRAINT fk_hr_finance_recon_ap_invoice
        FOREIGN KEY (apinvoiceid)
        REFERENCES arap_ap_invoice(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE;

CREATE INDEX IF NOT EXISTS ix_hr_finance_recon_status ON hr_finance_reconciliation(status);
CREATE INDEX IF NOT EXISTS ix_hr_finance_recon_payroll ON hr_finance_reconciliation(payrollid);

CREATE TABLE IF NOT EXISTS hr_payroll_zatca (
    id UUID PRIMARY KEY,
    payrollid BIGINT NOT NULL,
    zatcahashid UUID NOT NULL,
    submissionstatus INT NOT NULL DEFAULT 0,
    create_time TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

ALTER TABLE hr_payroll_zatca
    DROP CONSTRAINT IF EXISTS fk_hr_payroll_zatca_payroll;
ALTER TABLE hr_payroll_zatca
    ADD CONSTRAINT fk_hr_payroll_zatca_payroll
        FOREIGN KEY (payrollid)
        REFERENCES hr_payroll(payrollid)
        ON DELETE CASCADE
        ON UPDATE CASCADE;

ALTER TABLE hr_payroll_zatca
    DROP CONSTRAINT IF EXISTS fk_hr_payroll_zatca_hash;
ALTER TABLE hr_payroll_zatca
    ADD CONSTRAINT fk_hr_payroll_zatca_hash
        FOREIGN KEY (zatcahashid)
        REFERENCES zatca_invoice_hash(id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE;

CREATE INDEX IF NOT EXISTS ix_hr_payroll_zatca_payroll ON hr_payroll_zatca(payrollid);

CREATE TABLE IF NOT EXISTS hr_financial_audit_log (
    logid UUID PRIMARY KEY,
    action VARCHAR(200) NOT NULL,
    userid BIGINT NOT NULL,
    "timestamp" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    changes TEXT NULL
);

CREATE INDEX IF NOT EXISTS ix_hr_financial_audit_log_timestamp ON hr_financial_audit_log("timestamp");
CREATE INDEX IF NOT EXISTS ix_hr_financial_audit_log_user ON hr_financial_audit_log(userid);


