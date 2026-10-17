-- Dictionary Query for sys_post
-- Class: SysPost
-- DictType: sql_sys_post

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_post' AS dictType
FROM sys_post
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
