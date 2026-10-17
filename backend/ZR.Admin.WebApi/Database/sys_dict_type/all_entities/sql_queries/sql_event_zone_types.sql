-- Dictionary Query for event_zone_types
-- Class: EventZoneType
-- DictType: sql_event_zone_types

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_zone_types' AS dictType
FROM event_zone_types
WHERE isactive = '1' OR status = '0'
ORDER BY id;
