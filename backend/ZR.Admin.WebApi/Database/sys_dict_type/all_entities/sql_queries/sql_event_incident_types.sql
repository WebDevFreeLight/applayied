-- Dictionary Query for event_incident_types
-- Class: IncidentType
-- DictType: sql_event_incident_types

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_incident_types' AS dictType
FROM event_incident_types
WHERE isactive = '1' OR status = '0'
ORDER BY id;
