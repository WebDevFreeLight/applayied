-- Dictionary Query for hr_analytics_report
-- Class: HrAnalyticsReport
-- DictType: sql_hr_analytics_report

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_analytics_report' AS dictType
FROM hr_analytics_report
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
