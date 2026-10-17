-- Dictionary Query for event_types
-- Class: EventType
-- DictType: sql_event_types

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_types' AS dictType
FROM event_types
WHERE isactive = '1' OR status = '0'
ORDER BY id;
