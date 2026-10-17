-- Dictionary Query for hr_notification
-- Class: HrNotification
-- DictType: sql_hr_notification

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_hr_notification' AS dictType
FROM hr_notification
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
