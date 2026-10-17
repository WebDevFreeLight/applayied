-- Dictionary Query for sys_post_dto
-- Class: SysPostDto
-- DictType: sql_sys_post_dto

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_post_dto' AS dictType
FROM sys_post_dto
WHERE isactive = '1' OR status = '0'
ORDER BY id;
