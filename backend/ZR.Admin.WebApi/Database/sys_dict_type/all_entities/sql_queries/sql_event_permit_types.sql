-- Dictionary Query for event_permit_types
-- Class: PermitType
-- DictType: sql_event_permit_types

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_permit_types' AS dictType
FROM event_permit_types
WHERE isactive = '1' OR status = '0'
ORDER BY id;
