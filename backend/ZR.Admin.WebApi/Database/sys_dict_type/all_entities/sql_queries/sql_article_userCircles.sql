-- Dictionary Query for article_userCircles
-- Class: ArticleUserCircles
-- DictType: sql_article_userCircles

SELECT
    articleusercircles AS dictValue,
    articleusercircles::text AS dictLabel,
    remark,
    'sql_article_userCircles' AS dictType
FROM article_userCircles
WHERE isactive = '1' OR status = '0'
ORDER BY articleusercircles;
