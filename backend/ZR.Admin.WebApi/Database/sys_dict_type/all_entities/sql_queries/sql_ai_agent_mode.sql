-- Dictionary Query for ai_agent_mode
-- Class: AiAgentMode
-- DictType: sql_ai_agent_mode

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_ai_agent_mode' AS dictType
FROM ai_agent_mode
WHERE isactive = '1' OR status = '0'
ORDER BY id;
