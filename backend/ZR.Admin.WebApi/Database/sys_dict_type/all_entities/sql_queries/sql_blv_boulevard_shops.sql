-- Dictionary Query for blv_boulevard_shops
-- Class: BlvBoulevardShops
-- DictType: sql_blv_boulevard_shops

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_blv_boulevard_shops' AS dictType
FROM blv_boulevard_shops
WHERE isactive = '1' OR status = '0'
ORDER BY id;
