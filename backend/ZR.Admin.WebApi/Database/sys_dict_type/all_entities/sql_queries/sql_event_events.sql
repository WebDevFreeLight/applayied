-- Dictionary Query for event_events
-- Class: Event
-- DictType: sql_event_events

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_event_events' AS dictType
FROM event_events
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
