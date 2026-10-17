-- Dictionary Query for articleCategory
-- Class: ArticleCategory
-- DictType: sql_articleCategory

SELECT
    articlecategory AS dictValue,
     AS dictLabel,
    remark,
    'sql_articleCategory' AS dictType
FROM articleCategory
WHERE isactive = '1' OR status = '0'
ORDER BY articlecategory;
