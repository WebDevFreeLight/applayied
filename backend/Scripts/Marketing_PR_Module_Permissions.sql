-- ====================================================
-- Seed Permissions for Marketing PR Module
-- AI Targeting, Campaigns, Journeys, Investors, Crowd,
-- Social Media, Email, Content, PR, Advertising
-- Generated: 2026-02-12
-- ====================================================
-- Safe to re-run: uses ON CONFLICT DO NOTHING
-- Run via: docker compose exec postgres psql -U mahamadmin -d MahamDB -f /path/to/this.sql
-- Or paste into psql shell directly
-- IDs: 60000-60999
-- ====================================================

-- ====================================================
-- SECTION 1: MARKETING PR MODULE PARENT (ID: 60000)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark", "menuName_key")
VALUES (60000, 'Marketing & PR', 0, 25, 'marketing-pr', NULL, 1, 0, 'M', '0', '0', '', 'megaphone', 'admin', NOW(), 'Marketing & PR Module', 'sidebar-menu-marketing-pr')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 2: DASHBOARD (ID: 60100)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (60100, 'Marketing Dashboard', 60000, 1, 'dashboard', NULL, 1, 0, 'C', '0', '0', 'marketing:pr:dashboard', 'chart-bar', 'admin', NOW(), 'Marketing PR Dashboard')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(60101, 'View Dashboard', 60100, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:pr:dashboard:view', '#', 'admin', NOW(), 'View Marketing Dashboard')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 3: CAMPAIGNS (ID: 60200)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (60200, 'Campaigns', 60000, 2, 'campaigns', NULL, 1, 0, 'C', '0', '0', 'marketing:campaigns:list', 'megaphone', 'admin', NOW(), 'Campaign Management')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(60201, 'Query Campaigns', 60200, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:campaigns:list', '#', 'admin', NOW(), 'Query Campaigns'),
(60202, 'View Campaign', 60200, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:campaigns:view', '#', 'admin', NOW(), 'View Campaign Detail'),
(60203, 'Add Campaign', 60200, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:campaigns:add', '#', 'admin', NOW(), 'Add Campaign'),
(60204, 'Edit Campaign', 60200, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:campaigns:edit', '#', 'admin', NOW(), 'Edit Campaign'),
(60205, 'Delete Campaign', 60200, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:campaigns:delete', '#', 'admin', NOW(), 'Delete Campaign'),
(60206, 'Export Campaigns', 60200, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:campaigns:export', '#', 'admin', NOW(), 'Export Campaigns')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 4: AI TARGETING ENGINE (ID: 60300)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (60300, 'AI Targeting', 60000, 3, 'ai-targeting', NULL, 1, 0, 'C', '0', '0', 'marketing:ai:list', 'robot', 'admin', NOW(), 'AI Predictive Targeting Engine')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(60301, 'View AI Segments', 60300, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ai:segments', '#', 'admin', NOW(), 'View AI Segments'),
(60302, 'Manage Models', 60300, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ai:models', '#', 'admin', NOW(), 'Manage Predictive Models'),
(60303, 'View Predictions', 60300, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ai:predictions', '#', 'admin', NOW(), 'View Behavior Predictions'),
(60304, 'Manage AI Agents', 60300, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ai:agents', '#', 'admin', NOW(), 'Manage AI Agents'),
(60305, 'Competitor Analysis', 60300, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ai:competitors', '#', 'admin', NOW(), 'Competitor Analysis'),
(60306, 'Budget Optimization', 60300, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ai:budget', '#', 'admin', NOW(), 'Budget Optimization')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 5: CUSTOMER JOURNEYS (ID: 60400)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (60400, 'Customer Journeys', 60000, 4, 'customer-journeys', NULL, 1, 0, 'C', '0', '0', 'marketing:journeys:list', 'flow-arrow', 'admin', NOW(), 'Customer Journey Automation')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(60401, 'Query Journeys', 60400, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:journeys:list', '#', 'admin', NOW(), 'Query Journeys'),
(60402, 'Add Journey', 60400, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:journeys:add', '#', 'admin', NOW(), 'Add Journey'),
(60403, 'Edit Journey', 60400, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:journeys:edit', '#', 'admin', NOW(), 'Edit Journey'),
(60404, 'Delete Journey', 60400, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:journeys:delete', '#', 'admin', NOW(), 'Delete Journey'),
(60405, 'Manage Triggers', 60400, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:journeys:triggers', '#', 'admin', NOW(), 'Manage Journey Triggers'),
(60406, 'View Analytics', 60400, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:journeys:analytics', '#', 'admin', NOW(), 'View Journey Analytics')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 6: INVESTOR NEXUS (ID: 60500)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (60500, 'Investor Nexus', 60000, 5, 'investor-nexus', NULL, 1, 0, 'C', '0', '0', 'marketing:investors:list', 'handshake', 'admin', NOW(), 'Merchant-Investor Nexus')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(60501, 'View Opportunities', 60500, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:investors:opportunities', '#', 'admin', NOW(), 'View Investment Opportunities'),
(60502, 'Manage Packages', 60500, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:investors:packages', '#', 'admin', NOW(), 'Manage Sponsor Packages'),
(60503, 'Review Applications', 60500, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:investors:applications', '#', 'admin', NOW(), 'Review Sponsor Applications'),
(60504, 'KPI Reports', 60500, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:investors:kpi', '#', 'admin', NOW(), 'Generate KPI Reports'),
(60505, 'Export Reports', 60500, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:investors:export', '#', 'admin', NOW(), 'Export Investor Reports')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 7: CROWD MARKETING (ID: 60600)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (60600, 'Crowd Marketing', 60000, 6, 'crowd-marketing', NULL, 1, 0, 'C', '0', '0', 'marketing:crowd:list', 'users-four', 'admin', NOW(), 'Crowd-Sourced Marketing Analytics')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(60601, 'View Analytics', 60600, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:crowd:analytics', '#', 'admin', NOW(), 'View Crowd Analytics'),
(60602, 'View Heatmaps', 60600, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:crowd:heatmaps', '#', 'admin', NOW(), 'View Crowd Heatmaps'),
(60603, 'Exhibitor Reports', 60600, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:crowd:exhibitors', '#', 'admin', NOW(), 'Exhibitor Reports'),
(60604, 'AI Insights', 60600, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:crowd:insights', '#', 'admin', NOW(), 'Real-time AI Insights')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 8: EMAIL MARKETING (ID: 60700)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (60700, 'Email Marketing', 60000, 7, 'email', NULL, 1, 0, 'C', '0', '0', 'marketing:email:list', 'envelope', 'admin', NOW(), 'Email Marketing Management')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(60701, 'Manage Templates', 60700, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:email:templates', '#', 'admin', NOW(), 'Manage Email Templates'),
(60702, 'Manage Campaigns', 60700, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:email:campaigns', '#', 'admin', NOW(), 'Manage Email Campaigns'),
(60703, 'Send Campaign', 60700, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:email:send', '#', 'admin', NOW(), 'Send Email Campaign'),
(60704, 'Manage Subscribers', 60700, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:email:subscribers', '#', 'admin', NOW(), 'Manage Subscribers'),
(60705, 'Import Subscribers', 60700, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:email:import', '#', 'admin', NOW(), 'Import Subscribers'),
(60706, 'View Analytics', 60700, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:email:analytics', '#', 'admin', NOW(), 'View Email Analytics')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 9: SOCIAL MEDIA (ID: 60800)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (60800, 'Social Media', 60000, 8, 'social-media', NULL, 1, 0, 'C', '0', '0', 'marketing:social:list', 'share-network', 'admin', NOW(), 'Social Media Management')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(60801, 'Manage Accounts', 60800, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:social:accounts', '#', 'admin', NOW(), 'Manage Social Accounts'),
(60802, 'Manage Posts', 60800, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:social:posts', '#', 'admin', NOW(), 'Manage Social Posts'),
(60803, 'Publish Post', 60800, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:social:publish', '#', 'admin', NOW(), 'Publish Social Post'),
(60804, 'AI Generate', 60800, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:social:ai-generate', '#', 'admin', NOW(), 'AI Generate Social Content'),
(60805, 'View Analytics', 60800, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:social:analytics', '#', 'admin', NOW(), 'View Social Analytics')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 10: CONTENT MANAGEMENT (ID: 60900)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (60900, 'Content Management', 60000, 9, 'content', NULL, 1, 0, 'C', '0', '0', 'marketing:content:list', 'article', 'admin', NOW(), 'Content Library & AI Generation')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(60901, 'Query Content', 60900, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:content:list', '#', 'admin', NOW(), 'Query Content'),
(60902, 'Add Content', 60900, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:content:add', '#', 'admin', NOW(), 'Add Content'),
(60903, 'Edit Content', 60900, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:content:edit', '#', 'admin', NOW(), 'Edit Content'),
(60904, 'Delete Content', 60900, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:content:delete', '#', 'admin', NOW(), 'Delete Content'),
(60905, 'AI Generate', 60900, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:content:ai-generate', '#', 'admin', NOW(), 'AI Generate Content'),
(60906, 'Approve AI Content', 60900, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:content:ai-approve', '#', 'admin', NOW(), 'Approve AI Content')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 11: ADVERTISING (ID: 61000)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (61000, 'Advertising', 60000, 10, 'ads', NULL, 1, 0, 'C', '0', '0', 'marketing:ads:campaigns', 'television', 'admin', NOW(), 'Multi-Platform Advertising')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(61001, 'Manage Ad Campaigns', 61000, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ads:campaigns', '#', 'admin', NOW(), 'Manage Ad Campaigns'),
(61002, 'Add Ad Campaign', 61000, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ads:campaigns:add', '#', 'admin', NOW(), 'Add Ad Campaign'),
(61003, 'Edit Ad Campaign', 61000, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ads:campaigns:edit', '#', 'admin', NOW(), 'Edit Ad Campaign'),
(61004, 'Delete Ad Campaign', 61000, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ads:campaigns:delete', '#', 'admin', NOW(), 'Delete Ad Campaign'),
(61005, 'Manage Creatives', 61000, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ads:creatives', '#', 'admin', NOW(), 'Manage Ad Creatives'),
(61006, 'View Performance', 61000, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ads:performance', '#', 'admin', NOW(), 'View Ad Performance'),
(61007, 'View Analytics', 61000, 7, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ads:analytics', '#', 'admin', NOW(), 'View Ad Analytics'),
(61008, 'Export Reports', 61000, 8, '', NULL, 1, 0, 'F', '0', '0', 'marketing:ads:export', '#', 'admin', NOW(), 'Export Ad Reports')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 12: PUBLIC RELATIONS (ID: 61100)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (61100, 'Public Relations', 60000, 11, 'public-relations', NULL, 1, 0, 'C', '0', '0', 'marketing:pr:releases', 'newspaper', 'admin', NOW(), 'PR & Media Relations')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(61101, 'Manage Press Releases', 61100, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:pr:releases', '#', 'admin', NOW(), 'Manage Press Releases'),
(61102, 'Add Press Release', 61100, 2, '', NULL, 1, 0, 'F', '0', '0', 'marketing:pr:releases:add', '#', 'admin', NOW(), 'Add Press Release'),
(61103, 'Distribute Release', 61100, 3, '', NULL, 1, 0, 'F', '0', '0', 'marketing:pr:releases:distribute', '#', 'admin', NOW(), 'Distribute Press Release'),
(61104, 'Approve Release', 61100, 4, '', NULL, 1, 0, 'F', '0', '0', 'marketing:pr:releases:approve', '#', 'admin', NOW(), 'Approve Press Release'),
(61105, 'Manage Contacts', 61100, 5, '', NULL, 1, 0, 'F', '0', '0', 'marketing:pr:contacts', '#', 'admin', NOW(), 'Manage Media Contacts'),
(61106, 'Manage Mentions', 61100, 6, '', NULL, 1, 0, 'F', '0', '0', 'marketing:pr:mentions', '#', 'admin', NOW(), 'Manage Media Mentions'),
(61107, 'View Sentiment', 61100, 7, '', NULL, 1, 0, 'F', '0', '0', 'marketing:pr:sentiment', '#', 'admin', NOW(), 'View PR Sentiment'),
(61108, 'View Analytics', 61100, 8, '', NULL, 1, 0, 'F', '0', '0', 'marketing:pr:analytics', '#', 'admin', NOW(), 'View PR Analytics')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 13: ANALYTICS (ID: 61200)
-- ====================================================

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES (61200, 'Marketing Analytics', 60000, 12, 'analytics', NULL, 1, 0, 'C', '0', '0', 'marketing:analytics:view', 'chart-line', 'admin', NOW(), 'Marketing Analytics & Reports')
ON CONFLICT ("menuId") DO NOTHING;

INSERT INTO sys_menu ("menuId", "menuName", "parentId", "orderNum", "path", "component", "isFrame", "isCache", "menuType", "visible", "status", "perms", "icon", "create_by", "create_time", "remark")
VALUES
(61201, 'View Analytics', 61200, 1, '', NULL, 1, 0, 'F', '0', '0', 'marketing:analytics:view', '#', 'admin', NOW(), 'View Marketing Analytics')
ON CONFLICT ("menuId") DO NOTHING;

-- ====================================================
-- SECTION 14: ASSIGN ALL PERMISSIONS TO ADMIN ROLE (role_id = 1)
-- ====================================================

INSERT INTO sys_role_menu ("roleId", "menuId")
SELECT 1, m."menuId"
FROM sys_menu m
WHERE m."menuId" BETWEEN 60000 AND 61299
  AND NOT EXISTS (
    SELECT 1 FROM sys_role_menu rm WHERE rm."roleId" = 1 AND rm."menuId" = m."menuId"
  );

-- ====================================================
-- DONE! Total: 1 parent + 12 pages + ~70 button permissions
-- ====================================================
