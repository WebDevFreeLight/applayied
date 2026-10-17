-- Dictionary Query for sys_dict_data
-- Class: SysDictData
-- DictType: sql_sys_dict_data

SELECT
    imaindbentity AS dictValue,
    imaindbentity::text AS dictLabel,
    remark,
    'sql_sys_dict_data' AS dictType
FROM sys_dict_data
WHERE isactive = '1' OR status = '0'
ORDER BY imaindbentity;
