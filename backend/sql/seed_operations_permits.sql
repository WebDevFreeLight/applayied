-- ============================================================
-- Operations Permits Module - Complete Seed & Permissions
-- Database: PostgreSQL (MahamDB)
-- Run: docker exec postgres psql -U abdulqader -d MahamDB -f /sql/seed_operations_permits.sql
--   or: copy into container first, then run
-- ============================================================

BEGIN;

-- ============ 1. ROLE ============
INSERT INTO sys_role (roleid, rolename, rolekey, rolesort, status, delflag, datascope, menu_check_strictly, dept_check_strictly, create_by, create_time)
VALUES (11, 'Operation Manager', 'operation_manager', 11, '0', '0', '1', true, true, 'admin', NOW())
ON CONFLICT (roleid) DO NOTHING;

-- ============ 2. SEED: WORK TYPES ============
DELETE FROM ops_work_type WHERE work_type_id BETWEEN 1001 AND 1010;
INSERT INTO ops_work_type (work_type_id, code, name, name_ar, display_order, is_active, create_by, create_time) VALUES
  (1001, 'CIVIL', 'Civil Work',   'أعمال مدنية',       1,  true, 'admin', NOW()),
  (1002, 'MECH',  'Mechanical',   'أعمال ميكانيكية',    2,  true, 'admin', NOW()),
  (1003, 'ELEC',  'Electrical',   'أعمال كهربائية',     3,  true, 'admin', NOW()),
  (1004, 'PLUMB', 'Plumbing',     'أعمال سباكة',       4,  true, 'admin', NOW()),
  (1005, 'PAINT', 'Painting',     'أعمال دهان',        5,  true, 'admin', NOW()),
  (1006, 'WELD',  'Welding',      'أعمال لحام',        6,  true, 'admin', NOW()),
  (1007, 'CARP',  'Carpentry',    'أعمال نجارة',       7,  true, 'admin', NOW()),
  (1008, 'HVAC',  'HVAC',         'تكييف وتبريد',      8,  true, 'admin', NOW()),
  (1009, 'FIRE',  'Fire Systems', 'أنظمة إطفاء',       9,  true, 'admin', NOW()),
  (1010, 'LAND',  'Landscaping',  'تنسيق حدائق',      10,  true, 'admin', NOW());

-- ============ 3. SEED: WORK CATEGORIES ============
DELETE FROM ops_work_category WHERE work_category_id BETWEEN 2001 AND 2007;
INSERT INTO ops_work_category (work_category_id, code, name, name_ar, display_order, is_active, create_by, create_time) VALUES
  (2001, 'HOT',    'Hot Work',          'أعمال ساخنة',    1, true, 'admin', NOW()),
  (2002, 'COLD',   'Cold Work',         'أعمال باردة',    2, true, 'admin', NOW()),
  (2003, 'HEIGHT', 'Work at Height',    'عمل على ارتفاع', 3, true, 'admin', NOW()),
  (2004, 'HEAVY',  'Heavy Maintenance', 'صيانة ثقيلة',    4, true, 'admin', NOW()),
  (2005, 'LIGHT',  'Light Maintenance', 'صيانة خفيفة',    5, true, 'admin', NOW()),
  (2006, 'CONF',   'Confined Space',    'أماكن مغلقة',    6, true, 'admin', NOW()),
  (2007, 'EXCAV',  'Excavation',        'أعمال حفر',      7, true, 'admin', NOW());

