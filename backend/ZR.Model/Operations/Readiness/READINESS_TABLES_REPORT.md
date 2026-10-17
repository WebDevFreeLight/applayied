# Operations Readiness Module — Database Tables Report

## Overview

The Readiness module manages **store/shop readiness tracking** for operations. It covers:
- Defining checklist categories and items
- Tracking per-store readiness (item-by-item completion)
- Running inspections and scoring stores
- Handover workflow (request → approve/reject)
- Score history for analytics

---

## Tables (9 total)

### 1. `ops_readiness_category`
**Purpose:** Master list of readiness categories (e.g. Electrical, Plumbing, Branding, etc.)

| Column | Type | Notes |
|--------|------|-------|
| `category_id` | long (PK) | Snowflake ID |
| `category_code` | string(50) | Unique code (e.g. "ELEC", "PLUMB") |
| `name_en` | string(200) | English name |
| `name_ar` | string(200) | Arabic name |
| `description` | string(500) | Nullable |
| `icon` | string(100) | Icon name for UI |
| `color` | string(50) | Color hex/name for UI |
| `weight` | decimal | Scoring weight multiplier |
| `sort_order` | int | Display order |
| `is_required` | bool | Must pass for handover approval |
| `is_active` | bool | Soft delete |
| + SysBase fields | | create_by, create_time, update_by, update_time |

**Computed (not in DB):** `ChecklistCount` — number of linked checklists.

---

### 2. `ops_readiness_checklist`
**Purpose:** A versioned checklist template linked to a category.

| Column | Type | Notes |
|--------|------|-------|
| `checklist_id` | long (PK) | Snowflake ID |
| `category_id` | long (FK) | → `ops_readiness_category` |
| `checklist_code` | string(50) | Unique code |
| `name_en` | string(200) | English name |
| `name_ar` | string(200) | Arabic name |
| `description` | string(500) | Nullable |
| `version` | int | Checklist version (default 1) |
| `applies_to_type` | string(50) | Nullable — "Kiosk", "Shop", or null (all) |
| `is_active` | bool | |
| + SysBase fields | | |

**Navigation:** `Items` → list of `OpsReadinessChecklistItem`.

---

### 3. `ops_readiness_checklist_item`
**Purpose:** Individual checklist line items (the actual things to check).

| Column | Type | Notes |
|--------|------|-------|
| `item_id` | long (PK) | Snowflake ID |
| `checklist_id` | long (FK) | Nullable → `ops_readiness_checklist` |
| `category_id` | long (FK) | → `ops_readiness_category` |
| `item_code` | string(50) | Unique code |
| `description_en` | string(500) | English description |
| `description_ar` | string(500) | Arabic description |
| `severity_weight` | int | 1 (low) to 4 (critical) |
| `photo_required` | bool | Must attach photo evidence |
| `is_mandatory` | bool | Cannot skip |
| `is_blocking` | bool | Blocks handover if failed |
| `sort_order` | int | Display order |
| `help_text` | string(1000) | Guidance for inspector |
| `is_active` | bool | |
| + SysBase fields | | |

**Navigation:** `ChecklistName`, `CategoryNameEn`, `CategoryNameAr`.

---

### 4. `ops_store_readiness`
**Purpose:** Per-store readiness tracking record. One record per store (per season if applicable).

| Column | Type | Notes |
|--------|------|-------|
| `readiness_id` | long (PK) | Snowflake ID |
| `shop_id` | long (FK) | → boulevard shop |
| `season_id` | long (FK) | Nullable — linked season |
| `assigned_inspector_id` | long (FK) | Nullable — assigned inspector employee |
| `status` | string(50) | Pending, InProgress, Completed, Blocked, Rejected |
| `handover_status` | string(50) | NotReady, Ready, Approved, Rejected |
| `overall_score` | decimal | Calculated score (0-100) |
| `score_grade` | string(20) | Red / Yellow / Green |
| `total_items` | int | Total checklist items |
| `completed_items` | int | Items marked completed |
| `blocked_items` | int | Items marked blocked |
| `category_scores` | text | JSON snapshot of per-category scores |
| `notes` | text | Nullable |
| `started_at` | DateTime | Nullable |
| `completed_at` | DateTime | Nullable |
| + SysBase fields | | |

