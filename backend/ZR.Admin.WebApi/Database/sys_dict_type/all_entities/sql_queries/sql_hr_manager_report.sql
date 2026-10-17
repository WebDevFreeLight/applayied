-- Dictionary Query for hr_manager_report
-- Class: HrManagerDashboard
-- DictType: sql_hr_manager_report

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_manager_report' AS dictType
FROM hr_manager_report
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
