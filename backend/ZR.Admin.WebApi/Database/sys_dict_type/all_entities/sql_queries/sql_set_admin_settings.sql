-- Dictionary Query for set_admin_settings
-- Class: admin_settings
-- DictType: sql_set_admin_settings

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_set_admin_settings' AS dictType
FROM set_admin_settings
WHERE isactive = '1' OR status = '0'
ORDER BY id;
