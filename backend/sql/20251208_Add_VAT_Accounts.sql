-- =============================================================================
-- Add VAT Input and VAT Output GL Accounts
-- Required for posting AR/AP invoices with tax
-- Run this script if VAT accounts are missing from your database
-- =============================================================================

-- First, find the parent accounts (Current Assets for VAT Input, Current Liabilities for VAT Output)
-- These should exist from the seeder

DO $$
DECLARE
    v_current_assets_id UUID;
    v_current_liabilities_id UUID;
    v_company_id BIGINT := 1; -- Adjust if your company ID is different
    v_created_by VARCHAR(50) := 'system';
BEGIN
    -- Find Current Assets parent (account number starting with "1100")
    SELECT account_id INTO v_current_assets_id
    FROM gl_account
    WHERE account_number LIKE '1100%' AND level = 2
    LIMIT 1;

    -- Find Current Liabilities parent (account number starting with "2100")
    SELECT account_id INTO v_current_liabilities_id
    FROM gl_account
    WHERE account_number LIKE '2100%' AND level = 2
    LIMIT 1;

    -- Check if VAT Input account already exists
    IF NOT EXISTS (
        SELECT 1 FROM gl_account
        WHERE account_name_en LIKE '%VAT Input%'
           OR account_number LIKE '%1150%'
           OR account_number LIKE '%1500%'
    ) THEN
        -- Insert VAT Input account
        INSERT INTO gl_account (
            account_id,
            account_number,
            full_account_number,
            account_name_ar,
            account_name_en,
            account_type,
            normal_balance,
            level,
            parent_account_id,
            is_leaf,
            is_active,
            is_system_account,
            allow_manual_entries,
            allow_posting_to_parent,
            is_cash_account,
            is_bank_account,
            currency,
            is_foreign_currency,
            child_count,
            descendant_count,
            row_version,
            company_id,
            create_by,
            create_time
        ) VALUES (
            gen_random_uuid(),
            '1150000000',
            '1150000000',
            'ضريبة القيمة المضافة - المدخلات',
            'VAT Input',
            0, -- Asset
            0, -- Debit
            3,
            v_current_assets_id,
            TRUE, -- IsLeaf = true for posting
            TRUE, -- IsActive
            TRUE, -- IsSystemAccount
            TRUE, -- AllowManualEntries
            FALSE,
            FALSE,
            FALSE,
            'SAR',
            FALSE,
            0,
            0,
            1,
            v_company_id,
            v_created_by,
            NOW()
        );

        RAISE NOTICE 'VAT Input account created successfully';
    ELSE
        RAISE NOTICE 'VAT Input account already exists';
    END IF;

    -- Check if VAT Output account already exists
    IF NOT EXISTS (
        SELECT 1 FROM gl_account
        WHERE account_name_en LIKE '%VAT Output%'
           OR account_number LIKE '%2140%'
    ) THEN
        -- Insert VAT Output account
        INSERT INTO gl_account (
            account_id,
            account_number,
            full_account_number,
            account_name_ar,
            account_name_en,
            account_type,
            normal_balance,
            level,
            parent_account_id,
            is_leaf,
            is_active,
            is_system_account,
            allow_manual_entries,
            allow_posting_to_parent,
            is_cash_account,
            is_bank_account,
            currency,
            is_foreign_currency,
            child_count,
            descendant_count,
            row_version,
            company_id,
            create_by,
            create_time
        ) VALUES (
            gen_random_uuid(),
            '2140000000',
            '2140000000',
            'ضريبة القيمة المضافة - المخرجات',
            'VAT Output',
            1, -- Liability
            1, -- Credit
            3,
            v_current_liabilities_id,
            TRUE, -- IsLeaf = true for posting
            TRUE, -- IsActive
            TRUE, -- IsSystemAccount
            TRUE, -- AllowManualEntries
            FALSE,
            FALSE,
            FALSE,
            'SAR',
            FALSE,
            0,
            0,
            1,
            v_company_id,
            v_created_by,
            NOW()
        );

        RAISE NOTICE 'VAT Output account created successfully';
    ELSE
        RAISE NOTICE 'VAT Output account already exists';
    END IF;

END $$;

-- Verify the accounts were created
SELECT
    account_id,
    account_number,
    account_name_en,
    account_name_ar,
    is_leaf,
    is_active
FROM gl_account
WHERE account_name_en LIKE '%VAT%'
   OR account_number IN ('1150000000', '2140000000', '1500000000', '2200000000')
ORDER BY account_number;
