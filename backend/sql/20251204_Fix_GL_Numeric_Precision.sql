-- Fix GL Journal and GL Journal Entry numeric column precision
-- PostgreSQL error 22003 "numeric field overflow" occurs when value exceeds column precision
--
-- ROOT CAUSE: exchange_rate was defined as NUMERIC(10,10) which means:
--   - 10 total digits, 10 after decimal = 0 digits before decimal!
--   - This causes overflow when inserting 1.0 (needs 1 digit before decimal)
--
-- This script fixes all numeric columns to have proper precision

-- =====================================================
-- GL Journal Table
-- =====================================================

-- Fix total_debit column
ALTER TABLE gl_journal
ALTER COLUMN total_debit TYPE NUMERIC(18,4);

-- Fix total_credit column
ALTER TABLE gl_journal
ALTER COLUMN total_credit TYPE NUMERIC(18,4);

-- CRITICAL FIX: exchange_rate was NUMERIC(10,10) which allows NO digits before decimal!
-- Change to NUMERIC(18,10) to allow 8 digits before decimal + 10 after
ALTER TABLE gl_journal
ALTER COLUMN exchange_rate TYPE NUMERIC(18,10);

-- =====================================================
-- GL Journal Entry Table
-- =====================================================

-- Fix debit column
ALTER TABLE gl_journal_entry
ALTER COLUMN debit TYPE NUMERIC(18,4);

-- Fix credit column
ALTER TABLE gl_journal_entry
ALTER COLUMN credit TYPE NUMERIC(18,4);

-- Fix base_debit column
ALTER TABLE gl_journal_entry
ALTER COLUMN base_debit TYPE NUMERIC(18,4);

-- Fix base_credit column
ALTER TABLE gl_journal_entry
ALTER COLUMN base_credit TYPE NUMERIC(18,4);

-- Fix exchange_rate column
ALTER TABLE gl_journal_entry
ALTER COLUMN exchange_rate TYPE NUMERIC(18,6);

-- =====================================================
-- Asset Transaction Table
-- =====================================================

-- Fix amount column
ALTER TABLE asset_transaction
ALTER COLUMN amount TYPE NUMERIC(18,4);

-- Fix previous_book_value column
ALTER TABLE asset_transaction
ALTER COLUMN previous_book_value TYPE NUMERIC(18,4);

-- Fix new_book_value column
ALTER TABLE asset_transaction
ALTER COLUMN new_book_value TYPE NUMERIC(18,4);

-- Fix gain_loss_amount column
ALTER TABLE asset_transaction
ALTER COLUMN gain_loss_amount TYPE NUMERIC(18,4);

-- =====================================================
-- Fixed Asset Table
-- =====================================================

-- Fix purchase_cost column
ALTER TABLE fixed_asset
ALTER COLUMN purchase_cost TYPE NUMERIC(18,4);

-- Fix salvage_value column
ALTER TABLE fixed_asset
ALTER COLUMN salvage_value TYPE NUMERIC(18,4);

-- Fix current_book_value column
ALTER TABLE fixed_asset
ALTER COLUMN current_book_value TYPE NUMERIC(18,4);

-- Fix accumulated_depreciation column
ALTER TABLE fixed_asset
ALTER COLUMN accumulated_depreciation TYPE NUMERIC(18,4);

-- Verify the changes
SELECT
    table_name,
    column_name,
    data_type,
    numeric_precision,
    numeric_scale
FROM information_schema.columns
WHERE table_name IN ('gl_journal', 'gl_journal_entry', 'asset_transaction', 'fixed_asset')
AND data_type = 'numeric'
ORDER BY table_name, column_name;
