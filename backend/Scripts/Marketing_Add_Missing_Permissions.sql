-- ====================================================
-- Marketing Module - Add Missing Button Permissions
-- Date: 2026-01-07
-- ====================================================
-- This script adds the missing action/button permissions
-- to the existing marketing menu items
-- ====================================================

-- Get existing parent menu IDs (from your query results)
-- Leads Menu ID: 2007854564853817344
-- Team Menu ID: 2007854564807680000
-- Marketing Parent ID: 2007854564761542656

-- ====================================================
-- STEP 1: Add Lead Action Permissions (under Leads menu)
-- ====================================================

-- Generate new Snowflake IDs for button permissions
-- Using current timestamp-based IDs to avoid conflicts

INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
-- Lead View Permission
(2007854564853817345, 'View Lead', 2007854564853817344, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:view', '#', 'admin', NOW(), 'View lead details'),
-- Lead Add Permission
(2007854564853817346, 'Add Lead', 2007854564853817344, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:add', '#', 'admin', NOW(), 'Create new lead'),
-- Lead Edit Permission
(2007854564853817347, 'Edit Lead', 2007854564853817344, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:edit', '#', 'admin', NOW(), 'Edit lead information'),
-- Lead Delete Permission
(2007854564853817348, 'Delete Lead', 2007854564853817344, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:delete', '#', 'admin', NOW(), 'Delete leads'),
-- Lead Export Permission
(2007854564853817349, 'Export Leads', 2007854564853817344, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:export', '#', 'admin', NOW(), 'Export leads to Excel'),
-- Lead Import Permission
(2007854564853817350, 'Import Leads', 2007854564853817344, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:import', '#', 'admin', NOW(), 'Import leads from Excel')
ON DUPLICATE KEY UPDATE menuName = VALUES(menuName);

-- ====================================================
-- STEP 2: Add Employee/Team Action Permissions
-- ====================================================

INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(2007854564807680001, 'View Employee', 2007854564807680000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:employees:query', '#', 'admin', NOW(), 'View employee details and KPIs')
ON DUPLICATE KEY UPDATE menuName = VALUES(menuName);

-- ====================================================
-- STEP 3: Add Activities Menu and Permissions
-- ====================================================

-- Activities parent menu (under Marketing)
INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(2007854564870000000, 'Lead Activities', 2007854564761542656, 5, 'activities', 'marketing/activities/index', 1, 0, 'C', '0', '0', 'marketing:activities:list', 'activity', 'admin', NOW(), 'Lead activities management')
ON DUPLICATE KEY UPDATE menuName = VALUES(menuName);

-- Activity action permissions
INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(2007854564870000001, 'View Activity', 2007854564870000000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:view', '#', 'admin', NOW(), 'View activity details'),
(2007854564870000002, 'Add Activity', 2007854564870000000, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:add', '#', 'admin', NOW(), 'Create new activity'),
(2007854564870000003, 'Delete Activity', 2007854564870000000, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:delete', '#', 'admin', NOW(), 'Delete activities'),
(2007854564870000004, 'Activity Stats', 2007854564870000000, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:stats', '#', 'admin', NOW(), 'View activity statistics')
ON DUPLICATE KEY UPDATE menuName = VALUES(menuName);

-- ====================================================
-- STEP 4: Add Assignments Menu and Permissions
-- ====================================================

-- Assignments parent menu (under Marketing)
INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(2007854564880000000, 'Lead Assignments', 2007854564761542656, 6, 'assignments', 'marketing/assignments/index', 1, 0, 'C', '0', '0', 'marketing:assignments:list', 'user-check', 'admin', NOW(), 'Lead assignment management')
ON DUPLICATE KEY UPDATE menuName = VALUES(menuName);

-- Assignment action permissions
INSERT INTO sys_menu (menuId, menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(2007854564880000001, 'View Assignment', 2007854564880000000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:view', '#', 'admin', NOW(), 'View assignment details'),
(2007854564880000002, 'Assign Lead', 2007854564880000000, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:assign', '#', 'admin', NOW(), 'Assign/reassign leads'),
(2007854564880000003, 'Assignment Stats', 2007854564880000000, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:stats', '#', 'admin', NOW(), 'View assignment statistics')
ON DUPLICATE KEY UPDATE menuName = VALUES(menuName);

-- ====================================================
-- STEP 5: Assign Permissions to Roles
-- ====================================================

-- First, get the role IDs from your database
-- Based on your query: المشرف الممتاز (Super Admin), تحرير الدور (Edit Role), شخصية طبيعية (Natural Person)

-- Get role IDs (run this query first to get actual IDs)
-- SELECT roleId, roleName FROM sys_role WHERE roleName IN ('المشرف الممتاز', 'تحرير الدور', 'شخصية طبيعية');

-- Assuming these role IDs based on typical setup:
-- Replace @admin_role_id, @editor_role_id, @natural_role_id with actual values

SET @admin_role_id = (SELECT roleId FROM sys_role WHERE roleName = 'المشرف الممتاز' LIMIT 1);
SET @editor_role_id = (SELECT roleId FROM sys_role WHERE roleName = 'تحرير الدور' LIMIT 1);
SET @natural_role_id = (SELECT roleId FROM sys_role WHERE roleName = 'شخصية طبيعية' LIMIT 1);

-- Assign Lead action permissions to Super Admin (المشرف الممتاز)
INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id) VALUES
(@admin_role_id, 2007854564853817345),  -- View Lead
(@admin_role_id, 2007854564853817346),  -- Add Lead
(@admin_role_id, 2007854564853817347),  -- Edit Lead
(@admin_role_id, 2007854564853817348),  -- Delete Lead
(@admin_role_id, 2007854564853817349),  -- Export Leads
(@admin_role_id, 2007854564853817350),  -- Import Leads
(@admin_role_id, 2007854564807680001),  -- View Employee
(@admin_role_id, 2007854564870000000),  -- Activities List
(@admin_role_id, 2007854564870000001),  -- View Activity
(@admin_role_id, 2007854564870000002),  -- Add Activity
(@admin_role_id, 2007854564870000003),  -- Delete Activity
(@admin_role_id, 2007854564870000004),  -- Activity Stats
(@admin_role_id, 2007854564880000000),  -- Assignments List
(@admin_role_id, 2007854564880000001),  -- View Assignment
(@admin_role_id, 2007854564880000002),  -- Assign Lead
(@admin_role_id, 2007854564880000003);  -- Assignment Stats

-- Assign Lead action permissions to Edit Role (تحرير الدور)
INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id) VALUES
(@editor_role_id, 2007854564853817345),  -- View Lead
(@editor_role_id, 2007854564853817346),  -- Add Lead
(@editor_role_id, 2007854564853817347),  -- Edit Lead
(@editor_role_id, 2007854564853817348),  -- Delete Lead
(@editor_role_id, 2007854564853817349),  -- Export Leads
(@editor_role_id, 2007854564853817350),  -- Import Leads
(@editor_role_id, 2007854564807680001),  -- View Employee
(@editor_role_id, 2007854564870000000),  -- Activities List
(@editor_role_id, 2007854564870000001),  -- View Activity
(@editor_role_id, 2007854564870000002),  -- Add Activity
(@editor_role_id, 2007854564870000003),  -- Delete Activity
(@editor_role_id, 2007854564870000004),  -- Activity Stats
(@editor_role_id, 2007854564880000000),  -- Assignments List
(@editor_role_id, 2007854564880000001),  -- View Assignment
(@editor_role_id, 2007854564880000002),  -- Assign Lead
(@editor_role_id, 2007854564880000003);  -- Assignment Stats

-- Assign limited permissions to Natural Person (شخصية طبيعية) - only view, add, edit (own leads)
INSERT IGNORE INTO sys_role_menu (role_Id, menu_Id) VALUES
(@natural_role_id, 2007854564853817345),  -- View Lead
(@natural_role_id, 2007854564853817346),  -- Add Lead
(@natural_role_id, 2007854564853817347),  -- Edit Lead
(@natural_role_id, 2007854564807680001),  -- View Employee
(@natural_role_id, 2007854564870000000),  -- Activities List
(@natural_role_id, 2007854564870000001),  -- View Activity
(@natural_role_id, 2007854564870000002);  -- Add Activity

-- ====================================================
-- STEP 6: Verify the changes
-- ====================================================

-- Check all marketing permissions now
SELECT m.menuId, m.menuName, m.perms, m.menuType, m.parentId
FROM sys_menu m
WHERE m.perms LIKE 'marketing:%'
ORDER BY m.parentId, m.orderNum;

-- Check permissions assigned to roles
SELECT r.roleName, m.menuName, m.perms
FROM sys_role r
JOIN sys_role_menu rm ON r.roleId = rm.role_Id
JOIN sys_menu m ON rm.menu_Id = m.menuId
WHERE m.perms LIKE 'marketing:%'
ORDER BY r.roleName, m.perms;
