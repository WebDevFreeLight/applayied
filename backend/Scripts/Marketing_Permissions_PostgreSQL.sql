-- ====================================================
-- Marketing Module - Add Missing Permissions (PostgreSQL)
-- Date: 2026-01-07
-- ====================================================

-- STEP 1: Add Lead action buttons (under Leads menu ID: 2007854564853817344)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(2007854564853817345, 'View Lead', 2007854564853817344, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:view', '#', 'admin', NOW(), 'View lead details'),
(2007854564853817346, 'Add Lead', 2007854564853817344, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:add', '#', 'admin', NOW(), 'Create new lead'),
(2007854564853817347, 'Edit Lead', 2007854564853817344, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:edit', '#', 'admin', NOW(), 'Edit lead'),
(2007854564853817348, 'Delete Lead', 2007854564853817344, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:delete', '#', 'admin', NOW(), 'Delete leads'),
(2007854564853817349, 'Export Leads', 2007854564853817344, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:export', '#', 'admin', NOW(), 'Export to Excel'),
(2007854564853817350, 'Import Leads', 2007854564853817344, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:import', '#', 'admin', NOW(), 'Import from Excel')
ON CONFLICT ("menuId") DO NOTHING;

-- STEP 2: Add Employee query button (under Team menu ID: 2007854564807680000)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (2007854564807680001, 'View Employee', 2007854564807680000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:employees:query', '#', 'admin', NOW(), 'View employee KPIs')
ON CONFLICT ("menuId") DO NOTHING;

-- STEP 3: Add Activities menu and permissions (under Marketing parent: 2007854564761542656)
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(2007854564870000000, 'Lead Activities', 2007854564761542656, 5, 'activities', 'marketing/activities/index', 1, 0, 'C', '0', '0', 'marketing:activities:list', 'activity', 'admin', NOW(), 'Activities'),
(2007854564870000001, 'View Activity', 2007854564870000000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:view', '#', 'admin', NOW(), 'View activity'),
(2007854564870000002, 'Add Activity', 2007854564870000000, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:add', '#', 'admin', NOW(), 'Add activity'),
(2007854564870000003, 'Delete Activity', 2007854564870000000, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:delete', '#', 'admin', NOW(), 'Delete activity'),
(2007854564870000004, 'Activity Stats', 2007854564870000000, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:stats', '#', 'admin', NOW(), 'Activity statistics')
ON CONFLICT ("menuId") DO NOTHING;

-- STEP 4: Add Assignment menu and permissions
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(2007854564880000000, 'Lead Assignments', 2007854564761542656, 6, 'assignments', 'marketing/assignments/index', 1, 0, 'C', '0', '0', 'marketing:assignments:list', 'user-check', 'admin', NOW(), 'Assignments'),
(2007854564880000001, 'View Assignment', 2007854564880000000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:view', '#', 'admin', NOW(), 'View assignment'),
(2007854564880000002, 'Assign Lead', 2007854564880000000, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:assign', '#', 'admin', NOW(), 'Assign lead'),
(2007854564880000003, 'Assignment Stats', 2007854564880000000, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:stats', '#', 'admin', NOW(), 'Assignment stats')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- STEP 5: Assign permissions to ALL roles that have marketing:leads:list
-- ====================================================

-- Assign all new lead permissions to roles that have leads:list
INSERT INTO sys_role_menu ("role_Id", "menu_Id")
SELECT DISTINCT rm."role_Id", new_menu."menuId"
FROM sys_role_menu rm
JOIN sys_menu m ON rm."menu_Id" = m."menuId"
CROSS JOIN sys_menu new_menu
WHERE m."perms" = 'marketing:leads:list'
AND new_menu."perms" IN (
    'marketing:leads:view',
    'marketing:leads:add',
    'marketing:leads:edit',
    'marketing:leads:delete',
    'marketing:leads:export',
    'marketing:leads:import'
)
ON CONFLICT DO NOTHING;

-- Assign employee query to roles that have employees:list
INSERT INTO sys_role_menu ("role_Id", "menu_Id")
SELECT DISTINCT rm."role_Id", new_menu."menuId"
FROM sys_role_menu rm
JOIN sys_menu m ON rm."menu_Id" = m."menuId"
CROSS JOIN sys_menu new_menu
WHERE m."perms" = 'marketing:employees:list'
AND new_menu."perms" = 'marketing:employees:query'
ON CONFLICT DO NOTHING;

-- Assign activity permissions to roles that have leads:list
INSERT INTO sys_role_menu ("role_Id", "menu_Id")
SELECT DISTINCT rm."role_Id", new_menu."menuId"
FROM sys_role_menu rm
JOIN sys_menu m ON rm."menu_Id" = m."menuId"
CROSS JOIN sys_menu new_menu
WHERE m."perms" = 'marketing:leads:list'
AND new_menu."perms" IN (
    'marketing:activities:list',
    'marketing:activities:view',
    'marketing:activities:add',
    'marketing:activities:delete',
    'marketing:activities:stats'
)
ON CONFLICT DO NOTHING;

-- Assign assignment permissions to roles that have leads:list
INSERT INTO sys_role_menu ("role_Id", "menu_Id")
SELECT DISTINCT rm."role_Id", new_menu."menuId"
FROM sys_role_menu rm
JOIN sys_menu m ON rm."menu_Id" = m."menuId"
CROSS JOIN sys_menu new_menu
WHERE m."perms" = 'marketing:leads:list'
AND new_menu."perms" IN (
    'marketing:assignments:list',
    'marketing:assignments:view',
    'marketing:assignments:assign',
    'marketing:assignments:stats'
)
ON CONFLICT DO NOTHING;

-- ====================================================
-- VERIFICATION
-- ====================================================
SELECT 'All Marketing Permissions:' AS "Info";
SELECT "menuId", "menuName", "perms", "menuType"
FROM sys_menu
WHERE "perms" LIKE 'marketing:%'
ORDER BY "perms";

SELECT 'Permissions per Role:' AS "Info";
SELECT r."roleName", COUNT(*) AS "permissionCount"
FROM sys_role r
JOIN sys_role_menu rm ON r."roleId" = rm."role_Id"
JOIN sys_menu m ON rm."menu_Id" = m."menuId"
WHERE m."perms" LIKE 'marketing:%'
GROUP BY r."roleName";
