-- =============================================================================
-- ARAP Critical Fields Migration Script
-- Version: 20251202_0100
-- Description: Add payment terms, address fields, tax-inclusive support,
--              invoice number sequence, and retained earnings flag
-- Database: PostgreSQL
-- Author: Claude Code
-- Date: 2025-12-02
-- =============================================================================

-- IDEMPOTENT: All statements use IF NOT EXISTS or check for column existence

BEGIN;

-- =============================================================================
-- PART 1: Create arap_payment_terms Table
-- =============================================================================

CREATE TABLE IF NOT EXISTS arap_payment_terms (
    id SERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name_en VARCHAR(200) NOT NULL,
    name_ar VARCHAR(200),
    net_days INT NOT NULL DEFAULT 30,
    discount_days INT,
    discount_percent DECIMAL(5,2),
    description_en VARCHAR(500),
    description_ar VARCHAR(500),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    sort_order INT NOT NULL DEFAULT 0,
    create_by VARCHAR(64),
    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    update_by VARCHAR(64),
    update_time TIMESTAMP,
    remark VARCHAR(500),
    company_id BIGINT,
    branch_id BIGINT
);

-- Add unique constraint on code + company_id (idempotent)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'uk_payment_terms_code_company'
    ) THEN
        ALTER TABLE arap_payment_terms
        ADD CONSTRAINT uk_payment_terms_code_company UNIQUE (code, company_id);
    END IF;
END $$;

-- =============================================================================
-- PART 2: Add columns to arap_customer
-- =============================================================================

-- Add address_ar column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_customer' AND column_name = 'address_ar'
    ) THEN
        ALTER TABLE arap_customer ADD COLUMN address_ar VARCHAR(500);
    END IF;
END $$;

-- Add address_en column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_customer' AND column_name = 'address_en'
    ) THEN
        ALTER TABLE arap_customer ADD COLUMN address_en VARCHAR(500);
    END IF;
END $$;

-- Add tax_number column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_customer' AND column_name = 'tax_number'
    ) THEN
        ALTER TABLE arap_customer ADD COLUMN tax_number VARCHAR(50);
    END IF;
END $$;

-- Add default_currency_id column (FK to currencies table)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_customer' AND column_name = 'default_currency_id'
    ) THEN
        ALTER TABLE arap_customer ADD COLUMN default_currency_id UUID;
    END IF;
END $$;

-- Add default_tax_group_id column (FK to tax_group table)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_customer' AND column_name = 'default_tax_group_id'
    ) THEN
        ALTER TABLE arap_customer ADD COLUMN default_tax_group_id UUID;
    END IF;
END $$;

-- Add payment_terms_id column (FK to arap_payment_terms)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_customer' AND column_name = 'payment_terms_id'
    ) THEN
        ALTER TABLE arap_customer ADD COLUMN payment_terms_id INT;
    END IF;
END $$;

-- Add credit_days column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_customer' AND column_name = 'credit_days'
    ) THEN
        ALTER TABLE arap_customer ADD COLUMN credit_days INT DEFAULT 30;
    END IF;
END $$;

-- =============================================================================
-- PART 3: Add columns to arap_vendor
-- =============================================================================

-- Add address_ar column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_vendor' AND column_name = 'address_ar'
    ) THEN
        ALTER TABLE arap_vendor ADD COLUMN address_ar VARCHAR(500);
    END IF;
END $$;

-- Add address_en column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_vendor' AND column_name = 'address_en'
    ) THEN
        ALTER TABLE arap_vendor ADD COLUMN address_en VARCHAR(500);
    END IF;
END $$;

-- Add tax_number column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_vendor' AND column_name = 'tax_number'
    ) THEN
        ALTER TABLE arap_vendor ADD COLUMN tax_number VARCHAR(50);
    END IF;
END $$;

-- Add default_currency_id column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_vendor' AND column_name = 'default_currency_id'
    ) THEN
        ALTER TABLE arap_vendor ADD COLUMN default_currency_id UUID;
    END IF;
END $$;

