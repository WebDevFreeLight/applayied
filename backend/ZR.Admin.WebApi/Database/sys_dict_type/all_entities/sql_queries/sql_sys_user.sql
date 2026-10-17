-- Dictionary Query for sys_user
-- Class: SysUser
-- DictType: sql_sys_user

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_user' AS dictType
FROM sys_user
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
