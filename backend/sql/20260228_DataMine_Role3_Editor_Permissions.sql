-- ============================================================
-- Data Mine - Full Access for Role 3 (Editor)
-- PostgreSQL
-- ============================================================
-- Creates missing Data Mine page menus (52200-52600),
-- then assigns ALL Data Mine menus to Role 3 (Editor).
-- ============================================================

BEGIN;

-- ──────────────────────────────────────────────────────────────
-- STEP 1: Ensure Data Mine parent directory exists (52000)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52000, 'Data Mine', 0, 60, 'data-mine', NULL, '1', '0', 'M', '0', '0', '', 'database', 'admin', NOW(), 'Data Mine - Cross-Department Intelligence')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 2: Create missing page menus
-- ──────────────────────────────────────────────────────────────

-- Command Center (main data-mine page)
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52200, 'Command Center', 52000, 0, 'command-center', NULL, '1', '0', 'C', '0', '0', 'datamine:command:list', 'chart-bar', 'admin', NOW(), 'Data Mine Command Center')
ON CONFLICT (menuid) DO NOTHING;

-- Contacts / Universe (52100 already exists, ensure it's there)
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52100, 'Contacts', 52000, 1, 'universe', NULL, '1', '0', 'C', '0', '0', 'datamine:contacts:list', 'peoples', 'admin', NOW(), 'Data Mine Contacts / Universe')
ON CONFLICT (menuid) DO NOTHING;

-- Intelligence
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52300, 'Intelligence', 52000, 2, 'intelligence', NULL, '1', '0', 'C', '0', '0', 'datamine:intelligence:list', 'robot', 'admin', NOW(), 'AI-Powered Lead Scoring & Intelligence')
ON CONFLICT (menuid) DO NOTHING;

-- Pipeline
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52400, 'Pipeline', 52000, 3, 'pipeline', NULL, '1', '0', 'C', '0', '0', 'datamine:pipeline:list', 'flow-arrow', 'admin', NOW(), 'Full Customer Journey Pipeline')
ON CONFLICT (menuid) DO NOTHING;

-- Analytics
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52500, 'Analytics', 52000, 4, 'analytics', NULL, '1', '0', 'C', '0', '0', 'datamine:analytics:list', 'chart-line-up', 'admin', NOW(), 'Advanced Reports & Statistics')
ON CONFLICT (menuid) DO NOTHING;

-- Compliance
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52600, 'Compliance', 52000, 5, 'compliance', NULL, '1', '0', 'C', '0', '0', 'datamine:compliance:list', 'shield-check', 'admin', NOW(), 'Data Governance & PDPL Compliance')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 3: Function buttons for each page
-- ──────────────────────────────────────────────────────────────

-- Contacts buttons (52101-52105 already exist, ensure they're there)
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52101, 'Query Contacts',  52100, 1, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:list',   '#', 'admin', NOW(), 'Query Contacts'),
(52102, 'Import Contacts', 52100, 2, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:import', '#', 'admin', NOW(), 'Import Contacts'),
(52103, 'Export Contacts', 52100, 3, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:export', '#', 'admin', NOW(), 'Export Contacts'),
(52104, 'Pull to Leads',   52100, 4, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:pull',   '#', 'admin', NOW(), 'Pull Contacts to Leads'),
(52105, 'Delete Contacts', 52100, 5, '', NULL, '1', '0', 'F', '0', '0', 'datamine:contacts:delete', '#', 'admin', NOW(), 'Delete Contacts')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 4: Assign ALL Data Mine menus to Role 3 (Editor)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
-- Parent directory
(3, 52000),
-- Command Center
(3, 52200),
-- Contacts / Universe + buttons
(3, 52100), (3, 52101), (3, 52102), (3, 52103), (3, 52104), (3, 52105),
-- Intelligence
(3, 52300),
-- Pipeline
(3, 52400),
-- Analytics
(3, 52500),
-- Compliance
(3, 52600)
ON CONFLICT (role_id, menu_id) DO NOTHING;

COMMIT;

-- ══════════════════════════════════════════════════════════════
-- VERIFY: Run after COMMIT
-- ══════════════════════════════════════════════════════════════

-- 1) Check all Data Mine menu entries
SELECT menuid, menuname, parentid, menutype, perms, ordernum
FROM sys_menu
WHERE menuid BETWEEN 52000 AND 52600
ORDER BY menuid;

-- 2) Check Role 3 (Editor) assignments
SELECT rm.role_id, m.menuid, m.menuname, m.menutype, m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 3 AND m.menuid BETWEEN 52000 AND 52600
ORDER BY m.menuid;

-- 3) Verify permissions loaded for a specific Editor user (replace USER_ID)
-- SELECT DISTINCT m.perms
-- FROM sys_menu m
-- JOIN sys_role_menu rm ON m.menuid = rm.menu_id
-- JOIN sys_user_role ur ON rm.role_id = ur.role_id
-- WHERE ur.user_id = YOUR_USER_ID AND m.perms LIKE 'datamine:%'
-- ORDER BY m.perms;
