-- =============================================================================
-- Cleanup Duplicate Tax Configuration Records
-- =============================================================================

-- Step 1: View all tax_configuration records with companyid=0 or NULL
SELECT id, company_id, default_ar_account_id, default_ap_account_id, is_configured, create_time
FROM tax_configuration
WHERE company_id = 0 OR company_id IS NULL
ORDER BY create_time;

-- Step 2: Delete duplicate records, keeping only the FIRST one (9b2b185a)
DELETE FROM tax_configuration
WHERE id IN (
    '59ca022e-3c44-4df5-bfbe-56f7b2e1c1bf',
    '80a4292d-8f76-4f67-99f3-a88f10b859f6',
    '5ded4e50-2e7c-4a6a-a85c-fc2bb4c83a7d',
    '163e0064-b2b6-4f74-9d96-6b27c85ce6b7'
);

-- Step 3: Get one of the deleted records that has AR/AP values (for reference)
-- From your database dump, record 163e0064 has:
-- default_ar_account_id: 13333333-3333-3333-3333-333333333332
-- default_ap_account_id: 23333333-3333-3333-3333-333333333332

-- Step 4: Update the remaining record with the correct AR/AP values
-- Replace the GUIDs below with your actual account IDs
UPDATE tax_configuration
SET
    default_ar_account_id = '13333333-3333-3333-3333-333333333332',  -- Replace with your AR account
    default_ap_account_id = '23333333-3333-3333-3333-333333333332',  -- Replace with your AP account
    vat_input_account_id = vat_input_account_id,  -- Keep existing value
    vat_output_account_id = vat_output_account_id,  -- Keep existing value
    is_configured = true,
    update_time = NOW()
WHERE id = '9b2b185a-ed9f-410a-8c6e-9ad04732b9dc';

-- Step 5: Verify the result
SELECT id, company_id, default_ar_account_id, default_ap_account_id, is_configured
FROM tax_configuration
WHERE company_id = 0 OR company_id IS NULL;

-- Expected result: Only 1 record with id = 9b2b185a-ed9f-410a-8c6e-9ad04732b9dc
