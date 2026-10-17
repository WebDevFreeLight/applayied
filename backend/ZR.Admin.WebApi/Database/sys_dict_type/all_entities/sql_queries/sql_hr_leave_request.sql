-- Dictionary Query for hr_leave_request
-- Class: HrLeaveRequest
-- DictType: sql_hr_leave_request

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_hr_leave_request' AS dictType
FROM hr_leave_request
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
