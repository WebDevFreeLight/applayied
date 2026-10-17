-- ============================================================================
-- COMPLETE CONTRACT SYSTEM PERMISSIONS + NEW ROLE
-- ============================================================================
-- Covers ALL ERP contract module features:
--   Contracts CRUD, Lifecycle, Units, Billing Schedules, Amendments,
--   Pricing Rules, Invoices, Billing Engine, Deposits, Contract Customers
-- ============================================================================
-- Generated: 2026-02-12
-- Safe to re-run: uses ON CONFLICT DO NOTHING
-- Column names lowercase (SqlSugar PgSqlIsAutoToLower=true)
-- ============================================================================
-- ID Ranges:
--   52000-52099  Contracts System menus & permissions
--   51030-51035  Contract Customers (existing, reassigned to new role too)
--   Role ID: 20  Contracts Manager
-- ============================================================================

BEGIN;

-- ============================================================================
-- SECTION 0: CREATE "CONTRACTS MANAGER" ROLE  (role_id = 20)
-- ============================================================================

INSERT INTO sys_role (role_id, role_name, role_key, role_sort, status, data_scope, menu_check_strictly, dept_check_strictly, create_by, create_time, remark)
VALUES (20, 'Contracts Manager', 'contracts_manager', 20, '0', 1, true, true, 'admin', NOW(), 'Full access to the ERP Contracts Management module – contracts, units, billing, amendments, pricing, invoices, deposits, and contract customers.')
ON CONFLICT (role_id) DO NOTHING;