-- ============ 4. SEED: SAFETY PRECAUTIONS ============
DELETE FROM ops_safety_precaution WHERE precaution_id BETWEEN 3001 AND 3012;
INSERT INTO ops_safety_precaution (precaution_id, code, description, description_ar, display_order, is_active, create_by, create_time) VALUES
  (3001, 'SAFETY_MTG',  'Pre-job safety meeting conducted',           'تم عقد اجتماع السلامة قبل بدء العمل',        1,  true, 'admin', NOW()),
  (3002, 'ISOLATION',   'Area isolation and barricading done',        'تم عزل وتسييج منطقة العمل',                  2,  true, 'admin', NOW()),
  (3003, 'ELEC_SAFE',   'Electrical isolation verified',              'تم التحقق من العزل الكهربائي',                3,  true, 'admin', NOW()),
  (3004, 'CHEM_SAFE',   'Chemical hazards identified and controlled', 'تم تحديد المخاطر الكيميائية والسيطرة عليها',   4,  true, 'admin', NOW()),
  (3005, 'EVAC_PLAN',   'Evacuation plan communicated',               'تم إبلاغ خطة الإخلاء',                       5,  true, 'admin', NOW()),
  (3006, 'BARRIERS',    'Safety barriers and signs installed',        'تم تركيب حواجز وعلامات السلامة',              6,  true, 'admin', NOW()),
  (3007, 'TOOLS_CHK',   'Tools and equipment inspected',              'تم فحص الأدوات والمعدات',                     7,  true, 'admin', NOW()),
  (3008, 'PPE',         'Personal protective equipment provided',     'تم توفير معدات الحماية الشخصية',              8,  true, 'admin', NOW()),
  (3009, 'VENTILATION', 'Adequate ventilation ensured',               'تم التأكد من التهوية الكافية',                9,  true, 'admin', NOW()),
  (3010, 'LIFT_PLAN',   'Lifting plan reviewed and approved',         'تم مراجعة واعتماد خطة الرفع',                10, true, 'admin', NOW()),
  (3011, 'HARNESS',     'Safety harness / seat belt required',        'حزام الأمان مطلوب',                          11, true, 'admin', NOW()),
  (3012, 'FIRE_EXT',    'Fire extinguisher available on site',        'طفاية حريق متوفرة في الموقع',                12, true, 'admin', NOW());

-- ============ 5. PERMISSIONS: MENU ENTRIES ============
-- Clean up if re-running
DELETE FROM sys_role_menu WHERE menu_id BETWEEN 52000 AND 52055;
DELETE FROM sys_menu WHERE menuid BETWEEN 52000 AND 52055;

-- Parent directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, menutype, visible, status, perms, icon, create_by, create_time) VALUES
  (52000, 'Registration & Permits', 51000, 1, 'registration-permits', 'M', '0', '0', '', 'id-card', 'admin', NOW());

-- Permits Management
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, menutype, visible, status, perms, icon, create_by, create_time) VALUES
  (52010, 'Permits Management', 52000, 1, 'permits', 'C', '0', '0', 'operations:permit:list', 'certificate', 'admin', NOW());
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, menutype, visible, status, perms, create_by, create_time) VALUES
  (52011, 'Query Permits',   52010, 1, 'F', '0', '0', 'operations:permit:query',   'admin', NOW()),
  (52012, 'Add Permit',      52010, 2, 'F', '0', '0', 'operations:permit:add',     'admin', NOW()),
  (52013, 'Edit Permit',     52010, 3, 'F', '0', '0', 'operations:permit:edit',    'admin', NOW()),
  (52014, 'Delete Permit',   52010, 4, 'F', '0', '0', 'operations:permit:delete',  'admin', NOW()),
  (52015, 'Approve Permit',  52010, 5, 'F', '0', '0', 'operations:permit:approve', 'admin', NOW()),
  (52016, 'Export Permits',  52010, 6, 'F', '0', '0', 'operations:permit:export',  'admin', NOW());

-- Permit Types
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, menutype, visible, status, perms, icon, create_by, create_time) VALUES
  (52020, 'Permit Types', 52000, 2, 'permit-types', 'C', '0', '0', 'operations:permittype:list', 'file-text', 'admin', NOW());
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, menutype, visible, status, perms, create_by, create_time) VALUES
  (52021, 'Query Permit Types',  52020, 1, 'F', '0', '0', 'operations:permittype:query',  'admin', NOW()),
  (52022, 'Add Permit Type',     52020, 2, 'F', '0', '0', 'operations:permittype:add',    'admin', NOW()),
  (52023, 'Edit Permit Type',    52020, 3, 'F', '0', '0', 'operations:permittype:edit',   'admin', NOW()),
  (52024, 'Delete Permit Type',  52020, 4, 'F', '0', '0', 'operations:permittype:delete', 'admin', NOW()),
  (52025, 'Export Permit Types', 52020, 5, 'F', '0', '0', 'operations:permittype:export', 'admin', NOW());

