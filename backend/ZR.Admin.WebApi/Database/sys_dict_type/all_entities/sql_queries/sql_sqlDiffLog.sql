-- Dictionary Query for sqlDiffLog
-- Class: SqlDiffLog
-- DictType: sql_sqlDiffLog

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sqlDiffLog' AS dictType
FROM sqlDiffLog
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
