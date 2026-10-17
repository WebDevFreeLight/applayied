-- Dictionary Query for sys_file
-- Class: SysFile
-- DictType: sql_sys_file

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_file' AS dictType
FROM sys_file
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
