-- Dictionary Query for hr_skills_matrix
-- Class: HrSkillsMatrix
-- DictType: sql_hr_skills_matrix

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_skills_matrix' AS dictType
FROM hr_skills_matrix
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
