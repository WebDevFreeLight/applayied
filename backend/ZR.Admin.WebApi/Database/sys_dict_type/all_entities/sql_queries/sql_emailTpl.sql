-- Dictionary Query for emailTpl
-- Class: EmailTpl
-- DictType: sql_emailTpl

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_emailTpl' AS dictType
FROM emailTpl
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
