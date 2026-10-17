-- Dictionary Query for event_contracts
-- Class: EventContract
-- DictType: sql_event_contracts

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_event_contracts' AS dictType
FROM event_contracts
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
