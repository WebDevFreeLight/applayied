-- ============================================
-- Permission System Setup SQL
-- ============================================
-- Purpose: Connect users to roles and roles to menus
-- This gives users permissions to access specific pages
-- ============================================

-- ============================================
-- STEP 1: Verify/Create Admin Role
-- ============================================

-- Check if admin role exists
SELECT * FROM sys_role WHERE role_id = 1;

-- If not exists, create it (uncomment if needed)
-- INSERT INTO sys_role (role_id, role_name, role_key, role_sort, status, data_scope, menu_check_strictly, dept_check_strictly, create_by, create_time, remark)
-- VALUES (1, 'المشرف الممتاز', 'admin', 1, 0, 1, true, false, 'admin', NOW(), 'المشرف الممتاز');


-- ============================================
-- STEP 2: Connect Admin User to Admin Role
-- ============================================

-- Delete existing user-role assignment (if any)
DELETE FROM sys_user_role WHERE user_id = 1;

-- Assign user_id=1 (admin) to role_id=1 (admin role)
INSERT INTO sys_user_role (user_id, role_id, create_by, create_time)
VALUES (1, 1, 'admin', NOW());

-- Verify assignment
SELECT ur.*, u.user_name, r.role_name
FROM sys_user_role ur
JOIN sys_user u ON ur.user_id = u.user_id
JOIN sys_role r ON ur.role_id = r.role_id
WHERE ur.user_id = 1;


-- ============================================
-- STEP 3: View All Available Menus
-- ============================================

-- See all menus with their permissions
SELECT
    menu_id,
    menu_name,
    menuname_key,
    perms,
    menu_type,
    parent_id,
    path,
    status
FROM sys_menu
WHERE status = '0'  -- Only active menus
ORDER BY parent_id, ordernum;


-- ============================================
-- STEP 4: Assign ALL Menus to Admin Role
-- ============================================

-- Delete existing role-menu assignments for admin role
DELETE FROM sys_role_menu WHERE role_id = 1;

-- Assign ALL active menus to admin role (role_id = 1)
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT
    1 as role_id,
    menu_id,
    'admin' as create_by,
    NOW() as create_time
FROM sys_menu
WHERE status = '0'  -- Only active menus
ON CONFLICT DO NOTHING;  -- Skip if already exists

-- Verify admin role has all menus
SELECT
    rm.role_id,
    r.role_name,
    COUNT(*) as menu_count
FROM sys_role_menu rm
JOIN sys_role r ON rm.role_id = r.role_id
WHERE rm.role_id = 1
GROUP BY rm.role_id, r.role_name;


-- ============================================
-- STEP 5: View Menus Assigned to Admin Role
-- ============================================

SELECT
    m.menu_id,
    m.menu_name,
    m.menuname_key,
    m.perms,
    m.menu_type,
    m.path,
    rm.role_id,
    r.role_name
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menu_id
JOIN sys_role r ON rm.role_id = r.role_id
WHERE rm.role_id = 1
ORDER BY m.parent_id, m.ordernum;


-- ============================================
-- STEP 6: Create Limited Role (Example)
-- ============================================

-- Example: Create an "HR Manager" role with limited permissions

-- 1. Create the role
INSERT INTO sys_role (role_id, role_name, role_key, role_sort, status, data_scope, menu_check_strictly, dept_check_strictly, create_by, create_time, remark)
VALUES (10, 'HR Manager', 'hr_manager', 10, 0, 2, true, false, 'admin', NOW(), 'HR Manager role with HR module access only')
ON CONFLICT (role_id) DO NOTHING;

-- 2. Assign specific menus to HR Manager role
-- Only HR-related menus

DELETE FROM sys_role_menu WHERE role_id = 10;

-- Assign Dashboard (always visible)
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT
    10 as role_id,
    menu_id,
    'admin' as create_by,
    NOW() as create_time
FROM sys_menu
WHERE menuname_key = 'sidebar-menu-file-manager'  -- Dashboard
   OR menuname_key LIKE '%hr%'  -- All HR menus
   AND status = '0'
ON CONFLICT DO NOTHING;

-- Verify HR Manager role assignments
SELECT
    m.menu_name,
    m.menuname_key,
    m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE rm.role_id = 10
ORDER BY m.ordernum;


-- ============================================
-- STEP 7: Assign User to Limited Role
-- ============================================