-- Permit Settings (Work Types / Categories / Safety Precautions)
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, menutype, visible, status, perms, icon, create_by, create_time) VALUES
  (52030, 'Permit Settings', 52000, 3, 'settings', 'C', '0', '0', 'operations:worktype:list', 'gear', 'admin', NOW());
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, menutype, visible, status, perms, create_by, create_time) VALUES
  -- Work Types
  (52031, 'Query Work Types',  52030, 1,  'F', '0', '0', 'operations:worktype:query',  'admin', NOW()),
  (52032, 'Add Work Type',     52030, 2,  'F', '0', '0', 'operations:worktype:add',    'admin', NOW()),
  (52033, 'Edit Work Type',    52030, 3,  'F', '0', '0', 'operations:worktype:edit',   'admin', NOW()),
  (52034, 'Delete Work Type',  52030, 4,  'F', '0', '0', 'operations:worktype:delete', 'admin', NOW()),
  (52035, 'Export Work Types', 52030, 5,  'F', '0', '0', 'operations:worktype:export', 'admin', NOW()),
  -- Work Categories
  (52036, 'Query Work Categories',  52030, 6,  'F', '0', '0', 'operations:workcategory:query',  'admin', NOW()),
  (52037, 'Add Work Category',      52030, 7,  'F', '0', '0', 'operations:workcategory:add',    'admin', NOW()),
  (52038, 'Edit Work Category',     52030, 8,  'F', '0', '0', 'operations:workcategory:edit',   'admin', NOW()),
  (52039, 'Delete Work Category',   52030, 9,  'F', '0', '0', 'operations:workcategory:delete', 'admin', NOW()),
  (52040, 'Export Work Categories', 52030, 10, 'F', '0', '0', 'operations:workcategory:export', 'admin', NOW()),
  -- Safety Precautions
  (52041, 'Query Safety Precautions',  52030, 11, 'F', '0', '0', 'operations:safetyprecaution:query',  'admin', NOW()),
  (52042, 'Add Safety Precaution',     52030, 12, 'F', '0', '0', 'operations:safetyprecaution:add',    'admin', NOW()),
  (52043, 'Edit Safety Precaution',    52030, 13, 'F', '0', '0', 'operations:safetyprecaution:edit',   'admin', NOW()),
  (52044, 'Delete Safety Precaution',  52030, 14, 'F', '0', '0', 'operations:safetyprecaution:delete', 'admin', NOW()),
  (52045, 'Export Safety Precautions', 52030, 15, 'F', '0', '0', 'operations:safetyprecaution:export', 'admin', NOW());

-- Customer Workers
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, menutype, visible, status, perms, create_by, create_time) VALUES
  (52050, 'Customer Workers',        52000, 4, 'C', '0', '0', 'operations:customerworker:list',   'admin', NOW()),
  (52051, 'Query Customer Workers',  52050, 1, 'F', '0', '0', 'operations:customerworker:query',  'admin', NOW()),
  (52052, 'Add Customer Worker',     52050, 2, 'F', '0', '0', 'operations:customerworker:add',    'admin', NOW()),
  (52053, 'Edit Customer Worker',    52050, 3, 'F', '0', '0', 'operations:customerworker:edit',   'admin', NOW()),
  (52054, 'Delete Customer Worker',  52050, 4, 'F', '0', '0', 'operations:customerworker:delete', 'admin', NOW()),
  (52055, 'Export Customer Workers', 52050, 5, 'F', '0', '0', 'operations:customerworker:export', 'admin', NOW());

-- ============ 6. ASSIGN ALL TO ROLE 11 ============
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
SELECT 11, menuid, 'admin', NOW()
FROM sys_menu
WHERE menuid BETWEEN 52000 AND 52055;

-- Also ensure parent Operations menu is assigned
INSERT INTO sys_role_menu (role_id, menu_id, create_by, create_time)
VALUES (11, 51000, 'admin', NOW())
ON CONFLICT DO NOTHING;

-- ============ 7. VERIFY ============
SELECT '=== ROLE ===' AS section;
SELECT roleid, rolename, rolekey FROM sys_role WHERE roleid = 11;

SELECT '=== SEED COUNTS ===' AS section;
SELECT 'ops_work_type' AS tbl, COUNT(*) FROM ops_work_type
UNION ALL SELECT 'ops_work_category', COUNT(*) FROM ops_work_category
UNION ALL SELECT 'ops_safety_precaution', COUNT(*) FROM ops_safety_precaution;

SELECT '=== PERMISSIONS ===' AS section;
SELECT COUNT(*) AS menu_entries FROM sys_menu WHERE menuid BETWEEN 52000 AND 52055;
SELECT COUNT(*) AS role_assignments FROM sys_role_menu WHERE role_id = 11;

COMMIT;
