-- Dictionary Query for sys_role
-- Class: SysRole
-- DictType: sql_sys_role

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_role' AS dictType
FROM sys_role
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
