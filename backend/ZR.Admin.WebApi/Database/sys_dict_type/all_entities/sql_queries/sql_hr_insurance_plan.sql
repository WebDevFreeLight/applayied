-- Dictionary Query for hr_insurance_plan
-- Class: HrInsurancePlan
-- DictType: sql_hr_insurance_plan

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_insurance_plan' AS dictType
FROM hr_insurance_plan
WHERE isactive = '1' OR status = '0'
ORDER BY id;
