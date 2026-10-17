-- =============================================
-- HR Normalization & Security - Phase 1
-- Date: 2025-12-20
-- Target: hr_* tables (HR module)
-- DB: PostgreSQL-style idempotent DDL (uses IF NOT EXISTS / DO $$ blocks)
-- Notes:
-- - Zero-downtime: additive columns/tables/indexes/FKs first
-- - Sensitive fields: add encrypted columns; migrate data separately; then stop persisting plaintext
-- =============================================

-- =========================================================
-- 1) hr_employee: add finance linkage + encrypted bank fields
-- =========================================================
ALTER TABLE hr_employee
    ADD COLUMN IF NOT EXISTS glaccountid UUID NULL,
    ADD COLUMN IF NOT EXISTS iban_encrypted TEXT NULL,
    ADD COLUMN IF NOT EXISTS accountnumber_encrypted TEXT NULL,
    ADD COLUMN IF NOT EXISTS bankaccount_encrypted TEXT NULL;

-- FK -> gl_account(account_id) if Finance is installed
ALTER TABLE hr_employee
    DROP CONSTRAINT IF EXISTS fk_hr_employee_gl_account;
DO $$
BEGIN
    IF to_regclass('public.gl_account') IS NULL THEN
        RAISE NOTICE 'Skipping fk_hr_employee_gl_account: table gl_account not found';
        RETURN;
    END IF;
    EXECUTE 'ALTER TABLE hr_employee
             ADD CONSTRAINT fk_hr_employee_gl_account
             FOREIGN KEY (glaccountid)
             REFERENCES gl_account(account_id)
             ON DELETE SET NULL
             ON UPDATE CASCADE';
END $$;

CREATE INDEX IF NOT EXISTS ix_hr_employee_department ON hr_employee(departmentid);

-- =========================================================
-- 2) hr_salary_history: effective-dated salary source-of-truth
-- =========================================================
CREATE TABLE IF NOT EXISTS hr_salary_history (
    salaryhistoryid BIGSERIAL PRIMARY KEY,
    employeeid BIGINT NOT NULL,
    effectivefrom DATE NOT NULL,
    effectiveto DATE NULL,
    basicsalary NUMERIC(18,4) NOT NULL DEFAULT 0,
    housingallowance NUMERIC(18,4) NOT NULL DEFAULT 0,
    transportallowance NUMERIC(18,4) NOT NULL DEFAULT 0,
    otherallowances NUMERIC(18,4) NOT NULL DEFAULT 0,
    currencyid UUID NULL,
    exchangerate NUMERIC(18,10) NOT NULL DEFAULT 1.0,
    create_by VARCHAR(64) NULL,
    create_time TIMESTAMPTZ NULL,
    update_by VARCHAR(64) NULL,
    update_time TIMESTAMPTZ NULL,
    remark VARCHAR(500) NULL,
    companyid BIGINT NULL,
    branchid BIGINT NULL
);

CREATE INDEX IF NOT EXISTS ix_hr_salary_history_employee ON hr_salary_history(employeeid);
CREATE INDEX IF NOT EXISTS ix_hr_salary_history_effective ON hr_salary_history(employeeid, effectivefrom);

ALTER TABLE hr_salary_history
    DROP CONSTRAINT IF EXISTS fk_hr_salary_history_employee;
ALTER TABLE hr_salary_history
    ADD CONSTRAINT fk_hr_salary_history_employee
    FOREIGN KEY (employeeid)
    REFERENCES hr_employee(employeeid)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- =========================================================
-- 3) hr_payroll: uniqueness + FKs
-- =========================================================
CREATE UNIQUE INDEX IF NOT EXISTS ux_hr_payroll_employee_period ON hr_payroll(employeeid, payperiod);

ALTER TABLE hr_payroll
    DROP CONSTRAINT IF EXISTS fk_hr_payroll_employee;
ALTER TABLE hr_payroll
    ADD CONSTRAINT fk_hr_payroll_employee
    FOREIGN KEY (employeeid)
    REFERENCES hr_employee(employeeid)
    ON DELETE RESTRICT
    ON UPDATE CASCADE;

CREATE INDEX IF NOT EXISTS ix_hr_payroll_period ON hr_payroll(payperiod);
CREATE INDEX IF NOT EXISTS ix_hr_payroll_status ON hr_payroll(status);

-- =========================================================
-- 4) hr_payroll_detail: FKs
-- =========================================================
CREATE INDEX IF NOT EXISTS ix_hr_payroll_detail_payrollid ON hr_payroll_detail(payrollid);

ALTER TABLE hr_payroll_detail
    DROP CONSTRAINT IF EXISTS fk_hr_payroll_detail_payroll;
ALTER TABLE hr_payroll_detail
    ADD CONSTRAINT fk_hr_payroll_detail_payroll
    FOREIGN KEY (payrollid)
    REFERENCES hr_payroll(payrollid)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- =========================================================
-- 5) hr_loan + hr_loan_repayment: FKs + uniqueness
-- =========================================================
CREATE INDEX IF NOT EXISTS ix_hr_loan_employee ON hr_loan(employeeid);
CREATE INDEX IF NOT EXISTS ix_hr_loan_status ON hr_loan(status);

ALTER TABLE hr_loan
    DROP CONSTRAINT IF EXISTS fk_hr_loan_employee;
ALTER TABLE hr_loan
    ADD CONSTRAINT fk_hr_loan_employee
    FOREIGN KEY (employeeid)
    REFERENCES hr_employee(employeeid)
    ON DELETE RESTRICT
    ON UPDATE CASCADE;

-- Prevent double repayments per period per loan
CREATE UNIQUE INDEX IF NOT EXISTS ux_hr_loan_repayment_loan_period ON hr_loan_repayment(loanid, payperiod);
CREATE INDEX IF NOT EXISTS ix_hr_loan_repayment_employee_period ON hr_loan_repayment(employeeid, payperiod);

ALTER TABLE hr_loan_repayment
    DROP CONSTRAINT IF EXISTS fk_hr_loan_repayment_loan;
ALTER TABLE hr_loan_repayment
    ADD CONSTRAINT fk_hr_loan_repayment_loan
    FOREIGN KEY (loanid)
    REFERENCES hr_loan(loanid)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

ALTER TABLE hr_loan_repayment
    DROP CONSTRAINT IF EXISTS fk_hr_loan_repayment_employee;
ALTER TABLE hr_loan_repayment
    ADD CONSTRAINT fk_hr_loan_repayment_employee
    FOREIGN KEY (employeeid)
    REFERENCES hr_employee(employeeid)
    ON DELETE RESTRICT
    ON UPDATE CASCADE;

-- =========================================================
-- 6) Sample integrity checks (safe SELECTs)
-- =========================================================
-- Payroll duplicates check (should be 0 rows after unique index enforcement)
-- SELECT employeeid, payperiod, COUNT(*) FROM hr_payroll GROUP BY employeeid, payperiod HAVING COUNT(*) > 1;
--
-- Loan repayment duplicates check (should be 0 rows after unique index enforcement)
-- SELECT loanid, payperiod, COUNT(*) FROM hr_loan_repayment GROUP BY loanid, payperiod HAVING COUNT(*) > 1;














