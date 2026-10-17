-- Dictionary Query for gen_table_column
-- Class: GenTableColumn
-- DictType: sql_gen_table_column

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_gen_table_column' AS dictType
FROM gen_table_column
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
