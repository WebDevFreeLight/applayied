-- Dictionary Query for hr_loan
-- Class: HrLoan
-- DictType: sql_hr_loan

SELECT
    sysbase AS dictValue,
    sysbase::text AS dictLabel,
    remark,
    'sql_hr_loan' AS dictType
FROM hr_loan
WHERE isactive = '1' OR status = '0'
ORDER BY sysbase;
