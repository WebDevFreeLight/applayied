-- ============================================================
-- Rollback All Pulled Leads from Data Mine
-- PostgreSQL
-- ============================================================
-- Deletes leads that were created via "Pull to Leads" from
-- dm_contact, and resets the contact pull status back to
-- available.
--
-- Strategy: uses dm_contact.pulled_lead_id as the direct FK
-- linking contact → lead. This is the most reliable join.
--
-- Also cleans up related tables:
--   bp_lead_activity, bp_lead_assignment_log
-- ============================================================

BEGIN;

-- ──────────────────────────────────────────────────────────────
-- STEP 0: Preview — see what will be affected (run before COMMIT)
-- ──────────────────────────────────────────────────────────────
-- SELECT c.contact_id, c.brand_name, c.owner_phone, c.pulled_lead_id,
--        l.leadid, l.brandname, l.leadstatus, l.create_time
-- FROM dm_contact c
-- JOIN bp_lead l ON c.pulled_lead_id = l.leadid
-- WHERE c.is_pulled = true AND c.pulled_lead_id IS NOT NULL
-- ORDER BY c.create_time DESC;

-- ──────────────────────────────────────────────────────────────
-- STEP 1: Delete lead activities for pulled leads
-- ──────────────────────────────────────────────────────────────
DELETE FROM bp_lead_activity
WHERE lead_id IN (
    SELECT pulled_lead_id
    FROM dm_contact
    WHERE is_pulled = true AND pulled_lead_id IS NOT NULL
);

-- ──────────────────────────────────────────────────────────────
-- STEP 2: Delete lead assignment logs for pulled leads
-- ──────────────────────────────────────────────────────────────
DELETE FROM bp_lead_assignment_log
WHERE lead_id IN (
    SELECT pulled_lead_id
    FROM dm_contact
    WHERE is_pulled = true AND pulled_lead_id IS NOT NULL
);

-- ──────────────────────────────────────────────────────────────
-- STEP 3: Delete the leads themselves
-- ──────────────────────────────────────────────────────────────
DELETE FROM bp_lead
WHERE leadid IN (
    SELECT pulled_lead_id
    FROM dm_contact
    WHERE is_pulled = true AND pulled_lead_id IS NOT NULL
);

-- ──────────────────────────────────────────────────────────────
-- STEP 4: Reset dm_contact pull status back to available
-- ──────────────────────────────────────────────────────────────
UPDATE dm_contact
SET is_pulled     = false,
    pulled_at     = NULL,
    pulled_by     = NULL,
    pulled_lead_id = NULL,
    update_time   = NOW()
WHERE is_pulled = true AND pulled_lead_id IS NOT NULL;

COMMIT;

-- ══════════════════════════════════════════════════════════════
-- VERIFY: Run after COMMIT
-- ══════════════════════════════════════════════════════════════

-- 1) Should return 0 rows — no pulled contacts left
SELECT COUNT(*) AS still_pulled FROM dm_contact WHERE is_pulled = true;

-- 2) Confirm no orphan leads from Data Mine remain
--    (matches by leadsource = 'Data Mine')
SELECT leadid, brandname, ownerphone, leadstatus, leadsource
FROM bp_lead
WHERE leadsource = 'Data Mine';

-- 3) Check dm_contact statistics
SELECT
  COUNT(*) AS total_contacts,
  COUNT(*) FILTER (WHERE is_pulled = false) AS available,
  COUNT(*) FILTER (WHERE is_pulled = true)  AS still_pulled
FROM dm_contact;
