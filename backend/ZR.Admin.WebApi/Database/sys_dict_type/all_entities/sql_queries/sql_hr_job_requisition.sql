-- Dictionary Query for hr_job_requisition
-- Class: HrJobRequisition
-- DictType: sql_hr_job_requisition

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_job_requisition' AS dictType
FROM hr_job_requisition
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
