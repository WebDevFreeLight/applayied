-- ============================================================
-- Data Mine Contacts Permissions
-- PostgreSQL
-- ============================================================
-- Adds Data Mine parent directory (52000) if not exists,
-- then Data Mine Contacts page (52100) with function buttons.
-- Assigns to roles: 1 (Admin), 11 (Operation Manager), 12 (Marketing)
-- ============================================================

BEGIN;

-- ──────────────────────────────────────────────────────────────
-- STEP 1: Data Mine parent menu (52000) - Directory
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52000, 'Data Mine', 0, 60, 'data-mine', NULL, '1', '0', 'M', '0', '0', '', 'database', 'admin', NOW(), 'Data Mine - Cross-Department Intelligence')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 2: Data Mine Contacts page (52100) - Component
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52100, 'Contacts', 52000, 1, 'universe', NULL, '1', '0', 'C', '0', '0', 'datamine:contacts:list', 'peoples', 'admin', NOW(), 'Data Mine Contacts / Universe')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 3: Function buttons for Contacts
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52101, 'Query Contacts',  52100, 1, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:list',   '#', 'admin', NOW(), 'Query Contacts'),
(52102, 'Import Contacts', 52100, 2, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:import', '#', 'admin', NOW(), 'Import Contacts from Excel'),
(52103, 'Export Contacts', 52100, 3, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:export', '#', 'admin', NOW(), 'Export Contacts to Excel'),
(52104, 'Pull to Leads',   52100, 4, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:pull',   '#', 'admin', NOW(), 'Pull Contacts to Leads Pipeline'),
(52105, 'Delete Contacts', 52100, 5, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:delete', '#', 'admin', NOW(), 'Delete Contacts')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 4: Assign to roles
-- ──────────────────────────────────────────────────────────────

-- Admin (role 1) - full access
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(1, 52000), (1, 52100), (1, 52101), (1, 52102), (1, 52103), (1, 52104), (1, 52105)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Operation Manager (role 11) - full access
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(11, 52000), (11, 52100), (11, 52101), (11, 52102), (11, 52103), (11, 52104), (11, 52105)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Marketing (role 12) - full access
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(12, 52000), (12, 52100), (12, 52101), (12, 52102), (12, 52103), (12, 52104), (12, 52105)
ON CONFLICT (role_id, menu_id) DO NOTHING;

COMMIT;

-- ══════════════════════════════════════════════════════════════
-- VERIFY: Run after COMMIT
-- ══════════════════════════════════════════════════════════════

-- 1) Check new menu entries
SELECT menuid, menuname, parentid, menutype, perms, ordernum
FROM sys_menu
WHERE menuid BETWEEN 52000 AND 52105
ORDER BY menuid;

-- 2) Check role assignments
SELECT rm.role_id, m.menuid, m.menuname, m.menutype, m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE m.menuid BETWEEN 52000 AND 52105
ORDER BY rm.role_id, m.menuid;
