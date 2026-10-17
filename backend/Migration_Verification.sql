-- =====================================================================
-- Migration Verification Script
-- =====================================================================
-- Purpose: Verify data integrity after hr_employee → EmployeeCore migration
-- Run this AFTER the main migration script completes
-- =====================================================================

\echo '=========================================='
\echo 'EMPLOYEE MIGRATION VERIFICATION REPORT'
\echo '=========================================='
\echo ''

-- =====================================================================
-- 1. Record Count Comparison
-- =====================================================================
\echo '1. RECORD COUNT VERIFICATION'
\echo '----------------------------'

WITH source_count AS (
    SELECT COUNT(*) AS total FROM hr_employee
),
target_counts AS (
    SELECT
        (SELECT COUNT(*) FROM hr_person) AS person_count,
        (SELECT COUNT(*) FROM hr_employee_core) AS employee_count,
        (SELECT COUNT(*) FROM hr_person_name) AS name_count,
        (SELECT COUNT(*) FROM hr_person_contact) AS contact_count,
        (SELECT COUNT(*) FROM hr_person_demographic) AS demographic_count,
        (SELECT COUNT(*) FROM hr_employee_compensation_package) AS comp_package_count
)
SELECT
    'Source Employees' AS metric,
    s.total AS count,
    'Expected target' AS status
FROM source_count s
UNION ALL
SELECT
    'hr_person',
    t.person_count,
    CASE WHEN t.person_count = s.total THEN '✓ PASS' ELSE '✗ FAIL' END
FROM source_count s, target_counts t
UNION ALL
SELECT
    'hr_employee_core',
    t.employee_count,
    CASE WHEN t.employee_count = s.total THEN '✓ PASS' ELSE '✗ FAIL' END
FROM source_count s, target_counts t
UNION ALL
SELECT
    'hr_person_name',
    t.name_count,
    CASE WHEN t.name_count > 0 THEN '✓ PASS' ELSE '⚠ WARNING' END
FROM source_count s, target_counts t
UNION ALL
SELECT
    'hr_person_contact',
    t.contact_count,
    CASE WHEN t.contact_count > 0 THEN '✓ PASS' ELSE '⚠ WARNING' END
FROM source_count s, target_counts t
UNION ALL
SELECT
    'hr_person_demographic',
    t.demographic_count,
    CASE WHEN t.demographic_count = s.total THEN '✓ PASS' ELSE '⚠ WARNING' END
FROM source_count s, target_counts t;

\echo ''

-- =====================================================================
-- 2. Referential Integrity Checks
-- =====================================================================
\echo '2. REFERENTIAL INTEGRITY CHECKS'
\echo '--------------------------------'

-- Check: All employees have person records
SELECT
    'Employees without Person records' AS check_name,
    COUNT(*) AS issue_count,
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '✗ FAIL' END AS status
FROM hr_employee_core ec
LEFT JOIN hr_person p ON p.person_id = ec.person_id
WHERE p.person_id IS NULL

UNION ALL

-- Check: All employees have names
SELECT
    'Employees without Names',
    COUNT(*),
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '⚠ WARNING' END
FROM hr_employee_core ec
LEFT JOIN hr_person_name pn ON pn.person_id = ec.person_id AND pn.is_primary = true
WHERE pn.person_name_id IS NULL

UNION ALL

-- Check: Orphaned person records
SELECT
    'Person records without Employee',
    COUNT(*),
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '⚠ WARNING (may be valid)' END
FROM hr_person p
LEFT JOIN hr_employee_core ec ON ec.person_id = p.person_id
WHERE ec.employee_id IS NULL

UNION ALL

-- Check: Orphaned compensation components
SELECT
    'Orphaned Compensation Components',
    COUNT(*),
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '✗ FAIL' END
FROM hr_employee_compensation_component cc
LEFT JOIN hr_employee_compensation_package cp ON cp.comp_package_id = cc.comp_package_id
WHERE cp.comp_package_id IS NULL;

\echo ''

-- =====================================================================
-- 3. Data Quality Checks
-- =====================================================================
\echo '3. DATA QUALITY CHECKS'
\echo '----------------------'

-- Check: NULL employee codes
SELECT
    'Employees with NULL codes' AS check_name,
    COUNT(*) AS issue_count,
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '⚠ WARNING' END AS status
FROM hr_employee_core
WHERE employee_code IS NULL OR employee_code = ''

UNION ALL

-- Check: Duplicate employee codes
SELECT
    'Duplicate employee codes',
    COUNT(*),
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '✗ FAIL' END
FROM (
    SELECT employee_code
    FROM hr_employee_core
    WHERE employee_code IS NOT NULL
    GROUP BY employee_code
    HAVING COUNT(*) > 1
) duplicates

