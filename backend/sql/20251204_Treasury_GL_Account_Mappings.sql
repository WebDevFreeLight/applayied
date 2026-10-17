-- Treasury GL Account Mappings Setup
-- This script adds the missing AccountsReceivable and AccountsPayable mappings
-- Required for PaymentHub to create GL journal entries for customer receipts and vendor payments

-- Insert Accounts Receivable mapping (for customer receipts)
-- Using GL Account 1120 - Accounts Receivable (الذمم المدينة)
INSERT INTO treasury_gl_account_mapping (
    id,
    mapping_type,
    gl_account_id,
    gl_account_code,
    gl_account_name,
    currency,
    description,
    is_active,
    priority,
    effective_from,
    effective_to,
    create_by,
    create_time,
    companyid
)
VALUES (
    gen_random_uuid(),
    'AccountsReceivable',
    'e80cceb9-aed5-4a91-aa7a-a03264f43ca8',  -- GL Account 1120 - Accounts Receivable
    '1120',
    'Accounts Receivable',
    NULL,  -- All currencies
    'Default Accounts Receivable GL account for customer receipts',
    true,
    100,
    '2020-01-01',
    NULL,
    'system',
    NOW(),
    0
)
ON CONFLICT DO NOTHING;

-- Insert Accounts Payable mapping (for vendor payments)
-- Using GL Account 2110 - Accounts Payable (الذمم الدائنة)
INSERT INTO treasury_gl_account_mapping (
    id,
    mapping_type,
    gl_account_id,
    gl_account_code,
    gl_account_name,
    currency,
    description,
    is_active,
    priority,
    effective_from,
    effective_to,
    create_by,
    create_time,
    companyid
)
VALUES (
    gen_random_uuid(),
    'AccountsPayable',
    'b5f19fb3-6102-4695-a391-7a4be1e741b8',  -- GL Account 2110 - Accounts Payable
    '2110',
    'Accounts Payable',
    NULL,  -- All currencies
    'Default Accounts Payable GL account for vendor payments',
    true,
    100,
    '2020-01-01',
    NULL,
    'system',
    NOW(),
    0
)
ON CONFLICT DO NOTHING;

-- Verify the mappings
SELECT * FROM treasury_gl_account_mapping ORDER BY mapping_type;
