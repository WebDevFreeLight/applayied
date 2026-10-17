-- Dictionary Query for hr_performance_review
-- Class: HrPerformanceReview
-- DictType: sql_hr_performance_review

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_hr_performance_review' AS dictType
FROM hr_performance_review
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
