-- Dictionary Query for hr_recruitment
-- Class: HrRecruitment
-- DictType: sql_hr_recruitment

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_recruitment' AS dictType
FROM hr_recruitment
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
