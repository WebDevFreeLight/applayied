-- =============================================
-- MARKETING MODULE - MENU & PERMISSIONS
-- Parent ID: 4
-- =============================================

-- Marketing Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(4, 'Marketing', 0, 4, 'marketing', NULL, '1', '0', 'M', '0', '0', '', 'chart', 'admin', NOW(), 'Marketing Module')
ON CONFLICT (menuid) DO NOTHING;

-- Leads
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(400, 'Leads', 4, 1, 'leads', 'marketing/leads/index', '1', '0', 'C', '0', '0', 'marketing:leads:list',
'peoples', 'admin', NOW(), 'Leads Management'),
(4001, 'View Lead', 400, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:leads:view', '#', 'admin', NOW(),
'View Lead'),
(4002, 'Add Lead', 400, 2, '', NULL, '1', '0', 'F', '0', '0', 'marketing:leads:add', '#', 'admin', NOW(),
'Add Lead'),
(4003, 'Edit Lead', 400, 3, '', NULL, '1', '0', 'F', '0', '0', 'marketing:leads:edit', '#', 'admin', NOW(),
'Edit Lead'),
(4004, 'Delete Lead', 400, 4, '', NULL, '1', '0', 'F', '0', '0', 'marketing:leads:delete', '#', 'admin',
NOW(), 'Delete Lead'),
(4005, 'Export Leads', 400, 5, '', NULL, '1', '0', 'F', '0', '0', 'marketing:leads:export', '#', 'admin',
NOW(), 'Export Leads'),
(4006, 'Import Leads', 400, 6, '', NULL, '1', '0', 'F', '0', '0', 'marketing:leads:import', '#', 'admin',
NOW(), 'Import Leads')
ON CONFLICT (menuid) DO NOTHING;

