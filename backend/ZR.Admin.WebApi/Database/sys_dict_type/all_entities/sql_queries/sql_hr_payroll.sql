-- Dictionary Query for hr_payroll
-- Class: HrPayroll
-- DictType: sql_hr_payroll

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_payroll' AS dictType
FROM hr_payroll
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
