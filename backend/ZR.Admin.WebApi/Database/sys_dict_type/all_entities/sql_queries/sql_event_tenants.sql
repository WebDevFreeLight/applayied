-- Dictionary Query for event_tenants
-- Class: EventTenant
-- DictType: sql_event_tenants

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_event_tenants' AS dictType
FROM event_tenants
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
