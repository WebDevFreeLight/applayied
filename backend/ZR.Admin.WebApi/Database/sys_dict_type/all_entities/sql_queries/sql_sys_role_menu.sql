-- Dictionary Query for sys_role_menu
-- Class: SysRoleMenu
-- DictType: sql_sys_role_menu

SELECT
    id AS dictValue,
    id::text AS dictLabel,
    remark,
    'sql_sys_role_menu' AS dictType
FROM sys_role_menu
WHERE isactive = '1' OR status = '0'
ORDER BY id;
