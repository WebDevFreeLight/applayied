-- =============================================
-- GLAccount Cash Flow Classification Migration Script
-- Purpose: Add new columns for IAS 7 Cash Flow Statement classification
-- Date: December 14, 2024
-- Version: 1.0
-- =============================================

-- This script adds the following columns to gl_account table:
-- 1. cash_flow_category (INT) - IAS 7 classification: None=0, Operating=1, Investing=2, Financing=3
-- 2. account_flags (INT) - Bitwise flags for account classification
-- 3. working_capital_type (INT) - Type of working capital account (nullable)

-- =============================================
-- STEP 1: Add new columns
-- =============================================

-- Check if columns exist before adding (PostgreSQL)
DO $$
BEGIN
    -- Add cash_flow_category column
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'gl_account' AND column_name = 'cash_flow_category') THEN
        ALTER TABLE gl_account ADD COLUMN cash_flow_category INT NOT NULL DEFAULT 0;
        RAISE NOTICE 'Added cash_flow_category column';
    ELSE
        RAISE NOTICE 'cash_flow_category column already exists';
    END IF;

    -- Add account_flags column
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'gl_account' AND column_name = 'account_flags') THEN
        ALTER TABLE gl_account ADD COLUMN account_flags INT NOT NULL DEFAULT 0;
        RAISE NOTICE 'Added account_flags column';
    ELSE
        RAISE NOTICE 'account_flags column already exists';
    END IF;

    -- Add working_capital_type column (nullable)
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'gl_account' AND column_name = 'working_capital_type') THEN
        ALTER TABLE gl_account ADD COLUMN working_capital_type INT NULL;
        RAISE NOTICE 'Added working_capital_type column';
    ELSE
        RAISE NOTICE 'working_capital_type column already exists';
    END IF;
END $$;

-- =============================================
-- STEP 2: Add comments for documentation
-- =============================================

COMMENT ON COLUMN gl_account.cash_flow_category IS 'IAS 7 Cash Flow Category: 0=None, 1=Operating, 2=Investing, 3=Financing';
COMMENT ON COLUMN gl_account.account_flags IS 'Bitwise flags: 1=Depreciation, 2=Amortization, 4=Provision, 8=WorkingCapital, 16=FixedAsset, 32=IntangibleAsset, 64=Investment, 128=Loan, 256=Interest, 512=Dividend, 1024=Tax, 2048=GainLossOnDisposal, 4096=ForeignExchange, 8192=Impairment';
COMMENT ON COLUMN gl_account.working_capital_type IS 'Working Capital Type: 1=Receivables, 2=Inventory, 3=Prepaid, 4=Payables, 5=Accrued, 6=OtherCurrentAsset, 7=OtherCurrentLiability';

-- =============================================
-- STEP 3: Auto-classify existing accounts based on naming conventions
-- This provides initial classification for backward compatibility
-- =============================================

-- Depreciation accounts (ContraAsset with depreciation in name)
UPDATE gl_account
SET account_flags = account_flags | 1  -- Add Depreciation flag
WHERE account_type = 6  -- ContraAsset
  AND (LOWER(account_name_en) LIKE '%depreciation%'
       OR account_name_ar LIKE '%إهلاك%'
       OR account_name_ar LIKE '%استهلاك%')
  AND (account_flags & 1) = 0;  -- Not already set

-- Amortization accounts (ContraAsset with amortization in name)
UPDATE gl_account
SET account_flags = account_flags | 2  -- Add Amortization flag
WHERE account_type = 6  -- ContraAsset
  AND (LOWER(account_name_en) LIKE '%amortization%'
       OR account_name_ar LIKE '%إطفاء%')
  AND (account_flags & 2) = 0;  -- Not already set

-- Provision accounts (Liability with provision in name)
UPDATE gl_account
SET account_flags = account_flags | 4  -- Add Provision flag
WHERE account_type = 2  -- Liability
  AND (LOWER(account_name_en) LIKE '%provision%'
       OR account_name_ar LIKE '%مخصص%')
  AND (account_flags & 4) = 0;  -- Not already set

-- Fixed Asset accounts
UPDATE gl_account
SET account_flags = account_flags | 16,  -- Add FixedAsset flag
    cash_flow_category = 2  -- Investing
