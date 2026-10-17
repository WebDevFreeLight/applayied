-- Dictionary Query for hr_kpi
-- Class: HrKpi
-- DictType: sql_hr_kpi

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_hr_kpi' AS dictType
FROM hr_kpi
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
