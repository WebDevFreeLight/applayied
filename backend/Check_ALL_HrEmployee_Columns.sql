-- =====================================================================
-- Check ALL columns in hr_employee table
-- =====================================================================
-- Run this to see the EXACT column names in your database
-- This will show you what columns actually exist
-- =====================================================================

\echo 'ALL COLUMNS in hr_employee table:'
\echo '=================================='

SELECT
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'hr_employee'
ORDER BY ordinal_position;

\echo ''
\echo 'Specifically checking compensation fields:'
\echo '=========================================='

SELECT column_name
FROM information_schema.columns
WHERE table_name = 'hr_employee'
  AND (
    column_name LIKE '%salary%'
    OR column_name LIKE '%allowance%'
  )
ORDER BY column_name;

\echo ''
\echo 'Specifically checking encrypted fields:'
\echo '======================================='

SELECT column_name
FROM information_schema.columns
WHERE table_name = 'hr_employee'
  AND column_name LIKE '%encrypt%'
ORDER BY column_name;

\echo ''
\echo 'Specifically checking bank fields:'
\echo '=================================='

SELECT column_name
FROM information_schema.columns
WHERE table_name = 'hr_employee'
  AND (
    column_name LIKE '%bank%'
    OR column_name LIKE '%iban%'
    OR column_name LIKE '%account%'
  )
ORDER BY column_name;
