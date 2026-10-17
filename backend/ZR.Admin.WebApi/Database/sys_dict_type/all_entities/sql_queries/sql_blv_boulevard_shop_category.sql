-- Dictionary Query for blv_boulevard_shop_category
-- Class: BlvBoulevardShopCategory
-- DictType: sql_blv_boulevard_shop_category

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_blv_boulevard_shop_category' AS dictType
FROM blv_boulevard_shop_category
WHERE isactive = '1' OR status = '0'
ORDER BY id;