UNION ALL

-- Check: Multiple primary names per person
SELECT
    'Multiple primary names per person',
    COUNT(*),
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '✗ FAIL' END
FROM (
    SELECT person_id
    FROM hr_person_name
    WHERE is_primary = true
    GROUP BY person_id
    HAVING COUNT(*) > 1
) multi_primary

UNION ALL

-- Check: Effective-dated records without EffectiveFrom
SELECT
    'Person names without EffectiveFrom',
    COUNT(*),
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '✗ FAIL' END
FROM hr_person_name
WHERE effective_from IS NULL;

\echo ''

-- =====================================================================
-- 4. Compensation Verification
-- =====================================================================
\echo '4. COMPENSATION ACCURACY CHECK'
\echo '-------------------------------'

WITH comp_comparison AS (
    SELECT
        e.employeeid,  -- OLD: employeeid
        e.employeecode,
        (COALESCE(e.basicsalary, 0) +
         COALESCE(e.housingallowance, 0) +
         COALESCE(e.transportallowance, 0) +
         COALESCE(e.foodallowance, 0) +
         COALESCE(e.otherallowances, 0)) AS old_total,
        COALESCE((
            SELECT SUM(cc.amount)
            FROM hr_employee_compensation_component cc
            JOIN hr_employee_compensation_package cp ON cp.comp_package_id = cc.comp_package_id
            WHERE cp.employee_id = e.employeeid
        ), 0) AS new_total
    FROM hr_employee e
)
SELECT
    'Employees with matching compensation' AS metric,
    COUNT(*) AS count,
    CASE WHEN COUNT(*) = (SELECT COUNT(*) FROM hr_employee) THEN '✓ PASS' ELSE '⚠ CHECK DETAILS' END AS status
FROM comp_comparison
WHERE ABS(old_total - new_total) < 0.01

UNION ALL

SELECT
    'Employees with compensation mismatch',
    COUNT(*),
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '⚠ CHECK DETAILS' END
FROM comp_comparison
WHERE ABS(old_total - new_total) >= 0.01;

\echo ''
\echo 'Detailed compensation mismatches (if any):'

SELECT
    e.employeeid,  -- OLD: employeeid
    e.employeecode,
    (COALESCE(e.basicsalary, 0) +
     COALESCE(e.housingallowance, 0) +
     COALESCE(e.transportallowance, 0) +
     COALESCE(e.foodallowance, 0) +
     COALESCE(e.otherallowances, 0)) AS old_total,
    COALESCE((
        SELECT SUM(cc.amount)
        FROM hr_employee_compensation_component cc
        JOIN hr_employee_compensation_package cp ON cp.comp_package_id = cc.comp_package_id
        WHERE cp.employee_id = e.employeeid
    ), 0) AS new_total,
    ABS((COALESCE(e.basicsalary, 0) +
         COALESCE(e.housingallowance, 0) +
         COALESCE(e.transportallowance, 0) +
         COALESCE(e.foodallowance, 0) +
         COALESCE(e.otherallowances, 0)) -
        COALESCE((
            SELECT SUM(cc.amount)
            FROM hr_employee_compensation_component cc
            JOIN hr_employee_compensation_package cp ON cp.comp_package_id = cc.comp_package_id
            WHERE cp.employee_id = e.employeeid
        ), 0)) AS difference
FROM hr_employee e
WHERE ABS((COALESCE(e.basicsalary, 0) +
           COALESCE(e.housingallowance, 0) +
           COALESCE(e.transportallowance, 0) +
           COALESCE(e.foodallowance, 0) +
           COALESCE(e.otherallowances, 0)) -
          COALESCE((
              SELECT SUM(cc.amount)
              FROM hr_employee_compensation_component cc
              JOIN hr_employee_compensation_package cp ON cp.comp_package_id = cc.comp_package_id
              WHERE cp.employee_id = e.employeeid
          ), 0)) >= 0.01
LIMIT 10;

\echo ''

-- =====================================================================
-- 5. Encrypted Fields Verification
-- =====================================================================
\echo '5. ENCRYPTED FIELDS VERIFICATION'
\echo '---------------------------------'

SELECT
    'Employees with encrypted IBAN (old)' AS metric,
    COUNT(*) AS count,
    'Source data' AS status
FROM hr_employee
WHERE iban_encrypted IS NOT NULL

UNION ALL

SELECT
    'Employees with encrypted IBAN (new)',
    COUNT(*),
    CASE WHEN COUNT(*) = (SELECT COUNT(*) FROM hr_employee WHERE iban_encrypted IS NOT NULL)
         THEN '✓ PASS' ELSE '✗ FAIL' END
