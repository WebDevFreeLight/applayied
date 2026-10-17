-- Dictionary Query for event_gate_types
-- Class: EventGateType
-- DictType: sql_event_gate_types

SELECT
    id AS dictValue,
    CONCAT(, ' - ', typename, ' - ', typenamear) AS dictLabel,
    remark,
    'sql_event_gate_types' AS dictType
FROM event_gate_types
WHERE isactive = '1' OR status = '0'
ORDER BY id;
