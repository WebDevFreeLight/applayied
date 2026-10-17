-- =============================================================================
-- ARAP Critical Fields ROLLBACK Script
-- Version: 20251202_0100
-- Description: Rollback all changes from 20251202_0100_Add_ARAP_Critical_Fields.sql
-- Database: PostgreSQL
-- WARNING: This script will DROP columns and tables. Ensure you have a backup!
-- =============================================================================

BEGIN;

-- =============================================================================
-- PART 1: Remove columns from arap_customer
-- =============================================================================

ALTER TABLE arap_customer DROP COLUMN IF EXISTS address_ar;
ALTER TABLE arap_customer DROP COLUMN IF EXISTS address_en;
ALTER TABLE arap_customer DROP COLUMN IF EXISTS tax_number;
ALTER TABLE arap_customer DROP COLUMN IF EXISTS default_currency_id;
ALTER TABLE arap_customer DROP COLUMN IF EXISTS default_tax_group_id;
ALTER TABLE arap_customer DROP COLUMN IF EXISTS payment_terms_id;
ALTER TABLE arap_customer DROP COLUMN IF EXISTS credit_days;

-- =============================================================================
-- PART 2: Remove columns from arap_vendor
-- =============================================================================

ALTER TABLE arap_vendor DROP COLUMN IF EXISTS address_ar;
ALTER TABLE arap_vendor DROP COLUMN IF EXISTS address_en;
ALTER TABLE arap_vendor DROP COLUMN IF EXISTS tax_number;
ALTER TABLE arap_vendor DROP COLUMN IF EXISTS default_currency_id;
ALTER TABLE arap_vendor DROP COLUMN IF EXISTS default_tax_group_id;
ALTER TABLE arap_vendor DROP COLUMN IF EXISTS payment_terms_id;
ALTER TABLE arap_vendor DROP COLUMN IF EXISTS credit_days;

-- =============================================================================
-- PART 3: Remove is_tax_inclusive from invoice tables
-- =============================================================================

ALTER TABLE arap_ar_invoice DROP COLUMN IF EXISTS is_tax_inclusive;
ALTER TABLE arap_ap_invoice DROP COLUMN IF EXISTS is_tax_inclusive;

-- =============================================================================
-- PART 4: Remove tax fields from invoice line tables
-- =============================================================================

ALTER TABLE arap_ar_invoice_line DROP COLUMN IF EXISTS taxable_amount;
ALTER TABLE arap_ar_invoice_line DROP COLUMN IF EXISTS tax_inclusive_amount;
ALTER TABLE arap_ap_invoice_line DROP COLUMN IF EXISTS taxable_amount;
ALTER TABLE arap_ap_invoice_line DROP COLUMN IF EXISTS tax_inclusive_amount;

-- =============================================================================
-- PART 5: Remove is_retained_earnings_account from gl_account
-- =============================================================================

ALTER TABLE gl_account DROP COLUMN IF EXISTS is_retained_earnings_account;

-- =============================================================================
-- PART 6: Drop indexes
-- =============================================================================

DROP INDEX IF EXISTS ix_customer_payment_terms;
DROP INDEX IF EXISTS ix_vendor_payment_terms;
DROP INDEX IF EXISTS ix_customer_tax_number;
DROP INDEX IF EXISTS ix_vendor_tax_number;
DROP INDEX IF EXISTS ix_gl_account_retained_earnings;
DROP INDEX IF EXISTS ix_payment_terms_code;
DROP INDEX IF EXISTS ix_payment_terms_active;

-- =============================================================================
-- PART 7: Drop tables
-- =============================================================================

DROP TABLE IF EXISTS arap_invoice_number_sequence;
DROP TABLE IF EXISTS arap_payment_terms;

-- =============================================================================
-- PART 8: Remove migration record
-- =============================================================================

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'schema_migrations') THEN
        DELETE FROM schema_migrations WHERE version = '20251202_0100';
    END IF;
END $$;

COMMIT;

-- =============================================================================
-- VERIFICATION: Tables and columns should be removed
-- =============================================================================

-- SELECT table_name FROM information_schema.tables WHERE table_name IN ('arap_payment_terms', 'arap_invoice_number_sequence');
-- SELECT column_name FROM information_schema.columns WHERE table_name = 'arap_customer' AND column_name IN ('address_ar', 'tax_number', 'payment_terms_id');
