-- Phase 1: System Stabilization Migration
-- Run on Docker PostgreSQL: docker exec -i postgres psql -U abdulqader -d MahamDB

-- ============================================================
-- 1. Soft delete columns on 7 tables
-- ============================================================
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE bp_lead ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP NULL;

ALTER TABLE arap_customer ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE arap_customer ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP NULL;

ALTER TABLE sales_deal ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE sales_deal ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP NULL;

ALTER TABLE ops_site_visit ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE ops_site_visit ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP NULL;

ALTER TABLE erp_contract ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE erp_contract ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP NULL;

ALTER TABLE erp_contract_unit ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE erp_contract_unit ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP NULL;

ALTER TABLE bp_lead_activity ADD COLUMN IF NOT EXISTS is_deleted BOOLEAN NOT NULL DEFAULT false;
ALTER TABLE bp_lead_activity ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP NULL;

-- ============================================================
-- 2. Entity Status Audit Table
-- ============================================================
CREATE TABLE IF NOT EXISTS entity_status_audit (
    id BIGINT PRIMARY KEY,
    entity_type VARCHAR(50) NOT NULL,
    entity_id VARCHAR(100) NOT NULL,
    old_status VARCHAR(50),
    new_status VARCHAR(50) NOT NULL,
    notes TEXT,
    changed_by VARCHAR(64) NOT NULL,
    changed_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS IX_audit_entity ON entity_status_audit(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS IX_audit_changed_at ON entity_status_audit(changed_at);

-- ============================================================
-- 3. Unique index to prevent duplicate customers per lead
-- ============================================================
CREATE UNIQUE INDEX IF NOT EXISTS UX_customer_lead_id
ON arap_customer(lead_id)
WHERE lead_id IS NOT NULL AND is_deleted = false;
