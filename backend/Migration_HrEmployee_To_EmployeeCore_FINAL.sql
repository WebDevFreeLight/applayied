-- =====================================================================
-- Migration Script: hr_employee → EmployeeCore Tables (FINAL)
-- =====================================================================
-- Purpose: Migrate data from legacy hr_employee table to normalized
--          EmployeeCore structure
-- Date: 2025-12-25
-- Version: FINAL (Based on actual table structure)
-- =====================================================================
-- NOTES:
-- - Source table does NOT have compensation fields (will skip those)
-- - Source table does NOT have encrypted fields (will use plain text)
-- - Source table has: iban, bankaccount, accountnumber (plain text)
-- =====================================================================

BEGIN;

-- =====================================================================
-- Step 1: Create HrPerson records (Core identity)
-- =====================================================================
INSERT INTO hr_person (
    person_id,
    is_active,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    CASE WHEN LOWER(status) = 'active' THEN true ELSE false END AS is_active,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE NOT EXISTS (
    SELECT 1 FROM hr_person p WHERE p.person_id = hr_employee.employeeid
);

-- =====================================================================
-- Step 2: Create HrPersonName records
-- =====================================================================
INSERT INTO hr_person_name (
    person_id,
    name_type_dict_code,
    given_name,
    middle_name,
    family_name,
    full_name_ar,
    effective_from,
    effective_to,
    is_primary,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    NULL AS name_type_dict_code,
    firstname AS given_name,
    NULL AS middle_name,
    lastname AS family_name,
    namear AS full_name_ar,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    true AS is_primary,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE firstname IS NOT NULL OR lastname IS NOT NULL OR namear IS NOT NULL;

-- =====================================================================
-- Step 3a: Create HrPersonContact records (Email - Work)
-- =====================================================================
INSERT INTO hr_person_contact (
    person_id,
    contact_type_dict_code,
    contact_value,
    effective_from,
    effective_to,
    is_primary,
    is_verified,
    verified_time,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    1 AS contact_type_dict_code,
    email AS contact_value,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    true AS is_primary,
    false AS is_verified,
    NULL AS verified_time,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE email IS NOT NULL AND email != '';

-- =====================================================================
-- Step 3b: Create HrPersonContact records (Email - Personal)
-- =====================================================================
INSERT INTO hr_person_contact (
    person_id,
    contact_type_dict_code,
    contact_value,
    effective_from,
    effective_to,
    is_primary,
    is_verified,
    verified_time,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    2 AS contact_type_dict_code,
    personalemail AS contact_value,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    false AS is_primary,
    false AS is_verified,
    NULL AS verified_time,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE personalemail IS NOT NULL AND personalemail != '';

-- =====================================================================
-- Step 3c: Create HrPersonContact records (Phone - Work)
-- =====================================================================
INSERT INTO hr_person_contact (
    person_id,
    contact_type_dict_code,
    contact_value,
    effective_from,
    effective_to,
    is_primary,
    is_verified,
    verified_time,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    3 AS contact_type_dict_code,
    phone AS contact_value,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    true AS is_primary,
    false AS is_verified,
    NULL AS verified_time,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE phone IS NOT NULL AND phone != '';

-- =====================================================================
-- Step 3d: Create HrPersonContact records (Mobile - Personal)
-- =====================================================================
INSERT INTO hr_person_contact (
    person_id,
    contact_type_dict_code,
    contact_value,
    effective_from,
    effective_to,
    is_primary,
    is_verified,
    verified_time,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    4 AS contact_type_dict_code,
    mobilenumber AS contact_value,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    false AS is_primary,
    false AS is_verified,
    NULL AS verified_time,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE mobilenumber IS NOT NULL AND mobilenumber != '';

-- =====================================================================
-- Step 4: Create HrPersonAddress records
-- =====================================================================
INSERT INTO hr_person_address (
    person_id,
    address_type_dict_code,
    country_dict_code,
    city,
    postal_code,
    address_line_1,
    address_line_2,
    effective_from,
    effective_to,
    is_primary,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    1 AS address_type_dict_code,
    NULL AS country_dict_code,
    NULL AS city,
    NULL AS postal_code,
    address AS address_line_1,
    NULL AS address_line_2,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    true AS is_primary,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE address IS NOT NULL AND address != '';

-- =====================================================================
-- Step 5: Create HrPersonDemographic records
-- =====================================================================
INSERT INTO hr_person_demographic (
    person_id,
    date_of_birth,
    gender_dict_code,
    marital_status_dict_code,
    nationality_dict_code,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    dateofbirth AS date_of_birth,
    CASE
        WHEN LOWER(gender) = 'male' THEN 1
        WHEN LOWER(gender) = 'female' THEN 2
        ELSE NULL
    END AS gender_dict_code,
    CASE
        WHEN LOWER(maritalstatus) = 'single' THEN 1
        WHEN LOWER(maritalstatus) = 'married' THEN 2
        WHEN LOWER(maritalstatus) = 'divorced' THEN 3
        WHEN LOWER(maritalstatus) = 'widowed' THEN 4
        ELSE NULL
    END AS marital_status_dict_code,
    NULL AS nationality_dict_code,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee;

-- =====================================================================
-- Step 6a: Create HrPersonIdentifier records (National ID/Iqama)
-- =====================================================================
INSERT INTO hr_person_identifier (
    person_id,
    identifier_type_dict_code,
    identifier_number,
    issuing_country_dict_code,
    issue_date,
    expiry_date,
    is_primary,
    is_verified,
    verified_time,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    CASE WHEN issaudi = true THEN 1 ELSE 2 END AS identifier_type_dict_code,
    idnumber AS identifier_number,
    NULL AS issuing_country_dict_code,
    NULL AS issue_date,
    NULL AS expiry_date,
    true AS is_primary,
    false AS is_verified,
    NULL AS verified_time,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE idnumber IS NOT NULL AND idnumber != '';

-- =====================================================================
-- Step 6b: Create HrPersonIdentifier records (Passport)
-- =====================================================================
INSERT INTO hr_person_identifier (
    person_id,
    identifier_type_dict_code,
    identifier_number,
    issuing_country_dict_code,
    issue_date,
    expiry_date,
    is_primary,
    is_verified,
    verified_time,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    3 AS identifier_type_dict_code,
    passportnumber AS identifier_number,
    NULL AS issuing_country_dict_code,
    NULL AS issue_date,
    passportexpirydate AS expiry_date,
    false AS is_primary,
    false AS is_verified,
    NULL AS verified_time,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE passportnumber IS NOT NULL AND passportnumber != '';

-- =====================================================================
-- Step 6c: Create HrPersonIdentifier records (Visa)
-- =====================================================================
INSERT INTO hr_person_identifier (
    person_id,
    identifier_type_dict_code,
    identifier_number,
    issuing_country_dict_code,
    issue_date,
    expiry_date,
    is_primary,
    is_verified,
    verified_time,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS person_id,
    4 AS identifier_type_dict_code,
    visanumber AS identifier_number,
    NULL AS issuing_country_dict_code,
    NULL AS issue_date,
    visaexpirydate AS expiry_date,
    false AS is_primary,
    false AS is_verified,
    NULL AS verified_time,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE visanumber IS NOT NULL AND visanumber != '';

-- =====================================================================
-- Step 7: Create HrEmployeeCore records
-- =====================================================================
INSERT INTO hr_employee_core (
    employee_id,
    person_id,
    user_id,
    employee_code,
    hire_date,
    termination_date,
    employment_type_dict_code,
    is_active,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS employee_id,
    employeeid AS person_id,
    userid AS user_id,
    employeecode AS employee_code,
    COALESCE(hiredate, joiningdate) AS hire_date,
    terminationdate AS termination_date,
    CASE
        WHEN LOWER(employmenttype) = 'full-time' OR LOWER(employmenttype) = 'full time' THEN 1
        WHEN LOWER(employmenttype) = 'part-time' OR LOWER(employmenttype) = 'part time' THEN 2
        WHEN LOWER(employmenttype) = 'contract' THEN 3
        WHEN LOWER(employmenttype) = 'temporary' THEN 4
        ELSE NULL
    END AS employment_type_dict_code,
    CASE WHEN LOWER(status) = 'active' THEN true ELSE false END AS is_active,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee;

-- =====================================================================
-- Step 8: Create HrEmployeeContract records
-- =====================================================================
INSERT INTO hr_employee_contract (
    employee_id,
    contract_type_dict_code,
    contract_start_date,
    contract_end_date,
    probation_end_date,
    effective_from,
    effective_to,
    is_current,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS employee_id,
    NULL AS contract_type_dict_code,
    contractstartdate AS contract_start_date,
    contractenddate AS contract_end_date,
    NULL AS probation_end_date,
    COALESCE(contractstartdate, hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    CASE
        WHEN contractenddate IS NULL OR contractenddate > CURRENT_DATE THEN true
        ELSE false
    END AS is_current,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE contractstartdate IS NOT NULL OR hiredate IS NOT NULL OR joiningdate IS NOT NULL;

-- =====================================================================
-- Step 9: Create HrEmployeeAssignment records
-- =====================================================================
INSERT INTO hr_employee_assignment (
    employee_id,
    department_id,
    position_id,
    manager_employee_id,
    work_location_id,
    effective_from,
    effective_to,
    is_primary,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS employee_id,
    departmentid AS department_id,
    positionid AS position_id,
    managerid AS manager_employee_id,
    NULL AS work_location_id,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    true AS is_primary,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE departmentid IS NOT NULL OR positionid IS NOT NULL OR managerid IS NOT NULL;

-- =====================================================================
-- Step 10: Create HrEmployeeStatus records
-- =====================================================================
INSERT INTO hr_employee_status (
    employee_id,
    employment_status_dict_code,
    status_reason_dict_code,
    effective_from,
    effective_to,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS employee_id,
    CASE
        WHEN LOWER(status) = 'active' THEN 1
        WHEN LOWER(status) = 'inactive' THEN 2
        WHEN LOWER(status) = 'terminated' THEN 3
        WHEN LOWER(status) = 'suspended' THEN 4
        ELSE 1
    END AS employment_status_dict_code,
    NULL AS status_reason_dict_code,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    terminationdate AS effective_to,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee;

-- =====================================================================
-- Step 11: Create HrEmployeeBankAccount records (Plain Text - NO Encryption)
-- =====================================================================
-- NOTE: Source table has plain text fields: iban, bankaccount, accountnumber
-- These will be stored as NULL in encrypted fields
-- You'll need to encrypt these separately after migration
INSERT INTO hr_employee_bank_account (
    employee_id,
    bank_name,
    iban_encrypted,
    account_number_encrypted,
    bank_account_encrypted,
    currency_id,
    effective_from,
    effective_to,
    is_primary,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS employee_id,
    bankname AS bank_name,
    NULL AS iban_encrypted,  -- TODO: Encrypt iban field separately
    NULL AS account_number_encrypted,  -- TODO: Encrypt accountnumber field separately
    NULL AS bank_account_encrypted,  -- TODO: Encrypt bankaccount field separately
    NULL AS currency_id,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    true AS is_primary,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE bankname IS NOT NULL
   OR iban IS NOT NULL
   OR accountnumber IS NOT NULL
   OR bankaccount IS NOT NULL;

-- =====================================================================
-- Step 12: SKIP COMPENSATION (Fields don't exist in source table)
-- =====================================================================
-- NOTE: Source table does NOT have these fields:
-- - basicsalary
-- - housingallowance
-- - transportallowance
-- - foodallowance
-- - otherallowances
--
-- You'll need to add compensation data separately after migration
-- or add these fields to hr_employee first, then re-run migration

-- =====================================================================
-- Step 13: Create HrEmployeeStatutory records
-- =====================================================================
INSERT INTO hr_employee_statutory (
    employee_id,
    country_dict_code,
    has_gosi,
    gosi_number,
    is_saudi,
    medical_insurance_number,
    effective_from,
    effective_to,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS employee_id,
    NULL AS country_dict_code,
    CASE WHEN gosinumber IS NOT NULL AND gosinumber != '' THEN true ELSE false END AS has_gosi,
    gosinumber AS gosi_number,
    issaudi AS is_saudi,
    medicalinsurancenumber AS medical_insurance_number,
    COALESCE(hiredate, joiningdate, create_time) AS effective_from,
    NULL AS effective_to,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee;

-- =====================================================================
-- Step 14: Create HrEmployeeEmergencyContact records
-- =====================================================================
INSERT INTO hr_employee_emergency_contact (
    employee_id,
    contact_name,
    relationship_dict_code,
    phone,
    alternate_phone,
    email,
    address,
    is_primary,
    is_active,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS employee_id,
    emergencycontact AS contact_name,
    NULL AS relationship_dict_code,
    emergencyphone AS phone,
    emergencycontactphone AS alternate_phone,
    NULL AS email,
    NULL AS address,
    true AS is_primary,
    true AS is_active,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE emergencycontact IS NOT NULL AND emergencycontact != '';

-- =====================================================================
-- VERIFICATION QUERIES
-- =====================================================================
SELECT '=======================' AS separator;
SELECT 'MIGRATION VERIFICATION' AS title;
SELECT '=======================' AS separator;

SELECT 'hr_person' AS table_name, COUNT(*) AS record_count FROM hr_person
UNION ALL
SELECT 'hr_employee (source)', COUNT(*) FROM hr_employee
UNION ALL
SELECT 'hr_person_name', COUNT(*) FROM hr_person_name
UNION ALL
SELECT 'hr_person_contact', COUNT(*) FROM hr_person_contact
UNION ALL
SELECT 'hr_person_address', COUNT(*) FROM hr_person_address
UNION ALL
SELECT 'hr_person_demographic', COUNT(*) FROM hr_person_demographic
UNION ALL
SELECT 'hr_person_identifier', COUNT(*) FROM hr_person_identifier
UNION ALL
SELECT 'hr_employee_core', COUNT(*) FROM hr_employee_core
UNION ALL
SELECT 'hr_employee_contract', COUNT(*) FROM hr_employee_contract
UNION ALL
SELECT 'hr_employee_assignment', COUNT(*) FROM hr_employee_assignment
UNION ALL
SELECT 'hr_employee_status', COUNT(*) FROM hr_employee_status
UNION ALL
SELECT 'hr_employee_bank_account', COUNT(*) FROM hr_employee_bank_account
UNION ALL
SELECT 'hr_employee_statutory', COUNT(*) FROM hr_employee_statutory
UNION ALL
SELECT 'hr_employee_emergency_contact', COUNT(*) FROM hr_employee_emergency_contact;

SELECT '=======================' AS separator;
SELECT 'Sample data comparison (first employee):' AS title;
SELECT '=======================' AS separator;

SELECT
    e.employeeid,
    e.employeecode AS old_code,
    ec.employee_code AS new_code,
    e.firstname || ' ' || e.lastname AS old_name,
    pn.given_name || ' ' || pn.family_name AS new_name,
    e.status AS old_status,
    CASE WHEN ec.is_active THEN 'active' ELSE 'inactive' END AS new_status
FROM hr_employee e
LEFT JOIN hr_employee_core ec ON ec.employee_id = e.employeeid
LEFT JOIN hr_person_name pn ON pn.person_id = e.employeeid AND pn.is_primary = true
ORDER BY e.employeeid
LIMIT 3;

COMMIT;

-- =====================================================================
-- POST-MIGRATION NOTES
-- =====================================================================
-- 1. Compensation data NOT migrated (fields don't exist in old table)
--    - Add compensation packages manually or via separate script
--
-- 2. Bank account encryption NOT done (plain text in old table)
--    - Old table has: iban, bankaccount, accountnumber (plain text)
--    - New table stored NULL in encrypted fields
--    - TODO: Create script to encrypt and populate these fields
--
-- 3. Fields NOT migrated:
--    - biometricid (consider adding to hr_employee_core)
--    - worklocation (stored as string, need location lookup table)
--    - shiftid (consider adding to hr_employee_assignment)
--    - educationlevel, fieldofstudy, university, graduationyear
--    - previouscompany, yearsofexperience
--    - roleids (JSON array)
--    - profilepicture
--    - companyid, branchid (in old table but not in new structure)
--    - glaccountid (in old table, consider adding to new structure)
-- =====================================================================
