-- Check what encrypted columns actually exist in hr_employee table
SELECT
    column_name,
    data_type,
    character_maximum_length
FROM information_schema.columns
WHERE table_name = 'hr_employee'
  AND (
    column_name LIKE '%encrypt%'
    OR column_name LIKE '%encrpt%'  -- Check for typo
    OR column_name LIKE 'iban%'
    OR column_name LIKE 'bankaccount%'
    OR column_name LIKE 'accountnumber%'
  )
ORDER BY column_name;

-- Also check all columns to see what's actually there
SELECT column_name
FROM information_schema.columns
WHERE table_name = 'hr_employee'
ORDER BY column_name;
