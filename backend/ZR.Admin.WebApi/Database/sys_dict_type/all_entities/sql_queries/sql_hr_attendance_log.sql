-- Dictionary Query for hr_attendance_log
-- Class: HrAttendanceLog
-- DictType: sql_hr_attendance_log

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_attendance_log' AS dictType
FROM hr_attendance_log
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
