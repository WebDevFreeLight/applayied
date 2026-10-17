-- Dictionary Query for hr_document
-- Class: HrDocument
-- DictType: sql_hr_document

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_document' AS dictType
FROM hr_document
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
