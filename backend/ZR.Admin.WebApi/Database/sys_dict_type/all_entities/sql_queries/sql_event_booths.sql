-- Dictionary Query for event_booths
-- Class: EventBooth
-- DictType: sql_event_booths

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_event_booths' AS dictType
FROM event_booths
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
