-- Dictionary Query for event_visitors
-- Class: EventVisitor
-- DictType: sql_event_visitors

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_event_visitors' AS dictType
FROM event_visitors
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
