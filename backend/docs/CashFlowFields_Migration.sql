-- ==============================================================================
-- Cash Flow Classification Fields - Migration Script
-- ==============================================================================
-- Purpose: Update existing GL accounts with proper Cash Flow Classification
-- Date: January 2026
-- IAS 7 Compliant Cash Flow Statement Support
-- ==============================================================================

-- Note: This script updates existing accounts based on account name patterns.
-- It should be run ONCE after deploying the seeder updates.
-- Always backup data before running migrations!

-- ==============================================================================
-- STEP 1: Update Receivables (Working Capital)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 1, -- Operating
    account_flags = COALESCE(account_flags, 0) | 8, -- WorkingCapital flag
    working_capital_type = 1 -- Receivables
WHERE
    (account_name_en LIKE '%receivable%' OR
     account_name_en LIKE '%debtor%' OR
     account_name_ar LIKE '%ذمم مدينة%' OR
     account_name_ar LIKE '%مدين%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 2: Update Inventory (Working Capital)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 1, -- Operating
    account_flags = COALESCE(account_flags, 0) | 8, -- WorkingCapital flag
    working_capital_type = 2 -- Inventory
WHERE
    (account_name_en LIKE '%inventory%' OR
     account_name_en LIKE '%stock%' OR
     account_name_ar LIKE '%مخزون%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 3: Update Prepaid Expenses (Working Capital)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 1, -- Operating
    account_flags = COALESCE(account_flags, 0) | 8, -- WorkingCapital flag
    working_capital_type = 3 -- Prepaid
WHERE
    (account_name_en LIKE '%prepaid%' OR
     account_name_en LIKE '%advance payment%' OR
     account_name_ar LIKE '%مدفوع مقدم%' OR
     account_name_ar LIKE '%مقدم%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 4: Update Payables (Working Capital)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 1, -- Operating
    account_flags = COALESCE(account_flags, 0) | 8, -- WorkingCapital flag
    working_capital_type = 4 -- Payables
WHERE
    (account_name_en LIKE '%payable%' OR
     account_name_en LIKE '%creditor%' OR
     account_name_ar LIKE '%ذمم دائنة%' OR
     account_name_ar LIKE '%دائن%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 5: Update Accrued Expenses (Working Capital)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 1, -- Operating
    account_flags = COALESCE(account_flags, 0) | 8, -- WorkingCapital flag
    working_capital_type = 5 -- Accrued
WHERE
    (account_name_en LIKE '%accrued%' OR
     account_name_en LIKE '%accrual%' OR
     account_name_ar LIKE '%مستحق%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 6: Update Fixed Assets (Investing)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 2, -- Investing
    account_flags = COALESCE(account_flags, 0) | 16 -- FixedAsset flag
WHERE
    (account_name_en LIKE '%property%' OR
     account_name_en LIKE '%plant%' OR
     account_name_en LIKE '%equipment%' OR
     account_name_en LIKE '%fixed asset%' OR
     account_name_en LIKE '%building%' OR
     account_name_en LIKE '%vehicle%' OR
     account_name_ar LIKE '%ممتلكات%' OR
     account_name_ar LIKE '%معدات%' OR
     account_name_ar LIKE '%أصول ثابتة%' OR
     account_name_ar LIKE '%مبنى%')
    AND account_name_en NOT LIKE '%depreciation%'
    AND account_name_en NOT LIKE '%accumulated%'
    AND account_name_ar NOT LIKE '%إهلاك%'
    AND account_name_ar NOT LIKE '%مجمع%'
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 7: Update Accumulated Depreciation (Non-cash adjustment)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 1, -- Operating (non-cash adjustment)
    account_flags = COALESCE(account_flags, 0) | 1 -- Depreciation flag
WHERE
    (account_name_en LIKE '%depreciation%' OR
     account_name_en LIKE '%accumulated%' OR
     account_name_ar LIKE '%إهلاك%' OR
     account_name_ar LIKE '%مجمع%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 8: Update Intangible Assets (Investing)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 2, -- Investing
    account_flags = COALESCE(account_flags, 0) | 32 -- IntangibleAsset flag
WHERE
    (account_name_en LIKE '%intangible%' OR
     account_name_en LIKE '%goodwill%' OR
     account_name_en LIKE '%patent%' OR
     account_name_en LIKE '%trademark%' OR
     account_name_ar LIKE '%غير ملموس%' OR
     account_name_ar LIKE '%شهرة%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 9: Update Investments (Investing)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 2, -- Investing
    account_flags = COALESCE(account_flags, 0) | 64 -- Investment flag
WHERE
    (account_name_en LIKE '%investment%' OR
     account_name_ar LIKE '%استثمار%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 10: Update Loans (Financing)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 3, -- Financing
    account_flags = COALESCE(account_flags, 0) | 128 -- Loan flag
WHERE
    (account_name_en LIKE '%loan%' OR
     account_name_en LIKE '%borrowing%' OR
     account_name_ar LIKE '%قرض%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 11: Update Equity Accounts (Financing)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 3 -- Financing
WHERE
    account_type = 2 -- Equity
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 12: Update Tax Accounts (Tax flag)
-- ==============================================================================
UPDATE gl_account SET
    account_flags = COALESCE(account_flags, 0) | 1024 -- Tax flag
WHERE
    (account_name_en LIKE '%tax%' OR
     account_name_en LIKE '%vat%' OR
     account_name_ar LIKE '%ضريب%')
    AND (account_flags IS NULL OR account_flags & 1024 = 0)
    AND is_active = 1;

-- ==============================================================================
-- STEP 13: Update Provision Accounts (Provision flag)
-- ==============================================================================
UPDATE gl_account SET
    account_flags = COALESCE(account_flags, 0) | 4 -- Provision flag
WHERE
    (account_name_en LIKE '%provision%' OR
     account_name_en LIKE '%allowance%' OR
     account_name_ar LIKE '%مخصص%')
    AND (account_flags IS NULL OR account_flags & 4 = 0)
    AND is_active = 1;

-- ==============================================================================
-- STEP 14: Update Foreign Exchange Accounts (FX flag)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 1, -- Operating (non-cash adjustment)
    account_flags = COALESCE(account_flags, 0) | 4096 -- ForeignExchange flag
WHERE
    (account_name_en LIKE '%foreign exchange%' OR
     account_name_en LIKE '%fx gain%' OR
     account_name_en LIKE '%fx loss%' OR
     account_name_ar LIKE '%صرف العمل%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- STEP 15: Update Gain/Loss on Disposal (Non-cash adjustment)
-- ==============================================================================
UPDATE gl_account SET
    cash_flow_category = 1, -- Operating (non-cash adjustment)
    account_flags = COALESCE(account_flags, 0) | 2048 -- GainLossOnDisposal flag
WHERE
    (account_name_en LIKE '%gain%loss%' OR
     account_name_en LIKE '%disposal%' OR
     account_name_ar LIKE '%ربح%خسارة%' OR
     account_name_ar LIKE '%تخلص%')
    AND cash_flow_category = 0
    AND is_active = 1;

-- ==============================================================================
-- VERIFICATION QUERIES
-- ==============================================================================

-- Check accounts without Cash Flow classification
SELECT
    account_number,
    account_name_en,
    account_type,
    cash_flow_category,
    account_flags,
    working_capital_type
FROM gl_account
WHERE
    cash_flow_category = 0
    AND account_flags IS NULL OR account_flags = 0
    AND is_active = 1
    AND is_leaf = 1
ORDER BY account_number;

-- Summary of Cash Flow classifications
SELECT
    CASE cash_flow_category
        WHEN 0 THEN 'None'
        WHEN 1 THEN 'Operating'
        WHEN 2 THEN 'Investing'
        WHEN 3 THEN 'Financing'
    END AS category,
    COUNT(*) AS account_count
FROM gl_account
WHERE is_active = 1
GROUP BY cash_flow_category
ORDER BY cash_flow_category;

-- Summary of Working Capital types
SELECT
    CASE working_capital_type
        WHEN 1 THEN 'Receivables'
        WHEN 2 THEN 'Inventory'
        WHEN 3 THEN 'Prepaid'
        WHEN 4 THEN 'Payables'
        WHEN 5 THEN 'Accrued'
        WHEN 6 THEN 'OtherCurrentAsset'
        WHEN 7 THEN 'OtherCurrentLiability'
        ELSE 'Not Set'
    END AS wc_type,
    COUNT(*) AS account_count
FROM gl_account
WHERE is_active = 1 AND account_flags & 8 = 8
GROUP BY working_capital_type
ORDER BY working_capital_type;

-- ==============================================================================
-- END OF MIGRATION SCRIPT
-- ==============================================================================
