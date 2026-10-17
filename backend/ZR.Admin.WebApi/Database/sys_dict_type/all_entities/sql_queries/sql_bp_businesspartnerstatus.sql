-- Dictionary Query for bp_businesspartnerstatus
-- Class: BpBusinesspartnerstatus
-- DictType: sql_bp_businesspartnerstatus

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_bp_businesspartnerstatus' AS dictType
FROM bp_businesspartnerstatus
WHERE isactive = '1' OR status = '0'
ORDER BY id;
