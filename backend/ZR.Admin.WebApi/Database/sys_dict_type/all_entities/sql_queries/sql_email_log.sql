-- Dictionary Query for email_log
-- Class: EmailLog
-- DictType: sql_email_log

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_email_log' AS dictType
FROM email_log
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
