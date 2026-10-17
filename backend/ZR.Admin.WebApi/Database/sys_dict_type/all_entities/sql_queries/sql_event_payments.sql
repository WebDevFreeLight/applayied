-- Dictionary Query for event_payments
-- Class: EventPayment
-- DictType: sql_event_payments

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_event_payments' AS dictType
FROM event_payments
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
