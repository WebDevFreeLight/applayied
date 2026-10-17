-- ============================================================
-- Create "Marketer" Role (ID 13)
-- PostgreSQL
-- ============================================================
-- Copies ALL menu/permission assignments from Role 3 (Editor)
-- EXCEPT sales-related menus (50000-50999 range + any sales:* perms)
-- ============================================================

BEGIN;

-- ──────────────────────────────────────────────────────────────
-- STEP 1: Create the Marketer role
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_role (roleid, rolename, rolekey, rolesort, datascope, menu_check_strictly, dept_check_strictly, status, delflag, create_by, create_time, remark)
VALUES (13, 'مسوّق', 'marketer', 13, 5, true, false, 0, 0, 'admin', NOW(), 'Marketing team role - all Editor permissions except Sales')
ON CONFLICT (roleid) DO NOTHING;

-- ──────────────────────────────────────────────────────────────
-- STEP 2: Copy ALL Editor (role 3) menu assignments
--         EXCEPT sales menus
-- ──────────────────────────────────────────────────────────────
INSERT INTO sys_role_menu (role_id, menu_id)
SELECT 13, rm.menu_id
FROM sys_role_menu rm
WHERE rm.role_id = 3
  -- Exclude sales parent + children (ID range 50000-50999)
  AND rm.menu_id NOT BETWEEN 50000 AND 50999
  -- Also exclude any menu with sales:* permission (catches quotations etc. outside the range)
  AND rm.menu_id NOT IN (
      SELECT menuid FROM sys_menu WHERE perms LIKE 'sales:%'
  )
ON CONFLICT (role_id, menu_id) DO NOTHING;

COMMIT;

-- ══════════════════════════════════════════════════════════════
-- VERIFY: Run after COMMIT
-- ══════════════════════════════════════════════════════════════

-- 1) Check the new role
SELECT roleid, rolename, rolekey, rolesort, status
FROM sys_role
WHERE roleid = 13;

-- 2) Compare menu counts: Editor vs Marketer
SELECT
  (SELECT COUNT(*) FROM sys_role_menu WHERE role_id = 3) AS editor_total,
  (SELECT COUNT(*) FROM sys_role_menu WHERE role_id = 13) AS marketer_total,
  (SELECT COUNT(*) FROM sys_role_menu WHERE role_id = 3) -
  (SELECT COUNT(*) FROM sys_role_menu WHERE role_id = 13) AS sales_excluded;

-- 3) Verify NO sales permissions in Marketer role
SELECT rm.role_id, m.menuid, m.menuname, m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 13 AND (m.perms LIKE 'sales:%' OR m.menuid BETWEEN 50000 AND 50999);
-- Should return 0 rows

-- 4) Show all Marketer permissions
SELECT m.perms, m.menuname
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 13 AND m.perms IS NOT NULL AND m.perms != ''
ORDER BY m.perms;
