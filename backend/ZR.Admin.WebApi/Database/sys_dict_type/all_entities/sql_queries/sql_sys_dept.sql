-- Dictionary Query for sys_dept
-- Class: SysDept
-- DictType: sql_sys_dept

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_dept' AS dictType
FROM sys_dept
WHERE isactive = '1' OR status = '0'
ORDER BY id;
