-- ====================================================
-- Seed Permissions for Sales Quotations (IDs: 50500-50506)
-- ====================================================
-- Generated: 2026-02-15
-- Safe to re-run: uses ON CONFLICT DO NOTHING
-- Assigns to role 3
-- ====================================================

-- ====================================================
-- SECTION 1: QUOTATIONS PAGE (ID: 50500)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (50500, 'Quotations', 50000, 6, 'quotations', NULL, '1', '0', 'C', '0', '0', 'sales:quotations:list', 'file-text', 'admin', NOW(), 'Sales Quotations Management', 'sidebar-menu-sales-quotations')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 2: QUOTATION BUTTON PERMISSIONS (IDs: 50501-50505)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(50501, 'Query Quotations', 50500, 1, '', NULL, '1', '0', 'F', '0', '0', 'sales:quotations:list', '#', 'admin', NOW(), 'Query Quotations List'),
(50502, 'Generate Quotation', 50500, 2, '', NULL, '1', '0', 'F', '0', '0', 'sales:quotations:generate', '#', 'admin', NOW(), 'Generate New Quotation'),
(50503, 'Print Quotation', 50500, 3, '', NULL, '1', '0', 'F', '0', '0', 'sales:quotations:print', '#', 'admin', NOW(), 'Print Quotation PDF'),
(50504, 'View Quotation', 50500, 4, '', NULL, '1', '0', 'F', '0', '0', 'sales:quotations:view', '#', 'admin', NOW(), 'View Quotation Details'),
(50505, 'Export Quotations', 50500, 5, '', NULL, '1', '0', 'F', '0', '0', 'sales:quotations:export', '#', 'admin', NOW(), 'Export Quotations Data')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 3: ASSIGN QUOTATION PERMISSIONS TO ROLE 3
-- ====================================================

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 50500 AND 50505
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 3 AND rm.menu_id = m.menuid
  );

-- ====================================================
-- SECTION 4: ALSO ASSIGN TO ADMIN (role_id = 1)
-- ====================================================

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 50500 AND 50505
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 1 AND rm.menu_id = m.menuid
  );

-- ====================================================
-- SECTION 5: ALSO ADD SALES PARENT MENU TO ROLE 3
-- (in case role 3 doesn't have the parent yet)
-- ====================================================

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, 50000, 'admin', NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 3 AND rm.menu_id = 50000
);

-- ====================================================
-- VERIFICATION
-- ====================================================
SELECT menuid, menuname, menutype, perms FROM sys_menu WHERE menuid BETWEEN 50500 AND 50505 ORDER BY menuid;
SELECT COUNT(*) as quotation_perms FROM sys_menu WHERE menuid BETWEEN 50500 AND 50505;
SELECT COUNT(*) as role3_quotation_menus FROM sys_role_menu WHERE role_id = 3 AND menu_id BETWEEN 50500 AND 50505;

-- ====================================================
-- DONE! Total: 1 page + 5 button permissions
-- ====================================================
-- Summary:
--   50500       Quotations (page menu under Sales 50000)
--   50501       Query Quotations     (sales:quotations:list)
--   50502       Generate Quotation   (sales:quotations:generate)
--   50503       Print Quotation      (sales:quotations:print)
--   50504       View Quotation       (sales:quotations:view)
--   50505       Export Quotations    (sales:quotations:export)
-- All assigned to role 3 and admin (role 1)
-- ====================================================
