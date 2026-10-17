-- Dictionary Query for event_booth_types
-- Class: BoothType
-- DictType: sql_event_booth_types

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_booth_types' AS dictType
FROM event_booth_types
WHERE isactive = '1' OR status = '0'
ORDER BY id;
