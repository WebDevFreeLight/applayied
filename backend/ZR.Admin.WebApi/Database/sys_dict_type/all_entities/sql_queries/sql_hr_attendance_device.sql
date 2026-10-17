-- Dictionary Query for hr_attendance_device
-- Class: HrAttendanceDevice
-- DictType: sql_hr_attendance_device

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_attendance_device' AS dictType
FROM hr_attendance_device
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
