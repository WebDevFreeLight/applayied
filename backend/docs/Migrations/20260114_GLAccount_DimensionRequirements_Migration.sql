-- =====================================================
-- Migration: GLAccount DimensionRequirements Consolidation
-- Date: 2026-01-14
-- Description: Consolidates RequiresCostCenter, RequiresDepartment,
--              RequiresProject boolean fields into a single
--              DimensionRequirements enum flags field
--
-- DimensionRequirements Enum Values:
--   None = 0
--   CostCenter = 1
--   Department = 2
--   Project = 4
-- (Flags can be combined using bitwise OR)
-- =====================================================

-- Step 1: Add the new dimension_requirements column
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'gl_account' AND column_name = 'dimension_requirements'
    ) THEN
        ALTER TABLE gl_account
        ADD COLUMN dimension_requirements INTEGER NOT NULL DEFAULT 0;

        RAISE NOTICE 'Column dimension_requirements added successfully';
    ELSE
        RAISE NOTICE 'Column dimension_requirements already exists';
    END IF;
END $$;

-- Step 2: Migrate data from boolean fields to the new flags field
-- Convert existing boolean values to flags:
--   RequiresCostCenter = true  -> dimension_requirements |= 1 (CostCenter)
--   RequiresDepartment = true  -> dimension_requirements |= 2 (Department)
--   RequiresProject = true     -> dimension_requirements |= 4 (Project)
UPDATE gl_account
SET dimension_requirements =
    COALESCE(
        (CASE WHEN requires_cost_center = true THEN 1 ELSE 0 END) |
        (CASE WHEN requires_department = true THEN 2 ELSE 0 END) |
        (CASE WHEN requires_project = true THEN 4 ELSE 0 END),
        0
    )
WHERE dimension_requirements = 0;

-- Step 3: Create index on the new column for efficient querying
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE tablename = 'gl_account' AND indexname = 'ix_gl_account_dimension_requirements'
    ) THEN
        CREATE INDEX ix_gl_account_dimension_requirements
        ON gl_account(dimension_requirements);

        RAISE NOTICE 'Index ix_gl_account_dimension_requirements created successfully';
    ELSE
        RAISE NOTICE 'Index ix_gl_account_dimension_requirements already exists';
    END IF;
END $$;

-- Step 4: Add comments for documentation
COMMENT ON COLUMN gl_account.dimension_requirements IS
    'Dimension requirements flags: None=0, CostCenter=1, Department=2, Project=4 (bitwise combinable)';

-- =====================================================
-- Verification queries
-- =====================================================

-- Verify migration: Count accounts with each dimension requirement
SELECT
    'Total accounts' as description,
    COUNT(*) as count
FROM gl_account
UNION ALL
SELECT
    'Requires CostCenter (flag 1)',
    COUNT(*)
FROM gl_account
WHERE (dimension_requirements & 1) = 1
UNION ALL
SELECT
    'Requires Department (flag 2)',
    COUNT(*)
FROM gl_account
WHERE (dimension_requirements & 2) = 2
UNION ALL
SELECT
    'Requires Project (flag 4)',
    COUNT(*)
FROM gl_account
WHERE (dimension_requirements & 4) = 4
UNION ALL
SELECT
    'No requirements (flag 0)',
    COUNT(*)
FROM gl_account
WHERE dimension_requirements = 0;

-- Verify data integrity: Compare old boolean fields with new flags
SELECT
    account_id,
    account_number,
    requires_cost_center,
    requires_department,
    requires_project,
    dimension_requirements,
    CASE
        WHEN (
            ((dimension_requirements & 1) = 1) = COALESCE(requires_cost_center, false) AND
            ((dimension_requirements & 2) = 2) = COALESCE(requires_department, false) AND
            ((dimension_requirements & 4) = 4) = COALESCE(requires_project, false)
        ) THEN 'OK'
        ELSE 'MISMATCH'
    END as migration_status
FROM gl_account
WHERE (
    ((dimension_requirements & 1) = 1) != COALESCE(requires_cost_center, false) OR
    ((dimension_requirements & 2) = 2) != COALESCE(requires_department, false) OR
    ((dimension_requirements & 4) = 4) != COALESCE(requires_project, false)
);

-- =====================================================
-- Step 5: Drop old boolean columns (now replaced by dimension_requirements)
-- =====================================================
ALTER TABLE gl_account DROP COLUMN IF EXISTS requires_cost_center;
ALTER TABLE gl_account DROP COLUMN IF EXISTS requires_department;
ALTER TABLE gl_account DROP COLUMN IF EXISTS requires_project;

-- =====================================================
-- Rollback script (if needed) - Run these commands to revert
-- =====================================================
-- Step 1: Re-add the boolean columns
-- ALTER TABLE gl_account ADD COLUMN IF NOT EXISTS requires_cost_center BOOLEAN DEFAULT false;
-- ALTER TABLE gl_account ADD COLUMN IF NOT EXISTS requires_department BOOLEAN DEFAULT false;
-- ALTER TABLE gl_account ADD COLUMN IF NOT EXISTS requires_project BOOLEAN DEFAULT false;
--
-- Step 2: Restore boolean values from dimension_requirements
-- UPDATE gl_account
-- SET
--     requires_cost_center = (dimension_requirements & 1) = 1,
--     requires_department = (dimension_requirements & 2) = 2,
--     requires_project = (dimension_requirements & 4) = 4;
--
-- Step 3: Drop the new column and index
-- DROP INDEX IF EXISTS ix_gl_account_dimension_requirements;
-- ALTER TABLE gl_account DROP COLUMN IF EXISTS dimension_requirements;
