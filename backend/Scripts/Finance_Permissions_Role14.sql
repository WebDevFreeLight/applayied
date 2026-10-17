-- ====================================================
-- Finance Module: Comprehensive Permissions & Role 14
-- ====================================================
-- Creates all finance menu entries matching the actual
-- frontend menu-items.tsx and backend ActionPermissionFilter
-- attributes, then assigns them to Role 14 (Finance) and
-- Role 1 (Admin).
--
-- Generated: 2026-03-05
-- Safe to re-run: uses ON CONFLICT DO NOTHING
-- Column names are lowercase (SqlSugar PgSqlIsAutoToLower=true)
-- ====================================================
-- ID Ranges:
--   11           Finance parent directory
--   1100-1199    Pages (C) and sub-directories (M)
--   11000-11999  Function buttons (F)
-- ====================================================

-- ====================================================
-- SECTION 0: CREATE ROLE 14 (FINANCE) IF NOT EXISTS
-- ====================================================

INSERT INTO sys_role (role_id, role_name, role_key, role_sort, status, del_flag, data_scope, menu_check_strictly, dept_check_strictly, create_by, create_time, remark)
VALUES (14, 'Finance', 'finance', 14, 0, 0, 1, true, true, 'admin', NOW(), 'Finance department role - full access to finance module')
ON CONFLICT (role_id) DO NOTHING;

-- ====================================================
-- SECTION 1: CLEAN UP OLD GENERIC FINANCE MENUS
-- ====================================================
-- Remove old role_menu assignments first (FK safety)
DELETE FROM sys_role_menu WHERE menu_id IN (
    SELECT menuid FROM sys_menu WHERE menuid IN (11, 1100, 1101, 1102, 1103, 1104, 1105)
);
DELETE FROM sys_role_menu WHERE menu_id IN (
    SELECT menuid FROM sys_menu WHERE menuid BETWEEN 11001 AND 11053
);

-- Remove old menu entries
DELETE FROM sys_menu WHERE menuid BETWEEN 11001 AND 11053;
DELETE FROM sys_menu WHERE menuid IN (11, 1100, 1101, 1102, 1103, 1104, 1105);

-- ====================================================
-- SECTION 2: FINANCE PARENT DIRECTORY (ID: 11)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (11, 'Finance', 0, 11, 'finance', NULL, '1', '0', 'M', '0', '0', 'finance:menu:list', 'money', 'admin', NOW(), 'Finance Module', 'sidebar-menu-financial')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 3: DASHBOARD (ID: 1100)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1100, 'Finance Dashboard', 11, 1, 'dashboard', NULL, '1', '0', 'C', '0', '0', 'finance:dashboard:view', 'chart-bar', 'admin', NOW(), 'Finance Dashboard', 'sidebar-menu-financial-dashboard')
ON CONFLICT (menuid) DO NOTHING;

-- Dashboard buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11001, 'View Dashboard', 1100, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:dashboard:view', '#', 'admin', NOW(), 'View Finance Dashboard')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 4: GENERAL LEDGER (IDs: 1101-1105)
-- ====================================================

-- GL Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1101, 'General Ledger', 11, 2, 'gl', NULL, '1', '0', 'M', '0', '0', '', 'book-open', 'admin', NOW(), 'General Ledger', 'sidebar-menu-financial-general-ledger')
ON CONFLICT (menuid) DO NOTHING;

-- GL Accounts page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1102, 'Chart of Accounts', 1101, 1, 'accounts', NULL, '1', '0', 'C', '0', '0', 'finance:account:list', 'wallet', 'admin', NOW(), 'GL Chart of Accounts', 'sidebar-menu-financial-accounts')
ON CONFLICT (menuid) DO NOTHING;

