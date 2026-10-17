-- =============================================
-- Script: Assign All Menus to Admin Role
-- Purpose: Populates sys_role_menu table to link menus with admin role
-- Problem: After deleting sys_role_menu rows, the menu API returns empty
--          because it does an INNER JOIN between sys_menu and sys_role_menu
-- =============================================

-- Step 1: Check existing roles to find admin role
-- SELECT * FROM sys_role ORDER BY role_id;

-- Step 2: Check if sys_role_menu is empty
-- SELECT COUNT(*) FROM sys_role_menu;

-- Step 3: Check how many menus exist
-- SELECT COUNT(*) FROM sys_menu;

-- =============================================
-- MAIN SCRIPT: Assign ALL menus to admin role (role_id = 1)
-- =============================================

-- Clear existing role-menu assignments for admin role (optional, if you want a fresh start)
-- DELETE FROM sys_role_menu WHERE role_id = 1;

-- Insert all menus for admin role (role_id = 1)
-- This assigns EVERY menu in sys_menu to the admin role
INSERT INTO sys_role_menu (role_id, menu_id)
SELECT 1, menuid
FROM sys_menu
WHERE menuid NOT IN (SELECT menu_id FROM sys_role_menu WHERE role_id = 1)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- =============================================
-- Alternative: Assign to a specific role by name
-- =============================================
-- INSERT INTO sys_role_menu (role_id, menu_id)
-- SELECT r.role_id, m.menuid
-- FROM sys_menu m
-- CROSS JOIN sys_role r
-- WHERE r.role_name = 'admin'
-- ON CONFLICT (role_id, menu_id) DO NOTHING;

-- =============================================
-- Verify the assignment
-- =============================================
-- SELECT
--     r.role_name,
--     COUNT(rm.menu_id) as menu_count
-- FROM sys_role r
-- LEFT JOIN sys_role_menu rm ON r.role_id = rm.role_id
-- GROUP BY r.role_id, r.role_name
-- ORDER BY r.role_id;

-- =============================================
-- Detailed view of menus assigned to admin role
-- =============================================
-- SELECT
--     rm.role_id,
--     m.menuid,
--     m.menuname,
--     m.menutype,
--     m.perms
-- FROM sys_role_menu rm
-- JOIN sys_menu m ON rm.menu_id = m.menuid
-- WHERE rm.role_id = 1
-- ORDER BY m.menuid;
