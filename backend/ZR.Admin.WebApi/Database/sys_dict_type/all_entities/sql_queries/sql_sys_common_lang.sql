-- Dictionary Query for sys_common_lang
-- Class: CommonLang
-- DictType: sql_sys_common_lang

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_common_lang' AS dictType
FROM sys_common_lang
WHERE isactive = '1' OR status = '0'
ORDER BY id;
