-- Dictionary Query for article_browsing_log
-- Class: ArticleBrowsingLog
-- DictType: sql_article_browsing_log

SELECT
    articlebrowsinglog AS dictValue,
    articlebrowsinglog::text AS dictLabel,
    remark,
    'sql_article_browsing_log' AS dictType
FROM article_browsing_log
WHERE isactive = '1' OR status = '0'
ORDER BY articlebrowsinglog;
