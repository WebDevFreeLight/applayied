use maham-ai;

-- Bp_PartnerDocsments菜单
INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time) 
VALUES ('Bp_PartnerDocsments', 1966153658936725504, 999, 'BpPartnerdocsments', 'partnertype/BpPartnerdocsments', 0, 0, 'C', '0', '0', 'Partnertype:bppartnerdocsments:list', 'icon1', 'system', null);

-- 按钮父菜单id
DO $$
	DECLARE @menuId integer;
	DECLARE @nowTime datetime;
BEGIN
	SELECT @@identity INTO @menuId;
	SELECT now()::timestamp(0)without time zone INTO @nowTime

	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
	VALUES ('查询', @menuId, 1, '#', NULL, 0, 0, 'F', '0', '0', 'Partnertype:bppartnerdocsments:query', '', 'system', @nowTime);

	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
	VALUES ('新增', @menuId, 2, '#', NULL, 0, 0, 'F', '0', '0', 'Partnertype:bppartnerdocsments:add', '', 'system', @nowTime);

	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
	VALUES ('删除', @menuId, 3, '#', NULL, 0, 0, 'F', '0', '0', 'Partnertype:bppartnerdocsments:delete', '', 'system', @nowTime);

	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
	VALUES ('修改', @menuId, 4, '#', NULL, 0, 0, 'F', '0', '0', 'Partnertype:bppartnerdocsments:edit', '', 'system', @nowTime);
	   
   	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
   	VALUES ('导出', @menuId, 5, '#', NULL, 0, 0, 'F', '0', '0', 'Partnertype:bppartnerdocsments:export', '', 'system', @nowTime);

END $$;

SELECT * FROM sys_menu WHERE parentId = @menuId;
SELECT * FROM sys_menu WHERE menuId = @menuId;
