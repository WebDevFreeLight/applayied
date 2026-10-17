-- Dictionary Query for sys_menu
-- Class: SysMenu
-- DictType: sql_sys_menu

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_menu' AS dictType
FROM sys_menu
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
