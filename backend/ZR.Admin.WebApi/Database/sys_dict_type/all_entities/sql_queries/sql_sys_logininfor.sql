-- Dictionary Query for sys_logininfor
-- Class: SysLogininfor
-- DictType: sql_sys_logininfor

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_sys_logininfor' AS dictType
FROM sys_logininfor
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
