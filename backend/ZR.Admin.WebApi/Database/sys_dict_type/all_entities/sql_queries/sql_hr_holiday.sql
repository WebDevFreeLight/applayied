-- Dictionary Query for hr_holiday
-- Class: HrHoliday
-- DictType: sql_hr_holiday

SELECT
    id AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_hr_holiday' AS dictType
FROM hr_holiday
WHERE isactive = '1' OR status = '0'
ORDER BY id;