-- Add default_tax_group_id column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_vendor' AND column_name = 'default_tax_group_id'
    ) THEN
        ALTER TABLE arap_vendor ADD COLUMN default_tax_group_id UUID;
    END IF;
END $$;

-- Add payment_terms_id column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_vendor' AND column_name = 'payment_terms_id'
    ) THEN
        ALTER TABLE arap_vendor ADD COLUMN payment_terms_id INT;
    END IF;
END $$;

-- Add credit_days column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_vendor' AND column_name = 'credit_days'
    ) THEN
        ALTER TABLE arap_vendor ADD COLUMN credit_days INT DEFAULT 30;
    END IF;
END $$;

-- =============================================================================
-- PART 4: Add is_tax_inclusive to AR/AP Invoice tables
-- =============================================================================

-- Add is_tax_inclusive to arap_ar_invoice
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_ar_invoice' AND column_name = 'is_tax_inclusive'
    ) THEN
        ALTER TABLE arap_ar_invoice ADD COLUMN is_tax_inclusive BOOLEAN NOT NULL DEFAULT FALSE;
    END IF;
END $$;

-- Add is_tax_inclusive to arap_ap_invoice
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_ap_invoice' AND column_name = 'is_tax_inclusive'
    ) THEN
        ALTER TABLE arap_ap_invoice ADD COLUMN is_tax_inclusive BOOLEAN NOT NULL DEFAULT FALSE;
    END IF;
END $$;

-- =============================================================================
-- PART 5: Add tax fields to invoice line tables
-- =============================================================================

-- Add taxable_amount to arap_ar_invoice_line
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_ar_invoice_line' AND column_name = 'taxable_amount'
    ) THEN
        ALTER TABLE arap_ar_invoice_line ADD COLUMN taxable_amount DECIMAL(18,4);
    END IF;
END $$;

-- Add tax_inclusive_amount to arap_ar_invoice_line
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_ar_invoice_line' AND column_name = 'tax_inclusive_amount'
    ) THEN
        ALTER TABLE arap_ar_invoice_line ADD COLUMN tax_inclusive_amount DECIMAL(18,4);
    END IF;
END $$;

-- Add taxable_amount to arap_ap_invoice_line
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_ap_invoice_line' AND column_name = 'taxable_amount'
    ) THEN
        ALTER TABLE arap_ap_invoice_line ADD COLUMN taxable_amount DECIMAL(18,4);
    END IF;
END $$;

-- Add tax_inclusive_amount to arap_ap_invoice_line
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'arap_ap_invoice_line' AND column_name = 'tax_inclusive_amount'
    ) THEN
        ALTER TABLE arap_ap_invoice_line ADD COLUMN tax_inclusive_amount DECIMAL(18,4);
    END IF;
END $$;

-- =============================================================================
-- PART 6: Add is_retained_earnings_account to gl_account
-- =============================================================================

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'gl_account' AND column_name = 'is_retained_earnings_account'
    ) THEN
        ALTER TABLE gl_account ADD COLUMN is_retained_earnings_account BOOLEAN NOT NULL DEFAULT FALSE;
    END IF;
END $$;

-- =============================================================================
-- PART 7: Create arap_invoice_number_sequence Table
-- =============================================================================

CREATE TABLE IF NOT EXISTS arap_invoice_number_sequence (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sequence_type VARCHAR(10) NOT NULL,  -- 'AR' or 'AP'
    company_id BIGINT,
    fiscal_year INT NOT NULL,
    current_number BIGINT NOT NULL DEFAULT 0,
    min_digits INT NOT NULL DEFAULT 5,
    row_version INT NOT NULL DEFAULT 1,
    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    update_time TIMESTAMP
);

-- Add unique constraint on sequence_type + company_id + fiscal_year (idempotent)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'uk_invoice_seq'
    ) THEN
        ALTER TABLE arap_invoice_number_sequence
        ADD CONSTRAINT uk_invoice_seq UNIQUE (sequence_type, company_id, fiscal_year);
    END IF;
END $$;

-- =============================================================================
-- PART 8: Create Indexes for Performance
-- =============================================================================

