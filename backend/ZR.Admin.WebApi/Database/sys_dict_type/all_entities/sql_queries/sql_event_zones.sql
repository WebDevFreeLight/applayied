-- Dictionary Query for event_zones
-- Class: EventZone
-- DictType: sql_event_zones

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_event_zones' AS dictType
FROM event_zones
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
