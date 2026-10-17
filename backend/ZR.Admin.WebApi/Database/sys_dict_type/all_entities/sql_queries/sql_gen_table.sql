-- Dictionary Query for gen_table
-- Class: GenTable
-- DictType: sql_gen_table

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_gen_table' AS dictType
FROM gen_table
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
