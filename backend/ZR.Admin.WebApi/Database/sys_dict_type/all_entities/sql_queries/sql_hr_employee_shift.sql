-- Dictionary Query for hr_employee_shift
-- Class: HrEmployeeShift
-- DictType: sql_hr_employee_shift

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_hr_employee_shift' AS dictType
FROM hr_employee_shift
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
