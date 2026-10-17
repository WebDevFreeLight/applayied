-- ============================================================
-- Data Mine Contacts Table
-- Created: 2026-02-25
-- Purpose: Stores raw customer contact data imported via Excel,
--          separate from the leads pipeline (bp_lead).
-- ============================================================

CREATE TABLE IF NOT EXISTS dm_contact (
    contact_id       BIGINT PRIMARY KEY,
    brand_name       VARCHAR(200) NOT NULL,
    brand_name_ar    VARCHAR(200) NULL,
    owner_name       VARCHAR(200) NOT NULL,
    owner_phone      VARCHAR(50) NOT NULL,
    owner_email      VARCHAR(200) NULL,
    company_name     VARCHAR(200) NULL,
    industry         VARCHAR(100) NULL,
    city             VARCHAR(100) NULL,
    region           VARCHAR(100) NULL,
    company_size     VARCHAR(50) NULL,
    website          VARCHAR(500) NULL,
    social_media     VARCHAR(500) NULL,
    data_source      VARCHAR(200) NULL,
    is_main          BOOLEAN DEFAULT false,
    tags             VARCHAR(500) NULL,
    lead_source      VARCHAR(100) NULL,
    budget_range     VARCHAR(50) NULL,
    expected_revenue NUMERIC(18,2) NULL,
    notes            TEXT NULL,
    is_pulled        BOOLEAN DEFAULT false,
    pulled_at        TIMESTAMP NULL,
    pulled_by        VARCHAR(100) NULL,
    pulled_lead_id   BIGINT NULL,
    create_by        VARCHAR(64) NULL,
    create_time      TIMESTAMP NULL,
    update_by        VARCHAR(64) NULL,
    update_time      TIMESTAMP NULL,
    remark           VARCHAR(500) NULL,
    companyid        BIGINT NULL,
    branchid         BIGINT NULL
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_dm_contact_owner_phone ON dm_contact (owner_phone);
CREATE INDEX IF NOT EXISTS idx_dm_contact_owner_email ON dm_contact (owner_email);
CREATE INDEX IF NOT EXISTS idx_dm_contact_is_pulled ON dm_contact (is_pulled);
CREATE INDEX IF NOT EXISTS idx_dm_contact_data_source ON dm_contact (data_source);
CREATE INDEX IF NOT EXISTS idx_dm_contact_industry ON dm_contact (industry);
CREATE INDEX IF NOT EXISTS idx_dm_contact_create_time ON dm_contact (create_time);
