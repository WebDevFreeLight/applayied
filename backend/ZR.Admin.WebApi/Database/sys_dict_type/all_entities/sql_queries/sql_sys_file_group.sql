-- Dictionary Query for sys_file_group
-- Class: SysFileGroup
-- DictType: sql_sys_file_group

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_file_group' AS dictType
FROM sys_file_group
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
