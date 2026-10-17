-- Dictionary Query for hr_insurance_enrollment_status
-- Class: HrInsuranceEnrollmentStatus
-- DictType: sql_hr_insurance_enrollment_status

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_insurance_enrollment_status' AS dictType
FROM hr_insurance_enrollment_status
WHERE isactive = '1' OR status = '0'
ORDER BY id;
