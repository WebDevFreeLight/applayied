 SQL: DROP Old Tables

  -- ═══════════════════════════════════════════════════════════
  -- DROP ALL OLD READINESS TABLES (9 tables)
  -- Run in dependency order (children first)
  -- ═══════════════════════════════════════════════════════════

  DROP TABLE IF EXISTS ops_inspection_result;
  DROP TABLE IF EXISTS ops_store_inspection;
  DROP TABLE IF EXISTS ops_store_readiness_item;
  DROP TABLE IF EXISTS ops_store_score_history;
  DROP TABLE IF EXISTS ops_store_handover;
  DROP TABLE IF EXISTS ops_store_readiness;
  DROP TABLE IF EXISTS ops_readiness_checklist_item;
  DROP TABLE IF EXISTS ops_readiness_checklist;
  DROP TABLE IF EXISTS ops_readiness_category;

  SQL: CREATE New Tables (PostgreSQL)

  -- ═══════════════════════════════════════════════════════════
  -- CREATE NEW TABLES (3 tables)
  -- ═══════════════════════════════════════════════════════════

  -- 1. Flat requirement master list
  CREATE TABLE ops_handover_requirement (
      requirement_id  BIGINT PRIMARY KEY,
      code            VARCHAR(50) NOT NULL UNIQUE,
      name_en         VARCHAR(300) NOT NULL,
      name_ar         VARCHAR(300) NOT NULL,
      description_en  VARCHAR(1000),
      description_ar  VARCHAR(1000),
      percentage      DECIMAL(5,2) NOT NULL DEFAULT 0,
      photo_required  BOOLEAN NOT NULL DEFAULT FALSE,
      is_blocking     BOOLEAN NOT NULL DEFAULT FALSE,
      sort_order      INTEGER NOT NULL DEFAULT 0,
      is_active       BOOLEAN NOT NULL DEFAULT TRUE,
      -- SysBase
      "Create_by"     VARCHAR(64),
      "Create_time"   TIMESTAMP DEFAULT NOW(),
      "Update_by"     VARCHAR(64),
      "Update_time"   TIMESTAMP,
      "Remark"        VARCHAR(500),
      "CompanyID"     BIGINT,
      "BranchID"      BIGINT
  );

  -- 2. Per-store handover tracking + workflow
  CREATE TABLE ops_store_handover (
      handover_id     BIGINT PRIMARY KEY,
      shop_id         BIGINT NOT NULL,
      season_id       BIGINT,
      progress        DECIMAL(5,2) NOT NULL DEFAULT 0,
      status          VARCHAR(30) NOT NULL DEFAULT 'Pending',
      notes           TEXT,
      requested_by    VARCHAR(100),
      requested_at    TIMESTAMP,
      approved_by     VARCHAR(100),
      approved_at     TIMESTAMP,
      rejected_by     VARCHAR(100),
      rejected_at     TIMESTAMP,
      rejection_reason VARCHAR(500),
      -- SysBase
      "Create_by"     VARCHAR(64),
      "Create_time"   TIMESTAMP DEFAULT NOW(),
      "Update_by"     VARCHAR(64),
      "Update_time"   TIMESTAMP,
      "Remark"        VARCHAR(500),
      "CompanyID"     BIGINT,
      "BranchID"      BIGINT
  );

  -- 3. Per-requirement per-store result
  CREATE TABLE ops_store_handover_item (
      item_id         BIGINT PRIMARY KEY,
      handover_id     BIGINT NOT NULL REFERENCES ops_store_handover(handover_id),
      requirement_id  BIGINT NOT NULL REFERENCES ops_handover_requirement(requirement_id),
      is_completed    BOOLEAN NOT NULL DEFAULT FALSE,
      photo_urls      TEXT,
      completed_at    TIMESTAMP,
      completed_by    VARCHAR(100),
      notes           TEXT,
      -- SysBase
      "Create_by"     VARCHAR(64),
      "Create_time"   TIMESTAMP DEFAULT NOW(),
      "Update_by"     VARCHAR(64),
      "Update_time"   TIMESTAMP,
      "Remark"        VARCHAR(500),
      "CompanyID"     BIGINT,
      "BranchID"      BIGINT
  );

  -- Indexes
  CREATE INDEX idx_handover_shop ON ops_store_handover(shop_id);
  CREATE INDEX idx_handover_status ON ops_store_handover(status);
  CREATE INDEX idx_handover_item_handover ON ops_store_handover_item(handover_id);
  CREATE INDEX idx_handover_item_requirement ON ops_store_handover_item(requirement_id);
  CREATE INDEX idx_requirement_active ON ops_handover_requirement(is_active);
