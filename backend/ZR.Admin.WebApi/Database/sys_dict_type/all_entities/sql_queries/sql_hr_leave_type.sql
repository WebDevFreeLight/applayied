-- Dictionary Query for hr_leave_type
-- Class: HrLeaveType
-- DictType: sql_hr_leave_type

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_hr_leave_type' AS dictType
FROM hr_leave_type
WHERE isactive = '1' OR status = '0'
ORDER BY id;
