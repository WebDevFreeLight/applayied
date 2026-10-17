-- Dictionary Query for banner_config
-- Class: BannerConfig
-- DictType: sql_banner_config

SELECT
    bannerconfig AS dictValue,
    bannerconfig::text AS dictLabel,
    remark,
    'sql_banner_config' AS dictType
FROM banner_config
WHERE isactive = '1' OR status = '0'
ORDER BY bannerconfig;
