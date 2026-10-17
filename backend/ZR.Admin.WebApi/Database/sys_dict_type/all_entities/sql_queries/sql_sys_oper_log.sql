-- Dictionary Query for sys_oper_log
-- Class: SysOperLog
-- DictType: sql_sys_oper_log

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_oper_log' AS dictType
FROM sys_oper_log
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