-- Activities
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(401, 'Lead Activities', 4, 2, 'activities', 'marketing/activities/index', '1', '0', 'C', '0', '0',
'marketing:activities:list', 'activity', 'admin', NOW(), 'Lead Activities'),
(4011, 'View Activity', 401, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:activities:view', '#', 'admin', NOW(), 'View Activity'),
(4012, 'Add Activity', 401, 2, '', NULL, '1', '0', 'F', '0', '0', 'marketing:activities:add', '#', 'admin',
NOW(), 'Add Activity'),
(4013, 'Delete Activity', 401, 3, '', NULL, '1', '0', 'F', '0', '0', 'marketing:activities:delete', '#',
'admin', NOW(), 'Delete Activity'),
(4014, 'Activity Stats', 401, 4, '', NULL, '1', '0', 'F', '0', '0', 'marketing:activities:stats', '#',
'admin', NOW(), 'Activity Stats')
ON CONFLICT (menuid) DO NOTHING;

-- Assignments
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(402, 'Lead Assignments', 4, 3, 'assignments', 'marketing/assignments/index', '1', '0', 'C', '0', '0',
'marketing:assignments:list', 'user-check', 'admin', NOW(), 'Lead Assignments'),
(4021, 'View Assignment', 402, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:assignments:view', '#',
'admin', NOW(), 'View Assignment'),
(4022, 'Assign Lead', 402, 2, '', NULL, '1', '0', 'F', '0', '0', 'marketing:assignments:assign', '#',
'admin', NOW(), 'Assign Lead'),
(4023, 'Assignment Stats', 402, 3, '', NULL, '1', '0', 'F', '0', '0', 'marketing:assignments:stats', '#',
'admin', NOW(), 'Assignment Stats')
ON CONFLICT (menuid) DO NOTHING;

-- Team
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(403, 'Marketing Team', 4, 4, 'employees', 'marketing/employees/index', '1', '0', 'C', '0', '0',
'marketing:employees:list', 'peoples', 'admin', NOW(), 'Marketing Team'),
(4031, 'Query Employee', 403, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:employees:query', '#',
'admin', NOW(), 'Query Employee')
ON CONFLICT (menuid) DO NOTHING;

-- Meetings
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(404, 'Meetings', 4, 5, 'meetings', 'marketing/meetings/index', '1', '0', 'C', '0', '0',
'marketing:meetings:list', 'date', 'admin', NOW(), 'Meetings'),
(4041, 'View Meeting', 404, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:meetings:view', '#', 'admin',
NOW(), 'View Meeting'),
(4042, 'Add Meeting', 404, 2, '', NULL, '1', '0', 'F', '0', '0', 'marketing:meetings:add', '#', 'admin',
NOW(), 'Add Meeting'),
(4043, 'Edit Meeting', 404, 3, '', NULL, '1', '0', 'F', '0', '0', 'marketing:meetings:edit', '#', 'admin',
NOW(), 'Edit Meeting'),
(4044, 'Cancel Meeting', 404, 4, '', NULL, '1', '0', 'F', '0', '0', 'marketing:meetings:cancel', '#',
'admin', NOW(), 'Cancel Meeting'),
(4045, 'Complete Meeting', 404, 5, '', NULL, '1', '0', 'F', '0', '0', 'marketing:meetings:complete', '#',
'admin', NOW(), 'Complete Meeting'),
(4046, 'Meeting Stats', 404, 6, '', NULL, '1', '0', 'F', '0', '0', 'marketing:meetings:stats', '#', 'admin',
NOW(), 'Meeting Stats')
ON CONFLICT (menuid) DO NOTHING;

-- Promotions
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(405, 'Promotions', 4, 6, 'promotions', 'marketing/promotions/index', '1', '0', 'C', '0', '0',
'marketing:promotion:list', 'shopping', 'admin', NOW(), 'Promotions'),
(4051, 'Query Promotion', 405, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promotion:query', '#',
'admin', NOW(), 'Query Promotion'),
(4052, 'Add Promotion', 405, 2, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promotion:add', '#', 'admin',
NOW(), 'Add Promotion'),
(4053, 'Edit Promotion', 405, 3, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promotion:edit', '#', 'admin', NOW(), 'Edit Promotion'),
(4054, 'Delete Promotion', 405, 4, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promotion:delete', '#',
'admin', NOW(), 'Delete Promotion'),
(4055, 'Export Promotion', 405, 5, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promotion:export', '#',
'admin', NOW(), 'Export Promotion')
ON CONFLICT (menuid) DO NOTHING;

-- Gift Cards
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(406, 'Gift Cards', 4, 7, 'giftcards', 'marketing/giftcards/index', '1', '0', 'C', '0', '0',
'marketing:giftcard:list', 'gift', 'admin', NOW(), 'Gift Cards'),
(4061, 'Query Gift Card', 406, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:query', '#',
'admin', NOW(), 'Query Gift Card'),
(4062, 'Add Gift Card', 406, 2, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:add', '#', 'admin',
NOW(), 'Add Gift Card'),
(4063, 'Edit Gift Card', 406, 3, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:edit', '#', 'admin',
NOW(), 'Edit Gift Card'),
(4064, 'Delete Gift Card', 406, 4, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:delete', '#',
'admin', NOW(), 'Delete Gift Card'),
(4065, 'Activate Gift Card', 406, 5, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:activate', '#',
'admin', NOW(), 'Activate Gift Card'),
(4066, 'Redeem Gift Card', 406, 6, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:redeem', '#',
'admin', NOW(), 'Redeem Gift Card'),
(4067, 'Refund Gift Card', 406, 7, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:refund', '#',
'admin', NOW(), 'Refund Gift Card'),
(4068, 'Cancel Gift Card', 406, 8, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:cancel', '#',
'admin', NOW(), 'Cancel Gift Card'),
(4069, 'Send Gift Card', 406, 9, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:send', '#', 'admin',
NOW(), 'Send Gift Card'),
(4070, 'Gift Card Stats', 406, 10, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:statistics', '#',
'admin', NOW(), 'Gift Card Stats'),
(4071, 'Export Gift Cards', 406, 11, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:export', '#',
'admin', NOW(), 'Export Gift Cards'),
(40610, 'Use Gift Card', 406, 12, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:use', '#', 'admin',
NOW(), 'Use Gift Card'),
(40611, 'Deactivate Gift Card', 406, 13, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:deactivate',
'#', 'admin', NOW(), 'Deactivate Gift Card'),
(40612, 'Gift Card Report', 406, 14, '', NULL, '1', '0', 'F', '0', '0', 'marketing:giftcard:report', '#',
'admin', NOW(), 'Gift Card Report')
ON CONFLICT (menuid) DO NOTHING;

-- Loyalty Program
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(407, 'Loyalty Program', 4, 8, 'loyalty', 'marketing/loyalty/index', '1', '0', 'C', '0', '0',
'marketing:loyalty:list', 'star', 'admin', NOW(), 'Loyalty Program'),
(4072, 'Query Loyalty', 407, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:query', '#', 'admin',
NOW(), 'Query Loyalty'),
(4073, 'Earn Points', 407, 2, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:earn', '#', 'admin',
NOW(), 'Earn Points'),
(4074, 'Redeem Points', 407, 3, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:redeem', '#', 'admin',
NOW(), 'Redeem Points'),
(4075, 'Manage Tiers', 407, 4, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:tiers', '#', 'admin',
NOW(), 'Manage Tiers'),
(4076, 'Add Tier', 407, 5, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:tiers:add', '#', 'admin',
NOW(), 'Add Tier'),
(4077, 'Edit Tier', 407, 6, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:tiers:edit', '#', 'admin',
NOW(), 'Edit Tier'),
(4078, 'Delete Tier', 407, 7, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:tiers:delete', '#',
'admin', NOW(), 'Delete Tier'),
(4079, 'Loyalty Analytics', 407, 8, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:analytics', '#',
'admin', NOW(), 'Loyalty Analytics'),
(4080, 'Manage Rules', 407, 9, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:rules', '#', 'admin',
NOW(), 'Manage Rules'),
(4081, 'Add Rule', 407, 10, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:rules:add', '#', 'admin',
NOW(), 'Add Rule'),
(4082, 'Edit Rule', 407, 11, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:rules:edit', '#', 'admin', NOW(), 'Edit Rule'),
(4083, 'Delete Rule', 407, 12, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:rules:delete', '#',
'admin', NOW(), 'Delete Rule'),
(40713, 'Add Program', 407, 13, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:add', '#', 'admin',
NOW(), 'Add Program'),
(40714, 'Edit Program', 407, 14, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:edit', '#', 'admin',
NOW(), 'Edit Program'),
(40715, 'Delete Program', 407, 15, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:delete', '#',
'admin', NOW(), 'Delete Program'),
(40716, 'Enroll Member', 407, 16, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:enroll', '#',
'admin', NOW(), 'Enroll Member'),
(40717, 'Unenroll Member', 407, 17, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:unenroll', '#',
'admin', NOW(), 'Unenroll Member'),
(40718, 'Adjust Points', 407, 18, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:adjust', '#',
'admin', NOW(), 'Adjust Points'),
(40719, 'Expire Points', 407, 19, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:expire', '#',
'admin', NOW(), 'Expire Points'),
(40720, 'Loyalty Report', 407, 20, '', NULL, '1', '0', 'F', '0', '0', 'marketing:loyalty:report', '#',
'admin', NOW(), 'Loyalty Report')
ON CONFLICT (menuid) DO NOTHING;

-- Promo Codes
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(408, 'Promo Codes', 4, 9, 'promo-codes', 'marketing/promo-codes/index', '1', '0', 'C', '0', '0',
'marketing:promocode:list', 'coupon', 'admin', NOW(), 'Promo Codes'),
(4084, 'Query Promo Code', 408, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promocode:query', '#',
'admin', NOW(), 'Query Promo Code'),
(4085, 'Add Promo Code', 408, 2, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promocode:add', '#', 'admin',
NOW(), 'Add Promo Code'),
(4086, 'Edit Promo Code', 408, 3, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promocode:edit', '#',
'admin', NOW(), 'Edit Promo Code'),
(4087, 'Delete Promo Code', 408, 4, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promocode:delete', '#',
'admin', NOW(), 'Delete Promo Code'),
(4088, 'Validate Promo Code', 408, 5, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promocode:validate', '#', 'admin', NOW(), 'Validate Promo Code'),
(4089, 'Apply Promo Code', 408, 6, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promocode:apply', '#',
'admin', NOW(), 'Apply Promo Code'),
(4090, 'Record Usage', 408, 7, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promocode:usage', '#', 'admin',
NOW(), 'Record Usage'),
(4091, 'Usage Report', 408, 8, '', NULL, '1', '0', 'F', '0', '0', 'marketing:promocode:report', '#', 'admin', NOW(), 'Usage Report')
ON CONFLICT (menuid) DO NOTHING;

-- Notifications
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(409, 'Notifications', 4, 10, 'notifications', 'marketing/notifications/index', '1', '0', 'C', '0', '0',
'marketing:notification:list', 'message', 'admin', NOW(), 'Notifications'),
(40911, 'Query Notification', 409, 1, '', NULL, '1', '0', 'F', '0', '0', 'marketing:notification:query', '#', 'admin', NOW(), 'Query Notification'),
(40912, 'Add Notification', 409, 2, '', NULL, '1', '0', 'F', '0', '0', 'marketing:notification:add', '#',
'admin', NOW(), 'Add Notification'),
(40913, 'Edit Notification', 409, 3, '', NULL, '1', '0', 'F', '0', '0', 'marketing:notification:edit', '#',
'admin', NOW(), 'Edit Notification'),
(40914, 'Delete Notification', 409, 4, '', NULL, '1', '0', 'F', '0', '0', 'marketing:notification:delete',
'#', 'admin', NOW(), 'Delete Notification'),
(40915, 'Send Bulk', 409, 5, '', NULL, '1', '0', 'F', '0', '0', 'marketing:notification:send', '#', 'admin',
NOW(), 'Send Bulk'),
(40916, 'Send SMS', 409, 6, '', NULL, '1', '0', 'F', '0', '0', 'marketing:notification:sms', '#', 'admin',
NOW(), 'Send SMS'),
(40917, 'Send Email', 409, 7, '', NULL, '1', '0', 'F', '0', '0', 'marketing:notification:email', '#',
'admin', NOW(), 'Send Email')
ON CONFLICT (menuid) DO NOTHING;

-- =============================================
-- OPERATIONS MODULE - MENU & PERMISSIONS
-- Parent ID: 60000
-- =============================================

-- Operations Directory
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(60000, 'Operations', 0, 8, 'operations', NULL, '1', '0', 'M', '0', '0', '', 'gear', 'admin', NOW(), 'Operations Module')
ON CONFLICT (menuid) DO NOTHING;

-- Operations Customers
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(60100, 'Customers', 60000, 1, 'customers', 'operations/customers/index', '1', '0', 'C', '0', '0',
'operations:customers:list', 'peoples', 'admin', NOW(), 'Operations Customers'),
(60101, 'List Customers', 60100, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:customers:list', '#', 'admin', NOW(), 'List Customers'),
(60102, 'Edit Customer', 60100, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:customers:edit', '#', 'admin', NOW(), 'Edit Customer')
ON CONFLICT (menuid) DO NOTHING;

-- Operations Site Visits
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(60200, 'Site Visits', 60000, 2, 'visits', 'operations/visits/index', '1', '0', 'C', '0', '0',
'operations:visits:list', 'map-pin', 'admin', NOW(), 'Site Visits'),
(60201, 'List Visits', 60200, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:list', '#', 'admin', NOW(), 'List Visits'),
(60202, 'Add Visit', 60200, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:add', '#', 'admin', NOW(), 'Add Visit'),
(60203, 'Edit Visit', 60200, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:edit', '#', 'admin', NOW(), 'Edit Visit'),
(60204, 'Delete Visit', 60200, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:visits:delete', '#', 'admin', NOW(), 'Delete Visit')
ON CONFLICT (menuid) DO NOTHING;

-- Operations Tickets
INSERT INTO sys_menu (menuid, menuname, parentid, ordernum, path, component, isframe,
iscache, menutype, visible, status, perms, icon, create_by, create_time, remark)
VALUES
(60300, 'Tickets', 60000, 3, 'tickets', 'operations/tickets/index', '1', '0', 'C', '0', '0',
'operations:tickets:list', 'file-text', 'admin', NOW(), 'Operations Tickets'),
(60301, 'List Tickets', 60300, 1, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:list', '#', 'admin', NOW(), 'List Tickets'),
(60302, 'Add Ticket', 60300, 2, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:add', '#', 'admin', NOW(), 'Add Ticket'),
(60303, 'Edit Ticket', 60300, 3, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:edit', '#', 'admin', NOW(), 'Edit Ticket'),
(60304, 'Delete Ticket', 60300, 4, '', NULL, '1', '0', 'F', '0', '0', 'operations:tickets:delete', '#', 'admin', NOW(), 'Delete Ticket')
ON CONFLICT (menuid) DO NOTHING;

-- =============================================
-- ROLE ASSIGNMENTS
-- =============================================

-- Assign Marketing permissions to editor role (role_id = 3)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
-- Marketing directory
(3, 4),
-- Leads
(3, 400), (3, 4001), (3, 4002), (3, 4003), (3, 4004), (3, 4005), (3, 4006),
-- Activities
(3, 401), (3, 4011), (3, 4012), (3, 4013), (3, 4014),
-- Assignments
(3, 402), (3, 4021), (3, 4022), (3, 4023),
-- Team
(3, 403), (3, 4031),
-- Meetings
(3, 404), (3, 4041), (3, 4042), (3, 4043), (3, 4044), (3, 4045), (3, 4046),
-- Promotions
(3, 405), (3, 4051), (3, 4052), (3, 4053), (3, 4054), (3, 4055),
-- Gift Cards
(3, 406), (3, 4061), (3, 4062), (3, 4063), (3, 4064), (3, 4065), (3, 4066), (3, 4067), (3, 4068), (3,
4069), (3, 4070), (3, 4071), (3, 40610), (3, 40611), (3, 40612),
-- Loyalty
(3, 407), (3, 4072), (3, 4073), (3, 4074), (3, 4075), (3, 4076), (3, 4077), (3, 4078), (3, 4079), (3,
4080), (3, 4081), (3, 4082), (3, 4083), (3, 40713), (3, 40714), (3, 40715), (3, 40716), (3, 40717), (3,
40718), (3, 40719), (3, 40720),
-- Promo Codes
(3, 408), (3, 4084), (3, 4085), (3, 4086), (3, 4087), (3, 4088), (3, 4089), (3, 4090), (3, 4091),
-- Notifications
(3, 409), (3, 40911), (3, 40912), (3, 40913), (3, 40914), (3, 40915), (3, 40916), (3, 40917)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Assign Sales permissions to editor role (role_id = 3)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(3, 50000),
(3, 50100), (3, 50101), (3, 50102), (3, 50103),
(3, 50200), (3, 50201), (3, 50202), (3, 50203), (3, 50204), (3, 50205),
(3, 50300), (3, 50301), (3, 50302),
(3, 50400), (3, 50401)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Assign Operations permissions to editor role (role_id = 3)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(3, 60000),
-- Customers
(3, 60100), (3, 60101), (3, 60102),
-- Visits
(3, 60200), (3, 60201), (3, 60202), (3, 60203), (3, 60204),
-- Tickets
(3, 60300), (3, 60301), (3, 60302), (3, 60303), (3, 60304)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Marketing to admin (role_id = 1)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(1, 4),
(1, 400), (1, 4001), (1, 4002), (1, 4003), (1, 4004), (1, 4005), (1, 4006),
(1, 401), (1, 4011), (1, 4012), (1, 4013), (1, 4014),
(1, 402), (1, 4021), (1, 4022), (1, 4023),
(1, 403), (1, 4031),
(1, 404), (1, 4041), (1, 4042), (1, 4043), (1, 4044), (1, 4045), (1, 4046),
(1, 405), (1, 4051), (1, 4052), (1, 4053), (1, 4054), (1, 4055),
(1, 406), (1, 4061), (1, 4062), (1, 4063), (1, 4064), (1, 4065), (1, 4066), (1, 4067), (1, 4068), (1,
4069), (1, 4070), (1, 4071), (1, 40610), (1, 40611), (1, 40612),
(1, 407), (1, 4072), (1, 4073), (1, 4074), (1, 4075), (1, 4076), (1, 4077), (1, 4078), (1, 4079), (1,
4080), (1, 4081), (1, 4082), (1, 4083), (1, 40713), (1, 40714), (1, 40715), (1, 40716), (1, 40717), (1,
40718), (1, 40719), (1, 40720),
(1, 408), (1, 4084), (1, 4085), (1, 4086), (1, 4087), (1, 4088), (1, 4089), (1, 4090), (1, 4091),
(1, 409), (1, 40911), (1, 40912), (1, 40913), (1, 40914), (1, 40915), (1, 40916), (1, 40917)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Sales to admin (role_id = 1)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(1, 50000),
(1, 50100), (1, 50101), (1, 50102), (1, 50103),
(1, 50200), (1, 50201), (1, 50202), (1, 50203), (1, 50204), (1, 50205),
(1, 50300), (1, 50301), (1, 50302),
(1, 50400), (1, 50401)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Operations to admin (role_id = 1)
INSERT INTO sys_role_menu (role_id, menu_id) VALUES
(1, 60000),
-- Customers
(1, 60100), (1, 60101), (1, 60102),
-- Visits
(1, 60200), (1, 60201), (1, 60202), (1, 60203), (1, 60204),
-- Tickets
(1, 60300), (1, 60301), (1, 60302), (1, 60303), (1, 60304)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- =============================================
-- VERIFY
-- =============================================

-- Count permissions per role
SELECT r.roleid, r.rolename,
  SUM(CASE WHEN m.perms LIKE 'marketing:%' THEN 1 ELSE 0 END) AS marketing_perms,
  SUM(CASE WHEN m.perms LIKE 'sales:%' THEN 1 ELSE 0 END) AS sales_perms,
  SUM(CASE WHEN m.perms LIKE 'operations:%' THEN 1 ELSE 0 END) AS operations_perms
FROM sys_role r
JOIN sys_role_menu rm ON r.roleid = rm.role_id
JOIN sys_menu m ON rm.menu_id = m.menuid
WHERE r.roleid IN (1, 3)
GROUP BY r.roleid, r.rolename;
