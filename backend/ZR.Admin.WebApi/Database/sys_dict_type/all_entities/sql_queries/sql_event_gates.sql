-- Dictionary Query for event_gates
-- Class: EventGate
-- DictType: sql_event_gates

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_event_gates' AS dictType
FROM event_gates
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
