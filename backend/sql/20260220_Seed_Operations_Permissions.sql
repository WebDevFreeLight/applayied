-- ============================================================
-- Seed: Operations Module Permissions
-- PostgreSQL
-- ============================================================
-- Actual DB state:
--   51000  Operations Management (Module M) — exists, assigned to role 11
--   51010  Site Visits (Component C)        — exists, assigned to role 11
--          + function entries for visits    — exist, assigned to role 11
--
-- This script adds 8 new sub-modules under Operations (51000):
--   51020  Tickets            (51021-51025)
--   51030  Dashboard          (51031-51032)
--   51040  Customers          (51041-51045)
--   51050  Cleaning           (51051-51055)
--   51060  Energy             (51061-51065)
--   51070  Maintenance        (51071-51075)
--   51080  NOC                (51081-51085)
--   51090  Waste              (51091-51095)
-- ============================================================

-- ██████████████████████████████████████████████████████████████
-- PRE-CHECK: Run these SELECTs FIRST to verify current DB state
-- ██████████████████████████████████████████████████████████████

-- 1) All existing operations menu entries
SELECT menuid, menuname, parentid, menutype, perms, ordernum
FROM sys_menu
WHERE parentid = 51000 OR menuid = 51000
ORDER BY menuid;

-- 2) Check if any of the new IDs (51020-51095) already exist
SELECT menuid, menuname, perms
FROM sys_menu
WHERE menuid BETWEEN 51020 AND 51095
ORDER BY menuid;

-- 3) What role 11 currently has for operations
SELECT rm.role_id, m.menuid, m.menuname, m.menutype, m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 11 AND m.menuid BETWEEN 51000 AND 51099
ORDER BY m.menuid;

-- 4) All permissions currently returned for role 11
SELECT DISTINCT m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 11 AND m.perms LIKE 'operations:%'
ORDER BY m.perms;

-- ██████████████████████████████████████████████████████████████
-- If the checks above look good, run everything below
-- ██████████████████████████████████████████████████████████████

BEGIN;

