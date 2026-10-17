-- ====================================================
-- Marketing Module Permissions Setup Script
-- Version: 1.0
-- Date: 2026-01-07
-- ====================================================
-- This script creates all required permissions for the Marketing module
-- and assigns them to roles. Run this script to ensure all permissions exist.
-- ====================================================

-- ====================================================
-- STEP 1: Insert Marketing Module Menu Items
-- ====================================================

-- Marketing Parent Menu
INSERT IGNORE INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES (9000, 'Marketing', 0, 8, 'marketing', NULL, 1, 0, 'M', '0', '0', NULL, 'chart-line', 'admin', NOW(), 'Marketing module');

-- Leads Management
INSERT IGNORE INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES (9001, 'Leads Management', 9000, 1, 'leads', 'marketing/leads/index', 1, 0, 'C', '0', '0', 'marketing:leads:list', 'users', 'admin', NOW(), 'Leads list and management');

-- Lead Permissions (Buttons)
INSERT IGNORE INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(9002, 'View Lead', 9001, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:view', '#', 'admin', NOW(), 'View lead details'),
(9003, 'Add Lead', 9001, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:add', '#', 'admin', NOW(), 'Create new lead'),
(9004, 'Edit Lead', 9001, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:edit', '#', 'admin', NOW(), 'Edit lead information'),
(9005, 'Delete Lead', 9001, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:delete', '#', 'admin', NOW(), 'Delete leads'),
(9006, 'Export Leads', 9001, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:export', '#', 'admin', NOW(), 'Export leads to Excel'),
(9007, 'Import Leads', 9001, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:import', '#', 'admin', NOW(), 'Import leads from Excel');

-- Lead Activities
INSERT IGNORE INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES (9010, 'Lead Activities', 9000, 2, 'activities', 'marketing/activities/index', 1, 0, 'C', '0', '0', 'marketing:activities:list', 'activity', 'admin', NOW(), 'Lead activities management');

-- Activity Permissions
INSERT IGNORE INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(9011, 'View Activity', 9010, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:view', '#', 'admin', NOW(), 'View activity details'),
(9012, 'Add Activity', 9010, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:add', '#', 'admin', NOW(), 'Create new activity'),
(9013, 'Delete Activity', 9010, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:delete', '#', 'admin', NOW(), 'Delete activities'),
(9014, 'Activity Stats', 9010, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:stats', '#', 'admin', NOW(), 'View activity statistics');

-- Lead Assignments
INSERT IGNORE INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES (9020, 'Lead Assignments', 9000, 3, 'assignments', 'marketing/assignments/index', 1, 0, 'C', '0', '0', 'marketing:assignments:list', 'user-check', 'admin', NOW(), 'Lead assignment management');

-- Assignment Permissions
INSERT IGNORE INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(9021, 'View Assignment', 9020, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:view', '#', 'admin', NOW(), 'View assignment details'),
(9022, 'Assign Lead', 9020, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:assign', '#', 'admin', NOW(), 'Assign/reassign leads'),
(9023, 'Assignment Stats', 9020, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:stats', '#', 'admin', NOW(), 'View assignment statistics');

-- Marketing Employees
INSERT IGNORE INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES (9030, 'Marketing Team', 9000, 4, 'team', 'marketing/team/index', 1, 0, 'C', '0', '0', 'marketing:employees:list', 'team', 'admin', NOW(), 'Marketing team management');

-- Marketing Employee Permissions
INSERT IGNORE INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(9031, 'View Employee', 9030, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:employees:query', '#', 'admin', NOW(), 'View employee details and KPIs');

-- ====================================================
-- STEP 2: Create Marketing Role
-- ====================================================

-- Marketing Staff Role (basic access)
INSERT IGNORE INTO sys_role (roleId, roleName, roleKey, roleSort, dataScope, menuCheckStrictly, deptCheckStrictly, status, delFlag, create_by, create_time, remark)
VALUES (100, 'Marketing Staff', 'marketing_staff', 10, '5', 1, 1, '0', '0', 'admin', NOW(), 'Marketing team member with basic lead access');

-- Marketing Manager Role (full access)
INSERT IGNORE INTO sys_role (roleId, roleName, roleKey, roleSort, dataScope, menuCheckStrictly, deptCheckStrictly, status, delFlag, create_by, create_time, remark)
VALUES (101, 'Marketing Manager', 'marketing_manager', 9, '2', 1, 1, '0', '0', 'admin', NOW(), 'Marketing manager with full module access');

-- ====================================================
-- STEP 3: Assign Permissions to Roles
-- ====================================================

-- Marketing Staff Role Permissions (limited)
INSERT IGNORE INTO sys_role_menu (roleId, menuId) VALUES
(100, 9000),  -- Marketing menu
(100, 9001),  -- Leads list
(100, 9002),  -- View lead
(100, 9003),  -- Add lead (auto-assigned to self)
(100, 9004),  -- Edit lead (own leads only)
(100, 9010),  -- Activities list
(100, 9011),  -- View activity
(100, 9012),  -- Add activity
(100, 9030),  -- Team list
(100, 9031); -- View employee

-- Marketing Manager Role Permissions (full access)
INSERT IGNORE INTO sys_role_menu (roleId, menuId) VALUES
(101, 9000),  -- Marketing menu
(101, 9001),  -- Leads list
(101, 9002),  -- View lead
(101, 9003),  -- Add lead
(101, 9004),  -- Edit lead
(101, 9005),  -- Delete lead
(101, 9006),  -- Export leads
(101, 9007),  -- Import leads
(101, 9010),  -- Activities list
(101, 9011),  -- View activity
(101, 9012),  -- Add activity
(101, 9013),  -- Delete activity
(101, 9014),  -- Activity stats
(101, 9020),  -- Assignments list
(101, 9021),  -- View assignment
(101, 9022),  -- Assign lead
(101, 9023),  -- Assignment stats
(101, 9030),  -- Team list
(101, 9031); -- View employee

-- ====================================================
-- STEP 4: Verification Query
-- ====================================================

-- Check all marketing permissions
SELECT m.menuId, m.menuName, m.perms, m.menuType, r.roleName
FROM sys_menu m
LEFT JOIN sys_role_menu rm ON m.menuId = rm.menuId
LEFT JOIN sys_role r ON rm.roleId = r.roleId
WHERE m.perms LIKE 'marketing:%'
ORDER BY m.menuId;

-- ====================================================
-- PERMISSIONS SUMMARY
-- ====================================================
/*
Marketing Module Permissions:

LEADS:
- marketing:leads:list      - View leads list (filtered by role)
- marketing:leads:view      - View individual lead details
- marketing:leads:add       - Add new lead (auto-assigned for non-admin)
- marketing:leads:edit      - Edit lead details and status
- marketing:leads:delete    - Delete leads
- marketing:leads:export    - Export leads to Excel
- marketing:leads:import    - Import leads from Excel

ACTIVITIES:
- marketing:activities:list    - View activities list
- marketing:activities:view    - View activity details
- marketing:activities:add     - Create new activity
- marketing:activities:delete  - Delete activities
- marketing:activities:stats   - View activity statistics

ASSIGNMENTS:
- marketing:assignments:list   - View assignment history
- marketing:assignments:view   - View assignment details
- marketing:assignments:assign - Assign/reassign leads
- marketing:assignments:stats  - View assignment statistics

EMPLOYEES:
- marketing:employees:list     - View marketing team list
- marketing:employees:query    - View employee details and KPIs

ROLE-BASED DATA FILTERING:
- Admin users: See ALL leads in the system
- Non-admin users: See ONLY leads assigned to them
- Auto-assignment: Non-admin users creating leads are auto-assigned

*/
