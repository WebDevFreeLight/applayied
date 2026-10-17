-- =====================================================
-- Migration: GLAccountFlags Validation & Cleanup
-- Date: 2026-01-14
-- Description: Adds missing account flags to existing accounts
--              based on account names and types.
--              Also validates and fixes CashFlowCategory assignments.
--
-- GLAccountFlags Enum Values (Bitwise):
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
-- CashFlowCategory Enum Values:
--   None = 0
--   Operating = 1
--   Investing = 2
--   Financing = 3
-- =====================================================

-- =====================================================
-- STEP 1: Add Investment flag to investment accounts
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET account_flags = account_flags | 64  -- Investment = 64
    WHERE account_type = 1  -- Asset
      AND is_active = TRUE
      AND (account_flags & 64) = 0
      AND (LOWER(account_name_en) LIKE '%investment%'
           OR account_name_ar LIKE '%استثمار%');

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Added Investment flag to % accounts', updated_count;
END $$;

-- =====================================================
-- STEP 2: Add Loan flag to loan/borrowing accounts
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET account_flags = account_flags | 128  -- Loan = 128
    WHERE account_type = 2  -- Liability
      AND is_active = TRUE
      AND (account_flags & 128) = 0
      AND (LOWER(account_name_en) LIKE '%loan%'
           OR LOWER(account_name_en) LIKE '%borrowing%'
           OR LOWER(account_name_en) LIKE '%debt%'
           OR account_name_ar LIKE '%قرض%'
           OR account_name_ar LIKE '%اقتراض%');

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Added Loan flag to % accounts', updated_count;
END $$;

-- =====================================================
-- STEP 3: Add Impairment flag to impairment accounts
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET account_flags = account_flags | 8192  -- Impairment = 8192
    WHERE is_active = TRUE
      AND (account_flags & 8192) = 0
      AND (LOWER(account_name_en) LIKE '%impairment%'
           OR account_name_ar LIKE '%اضمحلال%'
           OR account_name_ar LIKE '%انخفاض القيمة%');

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Added Impairment flag to % accounts', updated_count;
END $$;

-- =====================================================
-- STEP 4: Add ForeignExchange flag to FX accounts
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET account_flags = account_flags | 4096  -- ForeignExchange = 4096
    WHERE is_active = TRUE
      AND (account_flags & 4096) = 0
      AND (LOWER(account_name_en) LIKE '%foreign exchange%'
           OR LOWER(account_name_en) LIKE '%fx gain%'
           OR LOWER(account_name_en) LIKE '%fx loss%'
           OR LOWER(account_name_en) LIKE '%exchange gain%'
           OR LOWER(account_name_en) LIKE '%exchange loss%'
           OR account_name_ar LIKE '%فروق عملات%'
           OR account_name_ar LIKE '%أرباح صرف%'
           OR account_name_ar LIKE '%خسائر صرف%');

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Added ForeignExchange flag to % accounts', updated_count;
END $$;

-- =====================================================
-- STEP 5: Add Dividend flag to dividend accounts
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET account_flags = account_flags | 512  -- Dividend = 512
    WHERE is_active = TRUE
      AND (account_flags & 512) = 0
      AND (LOWER(account_name_en) LIKE '%dividend%'
           OR account_name_ar LIKE '%أرباح موزعة%'
           OR account_name_ar LIKE '%توزيعات%');

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Added Dividend flag to % accounts', updated_count;
END $$;

-- =====================================================
-- STEP 6: Add Interest flag to interest accounts
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET account_flags = account_flags | 256  -- Interest = 256
    WHERE is_active = TRUE
      AND (account_flags & 256) = 0
      AND (LOWER(account_name_en) LIKE '%interest income%'
           OR LOWER(account_name_en) LIKE '%interest expense%'
           OR LOWER(account_name_en) LIKE '%interest revenue%'
           OR account_name_ar LIKE '%إيراد فوائد%'
           OR account_name_ar LIKE '%مصروف فوائد%');

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Added Interest flag to % accounts', updated_count;
END $$;

-- =====================================================
-- STEP 7: Add Tax flag to tax accounts
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET account_flags = account_flags | 1024  -- Tax = 1024
    WHERE is_active = TRUE
      AND (account_flags & 1024) = 0
      AND (LOWER(account_name_en) LIKE '%income tax%'
           OR LOWER(account_name_en) LIKE '%tax expense%'
           OR LOWER(account_name_en) LIKE '%tax payable%'
           OR account_name_ar LIKE '%ضريبة الدخل%'
           OR account_name_ar LIKE '%مصروف الضريبة%');

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RAISE NOTICE 'Added Tax flag to % accounts', updated_count;
END $$;

-- =====================================================
-- STEP 8: Fix IntangibleAsset category to Investing
-- Accounts with IntangibleAsset flag should be in Investing category
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET cash_flow_category = 2  -- Investing
    WHERE (account_flags & 32) != 0  -- IntangibleAsset
      AND cash_flow_category NOT IN (0, 2);

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    IF updated_count > 0 THEN
        RAISE NOTICE 'Fixed CashFlowCategory for % IntangibleAsset accounts', updated_count;
    END IF;
END $$;

-- =====================================================
-- STEP 9: Fix Investment category to Investing
-- Accounts with Investment flag should be in Investing category
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET cash_flow_category = 2  -- Investing
    WHERE (account_flags & 64) != 0  -- Investment
      AND cash_flow_category NOT IN (0, 2);

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    IF updated_count > 0 THEN
        RAISE NOTICE 'Fixed CashFlowCategory for % Investment accounts', updated_count;
    END IF;
