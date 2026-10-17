-- Dictionary Query for event_permits
-- Class: EventPermit
-- DictType: sql_event_permits

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_event_permits' AS dictType
FROM event_permits
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
