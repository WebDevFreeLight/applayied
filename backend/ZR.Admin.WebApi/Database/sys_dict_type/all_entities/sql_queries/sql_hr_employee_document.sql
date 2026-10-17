-- Dictionary Query for hr_employee_document
-- Class: HrEmployeeDocument
-- DictType: sql_hr_employee_document

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_employee_document' AS dictType
FROM hr_employee_document
WHERE isactive = '1' OR status = '0'
ORDER BY id;
