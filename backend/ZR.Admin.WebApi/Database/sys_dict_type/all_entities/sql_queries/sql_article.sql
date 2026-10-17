-- Dictionary Query for article
-- Class: Article
-- DictType: sql_article

SELECT
    article AS dictValue,
     AS dictLabel,
    remark,
    'sql_article' AS dictType
FROM article
WHERE isactive = '1' OR status = '0'
ORDER BY article;
