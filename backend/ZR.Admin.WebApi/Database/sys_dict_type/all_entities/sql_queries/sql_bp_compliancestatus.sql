-- Dictionary Query for bp_compliancestatus
-- Class: BpCompliancestatus
-- DictType: sql_bp_compliancestatus

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_bp_compliancestatus' AS dictType
FROM bp_compliancestatus
WHERE isactive = '1' OR status = '0'
ORDER BY id;
