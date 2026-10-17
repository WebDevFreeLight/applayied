-- ============================================================
-- Create Missing Readiness Tables
-- Tables: ops_store_readiness, ops_store_readiness_item, ops_store_handover
-- Safe to re-run: uses IF NOT EXISTS
-- ============================================================

-- ============================================
-- 1. ops_store_readiness (main tracking record per store)
-- ============================================
CREATE TABLE IF NOT EXISTS ops_store_readiness (
    readiness_id    BIGINT PRIMARY KEY,
    shop_id         BIGINT NOT NULL,
    season_id       BIGINT NULL,
    status          VARCHAR(30) NOT NULL DEFAULT 'Pending',
    overall_score   NUMERIC NOT NULL DEFAULT 0,
    score_grade     VARCHAR(20) NULL,
    handover_status VARCHAR(30) NOT NULL DEFAULT 'NotReady',
    assigned_inspector_id BIGINT NULL,
    total_items     INTEGER NOT NULL DEFAULT 0,
    completed_items INTEGER NOT NULL DEFAULT 0,
    blocked_items   INTEGER NOT NULL DEFAULT 0,
    category_scores TEXT NULL,
    notes           TEXT NULL,
    last_calculated_at TIMESTAMP NULL,
    create_by       VARCHAR(64) NULL,
    create_time     TIMESTAMP NULL,
    update_by       VARCHAR(64) NULL,
    update_time     TIMESTAMP NULL,
    remark          VARCHAR(500) NULL,
    companyid       BIGINT NULL,
    branchid        BIGINT NULL
);

CREATE INDEX IF NOT EXISTS ix_store_readiness_shop ON ops_store_readiness(shop_id);
CREATE INDEX IF NOT EXISTS ix_store_readiness_status ON ops_store_readiness(status);
CREATE INDEX IF NOT EXISTS ix_store_readiness_handover ON ops_store_readiness(handover_status);

-- ============================================
-- 2. ops_store_readiness_item (per-item tracking)
-- ============================================
CREATE TABLE IF NOT EXISTS ops_store_readiness_item (
    item_result_id      BIGINT PRIMARY KEY,
    readiness_id        BIGINT NOT NULL,
    shop_id             BIGINT NOT NULL,
    checklist_item_id   BIGINT NOT NULL,
    category_id         BIGINT NOT NULL,
    status              VARCHAR(20) NOT NULL DEFAULT 'Pending',
    notes               TEXT NULL,
    before_photo_urls   TEXT NULL,
    after_photo_urls    TEXT NULL,
    inspector_user_id   BIGINT NULL,
    contractor_name     VARCHAR(200) NULL,
    contractor_contact  VARCHAR(200) NULL,
    expected_date       TIMESTAMP NULL,
    completed_at        TIMESTAMP NULL,
    last_updated_at     TIMESTAMP NULL,
    last_updated_by     VARCHAR(100) NULL,
    create_by           VARCHAR(64) NULL,
    create_time         TIMESTAMP NULL,
    update_by           VARCHAR(64) NULL,
    update_time         TIMESTAMP NULL,
    remark              VARCHAR(500) NULL,
    companyid           BIGINT NULL,
    branchid            BIGINT NULL
);

CREATE INDEX IF NOT EXISTS ix_readiness_item_readiness ON ops_store_readiness_item(readiness_id);
CREATE INDEX IF NOT EXISTS ix_readiness_item_shop ON ops_store_readiness_item(shop_id);
CREATE INDEX IF NOT EXISTS ix_readiness_item_category ON ops_store_readiness_item(category_id);
CREATE INDEX IF NOT EXISTS ix_readiness_item_status ON ops_store_readiness_item(status);

-- ============================================
-- 3. ops_store_handover (handover workflow)
-- ============================================
CREATE TABLE IF NOT EXISTS ops_store_handover (
    handover_id             BIGINT PRIMARY KEY,
    shop_id                 BIGINT NOT NULL,
    readiness_id            BIGINT NOT NULL,
    handover_status         VARCHAR(30) NOT NULL DEFAULT 'Pending',
    requested_by            VARCHAR(100) NULL,
    requested_at            TIMESTAMP NULL,
    approved_by             VARCHAR(100) NULL,
    approved_at             TIMESTAMP NULL,
    rejected_by             VARCHAR(100) NULL,
    rejected_at             TIMESTAMP NULL,
    rejection_reason        VARCHAR(500) NULL,
    reopened_by             VARCHAR(100) NULL,
    reopened_at             TIMESTAMP NULL,
    reopen_reason           VARCHAR(500) NULL,
    final_score             NUMERIC NULL,
    final_grade             VARCHAR(20) NULL,
    category_scores_snapshot TEXT NULL,
    notes                   TEXT NULL,
    create_by               VARCHAR(64) NULL,
    create_time             TIMESTAMP NULL,
    update_by               VARCHAR(64) NULL,
    update_time             TIMESTAMP NULL,
    remark                  VARCHAR(500) NULL,
    companyid               BIGINT NULL,
    branchid                BIGINT NULL
);

CREATE INDEX IF NOT EXISTS ix_handover_shop ON ops_store_handover(shop_id);
CREATE INDEX IF NOT EXISTS ix_handover_readiness ON ops_store_handover(readiness_id);
CREATE INDEX IF NOT EXISTS ix_handover_status ON ops_store_handover(handover_status);

-- ============================================
-- VERIFICATION
-- ============================================
SELECT table_name FROM information_schema.tables
WHERE table_name IN ('ops_store_readiness', 'ops_store_readiness_item', 'ops_store_handover')
ORDER BY table_name;
