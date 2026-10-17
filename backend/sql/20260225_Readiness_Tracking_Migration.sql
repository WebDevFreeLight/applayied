-- ============================================================
-- Readiness Tracking Migration
-- Transforms inspection-based system to continuous live tracking
-- Date: 2026-02-25
-- Safe to re-run: uses IF EXISTS / IF NOT EXISTS
-- ============================================================

-- 1. Rename statuses in readiness items: Passed→Completed, Failed→Rejected
-- (Only runs if table exists - it's created at runtime when stores are initialized)
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'ops_store_readiness_item') THEN
    UPDATE ops_store_readiness_item SET status = 'Completed' WHERE status = 'Passed';
    UPDATE ops_store_readiness_item SET status = 'Rejected' WHERE status = 'Failed';
    RAISE NOTICE 'Readiness item statuses migrated.';
  ELSE
    RAISE NOTICE 'ops_store_readiness_item does not exist yet (created at runtime). Skipping status rename.';
  END IF;
END $$;

-- 2. Score history: decouple from inspections
-- Add readiness_id column (skip if already exists)
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ops_store_score_history' AND column_name = 'readiness_id') THEN
    ALTER TABLE ops_store_score_history ADD COLUMN readiness_id BIGINT NULL;
    RAISE NOTICE 'Added readiness_id column.';
  ELSE
    RAISE NOTICE 'readiness_id column already exists. Skipping.';
  END IF;
END $$;

-- Add snapshot_type column (skip if already exists)
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ops_store_score_history' AND column_name = 'snapshot_type') THEN
    ALTER TABLE ops_store_score_history ADD COLUMN snapshot_type VARCHAR(30) DEFAULT 'inspection';
    RAISE NOTICE 'Added snapshot_type column.';
  ELSE
    RAISE NOTICE 'snapshot_type column already exists. Skipping.';
  END IF;
END $$;

-- Rename inspection_date → recorded_at (skip if already renamed)
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'ops_store_score_history' AND column_name = 'inspection_date') THEN
    ALTER TABLE ops_store_score_history RENAME COLUMN inspection_date TO recorded_at;
    RAISE NOTICE 'Renamed inspection_date to recorded_at.';
  ELSE
    RAISE NOTICE 'inspection_date already renamed (or does not exist). Skipping.';
  END IF;
END $$;

-- Make inspection_id nullable (safe to run multiple times)
ALTER TABLE ops_store_score_history ALTER COLUMN inspection_id DROP NOT NULL;

-- Index for efficient readiness-based queries (skip if exists)
CREATE INDEX IF NOT EXISTS ix_score_history_readiness ON ops_store_score_history(readiness_id);

-- ============================================================
-- Verification queries (run after migration):
-- ============================================================
-- Should return 0 (if table exists):
-- SELECT COUNT(*) FROM ops_store_readiness_item WHERE status IN ('Passed', 'Failed');
--
-- Should show new columns:
-- SELECT column_name FROM information_schema.columns WHERE table_name = 'ops_store_score_history' AND column_name IN ('readiness_id', 'snapshot_type', 'recorded_at');
