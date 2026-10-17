-- Dictionary Query for ai_modelprovider
-- Class: AiModelprovider
-- DictType: sql_ai_modelprovider

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_ai_modelprovider' AS dictType
FROM ai_modelprovider
WHERE isactive = '1' OR status = '0'
ORDER BY id;
