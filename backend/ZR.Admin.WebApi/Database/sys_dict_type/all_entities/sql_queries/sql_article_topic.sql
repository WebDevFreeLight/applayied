-- Dictionary Query for article_topic
-- Class: ArticleTopic
-- DictType: sql_article_topic

SELECT
    articletopic AS dictValue,
     AS dictLabel,
    remark,
    'sql_article_topic' AS dictType
FROM article_topic
WHERE isactive = '1' OR status = '0'
ORDER BY articletopic;
