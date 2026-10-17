-- Dictionary Query for hr_emergency_contact
-- Class: HrEmployeeProfile
-- DictType: sql_hr_emergency_contact

SELECT
    sysbase AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_emergency_contact' AS dictType
FROM hr_emergency_contact
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
