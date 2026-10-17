-- ====================================================
-- Complete Permission System - PostgreSQL
-- Generated: 2026-01-08
-- Description: All menus and permissions for Maham ERP System
-- ====================================================

-- IMPORTANT: Run this script after clearing sys_menu and sys_role_menu tables
-- DELETE FROM sys_role_menu;
-- DELETE FROM sys_menu;

-- ====================================================
-- SECTION 1: SYSTEM MANAGEMENT MODULE (ID: 1)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- System Management Directory
(1, 'System Management', 0, 1, 'system', NULL, 1, 0, 'M', '0', '0', '', 'settings', 'admin', NOW(), 'System Management Module'),

-- User Management
(100, 'User Management', 1, 1, 'user', 'system/user/index', 1, 0, 'C', '0', '0', 'system:user:list', 'user', 'admin', NOW(), 'User Management'),
(1001, 'Query User', 100, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:user:query', '#', 'admin', NOW(), 'Query User'),
(1002, 'Add User', 100, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:user:add', '#', 'admin', NOW(), 'Add User'),
(1003, 'Edit User', 100, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:user:edit', '#', 'admin', NOW(), 'Edit User'),
(1004, 'Delete User', 100, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:user:remove', '#', 'admin', NOW(), 'Delete User'),
(1005, 'Update Status', 100, 5, '', NULL, 1, 0, 'F', '0', '0', 'system:user:update', '#', 'admin', NOW(), 'Update Status'),
(1006, 'Reset Password', 100, 6, '', NULL, 1, 0, 'F', '0', '0', 'system:user:resetPwd', '#', 'admin', NOW(), 'Reset Password'),
(1007, 'Import Users', 100, 7, '', NULL, 1, 0, 'F', '0', '0', 'system:user:import', '#', 'admin', NOW(), 'Import Users'),
(1008, 'Export Users', 100, 8, '', NULL, 1, 0, 'F', '0', '0', 'system:user:export', '#', 'admin', NOW(), 'Export Users'),

-- Role Management
(101, 'Role Management', 1, 2, 'role', 'system/role/index', 1, 0, 'C', '0', '0', 'system:role:list', 'peoples', 'admin', NOW(), 'Role Management'),
(1011, 'Query Role', 101, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:role:query', '#', 'admin', NOW(), 'Query Role'),
(1012, 'Add Role', 101, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:role:add', '#', 'admin', NOW(), 'Add Role'),
(1013, 'Edit Role', 101, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:role:edit', '#', 'admin', NOW(), 'Edit Role'),
(1014, 'Delete Role', 101, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:role:remove', '#', 'admin', NOW(), 'Delete Role'),
(1015, 'Authorize Role', 101, 5, '', NULL, 1, 0, 'F', '0', '0', 'system:role:authorize', '#', 'admin', NOW(), 'Authorize Role'),
(1016, 'Export Roles', 101, 6, '', NULL, 1, 0, 'F', '0', '0', 'system:role:export', '#', 'admin', NOW(), 'Export Roles'),

-- Menu Management
(102, 'Menu Management', 1, 3, 'menu', 'system/menu/index', 1, 0, 'C', '0', '0', 'system:menu:list', 'tree-table', 'admin', NOW(), 'Menu Management'),
(1021, 'Query Menu', 102, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:menu:query', '#', 'admin', NOW(), 'Query Menu'),
(1022, 'Add Menu', 102, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:menu:add', '#', 'admin', NOW(), 'Add Menu'),
(1023, 'Edit Menu', 102, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:menu:edit', '#', 'admin', NOW(), 'Edit Menu'),
(1024, 'Delete Menu', 102, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:menu:remove', '#', 'admin', NOW(), 'Delete Menu'),
(1025, 'Update Sort', 102, 5, '', NULL, 1, 0, 'F', '0', '0', 'system:menu:update', '#', 'admin', NOW(), 'Update Sort'),
(1026, 'Sync Permissions', 102, 6, '', NULL, 1, 0, 'F', '0', '0', 'system:menu:sync', '#', 'admin', NOW(), 'Sync Permissions'),

-- Department Management
(103, 'Department Management', 1, 4, 'dept', 'system/dept/index', 1, 0, 'C', '0', '0', 'system:dept:list', 'tree', 'admin', NOW(), 'Department Management'),
(1031, 'Query Dept', 103, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:dept:query', '#', 'admin', NOW(), 'Query Dept'),
(1032, 'Add Dept', 103, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:dept:add', '#', 'admin', NOW(), 'Add Dept'),
(1033, 'Edit Dept', 103, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:dept:update', '#', 'admin', NOW(), 'Edit Dept'),
(1034, 'Delete Dept', 103, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:dept:remove', '#', 'admin', NOW(), 'Delete Dept'),

-- Post Management
(104, 'Post Management', 1, 5, 'post', 'system/post/index', 1, 0, 'C', '0', '0', 'system:post:list', 'post', 'admin', NOW(), 'Post Management'),
(1041, 'Query Post', 104, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:post:query', '#', 'admin', NOW(), 'Query Post'),
(1042, 'Add Post', 104, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:post:add', '#', 'admin', NOW(), 'Add Post'),
(1043, 'Edit Post', 104, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:post:edit', '#', 'admin', NOW(), 'Edit Post'),
(1044, 'Delete Post', 104, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:post:remove', '#', 'admin', NOW(), 'Delete Post'),
(1045, 'Export Posts', 104, 5, '', NULL, 1, 0, 'F', '0', '0', 'system:post:export', '#', 'admin', NOW(), 'Export Posts'),

-- Dictionary Management
(105, 'Dictionary Management', 1, 6, 'dict', 'system/dict/index', 1, 0, 'C', '0', '0', 'system:dict:list', 'dict', 'admin', NOW(), 'Dictionary Management'),
(1051, 'Query Dict', 105, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:dict:query', '#', 'admin', NOW(), 'Query Dict'),
(1052, 'Add Dict', 105, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:dict:add', '#', 'admin', NOW(), 'Add Dict'),
(1053, 'Edit Dict', 105, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:dict:edit', '#', 'admin', NOW(), 'Edit Dict'),
(1054, 'Delete Dict', 105, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:dict:remove', '#', 'admin', NOW(), 'Delete Dict'),
(1055, 'Export Dict', 105, 5, '', NULL, 1, 0, 'F', '0', '0', 'system:dict:export', '#', 'admin', NOW(), 'Export Dict'),

-- Config Management
(106, 'Config Management', 1, 7, 'config', 'system/config/index', 1, 0, 'C', '0', '0', 'system:config:list', 'edit', 'admin', NOW(), 'Config Management'),
(1061, 'Query Config', 106, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:config:query', '#', 'admin', NOW(), 'Query Config'),
(1062, 'Add Config', 106, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:config:add', '#', 'admin', NOW(), 'Add Config'),
(1063, 'Edit Config', 106, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:config:update', '#', 'admin', NOW(), 'Edit Config'),
(1064, 'Delete Config', 106, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:config:remove', '#', 'admin', NOW(), 'Delete Config'),

-- Notice Management
(107, 'Notice Management', 1, 8, 'notice', 'system/notice/index', 1, 0, 'C', '0', '0', 'system:notice:list', 'message', 'admin', NOW(), 'Notice Management'),
(1071, 'Query Notice', 107, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:notice:query', '#', 'admin', NOW(), 'Query Notice'),
(1072, 'Add Notice', 107, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:notice:add', '#', 'admin', NOW(), 'Add Notice'),
(1073, 'Edit Notice', 107, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:notice:update', '#', 'admin', NOW(), 'Edit Notice'),
(1074, 'Delete Notice', 107, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:notice:delete', '#', 'admin', NOW(), 'Delete Notice'),
(1075, 'Export Notice', 107, 5, '', NULL, 1, 0, 'F', '0', '0', 'system:notice:export', '#', 'admin', NOW(), 'Export Notice'),

-- Language Management
(108, 'Language Management', 1, 9, 'lang', 'system/lang/index', 1, 0, 'C', '0', '0', 'system:lang:list', 'language', 'admin', NOW(), 'Language Management'),
(1081, 'Query Lang', 108, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:lang:query', '#', 'admin', NOW(), 'Query Lang'),
(1082, 'Edit Lang', 108, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:lang:edit', '#', 'admin', NOW(), 'Edit Lang'),
(1083, 'Delete Lang', 108, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:lang:delete', '#', 'admin', NOW(), 'Delete Lang'),
(1084, 'Export Lang', 108, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:lang:export', '#', 'admin', NOW(), 'Export Lang'),
(1085, 'Import Lang', 108, 5, '', NULL, 1, 0, 'F', '0', '0', 'system:lang:import', '#', 'admin', NOW(), 'Import Lang'),

-- Post Role Management
(109, 'Post Role Management', 1, 10, 'post-role', 'system/postRole/index', 1, 0, 'C', '0', '0', 'system:postRole:list', 'user-check', 'admin', NOW(), 'Post Role Management'),
(1091, 'Query Post Role', 109, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:postRole:query', '#', 'admin', NOW(), 'Query Post Role'),
(1092, 'Add Post Role', 109, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:postRole:add', '#', 'admin', NOW(), 'Add Post Role'),
(1093, 'Edit Post Role', 109, 3, '', NULL, 1, 0, 'F', '0', '0', 'system:postRole:edit', '#', 'admin', NOW(), 'Edit Post Role'),
(1094, 'Delete Post Role', 109, 4, '', NULL, 1, 0, 'F', '0', '0', 'system:postRole:remove', '#', 'admin', NOW(), 'Delete Post Role'),

-- Employee Access
(110, 'Employee Access', 1, 11, 'employee-access', 'system/employeeAccess/index', 1, 0, 'C', '0', '0', 'system:employeeAccess:list', 'key', 'admin', NOW(), 'Employee Access'),
(1101, 'Query Employee Access', 110, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:employeeAccess:query', '#', 'admin', NOW(), 'Query Employee Access'),
(1102, 'Activate Access', 110, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:employeeAccess:activate', '#', 'admin', NOW(), 'Activate Access'),

-- Role Users
(111, 'Role Users', 1, 12, 'role-users', 'system/roleUsers/index', 1, 0, 'C', '0', '0', 'system:roleusers:list', 'users', 'admin', NOW(), 'Role Users'),
(1111, 'Add Role User', 111, 1, '', NULL, 1, 0, 'F', '0', '0', 'system:roleusers:add', '#', 'admin', NOW(), 'Add Role User'),
(1112, 'Remove Role User', 111, 2, '', NULL, 1, 0, 'F', '0', '0', 'system:roleusers:remove', '#', 'admin', NOW(), 'Remove Role User')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 2: MONITOR MODULE (ID: 2)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- Monitor Directory
(2, 'Monitor', 0, 2, 'monitor', NULL, 1, 0, 'M', '0', '0', '', 'monitor', 'admin', NOW(), 'Monitor Module'),

-- Online Users
(200, 'Online Users', 2, 1, 'online', 'monitor/online/index', 1, 0, 'C', '0', '0', 'monitor:online:list', 'online', 'admin', NOW(), 'Online Users'),
(2001, 'Force Logout', 200, 1, '', NULL, 1, 0, 'F', '0', '0', 'monitor:online:forceLogout', '#', 'admin', NOW(), 'Force Logout'),
(2002, 'Batch Logout', 200, 2, '', NULL, 1, 0, 'F', '0', '0', 'monitor:online:batchLogout', '#', 'admin', NOW(), 'Batch Logout'),

-- Job Scheduler
(201, 'Job Scheduler', 2, 2, 'job', 'monitor/job/index', 1, 0, 'C', '0', '0', 'monitor:job:list', 'job', 'admin', NOW(), 'Job Scheduler'),
(2011, 'Query Job', 201, 1, '', NULL, 1, 0, 'F', '0', '0', 'monitor:job:query', '#', 'admin', NOW(), 'Query Job'),
(2012, 'Add Job', 201, 2, '', NULL, 1, 0, 'F', '0', '0', 'monitor:job:add', '#', 'admin', NOW(), 'Add Job'),
(2013, 'Edit Job', 201, 3, '', NULL, 1, 0, 'F', '0', '0', 'monitor:job:edit', '#', 'admin', NOW(), 'Edit Job'),
(2014, 'Delete Job', 201, 4, '', NULL, 1, 0, 'F', '0', '0', 'monitor:job:delete', '#', 'admin', NOW(), 'Delete Job'),
(2015, 'Start Job', 201, 5, '', NULL, 1, 0, 'F', '0', '0', 'monitor:job:start', '#', 'admin', NOW(), 'Start Job'),
(2016, 'Stop Job', 201, 6, '', NULL, 1, 0, 'F', '0', '0', 'monitor:job:stop', '#', 'admin', NOW(), 'Stop Job'),
(2017, 'Run Job', 201, 7, '', NULL, 1, 0, 'F', '0', '0', 'monitor:job:run', '#', 'admin', NOW(), 'Run Job'),
(2018, 'Export Job', 201, 8, '', NULL, 1, 0, 'F', '0', '0', 'monitor:job:export', '#', 'admin', NOW(), 'Export Job'),

-- Operation Logs
(202, 'Operation Logs', 2, 3, 'operlog', 'monitor/operlog/index', 1, 0, 'C', '0', '0', 'monitor:operlog:list', 'form', 'admin', NOW(), 'Operation Logs'),
(2021, 'Delete Log', 202, 1, '', NULL, 1, 0, 'F', '0', '0', 'monitor:operlog:delete', '#', 'admin', NOW(), 'Delete Log'),
(2022, 'Export Log', 202, 2, '', NULL, 1, 0, 'F', '0', '0', 'monitor:operlog:export', '#', 'admin', NOW(), 'Export Log'),

-- Login Logs
(203, 'Login Logs', 2, 4, 'logininfor', 'monitor/logininfor/index', 1, 0, 'C', '0', '0', 'monitor:logininfor:list', 'logininfor', 'admin', NOW(), 'Login Logs'),
(2031, 'Delete Login Log', 203, 1, '', NULL, 1, 0, 'F', '0', '0', 'monitor:logininfor:remove', '#', 'admin', NOW(), 'Delete Login Log'),
(2032, 'Export Login Log', 203, 2, '', NULL, 1, 0, 'F', '0', '0', 'monitor:logininfor:export', '#', 'admin', NOW(), 'Export Login Log')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 3: TOOLS MODULE (ID: 3)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- Tools Directory
(3, 'Tools', 0, 3, 'tool', NULL, 1, 0, 'M', '0', '0', '', 'tool', 'admin', NOW(), 'Tools Module'),

-- Code Generator
(300, 'Code Generator', 3, 1, 'gen', 'tool/gen/index', 1, 0, 'C', '0', '0', 'tool:gen:list', 'code', 'admin', NOW(), 'Code Generator'),
(3001, 'Query Gen', 300, 1, '', NULL, 1, 0, 'F', '0', '0', 'tool:gen:query', '#', 'admin', NOW(), 'Query Gen'),
(3002, 'Edit Gen', 300, 2, '', NULL, 1, 0, 'F', '0', '0', 'tool:gen:edit', '#', 'admin', NOW(), 'Edit Gen'),
(3003, 'Delete Gen', 300, 3, '', NULL, 1, 0, 'F', '0', '0', 'tool:gen:remove', '#', 'admin', NOW(), 'Delete Gen'),
(3004, 'Import Gen', 300, 4, '', NULL, 1, 0, 'F', '0', '0', 'tool:gen:import', '#', 'admin', NOW(), 'Import Gen'),
(3005, 'Preview Gen', 300, 5, '', NULL, 1, 0, 'F', '0', '0', 'tool:gen:preview', '#', 'admin', NOW(), 'Preview Gen'),
(3006, 'Generate Code', 300, 6, '', NULL, 1, 0, 'F', '0', '0', 'tool:gen:code', '#', 'admin', NOW(), 'Generate Code'),

-- File Management
(301, 'File Management', 3, 2, 'file', 'tool/file/index', 1, 0, 'C', '0', '0', 'tool:file:list', 'upload', 'admin', NOW(), 'File Management'),
(3011, 'Query File', 301, 1, '', NULL, 1, 0, 'F', '0', '0', 'tool:file:query', '#', 'admin', NOW(), 'Query File'),
(3012, 'Add File', 301, 2, '', NULL, 1, 0, 'F', '0', '0', 'tool:file:add', '#', 'admin', NOW(), 'Add File'),
(3013, 'Edit File', 301, 3, '', NULL, 1, 0, 'F', '0', '0', 'tool:file:edit', '#', 'admin', NOW(), 'Edit File'),
(3014, 'Delete File', 301, 4, '', NULL, 1, 0, 'F', '0', '0', 'tool:file:delete', '#', 'admin', NOW(), 'Delete File'),
(3015, 'Export File', 301, 5, '', NULL, 1, 0, 'F', '0', '0', 'tool:file:export', '#', 'admin', NOW(), 'Export File'),

-- Email Templates
(302, 'Email Templates', 3, 3, 'emailtpl', 'tool/email/index', 1, 0, 'C', '0', '0', 'tool:emailtpl:list', 'email', 'admin', NOW(), 'Email Templates'),
(3021, 'Add Template', 302, 1, '', NULL, 1, 0, 'F', '0', '0', 'tool:emailtpl:add', '#', 'admin', NOW(), 'Add Template'),
(3022, 'Edit Template', 302, 2, '', NULL, 1, 0, 'F', '0', '0', 'tool:emailtpl:edit', '#', 'admin', NOW(), 'Edit Template'),
(3023, 'Delete Template', 302, 3, '', NULL, 1, 0, 'F', '0', '0', 'tool:emailtpl:delete', '#', 'admin', NOW(), 'Delete Template'),
(3024, 'Send Email', 302, 4, '', NULL, 1, 0, 'F', '0', '0', 'tool:email:send', '#', 'admin', NOW(), 'Send Email')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 4: MARKETING MODULE (ID: 4)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- Marketing Directory
(4, 'Marketing', 0, 4, 'marketing', NULL, 1, 0, 'M', '0', '0', '', 'chart', 'admin', NOW(), 'Marketing Module'),

-- Leads
(400, 'Leads', 4, 1, 'leads', 'marketing/leads/index', 1, 0, 'C', '0', '0', 'marketing:leads:list', 'peoples', 'admin', NOW(), 'Leads Management'),
(4001, 'View Lead', 400, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:view', '#', 'admin', NOW(), 'View Lead'),
(4002, 'Add Lead', 400, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:add', '#', 'admin', NOW(), 'Add Lead'),
(4003, 'Edit Lead', 400, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:edit', '#', 'admin', NOW(), 'Edit Lead'),
(4004, 'Delete Lead', 400, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:delete', '#', 'admin', NOW(), 'Delete Lead'),
(4005, 'Export Leads', 400, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:export', '#', 'admin', NOW(), 'Export Leads'),
(4006, 'Import Leads', 400, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:leads:import', '#', 'admin', NOW(), 'Import Leads'),

-- Lead Activities
(401, 'Lead Activities', 4, 2, 'activities', 'marketing/activities/index', 1, 0, 'C', '0', '0', 'marketing:activities:list', 'activity', 'admin', NOW(), 'Lead Activities'),
(4011, 'View Activity', 401, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:view', '#', 'admin', NOW(), 'View Activity'),
(4012, 'Add Activity', 401, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:add', '#', 'admin', NOW(), 'Add Activity'),
(4013, 'Delete Activity', 401, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:delete', '#', 'admin', NOW(), 'Delete Activity'),
(4014, 'Activity Stats', 401, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:activities:stats', '#', 'admin', NOW(), 'Activity Stats'),

-- Lead Assignments
(402, 'Lead Assignments', 4, 3, 'assignments', 'marketing/assignments/index', 1, 0, 'C', '0', '0', 'marketing:assignments:list', 'user-check', 'admin', NOW(), 'Lead Assignments'),
(4021, 'View Assignment', 402, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:view', '#', 'admin', NOW(), 'View Assignment'),
(4022, 'Assign Lead', 402, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:assign', '#', 'admin', NOW(), 'Assign Lead'),
(4023, 'Assignment Stats', 402, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:assignments:stats', '#', 'admin', NOW(), 'Assignment Stats'),

-- Marketing Team
(403, 'Marketing Team', 4, 4, 'employees', 'marketing/employees/index', 1, 0, 'C', '0', '0', 'marketing:employees:list', 'peoples', 'admin', NOW(), 'Marketing Team'),
(4031, 'Query Employee', 403, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:employees:query', '#', 'admin', NOW(), 'Query Employee'),

-- Meetings
(404, 'Meetings', 4, 5, 'meetings', 'marketing/meetings/index', 1, 0, 'C', '0', '0', 'marketing:meetings:list', 'date', 'admin', NOW(), 'Meetings'),
(4041, 'View Meeting', 404, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:meetings:view', '#', 'admin', NOW(), 'View Meeting'),
(4042, 'Add Meeting', 404, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:meetings:add', '#', 'admin', NOW(), 'Add Meeting'),
(4043, 'Edit Meeting', 404, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:meetings:edit', '#', 'admin', NOW(), 'Edit Meeting'),
(4044, 'Cancel Meeting', 404, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:meetings:cancel', '#', 'admin', NOW(), 'Cancel Meeting'),
(4045, 'Complete Meeting', 404, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:meetings:complete', '#', 'admin', NOW(), 'Complete Meeting'),
(4046, 'Meeting Stats', 404, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:meetings:stats', '#', 'admin', NOW(), 'Meeting Stats'),

-- Promotions
(405, 'Promotions', 4, 6, 'promotions', 'marketing/promotions/index', 1, 0, 'C', '0', '0', 'marketing:promotion:list', 'shopping', 'admin', NOW(), 'Promotions'),
(4051, 'Query Promotion', 405, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promotion:query', '#', 'admin', NOW(), 'Query Promotion'),
(4052, 'Add Promotion', 405, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promotion:add', '#', 'admin', NOW(), 'Add Promotion'),
(4053, 'Edit Promotion', 405, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promotion:edit', '#', 'admin', NOW(), 'Edit Promotion'),
(4054, 'Delete Promotion', 405, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promotion:delete', '#', 'admin', NOW(), 'Delete Promotion'),
(4055, 'Export Promotion', 405, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promotion:export', '#', 'admin', NOW(), 'Export Promotion'),

-- Gift Cards
(406, 'Gift Cards', 4, 7, 'giftcards', 'marketing/giftcards/index', 1, 0, 'C', '0', '0', 'marketing:giftcard:list', 'gift', 'admin', NOW(), 'Gift Cards'),
(4061, 'Query Gift Card', 406, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:query', '#', 'admin', NOW(), 'Query Gift Card'),
(4062, 'Add Gift Card', 406, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:add', '#', 'admin', NOW(), 'Add Gift Card'),
(4063, 'Edit Gift Card', 406, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:edit', '#', 'admin', NOW(), 'Edit Gift Card'),
(4064, 'Delete Gift Card', 406, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:delete', '#', 'admin', NOW(), 'Delete Gift Card'),
(4065, 'Activate Gift Card', 406, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:activate', '#', 'admin', NOW(), 'Activate Gift Card'),
(4066, 'Redeem Gift Card', 406, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:redeem', '#', 'admin', NOW(), 'Redeem Gift Card'),
(4067, 'Refund Gift Card', 406, 7, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:refund', '#', 'admin', NOW(), 'Refund Gift Card'),
(4068, 'Cancel Gift Card', 406, 8, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:cancel', '#', 'admin', NOW(), 'Cancel Gift Card'),
(4069, 'Send Gift Card', 406, 9, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:send', '#', 'admin', NOW(), 'Send Gift Card'),
(4070, 'Gift Card Stats', 406, 10, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:statistics', '#', 'admin', NOW(), 'Gift Card Stats'),
(4071, 'Export Gift Cards', 406, 11, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:export', '#', 'admin', NOW(), 'Export Gift Cards'),
(40610, 'Use Gift Card', 406, 12, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:use', '#', 'admin', NOW(), 'Use Gift Card'),
(40611, 'Deactivate Gift Card', 406, 13, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:deactivate', '#', 'admin', NOW(), 'Deactivate Gift Card'),
(40612, 'Gift Card Report', 406, 14, '', NULL, 1, 0, 'F', '0', '0', 'marketing:giftcard:report', '#', 'admin', NOW(), 'Gift Card Report'),

-- Loyalty Program
(407, 'Loyalty Program', 4, 8, 'loyalty', 'marketing/loyalty/index', 1, 0, 'C', '0', '0', 'marketing:loyalty:list', 'star', 'admin', NOW(), 'Loyalty Program'),
(4072, 'Query Loyalty', 407, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:query', '#', 'admin', NOW(), 'Query Loyalty'),
(4073, 'Earn Points', 407, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:earn', '#', 'admin', NOW(), 'Earn Points'),
(4074, 'Redeem Points', 407, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:redeem', '#', 'admin', NOW(), 'Redeem Points'),
(4075, 'Manage Tiers', 407, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:tiers', '#', 'admin', NOW(), 'Manage Tiers'),
(4076, 'Add Tier', 407, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:tiers:add', '#', 'admin', NOW(), 'Add Tier'),
(4077, 'Edit Tier', 407, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:tiers:edit', '#', 'admin', NOW(), 'Edit Tier'),
(4078, 'Delete Tier', 407, 7, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:tiers:delete', '#', 'admin', NOW(), 'Delete Tier'),
(4079, 'Loyalty Analytics', 407, 8, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:analytics', '#', 'admin', NOW(), 'Loyalty Analytics'),
(4080, 'Manage Rules', 407, 9, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:rules', '#', 'admin', NOW(), 'Manage Rules'),
(4081, 'Add Rule', 407, 10, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:rules:add', '#', 'admin', NOW(), 'Add Rule'),
(4082, 'Edit Rule', 407, 11, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:rules:edit', '#', 'admin', NOW(), 'Edit Rule'),
(4083, 'Delete Rule', 407, 12, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:rules:delete', '#', 'admin', NOW(), 'Delete Rule'),
(40713, 'Add Program/Tier', 407, 13, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:add', '#', 'admin', NOW(), 'Add Program/Tier'),
(40714, 'Edit Program/Tier', 407, 14, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:edit', '#', 'admin', NOW(), 'Edit Program/Tier'),
(40715, 'Delete Program/Tier', 407, 15, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:delete', '#', 'admin', NOW(), 'Delete Program/Tier'),
(40716, 'Enroll Member', 407, 16, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:enroll', '#', 'admin', NOW(), 'Enroll Member'),
(40717, 'Unenroll Member', 407, 17, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:unenroll', '#', 'admin', NOW(), 'Unenroll Member'),
(40718, 'Adjust Points', 407, 18, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:adjust', '#', 'admin', NOW(), 'Adjust Points'),
(40719, 'Expire Points', 407, 19, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:expire', '#', 'admin', NOW(), 'Expire Points'),
(40720, 'Loyalty Report', 407, 20, '', NULL, 1, 0, 'F', '0', '0', 'marketing:loyalty:report', '#', 'admin', NOW(), 'Loyalty Report'),

-- Promo Codes
(408, 'Promo Codes', 4, 9, 'promo-codes', 'marketing/promo-codes/index', 1, 0, 'C', '0', '0', 'marketing:promocode:list', 'coupon', 'admin', NOW(), 'Promo Codes'),
(4084, 'Query Promo Code', 408, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promocode:query', '#', 'admin', NOW(), 'Query Promo Code'),
(4085, 'Add Promo Code', 408, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promocode:add', '#', 'admin', NOW(), 'Add Promo Code'),
(4086, 'Edit Promo Code', 408, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promocode:edit', '#', 'admin', NOW(), 'Edit Promo Code'),
(4087, 'Delete Promo Code', 408, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promocode:delete', '#', 'admin', NOW(), 'Delete Promo Code'),
(4088, 'Validate Promo Code', 408, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promocode:validate', '#', 'admin', NOW(), 'Validate Promo Code'),
(4089, 'Apply Promo Code', 408, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promocode:apply', '#', 'admin', NOW(), 'Apply Promo Code'),
(4090, 'Record Usage', 408, 7, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promocode:usage', '#', 'admin', NOW(), 'Record Usage'),
(4091, 'Usage Report', 408, 8, '', NULL, 1, 0, 'F', '0', '0', 'marketing:promocode:report', '#', 'admin', NOW(), 'Usage Report'),

-- Notifications
(409, 'Notifications', 4, 10, 'notifications', 'marketing/notifications/index', 1, 0, 'C', '0', '0', 'marketing:notification:list', 'message', 'admin', NOW(), 'Notifications'),
(40911, 'Query Notification', 409, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:notification:query', '#', 'admin', NOW(), 'Query Notification'),
(40912, 'Add Notification', 409, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:notification:add', '#', 'admin', NOW(), 'Add Notification'),
(40913, 'Edit Notification', 409, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:notification:edit', '#', 'admin', NOW(), 'Edit Notification'),
(40914, 'Delete Notification', 409, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:notification:delete', '#', 'admin', NOW(), 'Delete Notification'),
(40915, 'Send Bulk Notification', 409, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:notification:send', '#', 'admin', NOW(), 'Send Bulk Notification'),
(40916, 'Send SMS', 409, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:notification:sms', '#', 'admin', NOW(), 'Send SMS'),
(40917, 'Send Email', 409, 7, '', NULL, 1, 0, 'F', '0', '0', 'marketing:notification:email', '#', 'admin', NOW(), 'Send Email')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 5: HUMAN RESOURCES MODULE (ID: 5)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- HR Directory
(5, 'Human Resources', 0, 5, 'hr', NULL, 1, 0, 'M', '0', '0', '', 'peoples', 'admin', NOW(), 'Human Resources Module'),

-- Employees
(500, 'Employees', 5, 1, 'employees', 'hr/employees/index', 1, 0, 'C', '0', '0', 'hr:employees:list', 'user', 'admin', NOW(), 'Employees'),
(5001, 'Query Employee', 500, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:employees:query', '#', 'admin', NOW(), 'Query Employee'),
(5002, 'Add Employee', 500, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:employees:add', '#', 'admin', NOW(), 'Add Employee'),
(5003, 'Edit Employee', 500, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:employees:edit', '#', 'admin', NOW(), 'Edit Employee'),
(5004, 'Delete Employee', 500, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:employees:delete', '#', 'admin', NOW(), 'Delete Employee'),
(5005, 'Export Employees', 500, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:employees:export', '#', 'admin', NOW(), 'Export Employees'),

-- Employee Core (Comprehensive HR Data)
(501, 'Employee Core', 5, 2, 'employee-core', 'hr/employeeCore/index', 1, 0, 'C', '0', '0', 'hr:employeecore:list', 'user-settings', 'admin', NOW(), 'Employee Core Data'),
(5011, 'Create Employee', 501, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:create', '#', 'admin', NOW(), 'Create Employee'),
(5012, 'Update Employee', 501, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:update', '#', 'admin', NOW(), 'Update Employee'),
(5013, 'Query Employee', 501, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:query', '#', 'admin', NOW(), 'Query Employee'),
(5014, 'Terminate Employee', 501, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:terminate', '#', 'admin', NOW(), 'Terminate Employee'),
(5015, 'Activate Employee', 501, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:activate', '#', 'admin', NOW(), 'Activate Employee'),
(5016, 'Transfer Employee', 501, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:transfer', '#', 'admin', NOW(), 'Transfer Employee'),
(5017, 'Promote Employee', 501, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:promote', '#', 'admin', NOW(), 'Promote Employee'),
(5018, 'Person List', 501, 8, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:person:list', '#', 'admin', NOW(), 'Person List'),
(5019, 'Person Query', 501, 9, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:person:query', '#', 'admin', NOW(), 'Person Query'),
(5020, 'Person Edit', 501, 10, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:person:edit', '#', 'admin', NOW(), 'Person Edit'),
(5021, 'Employee List', 501, 11, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:employee:list', '#', 'admin', NOW(), 'Employee List'),
(5022, 'Employee Query', 501, 12, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:employee:query', '#', 'admin', NOW(), 'Employee Query'),
(5023, 'Contact Query', 501, 13, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:contact:query', '#', 'admin', NOW(), 'Contact Query'),
(5024, 'Contact Add', 501, 14, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:contact:add', '#', 'admin', NOW(), 'Contact Add'),
(5025, 'Contact Edit', 501, 15, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:contact:edit', '#', 'admin', NOW(), 'Contact Edit'),
(5026, 'Contact Delete', 501, 16, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:contact:delete', '#', 'admin', NOW(), 'Contact Delete'),
(5027, 'Address Query', 501, 17, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:address:query', '#', 'admin', NOW(), 'Address Query'),
(5028, 'Address Add', 501, 18, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:address:add', '#', 'admin', NOW(), 'Address Add'),
(5029, 'Address Edit', 501, 19, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:address:edit', '#', 'admin', NOW(), 'Address Edit'),
(5030, 'Address Delete', 501, 20, '', NULL, 1, 0, 'F', '0', '0', 'hr:employeecore:address:delete', '#', 'admin', NOW(), 'Address Delete'),

-- Attendance
(502, 'Attendance', 5, 3, 'attendance', 'hr/attendance/index', 1, 0, 'C', '0', '0', 'hr:attendance:list', 'time', 'admin', NOW(), 'Attendance'),
(5061, 'View Attendance', 502, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:view', '#', 'admin', NOW(), 'View Attendance'),
(5062, 'Query Attendance', 502, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:query', '#', 'admin', NOW(), 'Query Attendance'),
(5063, 'Add Attendance', 502, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:add', '#', 'admin', NOW(), 'Add Attendance'),
(5064, 'Edit Attendance', 502, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:edit', '#', 'admin', NOW(), 'Edit Attendance'),
(5065, 'Delete Attendance', 502, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:delete', '#', 'admin', NOW(), 'Delete Attendance'),
(5066, 'Check In', 502, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:checkin', '#', 'admin', NOW(), 'Check In'),
(5067, 'Check Out', 502, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:checkout', '#', 'admin', NOW(), 'Check Out'),
(5068, 'Statistics', 502, 8, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:statistics', '#', 'admin', NOW(), 'Statistics'),
(5069, 'Approve Attendance', 502, 9, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:approve', '#', 'admin', NOW(), 'Approve Attendance'),
(5070, 'Calculate Attendance', 502, 10, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:calculate', '#', 'admin', NOW(), 'Calculate Attendance'),
(5071, 'Import Attendance', 502, 11, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:import', '#', 'admin', NOW(), 'Import Attendance'),
(5072, 'Export Attendance', 502, 12, '', NULL, 1, 0, 'F', '0', '0', 'hr:attendance:export', '#', 'admin', NOW(), 'Export Attendance'),

-- Leave Management
(503, 'Leave Management', 5, 4, 'leave', 'hr/leave/index', 1, 0, 'C', '0', '0', 'hr:leave:list', 'vacation', 'admin', NOW(), 'Leave Management'),
(5081, 'View Leave', 503, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:leave:view', '#', 'admin', NOW(), 'View Leave'),
(5082, 'Query Leave', 503, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:leave:query', '#', 'admin', NOW(), 'Query Leave'),
(5083, 'Add Leave', 503, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:leave:add', '#', 'admin', NOW(), 'Add Leave'),
(5084, 'Edit Leave', 503, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:leave:edit', '#', 'admin', NOW(), 'Edit Leave'),
(5085, 'Approve Leave', 503, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:leave:approve', '#', 'admin', NOW(), 'Approve Leave'),
(5086, 'Reject Leave', 503, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:leave:reject', '#', 'admin', NOW(), 'Reject Leave'),
(5087, 'Cancel Leave', 503, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:leave:cancel', '#', 'admin', NOW(), 'Cancel Leave'),
(5088, 'Leave Balance', 503, 8, '', NULL, 1, 0, 'F', '0', '0', 'hr:leave:balance', '#', 'admin', NOW(), 'Leave Balance'),

-- Payroll
(504, 'Payroll', 5, 5, 'payroll', 'hr/payroll/index', 1, 0, 'C', '0', '0', 'hr:payroll:list', 'money', 'admin', NOW(), 'Payroll'),
(5091, 'View Payroll', 504, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:payroll:view', '#', 'admin', NOW(), 'View Payroll'),
(5092, 'Calculate Payroll', 504, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:payroll:calculate', '#', 'admin', NOW(), 'Calculate Payroll'),
(5093, 'Process Payroll', 504, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:payroll:process', '#', 'admin', NOW(), 'Process Payroll'),
(5094, 'Approve Payroll', 504, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:payroll:approve', '#', 'admin', NOW(), 'Approve Payroll'),
(5095, 'Payment', 504, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:payroll:payment', '#', 'admin', NOW(), 'Payment'),
(5096, 'View Payslip', 504, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:payroll:payslip', '#', 'admin', NOW(), 'View Payslip'),
(5097, 'Report', 504, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:payroll:report', '#', 'admin', NOW(), 'Report'),
(5098, 'Transfer', 504, 8, '', NULL, 1, 0, 'F', '0', '0', 'hr:payroll:transfer', '#', 'admin', NOW(), 'Transfer'),
(5099, 'Export Payroll', 504, 9, '', NULL, 1, 0, 'F', '0', '0', 'hr:payroll:export', '#', 'admin', NOW(), 'Export Payroll'),

-- Training
(505, 'Training', 5, 6, 'training', 'hr/training/index', 1, 0, 'C', '0', '0', 'hr:training:list', 'education', 'admin', NOW(), 'Training'),
(5101, 'Add Training', 505, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:training:add', '#', 'admin', NOW(), 'Add Training'),
(5102, 'Edit Training', 505, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:training:edit', '#', 'admin', NOW(), 'Edit Training'),
(5103, 'Delete Training', 505, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:training:delete', '#', 'admin', NOW(), 'Delete Training'),
(5104, 'Schedule Training', 505, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:training:schedule', '#', 'admin', NOW(), 'Schedule Training'),
(5105, 'Cancel Training', 505, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:training:cancel', '#', 'admin', NOW(), 'Cancel Training'),
(5106, 'Enroll Training', 505, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:training:enroll', '#', 'admin', NOW(), 'Enroll Training'),
(5107, 'Complete Training', 505, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:training:complete', '#', 'admin', NOW(), 'Complete Training'),
(5108, 'Training Export', 505, 8, '', NULL, 1, 0, 'F', '0', '0', 'hr:training:export', '#', 'admin', NOW(), 'Training Export'),

-- Performance
(506, 'Performance', 5, 7, 'performance', 'hr/performance/index', 1, 0, 'C', '0', '0', 'hr:performance:list', 'chart', 'admin', NOW(), 'Performance'),
(5121, 'View Performance', 506, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:performance:view', '#', 'admin', NOW(), 'View Performance'),
(5122, 'Query Performance', 506, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:performance:query', '#', 'admin', NOW(), 'Query Performance'),
(5123, 'Add Performance', 506, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:performance:add', '#', 'admin', NOW(), 'Add Performance'),
(5124, 'Edit Performance', 506, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:performance:edit', '#', 'admin', NOW(), 'Edit Performance'),
(5125, 'Delete Performance', 506, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:performance:delete', '#', 'admin', NOW(), 'Delete Performance'),
(5126, 'Submit Performance', 506, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:performance:submit', '#', 'admin', NOW(), 'Submit Performance'),
(5127, 'Approve Performance', 506, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:performance:approve', '#', 'admin', NOW(), 'Approve Performance'),
(5128, 'Performance Goals', 506, 8, '', NULL, 1, 0, 'F', '0', '0', 'hr:performance:goals', '#', 'admin', NOW(), 'Performance Goals'),
(5129, 'Export Performance', 506, 9, '', NULL, 1, 0, 'F', '0', '0', 'hr:performance:export', '#', 'admin', NOW(), 'Export Performance'),

-- Recruitment
(507, 'Recruitment', 5, 8, 'recruitment', 'hr/recruitment/index', 1, 0, 'C', '0', '0', 'hr:recruitment:list', 'search', 'admin', NOW(), 'Recruitment'),
(5131, 'View Recruitment', 507, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:recruitment:view', '#', 'admin', NOW(), 'View Recruitment'),
(5132, 'Query Recruitment', 507, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:recruitment:query', '#', 'admin', NOW(), 'Query Recruitment'),
(5133, 'Add Recruitment', 507, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:recruitment:add', '#', 'admin', NOW(), 'Add Recruitment'),
(5134, 'Edit Recruitment', 507, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:recruitment:edit', '#', 'admin', NOW(), 'Edit Recruitment'),
(5135, 'Approve Recruitment', 507, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:recruitment:approve', '#', 'admin', NOW(), 'Approve Recruitment'),
(5136, 'Publish Recruitment', 507, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:recruitment:publish', '#', 'admin', NOW(), 'Publish Recruitment'),
(5137, 'Close Recruitment', 507, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:recruitment:close', '#', 'admin', NOW(), 'Close Recruitment'),

-- Loans
(508, 'Loans', 5, 9, 'loans', 'hr/loans/index', 1, 0, 'C', '0', '0', 'hr:loans:list', 'money', 'admin', NOW(), 'Loans'),
(5151, 'Apply Loan', 508, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:loans:apply', '#', 'admin', NOW(), 'Apply Loan'),
(5152, 'Edit Loan', 508, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:loans:edit', '#', 'admin', NOW(), 'Edit Loan'),
(5153, 'Cancel Loan', 508, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:loans:cancel', '#', 'admin', NOW(), 'Cancel Loan'),
(5154, 'Approve Loan', 508, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:loans:approve', '#', 'admin', NOW(), 'Approve Loan'),
(5155, 'Repay Loan', 508, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:loans:repay', '#', 'admin', NOW(), 'Repay Loan'),
(5156, 'Process Loan', 508, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:loans:process', '#', 'admin', NOW(), 'Process Loan'),
(5157, 'Settle Loan', 508, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:loans:settle', '#', 'admin', NOW(), 'Settle Loan'),
(5158, 'Export Loans', 508, 8, '', NULL, 1, 0, 'F', '0', '0', 'hr:loans:export', '#', 'admin', NOW(), 'Export Loans'),

-- Benefits
(509, 'Benefits', 5, 10, 'benefits', 'hr/benefits/index', 1, 0, 'C', '0', '0', 'hr:benefits:list', 'gift', 'admin', NOW(), 'Benefits'),
(5171, 'Add Benefit', 509, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:benefits:add', '#', 'admin', NOW(), 'Add Benefit'),
(5172, 'Edit Benefit', 509, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:benefits:edit', '#', 'admin', NOW(), 'Edit Benefit'),
(5173, 'Delete Benefit', 509, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:benefits:delete', '#', 'admin', NOW(), 'Delete Benefit'),
(5174, 'Enroll Benefit', 509, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:benefits:enroll', '#', 'admin', NOW(), 'Enroll Benefit'),
(5175, 'Terminate Benefit', 509, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:benefits:terminate', '#', 'admin', NOW(), 'Terminate Benefit'),
(5176, 'Claim Benefit', 509, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:benefits:claim', '#', 'admin', NOW(), 'Claim Benefit'),
(5177, 'Approve Benefit', 509, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:benefits:approve', '#', 'admin', NOW(), 'Approve Benefit'),

-- KPI
(510, 'KPI', 5, 11, 'kpi', 'hr/kpi/index', 1, 0, 'C', '0', '0', 'hr:kpi:list', 'chart', 'admin', NOW(), 'KPI'),
(5181, 'Add KPI', 510, 1, '', NULL, 1, 0, 'F', '0', '0', 'hr:kpi:add', '#', 'admin', NOW(), 'Add KPI'),
(5182, 'Edit KPI', 510, 2, '', NULL, 1, 0, 'F', '0', '0', 'hr:kpi:edit', '#', 'admin', NOW(), 'Edit KPI'),
(5183, 'Delete KPI', 510, 3, '', NULL, 1, 0, 'F', '0', '0', 'hr:kpi:delete', '#', 'admin', NOW(), 'Delete KPI'),
(5184, 'Assign KPI', 510, 4, '', NULL, 1, 0, 'F', '0', '0', 'hr:kpi:assign', '#', 'admin', NOW(), 'Assign KPI'),
(5185, 'Record KPI', 510, 5, '', NULL, 1, 0, 'F', '0', '0', 'hr:kpi:record', '#', 'admin', NOW(), 'Record KPI'),
(5186, 'Verify KPI', 510, 6, '', NULL, 1, 0, 'F', '0', '0', 'hr:kpi:verify', '#', 'admin', NOW(), 'Verify KPI'),
(5187, 'Calculate KPI', 510, 7, '', NULL, 1, 0, 'F', '0', '0', 'hr:kpi:calculate', '#', 'admin', NOW(), 'Calculate KPI'),
(5188, 'Export KPI', 510, 8, '', NULL, 1, 0, 'F', '0', '0', 'hr:kpi:export', '#', 'admin', NOW(), 'Export KPI')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 6: EVENT MANAGEMENT MODULE (ID: 6)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- Event Management Directory
(6, 'Event Management', 0, 6, 'events', NULL, 1, 0, 'M', '0', '0', '', 'calendar', 'admin', NOW(), 'Event Management Module'),

-- Events
(600, 'Events', 6, 1, 'list', 'events/list/index', 1, 0, 'C', '0', '0', 'event:event:list', 'calendar', 'admin', NOW(), 'Events'),
(6001, 'View Event', 600, 1, '', NULL, 1, 0, 'F', '0', '0', 'event:event:view', '#', 'admin', NOW(), 'View Event'),
(6002, 'Add Event', 600, 2, '', NULL, 1, 0, 'F', '0', '0', 'event:event:add', '#', 'admin', NOW(), 'Add Event'),
(6003, 'Edit Event', 600, 3, '', NULL, 1, 0, 'F', '0', '0', 'event:event:edit', '#', 'admin', NOW(), 'Edit Event'),
(6004, 'Delete Event', 600, 4, '', NULL, 1, 0, 'F', '0', '0', 'event:event:delete', '#', 'admin', NOW(), 'Delete Event'),
(6005, 'Publish Event', 600, 5, '', NULL, 1, 0, 'F', '0', '0', 'event:event:publish', '#', 'admin', NOW(), 'Publish Event'),
(6006, 'Cancel Event', 600, 6, '', NULL, 1, 0, 'F', '0', '0', 'event:event:cancel', '#', 'admin', NOW(), 'Cancel Event'),
(6007, 'Export Events', 600, 7, '', NULL, 1, 0, 'F', '0', '0', 'event:event:export', '#', 'admin', NOW(), 'Export Events'),

-- Registrations
(601, 'Registrations', 6, 2, 'registrations', 'events/registrations/index', 1, 0, 'C', '0', '0', 'event:registration:list', 'form', 'admin', NOW(), 'Registrations'),
(6011, 'View Registration', 601, 1, '', NULL, 1, 0, 'F', '0', '0', 'event:registration:view', '#', 'admin', NOW(), 'View Registration'),
(6012, 'Add Registration', 601, 2, '', NULL, 1, 0, 'F', '0', '0', 'event:registration:add', '#', 'admin', NOW(), 'Add Registration'),
(6013, 'Edit Registration', 601, 3, '', NULL, 1, 0, 'F', '0', '0', 'event:registration:edit', '#', 'admin', NOW(), 'Edit Registration'),
(6014, 'Cancel Registration', 601, 4, '', NULL, 1, 0, 'F', '0', '0', 'event:registration:cancel', '#', 'admin', NOW(), 'Cancel Registration'),
(6015, 'Check In', 601, 5, '', NULL, 1, 0, 'F', '0', '0', 'event:registration:checkin', '#', 'admin', NOW(), 'Check In'),
(6016, 'Export Registrations', 601, 6, '', NULL, 1, 0, 'F', '0', '0', 'event:registration:export', '#', 'admin', NOW(), 'Export Registrations'),

-- Venues
(602, 'Venues', 6, 3, 'venues', 'events/venues/index', 1, 0, 'C', '0', '0', 'event:venue:list', 'location', 'admin', NOW(), 'Venues'),
(6021, 'View Venue', 602, 1, '', NULL, 1, 0, 'F', '0', '0', 'event:venue:view', '#', 'admin', NOW(), 'View Venue'),
(6022, 'Add Venue', 602, 2, '', NULL, 1, 0, 'F', '0', '0', 'event:venue:add', '#', 'admin', NOW(), 'Add Venue'),
(6023, 'Edit Venue', 602, 3, '', NULL, 1, 0, 'F', '0', '0', 'event:venue:edit', '#', 'admin', NOW(), 'Edit Venue'),
(6024, 'Delete Venue', 602, 4, '', NULL, 1, 0, 'F', '0', '0', 'event:venue:delete', '#', 'admin', NOW(), 'Delete Venue'),

-- Tickets
(606, 'Tickets', 6, 7, 'tickets', 'events/tickets/index', 1, 0, 'C', '0', '0', 'event:ticket:list', 'ticket', 'admin', NOW(), 'Tickets'),
(6061, 'Add Ticket', 606, 1, '', NULL, 1, 0, 'F', '0', '0', 'event:ticket:add', '#', 'admin', NOW(), 'Add Ticket'),
(6062, 'Edit Ticket', 606, 2, '', NULL, 1, 0, 'F', '0', '0', 'event:ticket:edit', '#', 'admin', NOW(), 'Edit Ticket'),
(6063, 'Delete Ticket', 606, 3, '', NULL, 1, 0, 'F', '0', '0', 'event:ticket:delete', '#', 'admin', NOW(), 'Delete Ticket'),
(6064, 'Issue Ticket', 606, 4, '', NULL, 1, 0, 'F', '0', '0', 'event:ticket:issue', '#', 'admin', NOW(), 'Issue Ticket'),
(6065, 'Refund Ticket', 606, 5, '', NULL, 1, 0, 'F', '0', '0', 'event:ticket:refund', '#', 'admin', NOW(), 'Refund Ticket')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 7: BUSINESS PARTNERS MODULE (ID: 7)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- Business Partners Directory
(7, 'Business Partners', 0, 7, 'bp', NULL, 1, 0, 'M', '0', '0', '', 'peoples', 'admin', NOW(), 'Business Partners Module'),

-- Partners
(700, 'Partners', 7, 1, 'partners', 'bp/partners/index', 1, 0, 'C', '0', '0', 'bp:partner:list', 'handshake', 'admin', NOW(), 'Partners'),
(7001, 'View Partner', 700, 1, '', NULL, 1, 0, 'F', '0', '0', 'bp:partner:view', '#', 'admin', NOW(), 'View Partner'),
(7002, 'Add Partner', 700, 2, '', NULL, 1, 0, 'F', '0', '0', 'bp:partner:add', '#', 'admin', NOW(), 'Add Partner'),
(7003, 'Edit Partner', 700, 3, '', NULL, 1, 0, 'F', '0', '0', 'bp:partner:edit', '#', 'admin', NOW(), 'Edit Partner'),
(7004, 'Delete Partner', 700, 4, '', NULL, 1, 0, 'F', '0', '0', 'bp:partner:delete', '#', 'admin', NOW(), 'Delete Partner'),
(7005, 'Export Partners', 700, 5, '', NULL, 1, 0, 'F', '0', '0', 'bp:partner:export', '#', 'admin', NOW(), 'Export Partners'),

-- Customers
(701, 'Customers', 7, 2, 'customers', 'bp/customers/index', 1, 0, 'C', '0', '0', 'bp:customer:list', 'user', 'admin', NOW(), 'Customers'),
(7011, 'View Customer', 701, 1, '', NULL, 1, 0, 'F', '0', '0', 'bp:customer:view', '#', 'admin', NOW(), 'View Customer'),
(7012, 'Add Customer', 701, 2, '', NULL, 1, 0, 'F', '0', '0', 'bp:customer:add', '#', 'admin', NOW(), 'Add Customer'),
(7013, 'Edit Customer', 701, 3, '', NULL, 1, 0, 'F', '0', '0', 'bp:customer:edit', '#', 'admin', NOW(), 'Edit Customer'),
(7014, 'Delete Customer', 701, 4, '', NULL, 1, 0, 'F', '0', '0', 'bp:customer:delete', '#', 'admin', NOW(), 'Delete Customer'),
(7015, 'Export Customers', 701, 5, '', NULL, 1, 0, 'F', '0', '0', 'bp:customer:export', '#', 'admin', NOW(), 'Export Customers'),

-- Vendors
(702, 'Vendors', 7, 3, 'vendors', 'bp/vendors/index', 1, 0, 'C', '0', '0', 'bp:vendor:list', 'shop', 'admin', NOW(), 'Vendors'),
(7021, 'View Vendor', 702, 1, '', NULL, 1, 0, 'F', '0', '0', 'bp:vendor:view', '#', 'admin', NOW(), 'View Vendor'),
(7022, 'Add Vendor', 702, 2, '', NULL, 1, 0, 'F', '0', '0', 'bp:vendor:add', '#', 'admin', NOW(), 'Add Vendor'),
(7023, 'Edit Vendor', 702, 3, '', NULL, 1, 0, 'F', '0', '0', 'bp:vendor:edit', '#', 'admin', NOW(), 'Edit Vendor'),
(7024, 'Delete Vendor', 702, 4, '', NULL, 1, 0, 'F', '0', '0', 'bp:vendor:delete', '#', 'admin', NOW(), 'Delete Vendor'),
(7025, 'Export Vendors', 702, 5, '', NULL, 1, 0, 'F', '0', '0', 'bp:vendor:export', '#', 'admin', NOW(), 'Export Vendors')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 8: BOULEVARD MODULE (ID: 8)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- Boulevard Directory
(8, 'Boulevard', 0, 8, 'boulevard', NULL, 1, 0, 'M', '0', '0', '', 'shop', 'admin', NOW(), 'Boulevard Integration Module'),

-- Locations
(800, 'Locations', 8, 1, 'locations', 'boulevard/locations/index', 1, 0, 'C', '0', '0', 'boulevard:location:list', 'location', 'admin', NOW(), 'Locations'),
(8001, 'View Location', 800, 1, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:location:view', '#', 'admin', NOW(), 'View Location'),
(8002, 'Sync Locations', 800, 2, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:location:sync', '#', 'admin', NOW(), 'Sync Locations'),

-- Services
(801, 'Services', 8, 2, 'services', 'boulevard/services/index', 1, 0, 'C', '0', '0', 'boulevard:service:list', 'service', 'admin', NOW(), 'Services'),
(8011, 'View Service', 801, 1, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:service:view', '#', 'admin', NOW(), 'View Service'),
(8012, 'Sync Services', 801, 2, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:service:sync', '#', 'admin', NOW(), 'Sync Services'),

-- Appointments
(802, 'Appointments', 8, 3, 'appointments', 'boulevard/appointments/index', 1, 0, 'C', '0', '0', 'boulevard:appointment:list', 'calendar', 'admin', NOW(), 'Appointments'),
(8021, 'View Appointment', 802, 1, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:appointment:view', '#', 'admin', NOW(), 'View Appointment'),
(8022, 'Create Appointment', 802, 2, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:appointment:create', '#', 'admin', NOW(), 'Create Appointment'),
(8023, 'Update Appointment', 802, 3, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:appointment:update', '#', 'admin', NOW(), 'Update Appointment'),
(8024, 'Cancel Appointment', 802, 4, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:appointment:cancel', '#', 'admin', NOW(), 'Cancel Appointment'),
(8025, 'Sync Appointments', 802, 5, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:appointment:sync', '#', 'admin', NOW(), 'Sync Appointments'),

-- Clients
(803, 'Clients', 8, 4, 'clients', 'boulevard/clients/index', 1, 0, 'C', '0', '0', 'boulevard:client:list', 'user', 'admin', NOW(), 'Clients'),
(8031, 'View Client', 803, 1, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:client:view', '#', 'admin', NOW(), 'View Client'),
(8032, 'Sync Clients', 803, 2, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:client:sync', '#', 'admin', NOW(), 'Sync Clients'),

-- Staff
(804, 'Staff', 8, 5, 'staff', 'boulevard/staff/index', 1, 0, 'C', '0', '0', 'boulevard:staff:list', 'peoples', 'admin', NOW(), 'Staff'),
(8041, 'View Staff', 804, 1, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:staff:view', '#', 'admin', NOW(), 'View Staff'),
(8042, 'Sync Staff', 804, 2, '', NULL, 1, 0, 'F', '0', '0', 'boulevard:staff:sync', '#', 'admin', NOW(), 'Sync Staff')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 9: AI ASSISTANT MODULE (ID: 9)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- AI Directory
(9, 'AI Assistant', 0, 9, 'ai', NULL, 1, 0, 'M', '0', '0', '', 'robot', 'admin', NOW(), 'AI Assistant Module'),

-- Chat
(900, 'Chat', 9, 1, 'chat', 'ai/chat/index', 1, 0, 'C', '0', '0', 'ai:chat:access', 'message', 'admin', NOW(), 'AI Chat'),
(9001, 'Send Message', 900, 1, '', NULL, 1, 0, 'F', '0', '0', 'ai:chat:send', '#', 'admin', NOW(), 'Send Message'),
(9002, 'View History', 900, 2, '', NULL, 1, 0, 'F', '0', '0', 'ai:chat:history', '#', 'admin', NOW(), 'View History'),

-- Knowledge Base
(901, 'Knowledge Base', 9, 2, 'knowledge', 'ai/knowledge/index', 1, 0, 'C', '0', '0', 'ai:knowledge:list', 'book', 'admin', NOW(), 'Knowledge Base'),
(9011, 'Add Knowledge', 901, 1, '', NULL, 1, 0, 'F', '0', '0', 'ai:knowledge:add', '#', 'admin', NOW(), 'Add Knowledge'),
(9012, 'Edit Knowledge', 901, 2, '', NULL, 1, 0, 'F', '0', '0', 'ai:knowledge:edit', '#', 'admin', NOW(), 'Edit Knowledge'),
(9013, 'Delete Knowledge', 901, 3, '', NULL, 1, 0, 'F', '0', '0', 'ai:knowledge:delete', '#', 'admin', NOW(), 'Delete Knowledge'),

-- AI Settings
(902, 'AI Settings', 9, 3, 'settings', 'ai/settings/index', 1, 0, 'C', '0', '0', 'ai:settings:view', 'setting', 'admin', NOW(), 'AI Settings'),
(9021, 'Update Settings', 902, 1, '', NULL, 1, 0, 'F', '0', '0', 'ai:settings:update', '#', 'admin', NOW(), 'Update Settings')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 10: CONTENT MODULE (ID: 10)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- Content Directory
(10, 'Content', 0, 10, 'content', NULL, 1, 0, 'M', '0', '0', '', 'edit', 'admin', NOW(), 'Content Management Module'),

-- Articles
(1000, 'Articles', 10, 1, 'articles', 'content/articles/index', 1, 0, 'C', '0', '0', 'content:article:list', 'documentation', 'admin', NOW(), 'Articles'),
(10001, 'View Article', 1000, 1, '', NULL, 1, 0, 'F', '0', '0', 'content:article:view', '#', 'admin', NOW(), 'View Article'),
(10002, 'Add Article', 1000, 2, '', NULL, 1, 0, 'F', '0', '0', 'content:article:add', '#', 'admin', NOW(), 'Add Article'),
(10003, 'Edit Article', 1000, 3, '', NULL, 1, 0, 'F', '0', '0', 'content:article:edit', '#', 'admin', NOW(), 'Edit Article'),
(10004, 'Delete Article', 1000, 4, '', NULL, 1, 0, 'F', '0', '0', 'content:article:delete', '#', 'admin', NOW(), 'Delete Article'),
(10005, 'Publish Article', 1000, 5, '', NULL, 1, 0, 'F', '0', '0', 'content:article:publish', '#', 'admin', NOW(), 'Publish Article'),

-- Categories
(1001, 'Categories', 10, 2, 'categories', 'content/categories/index', 1, 0, 'C', '0', '0', 'content:category:list', 'tree', 'admin', NOW(), 'Categories'),
(10011, 'Add Category', 1001, 1, '', NULL, 1, 0, 'F', '0', '0', 'content:category:add', '#', 'admin', NOW(), 'Add Category'),
(10012, 'Edit Category', 1001, 2, '', NULL, 1, 0, 'F', '0', '0', 'content:category:edit', '#', 'admin', NOW(), 'Edit Category'),
(10013, 'Delete Category', 1001, 3, '', NULL, 1, 0, 'F', '0', '0', 'content:category:delete', '#', 'admin', NOW(), 'Delete Category'),

-- Media
(1002, 'Media', 10, 3, 'media', 'content/media/index', 1, 0, 'C', '0', '0', 'content:media:list', 'image', 'admin', NOW(), 'Media'),
(10021, 'Upload Media', 1002, 1, '', NULL, 1, 0, 'F', '0', '0', 'content:media:upload', '#', 'admin', NOW(), 'Upload Media'),
(10022, 'Delete Media', 1002, 2, '', NULL, 1, 0, 'F', '0', '0', 'content:media:delete', '#', 'admin', NOW(), 'Delete Media')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 11: FINANCE MODULE (ID: 11)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- Finance Directory
(11, 'Finance', 0, 11, 'finance', NULL, 1, 0, 'M', '0', '0', '', 'money', 'admin', NOW(), 'Finance Module'),

-- Accounts
(1100, 'Accounts', 11, 1, 'accounts', 'finance/accounts/index', 1, 0, 'C', '0', '0', 'finance:account:list', 'wallet', 'admin', NOW(), 'Accounts'),
(11001, 'View Account', 1100, 1, '', NULL, 1, 0, 'F', '0', '0', 'finance:account:view', '#', 'admin', NOW(), 'View Account'),
(11002, 'Add Account', 1100, 2, '', NULL, 1, 0, 'F', '0', '0', 'finance:account:add', '#', 'admin', NOW(), 'Add Account'),
(11003, 'Edit Account', 1100, 3, '', NULL, 1, 0, 'F', '0', '0', 'finance:account:edit', '#', 'admin', NOW(), 'Edit Account'),
(11004, 'Delete Account', 1100, 4, '', NULL, 1, 0, 'F', '0', '0', 'finance:account:delete', '#', 'admin', NOW(), 'Delete Account'),

-- Transactions
(1101, 'Transactions', 11, 2, 'transactions', 'finance/transactions/index', 1, 0, 'C', '0', '0', 'finance:transaction:list', 'swap', 'admin', NOW(), 'Transactions'),
(11011, 'View Transaction', 1101, 1, '', NULL, 1, 0, 'F', '0', '0', 'finance:transaction:view', '#', 'admin', NOW(), 'View Transaction'),
(11012, 'Add Transaction', 1101, 2, '', NULL, 1, 0, 'F', '0', '0', 'finance:transaction:add', '#', 'admin', NOW(), 'Add Transaction'),
(11013, 'Edit Transaction', 1101, 3, '', NULL, 1, 0, 'F', '0', '0', 'finance:transaction:edit', '#', 'admin', NOW(), 'Edit Transaction'),
(11014, 'Delete Transaction', 1101, 4, '', NULL, 1, 0, 'F', '0', '0', 'finance:transaction:delete', '#', 'admin', NOW(), 'Delete Transaction'),
(11015, 'Approve Transaction', 1101, 5, '', NULL, 1, 0, 'F', '0', '0', 'finance:transaction:approve', '#', 'admin', NOW(), 'Approve Transaction'),
(11016, 'Export Transactions', 1101, 6, '', NULL, 1, 0, 'F', '0', '0', 'finance:transaction:export', '#', 'admin', NOW(), 'Export Transactions'),

-- Invoices
(1102, 'Invoices', 11, 3, 'invoices', 'finance/invoices/index', 1, 0, 'C', '0', '0', 'finance:invoice:list', 'bill', 'admin', NOW(), 'Invoices'),
(11021, 'View Invoice', 1102, 1, '', NULL, 1, 0, 'F', '0', '0', 'finance:invoice:view', '#', 'admin', NOW(), 'View Invoice'),
(11022, 'Add Invoice', 1102, 2, '', NULL, 1, 0, 'F', '0', '0', 'finance:invoice:add', '#', 'admin', NOW(), 'Add Invoice'),
(11023, 'Edit Invoice', 1102, 3, '', NULL, 1, 0, 'F', '0', '0', 'finance:invoice:edit', '#', 'admin', NOW(), 'Edit Invoice'),
(11024, 'Delete Invoice', 1102, 4, '', NULL, 1, 0, 'F', '0', '0', 'finance:invoice:delete', '#', 'admin', NOW(), 'Delete Invoice'),
(11025, 'Send Invoice', 1102, 5, '', NULL, 1, 0, 'F', '0', '0', 'finance:invoice:send', '#', 'admin', NOW(), 'Send Invoice'),
(11026, 'Export Invoices', 1102, 6, '', NULL, 1, 0, 'F', '0', '0', 'finance:invoice:export', '#', 'admin', NOW(), 'Export Invoices'),

-- Payments
(1103, 'Payments', 11, 4, 'payments', 'finance/payments/index', 1, 0, 'C', '0', '0', 'finance:payment:list', 'money', 'admin', NOW(), 'Payments'),
(11031, 'View Payment', 1103, 1, '', NULL, 1, 0, 'F', '0', '0', 'finance:payment:view', '#', 'admin', NOW(), 'View Payment'),
(11032, 'Add Payment', 1103, 2, '', NULL, 1, 0, 'F', '0', '0', 'finance:payment:add', '#', 'admin', NOW(), 'Add Payment'),
(11033, 'Refund Payment', 1103, 3, '', NULL, 1, 0, 'F', '0', '0', 'finance:payment:refund', '#', 'admin', NOW(), 'Refund Payment'),
(11034, 'Export Payments', 1103, 4, '', NULL, 1, 0, 'F', '0', '0', 'finance:payment:export', '#', 'admin', NOW(), 'Export Payments'),

-- Expenses
(1104, 'Expenses', 11, 5, 'expenses', 'finance/expenses/index', 1, 0, 'C', '0', '0', 'finance:expense:list', 'expense', 'admin', NOW(), 'Expenses'),
(11041, 'View Expense', 1104, 1, '', NULL, 1, 0, 'F', '0', '0', 'finance:expense:view', '#', 'admin', NOW(), 'View Expense'),
(11042, 'Add Expense', 1104, 2, '', NULL, 1, 0, 'F', '0', '0', 'finance:expense:add', '#', 'admin', NOW(), 'Add Expense'),
(11043, 'Edit Expense', 1104, 3, '', NULL, 1, 0, 'F', '0', '0', 'finance:expense:edit', '#', 'admin', NOW(), 'Edit Expense'),
(11044, 'Delete Expense', 1104, 4, '', NULL, 1, 0, 'F', '0', '0', 'finance:expense:delete', '#', 'admin', NOW(), 'Delete Expense'),
(11045, 'Approve Expense', 1104, 5, '', NULL, 1, 0, 'F', '0', '0', 'finance:expense:approve', '#', 'admin', NOW(), 'Approve Expense'),
(11046, 'Export Expenses', 1104, 6, '', NULL, 1, 0, 'F', '0', '0', 'finance:expense:export', '#', 'admin', NOW(), 'Export Expenses'),

-- Reports
(1105, 'Reports', 11, 6, 'reports', 'finance/reports/index', 1, 0, 'C', '0', '0', 'finance:report:list', 'chart', 'admin', NOW(), 'Reports'),
(11051, 'View Report', 1105, 1, '', NULL, 1, 0, 'F', '0', '0', 'finance:report:view', '#', 'admin', NOW(), 'View Report'),
(11052, 'Generate Report', 1105, 2, '', NULL, 1, 0, 'F', '0', '0', 'finance:report:generate', '#', 'admin', NOW(), 'Generate Report'),
(11053, 'Export Report', 1105, 3, '', NULL, 1, 0, 'F', '0', '0', 'finance:report:export', '#', 'admin', NOW(), 'Export Report')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 12: ERP MODULE (ID: 12)
-- ====================================================
INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
-- ERP Directory
(12, 'ERP', 0, 12, 'erp', NULL, 1, 0, 'M', '0', '0', '', 'component', 'admin', NOW(), 'ERP Module'),

-- Products
(1200, 'Products', 12, 1, 'products', 'erp/products/index', 1, 0, 'C', '0', '0', 'erp:product:list', 'shopping', 'admin', NOW(), 'Products'),
(12001, 'View Product', 1200, 1, '', NULL, 1, 0, 'F', '0', '0', 'erp:product:view', '#', 'admin', NOW(), 'View Product'),
(12002, 'Add Product', 1200, 2, '', NULL, 1, 0, 'F', '0', '0', 'erp:product:add', '#', 'admin', NOW(), 'Add Product'),
(12003, 'Edit Product', 1200, 3, '', NULL, 1, 0, 'F', '0', '0', 'erp:product:edit', '#', 'admin', NOW(), 'Edit Product'),
(12004, 'Delete Product', 1200, 4, '', NULL, 1, 0, 'F', '0', '0', 'erp:product:delete', '#', 'admin', NOW(), 'Delete Product'),
(12005, 'Export Products', 1200, 5, '', NULL, 1, 0, 'F', '0', '0', 'erp:product:export', '#', 'admin', NOW(), 'Export Products'),

-- Inventory
(1201, 'Inventory', 12, 2, 'inventory', 'erp/inventory/index', 1, 0, 'C', '0', '0', 'erp:inventory:list', 'inventory', 'admin', NOW(), 'Inventory'),
(12011, 'View Inventory', 1201, 1, '', NULL, 1, 0, 'F', '0', '0', 'erp:inventory:view', '#', 'admin', NOW(), 'View Inventory'),
(12012, 'Adjust Inventory', 1201, 2, '', NULL, 1, 0, 'F', '0', '0', 'erp:inventory:adjust', '#', 'admin', NOW(), 'Adjust Inventory'),
(12013, 'Transfer Inventory', 1201, 3, '', NULL, 1, 0, 'F', '0', '0', 'erp:inventory:transfer', '#', 'admin', NOW(), 'Transfer Inventory'),
(12014, 'Stock Count', 1201, 4, '', NULL, 1, 0, 'F', '0', '0', 'erp:inventory:count', '#', 'admin', NOW(), 'Stock Count'),
(12015, 'Export Inventory', 1201, 5, '', NULL, 1, 0, 'F', '0', '0', 'erp:inventory:export', '#', 'admin', NOW(), 'Export Inventory'),

-- Purchase Orders
(1202, 'Purchase Orders', 12, 3, 'purchase', 'erp/purchase/index', 1, 0, 'C', '0', '0', 'erp:purchase:list', 'shopping-cart', 'admin', NOW(), 'Purchase Orders'),
(12021, 'View Purchase', 1202, 1, '', NULL, 1, 0, 'F', '0', '0', 'erp:purchase:view', '#', 'admin', NOW(), 'View Purchase'),
(12022, 'Add Purchase', 1202, 2, '', NULL, 1, 0, 'F', '0', '0', 'erp:purchase:add', '#', 'admin', NOW(), 'Add Purchase'),
(12023, 'Edit Purchase', 1202, 3, '', NULL, 1, 0, 'F', '0', '0', 'erp:purchase:edit', '#', 'admin', NOW(), 'Edit Purchase'),
(12024, 'Delete Purchase', 1202, 4, '', NULL, 1, 0, 'F', '0', '0', 'erp:purchase:delete', '#', 'admin', NOW(), 'Delete Purchase'),
(12025, 'Approve Purchase', 1202, 5, '', NULL, 1, 0, 'F', '0', '0', 'erp:purchase:approve', '#', 'admin', NOW(), 'Approve Purchase'),
(12026, 'Receive Purchase', 1202, 6, '', NULL, 1, 0, 'F', '0', '0', 'erp:purchase:receive', '#', 'admin', NOW(), 'Receive Purchase'),

-- Sales Orders
(1203, 'Sales Orders', 12, 4, 'sales', 'erp/sales/index', 1, 0, 'C', '0', '0', 'erp:sales:list', 'sell', 'admin', NOW(), 'Sales Orders'),
(12031, 'View Sales', 1203, 1, '', NULL, 1, 0, 'F', '0', '0', 'erp:sales:view', '#', 'admin', NOW(), 'View Sales'),
(12032, 'Add Sales', 1203, 2, '', NULL, 1, 0, 'F', '0', '0', 'erp:sales:add', '#', 'admin', NOW(), 'Add Sales'),
(12033, 'Edit Sales', 1203, 3, '', NULL, 1, 0, 'F', '0', '0', 'erp:sales:edit', '#', 'admin', NOW(), 'Edit Sales'),
(12034, 'Delete Sales', 1203, 4, '', NULL, 1, 0, 'F', '0', '0', 'erp:sales:delete', '#', 'admin', NOW(), 'Delete Sales'),
(12035, 'Fulfill Sales', 1203, 5, '', NULL, 1, 0, 'F', '0', '0', 'erp:sales:fulfill', '#', 'admin', NOW(), 'Fulfill Sales'),

-- Warehouses
(1204, 'Warehouses', 12, 5, 'warehouses', 'erp/warehouses/index', 1, 0, 'C', '0', '0', 'erp:warehouse:list', 'warehouse', 'admin', NOW(), 'Warehouses'),
(12041, 'View Warehouse', 1204, 1, '', NULL, 1, 0, 'F', '0', '0', 'erp:warehouse:view', '#', 'admin', NOW(), 'View Warehouse'),
(12042, 'Add Warehouse', 1204, 2, '', NULL, 1, 0, 'F', '0', '0', 'erp:warehouse:add', '#', 'admin', NOW(), 'Add Warehouse'),
(12043, 'Edit Warehouse', 1204, 3, '', NULL, 1, 0, 'F', '0', '0', 'erp:warehouse:edit', '#', 'admin', NOW(), 'Edit Warehouse'),
(12044, 'Delete Warehouse', 1204, 4, '', NULL, 1, 0, 'F', '0', '0', 'erp:warehouse:delete', '#', 'admin', NOW(), 'Delete Warehouse')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 13: ASSIGN ALL PERMISSIONS TO ADMIN ROLE (role_id = 1)
-- ====================================================
-- IMPORTANT: This step is CRITICAL! Without role-menu assignments, the menu API returns empty.
-- This assigns ALL menu items to the admin role (role_id = 1)

-- Version 1: For lowercase column names (default PostgreSQL convention)
INSERT INTO sys_role_menu (role_id, menu_id)
SELECT 1, menuid FROM sys_menu
WHERE menuid NOT IN (SELECT menu_id FROM sys_role_menu WHERE role_id = 1)
ON CONFLICT (role_id, menu_id) DO NOTHING;

-- Version 2: If your table uses quoted/mixed-case column names, use this instead:
-- INSERT INTO sys_role_menu ("role_Id", "menu_Id")
-- SELECT 1, "menuId" FROM sys_menu
-- WHERE "menuId" NOT IN (SELECT "menu_Id" FROM sys_role_menu WHERE "role_Id" = 1)
-- ON CONFLICT ("role_Id", "menu_Id") DO NOTHING;

-- ====================================================
-- VERIFICATION QUERIES (Run these to check the results)
-- ====================================================
-- Count all menus by type
SELECT menutype, COUNT(*) as count FROM sys_menu GROUP BY menutype ORDER BY menutype;

-- Show all modules (top-level menus)
SELECT menuid, menuname, perms FROM sys_menu WHERE parentid = 0 ORDER BY ordernum;

-- Count total permissions
SELECT COUNT(*) as total_permissions FROM sys_menu WHERE perms != '' AND perms IS NOT NULL;

-- Show permissions assigned to admin role
SELECT COUNT(*) as admin_permissions FROM sys_role_menu WHERE role_id = 1;

-- Check if sys_role_menu is empty (if this returns 0, run SECTION 13 again)
SELECT COUNT(*) as total_role_menu_entries FROM sys_role_menu;