-- ============================================================================
-- SECTION 1: PARENT MODULE — Contracts Management  (52000)
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (52000, 'Contracts Management', 0, 40, 'contracts', NULL, '1', '0', 'M', '0', '0', '', 'file-text', 'admin', NOW(), 'ERP Contracts Management Module', 'sidebar-menu-contracts')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 2: CONTRACTS CRUD  (52010-52019)
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (52010, 'Contracts List', 52000, 1, 'list', NULL, '1', '0', 'C', '0', '0', 'contracts:contract:list', 'file-text', 'admin', NOW(), 'Contracts List Page', 'sidebar-menu-contracts-list')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52011, 'Query Contracts',         52010, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:contract:list',       '#', 'admin', NOW(), 'Query contracts list with pagination'),
(52012, 'View Contract',           52010, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:contract:query',      '#', 'admin', NOW(), 'View single contract details'),
(52013, 'Create Contract',         52010, 3, '', NULL, '1', '0', 'F', '0', '0', 'contracts:contract:add',        '#', 'admin', NOW(), 'Create a new contract'),
(52014, 'Edit Contract',           52010, 4, '', NULL, '1', '0', 'F', '0', '0', 'contracts:contract:edit',       '#', 'admin', NOW(), 'Update an existing contract'),
(52015, 'Delete Contract',         52010, 5, '', NULL, '1', '0', 'F', '0', '0', 'contracts:contract:delete',     '#', 'admin', NOW(), 'Delete contracts'),
(52016, 'Export Contracts',        52010, 6, '', NULL, '1', '0', 'F', '0', '0', 'contracts:contract:export',     '#', 'admin', NOW(), 'Export contracts to Excel'),
(52017, 'Contract Statistics',     52010, 7, '', NULL, '1', '0', 'F', '0', '0', 'contracts:contract:statistics', '#', 'admin', NOW(), 'View contract statistics dashboard'),
(52018, 'Check Contract Number',   52010, 8, '', NULL, '1', '0', 'F', '0', '0', 'contracts:contract:check',     '#', 'admin', NOW(), 'Check if contract number exists'),
(52019, 'Contracts Select List',   52010, 9, '', NULL, '1', '0', 'F', '0', '0', 'contracts:contract:select',    '#', 'admin', NOW(), 'Get contracts for dropdown selection')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 3: CONTRACT LIFECYCLE  (52020-52029)
-- Activate, Suspend, Resume, Terminate, Cancel, Renew, Auto-Renew
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES (52020, 'Contract Lifecycle', 52000, 2, '', NULL, '1', '0', 'C', '1', '0', 'contracts:lifecycle:manage', '#', 'admin', NOW(), 'Contract lifecycle operations parent')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52021, 'Activate Contract',        52020, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:lifecycle:activate',   '#', 'admin', NOW(), 'Activate a draft contract'),
(52022, 'Suspend Contract',         52020, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:lifecycle:suspend',    '#', 'admin', NOW(), 'Suspend an active contract'),
(52023, 'Resume Contract',          52020, 3, '', NULL, '1', '0', 'F', '0', '0', 'contracts:lifecycle:resume',     '#', 'admin', NOW(), 'Resume a suspended contract'),
(52024, 'Terminate Contract',       52020, 4, '', NULL, '1', '0', 'F', '0', '0', 'contracts:lifecycle:terminate',  '#', 'admin', NOW(), 'Terminate a contract'),
(52025, 'Cancel Contract',          52020, 5, '', NULL, '1', '0', 'F', '0', '0', 'contracts:lifecycle:cancel',     '#', 'admin', NOW(), 'Cancel a draft contract'),
(52026, 'Renew Contract',           52020, 6, '', NULL, '1', '0', 'F', '0', '0', 'contracts:lifecycle:renew',      '#', 'admin', NOW(), 'Renew an expiring contract'),
(52027, 'Process Auto-Renewals',    52020, 7, '', NULL, '1', '0', 'F', '0', '0', 'contracts:lifecycle:autorenew',  '#', 'admin', NOW(), 'Process automatic renewals for eligible contracts'),
(52028, 'Termination Financials',   52020, 8, '', NULL, '1', '0', 'F', '0', '0', 'contracts:lifecycle:termcalc',   '#', 'admin', NOW(), 'Calculate termination financials / checklist'),
(52029, 'Suspension Impact',        52020, 9, '', NULL, '1', '0', 'F', '0', '0', 'contracts:lifecycle:suspcalc',   '#', 'admin', NOW(), 'Calculate suspension billing impact')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 4: CONTRACT UNITS  (52030-52039)
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES (52030, 'Contract Units', 52000, 3, '', NULL, '1', '0', 'C', '1', '0', 'contracts:units:list', '#', 'admin', NOW(), 'Contract Units management')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52031, 'Query Units',           52030, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:units:list',    '#', 'admin', NOW(), 'Query contract units by contract'),
(52032, 'View Unit',             52030, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:units:query',   '#', 'admin', NOW(), 'View unit details'),
(52033, 'Create Unit',           52030, 3, '', NULL, '1', '0', 'F', '0', '0', 'contracts:units:add',     '#', 'admin', NOW(), 'Create new contract unit'),
(52034, 'Edit Unit',             52030, 4, '', NULL, '1', '0', 'F', '0', '0', 'contracts:units:edit',    '#', 'admin', NOW(), 'Update existing contract unit'),
(52035, 'Delete Unit',           52030, 5, '', NULL, '1', '0', 'F', '0', '0', 'contracts:units:delete',  '#', 'admin', NOW(), 'Delete contract unit'),
(52036, 'Toggle Unit Active',    52030, 6, '', NULL, '1', '0', 'F', '0', '0', 'contracts:units:toggle',  '#', 'admin', NOW(), 'Toggle unit active/inactive status'),
(52037, 'Calculate Total Price', 52030, 7, '', NULL, '1', '0', 'F', '0', '0', 'contracts:units:calc',    '#', 'admin', NOW(), 'Calculate total price for contract units')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 5: BILLING SCHEDULES  (52040-52049)
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES (52040, 'Billing Schedules', 52000, 4, '', NULL, '1', '0', 'C', '1', '0', 'contracts:billing:list', '#', 'admin', NOW(), 'Contract billing schedules management')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52041, 'Query Billing Schedules', 52040, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:billing:list',       '#', 'admin', NOW(), 'Query billing schedules by contract'),
(52042, 'View Billing Schedule',   52040, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:billing:query',      '#', 'admin', NOW(), 'View billing schedule details'),
(52043, 'Generate Schedules',      52040, 3, '', NULL, '1', '0', 'F', '0', '0', 'contracts:billing:add',        '#', 'admin', NOW(), 'Generate billing schedules for a contract'),
(52044, 'Edit Billing Schedule',   52040, 4, '', NULL, '1', '0', 'F', '0', '0', 'contracts:billing:edit',       '#', 'admin', NOW(), 'Update a billing schedule'),
(52045, 'Delete Billing Schedule', 52040, 5, '', NULL, '1', '0', 'F', '0', '0', 'contracts:billing:delete',     '#', 'admin', NOW(), 'Delete a billing schedule'),
(52046, 'Mark Invoiced/Paid',     52040, 6, '', NULL, '1', '0', 'F', '0', '0', 'contracts:billing:manage',      '#', 'admin', NOW(), 'Mark schedule as invoiced or paid'),
(52047, 'Cancel/Skip Schedule',   52040, 7, '', NULL, '1', '0', 'F', '0', '0', 'contracts:billing:cancel',      '#', 'admin', NOW(), 'Cancel or skip a billing schedule'),
(52048, 'Billing Statistics',     52040, 8, '', NULL, '1', '0', 'F', '0', '0', 'contracts:billing:statistics',   '#', 'admin', NOW(), 'View billing statistics for a contract'),
(52049, 'View Due Schedules',     52040, 9, '', NULL, '1', '0', 'F', '0', '0', 'contracts:billing:due',         '#', 'admin', NOW(), 'View schedules due for invoicing')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 6: CONTRACT AMENDMENTS  (52050-52059)
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES (52050, 'Contract Amendments', 52000, 5, '', NULL, '1', '0', 'C', '1', '0', 'contracts:amendments:list', '#', 'admin', NOW(), 'Contract amendments management')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52051, 'Query Amendments',          52050, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:amendments:list',    '#', 'admin', NOW(), 'Query amendments by contract'),
(52052, 'View Amendment',            52050, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:amendments:query',   '#', 'admin', NOW(), 'View amendment details'),
(52053, 'Create Amendment',          52050, 3, '', NULL, '1', '0', 'F', '0', '0', 'contracts:amendments:add',     '#', 'admin', NOW(), 'Create new contract amendment'),
(52054, 'Edit Amendment',            52050, 4, '', NULL, '1', '0', 'F', '0', '0', 'contracts:amendments:edit',    '#', 'admin', NOW(), 'Update existing amendment'),
(52055, 'Delete Amendment',          52050, 5, '', NULL, '1', '0', 'F', '0', '0', 'contracts:amendments:delete',  '#', 'admin', NOW(), 'Delete amendment'),
(52056, 'Submit for Approval',       52050, 6, '', NULL, '1', '0', 'F', '0', '0', 'contracts:amendments:submit',  '#', 'admin', NOW(), 'Submit amendment for approval'),
(52057, 'Approve Amendment',         52050, 7, '', NULL, '1', '0', 'F', '0', '0', 'contracts:amendments:approve', '#', 'admin', NOW(), 'Approve a pending amendment'),
(52058, 'Reject Amendment',          52050, 8, '', NULL, '1', '0', 'F', '0', '0', 'contracts:amendments:reject',  '#', 'admin', NOW(), 'Reject a pending amendment'),
(52059, 'Cancel Amendment',          52050, 9, '', NULL, '1', '0', 'F', '0', '0', 'contracts:amendments:cancel',  '#', 'admin', NOW(), 'Cancel an amendment')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 7: PRICING RULES  (52060-52069)
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES (52060, 'Pricing Rules', 52000, 6, '', NULL, '1', '0', 'C', '1', '0', 'contracts:pricing:list', '#', 'admin', NOW(), 'Contract pricing rules management')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52061, 'Query Pricing Rules',    52060, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:pricing:list',      '#', 'admin', NOW(), 'Query pricing rules by contract'),
(52062, 'View Pricing Rule',      52060, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:pricing:query',     '#', 'admin', NOW(), 'View pricing rule details'),
(52063, 'Create Pricing Rule',    52060, 3, '', NULL, '1', '0', 'F', '0', '0', 'contracts:pricing:add',       '#', 'admin', NOW(), 'Create new pricing rule'),
(52064, 'Edit Pricing Rule',      52060, 4, '', NULL, '1', '0', 'F', '0', '0', 'contracts:pricing:edit',      '#', 'admin', NOW(), 'Update existing pricing rule'),
(52065, 'Delete Pricing Rule',    52060, 5, '', NULL, '1', '0', 'F', '0', '0', 'contracts:pricing:delete',    '#', 'admin', NOW(), 'Delete pricing rule'),
(52066, 'Toggle Rule Active',     52060, 6, '', NULL, '1', '0', 'F', '0', '0', 'contracts:pricing:toggle',    '#', 'admin', NOW(), 'Toggle pricing rule active/inactive'),
(52067, 'Calculate Amount',       52060, 7, '', NULL, '1', '0', 'F', '0', '0', 'contracts:pricing:calculate', '#', 'admin', NOW(), 'Calculate contract amount with pricing rules')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 8: CONTRACT INVOICES  (52070-52079)
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES (52070, 'Contract Invoices', 52000, 7, '', NULL, '1', '0', 'C', '1', '0', 'contracts:invoices:list', '#', 'admin', NOW(), 'Contract invoices management')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52071, 'Query Contract Invoices',     52070, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:invoices:list',         '#', 'admin', NOW(), 'Query contract invoices with pagination'),
(52072, 'Create Invoice from Schedule',52070, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:invoices:add',          '#', 'admin', NOW(), 'Create AR invoice from billing schedule'),
(52073, 'Bulk Create Invoices',        52070, 3, '', NULL, '1', '0', 'F', '0', '0', 'contracts:invoices:bulkadd',      '#', 'admin', NOW(), 'Bulk create invoices from multiple schedules'),
(52074, 'Auto-Generate Invoices',      52070, 4, '', NULL, '1', '0', 'F', '0', '0', 'contracts:invoices:autogenerate', '#', 'admin', NOW(), 'Auto-generate invoices for due schedules'),
(52075, 'Void Invoice',                52070, 5, '', NULL, '1', '0', 'F', '0', '0', 'contracts:invoices:void',         '#', 'admin', NOW(), 'Void an invoice and reopen billing schedule'),
(52076, 'Validate Schedule',           52070, 6, '', NULL, '1', '0', 'F', '0', '0', 'contracts:invoices:validate',     '#', 'admin', NOW(), 'Validate schedule before invoice creation'),
(52077, 'Export Contract Invoices',    52070, 7, '', NULL, '1', '0', 'F', '0', '0', 'contracts:invoices:export',       '#', 'admin', NOW(), 'Export contract invoices to Excel')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 9: BILLING ENGINE & ANALYTICS  (52080-52089)
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES (52080, 'Billing Engine', 52000, 8, '', NULL, '1', '0', 'C', '1', '0', 'contracts:engine:list', '#', 'admin', NOW(), 'Billing engine and pricing calculations')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52081, 'Generate Invoices',      52080, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:engine:generate',  '#', 'admin', NOW(), 'Generate invoices via billing engine'),
(52082, 'Preview Invoice',        52080, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:engine:preview',   '#', 'admin', NOW(), 'Preview invoice before generation'),
(52083, 'Price Calculation',      52080, 3, '', NULL, '1', '0', 'F', '0', '0', 'contracts:engine:calculate', '#', 'admin', NOW(), 'Calculate price with adjustments, escalation, seasonal'),
(52084, 'Billing Summary',        52080, 4, '', NULL, '1', '0', 'F', '0', '0', 'contracts:engine:summary',   '#', 'admin', NOW(), 'View billing summary for contract'),
(52085, 'Billing Timeline',       52080, 5, '', NULL, '1', '0', 'F', '0', '0', 'contracts:engine:timeline',  '#', 'admin', NOW(), 'View billing timeline for contract'),
(52086, 'Overdue Invoices',       52080, 6, '', NULL, '1', '0', 'F', '0', '0', 'contracts:engine:overdue',   '#', 'admin', NOW(), 'View overdue invoice report'),
(52087, 'Revenue Forecast',       52080, 7, '', NULL, '1', '0', 'F', '0', '0', 'contracts:engine:forecast',  '#', 'admin', NOW(), 'View revenue forecast'),
(52088, 'Batch Generate Invoices',52080, 8, '', NULL, '1', '0', 'F', '0', '0', 'contracts:engine:batch',     '#', 'admin', NOW(), 'Batch generate invoices for all due schedules')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 10: DEPOSIT MANAGEMENT  (52090-52095)
-- ============================================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES (52090, 'Deposit Management', 52000, 9, '', NULL, '1', '0', 'C', '1', '0', 'contracts:deposits:list', '#', 'admin', NOW(), 'Security deposit management')
ON CONFLICT (menuid) DO NOTHING;

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(52091, 'Record Deposit',        52090, 1, '', NULL, '1', '0', 'F', '0', '0', 'contracts:deposits:add',       '#', 'admin', NOW(), 'Record security deposit payment'),
(52092, 'Process Deposit Refund',52090, 2, '', NULL, '1', '0', 'F', '0', '0', 'contracts:deposits:refund',    '#', 'admin', NOW(), 'Process deposit refund on termination'),
(52093, 'Apply Deduction',       52090, 3, '', NULL, '1', '0', 'F', '0', '0', 'contracts:deposits:deduction', '#', 'admin', NOW(), 'Apply deduction against deposit balance'),
(52094, 'View Deposit Balance',  52090, 4, '', NULL, '1', '0', 'F', '0', '0', 'contracts:deposits:query',     '#', 'admin', NOW(), 'View deposit balance for contract')
ON CONFLICT (menuid) DO NOTHING;

-- ============================================================================
-- SECTION 11: CONTRACT CUSTOMERS  (51030-51035 — already exist)
-- Re-parent under the new contracts module parent (52000)
-- ============================================================================

-- Update the parent of contract customers page to sit under contracts module
UPDATE sys_menu SET parentid = 52000, ordernum = 10 WHERE menuid = 51030 AND parentid != 52000;

-- ============================================================================
-- SECTION 12: ASSIGN ALL CONTRACT PERMISSIONS TO ADMIN (role_id = 1)
-- ============================================================================

-- Assign all new contract menus (52000-52099) to admin
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 52000 AND 52099
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 1 AND rm.menu_id = m.menuid
  );

-- Also ensure contract customers (51030-51035) are assigned to admin
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 51030 AND 51035
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 1 AND rm.menu_id = m.menuid
  );

-- ============================================================================
-- SECTION 13: ASSIGN ALL CONTRACT PERMISSIONS TO CONTRACTS MANAGER (role_id = 20)
-- ============================================================================

-- Assign all contract system menus (52000-52099) to contracts manager role
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 20, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 52000 AND 52099
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 20 AND rm.menu_id = m.menuid
  );

-- Assign contract customers (51030-51035) to contracts manager role
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 20, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 51030 AND 51035
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 20 AND rm.menu_id = m.menuid
  );

COMMIT;

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- All contract system permissions
SELECT menuid, menuname, menutype, perms, parentid
FROM sys_menu
WHERE menuid BETWEEN 52000 AND 52099
   OR menuid BETWEEN 51030 AND 51035
ORDER BY menuid;

-- Counts
SELECT 'Total contract permissions' as label, COUNT(*) as cnt FROM sys_menu WHERE menuid BETWEEN 52000 AND 52099;
SELECT 'Contract customers perms'   as label, COUNT(*) as cnt FROM sys_menu WHERE menuid BETWEEN 51030 AND 51035;

-- Role verification
SELECT r.role_id, r.role_name, r.role_key, COUNT(rm.menu_id) as menu_count
FROM sys_role r
LEFT JOIN sys_role_menu rm ON rm.role_id = r.role_id AND (rm.menu_id BETWEEN 52000 AND 52099 OR rm.menu_id BETWEEN 51030 AND 51035)
WHERE r.role_id IN (1, 20)
GROUP BY r.role_id, r.role_name, r.role_key;

-- ============================================================================
-- PERMISSION SUMMARY  (75 total permissions)
-- ============================================================================
--
-- ROLE: Contracts Manager (role_id = 20, role_key = contracts_manager)
--
-- ┌─────────┬──────────────────────────┬──────────┬──────────────────────────────────────┐
-- │  Range  │        Section           │  Count   │  Permission Prefix                   │
-- ├─────────┼──────────────────────────┼──────────┼──────────────────────────────────────┤
-- │  52000  │ Module Parent            │    1     │ (directory)                          │
-- │ 52010-19│ Contracts CRUD           │   10     │ contracts:contract:*                 │
-- │ 52020-29│ Lifecycle                │   10     │ contracts:lifecycle:*                │
-- │ 52030-37│ Units                    │    8     │ contracts:units:*                    │
-- │ 52040-49│ Billing Schedules        │   10     │ contracts:billing:*                  │
-- │ 52050-59│ Amendments               │   10     │ contracts:amendments:*               │
-- │ 52060-67│ Pricing Rules            │    8     │ contracts:pricing:*                  │
-- │ 52070-77│ Contract Invoices        │    8     │ contracts:invoices:*                 │
-- │ 52080-88│ Billing Engine           │    9     │ contracts:engine:*                   │
-- │ 52090-94│ Deposit Management       │    5     │ contracts:deposits:*                 │
-- │ 51030-32│ Contract Customers       │    3     │ contracts:customers:*                │
-- ├─────────┼──────────────────────────┼──────────┼──────────────────────────────────────┤
-- │  TOTAL  │                          │   82     │                                      │
-- └─────────┴──────────────────────────┴──────────┴──────────────────────────────────────┘
--
-- All 82 permissions assigned to:
--   • Admin          (role_id = 1)
--   • Contracts Mgr  (role_id = 20)
-- ============================================================================
