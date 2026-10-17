-- Dictionary Query for ai_ToolType
-- Class: ToolType
-- DictType: sql_ai_ToolType

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_ai_ToolType' AS dictType
FROM ai_ToolType
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
