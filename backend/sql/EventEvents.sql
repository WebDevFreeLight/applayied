use maham-ai;

-- Event Information - معلومات الفعالية菜单
INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by, create_time) 
VALUES ('Event Information - معلومات الفعالية', 1979887952561577984, 999, 'EventEvents', 'eventManagement/EventEvents', 0, 0, 'C', '0', '0', 'EventManagement:eventevents:list', 'icon1', 'system', null);

-- 按钮父菜单id
DO $$
	DECLARE @menuId integer;
	DECLARE @nowTime datetime;
BEGIN
	SELECT @@identity INTO @menuId;
	SELECT now()::timestamp(0)without time zone INTO @nowTime

	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
	VALUES ('查询', @menuId, 1, '#', NULL, 0, 0, 'F', '0', '0', 'EventManagement:eventevents:query', '', 'system', @nowTime);

	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
	VALUES ('新增', @menuId, 2, '#', NULL, 0, 0, 'F', '0', '0', 'EventManagement:eventevents:add', '', 'system', @nowTime);

	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
	VALUES ('删除', @menuId, 3, '#', NULL, 0, 0, 'F', '0', '0', 'EventManagement:eventevents:delete', '', 'system', @nowTime);

	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
	VALUES ('修改', @menuId, 4, '#', NULL, 0, 0, 'F', '0', '0', 'EventManagement:eventevents:edit', '', 'system', @nowTime);
	   
   	INSERT INTO sys_menu(menuName, parentId, orderNum, path, component, isFrame, isCache, menuType, visible, status, perms, icon, create_by,create_time) 
   	VALUES ('导出', @menuId, 5, '#', NULL, 0, 0, 'F', '0', '0', 'EventManagement:eventevents:export', '', 'system', @nowTime);

END $$;

SELECT * FROM sys_menu WHERE parentId = @menuId;
SELECT * FROM sys_menu WHERE menuId = @menuId;
