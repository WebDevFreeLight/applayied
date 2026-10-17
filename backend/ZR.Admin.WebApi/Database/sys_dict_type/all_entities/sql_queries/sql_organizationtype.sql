-- Dictionary Query for organizationtype
-- Class: Organizationtype
-- DictType: sql_organizationtype

SELECT
    organizationtype:sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_organizationtype' AS dictType
FROM organizationtype
WHERE isactive = '1' OR status = '0'
ORDER BY organizationtype:sysbase;
