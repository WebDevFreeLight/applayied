-- ============================================
-- QUICK START: Permission Setup
-- ============================================
-- Copy and paste these commands into your PostgreSQL
-- to set up admin with full permissions
-- ============================================

-- ============================================
-- OPTION 1: Give Admin User Full Access
-- ============================================

-- Step 1: Connect admin user to admin role
DELETE FROM sys_user_role WHERE user_id = 1;
INSERT INTO sys_user_role (user_id, role_id, create_by, create_time)
VALUES (1, 1, 'admin', NOW());

-- Step 2: Give admin role ALL menus
DELETE FROM sys_role_menu WHERE role_id = 1;
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, menu_id, 'admin', NOW()
FROM sys_menu
WHERE status = '0';

-- Step 3: Verify (should return all your permissions)
SELECT DISTINCT m.perms
FROM sys_user_role ur
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE ur.user_id = 1
  AND m.perms IS NOT NULL
  AND m.perms != ''
ORDER BY m.perms;

-- Expected output:
-- EventManagement:eventtypes:list
-- executive:overview:view
-- hr:dashboard:view
-- system:user:list
-- ... (all your permissions)


-- ============================================
-- OPTION 2: Give Admin Only Your Current Menus
-- ============================================

-- If you want admin to see only the 4 permissions you currently have:

DELETE FROM sys_role_menu WHERE role_id = 1;

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, menu_id, 'admin', NOW()
FROM sys_menu
WHERE perms IN (
    'system:user:list',
    'hr:dashboard:view',
    'executive:overview:view',
    'EventManagement:eventtypes:list'
)
AND status = '0';

-- Also include parent menus (they have no perms, but needed for hierarchy)
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, menu_id, 'admin', NOW()
FROM sys_menu
WHERE menu_id IN (
    SELECT parent_id
    FROM sys_menu
    WHERE perms IN (
        'system:user:list',
        'hr:dashboard:view',
        'executive:overview:view',
        'EventManagement:eventtypes:list'
    )
    AND parent_id != 0
)
AND status = '0'
ON CONFLICT DO NOTHING;


-- ============================================
-- VERIFY SETUP
-- ============================================

-- Check 1: Does admin user have admin role?
SELECT
    u.user_id,
    u.user_name,
    r.role_id,
    r.role_name
FROM sys_user u
JOIN sys_user_role ur ON u.user_id = ur.user_id
JOIN sys_role r ON ur.role_id = r.role_id
WHERE u.user_id = 1;

-- Expected: user_id=1, user_name=admin, role_id=1, role_name=المشرف الممتاز


-- Check 2: How many menus does admin role have?
SELECT
    r.role_id,
    r.role_name,
    COUNT(rm.menu_id) as menu_count
FROM sys_role r
LEFT JOIN sys_role_menu rm ON r.role_id = rm.role_id
WHERE r.role_id = 1
GROUP BY r.role_id, r.role_name;

-- Expected: Should show menu_count > 0


-- Check 3: What permissions will admin get?
SELECT DISTINCT m.perms
FROM sys_user_role ur
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE ur.user_id = 1
  AND m.perms IS NOT NULL
ORDER BY m.perms;

-- Expected: List of permission strings


-- Check 4: Full picture - what will admin see?
SELECT
    m.menu_name,
    m.menuname_key,
    m.perms,
    m.path,
    m.menu_type,
    CASE
        WHEN m.parent_id = 0 THEN 'TOP LEVEL'
        ELSE (SELECT menu_name FROM sys_menu WHERE menu_id = m.parent_id)
    END as parent_name
FROM sys_user_role ur
JOIN sys_role_menu rm ON ur.role_id = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menu_id
WHERE ur.user_id = 1
  AND m.status = '0'
ORDER BY m.parent_id, m.ordernum;


-- ============================================
-- AFTER RUNNING THESE QUERIES
-- ============================================
-- 1. Login to your application with admin user
-- 2. Check browser console (F12)
-- 3. Should see: permissions: ["system:user:list", "hr:dashboard:view", ...]
-- 4. Sidebar should show only menus you have permissions for
-- ============================================
