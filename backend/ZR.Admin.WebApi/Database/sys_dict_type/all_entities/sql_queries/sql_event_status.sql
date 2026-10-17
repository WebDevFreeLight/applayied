-- Dictionary Query for event_status
-- Class: EventStatus
-- DictType: sql_event_status

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_status' AS dictType
FROM event_status
WHERE isactive = '1' OR status = '0'
ORDER BY id;
