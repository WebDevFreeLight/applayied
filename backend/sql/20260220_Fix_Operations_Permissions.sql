-- ============================================================
-- FIX: Operations Permissions Cleanup & Correct Seeding
-- PostgreSQL
-- ============================================================
-- Problems:
--   1. Old duplicate entries at 51400-51965 (from first SQL run)
--   2. 51020 = visits function (NOT tickets) — tickets component missing
--   3. 51030 = contracts entry (NOT dashboard) — dashboard component missing
--   4. 51021-51025 orphaned (tickets functions pointing to 51020 = wrong parent)
--   5. 51031-51032 orphaned (dashboard functions pointing to 51030 = wrong parent)
--
-- Fix plan:
--   Step 1: Delete old duplicates (51400-51965) + their role assignments
--   Step 2: Delete orphaned function entries (51021-51025, 51031-51032)
--   Step 3: Create Tickets at 51200 + functions 51201-51205
--   Step 4: Create Dashboard at 51300 + functions 51301-51302
--   Step 5: Assign Tickets + Dashboard to roles 1 and 11
-- ============================================================

BEGIN;

-- ──────────────────────────────────────────────────────────────
-- STEP 1: Remove old duplicate entries (51400-51965) + role assignments
-- ──────────────────────────────────────────────────────────────
DELETE FROM sys_role_menu WHERE menu_id IN (
  51400, 51401, 51402,
  51500, 51501, 51502, 51503, 51504, 51505,
  51600, 51601, 51602, 51603, 51604, 51605,
  51700, 51701, 51702, 51703, 51704, 51705,
  51800, 51801, 51802, 51803, 51804, 51805,
  51900, 51901, 51902, 51903, 51904, 51905,
  51960, 51961, 51962, 51963, 51964, 51965
);

DELETE FROM sys_menu WHERE menuid IN (
  51400, 51401, 51402,
  51500, 51501, 51502, 51503, 51504, 51505,
  51600, 51601, 51602, 51603, 51604, 51605,
  51700, 51701, 51702, 51703, 51704, 51705,
  51800, 51801, 51802, 51803, 51804, 51805,
  51900, 51901, 51902, 51903, 51904, 51905,
  51960, 51961, 51962, 51963, 51964, 51965
);

-- ──────────────────────────────────────────────────────────────
-- STEP 2: Remove orphaned function entries (wrong parent)
-- ──────────────────────────────────────────────────────────────
DELETE FROM sys_role_menu WHERE menu_id IN (
  51021, 51022, 51023, 51024, 51025,
  51031, 51032
);

DELETE FROM sys_menu WHERE menuid IN (
  51021, 51022, 51023, 51024, 51025,
  51031, 51032
);

-- ──────────────────────────────────────────────────────────────
-- STEP 3: Create Tickets (51200) — free ID range
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51200, 'Tickets', 51000, 2, 'tickets', NULL, '1', '0', 'C', '0', '0', 'operations:tickets:list', 'file-text', 'admin', NOW(), 'Operations Tickets')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51201, 'Query Tickets',  51200, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:list',   '#', 'admin', NOW(), 'Query Tickets'),
(51202, 'View Ticket',    51200, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:view',   '#', 'admin', NOW(), 'View Ticket'),
(51203, 'Add Ticket',     51200, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:add',    '#', 'admin', NOW(), 'Add Ticket'),
(51204, 'Edit Ticket',    51200, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:edit',   '#', 'admin', NOW(), 'Edit Ticket'),
(51205, 'Delete Ticket',  51200, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:delete', '#', 'admin', NOW(), 'Delete Ticket')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 4: Create Dashboard (51300) — free ID range
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51300, 'Operations Dashboard', 51000, 3, 'dashboard', NULL, '1', '0', 'C', '0', '0', 'operations:dashboard:list', 'bar-chart', 'admin', NOW(), 'Operations Dashboard')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51301, 'View Dashboard',        51300, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:dashboard:list', '#', 'admin', NOW(), 'View Dashboard'),
(51302, 'View Dashboard Detail', 51300, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:dashboard:view', '#', 'admin', NOW(), 'View Dashboard Detail')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 5: Assign Tickets + Dashboard to roles 1 and 11
-- ──────────────────────────────────────────────────────────────

-- Admin (role 1)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(1, 51200), (1, 51201), (1, 51202), (1, 51203), (1, 51204), (1, 51205),
(1, 51300), (1, 51301), (1, 51302)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Operation Manager (role 11)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(11, 51200), (11, 51201), (11, 51202), (11, 51203), (11, 51204), (11, 51205),
(11, 51300), (11, 51301), (11, 51302)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 6: Ensure role 11 has ALL operations modules
-- (customers, cleaning, energy, maintenance, noc, waste were
--  already created at 51040-51095 — just ensure role assignment)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
-- Customers
(11, 51040), (11, 51041), (11, 51042), (11, 51043), (11, 51044), (11, 51045),
-- Cleaning
(11, 51050), (11, 51051), (11, 51052), (11, 51053), (11, 51054), (11, 51055),
-- Energy
(11, 51060), (11, 51061), (11, 51062), (11, 51063), (11, 51064), (11, 51065),
-- Maintenance
(11, 51070), (11, 51071), (11, 51072), (11, 51073), (11, 51074), (11, 51075),
-- NOC
(11, 51080), (11, 51081), (11, 51082), (11, 51083), (11, 51084), (11, 51085),
-- Waste
(11, 51090), (11, 51091), (11, 51092), (11, 51093), (11, 51094), (11, 51095)
ON CONFLICT (role_id, menu_id) DO NOTHING;

COMMIT;

-- ══════════════════════════════════════════════════════════════
-- VERIFY: Run after COMMIT to confirm everything is clean
-- ══════════════════════════════════════════════════════════════

-- 1) All operations menu entries (should be clean, no duplicates)
SELECT menuid, menuname, parentid, menutype, perms, ordernum
FROM sys_menu
WHERE parentid = 51000 OR menuid = 51000
ORDER BY menuid;

-- 2) All role 11 operations permissions
SELECT rm.role_id, m.menuid, m.menuname, m.menutype, m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 11 AND m.perms LIKE 'operations:%'
ORDER BY m.menuid;

-- 3) Confirm old duplicates are gone
SELECT menuid, menuname FROM sys_menu
WHERE menuid BETWEEN 51400 AND 51999;
