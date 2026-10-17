-- Dictionary Query for hr_insurance_coveragelevel
-- Class: HrInsuranceCoveragelevel
-- DictType: sql_hr_insurance_coveragelevel

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_insurance_coveragelevel' AS dictType
FROM hr_insurance_coveragelevel
WHERE isactive = '1' OR status = '0'
ORDER BY id;
