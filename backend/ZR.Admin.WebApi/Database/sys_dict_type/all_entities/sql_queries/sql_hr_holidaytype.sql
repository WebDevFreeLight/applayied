-- Dictionary Query for hr_holidaytype
-- Class: HrHolidaytype
-- DictType: sql_hr_holidaytype

SELECT
    id AS dictValue,
     AS dictLabel,
    remark,
    'sql_hr_holidaytype' AS dictType
FROM hr_holidaytype
WHERE isactive = '1' OR status = '0'
ORDER BY id;
