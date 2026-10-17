-- ============================================================================
-- ASSIGN ALL MARKETING PERMISSIONS TO ROLE 3
-- ============================================================================
-- This script assigns every marketing-related menu/permission to role_id = 3
-- Covers BOTH legacy marketing menus AND new Marketing PR module menus
-- ============================================================================
-- Generated: 2026-02-12
-- Safe to re-run: ON CONFLICT DO NOTHING
-- ============================================================================
-- Menu ID Ranges:
--   Legacy Marketing:     2007854564761542656 .. 2007854564890000002
--   New Marketing PR:     60000 .. 61299
--   ERP Marketing:        (perms LIKE 'erp:marketing:%' or 'marketing:%')
-- ============================================================================

BEGIN;

-- ============================================================================
-- SECTION 1: ASSIGN LEGACY MARKETING MENUS TO ROLE 3
-- Parent: 2007854564761542656  (Marketing module)
-- Leads:  2007854564853817344  + buttons 345-350
-- Team:   2007854564807680000  + button 001
-- Activities: 2007854564870000000 + buttons 001-004
-- Assignments: 2007854564880000000 + buttons 001-003
-- Qualified:  2007854564890000001
-- Reports:    2007854564890000002
-- ============================================================================

-- Marketing parent
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid = 2007854564761542656
  AND NOT EXISTS (SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 3 AND rm.menu_id = m.menuid);

-- Leads page + all lead buttons (list, view, add, edit, delete, export, import)
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid IN (
    2007854564853817344,  -- Leads page
    2007854564853817345,  -- View Lead
    2007854564853817346,  -- Add Lead
    2007854564853817347,  -- Edit Lead
    2007854564853817348,  -- Delete Lead
    2007854564853817349,  -- Export Leads
    2007854564853817350   -- Import Leads
)
AND NOT EXISTS (SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 3 AND rm.menu_id = m.menuid);

-- Team/Employees page + button
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid IN (
    2007854564807680000,  -- Team page
    2007854564807680001   -- View Employee
)
AND NOT EXISTS (SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 3 AND rm.menu_id = m.menuid);

-- Activities page + buttons
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid IN (
    2007854564870000000,  -- Activities page
    2007854564870000001,  -- View Activity
    2007854564870000002,  -- Add Activity
    2007854564870000003,  -- Delete Activity
    2007854564870000004   -- Activity Stats
)
AND NOT EXISTS (SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 3 AND rm.menu_id = m.menuid);

-- Assignments page + buttons
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid IN (
    2007854564880000000,  -- Assignments page
    2007854564880000001,  -- View Assignment
    2007854564880000002,  -- Assign Lead
    2007854564880000003   -- Assignment Stats
)
AND NOT EXISTS (SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 3 AND rm.menu_id = m.menuid);

-- Qualified Customers + Reports
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid IN (
    2007854564890000001,  -- Qualified Customers
    2007854564890000002   -- Reports
)
AND NOT EXISTS (SELECT 1 FROM sys_role_menu rm WHERE rm.role_id = 3 AND rm.menu_id = m.menuid);

-- ============================================================================
-- SECTION 2: ASSIGN NEW MARKETING PR MODULE MENUS (60000-61299) TO ROLE 3
-- Dashboard, Campaigns, AI Targeting, Journeys, Investors, Crowd,
-- Email, Social Media, Content, Advertising, Public Relations, Analytics
-- ============================================================================

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE m.menuid BETWEEN 60000 AND 61299
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 3 AND rm.menu_id = m.menuid
  );

-- ============================================================================
-- SECTION 3: CATCH-ALL — assign ANY menu with marketing perms to role 3
-- This catches anything that might exist outside the known ranges
-- ============================================================================

INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 3, m.menuid, 'admin', NOW()
FROM sys_menu m
WHERE (m.perms LIKE 'marketing:%' OR m.perms LIKE 'erp:marketing:%')
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm
    WHERE rm.role_id = 3 AND rm.menu_id = m.menuid
  );

COMMIT;

-- ============================================================================
-- VERIFICATION QUERIES
-- ============================================================================

-- 1. All marketing permissions assigned to role 3
SELECT m.menuid, m.menuname, m.menutype, m.perms
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 3
  AND (m.perms LIKE 'marketing:%' OR m.perms LIKE 'erp:marketing:%' OR m.menuid BETWEEN 60000 AND 61299)
ORDER BY m.menuid;

-- 2. Count summary
SELECT 'Legacy Marketing menus' as section, COUNT(*) as cnt
FROM sys_role_menu rm
WHERE rm.role_id = 3 AND rm.menu_id > 2007854564000000000
UNION ALL
SELECT 'New Marketing PR menus' as section, COUNT(*) as cnt
FROM sys_role_menu rm
WHERE rm.role_id = 3 AND rm.menu_id BETWEEN 60000 AND 61299
UNION ALL
SELECT 'Total marketing for role 3' as section, COUNT(*) as cnt
FROM sys_role_menu rm
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE rm.role_id = 3
  AND (m.perms LIKE 'marketing:%' OR m.perms LIKE 'erp:marketing:%' OR m.menuid BETWEEN 60000 AND 61299);

-- 3. Role 3 info
SELECT role_id, role_name, role_key FROM sys_role WHERE role_id = 3;

-- ============================================================================
-- PERMISSION SUMMARY
-- ============================================================================
--
-- LEGACY MARKETING (assigned to role 3):
--   2007854564761542656  Marketing Parent (directory)
--   2007854564853817344  Leads Page       + 6 buttons (view/add/edit/delete/export/import)
--   2007854564807680000  Team Page        + 1 button (view employee)
--   2007854564870000000  Activities Page  + 4 buttons (view/add/delete/stats)
--   2007854564880000000  Assignments Page + 3 buttons (view/assign/stats)
--   2007854564890000001  Qualified Customers
--   2007854564890000002  Reports
--   Subtotal: ~20 menus
--
-- NEW MARKETING PR MODULE (assigned to role 3):
--   60000   Marketing & PR Parent
--   60100   Dashboard          + 1 button
--   60200   Campaigns          + 6 buttons
--   60300   AI Targeting       + 6 buttons
--   60400   Customer Journeys  + 6 buttons
--   60500   Investor Nexus     + 5 buttons
--   60600   Crowd Marketing    + 4 buttons
--   60700   Email Marketing    + 6 buttons
--   60800   Social Media       + 5 buttons
--   60900   Content Management + 6 buttons
--   61000   Advertising        + 8 buttons
--   61100   Public Relations   + 8 buttons
--   61200   Analytics          + 1 button
--   Subtotal: ~75 menus
--
-- GRAND TOTAL: ~95 marketing permissions for role 3
-- ============================================================================
