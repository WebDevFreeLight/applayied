-- Dictionary Query for hr_workflow
-- Class: HrWorkflow
-- DictType: sql_hr_workflow

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_workflow' AS dictType
FROM hr_workflow
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
