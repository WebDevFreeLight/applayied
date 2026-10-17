-- Dictionary Query for bp_businesspartner
-- Class: BpBusinesspartner
-- DictType: sql_bp_businesspartner

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_bp_businesspartner' AS dictType
FROM bp_businesspartner
WHERE isactive = '1' OR status = '0'
ORDER BY id;
