-- =============================================
-- Budget Module Referential Integrity - Foreign Key Constraints
-- Date: 2025-12-14
-- Purpose: Enforce database-level referential integrity for Budget module
-- =============================================

-- =============================================
-- TYPE MISMATCH FIX APPLIED
-- =============================================
/*
ISSUE DISCOVERED AND FIXED:
BudgetHeader.CostCenterId was defined as UUID (Guid) in the C# model
BUT CostCenter.CostCenterId is defined as BIGINT (long) in the database

FIX APPLIED:
- Changed BudgetHeader.CostCenterId from Guid? to long? in the C# model
- Updated all related DTOs (BudgetHeaderQueryDto, BudgetHeaderListDto, CreateBudgetHeaderDto, UpdateBudgetHeaderDto)
- FK constraint FK_budget_header_cost_center can now be created

DATABASE MIGRATION:
If your existing database has cost_center_id as UUID type, see PART 6 for migration steps.
*/

-- =============================================
-- PART 1: Budget Lines -> Budget Header (CASCADE DELETE)
-- =============================================
-- When a budget header is deleted, all its lines should be deleted

-- Drop existing constraint if exists (idempotent)
ALTER TABLE finance_budget_lines
DROP CONSTRAINT IF EXISTS FK_budget_lines_budget_header;

