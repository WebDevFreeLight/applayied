-- Dictionary Query for sys_dict_type
-- Class: SysDictType
-- DictType: sql_sys_dict_type

SELECT
    imaindbentity AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_dict_type' AS dictType
FROM sys_dict_type
WHERE isactive = '1' OR status = '0'
ORDER BY imaindbentity;
