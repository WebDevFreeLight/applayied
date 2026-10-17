-- Dictionary Query for sys_tasks_log
-- Class: SysTasksLog
-- DictType: sql_sys_tasks_log

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_tasks_log' AS dictType
FROM sys_tasks_log
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