WHERE account_type = 0  -- Asset
  AND (LOWER(account_name_en) LIKE '%property%'
       OR LOWER(account_name_en) LIKE '%plant%'
       OR LOWER(account_name_en) LIKE '%equipment%'
       OR LOWER(account_name_en) LIKE '%fixed asset%'
       OR account_name_ar LIKE '%أصول ثابتة%'
       OR account_name_ar LIKE '%معدات%'
       OR account_name_ar LIKE '%آلات%'
       OR account_name_ar LIKE '%مباني%')
  AND (account_flags & 16) = 0;  -- Not already set

-- Intangible Asset accounts
UPDATE gl_account
SET account_flags = account_flags | 32,  -- Add IntangibleAsset flag
    cash_flow_category = 2  -- Investing
WHERE account_type = 0  -- Asset
  AND (LOWER(account_name_en) LIKE '%intangible%'
       OR LOWER(account_name_en) LIKE '%goodwill%'
       OR LOWER(account_name_en) LIKE '%software%'
       OR LOWER(account_name_en) LIKE '%patent%'
       OR account_name_ar LIKE '%أصول غير ملموسة%'
       OR account_name_ar LIKE '%شهرة%'
       OR account_name_ar LIKE '%برمجيات%')
  AND (account_flags & 32) = 0;  -- Not already set

-- Investment accounts
UPDATE gl_account
SET account_flags = account_flags | 64,  -- Add Investment flag
    cash_flow_category = 2  -- Investing
WHERE account_type = 0  -- Asset
  AND (LOWER(account_name_en) LIKE '%investment%'
       OR account_name_ar LIKE '%استثمار%')
  AND (account_flags & 64) = 0;  -- Not already set

-- Receivables (Working Capital)
UPDATE gl_account
SET account_flags = account_flags | 8,  -- Add WorkingCapital flag
    working_capital_type = 1,  -- Receivables
    cash_flow_category = 1  -- Operating
WHERE account_type = 0  -- Asset
  AND is_control_account = TRUE
  AND (LOWER(account_name_en) LIKE '%receivable%'
       OR account_name_ar LIKE '%مدين%'
       OR account_name_ar LIKE '%ذمم%')
  AND (account_flags & 8) = 0;  -- Not already set

-- Inventory (Working Capital)
UPDATE gl_account
SET account_flags = account_flags | 8,  -- Add WorkingCapital flag
    working_capital_type = 2,  -- Inventory
    cash_flow_category = 1  -- Operating
WHERE account_type = 0  -- Asset
  AND (LOWER(account_name_en) LIKE '%inventory%'
       OR account_name_ar LIKE '%مخزون%'
       OR account_name_ar LIKE '%بضاعة%')
  AND (account_flags & 8) = 0;  -- Not already set

-- Prepaid (Working Capital)
UPDATE gl_account
SET account_flags = account_flags | 8,  -- Add WorkingCapital flag
    working_capital_type = 3,  -- Prepaid
    cash_flow_category = 1  -- Operating
WHERE account_type = 0  -- Asset
  AND (LOWER(account_name_en) LIKE '%prepaid%'
       OR account_name_ar LIKE '%مقدم%'
       OR account_name_ar LIKE '%مصروفات مدفوعة%')
  AND (account_flags & 8) = 0;  -- Not already set

-- Payables (Working Capital)
UPDATE gl_account
SET account_flags = account_flags | 8,  -- Add WorkingCapital flag
    working_capital_type = 4,  -- Payables
    cash_flow_category = 1  -- Operating
WHERE account_type = 2  -- Liability
  AND is_control_account = TRUE
  AND (LOWER(account_name_en) LIKE '%payable%'
       OR account_name_ar LIKE '%دائن%'
       OR account_name_ar LIKE '%موردين%')
  AND (account_flags & 8) = 0;  -- Not already set

-- Accrued (Working Capital)
UPDATE gl_account
SET account_flags = account_flags | 8,  -- Add WorkingCapital flag
    working_capital_type = 5,  -- Accrued
    cash_flow_category = 1  -- Operating
WHERE account_type = 2  -- Liability
  AND (LOWER(account_name_en) LIKE '%accrued%'
       OR account_name_ar LIKE '%مستحق%')
  AND (account_flags & 8) = 0;  -- Not already set

-- Loan accounts
UPDATE gl_account
SET account_flags = account_flags | 128,  -- Add Loan flag
    cash_flow_category = 3  -- Financing
WHERE (LOWER(account_name_en) LIKE '%loan%'
       OR LOWER(account_name_en) LIKE '%borrowing%'
       OR LOWER(account_name_en) LIKE '%debt%'
       OR account_name_ar LIKE '%قرض%'
       OR account_name_ar LIKE '%اقتراض%')
  AND (account_flags & 128) = 0;  -- Not already set

