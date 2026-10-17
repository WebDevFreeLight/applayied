-- =============================================
-- GL Module Performance Hardening - Database Indexes
-- Date: 2025-12-14
-- Purpose: Add missing critical indexes for GL Journal and Budget modules
-- =============================================

-- =============================================
-- PART 1: GL Journal Indexes
-- =============================================

-- 1.1 Unique constraint: JournalNumber must be unique per company
CREATE UNIQUE INDEX IF NOT EXISTS UX_gl_journal_company_number
ON gl_journal(companyid, journal_number);

-- 1.2 Date-based queries (most frequent query pattern)
CREATE INDEX IF NOT EXISTS IX_gl_journal_date
ON gl_journal(journal_date DESC);

-- 1.3 Company + Date for tenant-isolated date queries
CREATE INDEX IF NOT EXISTS IX_gl_journal_company_date
ON gl_journal(companyid, journal_date DESC);

-- 1.4 Status filtering (Draft, Submitted, Approved, Posted, etc.)
CREATE INDEX IF NOT EXISTS IX_gl_journal_status
ON gl_journal(status);

-- 1.5 Company + Status for tenant-isolated status queries
CREATE INDEX IF NOT EXISTS IX_gl_journal_company_status
ON gl_journal(companyid, status);

-- 1.6 Fiscal period reporting (used by period closing and reports)
CREATE INDEX IF NOT EXISTS IX_gl_journal_fiscal_period
ON gl_journal(fiscal_year, fiscal_period);

-- 1.7 Company + Fiscal for tenant-isolated period reports
CREATE INDEX IF NOT EXISTS IX_gl_journal_company_fiscal
ON gl_journal(companyid, fiscal_year, fiscal_period);

-- 1.8 Source document lookup (for tracing originating documents)
CREATE INDEX IF NOT EXISTS IX_gl_journal_source_doc
ON gl_journal(source_document_id)
WHERE source_document_id IS NOT NULL;

-- 1.9 Posted date for audit trail queries
CREATE INDEX IF NOT EXISTS IX_gl_journal_posted_date
ON gl_journal(posted_date DESC)
WHERE posted_date IS NOT NULL;

-- 1.10 Reversal tracking (find reversals)
CREATE INDEX IF NOT EXISTS IX_gl_journal_original
ON gl_journal(original_journal_id)
WHERE original_journal_id IS NOT NULL;

-- 1.11 Reversal tracking (find reversed journals)
CREATE INDEX IF NOT EXISTS IX_gl_journal_reversal
ON gl_journal(reversal_journal_id)
WHERE reversal_journal_id IS NOT NULL;

-- =============================================
-- PART 2: GL Journal Entry Indexes
-- =============================================

-- 2.1 FK index for journal_id (critical for joins)
CREATE INDEX IF NOT EXISTS IX_gl_entry_journal
ON gl_journal_entry(journal_id);

-- 2.2 FK index for account_id (critical for balance calculations)
CREATE INDEX IF NOT EXISTS IX_gl_entry_account
ON gl_journal_entry(account_id);

-- 2.3 Combined index for balance calculations per account
CREATE INDEX IF NOT EXISTS IX_gl_entry_account_journal
ON gl_journal_entry(account_id, journal_id);

-- 2.4 Cost center analysis
CREATE INDEX IF NOT EXISTS IX_gl_entry_cost_center
ON gl_journal_entry(cost_center_id)
WHERE cost_center_id IS NOT NULL;

-- 2.5 Account + Cost center for dimension reports
CREATE INDEX IF NOT EXISTS IX_gl_entry_account_cost_center
ON gl_journal_entry(account_id, cost_center_id)
WHERE cost_center_id IS NOT NULL;

-- 2.6 Project analysis
CREATE INDEX IF NOT EXISTS IX_gl_entry_project
ON gl_journal_entry(project_id)
WHERE project_id IS NOT NULL;

-- 2.7 Account + Project for dimension reports
CREATE INDEX IF NOT EXISTS IX_gl_entry_account_project
ON gl_journal_entry(account_id, project_id)
WHERE project_id IS NOT NULL;

-- 2.8 Department analysis
CREATE INDEX IF NOT EXISTS IX_gl_entry_department
ON gl_journal_entry(department_id)
WHERE department_id IS NOT NULL;

-- 2.9 Company isolation index
CREATE INDEX IF NOT EXISTS IX_gl_entry_company
ON gl_journal_entry(companyid);

-- 2.10 Company + Account for tenant-isolated balance queries
CREATE INDEX IF NOT EXISTS IX_gl_entry_company_account
ON gl_journal_entry(companyid, account_id);

