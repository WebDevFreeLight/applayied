-- ====================================================
-- Seed Permissions for Contract Customers
-- ====================================================
-- Contract Customers page under Contracts Management
-- Generated: 2026-02-12
-- ====================================================
-- FIXED: Column names use lowercase (SqlSugar PgSqlIsAutoToLower=true)
-- ====================================================
-- Safe to re-run: uses ON CONFLICT DO NOTHING
-- IDs: 51030-51035 (Contract Customers range)
-- ====================================================

-- ====================================================
-- SECTION 1: CONTRACT CUSTOMERS PAGE (ID: 51030)
-- Parent should already exist for Contracts Management
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (51030, 'Contract Customers', 0, 36, 'contracts/customers', NULL, '1', '0', 'C', '0', '0', 'contracts:customers:list', 'users', 'admin', NOW(), 'Contract Customers Management Page', 'sidebar-menu-contracts-customers')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 2: BUTTON-LEVEL PERMISSIONS (ID: 51031-51035)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(51031, 'Query Contract Customers', 51030, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:customers:list', '#', 'admin', NOW(), 'Query Contract Customers List'),
(51032, 'View Customer Contract Details', 51030, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:customers:view', '#', 'admin', NOW(), 'View Customer Contract Details')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 3: ASSIGN ALL CONTRACT CUSTOMER PERMISSIONS TO ADMIN (role_id = 1)
-- ====================================================

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 51030 AND 51035
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 1 AND rm.menu_id = m.menuid
  );

-- ====================================================
-- VERIFICATION
-- ====================================================
SELECT menuid, menuname, menutype, perms FROM sys_menu WHERE menuid BETWEEN 51030 AND 51035 ORDER BY menuid;
SELECT COUNT(*) as total_contract_customer_perms FROM sys_menu WHERE menuid BETWEEN 51030 AND 51035;
SELECT COUNT(*) as admin_contract_customer_menus FROM sys_role_menu WHERE role_id = 1 AND menu_id BETWEEN 51030 AND 51035;

-- ====================================================
-- DONE! Total: 1 page + 2 button permissions
-- ====================================================
-- Summary:
--   51030       Contract Customers (page)
--   51031       Query Contract Customers (button: contracts:customers:list)
--   51032       View Customer Contract Details (button: contracts:customers:view)
-- ====================================================
