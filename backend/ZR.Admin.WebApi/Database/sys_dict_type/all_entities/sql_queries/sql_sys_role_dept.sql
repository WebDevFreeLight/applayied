-- Dictionary Query for sys_role_dept
-- Class: SysRoleDept
-- DictType: sql_sys_role_dept

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_sys_role_dept' AS dictType
FROM sys_role_dept
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
