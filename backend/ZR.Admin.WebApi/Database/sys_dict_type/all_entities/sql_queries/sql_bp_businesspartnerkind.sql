-- Dictionary Query for bp_businesspartnerkind
-- Class: BpBusinesspartnerkind
-- DictType: sql_bp_businesspartnerkind

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_bp_businesspartnerkind' AS dictType
FROM bp_businesspartnerkind
WHERE isactive = '1' OR status = '0'
ORDER BY id;
