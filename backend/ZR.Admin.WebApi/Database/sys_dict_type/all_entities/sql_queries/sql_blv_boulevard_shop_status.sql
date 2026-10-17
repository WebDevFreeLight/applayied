-- Dictionary Query for blv_boulevard_shop_status
-- Class: BlvBoulevardShopStatus
-- DictType: sql_blv_boulevard_shop_status

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_blv_boulevard_shop_status' AS dictType
FROM blv_boulevard_shop_status
WHERE isactive = '1' OR status = '0'
ORDER BY id;
