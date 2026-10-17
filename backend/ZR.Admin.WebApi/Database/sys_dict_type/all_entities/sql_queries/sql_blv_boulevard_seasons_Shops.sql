-- Dictionary Query for blv_boulevard_seasons_shops
-- Class: BlvBoulevardSeasonsShops
-- DictType: sql_blv_boulevard_seasons_shops

SELECT
    id AS dictValue,
    id::text AS dictLabel,
    remark,
    'sql_blv_boulevard_seasons_shops' AS dictType
FROM blv_boulevard_seasons_shops
WHERE isactive = '1' OR status = '0'
ORDER BY id;
