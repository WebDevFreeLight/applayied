-- Dictionary Query for sys_user_role
-- Class: SysUserRole
-- DictType: sql_sys_user_role

SELECT
    id AS dictValue,
    id::text AS dictLabel,
    remark,
    'sql_sys_user_role' AS dictType
FROM sys_user_role
WHERE isactive = '1' OR status = '0'
ORDER BY id;
