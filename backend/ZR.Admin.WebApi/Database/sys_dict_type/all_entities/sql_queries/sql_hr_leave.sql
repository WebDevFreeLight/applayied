-- Dictionary Query for hr_leave
-- Class: HrLeave
-- DictType: sql_hr_leave

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_hr_leave' AS dictType
FROM hr_leave
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