**Navigation:** `ShopName`, `ZoneName`, `UnitNumber`, `Items` list.

---

### 5. `ops_store_readiness_item`
**Purpose:** Per-item status for a specific store's readiness. One record per checklist item per store.

| Column | Type | Notes |
|--------|------|-------|
| `item_result_id` | long (PK) | Snowflake ID |
| `readiness_id` | long (FK) | → `ops_store_readiness` |
| `shop_id` | long (FK) | → boulevard shop |
| `checklist_item_id` | long (FK) | → `ops_readiness_checklist_item` |
| `category_id` | long (FK) | → `ops_readiness_category` |
| `status` | string(50) | Pending, InProgress, Completed, Blocked, Rejected, NA |
| `before_photo_urls` | string(2000) | Comma-separated URLs — "before" photos |
| `after_photo_urls` | string(2000) | Comma-separated URLs — "after" photos |
| `contractor_name` | string(200) | Assigned contractor (nullable) |
| `contractor_contact` | string(100) | Contractor phone/email (nullable) |
| `expected_date` | DateTime | Nullable — when item should be done |
| `completed_at` | DateTime | Nullable — when actually completed |
| `last_updated_at` | DateTime | Nullable |
| `notes` | text | Nullable |
| + SysBase fields | | |

**Navigation:** `ItemDescriptionEn`, `ItemDescriptionAr`, `CategoryNameEn`, `CategoryNameAr`, `InspectorName`, `SeverityWeight`, `IsBlocking`, `IsMandatory`.

---

### 6. `ops_store_inspection`
**Purpose:** A formal inspection event for a store, conducted by an inspector.

| Column | Type | Notes |
|--------|------|-------|
| `inspection_id` | long (PK) | Snowflake ID |
| `shop_id` | long (FK) | → boulevard shop |
| `inspector_id` | long (FK) | → employee |
| `inspection_type` | string(50) | Routine, Followup, PreContract, PostContract, Spot |
| `inspection_status` | string(50) | Scheduled, InProgress, Completed, Cancelled |
| `scheduled_date` | DateTime | Nullable |
| `scheduled_time` | string(20) | Nullable (e.g. "10:00") |
| `started_at` | DateTime | Nullable |
| `completed_at` | DateTime | Nullable |
| `overall_score` | decimal | 0-100 |
| `score_grade` | string(20) | Red / Yellow / Green |
| `cancel_reason` | string(500) | Nullable |
| `notes` | text | Nullable |
| + SysBase fields | | |

**Navigation:** `ShopName`, `ZoneName`, `UnitNumber`, `InspectorName`, `Results` list.

---

### 7. `ops_inspection_result`
**Purpose:** Result of each checklist item within an inspection.

| Column | Type | Notes |
|--------|------|-------|
| `result_id` | long (PK) | Snowflake ID |
| `inspection_id` | long (FK) | → `ops_store_inspection` |
| `checklist_item_id` | long (FK) | → `ops_readiness_checklist_item` |
| `category_id` | long (FK) | → `ops_readiness_category` |
| `result_value` | string(20) | Pass, Fail, NA |
| `comments` | string(1000) | Nullable |
| `photo_urls` | string(2000) | Comma-separated evidence photos |
| + SysBase fields | | |

**Navigation:** `ItemDescriptionEn`, `ItemDescriptionAr`, `CategoryNameEn`, `CategoryNameAr`.

---

### 8. `ops_store_handover`
**Purpose:** Formal handover request/approval workflow for a store.

