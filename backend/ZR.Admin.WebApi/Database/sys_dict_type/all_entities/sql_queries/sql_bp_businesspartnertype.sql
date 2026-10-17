-- Dictionary Query for bp_businesspartnertype
-- Class: BpBusinesspartnertype
-- DictType: sql_bp_businesspartnertype

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_bp_businesspartnertype' AS dictType
FROM bp_businesspartnertype
WHERE isactive = '1' OR status = '0'
ORDER BY id;
