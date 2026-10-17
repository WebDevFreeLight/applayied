-- Dictionary Query for event_visitor_types
-- Class: VisitorType
-- DictType: sql_event_visitor_types

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_visitor_types' AS dictType
FROM event_visitor_types
WHERE isactive = '1' OR status = '0'
ORDER BY id;
