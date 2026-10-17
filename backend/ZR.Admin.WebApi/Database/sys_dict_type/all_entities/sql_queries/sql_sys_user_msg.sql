-- Dictionary Query for sys_user_msg
-- Class: SysUserMsg
-- DictType: sql_sys_user_msg

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_sys_user_msg' AS dictType
FROM sys_user_msg
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
