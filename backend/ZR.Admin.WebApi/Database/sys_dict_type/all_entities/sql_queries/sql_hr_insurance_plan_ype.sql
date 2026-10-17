-- Dictionary Query for hr_insurance_plan_ype
-- Class: HrInsurancePlanYpe
-- DictType: sql_hr_insurance_plan_ype

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_insurance_plan_ype' AS dictType
FROM hr_insurance_plan_ype
WHERE isactive = '1' OR status = '0'
ORDER BY id;
