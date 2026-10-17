-- Dictionary Query for sys_config
-- Class: SysConfig
-- DictType: sql_sys_config

SELECT
    imaindbentity AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_config' AS dictType
FROM sys_config
WHERE isactive = '1' OR status = '0'
ORDER BY imaindbentity;
