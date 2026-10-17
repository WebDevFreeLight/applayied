-- ====================================================
-- Seed Permissions for Operations Readiness Module
-- ====================================================
-- Continuous Live Tracking System (NO inspections)
-- Updated: 2026-02-25
-- ====================================================
-- Safe to re-run: uses ON CONFLICT DO UPDATE
-- IDs: 51100-51199 (Operations Readiness range)
-- ====================================================
-- Depends on: Operations parent menu (51000) from
--   Operations_Visits_Permissions.sql
-- ====================================================

-- ====================================================
-- STEP 0: CLEAN UP OLD INSPECTION ENTRIES
-- ====================================================
-- Remove old inspection menu items & role mappings
DELETE FROM sys_role_menu WHERE menu_id IN (51120, 51121, 51122, 51123, 51124);
DELETE FROM sys_menu WHERE menuid IN (51120, 51121, 51122, 51123, 51124);

-- Remove all existing readiness menus to re-insert cleanly
DELETE FROM sys_role_menu WHERE menu_id BETWEEN 51100 AND 51199;
DELETE FROM sys_menu WHERE menuid BETWEEN 51100 AND 51199;

-- ====================================================
-- SECTION 1: READINESS SUB-DIRECTORY (under Operations 51000)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (51100, 'Readiness Management', 51000, 5, 'readiness', NULL, '1', '0', 'M', '0', '0', '', 'target', 'admin', NOW(), 'Store Readiness & Handover Module', 'sidebar-menu-operations-readiness');

-- ====================================================
-- SECTION 2: READINESS DASHBOARD PAGE (ID: 51105)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (51105, 'Readiness Dashboard', 51100, 1, 'dashboard', NULL, '1', '0', 'C', '0', '0', 'operations:readiness:list', 'dashboard', 'admin', NOW(), 'Readiness Overview Dashboard', 'sidebar-menu-operations-readiness-dashboard');

-- ====================================================
-- SECTION 3: READINESS TRACKING PAGE (ID: 51110)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (51110, 'Readiness Tracking', 51100, 2, 'tracking', NULL, '1', '0', 'C', '0', '0', 'operations:readiness:list', 'chart-bar', 'admin', NOW(), 'Store Readiness Tracking & Status Updates', 'sidebar-menu-operations-readiness-tracking');

-- Tracking button permissions
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51111, 'Query Readiness', 51110, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:list', '#', 'admin', NOW(), 'View readiness tracking list'),
(51112, 'View Readiness Detail', 51110, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:view', '#', 'admin', NOW(), 'View readiness details'),
(51113, 'Initialize Readiness', 51110, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:add', '#', 'admin', NOW(), 'Initialize store readiness tracking'),
(51114, 'Manage Readiness Items', 51110, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:manage', '#', 'admin', NOW(), 'Update readiness item statuses'),
(51115, 'Delete Readiness', 51110, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:delete', '#', 'admin', NOW(), 'Delete readiness record');

-- ====================================================
-- SECTION 4: HANDOVER PAGE (ID: 51130)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (51130, 'Store Handover', 51100, 3, 'handover', NULL, '1', '0', 'C', '0', '0', 'operations:readiness:handover', 'transfer', 'admin', NOW(), 'Digital Store Handover', 'sidebar-menu-operations-readiness-handover');

-- Handover button permissions
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51131, 'Query Handovers', 51130, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:handover', '#', 'admin', NOW(), 'View handover list'),
(51132, 'Request Handover', 51130, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:manage', '#', 'admin', NOW(), 'Request store handover'),
(51133, 'Approve Handover', 51130, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:handover', '#', 'admin', NOW(), 'Approve / reject handover'),
(51134, 'Complete Handover', 51130, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:handover', '#', 'admin', NOW(), 'Complete handover process');

-- ====================================================
-- SECTION 5: READINESS CONFIG PAGE (ID: 51140)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (51140, 'Readiness Config', 51100, 4, 'config', NULL, '1', '0', 'C', '0', '0', 'operations:readiness:configure', 'settings', 'admin', NOW(), 'Readiness Categories & Checklists Configuration', 'sidebar-menu-operations-readiness-config');

-- Config button permissions
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51141, 'Manage Categories', 51140, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:configure', '#', 'admin', NOW(), 'Create/Edit/Delete readiness categories'),
(51142, 'Manage Checklists', 51140, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:configure', '#', 'admin', NOW(), 'Create/Edit/Delete checklists & items');

-- ====================================================
-- SECTION 6: DASHBOARD / SCORES (under Tracking page)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51151, 'View Scores', 51110, 6, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:list', '#', 'admin', NOW(), 'View store readiness scores & dashboard'),
(51152, 'View Reports', 51110, 7, '', NULL, '1', '0', 'F', '0', '0', 'operations:readiness:report', '#', 'admin', NOW(), 'View readiness reports');

-- ====================================================
-- SECTION 7: ASSIGN ALL READINESS PERMISSIONS TO ADMIN ROLE (role_id = 1)
-- ====================================================

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 51100 AND 51199
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 1 AND rm.menu_id = m.menuid
  );

-- ====================================================
-- SECTION 8: ASSIGN ALL READINESS PERMISSIONS TO ROLES 11 AND 12
-- ====================================================

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 11, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 51100 AND 51199
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 11 AND rm.menu_id = m.menuid
  );

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 12, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 51100 AND 51199
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 12 AND rm.menu_id = m.menuid
  );

-- ====================================================
-- VERIFICATION
-- ====================================================
SELECT menuid, menuname, menutype, perms, visible FROM sys_menu WHERE menuid BETWEEN 51100 AND 51199 ORDER BY menuid;
SELECT COUNT(*) as total_readiness_menus FROM sys_menu WHERE menuid BETWEEN 51100 AND 51199;
SELECT COUNT(*) as admin_readiness_menus FROM sys_role_menu WHERE role_id = 1 AND menu_id BETWEEN 51100 AND 51199;

-- ====================================================
-- DONE! Summary (Continuous Tracking System):
-- ====================================================
--   51100       Readiness Management (M - parent directory)
--   51105       Readiness Dashboard (C - page)
--   51110       Readiness Tracking (C - page)
--   51111-51115 Tracking permissions (F - list, view, add, manage, delete)
--   51130       Store Handover (C - page)
--   51131-51134 Handover permissions (F - list, request, approve, complete)
--   51140       Readiness Config (C - page)
--   51141-51142 Config permissions (F - categories, checklists)
--   51151-51152 Score & Report permissions (F - under tracking)
--
-- Permission strings used:
--   operations:readiness:list       - View tracking/lists/dashboard
--   operations:readiness:view       - View details
--   operations:readiness:add        - Initialize/create
--   operations:readiness:manage     - Update item statuses, request handover
--   operations:readiness:delete     - Delete records
--   operations:readiness:configure  - Manage categories/checklists
--   operations:readiness:handover   - Handover approve/reject/reopen
--   operations:readiness:report     - View reports
--
-- REMOVED (deprecated):
--   operations:readiness:inspect    - No longer used
--   operations:readiness:edit       - Replaced by manage
--   operations:readiness:score      - Merged into list
-- ====================================================
