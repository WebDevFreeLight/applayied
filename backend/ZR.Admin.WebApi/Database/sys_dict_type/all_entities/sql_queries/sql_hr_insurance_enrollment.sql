-- Dictionary Query for hr_insurance_enrollment
-- Class: HrInsuranceEnrollment
-- DictType: sql_hr_insurance_enrollment

SELECT
    id AS dictValue,
    id::text AS dictLabel,
    remark,
    'sql_hr_insurance_enrollment' AS dictType
FROM hr_insurance_enrollment
WHERE isactive = '1' OR status = '0'
ORDER BY id;
