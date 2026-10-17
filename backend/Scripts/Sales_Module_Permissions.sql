-- ====================================================
-- Seed Permissions for Sales Module
-- Leads, Deals, Visit Requests, Dashboard, Reports
-- Generated: 2026-02-12
-- ====================================================
-- Safe to re-run: uses ON CONFLICT DO NOTHING
-- Run via: docker compose exec postgres psql -U mahamadmin -d MahamDB -f /path/to/this.sql
-- Or paste into psql shell directly
-- IDs: 50000-50999
-- ====================================================

-- ====================================================
-- SECTION 1: SALES MODULE PARENT (ID: 50000)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark", "menuName_key")
VALUES (50000, 'Sales Management', 0, 30, 'sales', NULL, 1, 0, 'M', '0', '0', '', 'shopping-bag', 'admin', NOW(), 'Sales Management Module', 'sidebar-menu-sales')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 2: SALES DASHBOARD (ID: 50050)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark", "menuName_key")
VALUES (50050, 'Sales Dashboard', 50000, 1, 'dashboard', NULL, 1, 0, 'C', '0', '0', 'sales:dashboard:view', 'chart-bar', 'admin', NOW(), 'Sales Dashboard & KPIs', 'sidebar-menu-sales-dashboard')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(50051, 'View Dashboard', 50050, 1, '', NULL, 1, 0, 'F', '0', '0', 'sales:dashboard:view', '#', 'admin', NOW(), 'View Sales Dashboard'),
(50052, 'View Revenue Reports', 50050, 2, '', NULL, 1, 0, 'F', '0', '0', 'sales:dashboard:revenue', '#', 'admin', NOW(), 'View Revenue Reports'),
(50053, 'View Performance Metrics', 50050, 3, '', NULL, 1, 0, 'F', '0', '0', 'sales:dashboard:performance', '#', 'admin', NOW(), 'View Team Performance Metrics'),
(50054, 'Export Dashboard', 50050, 4, '', NULL, 1, 0, 'F', '0', '0', 'sales:dashboard:export', '#', 'admin', NOW(), 'Export Dashboard Reports')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 3: LEADS MANAGEMENT (ID: 50100)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark", "menuName_key")
VALUES (50100, 'Leads Management', 50000, 2, 'leads', NULL, 1, 0, 'C', '0', '0', 'sales:leads:list', 'target', 'admin', NOW(), 'Sales Leads Management', 'sidebar-menu-sales-leads')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(50101, 'Query Leads', 50100, 1, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:list', '#', 'admin', NOW(), 'Query Leads List'),
(50102, 'View Lead', 50100, 2, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:view', '#', 'admin', NOW(), 'View Lead Details'),
(50103, 'Edit Lead', 50100, 3, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:edit', '#', 'admin', NOW(), 'Edit Lead Information'),
(50104, 'Update Lead Status', 50100, 4, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:status', '#', 'admin', NOW(), 'Update Lead Status / Qualify'),
(50105, 'Add Lead Notes', 50100, 5, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:notes', '#', 'admin', NOW(), 'Add Notes to Leads'),
(50106, 'View Lead Activities', 50100, 6, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:activities', '#', 'admin', NOW(), 'View Lead Activity History'),
(50107, 'Export Leads', 50100, 7, '', NULL, 1, 0, 'F', '0', '0', 'sales:leads:export', '#', 'admin', NOW(), 'Export Leads Data')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 4: DEALS MANAGEMENT (ID: 50200)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark", "menuName_key")
VALUES (50200, 'Deals Management', 50000, 3, 'deals', NULL, 1, 0, 'C', '0', '0', 'sales:deals:list', 'handshake', 'admin', NOW(), 'Sales Deals Pipeline', 'sidebar-menu-sales-deals')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(50201, 'Query Deals', 50200, 1, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:list', '#', 'admin', NOW(), 'Query Deals List'),
(50202, 'View Deal', 50200, 2, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:view', '#', 'admin', NOW(), 'View Deal Details'),
(50203, 'Add Deal', 50200, 3, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:add', '#', 'admin', NOW(), 'Create New Deal'),
(50204, 'Edit Deal', 50200, 4, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:edit', '#', 'admin', NOW(), 'Edit Deal Information'),
(50205, 'Delete Deal', 50200, 5, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:delete', '#', 'admin', NOW(), 'Delete Deal'),
(50206, 'Update Deal Status', 50200, 6, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:status', '#', 'admin', NOW(), 'Update Deal Stage / Status'),
(50207, 'View Pipeline', 50200, 7, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:pipeline', '#', 'admin', NOW(), 'View Deals Pipeline Board'),
(50208, 'Add Deal Notes', 50200, 8, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:notes', '#', 'admin', NOW(), 'Add Notes to Deals'),
(50209, 'View Deal Activities', 50200, 9, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:activities', '#', 'admin', NOW(), 'View Deal Activity History'),
(50210, 'Export Deals', 50200, 10, '', NULL, 1, 0, 'F', '0', '0', 'sales:deals:export', '#', 'admin', NOW(), 'Export Deals Data')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 5: VISIT REQUESTS (ID: 50300)
-- Sales only creates visit REQUESTS; Operations approves
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark", "menuName_key")
VALUES (50300, 'Visit Requests', 50000, 4, 'visits', NULL, 1, 0, 'C', '0', '0', 'sales:visits:list', 'map-pin', 'admin', NOW(), 'Sales Visit Requests (Operations approves)', 'sidebar-menu-sales-visits')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(50301, 'Query Visit Requests', 50300, 1, '', NULL, 1, 0, 'F', '0', '0', 'sales:visits:list', '#', 'admin', NOW(), 'Query Own Visit Requests'),
(50302, 'Submit Visit Request', 50300, 2, '', NULL, 1, 0, 'F', '0', '0', 'sales:visits:add', '#', 'admin', NOW(), 'Submit New Visit Request'),
(50303, 'View Visit Details', 50300, 3, '', NULL, 1, 0, 'F', '0', '0', 'sales:visits:view', '#', 'admin', NOW(), 'View Visit Request Details'),
(50304, 'Cancel Own Request', 50300, 4, '', NULL, 1, 0, 'F', '0', '0', 'sales:visits:cancel', '#', 'admin', NOW(), 'Cancel Own Pending Visit Request'),
(50305, 'View Visit Status', 50300, 5, '', NULL, 1, 0, 'F', '0', '0', 'sales:visits:status', '#', 'admin', NOW(), 'View Visit Approval Status'),
(50306, 'View Visit Notes', 50300, 6, '', NULL, 1, 0, 'F', '0', '0', 'sales:visits:notes', '#', 'admin', NOW(), 'View Operations Notes on Visits')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 6: SALES REPORTS & ANALYTICS (ID: 50400)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark", "menuName_key")
VALUES (50400, 'Sales Reports', 50000, 5, 'reports', NULL, 1, 0, 'C', '0', '0', 'sales:reports:view', 'chart-line', 'admin', NOW(), 'Sales Reports & Analytics', 'sidebar-menu-sales-reports')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(50401, 'View Reports', 50400, 1, '', NULL, 1, 0, 'F', '0', '0', 'sales:reports:view', '#', 'admin', NOW(), 'View Sales Reports'),
(50402, 'Lead Conversion Report', 50400, 2, '', NULL, 1, 0, 'F', '0', '0', 'sales:reports:conversion', '#', 'admin', NOW(), 'Lead-to-Deal Conversion Report'),
(50403, 'Revenue Forecast', 50400, 3, '', NULL, 1, 0, 'F', '0', '0', 'sales:reports:forecast', '#', 'admin', NOW(), 'Revenue Forecast Report'),
(50404, 'Team Performance', 50400, 4, '', NULL, 1, 0, 'F', '0', '0', 'sales:reports:team', '#', 'admin', NOW(), 'Team Performance Report'),
(50405, 'Visit Success Report', 50400, 5, '', NULL, 1, 0, 'F', '0', '0', 'sales:reports:visits', '#', 'admin', NOW(), 'Visit Success Rate Report'),
(50406, 'Export Reports', 50400, 6, '', NULL, 1, 0, 'F', '0', '0', 'sales:reports:export', '#', 'admin', NOW(), 'Export Sales Reports')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 7: ASSIGN ALL SALES PERMISSIONS TO ADMIN ROLE (role_id = 1)
-- ====================================================
-- Ensures admin user sees all new menu items immediately

INSERT INTO sys_role_menu ("role_id", "menu_id")
SELECT 1, m."menuId"
FROM sys_menu m
WHERE m."menuId" BETWEEN 50000 AND 50999
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm."role_id" = 1 AND rm."menu_id" = m."menuId"
  );

-- ====================================================
-- SECTION 8: CREATE SALES ROLE (optional - role_id = 10)
-- Assign only sales-specific permissions to a "Sales" role
-- ====================================================
-- Uncomment below to create a dedicated Sales role:

-- INSERT INTO sys_role ("roleId", "roleName", "roleKey", "roleSort", "dataScope", "status", "create_by", "create_time", "remark")
-- VALUES (10, 'Sales Team', 'sales', 10, '1', '0', 'admin', NOW(), 'Sales team role - leads, deals, visit requests')
-- ON CONFLICT ("roleId") DO NOTHING;

-- INSERT INTO sys_role_menu ("role_id", "menu_id")
-- SELECT 10, m."menuId"
-- FROM sys_menu m
-- WHERE m."menuId" BETWEEN 50000 AND 50499
--   AND m."menuType" IN ('M', 'C', 'F')
--   AND m."perms" NOT LIKE '%delete%'
--   AND m."perms" NOT LIKE '%export%'
--   AND NOT EXISTS (
--     SELECT 1 FROM sys_role_menu rm
--     WHERE rm."role_id" = 10 AND rm."menu_id" = m."menuId"
--   );

-- ====================================================
-- VERIFICATION QUERIES
-- ====================================================
-- SELECT "menuId", "menuName", "menuType", "perms" FROM sys_menu WHERE "menuId" BETWEEN 50000 AND 50999 ORDER BY "menuId";
-- SELECT COUNT(*) as total_sales_perms FROM sys_menu WHERE "menuId" BETWEEN 50000 AND 50999;
-- SELECT COUNT(*) as admin_sales_menus FROM sys_role_menu WHERE "role_id" = 1 AND "menu_id" BETWEEN 50000 AND 50999;

-- ====================================================
-- DONE! Total: 1 parent + 5 pages + ~33 button permissions
-- ====================================================
-- Summary:
--   50000       Sales Management (parent directory)
--   50050-50054 Sales Dashboard (4 buttons)
--   50100-50107 Leads Management (7 buttons)
--   50200-50210 Deals Management (10 buttons)
--   50300-50306 Visit Requests (6 buttons)
--   50400-50406 Sales Reports (6 buttons)
-- ====================================================
