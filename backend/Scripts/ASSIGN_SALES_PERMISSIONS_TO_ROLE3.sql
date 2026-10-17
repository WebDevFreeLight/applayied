-- ====================================================
-- ASSIGN ALL SALES PERMISSIONS TO ROLE 3
-- ====================================================
-- Purpose: Give role_id=3 full access to all Sales
-- module menus (IDs 50000-50999)
-- ====================================================
-- PRE-REQUISITE: Run Sales_Module_Permissions_FIXED.sql
-- first to create the menu entries!
-- ====================================================
-- Column names are lowercase (SqlSugar PgSqlIsAutoToLower=true)
-- Safe to re-run: uses NOT EXISTS to avoid duplicates
-- ====================================================

-- ====================================================
-- STEP 1: Verify role 3 exists
-- ====================================================
SELECT roleid, rolename, rolekey, status
FROM sys_role
WHERE roleid = 3;

-- ====================================================
-- STEP 2: Verify sales menus exist in sys_menu
-- ====================================================
SELECT menuid, menuname, menutype, perms
FROM sys_menu
WHERE menuid BETWEEN 50000 AND 50999
ORDER BY menuid;

-- ====================================================
-- STEP 3: Assign ALL Sales menus to Role 3
-- ====================================================
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 50000 AND 50999
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 3 AND rm.menu_id = m.menuid
  );

-- ====================================================
-- STEP 4: VERIFICATION
-- ====================================================

-- 4a. Count sales menus assigned to Role 3
SELECT COUNT(*) as sales_menu_count
FROM sys_role_menu rm
WHERE rm.role_id = 3
  AND rm.menu_id BETWEEN 50000 AND 50999;
-- Expected: 39 (1 parent + 5 pages + 33 buttons)

-- 4b. List all sales permissions assigned to Role 3
SELECT
    m.menuid,
    m.menuname,
    m.menutype,
    m.perms,
    CASE m.menutype
        WHEN 'M' THEN 'Directory'
        WHEN 'C' THEN 'Page'
        WHEN 'F' THEN 'Button'
    END as type_label
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 3
  AND m.menuid BETWEEN 50000 AND 50999
ORDER BY m.menuid;

-- 4c. Verify all sales permission strings
SELECT DISTINCT m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 3
  AND m.perms LIKE 'sales:%'
  AND m.perms IS NOT NULL
  AND m.perms != ''
ORDER BY m.perms;

-- 4d. Check for missing menus (should be empty)
SELECT m.menuid, m.menuname, m.perms
FROM sys_menu m
WHERE m.menuid BETWEEN 50000 AND 50999
  AND m.menuid NOT IN (
    SELECT rm.menu_id
    FROM sys_role_menu rm
    WHERE rm.role_id = 3
  );
-- Expected: Empty = all assigned!

-- ====================================================
-- DONE! Role 3 now has ALL 33 Sales permissions:
-- ====================================================
--
-- DASHBOARD (4): view, revenue, performance, export
-- LEADS (7): list, view, edit, status, notes, activities, export
-- DEALS (10): list, view, add, edit, delete, status, pipeline, notes, activities, export
-- VISITS (6): list, add, view, cancel, status, notes
-- REPORTS (6): view, conversion, forecast, team, visits, export
--
-- ====================================================
-- ROLLBACK (if needed):
-- DELETE FROM sys_role_menu WHERE role_id = 3 AND menu_id BETWEEN 50000 AND 50999;
-- ====================================================
