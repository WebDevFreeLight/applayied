-- ============================================================
-- Customer Assets Permissions for Operations Module
-- PostgreSQL
-- ============================================================
-- Adds three new pages under Operations (51000):
--   51400 = Asset Types
--   51410 = Asset Inventory
--   51420 = Asset Assignments
-- Assigns all permissions to roles 1 (Admin) and 11 (Operation Manager)
-- ============================================================

BEGIN;

-- ──────────────────────────────────────────────────────────────
-- STEP 1: Asset Types (51400)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51400, 'Asset Types', 51000, 10, 'asset-types', NULL, '1', '0', 'C', '0', '0', 'erp:customer-assets:type:list', 'tag', 'admin', NOW(), 'Customer Asset Types')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51401, 'Query Asset Types',  51400, 1, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:type:list',   '#', 'admin', NOW(), 'Query Asset Types'),
(51402, 'Add Asset Type',     51400, 2, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:type:add',    '#', 'admin', NOW(), 'Add Asset Type'),
(51403, 'Edit Asset Type',    51400, 3, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:type:edit',   '#', 'admin', NOW(), 'Edit Asset Type'),
(51404, 'Delete Asset Type',  51400, 4, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:type:delete', '#', 'admin', NOW(), 'Delete Asset Type')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 2: Asset Inventory (51410)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51410, 'Asset Inventory', 51000, 11, 'asset-inventory', NULL, '1', '0', 'C', '0', '0', 'erp:customer-assets:inventory:list', 'desktop', 'admin', NOW(), 'Customer Asset Inventory')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51411, 'Query Assets',  51410, 1, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:inventory:list',   '#', 'admin', NOW(), 'Query Assets'),
(51412, 'Add Asset',     51410, 2, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:inventory:add',    '#', 'admin', NOW(), 'Add Asset'),
(51413, 'Edit Asset',    51410, 3, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:inventory:edit',   '#', 'admin', NOW(), 'Edit Asset'),
(51414, 'Delete Asset',  51410, 4, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:inventory:delete', '#', 'admin', NOW(), 'Delete Asset')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 3: Asset Assignments (51420)
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51420, 'Asset Assignments', 51000, 12, 'asset-assignments', NULL, '1', '0', 'C', '0', '0', 'erp:customer-assets:assignment:list', 'handshake', 'admin', NOW(), 'Customer Asset Assignments')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51421, 'Query Assignments',  51420, 1, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:assignment:list',   '#', 'admin', NOW(), 'Query Assignments'),
(51422, 'Assign Asset',       51420, 2, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:assignment:assign', '#', 'admin', NOW(), 'Assign Asset to Customer'),
(51423, 'Return Asset',       51420, 3, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:assignment:return', '#', 'admin', NOW(), 'Return Asset from Customer'),
(51424, 'Manage Assignment',  51420, 4, '', NULL, '1', '0', 'F', '0', '0', 'erp:customer-assets:assignment:manage', '#', 'admin', NOW(), 'Mark Lost/Damaged')
ON CONFLICT (menuid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 4: Assign to Admin (role 1) and Operation Manager (role 11)
-- ──────────────────────────────────────────────────────────────

-- Admin (role 1)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
-- Asset Types
(1, 51400), (1, 51401), (1, 51402), (1, 51403), (1, 51404),
-- Asset Inventory
(1, 51410), (1, 51411), (1, 51412), (1, 51413), (1, 51414),
-- Asset Assignments
(1, 51420), (1, 51421), (1, 51422), (1, 51423), (1, 51424)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Operation Manager (role 11)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
-- Asset Types
(11, 51400), (11, 51401), (11, 51402), (11, 51403), (11, 51404),
-- Asset Inventory
(11, 51410), (11, 51411), (11, 51412), (11, 51413), (11, 51414),
-- Asset Assignments
(11, 51420), (11, 51421), (11, 51422), (11, 51423), (11, 51424)
ON CONFLICT (role_id, menu_id) DO NOTHING;

COMMIT;

-- ══════════════════════════════════════════════════════════════
-- VERIFY: Run after COMMIT
-- ══════════════════════════════════════════════════════════════

-- 1) Check new menu entries
SELECT menuid, menuname, parentid, menutype, perms, ordernum
FROM sys_menu
WHERE menuid BETWEEN 51400 AND 51424
ORDER BY menuid;

-- 2) Check role 11 customer-assets permissions
SELECT rm.role_id, m.menuid, m.menuname, m.menutype, m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 11 AND m.perms LIKE 'erp:customer-assets:%'
ORDER BY m.menuid;
