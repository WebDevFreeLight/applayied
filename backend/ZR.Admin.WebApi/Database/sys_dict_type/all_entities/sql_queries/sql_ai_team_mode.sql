-- Dictionary Query for ai_team_mode
-- Class: AiTeamMode
-- DictType: sql_ai_team_mode

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_ai_team_mode' AS dictType
FROM ai_team_mode
WHERE isactive = '1' OR status = '0'
ORDER BY id;
