-- =============================================================================
-- Create Tax Configuration Table
-- Stores VAT Input/Output and AR/AP account mappings per company
-- =============================================================================

-- Create the tax_configuration table
CREATE TABLE IF NOT EXISTS tax_configuration (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id BIGINT,

    -- VAT Account Mappings
    vat_input_account_id UUID,
    vat_output_account_id UUID,

    -- Default AR/AP Account Mappings
    default_ar_account_id UUID,
    default_ap_account_id UUID,

    -- Configuration Status
    is_configured BOOLEAN DEFAULT FALSE,

    -- Audit Fields (from SysBase)
    create_by VARCHAR(64),
    create_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    update_by VARCHAR(64),
    update_time TIMESTAMP,
    remark VARCHAR(500),

    -- Foreign Key Constraints
    CONSTRAINT fk_tax_config_vat_input FOREIGN KEY (vat_input_account_id)
        REFERENCES gl_account(account_id) ON DELETE SET NULL,
    CONSTRAINT fk_tax_config_vat_output FOREIGN KEY (vat_output_account_id)
        REFERENCES gl_account(account_id) ON DELETE SET NULL,
    CONSTRAINT fk_tax_config_ar FOREIGN KEY (default_ar_account_id)
        REFERENCES gl_account(account_id) ON DELETE SET NULL,
    CONSTRAINT fk_tax_config_ap FOREIGN KEY (default_ap_account_id)
        REFERENCES gl_account(account_id) ON DELETE SET NULL
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_tax_config_company ON tax_configuration(company_id);

-- Add unique constraint to ensure one config per company
CREATE UNIQUE INDEX IF NOT EXISTS idx_tax_config_company_unique
    ON tax_configuration(COALESCE(company_id, -1));

-- Add comments to describe the table and columns
COMMENT ON TABLE tax_configuration IS 'Tax Configuration - إعدادات الضرائب - Stores VAT and AR/AP account mappings';
COMMENT ON COLUMN tax_configuration.vat_input_account_id IS 'VAT Input Account - حساب ضريبة المدخلات - GL Account to debit for VAT on AP invoices';
COMMENT ON COLUMN tax_configuration.vat_output_account_id IS 'VAT Output Account - حساب ضريبة المخرجات - GL Account to credit for VAT on AR invoices';
COMMENT ON COLUMN tax_configuration.default_ar_account_id IS 'Default AR Account - حساب الذمم المدينة الافتراضي - Default GL Account for Accounts Receivable';
COMMENT ON COLUMN tax_configuration.default_ap_account_id IS 'Default AP Account - حساب الذمم الدائنة الافتراضي - Default GL Account for Accounts Payable';
COMMENT ON COLUMN tax_configuration.is_configured IS 'Configuration Status - حالة التكوين - True if VAT accounts have been configured';

-- Insert default empty configuration for company_id = 1
INSERT INTO tax_configuration (id, company_id, is_configured, create_by, create_time)
SELECT gen_random_uuid(), 1, FALSE, 'system', NOW()
WHERE NOT EXISTS (SELECT 1 FROM tax_configuration WHERE company_id = 1);

-- Verify the table was created
SELECT
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'tax_configuration'
ORDER BY ordinal_position;