-- =============================================
-- STEP 4: Create indexes for performance
-- =============================================

-- Index for cash_flow_category queries
CREATE INDEX IF NOT EXISTS idx_gl_account_cash_flow_category
ON gl_account(cash_flow_category)
WHERE is_active = TRUE;

-- Index for account_flags bitwise queries
CREATE INDEX IF NOT EXISTS idx_gl_account_account_flags
ON gl_account(account_flags)
WHERE is_active = TRUE;

-- Index for working_capital_type queries
CREATE INDEX IF NOT EXISTS idx_gl_account_working_capital_type
ON gl_account(working_capital_type)
WHERE working_capital_type IS NOT NULL;

-- =============================================
-- STEP 5: Verification queries
-- =============================================

-- Show counts by cash_flow_category
SELECT
    CASE cash_flow_category
        WHEN 0 THEN 'None'
        WHEN 1 THEN 'Operating'
        WHEN 2 THEN 'Investing'
        WHEN 3 THEN 'Financing'
    END AS category,
    COUNT(*) AS count
FROM gl_account
WHERE is_active = TRUE
GROUP BY cash_flow_category
ORDER BY cash_flow_category;

-- Show counts by account_flags
SELECT
    CASE
        WHEN (account_flags & 1) != 0 THEN 'Depreciation'
        WHEN (account_flags & 2) != 0 THEN 'Amortization'
        WHEN (account_flags & 4) != 0 THEN 'Provision'
        WHEN (account_flags & 8) != 0 THEN 'WorkingCapital'
        WHEN (account_flags & 16) != 0 THEN 'FixedAsset'
        WHEN (account_flags & 32) != 0 THEN 'IntangibleAsset'
        WHEN (account_flags & 64) != 0 THEN 'Investment'
        WHEN (account_flags & 128) != 0 THEN 'Loan'
        ELSE 'None'
    END AS flag_type,
    COUNT(*) AS count
FROM gl_account
WHERE is_active = TRUE AND account_flags != 0
GROUP BY
    CASE
        WHEN (account_flags & 1) != 0 THEN 'Depreciation'
        WHEN (account_flags & 2) != 0 THEN 'Amortization'
        WHEN (account_flags & 4) != 0 THEN 'Provision'
        WHEN (account_flags & 8) != 0 THEN 'WorkingCapital'
        WHEN (account_flags & 16) != 0 THEN 'FixedAsset'
        WHEN (account_flags & 32) != 0 THEN 'IntangibleAsset'
        WHEN (account_flags & 64) != 0 THEN 'Investment'
        WHEN (account_flags & 128) != 0 THEN 'Loan'
        ELSE 'None'
    END
ORDER BY count DESC;

-- =============================================
-- ROLLBACK SCRIPT (if needed)
-- =============================================
-- To rollback these changes, run:
-- ALTER TABLE gl_account DROP COLUMN IF EXISTS cash_flow_category;
-- ALTER TABLE gl_account DROP COLUMN IF EXISTS account_flags;
-- ALTER TABLE gl_account DROP COLUMN IF EXISTS working_capital_type;
-- DROP INDEX IF EXISTS idx_gl_account_cash_flow_category;
-- DROP INDEX IF EXISTS idx_gl_account_account_flags;
-- DROP INDEX IF EXISTS idx_gl_account_working_capital_type;

-- =============================================
-- Reference: Enum Values
-- =============================================
-- CashFlowCategory:
--   None = 0
--   Operating = 1
--   Investing = 2
--   Financing = 3
--
-- GLAccountFlags (Bitwise):
--   None = 0
--   Depreciation = 1
--   Amortization = 2
--   Provision = 4
--   WorkingCapital = 8
--   FixedAsset = 16
--   IntangibleAsset = 32
--   Investment = 64
--   Loan = 128
--   Interest = 256
--   Dividend = 512
--   Tax = 1024
--   GainLossOnDisposal = 2048
--   ForeignExchange = 4096
--   Impairment = 8192
--
-- WorkingCapitalType:
--   Receivables = 1
--   Inventory = 2
--   Prepaid = 3
--   Payables = 4
--   Accrued = 5
--   OtherCurrentAsset = 6
--   OtherCurrentLiability = 7
--
-- AccountType (for reference):
--   Asset = 0
--   Expense = 1
--   Liability = 2
--   Revenue = 3
--   Equity = 4
--   ContraAsset = 6
--   ContraLiability = 7
--   ContraEquity = 8
--   ContraRevenue = 9
--   OtherComprehensiveIncome = 10
