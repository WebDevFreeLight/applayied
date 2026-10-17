-- Dictionary Query for hr_training_course
-- Class: HrTrainingCourse
-- DictType: sql_hr_training_course

SELECT
    sysbase AS dictValue,
    CONCAT(, ' - ', namear) AS dictLabel,
    remark,
    'sql_hr_training_course' AS dictType
FROM hr_training_course
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
