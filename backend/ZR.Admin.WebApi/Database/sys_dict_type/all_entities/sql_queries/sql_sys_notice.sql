-- Dictionary Query for sys_notice
-- Class: SysNotice
-- DictType: sql_sys_notice

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_sys_notice' AS dictType
FROM sys_notice
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
