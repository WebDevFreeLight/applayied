-- Dictionary Query for hr_benefit_plan
-- Class: HrBenefitPlan
-- DictType: sql_hr_benefit_plan

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_hr_benefit_plan' AS dictType
FROM hr_benefit_plan
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
