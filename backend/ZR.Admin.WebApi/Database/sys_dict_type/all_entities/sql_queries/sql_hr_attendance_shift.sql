-- Dictionary Query for hr_attendance_shift
-- Class: HrAttendanceShift
-- DictType: sql_hr_attendance_shift

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_hr_attendance_shift' AS dictType
FROM hr_attendance_shift
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
