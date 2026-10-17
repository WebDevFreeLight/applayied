-- Dictionary Query for sys_tasks
-- Class: SysTasks
-- DictType: sql_sys_tasks

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_tasks' AS dictType
FROM sys_tasks
WHERE isactive = '1' OR status = '0'
ORDER BY id;
