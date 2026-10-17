-- Dictionary Query for hr_documenttype
-- Class: HrDocumenttype
-- DictType: sql_hr_documenttype

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_documenttype' AS dictType
FROM hr_documenttype
WHERE isactive = '1' OR status = '0'
ORDER BY id;
