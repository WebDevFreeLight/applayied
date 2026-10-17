-- Dictionary Query for hr_employee
-- Class: HrEmployee
-- DictType: sql_hr_employee

SELECT
    employeeid AS dictValue,
    CONCAT(firstname, ' ', lastname, ' ', namear) AS dictLabel,
    remark,
    'sql_hr_employee' AS dictType
FROM hr_employee
WHERE isactive = '1' OR status = '0'
ORDER BY employeeid;