-- Index on customer payment_terms_id
CREATE INDEX IF NOT EXISTS ix_customer_payment_terms ON arap_customer (payment_terms_id);

-- Index on vendor payment_terms_id
CREATE INDEX IF NOT EXISTS ix_vendor_payment_terms ON arap_vendor (payment_terms_id);

-- Index on customer tax_number
CREATE INDEX IF NOT EXISTS ix_customer_tax_number ON arap_customer (tax_number) WHERE tax_number IS NOT NULL;

-- Index on vendor tax_number
CREATE INDEX IF NOT EXISTS ix_vendor_tax_number ON arap_vendor (tax_number) WHERE tax_number IS NOT NULL;

-- Index on gl_account is_retained_earnings_account
CREATE INDEX IF NOT EXISTS ix_gl_account_retained_earnings ON gl_account (is_retained_earnings_account) WHERE is_retained_earnings_account = TRUE;

-- Index on payment_terms code
CREATE INDEX IF NOT EXISTS ix_payment_terms_code ON arap_payment_terms (code);

-- Index on payment_terms is_active
CREATE INDEX IF NOT EXISTS ix_payment_terms_active ON arap_payment_terms (is_active) WHERE is_active = TRUE;

-- =============================================================================
-- PART 9: Insert Default Payment Terms
-- =============================================================================

INSERT INTO arap_payment_terms (code, name_en, name_ar, net_days, is_active, sort_order, create_time)
SELECT 'NET30', 'Net 30 Days', 'صافي 30 يوم', 30, TRUE, 1, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM arap_payment_terms WHERE code = 'NET30');

INSERT INTO arap_payment_terms (code, name_en, name_ar, net_days, is_active, sort_order, create_time)
SELECT 'NET60', 'Net 60 Days', 'صافي 60 يوم', 60, TRUE, 2, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM arap_payment_terms WHERE code = 'NET60');

INSERT INTO arap_payment_terms (code, name_en, name_ar, net_days, is_active, sort_order, create_time)
SELECT 'NET90', 'Net 90 Days', 'صافي 90 يوم', 90, TRUE, 3, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM arap_payment_terms WHERE code = 'NET90');

INSERT INTO arap_payment_terms (code, name_en, name_ar, net_days, is_active, sort_order, create_time)
SELECT 'IMMEDIATE', 'Due Immediately', 'مستحق فوراً', 0, TRUE, 0, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM arap_payment_terms WHERE code = 'IMMEDIATE');

INSERT INTO arap_payment_terms (code, name_en, name_ar, net_days, discount_days, discount_percent, is_active, sort_order, create_time)
SELECT '2-10-NET30', '2% 10 Net 30', '2% خلال 10 أيام صافي 30', 30, 10, 2.00, TRUE, 4, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM arap_payment_terms WHERE code = '2-10-NET30');

-- =============================================================================
-- Record Migration in schema_migrations (if table exists)
-- =============================================================================

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'schema_migrations') THEN
        INSERT INTO schema_migrations (version, applied_at, description)
        SELECT '20251202_0100', CURRENT_TIMESTAMP, 'Add ARAP Critical Fields'
        WHERE NOT EXISTS (SELECT 1 FROM schema_migrations WHERE version = '20251202_0100');
    END IF;
END $$;

COMMIT;

-- =============================================================================
-- VERIFICATION QUERIES (run separately to verify migration)
-- =============================================================================

-- SELECT 'payment_terms' as table_name, COUNT(*) as row_count FROM arap_payment_terms;
-- SELECT column_name FROM information_schema.columns WHERE table_name = 'arap_customer' ORDER BY ordinal_position;
-- SELECT column_name FROM information_schema.columns WHERE table_name = 'arap_vendor' ORDER BY ordinal_position;
-- SELECT column_name FROM information_schema.columns WHERE table_name = 'arap_ar_invoice' ORDER BY ordinal_position;
-- SELECT column_name FROM information_schema.columns WHERE table_name = 'gl_account' WHERE column_name = 'is_retained_earnings_account';
