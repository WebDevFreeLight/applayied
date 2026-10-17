-- Dictionary Query for hr_hybrid_schedule
-- Class: HrHybridSchedule
-- DictType: sql_hr_hybrid_schedule

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_hr_hybrid_schedule' AS dictType
FROM hr_hybrid_schedule
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
