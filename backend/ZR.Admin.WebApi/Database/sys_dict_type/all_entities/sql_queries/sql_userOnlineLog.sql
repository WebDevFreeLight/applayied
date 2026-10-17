-- Dictionary Query for userOnlineLog
-- Class: UserOnlineLog
-- DictType: sql_userOnlineLog

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_userOnlineLog' AS dictType
FROM userOnlineLog
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
