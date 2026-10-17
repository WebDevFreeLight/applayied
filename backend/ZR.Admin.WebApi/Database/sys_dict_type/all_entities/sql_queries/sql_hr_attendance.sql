-- Dictionary Query for hr_attendance
-- Class: HrAttendance
-- DictType: sql_hr_attendance

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_hr_attendance' AS dictType
FROM hr_attendance
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
