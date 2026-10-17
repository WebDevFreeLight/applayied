-- =============================================
-- Add FK: hr_payroll.treasurytransactionid -> erp_treasury_transactions.(transactionid|transaction_id)
-- Date: 2025-12-18
-- Safe to run after Treasury tables are deployed.
-- =============================================

DO $$
BEGIN
    IF to_regclass('public.erp_treasury_transactions') IS NULL THEN
        RAISE NOTICE 'Skipping fk_hr_payroll_treasury_tx: table erp_treasury_transactions not found';
        RETURN;
    END IF;

    ALTER TABLE hr_payroll
        DROP CONSTRAINT IF EXISTS fk_hr_payroll_treasury_tx;

    -- Prefer transactionid if present
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'erp_treasury_transactions'
          AND column_name = 'transactionid'
    ) THEN
        EXECUTE 'ALTER TABLE hr_payroll ADD CONSTRAINT fk_hr_payroll_treasury_tx
                 FOREIGN KEY (treasurytransactionid)
                 REFERENCES erp_treasury_transactions(transactionid)
                 ON DELETE SET NULL
                 ON UPDATE CASCADE';
        RETURN;
    END IF;

    -- Fallback to transaction_id if present
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'erp_treasury_transactions'
          AND column_name = 'transaction_id'
    ) THEN
        EXECUTE 'ALTER TABLE hr_payroll ADD CONSTRAINT fk_hr_payroll_treasury_tx
                 FOREIGN KEY (treasurytransactionid)
                 REFERENCES erp_treasury_transactions(transaction_id)
                 ON DELETE SET NULL
                 ON UPDATE CASCADE';
        RETURN;
    END IF;

    RAISE EXCEPTION 'Cannot create fk_hr_payroll_treasury_tx: PK column not found (expected transactionid or transaction_id)';
END $$;

-- Verify (will return 1 row when the FK exists)
SELECT
    tc.constraint_name,
    kcu.table_name,
    kcu.column_name,
    ccu.table_name AS referenced_table,
    ccu.column_name AS referenced_column
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu
  ON tc.constraint_name = kcu.constraint_name
  AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage ccu
  ON ccu.constraint_name = tc.constraint_name
  AND ccu.table_schema = tc.table_schema
WHERE tc.table_schema = 'public'
  AND tc.table_name = 'hr_payroll'
  AND tc.constraint_type = 'FOREIGN KEY'
  AND tc.constraint_name = 'fk_hr_payroll_treasury_tx';