END $$;

-- =====================================================
-- STEP 10: Fix FixedAsset category to Investing
-- Accounts with FixedAsset flag should be in Investing category
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET cash_flow_category = 2  -- Investing
    WHERE (account_flags & 16) != 0  -- FixedAsset
      AND cash_flow_category NOT IN (0, 2);

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    IF updated_count > 0 THEN
        RAISE NOTICE 'Fixed CashFlowCategory for % FixedAsset accounts', updated_count;
    END IF;
END $$;

-- =====================================================
-- STEP 11: Fix Loan category to Financing
-- Accounts with Loan flag should be in Financing category
-- =====================================================
DO $$
DECLARE
    updated_count INT;
BEGIN
    UPDATE gl_account
    SET cash_flow_category = 3  -- Financing
    WHERE (account_flags & 128) != 0  -- Loan
      AND cash_flow_category NOT IN (0, 3);

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    IF updated_count > 0 THEN
        RAISE NOTICE 'Fixed CashFlowCategory for % Loan accounts', updated_count;
    END IF;
END $$;

-- =====================================================
-- VERIFICATION QUERIES
-- =====================================================

-- Summary of accounts with each flag
SELECT
    'Depreciation (1)' as flag_name, COUNT(*) as account_count
FROM gl_account WHERE (account_flags & 1) != 0
UNION ALL
SELECT 'Amortization (2)', COUNT(*) FROM gl_account WHERE (account_flags & 2) != 0
UNION ALL
SELECT 'Provision (4)', COUNT(*) FROM gl_account WHERE (account_flags & 4) != 0
UNION ALL
SELECT 'WorkingCapital (8)', COUNT(*) FROM gl_account WHERE (account_flags & 8) != 0
UNION ALL
SELECT 'FixedAsset (16)', COUNT(*) FROM gl_account WHERE (account_flags & 16) != 0
UNION ALL
SELECT 'IntangibleAsset (32)', COUNT(*) FROM gl_account WHERE (account_flags & 32) != 0
UNION ALL
SELECT 'Investment (64)', COUNT(*) FROM gl_account WHERE (account_flags & 64) != 0
UNION ALL
SELECT 'Loan (128)', COUNT(*) FROM gl_account WHERE (account_flags & 128) != 0
UNION ALL
SELECT 'Interest (256)', COUNT(*) FROM gl_account WHERE (account_flags & 256) != 0
UNION ALL
SELECT 'Dividend (512)', COUNT(*) FROM gl_account WHERE (account_flags & 512) != 0
UNION ALL
SELECT 'Tax (1024)', COUNT(*) FROM gl_account WHERE (account_flags & 1024) != 0
UNION ALL
SELECT 'GainLossOnDisposal (2048)', COUNT(*) FROM gl_account WHERE (account_flags & 2048) != 0
UNION ALL
SELECT 'ForeignExchange (4096)', COUNT(*) FROM gl_account WHERE (account_flags & 4096) != 0
UNION ALL
SELECT 'Impairment (8192)', COUNT(*) FROM gl_account WHERE (account_flags & 8192) != 0
ORDER BY flag_name;

-- Check for invalid flag combinations (WorkingCapital with incompatible flags)
SELECT
    account_id,
    account_number,
    account_name_en,
    account_flags,
    'WorkingCapital with FixedAsset' as issue
FROM gl_account
WHERE (account_flags & 8) != 0  -- WorkingCapital
  AND (account_flags & 16) != 0  -- FixedAsset
UNION ALL
SELECT
    account_id,
    account_number,
    account_name_en,
    account_flags,
    'WorkingCapital with IntangibleAsset'
FROM gl_account
WHERE (account_flags & 8) != 0  -- WorkingCapital
  AND (account_flags & 32) != 0  -- IntangibleAsset
UNION ALL
SELECT
    account_id,
    account_number,
    account_name_en,
    account_flags,
    'WorkingCapital with Investment'
FROM gl_account
WHERE (account_flags & 8) != 0  -- WorkingCapital
  AND (account_flags & 64) != 0  -- Investment
UNION ALL
SELECT
    account_id,
    account_number,
    account_name_en,
    account_flags,
    'Multiple BalanceSheet Classifications'
FROM gl_account
WHERE (
    CASE WHEN (account_flags & 16) != 0 THEN 1 ELSE 0 END +
    CASE WHEN (account_flags & 32) != 0 THEN 1 ELSE 0 END +
    CASE WHEN (account_flags & 64) != 0 THEN 1 ELSE 0 END
) > 1;

-- CashFlowCategory distribution
SELECT
    cash_flow_category,
    CASE cash_flow_category
        WHEN 0 THEN 'None'
        WHEN 1 THEN 'Operating'
        WHEN 2 THEN 'Investing'
        WHEN 3 THEN 'Financing'
        ELSE 'Unknown'
    END as category_name,
    COUNT(*) as account_count
FROM gl_account
WHERE is_active = TRUE
GROUP BY cash_flow_category
ORDER BY cash_flow_category;

-- =====================================================
-- ROLLBACK SCRIPT (if needed)
-- =====================================================
-- To rollback, you would need to track which accounts were
-- modified and restore their original flag values.
-- This is not typically needed as these changes are additive.
--
-- Example to remove specific flags that were added:
-- UPDATE gl_account
-- SET account_flags = account_flags & ~64  -- Remove Investment flag
-- WHERE (account_flags & 64) != 0
--   AND LOWER(account_name_en) LIKE '%investment%';
