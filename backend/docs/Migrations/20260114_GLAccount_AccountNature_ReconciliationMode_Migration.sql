-- ============================================================================
-- GLAccount AccountNature and ReconciliationMode Migration Script
-- Date: 2026-01-14
-- Description: Replaces boolean fields with enums for account nature and reconciliation mode
--
-- Changes:
--   1. Add new column: account_nature (replaces is_cash_account and is_bank_account)
--   2. Add new column: reconciliation_mode (replaces is_reconciliation_account)
--   3. Update reconciliation_status column type from int to enum (already compatible)
--   4. Migrate existing data to new columns
--   5. Drop old columns
--   6. Add new indexes
--
-- Enum Values:
--   AccountNature: None=0, Cash=1, Bank=2
--   ReconciliationMode: None=0, Required=1
--   ReconciliationStatus: NotReconciled=0, Reconciled=1, PendingReview=2
-- ============================================================================

-- ============================================================================
-- STEP 1: Add new columns (PostgreSQL)
-- ============================================================================

-- Add account_nature column
ALTER TABLE gl_account
ADD COLUMN IF NOT EXISTS account_nature INTEGER NOT NULL DEFAULT 0;

-- Add reconciliation_mode column
ALTER TABLE gl_account
ADD COLUMN IF NOT EXISTS reconciliation_mode INTEGER NOT NULL DEFAULT 0;

-- Add comment for clarity
COMMENT ON COLUMN gl_account.account_nature IS 'Account Nature enum: 0=None, 1=Cash, 2=Bank';
COMMENT ON COLUMN gl_account.reconciliation_mode IS 'Reconciliation Mode enum: 0=None, 1=Required';
COMMENT ON COLUMN gl_account.reconciliation_status IS 'Reconciliation Status enum: 0=NotReconciled, 1=Reconciled, 2=PendingReview';

-- ============================================================================
-- STEP 2: Migrate data from old boolean columns to new enum columns
-- ============================================================================

-- Set account_nature based on is_cash_account and is_bank_account
-- Cash accounts get AccountNature.Cash (1)
-- Bank accounts get AccountNature.Bank (2)
-- Neither gets AccountNature.None (0)
UPDATE gl_account
SET account_nature = CASE
    WHEN is_cash_account = true THEN 1  -- Cash
    WHEN is_bank_account = true THEN 2  -- Bank
    ELSE 0  -- None
END
WHERE is_cash_account IS NOT NULL OR is_bank_account IS NOT NULL;

-- Set reconciliation_mode based on is_reconciliation_account
-- Required gets ReconciliationMode.Required (1)
-- Not required gets ReconciliationMode.None (0)
UPDATE gl_account
SET reconciliation_mode = CASE
    WHEN is_reconciliation_account = true THEN 1  -- Required
    ELSE 0  -- None
END
WHERE is_reconciliation_account IS NOT NULL;

-- ============================================================================
-- STEP 3: Drop old columns (OPTIONAL - can be done in a later migration)
-- Uncomment these lines when ready to permanently remove old columns
-- ============================================================================

-- Drop old boolean columns
-- ALTER TABLE gl_account DROP COLUMN IF EXISTS is_cash_account;
-- ALTER TABLE gl_account DROP COLUMN IF EXISTS is_bank_account;
-- ALTER TABLE gl_account DROP COLUMN IF EXISTS is_reconciliation_account;

-- ============================================================================
-- STEP 4: Create new indexes for performance
-- ============================================================================

-- Index for account nature filtering
CREATE INDEX IF NOT EXISTS ix_gl_account_nature
ON gl_account (account_nature)
WHERE is_active = true;

-- Index for reconciliation mode filtering
CREATE INDEX IF NOT EXISTS ix_gl_account_reconciliation_mode
ON gl_account (reconciliation_mode)
WHERE is_active = true;

-- Composite index for cash/bank account queries (common pattern)
CREATE INDEX IF NOT EXISTS ix_gl_account_nature_active
ON gl_account (company_id, account_nature, is_active);

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- Verify migration counts
DO $$
DECLARE
    cash_count_old INTEGER;
    cash_count_new INTEGER;
    bank_count_old INTEGER;
    bank_count_new INTEGER;
    recon_count_old INTEGER;
    recon_count_new INTEGER;
BEGIN
    -- Count old cash accounts
    SELECT COUNT(*) INTO cash_count_old FROM gl_account WHERE is_cash_account = true;
    SELECT COUNT(*) INTO cash_count_new FROM gl_account WHERE account_nature = 1;

    -- Count old bank accounts
    SELECT COUNT(*) INTO bank_count_old FROM gl_account WHERE is_bank_account = true;
    SELECT COUNT(*) INTO bank_count_new FROM gl_account WHERE account_nature = 2;

    -- Count old reconciliation accounts
    SELECT COUNT(*) INTO recon_count_old FROM gl_account WHERE is_reconciliation_account = true;
    SELECT COUNT(*) INTO recon_count_new FROM gl_account WHERE reconciliation_mode = 1;

    RAISE NOTICE 'Migration Verification:';
    RAISE NOTICE 'Cash accounts: Old=%, New=%', cash_count_old, cash_count_new;
    RAISE NOTICE 'Bank accounts: Old=%, New=%', bank_count_old, bank_count_new;
    RAISE NOTICE 'Reconciliation accounts: Old=%, New=%', recon_count_old, recon_count_new;

    -- Verify counts match
    IF cash_count_old = cash_count_new AND bank_count_old = bank_count_new AND recon_count_old = recon_count_new THEN
        RAISE NOTICE 'SUCCESS: All counts match - migration successful!';
    ELSE
        RAISE EXCEPTION 'FAILURE: Counts do not match - please review migration!';
    END IF;
END $$;

-- ============================================================================
-- ROLLBACK SCRIPT (if needed)
-- ============================================================================

-- To rollback this migration, run the following:
/*
-- Recreate old columns
ALTER TABLE gl_account ADD COLUMN IF NOT EXISTS is_cash_account BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE gl_account ADD COLUMN IF NOT EXISTS is_bank_account BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE gl_account ADD COLUMN IF NOT EXISTS is_reconciliation_account BOOLEAN NOT NULL DEFAULT false;

-- Restore data
UPDATE gl_account SET is_cash_account = (account_nature = 1);
UPDATE gl_account SET is_bank_account = (account_nature = 2);
UPDATE gl_account SET is_reconciliation_account = (reconciliation_mode = 1);

-- Drop new columns
ALTER TABLE gl_account DROP COLUMN IF EXISTS account_nature;
ALTER TABLE gl_account DROP COLUMN IF EXISTS reconciliation_mode;

-- Drop new indexes
DROP INDEX IF EXISTS ix_gl_account_nature;
DROP INDEX IF EXISTS ix_gl_account_reconciliation_mode;
DROP INDEX IF EXISTS ix_gl_account_nature_active;
*/
