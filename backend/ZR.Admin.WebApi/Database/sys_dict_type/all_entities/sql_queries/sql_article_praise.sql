-- Dictionary Query for article_praise
-- Class: ArticlePraise
-- DictType: sql_article_praise

SELECT
    articlepraise AS dictValue,
    articlepraise::text AS dictLabel,
    remark,
    'sql_article_praise' AS dictType
FROM article_praise
WHERE isactive = '1' OR status = '0'
ORDER BY articlepraise;
