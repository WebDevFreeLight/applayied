-- ============================================================
-- Cleanup Orphan Leads Created from Data Mine (by phone match)
-- PostgreSQL
-- ============================================================
-- The pulled_lead_id FK was never set, so we match by phone.
-- Deletes leads + related records, resets dm_contact flags.
-- ============================================================

-- Delete activities for matched leads
DELETE FROM bp_lead_activity
WHERE lead_id IN (
    SELECT l.leadid FROM bp_lead l
    INNER JOIN dm_contact c ON REPLACE(l.ownerphone, ' ', '') = REPLACE(c.owner_phone, ' ', '')
);

-- Delete assignment logs for matched leads
DELETE FROM bp_lead_assignment_log
WHERE lead_id IN (
    SELECT l.leadid FROM bp_lead l
    INNER JOIN dm_contact c ON REPLACE(l.ownerphone, ' ', '') = REPLACE(c.owner_phone, ' ', '')
);

-- Delete the leads themselves
DELETE FROM bp_lead
WHERE leadid IN (
    SELECT l.leadid FROM bp_lead l
    INNER JOIN dm_contact c ON REPLACE(l.ownerphone, ' ', '') = REPLACE(c.owner_phone, ' ', '')
);

-- Reset all dm_contact pull flags
UPDATE dm_contact
SET is_pulled      = false,
    pulled_at      = NULL,
    pulled_by      = NULL,
    pulled_lead_id = NULL,
    update_time    = NOW()
WHERE is_pulled = true;

-- ══════════════════════════════════════════════════════════════
-- VERIFY
-- ══════════════════════════════════════════════════════════════
SELECT COUNT(*) AS remaining_matched_leads
FROM bp_lead l
WHERE EXISTS (
    SELECT 1 FROM dm_contact c
    WHERE REPLACE(c.owner_phone, ' ', '') = REPLACE(l.ownerphone, ' ', '')
);

SELECT
  COUNT(*) AS total_contacts,
  COUNT(*) FILTER (WHERE is_pulled = false) AS available,
  COUNT(*) FILTER (WHERE is_pulled = true)  AS still_pulled
FROM dm_contact;

SELECT COUNT(*) AS total_leads FROM bp_lead;
