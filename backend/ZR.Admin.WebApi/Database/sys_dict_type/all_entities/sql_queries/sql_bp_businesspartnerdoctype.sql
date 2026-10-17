-- Dictionary Query for bp_businesspartnerdoctype
-- Class: BpBusinesspartnerdoctype
-- DictType: sql_bp_businesspartnerdoctype

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_bp_businesspartnerdoctype' AS dictType
FROM bp_businesspartnerdoctype
WHERE isactive = '1' OR status = '0'
ORDER BY id;
