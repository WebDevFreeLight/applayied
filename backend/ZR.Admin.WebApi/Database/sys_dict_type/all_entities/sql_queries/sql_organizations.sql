-- Dictionary Query for organizations
-- Class: Organizations
-- DictType: sql_organizations

SELECT
    organizations:sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_organizations' AS dictType
FROM organizations
WHERE isactive = '1' OR status = '0'
ORDER BY organizations:sysbase;
