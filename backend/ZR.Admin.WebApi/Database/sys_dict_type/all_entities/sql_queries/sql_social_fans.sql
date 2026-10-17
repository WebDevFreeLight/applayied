-- Dictionary Query for social_fans
-- Class: SocialFans
-- DictType: sql_social_fans

SELECT
    socialfans AS dictValue,
     AS dictLabel,
    remark,
    'sql_social_fans' AS dictType
FROM social_fans
WHERE isactive = '1' OR status = '0'
ORDER BY socialfans;
