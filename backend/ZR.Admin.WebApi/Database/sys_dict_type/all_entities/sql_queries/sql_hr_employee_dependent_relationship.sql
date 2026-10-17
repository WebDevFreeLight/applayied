-- Dictionary Query for hr_employee_dependent_relationship
-- Class: HrEmployeeDependentRelationship
-- DictType: sql_hr_employee_dependent_relationship

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_employee_dependent_relationship' AS dictType
FROM hr_employee_dependent_relationship
WHERE isactive = '1' OR status = '0'
ORDER BY id;
