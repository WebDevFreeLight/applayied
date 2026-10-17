-- Dictionary Query for hr_leave_balance
-- Class: HrLeaveBalance
-- DictType: sql_hr_leave_balance

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_hr_leave_balance' AS dictType
FROM hr_leave_balance
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
