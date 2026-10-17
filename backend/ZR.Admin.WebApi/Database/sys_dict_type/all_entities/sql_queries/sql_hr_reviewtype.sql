-- Dictionary Query for hr_reviewtype
-- Class: HrReviewtype
-- DictType: sql_hr_reviewtype

SELECT
    businesspartner:sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_reviewtype' AS dictType
FROM hr_reviewtype
WHERE isactive = '1' OR status = '0'
ORDER BY businesspartner:sysbase;
