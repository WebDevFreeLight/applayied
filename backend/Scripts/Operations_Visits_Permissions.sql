-- ====================================================
-- Seed Permissions for Operations Visits
-- ====================================================
-- Operations team can create/schedule, approve, reject,
-- reschedule, start, complete, cancel, and delete visits
-- Generated: 2026-02-12
-- ====================================================
-- FIXED: Column names use lowercase (SqlSugar PgSqlIsAutoToLower=true)
-- ====================================================
-- Safe to re-run: uses ON CONFLICT DO NOTHING
-- IDs: 51000-51099 (Operations Visits range)
-- ====================================================

-- ====================================================
-- SECTION 1: OPERATIONS MODULE PARENT (ID: 51000)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (51000, 'Operations Management', 0, 35, 'operations', NULL, '1', '0', 'M', '0', '0', '', 'clipboard-check', 'admin', NOW(), 'Operations Management Module', 'sidebar-menu-operations')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 2: SITE VISITS MANAGEMENT (ID: 51010)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (51010, 'Site Visits', 51000, 1, 'visits', NULL, '1', '0', 'C', '0', '0', 'operations:visits:list', 'map-pin', 'admin', NOW(), 'Operations Site Visits Management', 'sidebar-menu-operations-visits')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51011, 'Query Visits', 51010, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:list', '#', 'admin', NOW(), 'Query Site Visits List'),
(51012, 'Schedule Visit', 51010, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:add', '#', 'admin', NOW(), 'Create / Schedule New Visit'),
(51013, 'View Visit', 51010, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:view', '#', 'admin', NOW(), 'View Visit Details'),
(51014, 'Edit Visit', 51010, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:edit', '#', 'admin', NOW(), 'Edit Visit / Approve / Reject / Reschedule / Status Change'),
(51015, 'Delete Visit', 51010, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:delete', '#', 'admin', NOW(), 'Delete Visit Record'),
(51016, 'Approve Visit', 51010, 6, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:approve', '#', 'admin', NOW(), 'Approve Requested Visit'),
(51017, 'Reject Visit', 51010, 7, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:reject', '#', 'admin', NOW(), 'Reject Requested Visit'),
(51018, 'Reschedule Visit', 51010, 8, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:reschedule', '#', 'admin', NOW(), 'Reschedule Visit Date/Time'),
(51019, 'Update Visit Status', 51010, 9, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:status', '#', 'admin', NOW(), 'Start / Complete / Cancel Visit'),
(51020, 'View Dashboard', 51010, 10, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:dashboard', '#', 'admin', NOW(), 'View Visits Dashboard & Stats')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 3: ASSIGN ALL OPERATIONS VISIT PERMISSIONS TO ADMIN (role_id = 1)
-- ====================================================

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 51000 AND 51099
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 1 AND rm.menu_id = m.menuid
  );

-- ====================================================
-- VERIFICATION
-- ====================================================
SELECT menuid, menuname, menutype, perms FROM sys_menu WHERE menuid BETWEEN 51000 AND 51099 ORDER BY menuid;
SELECT COUNT(*) as total_ops_visit_perms FROM sys_menu WHERE menuid BETWEEN 51000 AND 51099;
SELECT COUNT(*) as admin_ops_visit_menus FROM sys_role_menu WHERE role_id = 1 AND menu_id BETWEEN 51000 AND 51099;

-- ====================================================
-- DONE! Total: 1 parent + 1 page + 10 button permissions
-- ====================================================
-- Summary:
--   51000       Operations Management (parent directory)
--   51010       Site Visits (page)
--   51011-51020 Visit Permissions (10 buttons)
--     - list, add, view, edit, delete
--     - approve, reject, reschedule, status, dashboard
-- ====================================================
