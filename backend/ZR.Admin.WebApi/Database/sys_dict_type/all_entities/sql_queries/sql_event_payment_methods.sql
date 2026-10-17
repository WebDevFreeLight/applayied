-- Dictionary Query for event_payment_methods
-- Class: PaymentMethod
-- DictType: sql_event_payment_methods

SELECT
    methodcode AS dictValue,
    CONCAT(methodcode, ' - ', methodname, ' - ', methodnamear) AS dictLabel,
    remark,
    'sql_event_payment_methods' AS dictType
FROM event_payment_methods
WHERE isactive = '1' OR status = '0'
ORDER BY methodcode;