-- =============================================
-- PART 3: Budget Header Indexes
-- =============================================

-- 3.1 Unique constraint: BudgetCode must be unique per company
CREATE UNIQUE INDEX IF NOT EXISTS UX_budget_header_company_code
ON finance_budget_header(companyid, budget_code);

-- 3.2 Company isolation index
CREATE INDEX IF NOT EXISTS IX_budget_header_company
ON finance_budget_header(companyid);

-- 3.3 Fiscal year queries per company
CREATE INDEX IF NOT EXISTS IX_budget_header_company_fiscal_year
ON finance_budget_header(companyid, fiscal_year);

-- 3.4 Status filtering per company
CREATE INDEX IF NOT EXISTS IX_budget_header_company_status
ON finance_budget_header(companyid, status);

-- 3.5 Cost center lookup
CREATE INDEX IF NOT EXISTS IX_budget_header_cost_center
ON finance_budget_header(cost_center_id)
WHERE cost_center_id IS NOT NULL;

-- 3.6 Project lookup
CREATE INDEX IF NOT EXISTS IX_budget_header_project
ON finance_budget_header(project_id)
WHERE project_id IS NOT NULL;

-- 3.7 Department lookup
CREATE INDEX IF NOT EXISTS IX_budget_header_department
ON finance_budget_header(department_id)
WHERE department_id IS NOT NULL;

-- 3.8 Type filtering per company
CREATE INDEX IF NOT EXISTS IX_budget_header_company_type
ON finance_budget_header(companyid, type);

-- =============================================
-- PART 4: Budget Line Indexes
-- =============================================

-- 4.1 Company isolation index
CREATE INDEX IF NOT EXISTS IX_budget_line_company
ON finance_budget_lines(companyid);

-- 4.2 Company + Header for filtered queries
CREATE INDEX IF NOT EXISTS IX_budget_line_company_header
ON finance_budget_lines(companyid, budget_header_id);

-- =============================================
-- PART 5: Budget Variance Summary Indexes
-- =============================================

-- 5.1 Stale data identification
CREATE INDEX IF NOT EXISTS IX_variance_stale
ON finance_budget_variance_summary(is_stale)
WHERE is_stale = true;

-- 5.2 Company isolation index
CREATE INDEX IF NOT EXISTS IX_variance_company
ON finance_budget_variance_summary(companyid);

-- 5.3 Company + Header for filtered queries
CREATE INDEX IF NOT EXISTS IX_variance_company_header
ON finance_budget_variance_summary(companyid, budget_header_id);

-- =============================================
-- PART 6: Verify Index Creation
-- =============================================

-- PostgreSQL specific: List all indexes created
SELECT
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename IN ('gl_journal', 'gl_journal_entry', 'finance_budget_header', 'finance_budget_lines', 'finance_budget_variance_summary')
ORDER BY tablename, indexname;

-- =============================================
-- Index Justification Summary
-- =============================================
/*
GL JOURNAL INDEXES (12 indexes):
- UX_gl_journal_company_number: Enforce unique journal numbers per company
- IX_gl_journal_date: Speed up date range queries (most common query pattern)
- IX_gl_journal_company_date: Tenant-isolated date queries
- IX_gl_journal_status: Filter by status (Draft, Posted, etc.)
- IX_gl_journal_company_status: Tenant-isolated status filtering
- IX_gl_journal_fiscal_period: Period closing and fiscal reports
- IX_gl_journal_company_fiscal: Tenant-isolated period reports
- IX_gl_journal_source_doc: Trace source documents
- IX_gl_journal_posted_date: Audit trail queries
- IX_gl_journal_original: Find reversed journals
- IX_gl_journal_reversal: Find reversal journals

GL JOURNAL ENTRY INDEXES (10 indexes):
- IX_gl_entry_journal: FK join optimization
- IX_gl_entry_account: Balance calculation queries (CRITICAL)
- IX_gl_entry_account_journal: Combined balance + journal queries
- IX_gl_entry_cost_center: Cost center reports
- IX_gl_entry_account_cost_center: Cost center dimension reports
- IX_gl_entry_project: Project reports
- IX_gl_entry_account_project: Project dimension reports
- IX_gl_entry_department: Department analysis
- IX_gl_entry_company: Tenant isolation
- IX_gl_entry_company_account: Tenant-isolated balance queries

BUDGET INDEXES (12 indexes):
- Company-based unique constraints for budget codes
- Fiscal year filtering per company
- Status, type, cost center, project lookups
- Stale variance data identification

TOTAL: 34 indexes across 5 tables
*/
