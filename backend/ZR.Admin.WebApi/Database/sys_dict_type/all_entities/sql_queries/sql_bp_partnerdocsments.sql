-- Dictionary Query for bp_partnerdocsments
-- Class: BpPartnerdocsments
-- DictType: sql_bp_partnerdocsments

SELECT
    id AS dictValue,
    id::text AS dictLabel,
    remark,
    'sql_bp_partnerdocsments' AS dictType
FROM bp_partnerdocsments
WHERE isactive = '1' OR status = '0'
ORDER BY id;
