-- Dictionary Query for event_tickets
-- Class: EventTicket
-- DictType: sql_event_tickets

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_event_tickets' AS dictType
FROM event_tickets
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
