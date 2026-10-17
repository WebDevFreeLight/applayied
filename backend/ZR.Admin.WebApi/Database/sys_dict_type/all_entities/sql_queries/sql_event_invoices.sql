-- Dictionary Query for event_invoices
-- Class: EventInvoice
-- DictType: sql_event_invoices

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_event_invoices' AS dictType
FROM event_invoices
WHERE isactive = '1' OR status = '0'
ORDER BY id;