-- GL Accounts buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11020, 'Query Accounts', 1102, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:account:query', '#', 'admin', NOW(), 'Query GL Accounts'),
(11021, 'Add Account', 1102, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:account:add', '#', 'admin', NOW(), 'Add GL Account'),
(11022, 'Edit Account', 1102, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:account:edit', '#', 'admin', NOW(), 'Edit GL Account'),
(11023, 'Delete Account', 1102, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:account:delete', '#', 'admin', NOW(), 'Delete GL Account'),
(11024, 'Export Accounts', 1102, 5, '', NULL, '1', '0', 'F', '0', '0', 'finance:account:export', '#', 'admin', NOW(), 'Export GL Accounts'),
(11025, 'Admin Accounts', 1102, 6, '', NULL, '1', '0', 'F', '0', '0', 'finance:account:admin', '#', 'admin', NOW(), 'Admin GL Accounts')
ON CONFLICT (menuid) DO NOTHING;

-- Journals page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1103, 'Journals', 1101, 2, 'journals', NULL, '1', '0', 'C', '0', '0', 'finance:journal:list', 'receipt', 'admin', NOW(), 'GL Journals', 'sidebar-menu-financial-journals')
ON CONFLICT (menuid) DO NOTHING;

-- Journals buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11030, 'Query Journals', 1103, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:query', '#', 'admin', NOW(), 'Query Journals'),
(11031, 'Add Journal', 1103, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:add', '#', 'admin', NOW(), 'Add Journal'),
(11032, 'Edit Journal', 1103, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:edit', '#', 'admin', NOW(), 'Edit Journal'),
(11033, 'Delete Journal', 1103, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:delete', '#', 'admin', NOW(), 'Delete Journal'),
(11034, 'Post Journal', 1103, 5, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:post', '#', 'admin', NOW(), 'Post Journal'),
(11035, 'Cancel Journal', 1103, 6, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:cancel', '#', 'admin', NOW(), 'Cancel Journal'),
(11036, 'Reverse Journal', 1103, 7, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:reverse', '#', 'admin', NOW(), 'Reverse Journal'),
(11037, 'Submit Journal', 1103, 8, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:submit', '#', 'admin', NOW(), 'Submit Journal for Approval'),
(11038, 'Approve Journal', 1103, 9, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:approve', '#', 'admin', NOW(), 'Approve Journal'),
(11039, 'Reject Journal', 1103, 10, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:reject', '#', 'admin', NOW(), 'Reject Journal'),
(11040, 'Export Journals', 1103, 11, '', NULL, '1', '0', 'F', '0', '0', 'finance:journal:export', '#', 'admin', NOW(), 'Export Journals')
ON CONFLICT (menuid) DO NOTHING;

-- Journal Entries page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1150, 'Journal Entries', 1101, 3, 'entries', NULL, '1', '0', 'C', '0', '0', 'finance:entry:list', 'file-text', 'admin', NOW(), 'GL Journal Entries', 'sidebar-menu-financial-journal-entries')
ON CONFLICT (menuid) DO NOTHING;

-- Journal Entries buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11500, 'Query Entries', 1150, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:entry:query', '#', 'admin', NOW(), 'Query Journal Entries'),
(11501, 'Add Entry', 1150, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:entry:add', '#', 'admin', NOW(), 'Add Journal Entry'),
(11502, 'Edit Entry', 1150, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:entry:edit', '#', 'admin', NOW(), 'Edit Journal Entry'),
(11503, 'Delete Entry', 1150, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:entry:delete', '#', 'admin', NOW(), 'Delete Journal Entry'),
(11504, 'Export Entries', 1150, 5, '', NULL, '1', '0', 'F', '0', '0', 'finance:entry:export', '#', 'admin', NOW(), 'Export Journal Entries')
ON CONFLICT (menuid) DO NOTHING;

-- Opening Balances page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1104, 'Opening Balances', 1101, 4, 'opening-balances', NULL, '1', '0', 'C', '0', '0', 'finance:opening-balance:list', 'calculator', 'admin', NOW(), 'GL Opening Balances', 'sidebar-menu-financial-opening-balances')
ON CONFLICT (menuid) DO NOTHING;

-- Opening Balances buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11041, 'Query Opening Balances', 1104, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:opening-balance:query', '#', 'admin', NOW(), 'Query Opening Balances'),
(11042, 'Add Opening Balance', 1104, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:opening-balance:add', '#', 'admin', NOW(), 'Add Opening Balance'),
(11043, 'Edit Opening Balance', 1104, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:opening-balance:edit', '#', 'admin', NOW(), 'Edit Opening Balance'),
(11044, 'Delete Opening Balance', 1104, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:opening-balance:delete', '#', 'admin', NOW(), 'Delete Opening Balance'),
(11045, 'Lock Opening Balances', 1104, 5, '', NULL, '1', '0', 'F', '0', '0', 'finance:opening-balance:lock', '#', 'admin', NOW(), 'Lock Opening Balances'),
(11046, 'Export Opening Balances', 1104, 6, '', NULL, '1', '0', 'F', '0', '0', 'finance:opening-balance:export', '#', 'admin', NOW(), 'Export Opening Balances')
ON CONFLICT (menuid) DO NOTHING;

-- Year-End / Period Closing page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1105, 'Year-End Closing', 1101, 5, 'year-end-closing', NULL, '1', '0', 'C', '0', '0', 'erp:finance:period:list', 'lock-key', 'admin', NOW(), 'Year-End / Period Closing', 'sidebar-menu-financial-year-end-closing')
ON CONFLICT (menuid) DO NOTHING;

-- Period Closing buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11050, 'Query Periods', 1105, 1, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:period:query', '#', 'admin', NOW(), 'Query Fiscal Periods'),
(11051, 'Add Period', 1105, 2, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:period:add', '#', 'admin', NOW(), 'Add Fiscal Period'),
(11052, 'Close Period', 1105, 3, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:period:close', '#', 'admin', NOW(), 'Close Fiscal Period'),
(11053, 'Reopen Period', 1105, 4, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:period:reopen', '#', 'admin', NOW(), 'Reopen Fiscal Period'),
(11054, 'Lock Period', 1105, 5, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:period:lock', '#', 'admin', NOW(), 'Lock Fiscal Period'),
(11055, 'Generate Periods', 1105, 6, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:period:generate', '#', 'admin', NOW(), 'Generate Fiscal Periods'),
(11056, 'Delete Period', 1105, 7, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:period:delete', '#', 'admin', NOW(), 'Delete Fiscal Period'),
(11057, 'Year-End Close', 1105, 8, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:year-end:close', '#', 'admin', NOW(), 'Execute Year-End Close'),
(11058, 'Year-End Query', 1105, 9, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:year-end:query', '#', 'admin', NOW(), 'Query Year-End Status')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 5: ACCOUNTS RECEIVABLE (IDs: 1106-1109)
-- ====================================================

-- AR Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1106, 'Accounts Receivable', 11, 3, 'arap/ar', NULL, '1', '0', 'M', '0', '0', '', 'users', 'admin', NOW(), 'Accounts Receivable', 'sidebar-menu-financial-accounts-receivable')
ON CONFLICT (menuid) DO NOTHING;

-- Customers page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1107, 'Customers', 1106, 1, 'customers', NULL, '1', '0', 'C', '0', '0', 'finance:arap:customer:list', 'users', 'admin', NOW(), 'AR Customers', 'sidebar-menu-financial-customers')
ON CONFLICT (menuid) DO NOTHING;

-- Customers buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11070, 'Query Customers', 1107, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:customer:query', '#', 'admin', NOW(), 'Query Customers'),
(11071, 'Add Customer', 1107, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:customer:add', '#', 'admin', NOW(), 'Add Customer'),
(11072, 'Edit Customer', 1107, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:customer:edit', '#', 'admin', NOW(), 'Edit Customer'),
(11073, 'Delete Customer', 1107, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:customer:delete', '#', 'admin', NOW(), 'Delete Customer')
ON CONFLICT (menuid) DO NOTHING;

-- AR Invoices page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1108, 'AR Invoices', 1106, 2, 'ar-invoices', NULL, '1', '0', 'C', '0', '0', 'finance:arap:ar-invoice:list', 'file-text', 'admin', NOW(), 'AR Invoices', 'sidebar-menu-financial-ar-invoices')
ON CONFLICT (menuid) DO NOTHING;

-- AR Invoices buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11080, 'Query AR Invoices', 1108, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:ar-invoice:query', '#', 'admin', NOW(), 'Query AR Invoices'),
(11081, 'Add AR Invoice', 1108, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:ar-invoice:add', '#', 'admin', NOW(), 'Add AR Invoice'),
(11082, 'Edit AR Invoice', 1108, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:ar-invoice:edit', '#', 'admin', NOW(), 'Edit AR Invoice'),
(11083, 'Delete AR Invoice', 1108, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:ar-invoice:delete', '#', 'admin', NOW(), 'Delete AR Invoice')
ON CONFLICT (menuid) DO NOTHING;

-- Customer Receipts page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1109, 'Customer Receipts', 1106, 3, 'customer-receipts', NULL, '1', '0', 'C', '0', '0', 'finance:arap:ar-invoice:list', 'receipt', 'admin', NOW(), 'Customer Receipts', 'sidebar-menu-financial-customer-receipts')
ON CONFLICT (menuid) DO NOTHING;

-- Credit/Debit Notes page (AR)
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1151, 'Credit/Debit Notes (AR)', 1106, 4, 'ar-notes', NULL, '1', '0', 'C', '0', '0', 'finance:arap:note:list', 'file-minus', 'admin', NOW(), 'AR Credit/Debit Notes', 'sidebar-menu-financial-ar-notes')
ON CONFLICT (menuid) DO NOTHING;

-- AR Notes buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11510, 'Query AR Notes', 1151, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:note:query', '#', 'admin', NOW(), 'Query AR Credit/Debit Notes'),
(11511, 'Add AR Note', 1151, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:note:add', '#', 'admin', NOW(), 'Add AR Credit/Debit Note'),
(11512, 'Edit AR Note', 1151, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:note:edit', '#', 'admin', NOW(), 'Edit AR Credit/Debit Note')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 6: ACCOUNTS PAYABLE (IDs: 1110-1113)
-- ====================================================

-- AP Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1110, 'Accounts Payable', 11, 4, 'arap/ap', NULL, '1', '0', 'M', '0', '0', '', 'truck', 'admin', NOW(), 'Accounts Payable', 'sidebar-menu-financial-accounts-payable')
ON CONFLICT (menuid) DO NOTHING;

-- Vendors page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1111, 'Vendors', 1110, 1, 'vendors', NULL, '1', '0', 'C', '0', '0', 'finance:arap:vendor:list', 'truck', 'admin', NOW(), 'AP Vendors', 'sidebar-menu-financial-vendors')
ON CONFLICT (menuid) DO NOTHING;

-- Vendors buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11110, 'Query Vendors', 1111, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:vendor:query', '#', 'admin', NOW(), 'Query Vendors'),
(11111, 'Add Vendor', 1111, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:vendor:add', '#', 'admin', NOW(), 'Add Vendor'),
(11112, 'Edit Vendor', 1111, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:vendor:edit', '#', 'admin', NOW(), 'Edit Vendor'),
(11113, 'Delete Vendor', 1111, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:vendor:delete', '#', 'admin', NOW(), 'Delete Vendor')
ON CONFLICT (menuid) DO NOTHING;

-- AP Invoices page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1112, 'AP Invoices', 1110, 2, 'ap-invoices', NULL, '1', '0', 'C', '0', '0', 'finance:arap:ap-invoice:list', 'file', 'admin', NOW(), 'AP Invoices', 'sidebar-menu-financial-ap-invoices')
ON CONFLICT (menuid) DO NOTHING;

-- AP Invoices buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11120, 'Query AP Invoices', 1112, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:ap-invoice:query', '#', 'admin', NOW(), 'Query AP Invoices'),
(11121, 'Add AP Invoice', 1112, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:ap-invoice:add', '#', 'admin', NOW(), 'Add AP Invoice'),
(11122, 'Edit AP Invoice', 1112, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:ap-invoice:edit', '#', 'admin', NOW(), 'Edit AP Invoice'),
(11123, 'Delete AP Invoice', 1112, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:ap-invoice:delete', '#', 'admin', NOW(), 'Delete AP Invoice')
ON CONFLICT (menuid) DO NOTHING;

-- Vendor Payments page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1113, 'Vendor Payments', 1110, 3, 'vendor-payments', NULL, '1', '0', 'C', '0', '0', 'finance:arap:ap-invoice:list', 'money', 'admin', NOW(), 'Vendor Payments', 'sidebar-menu-financial-vendor-payments')
ON CONFLICT (menuid) DO NOTHING;

-- Credit/Debit Notes page (AP)
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1152, 'Credit/Debit Notes (AP)', 1110, 4, 'ap-notes', NULL, '1', '0', 'C', '0', '0', 'finance:arap:note:list', 'file-minus', 'admin', NOW(), 'AP Credit/Debit Notes', 'sidebar-menu-financial-ap-notes')
ON CONFLICT (menuid) DO NOTHING;

-- Aging Report page (shared AR/AP)
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11090, 'Aging Report', 1109, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:arap:aging:report', '#', 'admin', NOW(), 'AR/AP Aging Report')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 7: PAYMENT TERMS (ID: 1114)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1114, 'Payment Terms', 11, 5, 'arap/payment-terms', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'calendar', 'admin', NOW(), 'Payment Terms', 'sidebar-menu-financial-payment-terms')
ON CONFLICT (menuid) DO NOTHING;

-- Payment Terms buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11140, 'Query Payment Terms', 1114, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Payment Terms'),
(11141, 'Add Payment Term', 1114, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Add Payment Term'),
(11142, 'Edit Payment Term', 1114, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Payment Term'),
(11143, 'Delete Payment Term', 1114, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Payment Term')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 8: BUDGET MANAGEMENT (ID: 1115)
-- ====================================================

INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1115, 'Budget Management', 11, 6, 'budget', NULL, '1', '0', 'C', '0', '0', 'finance:budget:list', 'calculator', 'admin', NOW(), 'Budget Management', 'sidebar-menu-financial-budget')
ON CONFLICT (menuid) DO NOTHING;

-- Budget buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11150, 'Query Budgets', 1115, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:budget:query', '#', 'admin', NOW(), 'Query Budgets'),
(11151, 'Add Budget', 1115, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:budget:add', '#', 'admin', NOW(), 'Add Budget'),
(11152, 'Edit Budget', 1115, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:budget:edit', '#', 'admin', NOW(), 'Edit Budget'),
(11153, 'Delete Budget', 1115, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:budget:delete', '#', 'admin', NOW(), 'Delete Budget'),
(11154, 'Approve Budget', 1115, 5, '', NULL, '1', '0', 'F', '0', '0', 'finance:budget:approve', '#', 'admin', NOW(), 'Approve Budget'),
(11155, 'Close Budget', 1115, 6, '', NULL, '1', '0', 'F', '0', '0', 'finance:budget:close', '#', 'admin', NOW(), 'Close Budget'),
(11156, 'Refresh Budget', 1115, 7, '', NULL, '1', '0', 'F', '0', '0', 'finance:budget:refresh', '#', 'admin', NOW(), 'Refresh Budget Actuals'),
(11157, 'Budget Report', 1115, 8, '', NULL, '1', '0', 'F', '0', '0', 'finance:budget:report', '#', 'admin', NOW(), 'View Budget vs Actual Report')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 9: TREASURY (IDs: 1116-1122)
-- ====================================================

-- Treasury Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1116, 'Treasury', 11, 7, 'treasury', NULL, '1', '0', 'M', '0', '0', '', 'vault', 'admin', NOW(), 'Treasury Management', 'sidebar-menu-financial-treasury')
ON CONFLICT (menuid) DO NOTHING;

-- Cash Accounts page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1117, 'Cash Accounts', 1116, 1, 'cash-accounts', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'coins', 'admin', NOW(), 'Cash Accounts', 'sidebar-menu-financial-treasury-cash-accounts')
ON CONFLICT (menuid) DO NOTHING;

-- Cash Accounts buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11170, 'Query Cash Accounts', 1117, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Cash Accounts'),
(11171, 'Add Cash Account', 1117, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Add Cash Account'),
(11172, 'Edit Cash Account', 1117, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Cash Account'),
(11173, 'Delete Cash Account', 1117, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Cash Account')
ON CONFLICT (menuid) DO NOTHING;

-- Bank Accounts page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1118, 'Bank Accounts', 1116, 2, 'bank-accounts', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'bank', 'admin', NOW(), 'Bank Accounts', 'sidebar-menu-financial-treasury-bank-accounts')
ON CONFLICT (menuid) DO NOTHING;

-- Bank Accounts buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11180, 'Query Bank Accounts', 1118, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Bank Accounts'),
(11181, 'Add Bank Account', 1118, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Add Bank Account'),
(11182, 'Edit Bank Account', 1118, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Bank Account'),
(11183, 'Delete Bank Account', 1118, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Bank Account')
ON CONFLICT (menuid) DO NOTHING;

-- Cheques page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1119, 'Cheques', 1116, 3, 'cheques', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'receipt', 'admin', NOW(), 'Cheques Management', 'sidebar-menu-financial-treasury-cheques')
ON CONFLICT (menuid) DO NOTHING;

-- Cheques buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11190, 'Query Cheques', 1119, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Cheques'),
(11191, 'Add Cheque', 1119, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Add Cheque'),
(11192, 'Edit Cheque', 1119, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Cheque'),
(11193, 'Delete Cheque', 1119, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Cheque')
ON CONFLICT (menuid) DO NOTHING;

-- Bank Statements page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1120, 'Bank Statements', 1116, 4, 'bank-statements', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'file-text', 'admin', NOW(), 'Bank Statements', 'sidebar-menu-financial-treasury-bank-statements')
ON CONFLICT (menuid) DO NOTHING;

-- Bank Statements buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11200, 'Query Bank Statements', 1120, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Bank Statements'),
(11201, 'Add Bank Statement', 1120, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Add Bank Statement'),
(11202, 'Edit Bank Statement', 1120, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Bank Statement'),
(11203, 'Delete Bank Statement', 1120, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Bank Statement')
ON CONFLICT (menuid) DO NOTHING;

-- Reconciliation page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1121, 'Bank Reconciliation', 1116, 5, 'reconciliation', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'scales', 'admin', NOW(), 'Bank Reconciliation', 'sidebar-menu-financial-treasury-reconciliation')
ON CONFLICT (menuid) DO NOTHING;

-- Reconciliation buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11210, 'Query Reconciliations', 1121, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Bank Reconciliations'),
(11211, 'Add Reconciliation', 1121, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Add Bank Reconciliation'),
(11212, 'Edit Reconciliation', 1121, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Bank Reconciliation'),
(11213, 'Delete Reconciliation', 1121, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Bank Reconciliation')
ON CONFLICT (menuid) DO NOTHING;

-- Treasury Config page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1122, 'Treasury Config', 1116, 6, 'config', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'gear', 'admin', NOW(), 'Treasury Configuration', 'sidebar-menu-financial-treasury-config')
ON CONFLICT (menuid) DO NOTHING;

-- Treasury Config buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11220, 'Query Treasury Config', 1122, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Treasury Config'),
(11221, 'Add Treasury Config', 1122, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Add Treasury Config'),
(11222, 'Edit Treasury Config', 1122, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Treasury Config'),
(11223, 'Delete Treasury Config', 1122, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Treasury Config'),
(11224, 'Export Treasury Config', 1122, 5, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:export', '#', 'admin', NOW(), 'Export Treasury Config')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 10: FIXED ASSETS (IDs: 1123-1126)
-- ====================================================

-- Fixed Assets Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1123, 'Fixed Assets', 11, 8, 'assets', NULL, '1', '0', 'M', '0', '0', '', 'cube', 'admin', NOW(), 'Fixed Assets', 'sidebar-menu-financial-fixed-assets')
ON CONFLICT (menuid) DO NOTHING;

-- Assets page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1124, 'Assets', 1123, 1, '', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'cube', 'admin', NOW(), 'Fixed Assets Register', 'sidebar-menu-financial-assets')
ON CONFLICT (menuid) DO NOTHING;

-- Assets buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11240, 'Query Assets', 1124, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Fixed Assets'),
(11241, 'Add Asset', 1124, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Add Fixed Asset'),
(11242, 'Edit Asset', 1124, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Fixed Asset'),
(11243, 'Delete Asset', 1124, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Fixed Asset')
ON CONFLICT (menuid) DO NOTHING;

-- Asset Categories page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1125, 'Asset Categories', 1123, 2, 'categories', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'tag', 'admin', NOW(), 'Asset Categories', 'sidebar-menu-financial-asset-categories')
ON CONFLICT (menuid) DO NOTHING;

-- Asset Categories buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11250, 'Query Asset Categories', 1125, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Asset Categories'),
(11251, 'Add Asset Category', 1125, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Add Asset Category'),
(11252, 'Edit Asset Category', 1125, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Asset Category'),
(11253, 'Delete Asset Category', 1125, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Asset Category')
ON CONFLICT (menuid) DO NOTHING;

-- Depreciation page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1126, 'Depreciation', 1123, 3, 'depreciation', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:list', 'chart-line-down', 'admin', NOW(), 'Depreciation Schedule', 'sidebar-menu-financial-depreciation')
ON CONFLICT (menuid) DO NOTHING;

-- Depreciation buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11260, 'Query Depreciation', 1126, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:query', '#', 'admin', NOW(), 'Query Depreciation Schedule'),
(11261, 'Add Depreciation', 1126, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:add', '#', 'admin', NOW(), 'Run Depreciation'),
(11262, 'Edit Depreciation', 1126, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:edit', '#', 'admin', NOW(), 'Edit Depreciation Entry'),
(11263, 'Delete Depreciation', 1126, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:delete', '#', 'admin', NOW(), 'Delete Depreciation Entry')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 11: REPORTS CENTER (IDs: 1127-1135)
-- ====================================================

-- Reports Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1127, 'Reports Center', 11, 9, 'reports', NULL, '1', '0', 'M', '0', '0', '', 'chart-pie', 'admin', NOW(), 'Financial Reports', 'sidebar-menu-financial-reports')
ON CONFLICT (menuid) DO NOTHING;

-- Trial Balance page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1128, 'Trial Balance', 1127, 1, 'gl/trial-balance', NULL, '1', '0', 'C', '0', '0', 'finance:reports:view', 'chart-line-up', 'admin', NOW(), 'Trial Balance Report', 'sidebar-menu-financial-reports-trial-balance')
ON CONFLICT (menuid) DO NOTHING;

-- Balance Sheet page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1129, 'Balance Sheet', 1127, 2, 'gl/balance-sheet', NULL, '1', '0', 'C', '0', '0', 'finance:reports:view', 'scales', 'admin', NOW(), 'Balance Sheet Report', 'sidebar-menu-financial-reports-balance-sheet')
ON CONFLICT (menuid) DO NOTHING;

-- Income Statement page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1130, 'Income Statement', 1127, 3, 'gl/income-statement', NULL, '1', '0', 'C', '0', '0', 'finance:reports:view', 'chart-bar', 'admin', NOW(), 'Income Statement Report', 'sidebar-menu-financial-reports-income-statement')
ON CONFLICT (menuid) DO NOTHING;

-- Cash Flow page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1131, 'Cash Flow Statement', 1127, 4, 'gl/cash-flow', NULL, '1', '0', 'C', '0', '0', 'finance:reports:view', 'flow-arrow', 'admin', NOW(), 'Cash Flow Statement', 'sidebar-menu-financial-reports-cash-flow')
ON CONFLICT (menuid) DO NOTHING;

-- Cost Center P&L page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1132, 'Cost Center P&L', 1127, 5, 'gl/cost-center-pl', NULL, '1', '0', 'C', '0', '0', 'finance:reports:view', 'building-office', 'admin', NOW(), 'Cost Center Profit & Loss', 'sidebar-menu-financial-reports-cost-center-pl')
ON CONFLICT (menuid) DO NOTHING;

-- Project P&L page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1133, 'Project P&L', 1127, 6, 'gl/project-pl', NULL, '1', '0', 'C', '0', '0', 'finance:reports:view', 'projector-screen-chart', 'admin', NOW(), 'Project Profit & Loss', 'sidebar-menu-financial-reports-project-pl')
ON CONFLICT (menuid) DO NOTHING;

-- Customer Aging page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1134, 'Customer Aging', 1127, 7, 'arap/customer-aging', NULL, '1', '0', 'C', '0', '0', 'finance:reports:view', 'users', 'admin', NOW(), 'Customer Aging Report', 'sidebar-menu-financial-reports-customer-aging')
ON CONFLICT (menuid) DO NOTHING;

-- Vendor Aging page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1135, 'Vendor Aging', 1127, 8, 'arap/vendor-aging', NULL, '1', '0', 'C', '0', '0', 'finance:reports:view', 'truck', 'admin', NOW(), 'Vendor Aging Report', 'sidebar-menu-financial-reports-vendor-aging')
ON CONFLICT (menuid) DO NOTHING;

-- Reports buttons (shared for all report pages)
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11270, 'View Reports', 1127, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:reports:view', '#', 'admin', NOW(), 'View Financial Reports'),
(11271, 'Generate Report', 1127, 2, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:report:generate', '#', 'admin', NOW(), 'Generate Financial Report'),
(11272, 'Delete Report', 1127, 3, '', NULL, '1', '0', 'F', '0', '0', 'erp:finance:report:delete', '#', 'admin', NOW(), 'Delete Financial Report')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 12: FINANCIAL SETTINGS (IDs: 1136-1143)
-- ====================================================

-- Settings Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1136, 'Financial Settings', 11, 10, 'configuration', NULL, '1', '0', 'M', '0', '0', '', 'gear', 'admin', NOW(), 'Financial Settings', 'sidebar-menu-financial-settings')
ON CONFLICT (menuid) DO NOTHING;

-- Fiscal Years page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1137, 'Fiscal Years', 1136, 1, 'fiscal-years', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:fiscal-year:list', 'calendar', 'admin', NOW(), 'Fiscal Years', 'sidebar-menu-financial-fiscal-years')
ON CONFLICT (menuid) DO NOTHING;

-- Fiscal Years buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11370, 'Query Fiscal Years', 1137, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:fiscal-year:query', '#', 'admin', NOW(), 'Query Fiscal Years'),
(11371, 'Add Fiscal Year', 1137, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:fiscal-year:add', '#', 'admin', NOW(), 'Add Fiscal Year'),
(11372, 'Edit Fiscal Year', 1137, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:fiscal-year:edit', '#', 'admin', NOW(), 'Edit Fiscal Year'),
(11373, 'Delete Fiscal Year', 1137, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:fiscal-year:delete', '#', 'admin', NOW(), 'Delete Fiscal Year')
ON CONFLICT (menuid) DO NOTHING;

-- Fiscal Periods page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1138, 'Fiscal Periods', 1136, 2, 'fiscal-periods', NULL, '1', '0', 'C', '0', '0', 'erp:finance:period:list', 'calendar-check', 'admin', NOW(), 'Fiscal Periods', 'sidebar-menu-financial-fiscal-periods')
ON CONFLICT (menuid) DO NOTHING;

-- Currencies page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1139, 'Currencies', 1136, 3, 'currencies', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:currency:list', 'money', 'admin', NOW(), 'Currencies', 'sidebar-menu-financial-currencies')
ON CONFLICT (menuid) DO NOTHING;

-- Currencies buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11390, 'Query Currencies', 1139, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:currency:query', '#', 'admin', NOW(), 'Query Currencies'),
(11391, 'Add Currency', 1139, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:currency:add', '#', 'admin', NOW(), 'Add Currency'),
(11392, 'Edit Currency', 1139, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:currency:edit', '#', 'admin', NOW(), 'Edit Currency'),
(11393, 'Delete Currency', 1139, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:currency:delete', '#', 'admin', NOW(), 'Delete Currency')
ON CONFLICT (menuid) DO NOTHING;

-- Exchange Rates page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1140, 'Exchange Rates', 1136, 4, 'exchange-rates', NULL, '1', '0', 'C', '0', '0', 'finance:exchangerate:list', 'arrows-left-right', 'admin', NOW(), 'Exchange Rates', 'sidebar-menu-financial-exchange-rates')
ON CONFLICT (menuid) DO NOTHING;

-- Exchange Rates buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11400, 'Query Exchange Rates', 1140, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:exchangerate:query', '#', 'admin', NOW(), 'Query Exchange Rates'),
(11401, 'Add Exchange Rate', 1140, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:exchangerate:add', '#', 'admin', NOW(), 'Add Exchange Rate'),
(11402, 'Edit Exchange Rate', 1140, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:exchangerate:edit', '#', 'admin', NOW(), 'Edit Exchange Rate'),
(11403, 'Delete Exchange Rate', 1140, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:exchangerate:delete', '#', 'admin', NOW(), 'Delete Exchange Rate'),
(11404, 'Approve Exchange Rate', 1140, 5, '', NULL, '1', '0', 'F', '0', '0', 'finance:exchangerate:approve', '#', 'admin', NOW(), 'Approve Exchange Rate')
ON CONFLICT (menuid) DO NOTHING;

-- Taxes page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1141, 'Tax Rules', 1136, 5, 'taxes', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:tax-rule:list', 'receipt', 'admin', NOW(), 'Tax Rules', 'sidebar-menu-financial-taxes')
ON CONFLICT (menuid) DO NOTHING;

-- Tax Rules buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11410, 'Query Tax Rules', 1141, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:tax-rule:query', '#', 'admin', NOW(), 'Query Tax Rules'),
(11411, 'Add Tax Rule', 1141, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:tax-rule:add', '#', 'admin', NOW(), 'Add Tax Rule'),
(11412, 'Edit Tax Rule', 1141, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:tax-rule:edit', '#', 'admin', NOW(), 'Edit Tax Rule'),
(11413, 'Delete Tax Rule', 1141, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:tax-rule:delete', '#', 'admin', NOW(), 'Delete Tax Rule')
ON CONFLICT (menuid) DO NOTHING;

-- Cost Centers page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1142, 'Cost Centers', 1136, 6, 'cost-centers', NULL, '1', '0', 'C', '0', '0', 'finance:costcenter:list', 'building-office', 'admin', NOW(), 'Cost Centers', 'sidebar-menu-financial-cost-centers')
ON CONFLICT (menuid) DO NOTHING;

-- Cost Centers buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11420, 'Query Cost Centers', 1142, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:costcenter:query', '#', 'admin', NOW(), 'Query Cost Centers'),
(11421, 'Add Cost Center', 1142, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:costcenter:add', '#', 'admin', NOW(), 'Add Cost Center'),
(11422, 'Edit Cost Center', 1142, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:costcenter:edit', '#', 'admin', NOW(), 'Edit Cost Center'),
(11423, 'Delete Cost Center', 1142, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:costcenter:delete', '#', 'admin', NOW(), 'Delete Cost Center')
ON CONFLICT (menuid) DO NOTHING;

-- General Settings page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1143, 'General Settings', 1136, 7, 'settings', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:erp-settings:list', 'gear', 'admin', NOW(), 'General Finance Settings', 'sidebar-menu-financial-settings')
ON CONFLICT (menuid) DO NOTHING;

-- General Settings buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11430, 'Query Settings', 1143, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:erp-settings:query', '#', 'admin', NOW(), 'Query ERP Settings'),
(11431, 'Add Setting', 1143, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:erp-settings:add', '#', 'admin', NOW(), 'Add ERP Setting'),
(11432, 'Edit Setting', 1143, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:erp-settings:edit', '#', 'admin', NOW(), 'Edit ERP Setting'),
(11433, 'Delete Setting', 1143, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:erp-settings:delete', '#', 'admin', NOW(), 'Delete ERP Setting')
ON CONFLICT (menuid) DO NOTHING;

-- Auto-Posting Rules page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1153, 'Auto-Posting Rules', 1136, 8, 'auto-posting', NULL, '1', '0', 'C', '0', '0', 'finance:configuration:auto-posting:list', 'lightning', 'admin', NOW(), 'Auto-Posting Configuration', 'sidebar-menu-financial-auto-posting')
ON CONFLICT (menuid) DO NOTHING;

-- Auto-Posting buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11530, 'Query Auto-Posting', 1153, 1, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:auto-posting:query', '#', 'admin', NOW(), 'Query Auto-Posting Rules'),
(11531, 'Add Auto-Posting', 1153, 2, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:auto-posting:add', '#', 'admin', NOW(), 'Add Auto-Posting Rule'),
(11532, 'Edit Auto-Posting', 1153, 3, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:auto-posting:edit', '#', 'admin', NOW(), 'Edit Auto-Posting Rule'),
(11533, 'Delete Auto-Posting', 1153, 4, '', NULL, '1', '0', 'F', '0', '0', 'finance:configuration:auto-posting:delete', '#', 'admin', NOW(), 'Delete Auto-Posting Rule')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 13: ZATCA (IDs: 1144-1146)
-- ====================================================

-- ZATCA Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1144, 'ZATCA', 11, 11, 'zatca', NULL, '1', '0', 'M', '0', '0', '', 'file-text', 'admin', NOW(), 'ZATCA E-Invoicing', 'sidebar-menu-financial-zatca-reports')
ON CONFLICT (menuid) DO NOTHING;

-- ZATCA Logs page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1145, 'ZATCA Logs', 1144, 1, 'logs', NULL, '1', '0', 'C', '0', '0', 'erp:zatca:list', 'file-text', 'admin', NOW(), 'ZATCA Submission Logs', 'sidebar-menu-financial-zatca-logs')
ON CONFLICT (menuid) DO NOTHING;

-- ZATCA Logs buttons
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(11450, 'Query ZATCA Logs', 1145, 1, '', NULL, '1', '0', 'F', '0', '0', 'erp:zatca:query', '#', 'admin', NOW(), 'Query ZATCA Logs'),
(11451, 'Create ZATCA Submission', 1145, 2, '', NULL, '1', '0', 'F', '0', '0', 'erp:zatca:create', '#', 'admin', NOW(), 'Create ZATCA Submission'),
(11452, 'Submit to ZATCA', 1145, 3, '', NULL, '1', '0', 'F', '0', '0', 'erp:zatca:submit', '#', 'admin', NOW(), 'Submit to ZATCA'),
(11453, 'Sign ZATCA Invoice', 1145, 4, '', NULL, '1', '0', 'F', '0', '0', 'erp:zatca:sign', '#', 'admin', NOW(), 'Sign ZATCA Invoice'),
(11454, 'Export ZATCA', 1145, 5, '', NULL, '1', '0', 'F', '0', '0', 'erp:zatca:export', '#', 'admin', NOW(), 'Export ZATCA Data'),
(11455, 'ZATCA Settings', 1145, 6, '', NULL, '1', '0', 'F', '0', '0', 'erp:zatca:settings', '#', 'admin', NOW(), 'ZATCA Settings'),
(11456, 'ZATCA Certificate', 1145, 7, '', NULL, '1', '0', 'F', '0', '0', 'erp:zatca:certificate', '#', 'admin', NOW(), 'ZATCA Certificate Management')
ON CONFLICT (menuid) DO NOTHING;

-- ZATCA Reports page
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe, iscache, menutype, visible, status, perms, icon, create_by, create_time, remark, menuname_key)
VALUES (1146, 'ZATCA Reports', 1144, 2, 'reports', NULL, '1', '0', 'C', '0', '0', 'erp:zatca:report', 'chart-bar', 'admin', NOW(), 'ZATCA Reports', 'sidebar-menu-financial-zatca-reports')
ON CONFLICT (menuid) DO NOTHING;

-- ====================================================
-- SECTION 14: ASSIGN ALL FINANCE MENUS TO ROLE 14 (Finance)
-- ====================================================

-- Clear any stale role 14 finance assignments first
DELETE FROM sys_role_menu WHERE role_id = 14 AND menu_id IN (
    SELECT menuid FROM sys_menu WHERE menuid = 11
       OR menuid BETWEEN 1100 AND 1199
       OR menuid BETWEEN 11000 AND 11999
);

-- Assign parent menu (ID 11)
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 14, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid = 11
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 14 AND rm.menu_id = m.menuid
  );

-- Assign all page/directory menus (1100-1199)
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 14, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 1100 AND 1199
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 14 AND rm.menu_id = m.menuid
  );

-- Assign all function buttons (11000-11999)
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 14, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 11000 AND 11999
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 14 AND rm.menu_id = m.menuid
  );

-- ====================================================
-- SECTION 15: ASSIGN ALL FINANCE MENUS TO ROLE 1 (Admin)
-- ====================================================

-- Assign parent menu (ID 11) to admin
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid = 11
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 1 AND rm.menu_id = m.menuid
  );

-- Assign all page/directory menus (1100-1199) to admin
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 1100 AND 1199
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 1 AND rm.menu_id = m.menuid
  );

-- Assign all function buttons (11000-11999) to admin
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 1, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 11000 AND 11999
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 1 AND rm.menu_id = m.menuid
  );

-- ====================================================
-- VERIFICATION QUERIES
-- ====================================================

-- 1. Count page/directory entries
SELECT COUNT(*) as page_directory_count
FROM sys_menu WHERE menuid BETWEEN 1100 AND 1199;
-- Expected: ~50+ entries (M and C types)

-- 2. Count function button entries
SELECT COUNT(*) as button_count
FROM sys_menu WHERE menuid BETWEEN 11000 AND 11999;
-- Expected: ~110+ entries (F type)

-- 3. Count total finance menus
SELECT COUNT(*) as total_finance_menus
FROM sys_menu WHERE menuid = 11 OR menuid BETWEEN 1100 AND 1199 OR menuid BETWEEN 11000 AND 11999;

-- 4. Finance menus assigned to role 14
SELECT COUNT(*) as role14_finance_menus
FROM sys_role_menu
WHERE role_id = 14
  AND (menu_id = 11 OR menu_id BETWEEN 1100 AND 1199 OR menu_id BETWEEN 11000 AND 11999);

-- 5. Finance menus assigned to role 1 (admin)
SELECT COUNT(*) as admin_finance_menus
FROM sys_role_menu
WHERE role_id = 1
  AND (menu_id = 11 OR menu_id BETWEEN 1100 AND 1199 OR menu_id BETWEEN 11000 AND 11999);

-- 6. Full menu tree view
SELECT
    m.menuid,
    m.menuname,
    m.menutype,
    m.perms,
    m.parentid,
    CASE
        WHEN m.parentid = 0 THEN 'TOP LEVEL'
        ELSE (SELECT menuname FROM sys_menu WHERE menuid = m.parentid)
    END as parent_name
FROM sys_menu m
WHERE m.menuid = 11
   OR m.menuid BETWEEN 1100 AND 1199
   OR m.menuid BETWEEN 11000 AND 11999
ORDER BY m.menuid;

-- 7. Unique permission strings
SELECT DISTINCT perms
FROM sys_menu
WHERE (menuid = 11 OR menuid BETWEEN 1100 AND 1199 OR menuid BETWEEN 11000 AND 11999)
  AND perms IS NOT NULL
  AND perms != ''
ORDER BY perms;

-- ====================================================
-- SUMMARY
-- ====================================================
-- Parent:      11        Finance Module (M)
-- Directories: 1101, 1106, 1110, 1116, 1123, 1127, 1136, 1144 (8 M-type sub-dirs)
-- Pages:       1100, 1102-1105, 1107-1109, 1111-1115, 1117-1122,
--              1124-1126, 1128-1135, 1137-1143, 1145-1146,
--              1150-1153 (~47 C-type pages)
-- Buttons:     11001, 11020-11025, 11030-11040, 11041-11046,
--              11050-11058, 11070-11073, 11080-11083, 11090,
--              11110-11113, 11120-11123, 11140-11143, 11150-11157,
--              11170-11173, 11180-11183, 11190-11193, 11200-11203,
--              11210-11213, 11220-11224, 11240-11243, 11250-11253,
--              11260-11263, 11270-11272, 11370-11373, 11390-11393,
--              11400-11404, 11410-11413, 11420-11423, 11430-11433,
--              11450-11456, 11500-11504, 11510-11512, 11530-11533
--              (~115 F-type buttons)
-- ====================================================
-- Role 14 (Finance): All finance menus assigned
-- Role 1  (Admin):   All finance menus assigned
-- ====================================================