-- Create FK constraint with CASCADE DELETE
-- Note: BudgetHeader PK column is 'id' (not 'budget_id')
ALTER TABLE finance_budget_lines
ADD CONSTRAINT FK_budget_lines_budget_header
    FOREIGN KEY (budget_header_id)
    REFERENCES finance_budget_header(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

COMMENT ON CONSTRAINT FK_budget_lines_budget_header ON finance_budget_lines IS
'Budget lines are child records of budget headers. Deleting a budget cascades to delete all its lines.';

-- =============================================
-- PART 2: Budget Lines -> GL Account (RESTRICT)
-- =============================================
-- Cannot delete a GL account that is referenced by budget lines

ALTER TABLE finance_budget_lines
DROP CONSTRAINT IF EXISTS FK_budget_lines_gl_account;

ALTER TABLE finance_budget_lines
ADD CONSTRAINT FK_budget_lines_gl_account
    FOREIGN KEY (gl_account_id)
    REFERENCES gl_account(account_id)
    ON DELETE RESTRICT
    ON UPDATE CASCADE;

COMMENT ON CONSTRAINT FK_budget_lines_gl_account ON finance_budget_lines IS
'Budget lines must reference valid GL accounts. RESTRICT prevents deletion of accounts with budget allocations.';

-- =============================================
-- PART 3: Budget Variance Summary -> Budget Header (CASCADE DELETE)
-- =============================================
-- Variance summaries are derived data, should cascade with budget

ALTER TABLE finance_budget_variance_summary
DROP CONSTRAINT IF EXISTS FK_variance_budget_header;

-- Note: BudgetHeader PK column is 'id' (not 'budget_id')
ALTER TABLE finance_budget_variance_summary
ADD CONSTRAINT FK_variance_budget_header
    FOREIGN KEY (budget_header_id)
    REFERENCES finance_budget_header(id)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

COMMENT ON CONSTRAINT FK_variance_budget_header ON finance_budget_variance_summary IS
'Variance summaries are derived from budget headers. Cascade delete for cleanup.';

-- =============================================
-- PART 4: Budget Variance Summary -> GL Account (RESTRICT)
-- =============================================
-- Cannot delete a GL account referenced in variance summaries

ALTER TABLE finance_budget_variance_summary
DROP CONSTRAINT IF EXISTS FK_variance_gl_account;

ALTER TABLE finance_budget_variance_summary
ADD CONSTRAINT FK_variance_gl_account
    FOREIGN KEY (gl_account_id)
    REFERENCES gl_account(account_id)
    ON DELETE RESTRICT
    ON UPDATE CASCADE;

COMMENT ON CONSTRAINT FK_variance_gl_account ON finance_budget_variance_summary IS
'Variance summaries reference GL accounts. RESTRICT prevents orphan records.';

-- =============================================
-- PART 5: Budget Header -> Master Data References
-- =============================================

-- 5.1 Budget Header -> Project (SET NULL)
-- If a project is deleted, the budget remains but loses its project link
ALTER TABLE finance_budget_header
DROP CONSTRAINT IF EXISTS FK_budget_header_project;

ALTER TABLE finance_budget_header
ADD CONSTRAINT FK_budget_header_project
    FOREIGN KEY (project_id)
    REFERENCES finance_projects(id)
    ON DELETE SET NULL
    ON UPDATE CASCADE;

COMMENT ON CONSTRAINT FK_budget_header_project ON finance_budget_header IS
'Optional project reference. SET NULL preserves budget if project is deleted.';

-- 5.2 Budget Header -> Department (SET NULL)
-- If a department is deleted, the budget remains but loses its department link
ALTER TABLE finance_budget_header
DROP CONSTRAINT IF EXISTS FK_budget_header_department;

ALTER TABLE finance_budget_header
ADD CONSTRAINT FK_budget_header_department
    FOREIGN KEY (department_id)
    REFERENCES sys_dept(dept_id)
    ON DELETE SET NULL
    ON UPDATE CASCADE;

COMMENT ON CONSTRAINT FK_budget_header_department ON finance_budget_header IS
'Optional department reference. SET NULL preserves budget if department is deleted.';

-- 5.3 Budget Header -> Cost Center (SET NULL)
-- If a cost center is deleted, the budget remains but loses its cost center link
-- NOTE: Type mismatch was fixed - BudgetHeader.CostCenterId changed from Guid? to long?
ALTER TABLE finance_budget_header
DROP CONSTRAINT IF EXISTS FK_budget_header_cost_center;

ALTER TABLE finance_budget_header
ADD CONSTRAINT FK_budget_header_cost_center
    FOREIGN KEY (cost_center_id)
    REFERENCES finance_cost_centers(cost_center_id)
    ON DELETE SET NULL
    ON UPDATE CASCADE;

COMMENT ON CONSTRAINT FK_budget_header_cost_center ON finance_budget_header IS
'Optional cost center reference. SET NULL preserves budget if cost center is deleted.';

-- =============================================
-- PART 6: Database Column Type Migration (IF NEEDED)
-- =============================================
-- The C# model BudgetHeader.CostCenterId has been fixed to use long? instead of Guid?
-- If your existing database has cost_center_id as UUID, run this migration:

/*
-- STEP 1: Check current column type
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'finance_budget_header' AND column_name = 'cost_center_id';

-- STEP 2: If it's UUID, migrate to BIGINT:
-- WARNING: This will lose existing cost_center_id data if it contains UUID values!

-- Option A: Drop and recreate the column
ALTER TABLE finance_budget_header DROP COLUMN IF EXISTS cost_center_id;
ALTER TABLE finance_budget_header ADD COLUMN cost_center_id BIGINT NULL;

-- STEP 3: Then create the FK constraint (already in PART 5.3 above)
*/

-- =============================================
-- PART 7: Verify Constraints Created
-- =============================================

-- List all FK constraints on budget tables
SELECT
    tc.table_name,
    tc.constraint_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name,
    rc.delete_rule,
    rc.update_rule
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
JOIN information_schema.referential_constraints AS rc
    ON tc.constraint_name = rc.constraint_name
    AND tc.table_schema = rc.constraint_schema
WHERE tc.constraint_type = 'FOREIGN KEY'
    AND tc.table_name IN ('finance_budget_header', 'finance_budget_lines', 'finance_budget_variance_summary')
ORDER BY tc.table_name, tc.constraint_name;

-- =============================================
-- PART 8: Check for Orphan Records BEFORE Adding Constraints
-- =============================================
-- Run these queries BEFORE executing the ALTER TABLE statements
-- to identify any existing orphan records that would violate FK constraints

-- 8.1 Budget lines without valid budget headers
SELECT bl.id, bl.budget_header_id, 'ORPHAN: No matching budget header' as issue
FROM finance_budget_lines bl
LEFT JOIN finance_budget_header bh ON bl.budget_header_id = bh.id
WHERE bh.id IS NULL;

-- 8.2 Budget lines with invalid GL account references
SELECT bl.id, bl.gl_account_id, 'ORPHAN: No matching GL account' as issue
FROM finance_budget_lines bl
LEFT JOIN gl_account ga ON bl.gl_account_id = ga.account_id
WHERE ga.account_id IS NULL;

-- 8.3 Variance summaries without valid budget headers
SELECT vs.id, vs.budget_header_id, 'ORPHAN: No matching budget header' as issue
FROM finance_budget_variance_summary vs
LEFT JOIN finance_budget_header bh ON vs.budget_header_id = bh.id
WHERE bh.id IS NULL;

-- 8.4 Variance summaries with invalid GL account references
SELECT vs.id, vs.gl_account_id, 'ORPHAN: No matching GL account' as issue
FROM finance_budget_variance_summary vs
LEFT JOIN gl_account ga ON vs.gl_account_id = ga.account_id
WHERE ga.account_id IS NULL;

-- 8.5 Budget headers with invalid project references
SELECT bh.id, bh.project_id, 'ORPHAN: No matching project' as issue
FROM finance_budget_header bh
LEFT JOIN finance_projects p ON bh.project_id = p.id
WHERE bh.project_id IS NOT NULL AND p.id IS NULL;

-- 8.6 Budget headers with invalid department references
SELECT bh.id, bh.department_id, 'ORPHAN: No matching department' as issue
FROM finance_budget_header bh
LEFT JOIN sys_dept d ON bh.department_id = d.dept_id
WHERE bh.department_id IS NOT NULL AND d.dept_id IS NULL;

-- =============================================
-- FK CONSTRAINT SUMMARY
-- =============================================
/*
CONSTRAINTS CREATED (7 total):

1. FK_budget_lines_budget_header
   finance_budget_lines.budget_header_id -> finance_budget_header.id
   ON DELETE CASCADE (child records deleted with parent)

2. FK_budget_lines_gl_account
   finance_budget_lines.gl_account_id -> gl_account.account_id
   ON DELETE RESTRICT (prevent GL account deletion if referenced)

3. FK_variance_budget_header
   finance_budget_variance_summary.budget_header_id -> finance_budget_header.id
   ON DELETE CASCADE (derived data deleted with parent)

4. FK_variance_gl_account
   finance_budget_variance_summary.gl_account_id -> gl_account.account_id
   ON DELETE RESTRICT (prevent GL account deletion if referenced)

5. FK_budget_header_project
   finance_budget_header.project_id -> finance_projects.id
   ON DELETE SET NULL (preserve budget, clear project reference)

6. FK_budget_header_department
   finance_budget_header.department_id -> sys_dept.dept_id
   ON DELETE SET NULL (preserve budget, clear department reference)

7. FK_budget_header_cost_center
   finance_budget_header.cost_center_id -> finance_cost_centers.cost_center_id
   ON DELETE SET NULL (preserve budget, clear cost center reference)

IMPACT ANALYSIS:
- Deleting a BudgetHeader will automatically delete all BudgetLines and VarianceSummaries
- Cannot delete GLAccounts that have budget allocations (must remove budget lines first)
- Deleting a Project, Department, or CostCenter preserves budgets but sets the reference to NULL
- ALL master data references now have database-level FK enforcement

CODE CHANGES MADE (to fix type mismatch):
- BudgetHeader.CostCenterId changed from Guid? to long? (matches CostCenter.CostCenterId)
- BudgetDto classes updated: BudgetHeaderQueryDto, BudgetHeaderListDto, CreateBudgetHeaderDto, UpdateBudgetHeaderDto

REMAINING SCHEMA ISSUE (out of scope for Budget module):
- Project.CostCenterId is still Guid? but CostCenter.CostCenterId is long
- This should be fixed in the Project model for consistency
*/