| Column | Type | Notes |
|--------|------|-------|
| `handover_id` | long (PK) | Snowflake ID |
| `shop_id` | long (FK) | → boulevard shop |
| `readiness_id` | long (FK) | → `ops_store_readiness` |
| `handover_status` | string(50) | Pending, Approved, Rejected, Reopened |
| `requested_by` | long | User who requested |
| `requested_at` | DateTime | |
| `approved_by` | long | Nullable — user who approved |
| `approved_at` | DateTime | Nullable |
| `rejected_by` | long | Nullable — user who rejected |
| `rejected_at` | DateTime | Nullable |
| `rejection_reason` | string(1000) | Nullable |
| `reopened_by` | long | Nullable — user who reopened |
| `reopened_at` | DateTime | Nullable |
| `reopen_reason` | string(1000) | Nullable |
| `final_score` | decimal | Score at handover time |
| `final_grade` | string(20) | Grade at handover time |
| `category_scores_snapshot` | text | JSON snapshot of all category scores |
| `notes` | text | Nullable |
| + SysBase fields | | |

**Navigation:** `ShopName`, `ZoneName`, `UnitNumber`, `ReadinessScore`, `ReadinessGrade`.

---

### 9. `ops_store_score_history`
**Purpose:** Historical score snapshots for trend/analytics.

| Column | Type | Notes |
|--------|------|-------|
| `score_id` | long (PK) | Snowflake ID |
| `shop_id` | long (FK) | → boulevard shop |
| `inspection_id` | long (FK) | Nullable → `ops_store_inspection` |
| `readiness_id` | long (FK) | Nullable → `ops_store_readiness` |
| `snapshot_type` | string(50) | "tracking" or "inspection" |
| `overall_score` | decimal | 0-100 |
| `score_grade` | string(20) | Red / Yellow / Green |
| `category_scores` | text | JSON breakdown |
| `recorded_at` | DateTime | When snapshot was taken |
| + SysBase fields | | |

**Navigation:** `ShopName`.

---

## Entity Relationships

```
ops_readiness_category
  ├── ops_readiness_checklist (1:N via category_id)
  │     └── ops_readiness_checklist_item (1:N via checklist_id)
  ├── ops_readiness_checklist_item (1:N via category_id — direct link)
  ├── ops_store_readiness_item (1:N via category_id)
  └── ops_inspection_result (1:N via category_id)

ops_store_readiness (per store)
  ├── ops_store_readiness_item (1:N via readiness_id)
  ├── ops_store_handover (1:N via readiness_id)
  └── ops_store_score_history (1:N via readiness_id)

ops_store_inspection (per store)
  ├── ops_inspection_result (1:N via inspection_id)
  └── ops_store_score_history (1:N via inspection_id)
```

---

## Workflows

### Readiness Tracking Flow
```
1. Initialize readiness for a store → status: Pending
2. Checklist items auto-created → each item: Pending
3. Field team updates items one by one → InProgress
4. Items get completed / blocked / rejected
5. When all non-blocking items done → status: Completed, handover_status: Ready
```

### Inspection Flow
```
1. Schedule inspection → status: Scheduled
2. Inspector starts → status: InProgress
3. Inspector submits results per item (Pass/Fail/NA)
4. System calculates score → status: Completed
5. Score snapshot saved to ops_store_score_history
```

### Handover Flow
```
1. Field team requests handover → status: Pending
2. Manager reviews:
   - Approve → status: Approved (store is officially handed over)
   - Reject → status: Rejected (with reason)
3. If rejected, can be reopened → status: Reopened (fix issues, re-request)
```

### Scoring
- **Score Grade:** Red (<60), Yellow (60-80), Green (>80)
- **Category Weight:** Each category has a `weight` multiplier
- **Severity Weight:** Each item has severity 1-4 (higher = more impactful on score)
- **Blocking Items:** `is_blocking = true` items prevent handover approval if not completed
