-- ============================================
-- ASSIGN ALL MENUS TO ADMIN ROLE
-- ============================================
-- This gives admin role (role_id=1) access to ALL menus
-- Admin will see ALL pages in sidebar (all 75+ permissions)
-- ============================================

-- Step 1: Clear existing admin role menu assignments
DELETE FROM sys_role_menu WHERE role_id = 1;

-- Step 2: Assign ALL menus to admin role (role_id = 1)
-- This includes all menu types: M (Directory), C (Component), L (Link)
-- Excludes only: F (Function/Button) and inactive menus
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT
    1 as role_id,                -- Admin role
    menu_id,                     -- All menu IDs
    'admin' as create_by,
    NOW() as create_time
FROM sys_menu
WHERE status = '0'               -- Only active menus
  AND menu_type IN ('M', 'C', 'L')  -- Directory, Component, Link (not buttons)
ON CONFLICT DO NOTHING;

-- Step 3: Verify admin role has all menus
SELECT
    COUNT(*) as total_menus_assigned
FROM sys_role_menu
WHERE role_id = 1;

-- Expected: Should show 75+ menus

-- Step 4: See all permissions admin will have
SELECT DISTINCT
    m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE rm.role_id = 1
  AND m.perms IS NOT NULL
  AND m.perms != ''
ORDER BY m.perms;

-- Expected: Should show all 75+ permission strings like:
-- EventManagement:eventtypes:list
-- executive:alerts:view
-- executive:charts:view
-- executive:heatmaps:view
-- executive:kpis:view
-- executive:overview:view
-- hr:dashboard:view
-- system:user:list
-- ... and many more!

-- Step 5: See complete menu structure admin will see
SELECT
    m.menu_id,
    m.menu_name,
    m.menuname_key,
    m.perms,
    m.path,
    m.menu_type,
    m.parent_id,
    CASE
        WHEN m.parent_id = 0 THEN '🔸 TOP LEVEL'
        ELSE '   └─ ' || (SELECT menu_name FROM sys_menu WHERE menu_id = m.parent_id)
    END as parent_structure
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE rm.role_id = 1
ORDER BY m.parent_id, m.ordernum;

-- ============================================
-- VERIFICATION QUERIES
-- ============================================

-- Check 1: Total menus assigned to admin
SELECT
    r.role_name,
    COUNT(rm.menu_id) as total_menus
FROM sys_role r
LEFT JOIN sys_role_menu rm ON r.role_id = rm.role_id
WHERE r.role_id = 1
GROUP BY r.role_name;

-- Check 2: Permissions by module
SELECT
    SPLIT_PART(m.perms, ':', 1) as module,
    COUNT(*) as permission_count
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE rm.role_id = 1
  AND m.perms IS NOT NULL
  AND m.perms != ''
GROUP BY SPLIT_PART(m.perms, ':', 1)
ORDER BY permission_count DESC;

-- Expected output (example):
-- module              | permission_count
-- hr                  | 25
-- system              | 20
-- executive           | 15
-- EventManagement     | 10
-- ... etc

-- Check 3: What admin user will see after login
SELECT DISTINCT
    m.perms
FROM sys_user u
JOIN sys_user_role ur ON u.user_id = ur.user_id
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE u.user_id = 1
  AND m.perms IS NOT NULL
  AND m.perms != ''
ORDER BY m.perms;

-- This is the exact array backend will return in login response!

-- ============================================
-- AFTER RUNNING THIS SCRIPT
-- ============================================
-- 1. Login to your application with admin user
-- 2. Backend should return ALL 75+ permissions
-- 3. Frontend sidebar should show ALL menu modules:
--    - Dashboard
--    - Executive Dashboard (with all 5 sub-items)
--    - Users & Access Management
--    - Event Management
--    - HR Management
--    - Monitor (System Monitor)
--    - ... and all other modules!
-- 4. Verify in console:
--    permissions: [75+ items array]
-- ============================================

-- ============================================
-- ROLLBACK (if needed)
-- ============================================
-- If you want to remove all assignments and start over:
-- DELETE FROM sys_role_menu WHERE role_id = 1;
-- ============================================