FROM hr_employee_bank_account
WHERE iban_encrypted IS NOT NULL

UNION ALL

SELECT
    'Encrypted fields NULL check',
    COUNT(*),
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '✗ FAIL - Encrypted data lost' END
FROM hr_employee e
LEFT JOIN hr_employee_bank_account ba ON ba.employee_id = e.employeeid  -- OLD: employeeid
WHERE e.iban_encrypted IS NOT NULL AND ba.iban_encrypted IS NULL;

\echo ''

-- =====================================================================
-- 6. Dictionary Code Mapping Verification
-- =====================================================================
\echo '6. DICTIONARY CODE VERIFICATION'
\echo '--------------------------------'

-- Check for unmapped gender codes
SELECT
    'Unmapped gender codes (NULL)' AS check_name,
    COUNT(*) AS count,
    CASE WHEN COUNT(*) > 0 THEN '⚠ WARNING - Review mapping' ELSE '✓ PASS' END AS status
FROM hr_person_demographic
WHERE gender_dict_code IS NULL AND EXISTS (
    SELECT 1 FROM hr_employee e WHERE e.employeeid = hr_person_demographic.person_id AND e.gender IS NOT NULL
)

UNION ALL

-- Check for unmapped marital status codes
SELECT
    'Unmapped marital status codes (NULL)',
    COUNT(*),
    CASE WHEN COUNT(*) > 0 THEN '⚠ WARNING - Review mapping' ELSE '✓ PASS' END
FROM hr_person_demographic
WHERE marital_status_dict_code IS NULL AND EXISTS (
    SELECT 1 FROM hr_employee e WHERE e.employeeid = hr_person_demographic.person_id AND e.maritalstatus IS NOT NULL
)

UNION ALL

-- Check for unmapped employment type codes
SELECT
    'Unmapped employment type codes (NULL)',
    COUNT(*),
    CASE WHEN COUNT(*) > 0 THEN '⚠ WARNING - Review mapping' ELSE '✓ PASS' END
FROM hr_employee_core
WHERE employment_type_dict_code IS NULL AND EXISTS (
    SELECT 1 FROM hr_employee e WHERE e.employeeid = hr_employee_core.employee_id AND e.employmenttype IS NOT NULL
);

\echo ''

-- =====================================================================
-- 7. Sample Data Comparison
-- =====================================================================
\echo '7. SAMPLE DATA COMPARISON (First 5 Employees)'
\echo '----------------------------------------------'

SELECT
    e.employeeid,  -- OLD: employeeid (lowercase, no underscore)
    e.employeecode AS old_code,
    ec.employee_code AS new_code,
    CONCAT(e.firstname, ' ', e.lastname) AS old_name,
    CONCAT(pn.given_name, ' ', pn.family_name) AS new_name,
    e.email AS old_email,
    (SELECT contact_value FROM hr_person_contact pc
     WHERE pc.person_id = e.employeeid AND pc.contact_type_dict_code = 1
     AND pc.is_primary = true LIMIT 1) AS new_email,
    e.status AS old_status,
    CASE WHEN ec.is_active THEN 'active' ELSE 'inactive' END AS new_status
FROM hr_employee e
LEFT JOIN hr_employee_core ec ON ec.employee_id = e.employeeid
LEFT JOIN hr_person_name pn ON pn.person_id = e.employeeid AND pn.is_primary = true
ORDER BY e.employeeid
LIMIT 5;

\echo ''

-- =====================================================================
-- 8. Contact Information Distribution
-- =====================================================================
\echo '8. CONTACT INFORMATION DISTRIBUTION'
\echo '------------------------------------'

SELECT
    contact_type_dict_code,
    COUNT(*) AS contact_count,
    COUNT(DISTINCT person_id) AS unique_persons,
    SUM(CASE WHEN is_primary THEN 1 ELSE 0 END) AS primary_count
FROM hr_person_contact
GROUP BY contact_type_dict_code
ORDER BY contact_type_dict_code;

\echo ''

-- =====================================================================
-- 9. Identifier Types Distribution
-- =====================================================================
\echo '9. IDENTIFIER TYPES DISTRIBUTION'
\echo '---------------------------------'

SELECT
    identifier_type_dict_code,
    COUNT(*) AS identifier_count,
    COUNT(DISTINCT person_id) AS unique_persons,
    SUM(CASE WHEN is_primary THEN 1 ELSE 0 END) AS primary_count
FROM hr_person_identifier
GROUP BY identifier_type_dict_code
ORDER BY identifier_type_dict_code;

\echo ''