-- Example: Assign user_id=2 to HR Manager role (role_id=10)

-- First, create a test user (if needed)
-- INSERT INTO sys_user (user_id, user_name, nick_name, password, status, dept_id, create_by, create_time)
-- VALUES (2, 'hr_user', 'HR User', '21232f297a57a5a743894a0e4a801fc3', 0, 0, 'admin', NOW());

-- Assign user to HR Manager role
INSERT INTO sys_user_role (user_id, role_id, create_by, create_time)
VALUES (2, 10, 'admin', NOW())
ON CONFLICT DO NOTHING;


-- ============================================
-- STEP 8: Assign Specific Menus to Role by Permission
-- ============================================

-- Example: Give role_id=10 only specific permissions

DELETE FROM sys_role_menu WHERE role_id = 10;

-- Assign menus with specific permissions
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT
    10 as role_id,
    menu_id,
    'admin' as create_by,
    NOW() as create_time
FROM sys_menu
WHERE perms IN (
    'system:user:list',           -- Users page
    'hr:dashboard:view',          -- HR Dashboard
    'executive:overview:view'     -- Executive Overview
)
AND status = '0'
ON CONFLICT DO NOTHING;


-- ============================================
-- STEP 9: View User Permissions (Complete Picture)
-- ============================================

-- See what permissions a user has
-- Replace user_id = 1 with your target user

SELECT DISTINCT
    u.user_id,
    u.user_name,
    r.role_name,
    m.menu_name,
    m.menuname_key,
    m.perms,
    m.path
FROM sys_user u
JOIN sys_user_role ur ON u.user_id = ur.user_id
JOIN sys_role r ON ur.role_id = r.role_id
JOIN sys_role_menu rm ON r.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE u.user_id = 1  -- Change this to target user
  AND m.status = '0'
ORDER BY m.ordernum;


-- ============================================
-- STEP 10: Get Permissions Array for Frontend
-- ============================================

-- This query returns the permissions array that backend should return
-- Replace user_id = 1 with your target user

SELECT DISTINCT
    m.perms
FROM sys_user u
JOIN sys_user_role ur ON u.user_id = ur.user_id
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE u.user_id = 1  -- Change this to target user
  AND m.perms IS NOT NULL
  AND m.perms != ''
  AND m.status = '0'
ORDER BY m.perms;


-- ============================================
-- USEFUL QUERIES FOR MANAGEMENT
-- ============================================

-- 1. List all users with their roles
SELECT
    u.user_id,
    u.user_name,
    u.nick_name,
    STRING_AGG(r.role_name, ', ') as roles
FROM sys_user u
LEFT JOIN sys_user_role ur ON u.user_id = ur.user_id
LEFT JOIN sys_role r ON ur.role_id = r.role_id
GROUP BY u.user_id, u.user_name, u.nick_name
ORDER BY u.user_id;


-- 2. List all roles with menu counts
SELECT
    r.role_id,
    r.role_name,
    r.role_key,
    COUNT(rm.menu_id) as menu_count
FROM sys_role r
LEFT JOIN sys_role_menu rm ON r.role_id = rm.role_id
GROUP BY r.role_id, r.role_name, r.role_key
ORDER BY r.role_id;


-- 3. List menus not assigned to any role
SELECT
    m.menu_id,
    m.menu_name,
    m.perms,
    m.path
FROM sys_menu m
LEFT JOIN sys_role_menu rm ON m.menu_id = rm.menu_id
WHERE rm.menu_id IS NULL
  AND m.status = '0'
ORDER BY m.ordernum;


-- 4. Find which roles have a specific permission
SELECT
    r.role_id,
    r.role_name,
    m.menu_name,
    m.perms
FROM sys_role r
JOIN sys_role_menu rm ON r.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE m.perms = 'system:user:list'  -- Change to target permission
ORDER BY r.role_id;


-- ============================================
-- CLEANUP QUERIES (USE WITH CAUTION!)
-- ============================================

-- Remove all role-menu assignments for a role
-- DELETE FROM sys_role_menu WHERE role_id = 10;

-- Remove all user-role assignments for a user
-- DELETE FROM sys_user_role WHERE user_id = 2;

-- Remove a role completely
-- DELETE FROM sys_role_menu WHERE role_id = 10;
-- DELETE FROM sys_user_role WHERE role_id = 10;
-- DELETE FROM sys_role WHERE role_id = 10;
