-- Dictionary Query for smsCode_log
-- Class: SmsCodeLog
-- DictType: sql_smsCode_log

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_smsCode_log' AS dictType
FROM smsCode_log
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
