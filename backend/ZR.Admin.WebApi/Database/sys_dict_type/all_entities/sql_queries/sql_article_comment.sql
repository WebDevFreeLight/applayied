-- Dictionary Query for article_comment
-- Class: ArticleComment
-- DictType: sql_article_comment

SELECT
    articlecomment AS dictValue,
    articlecomment::text AS dictLabel,
    remark,
    'sql_article_comment' AS dictType
FROM article_comment
WHERE isactive = '1' OR status = '0'
ORDER BY articlecomment;
