-- ====================================================
-- Marketing Module - Qualified Customers & Reports Permissions (PostgreSQL)
-- Date: 2026-02-07
--
-- RUN THIS SQL on your PostgreSQL database (MahamDB)
-- BEFORE deploying the frontend, so the sidebar shows the new pages
-- ====================================================

-- ====================================================
-- STEP 1: Add "Qualified Customers" page menu
-- Parent = Marketing module (ID: 2007854564761542656)
-- menuType = 'C' (Component/Page - shows in sidebar)
-- ====================================================
INSERT INTO sys_menu (
    "menuId", "menuName", "parentId", "orderNum", "path", "component",
    "isFrame", "isCache", "menuType", "visible", "status", "perms",
    "icon", "create_by", "create_time", "remark"
)
VALUES (
    2007854564890000001,                    -- Unique menu ID
    'Qualified Customers',                   -- Menu name (shows in admin panel)
    2007854564761542656,                     -- Parent = Marketing module
    7,                                       -- Order (after Leads which is ~4)
    'qualified-customers',                   -- Route path
    'marketing/qualified-customers/index',   -- Component path
    1, 0,                                    -- isFrame=1, isCache=0
    'C',                                     -- C = Component (sidebar page)
    '0',                                     -- visible = 0 (show)
    '0',                                     -- status = 0 (active)
    'marketing:qualified:list',              -- Permission string
    'check-circle',                          -- Icon
    'admin',                                 -- Created by
    NOW(),                                   -- Created time
    'Display-only page showing qualified leads/customers'
)
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- STEP 2: Add "Reports" page menu
-- Parent = Marketing module (ID: 2007854564761542656)
-- menuType = 'C' (Component/Page - shows in sidebar)
-- ====================================================
INSERT INTO sys_menu (
    "menuId", "menuName", "parentId", "orderNum", "path", "component",
    "isFrame", "isCache", "menuType", "visible", "status", "perms",
    "icon", "create_by", "create_time", "remark"
)
VALUES (
    2007854564890000002,                     -- Unique menu ID
    'Employee Reports',                      -- Menu name
    2007854564761542656,                     -- Parent = Marketing module
    8,                                       -- Order (after Qualified Customers)
    'reports',                               -- Route path
    'marketing/reports/index',               -- Component path
    1, 0,                                    -- isFrame=1, isCache=0
    'C',                                     -- C = Component (sidebar page)
    '0',                                     -- visible = 0 (show)
    '0',                                     -- status = 0 (active)
    'marketing:reports:view',                -- Permission string
    'chart-bar',                             -- Icon
    'admin',                                 -- Created by
    NOW(),                                   -- Created time
    'Marketing employee performance reports with charts'
)
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- STEP 3: Assign BOTH permissions to ALL roles that already have marketing:leads:list
-- (This means any manager/user who can see Leads will also see these new pages)
-- ====================================================

-- Give "Qualified Customers" to roles that have marketing:leads:list
INSERT INTO sys_role_menu ("role_Id", "menu_Id")
SELECT DISTINCT rm."role_Id", 2007854564890000001
FROM sys_role_menu rm
JOIN sys_menu m ON rm."menu_Id" = m."menuId"
WHERE m."perms" = 'marketing:leads:list'
ON CONFLICT DO NOTHING;

-- Give "Reports" to roles that have marketing:employees:list
INSERT INTO sys_role_menu ("role_Id", "menu_Id")
SELECT DISTINCT rm."role_Id", 2007854564890000002
FROM sys_role_menu rm
JOIN sys_menu m ON rm."menu_Id" = m."menuId"
WHERE m."perms" = 'marketing:employees:list'
ON CONFLICT DO NOTHING;

-- ALSO give "Reports" to roles that have marketing:leads:list (in case they don't have employees:list)
INSERT INTO sys_role_menu ("role_Id", "menu_Id")
SELECT DISTINCT rm."role_Id", 2007854564890000002
FROM sys_role_menu rm
JOIN sys_menu m ON rm."menu_Id" = m."menuId"
WHERE m."perms" = 'marketing:leads:list'
ON CONFLICT DO NOTHING;

-- ====================================================
-- STEP 4: Also assign to Admin role (role_id = 1) explicitly
-- ====================================================
INSERT INTO sys_role_menu ("role_Id", "menu_Id")
VALUES
    (1, 2007854564890000001),   -- Admin gets Qualified Customers
    (1, 2007854564890000002)    -- Admin gets Reports
ON CONFLICT DO NOTHING;

-- ====================================================
-- VERIFICATION - Run these to confirm it worked
-- ====================================================
SELECT '--- New Marketing Permissions Added ---' AS "Info";

SELECT "menuId", "menuName", "perms", "menuType", "status"
FROM sys_menu
WHERE "perms" IN ('marketing:qualified:list', 'marketing:reports:view')
ORDER BY "menuId";

SELECT '--- Roles with new permissions ---' AS "Info";

SELECT r."roleName", m."menuName", m."perms"
FROM sys_role r
JOIN sys_role_menu rm ON r."roleId" = rm."role_Id"
JOIN sys_menu m ON rm."menu_Id" = m."menuId"
WHERE m."perms" IN ('marketing:qualified:list', 'marketing:reports:view')
ORDER BY r."roleName", m."perms";

SELECT '--- All Marketing Permissions Summary ---' AS "Info";

SELECT "perms", "menuName", "menuType"
FROM sys_menu
WHERE "perms" LIKE 'marketing:%'
ORDER BY "perms";
