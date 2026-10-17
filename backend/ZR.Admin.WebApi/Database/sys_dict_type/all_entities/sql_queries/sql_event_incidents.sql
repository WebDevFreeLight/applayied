-- Dictionary Query for event_incidents
-- Class: EventIncident
-- DictType: sql_event_incidents

SELECT
    id AS dictValue,
    id::text AS dictLabel,
    remark,
    'sql_event_incidents' AS dictType
FROM event_incidents
WHERE isactive = '1' OR status = '0'
ORDER BY id;
