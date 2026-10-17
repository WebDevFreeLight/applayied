-- Dictionary Query for blv_customerregistrationform
-- Class: BlvCustomerregistrationform
-- DictType: sql_blv_customerregistrationform

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_blv_customerregistrationform' AS dictType
FROM blv_customerregistrationform
WHERE isactive = '1' OR status = '0'
ORDER BY id;
