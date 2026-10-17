-- Dictionary Query for event_locations
-- Class: EventLocation
-- DictType: sql_event_locations

SELECT
    locationid AS dictValue,
    CONCAT(locationid, ' - ', locationname, ' - ', city) AS dictLabel,
    remark,
    'sql_event_locations' AS dictType
FROM event_locations
WHERE isactive = '1' OR status = '0'
ORDER BY locationid;
