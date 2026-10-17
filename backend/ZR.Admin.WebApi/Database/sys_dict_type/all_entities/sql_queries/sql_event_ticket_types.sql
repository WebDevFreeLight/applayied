-- Dictionary Query for event_ticket_types
-- Class: TicketType
-- DictType: sql_event_ticket_types

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_ticket_types' AS dictType
FROM event_ticket_types
WHERE isactive = '1' OR status = '0'
ORDER BY id;