-- ──────────────────────────────────────────────────────────────
-- 1. TICKETS (51020)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51020, 'Tickets', 51000, 2, 'tickets', NULL, '1', '0', 'C', '0', '0', 'operations:tickets:list', 'file-text', 'admin', NOW(), 'Operations Tickets')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51021, 'Query Tickets',  51020, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:list',   '#', 'admin', NOW(), 'Query Tickets'),
(51022, 'View Ticket',    51020, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:view',   '#', 'admin', NOW(), 'View Ticket'),
(51023, 'Add Ticket',     51020, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:add',    '#', 'admin', NOW(), 'Add Ticket'),
(51024, 'Edit Ticket',    51020, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:edit',   '#', 'admin', NOW(), 'Edit Ticket'),
(51025, 'Delete Ticket',  51020, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:delete', '#', 'admin', NOW(), 'Delete Ticket')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- 2. OPERATIONS DASHBOARD (51030)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51030, 'Operations Dashboard', 51000, 3, 'dashboard', NULL, '1', '0', 'C', '0', '0', 'operations:dashboard:list', 'bar-chart', 'admin', NOW(), 'Operations Dashboard')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51031, 'View Dashboard',        51030, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:dashboard:list', '#', 'admin', NOW(), 'View Dashboard'),
(51032, 'View Dashboard Detail', 51030, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:dashboard:view', '#', 'admin', NOW(), 'View Dashboard Detail')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- 3. OPERATIONS CUSTOMERS (51040)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51040, 'Operations Customers', 51000, 4, 'customers', NULL, '1', '0', 'C', '0', '0', 'operations:customers:list', 'users', 'admin', NOW(), 'Operations Customers')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51041, 'Query Customers',  51040, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:customers:list',   '#', 'admin', NOW(), 'Query Customers'),
(51042, 'View Customer',    51040, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:customers:view',   '#', 'admin', NOW(), 'View Customer'),
(51043, 'Add Customer',     51040, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:customers:add',    '#', 'admin', NOW(), 'Add Customer'),
(51044, 'Edit Customer',    51040, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:customers:edit',   '#', 'admin', NOW(), 'Edit Customer'),
(51045, 'Delete Customer',  51040, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:customers:delete', '#', 'admin', NOW(), 'Delete Customer')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- 4. CLEANING (51050)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51050, 'Cleaning', 51000, 5, 'cleaning', NULL, '1', '0', 'C', '0', '0', 'operations:cleaning:list', 'spray-can', 'admin', NOW(), 'Cleaning Operations')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51051, 'Query Cleaning Tasks',  51050, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:cleaning:list',   '#', 'admin', NOW(), 'Query Cleaning Tasks'),
(51052, 'View Cleaning Task',    51050, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:cleaning:view',   '#', 'admin', NOW(), 'View Cleaning Task'),
(51053, 'Add Cleaning Task',     51050, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:cleaning:add',    '#', 'admin', NOW(), 'Add Cleaning Task'),
(51054, 'Edit Cleaning Task',    51050, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:cleaning:edit',   '#', 'admin', NOW(), 'Edit Cleaning Task'),
(51055, 'Delete Cleaning Task',  51050, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:cleaning:delete', '#', 'admin', NOW(), 'Delete Cleaning Task')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- 5. ENERGY (51060)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51060, 'Energy', 51000, 6, 'energy', NULL, '1', '0', 'C', '0', '0', 'operations:energy:list', 'zap', 'admin', NOW(), 'Energy Management')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51061, 'Query Energy Data',   51060, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:energy:list',   '#', 'admin', NOW(), 'Query Energy Data'),
(51062, 'View Energy Detail',  51060, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:energy:view',   '#', 'admin', NOW(), 'View Energy Detail'),
(51063, 'Add Energy Record',   51060, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:energy:add',    '#', 'admin', NOW(), 'Add Energy Record'),
(51064, 'Edit Energy Record',  51060, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:energy:edit',   '#', 'admin', NOW(), 'Edit Energy Record'),
(51065, 'Delete Energy Record',51060, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:energy:delete', '#', 'admin', NOW(), 'Delete Energy Record')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- 6. MAINTENANCE (51070)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51070, 'Maintenance', 51000, 7, 'maintenance', NULL, '1', '0', 'C', '0', '0', 'operations:maintenance:list', 'wrench', 'admin', NOW(), 'Maintenance Management')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51071, 'Query Work Orders',  51070, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:maintenance:list',   '#', 'admin', NOW(), 'Query Work Orders'),
(51072, 'View Work Order',    51070, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:maintenance:view',   '#', 'admin', NOW(), 'View Work Order'),
(51073, 'Add Work Order',     51070, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:maintenance:add',    '#', 'admin', NOW(), 'Add Work Order'),
(51074, 'Edit Work Order',    51070, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:maintenance:edit',   '#', 'admin', NOW(), 'Edit Work Order'),
(51075, 'Delete Work Order',  51070, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:maintenance:delete', '#', 'admin', NOW(), 'Delete Work Order')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- 7. NOC - Network Operations Center (51080)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51080, 'NOC', 51000, 8, 'noc', NULL, '1', '0', 'C', '0', '0', 'operations:noc:list', 'monitor', 'admin', NOW(), 'Network Operations Center')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51081, 'Query NOC Systems',  51080, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:noc:list',   '#', 'admin', NOW(), 'Query NOC Systems'),
(51082, 'View NOC Detail',    51080, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:noc:view',   '#', 'admin', NOW(), 'View NOC Detail'),
(51083, 'Add NOC Alert',      51080, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:noc:add',    '#', 'admin', NOW(), 'Add NOC Alert'),
(51084, 'Edit NOC Alert',     51080, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:noc:edit',   '#', 'admin', NOW(), 'Edit NOC Alert'),
(51085, 'Delete NOC Alert',   51080, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:noc:delete', '#', 'admin', NOW(), 'Delete NOC Alert')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- 8. WASTE MANAGEMENT (51090)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51090, 'Waste Management', 51000, 9, 'waste', NULL, '1', '0', 'C', '0', '0', 'operations:waste:list', 'trash-2', 'admin', NOW(), 'Waste Management')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51091, 'Query Waste Data',   51090, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:waste:list',   '#', 'admin', NOW(), 'Query Waste Data'),
(51092, 'View Waste Detail',  51090, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:waste:view',   '#', 'admin', NOW(), 'View Waste Detail'),
(51093, 'Add Waste Record',   51090, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:waste:add',    '#', 'admin', NOW(), 'Add Waste Record'),
(51094, 'Edit Waste Record',  51090, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:waste:edit',   '#', 'admin', NOW(), 'Edit Waste Record'),
(51095, 'Delete Waste Record',51090, 5, '', NULL, '1', '0', 'F', '0', '0', 'operations:waste:delete', '#', 'admin', NOW(), 'Delete Waste Record')
ON CONFLICT (menuid) DO NOTHING;

-- ══════════════════════════════════════════════════════════════
-- ASSIGN ALL NEW PERMISSIONS TO ADMIN ROLE (role_id = 1)
-- ══════════════════════════════════════════════════════════════
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
-- Tickets
(1, 51020), (1, 51021), (1, 51022), (1, 51023), (1, 51024), (1, 51025),
-- Dashboard
(1, 51030), (1, 51031), (1, 51032),
-- Customers
(1, 51040), (1, 51041), (1, 51042), (1, 51043), (1, 51044), (1, 51045),
-- Cleaning
(1, 51050), (1, 51051), (1, 51052), (1, 51053), (1, 51054), (1, 51055),
-- Energy
(1, 51060), (1, 51061), (1, 51062), (1, 51063), (1, 51064), (1, 51065),
-- Maintenance
(1, 51070), (1, 51071), (1, 51072), (1, 51073), (1, 51074), (1, 51075),
-- NOC
(1, 51080), (1, 51081), (1, 51082), (1, 51083), (1, 51084), (1, 51085),
-- Waste
(1, 51090), (1, 51091), (1, 51092), (1, 51093), (1, 51094), (1, 51095)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- ══════════════════════════════════════════════════════════════
-- ASSIGN ALL NEW PERMISSIONS TO ROLE 11 (operation-manager)
-- 51000 and 51010 (visits) already assigned — only adding new modules
-- ══════════════════════════════════════════════════════════════
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
-- Tickets
(11, 51020), (11, 51021), (11, 51022), (11, 51023), (11, 51024), (11, 51025),
-- Dashboard
(11, 51030), (11, 51031), (11, 51032),
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

-- ══════════════════════════════════════════════════════════════
-- VERIFICATION
-- ══════════════════════════════════════════════════════════════

-- All operations menu entries
SELECT menuid, menuname, parentid, menutype, perms, ordernum
FROM sys_menu
WHERE menuid BETWEEN 51000 AND 51099
ORDER BY menuid;

-- Role 11 operations assignments
SELECT rm.role_id, m.menuid, m.menuname, m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 11 AND m.menuid BETWEEN 51000 AND 51099
ORDER BY m.menuid;

COMMIT;
