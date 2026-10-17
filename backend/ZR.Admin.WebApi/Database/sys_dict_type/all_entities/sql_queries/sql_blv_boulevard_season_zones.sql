-- Dictionary Query for blv_boulevard_season_zones
-- Class: BlvBoulevardSeasonZones
-- DictType: sql_blv_boulevard_season_zones

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_blv_boulevard_season_zones' AS dictType
FROM blv_boulevard_season_zones
WHERE isactive = '1' OR status = '0'
ORDER BY id;
