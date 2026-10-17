-- ====================================================
-- Seed Permissions for New Modules
-- Sales, Operations, Contracts
-- Generated: 2026-02-08
-- ====================================================
-- Safe to re-run: uses ON CONFLICT DO NOTHING
-- Run via: docker compose exec postgres psql -U mahamadmin -d MahamDB -f /path/to/this.sql
-- Or paste into psql shell directly
-- ====================================================

-- ====================================================
-- SECTION 1: SALES MODULE (IDs: 50000-50499)
-- ====================================================

-- Sales Directory (top-level)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (50000, 'Sales Management', 0, 30, 'sales', NULL, 1, 0, 'M', '0', '0', '', 'shopping-bag', 'admin', NOW(), 'Sales Management Module')
ON CONFLICT ("menuId") DO NOTHING;

-- Sales Leads (Menu)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (50100, 'Leads Management', 50000, 1, 'leads', NULL, 1, 0, 'C', '0', '0', 'sales:leads:list', 'target', 'admin', NOW(), 'Sales Leads Management')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(50101, 'Query Leads', 50100, 1, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:list', '#', 'admin', NOW(), 'Query Leads'),
(50102, 'View Lead', 50100, 2, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:view', '#', 'admin', NOW(), 'View Lead'),
(50103, 'Edit Lead', 50100, 3, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:edit', '#', 'admin', NOW(), 'Edit Lead')
ON CONFLICT ("menuId") DO NOTHING;

-- Sales Deals (Menu)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (50200, 'Deals Management', 50000, 2, 'deals', NULL, 1, 0, 'C', '0', '0', 'sales:deals:list', 'handshake', 'admin', NOW(), 'Sales Deals Management')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(50201, 'Query Deals', 50200, 1, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:list', '#', 'admin', NOW(), 'Query Deals'),
(50202, 'View Deal', 50200, 2, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:view', '#', 'admin', NOW(), 'View Deal'),
(50203, 'Add Deal', 50200, 3, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:add', '#', 'admin', NOW(), 'Add Deal'),
(50204, 'Edit Deal', 50200, 4, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:edit', '#', 'admin', NOW(), 'Edit Deal'),
(50205, 'Delete Deal', 50200, 5, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:delete', '#', 'admin', NOW(), 'Delete Deal')
ON CONFLICT ("menuId") DO NOTHING;

-- Sales Visits (Menu)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (50300, 'Sales Visits', 50000, 3, 'visits', NULL, 1, 0, 'C', '0', '0', 'sales:visits:list', 'map-pin', 'admin', NOW(), 'Sales Visits')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(50301, 'Query Visits', 50300, 1, '', NULL, 1, 0, 'F', '0', '0', 'sales:visits:list', '#', 'admin', NOW(), 'Query Visits'),
(50302, 'Add Visit', 50300, 2, '', NULL, 1, 0, 'F', '0', '0', 'sales:visits:add', '#', 'admin', NOW(), 'Add Visit')
ON CONFLICT ("menuId") DO NOTHING;

-- Sales Dashboard (Menu)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (50400, 'Sales Dashboard', 50000, 4, 'dashboard', NULL, 1, 0, 'C', '0', '0', 'sales:dashboard:view', 'chart-bar', 'admin', NOW(), 'Sales Dashboard')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(50401, 'View Dashboard', 50400, 1, '', NULL, 1, 0, 'F', '0', '0', 'sales:dashboard:view', '#', 'admin', NOW(), 'View Dashboard')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 2: OPERATIONS MODULE (IDs: 51000-51499)
-- ====================================================

-- Operations Directory (top-level)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (51000, 'Operations Management', 0, 31, 'operations', NULL, 1, 0, 'M', '0', '0', '', 'gear', 'admin', NOW(), 'Operations Management Module')
ON CONFLICT ("menuId") DO NOTHING;

-- Operations Site Visits (Menu)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (51100, 'Site Visits', 51000, 1, 'visits', NULL, 1, 0, 'C', '0', '0', 'operations:visits:list', 'map-pin', 'admin', NOW(), 'Operations Site Visits')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(51101, 'Query Visits', 51100, 1, '', NULL, 1, 0, 'F', '0', '0', 'operations:visits:list', '#', 'admin', NOW(), 'Query Visits'),
(51102, 'View Visit', 51100, 2, '', NULL, 1, 0, 'F', '0', '0', 'operations:visits:view', '#', 'admin', NOW(), 'View Visit'),
(51103, 'Add Visit', 51100, 3, '', NULL, 1, 0, 'F', '0', '0', 'operations:visits:add', '#', 'admin', NOW(), 'Add Visit'),
(51104, 'Edit Visit', 51100, 4, '', NULL, 1, 0, 'F', '0', '0', 'operations:visits:edit', '#', 'admin', NOW(), 'Edit Visit'),
(51105, 'Delete Visit', 51100, 5, '', NULL, 1, 0, 'F', '0', '0', 'operations:visits:delete', '#', 'admin', NOW(), 'Delete Visit')
ON CONFLICT ("menuId") DO NOTHING;

-- Operations Tickets (Menu)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (51200, 'Tickets', 51000, 2, 'tickets', NULL, 1, 0, 'C', '0', '0', 'operations:tickets:list', 'file-text', 'admin', NOW(), 'Operations Tickets')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(51201, 'Query Tickets', 51200, 1, '', NULL, 1, 0, 'F', '0', '0', 'operations:tickets:list', '#', 'admin', NOW(), 'Query Tickets'),
(51202, 'View Ticket', 51200, 2, '', NULL, 1, 0, 'F', '0', '0', 'operations:tickets:view', '#', 'admin', NOW(), 'View Ticket'),
(51203, 'Add Ticket', 51200, 3, '', NULL, 1, 0, 'F', '0', '0', 'operations:tickets:add', '#', 'admin', NOW(), 'Add Ticket'),
(51204, 'Edit Ticket', 51200, 4, '', NULL, 1, 0, 'F', '0', '0', 'operations:tickets:edit', '#', 'admin', NOW(), 'Edit Ticket'),
(51205, 'Delete Ticket', 51200, 5, '', NULL, 1, 0, 'F', '0', '0', 'operations:tickets:delete', '#', 'admin', NOW(), 'Delete Ticket')
ON CONFLICT ("menuId") DO NOTHING;

-- Operations Leads (Menu)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (51300, 'Operations Leads', 51000, 3, 'leads', NULL, 1, 0, 'C', '0', '0', 'operations:leads:list', 'target', 'admin', NOW(), 'Operations Leads')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(51301, 'Query Leads', 51300, 1, '', NULL, 1, 0, 'F', '0', '0', 'operations:leads:list', '#', 'admin', NOW(), 'Query Leads'),
(51302, 'View Lead', 51300, 2, '', NULL, 1, 0, 'F', '0', '0', 'operations:leads:view', '#', 'admin', NOW(), 'View Lead')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 3: CONTRACTS MODULE (IDs: 52000-52499)
-- ====================================================

-- Contracts Directory (top-level)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (52000, 'Contracts Management', 0, 32, 'contracts', NULL, 1, 0, 'M', '0', '0', '', 'file-text', 'admin', NOW(), 'Contracts Management Module')
ON CONFLICT ("menuId") DO NOTHING;

-- Contracts List (Menu)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (52100, 'Contracts List', 52000, 1, 'list', NULL, 1, 0, 'C', '0', '0', 'contracts:contract:list', 'file-text', 'admin', NOW(), 'Contracts List')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(52101, 'Query Contracts', 52100, 1, '', NULL, 1, 0, 'F', '0', '0', 'contracts:contract:list', '#', 'admin', NOW(), 'Query Contracts'),
(52102, 'View Contract', 52100, 2, '', NULL, 1, 0, 'F', '0', '0', 'contracts:contract:view', '#', 'admin', NOW(), 'View Contract'),
(52103, 'Add Contract', 52100, 3, '', NULL, 1, 0, 'F', '0', '0', 'contracts:contract:add', '#', 'admin', NOW(), 'Add Contract'),
(52104, 'Edit Contract', 52100, 4, '', NULL, 1, 0, 'F', '0', '0', 'contracts:contract:edit', '#', 'admin', NOW(), 'Edit Contract'),
(52105, 'Delete Contract', 52100, 5, '', NULL, 1, 0, 'F', '0', '0', 'contracts:contract:delete', '#', 'admin', NOW(), 'Delete Contract')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 4: ASSIGN ALL NEW MENUS TO ADMIN ROLE (role_id = 1)
-- ====================================================
-- This ensures the admin user sees all new menu items

INSERT INTO sys_role_menu ("role_id", "menu_id")
SELECT 1, m."menuId"
FROM sys_menu m
WHERE m."menuId" BETWEEN 50000 AND 52999
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm."role_id" = 1 AND rm."menu_id" = m."menuId"
  );

-- ====================================================
-- VERIFICATION: Run these queries to check
-- ====================================================
-- SELECT "menuId", "menuName", "perms", "menuType" FROM sys_menu WHERE "menuId" BETWEEN 50000 AND 52999 ORDER BY "menuId";
-- SELECT COUNT(*) as admin_menu_count FROM sys_role_menu WHERE "role_id" = 1 AND "menu_id" BETWEEN 50000 AND 52999;
