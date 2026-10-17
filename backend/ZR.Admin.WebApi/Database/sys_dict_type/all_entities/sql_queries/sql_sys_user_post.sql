-- Dictionary Query for sys_user_post
-- Class: SysUserPost
-- DictType: sql_sys_user_post

SELECT
    id AS dictValue,
    id::text AS dictLabel,
    remark,
    'sql_sys_user_post' AS dictType
FROM sys_user_post
WHERE isactive = '1' OR status = '0'
ORDER BY id;
