-- Dictionary Query for social_fans_info
-- Class: SocialFansInfo
-- DictType: sql_social_fans_info

SELECT
    socialfansinfo AS dictValue,
    socialfansinfo::text AS dictLabel,
    remark,
    'sql_social_fans_info' AS dictType
FROM social_fans_info
WHERE isactive = '1' OR status = '0'
ORDER BY socialfansinfo;
