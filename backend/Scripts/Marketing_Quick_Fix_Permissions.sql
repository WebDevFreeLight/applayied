-- ====================================================
-- QUICK FIX: Add Missing Marketing Button Permissions
-- Run this script directly
-- ====================================================

-- STEP 1: Add Lead action buttons (under Leads menu ID: 2007854564853817344)
INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
SELECT * FROM (
    SELECT 2007854564853817345 AS menuId, 'View Lead' AS menuName, 2007854564853817344 AS parentId, 1 AS orderNum, '' AS path, NULL AS component, 1 AS isFrame, 0 AS isCache, 'F' AS menuType, '0' AS visible, '0' AS status, 'marketing:leads:view' AS perms, '#' AS icon, 'admin' AS create_by, NOW() AS create_time, 'View lead details' AS remark
    UNION SELECT 2007854564853817346, 'Add Lead', 2007854564853817344, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:add', '#', 'admin', NOW(), 'Create new lead'
    UNION SELECT 2007854564853817347, 'Edit Lead', 2007854564853817344, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:edit', '#', 'admin', NOW(), 'Edit lead'
    UNION SELECT 2007854564853817348, 'Delete Lead', 2007854564853817344, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:delete', '#', 'admin', NOW(), 'Delete leads'
    UNION SELECT 2007854564853817349, 'Export Leads', 2007854564853817344, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:export', '#', 'admin', NOW(), 'Export to Excel'
    UNION SELECT 2007854564853817350, 'Import Leads', 2007854564853817344, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:import', '#', 'admin', NOW(), 'Import from Excel'
) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM sys_menu WHERE perms = tmp.perms);

-- STEP 2: Add Employee query button (under Team menu ID: 2007854564807680000)
INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
SELECT 2007854564807680001, 'View Employee', 2007854564807680000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:employees:query', '#', 'admin', NOW(), 'View employee KPIs'
WHERE NOT EXISTS (SELECT 1 FROM sys_menu WHERE perms = 'marketing:employees:query');

-- STEP 3: Add Activities permissions (under Marketing parent: 2007854564761542656)
INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
SELECT * FROM (
    SELECT 2007854564870000000 AS menuId, 'Lead Activities' AS menuName, 2007854564761542656 AS parentId, 5 AS orderNum, 'activities' AS path, 'marketing/activities/index' AS component, 1 AS isFrame, 0 AS isCache, 'C' AS menuType, '0' AS visible, '0' AS status, 'marketing:activities:list' AS perms, 'activity' AS icon, 'admin' AS create_by, NOW() AS create_time, 'Activities' AS remark
    UNION SELECT 2007854564870000001, 'View Activity', 2007854564870000000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:view', '#', 'admin', NOW(), 'View activity'
    UNION SELECT 2007854564870000002, 'Add Activity', 2007854564870000000, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:add', '#', 'admin', NOW(), 'Add activity'
    UNION SELECT 2007854564870000003, 'Delete Activity', 2007854564870000000, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:delete', '#', 'admin', NOW(), 'Delete activity'
    UNION SELECT 2007854564870000004, 'Activity Stats', 2007854564870000000, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:stats', '#', 'admin', NOW(), 'Activity statistics'
) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM sys_menu WHERE perms = tmp.perms);

-- STEP 4: Add Assignment permissions
INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
SELECT * FROM (
    SELECT 2007854564880000000 AS menuId, 'Lead Assignments' AS menuName, 2007854564761542656 AS parentId, 6 AS orderNum, 'assignments' AS path, 'marketing/assignments/index' AS component, 1 AS isFrame, 0 AS isCache, 'C' AS menuType, '0' AS visible, '0' AS status, 'marketing:assignments:list' AS perms, 'user-check' AS icon, 'admin' AS create_by, NOW() AS create_time, 'Assignments' AS remark
    UNION SELECT 2007854564880000001, 'View Assignment', 2007854564880000000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:view', '#', 'admin', NOW(), 'View assignment'
    UNION SELECT 2007854564880000002, 'Assign Lead', 2007854564880000000, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:assign', '#', 'admin', NOW(), 'Assign lead'
    UNION SELECT 2007854564880000003, 'Assignment Stats', 2007854564880000000, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:stats', '#', 'admin', NOW(), 'Assignment stats'
) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM sys_menu WHERE perms = tmp.perms);

-- ====================================================
-- STEP 5: Assign new permissions to roles
-- ====================================================

-- Get menu IDs for the new permissions
SET @view_lead = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:leads:view');
SET @add_lead = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:leads:add');
SET @edit_lead = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:leads:edit');
SET @delete_lead = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:leads:delete');
SET @export_leads = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:leads:export');
SET @import_leads = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:leads:import');
SET @view_employee = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:employees:query');
SET @activities_list = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:activities:list');
SET @view_activity = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:activities:view');
SET @add_activity = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:activities:add');
SET @delete_activity = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:activities:delete');
SET @activity_stats = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:activities:stats');
SET @assignments_list = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:assignments:list');
SET @view_assignment = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:assignments:view');
SET @assign_lead = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:assignments:assign');
SET @assignment_stats = (SELECT menuId FROM sys_menu WHERE perms = 'marketing:assignments:stats');

-- Assign to ALL existing roles that have marketing:leads:list
INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @view_lead FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @view_lead IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @add_lead FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @add_lead IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @edit_lead FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @edit_lead IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @delete_lead FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @delete_lead IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @export_leads FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @export_leads IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @import_leads FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @import_leads IS NOT NULL;

-- Assign employee query to roles that have marketing:employees:list
INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @view_employee FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:employees:list' AND @view_employee IS NOT NULL;

-- Assign activity permissions to roles that have marketing:leads:list
INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @activities_list FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @activities_list IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @view_activity FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @view_activity IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @add_activity FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @add_activity IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @delete_activity FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @delete_activity IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @activity_stats FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @activity_stats IS NOT NULL;

-- Assign assignment permissions to roles that have marketing:leads:list
INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @assignments_list FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @assignments_list IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @view_assignment FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @view_assignment IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @assign_lead FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @assign_lead IS NOT NULL;

INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id)
SELECT rm.role_Id, @assignment_stats FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_Id = m.menuId WHERE m.perms = 'marketing:leads:list' AND @assignment_stats IS NOT NULL;

-- ====================================================
-- VERIFICATION
-- ====================================================
SELECT 'New Marketing Permissions:' AS Info;
SELECT m.menuId, m.menuName, m.perms, m.menuType
FROM sys_menu m
WHERE m.perms LIKE 'marketing:%'
ORDER BY m.perms;

SELECT 'Permissions assigned to roles:' AS Info;
SELECT r.roleName, COUNT(*) as permissionCount
FROM sys_role r
JOIN sys_role_menu rm ON r.roleId = rm.role_Id
JOIN sys_menu m ON rm.menu_Id = m.menuId
WHERE m.perms LIKE 'marketing:%'
GROUP BY r.roleName;
