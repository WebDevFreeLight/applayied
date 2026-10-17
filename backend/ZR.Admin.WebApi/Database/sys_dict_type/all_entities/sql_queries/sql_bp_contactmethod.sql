-- Dictionary Query for bp_contactmethod
-- Class: BpContactmethod
-- DictType: sql_bp_contactmethod

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_bp_contactmethod' AS dictType
FROM bp_contactmethod
WHERE isactive = '1' OR status = '0'
ORDER BY id;