-- =====================================================================
-- 10. Effective Dating Validation
-- =====================================================================
\echo '10. EFFECTIVE DATING VALIDATION'
\echo '--------------------------------'

-- Check for records with EffectiveTo before EffectiveFrom
SELECT
    'Invalid date ranges (to < from)' AS check_name,
    COUNT(*) AS issue_count,
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '✗ FAIL' END AS status
FROM (
    SELECT 'person_name' AS tbl FROM hr_person_name WHERE effective_to < effective_from
    UNION ALL
    SELECT 'person_contact' FROM hr_person_contact WHERE effective_to < effective_from
    UNION ALL
    SELECT 'person_address' FROM hr_person_address WHERE effective_to < effective_from
    UNION ALL
    SELECT 'employee_contract' FROM hr_employee_contract WHERE effective_to < effective_from
    UNION ALL
    SELECT 'employee_assignment' FROM hr_employee_assignment WHERE effective_to < effective_from
    UNION ALL
    SELECT 'employee_bank_account' FROM hr_employee_bank_account WHERE effective_to < effective_from
) invalid_dates

UNION ALL

-- Check for overlapping primary records
SELECT
    'Overlapping primary name records',
    COUNT(*),
    CASE WHEN COUNT(*) = 0 THEN '✓ PASS' ELSE '⚠ WARNING' END
FROM (
    SELECT pn1.person_id
    FROM hr_person_name pn1
    JOIN hr_person_name pn2 ON pn1.person_id = pn2.person_id
        AND pn1.person_name_id != pn2.person_name_id
        AND pn1.is_primary = true
        AND pn2.is_primary = true
        AND pn1.effective_from < COALESCE(pn2.effective_to, '9999-12-31')
        AND COALESCE(pn1.effective_to, '9999-12-31') > pn2.effective_from
    GROUP BY pn1.person_id
) overlaps;

\echo ''

-- =====================================================================
-- 11. Summary Statistics
-- =====================================================================
\echo '11. SUMMARY STATISTICS'
\echo '----------------------'

SELECT
    'Total source employees' AS metric,
    COUNT(*)::text AS value
FROM hr_employee

UNION ALL

SELECT
    'Total migrated persons',
    COUNT(*)::text
FROM hr_person

UNION ALL

SELECT
    'Total migrated employees',
    COUNT(*)::text
FROM hr_employee_core

UNION ALL

SELECT
    'Active employees (new)',
    COUNT(*)::text
FROM hr_employee_core
WHERE is_active = true

UNION ALL

SELECT
    'Employees with compensation',
    COUNT(DISTINCT employee_id)::text
FROM hr_employee_compensation_package

UNION ALL

SELECT
    'Total compensation components',
    COUNT(*)::text
FROM hr_employee_compensation_component

UNION ALL

SELECT
    'Employees with bank accounts',
    COUNT(*)::text
FROM hr_employee_bank_account

UNION ALL

SELECT
    'Emergency contacts migrated',
    COUNT(*)::text
FROM hr_employee_emergency_contact

UNION ALL

SELECT
    'Statutory records created',
    COUNT(*)::text
FROM hr_employee_statutory;

\echo ''
\echo '=========================================='
\echo 'VERIFICATION COMPLETE'
\echo '=========================================='
\echo ''
\echo 'Next Steps:'
\echo '1. Review any FAIL or WARNING statuses above'
\echo '2. Fix dictionary code mappings if needed'
\echo '3. Resolve any data quality issues'
\echo '4. Run application tests'
\echo '5. Document any unmigrated fields'
\echo ''

-- =====================================================================
-- Optional: Detailed Issue Reports
-- =====================================================================

\echo ''
\echo 'DETAILED ISSUE REPORTS (if applicable)'
\echo '======================================='

\echo ''
\echo 'Employees without person records:'
SELECT ec.employee_id, ec.employee_code
FROM hr_employee_core ec
LEFT JOIN hr_person p ON p.person_id = ec.person_id
WHERE p.person_id IS NULL
LIMIT 10;

\echo ''
\echo 'Employees without primary names:'
SELECT ec.employee_id, ec.employee_code
FROM hr_employee_core ec
LEFT JOIN hr_person_name pn ON pn.person_id = ec.person_id AND pn.is_primary = true
WHERE pn.person_name_id IS NULL
LIMIT 10;

\echo ''
\echo 'Duplicate employee codes:'
SELECT employee_code, COUNT(*) AS duplicate_count
FROM hr_employee_core
WHERE employee_code IS NOT NULL
GROUP BY employee_code
HAVING COUNT(*) > 1
LIMIT 10;

\echo ''
\echo '=========================================='
\echo 'END OF VERIFICATION REPORT'
\echo '=========================================='
