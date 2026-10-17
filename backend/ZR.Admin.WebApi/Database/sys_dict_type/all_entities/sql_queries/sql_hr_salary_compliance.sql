-- Dictionary Query for hr_salary_compliance
-- Class: HrSalaryCompliance
-- DictType: sql_hr_salary_compliance

SELECT
    id AS dictValue,
    id::text AS dictLabel,
    remark,
    'sql_hr_salary_compliance' AS dictType
FROM hr_salary_compliance
WHERE isactive = '1' OR status = '0'
ORDER BY id;
