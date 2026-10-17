-- =====================================================================
-- Migration Script: hr_employee → EmployeeCore Tables (CORRECTED)
-- =====================================================================
-- Purpose: Migrate data from legacy hr_employee table to normalized
--          EmployeeCore structure with effective dating support
-- Date: 2025-12-25
-- Version: 2.0 (Column names corrected)
-- =====================================================================

-- COLUMN NAMING CONVENTIONS:
-- OLD table (hr_employee): lowercase no underscores (employeeid, firstname)
-- NEW tables: snake_case with underscores (employee_id, given_name)
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
    employeeid AS person_id,  -- OLD: employeeid, NEW: person_id
    CASE
        WHEN LOWER(status) = 'active' THEN true
        ELSE false
    END AS is_active,
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
    employeeid AS person_id,  -- OLD: employeeid
    NULL AS name_type_dict_code,  -- TODO: Map to "Legal Name" or similar
    firstname AS given_name,  -- OLD: firstname
    NULL AS middle_name,
    lastname AS family_name,  -- OLD: lastname
    namear AS full_name_ar,  -- OLD: namear
    COALESCE(hiredate, create_time) AS effective_from,  -- OLD: hiredate
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
    employeeid AS person_id,  -- OLD: employeeid
    1 AS contact_type_dict_code,  -- TODO: Map to "Work Email" dict code
    email AS contact_value,  -- OLD: email
    COALESCE(hiredate, create_time) AS effective_from,
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
    2 AS contact_type_dict_code,  -- TODO: Map to "Personal Email" dict code
    personalemail AS contact_value,  -- OLD: personalemail
    COALESCE(hiredate, create_time) AS effective_from,
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
    3 AS contact_type_dict_code,  -- TODO: Map to "Work Phone" dict code
    phone AS contact_value,  -- OLD: phone
    COALESCE(hiredate, create_time) AS effective_from,
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
    4 AS contact_type_dict_code,  -- TODO: Map to "Mobile Phone" dict code
    mobilenumber AS contact_value,  -- OLD: mobilenumber
    COALESCE(hiredate, create_time) AS effective_from,
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
    1 AS address_type_dict_code,  -- TODO: Map to "Residential" or "Primary" address type
    NULL AS country_dict_code,  -- TODO: Extract from address or use nationality
    NULL AS city,
    NULL AS postal_code,
    address AS address_line_1,  -- OLD: address
    NULL AS address_line_2,
    COALESCE(hiredate, create_time) AS effective_from,
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
    dateofbirth AS date_of_birth,  -- OLD: dateofbirth
    CASE
        WHEN LOWER(gender) = 'male' THEN 1  -- OLD: gender
        WHEN LOWER(gender) = 'female' THEN 2
        ELSE NULL
    END AS gender_dict_code,  -- TODO: Map to actual gender dictionary codes
    CASE
        WHEN LOWER(maritalstatus) = 'single' THEN 1  -- OLD: maritalstatus
        WHEN LOWER(maritalstatus) = 'married' THEN 2
        WHEN LOWER(maritalstatus) = 'divorced' THEN 3
        WHEN LOWER(maritalstatus) = 'widowed' THEN 4
        ELSE NULL
    END AS marital_status_dict_code,  -- TODO: Map to actual marital status dictionary codes
    NULL AS nationality_dict_code,  -- TODO: Map nationality string to dictionary code
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
    CASE
        WHEN issaudi = true THEN 1  -- OLD: issaudi - National ID
        ELSE 2  -- Iqama
    END AS identifier_type_dict_code,  -- TODO: Map to actual identifier type dict codes
    idnumber AS identifier_number,  -- OLD: idnumber
    NULL AS issuing_country_dict_code,  -- TODO: Saudi Arabia dict code
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
    3 AS identifier_type_dict_code,  -- TODO: Map to "Passport" dict code
    passportnumber AS identifier_number,  -- OLD: passportnumber
    NULL AS issuing_country_dict_code,  -- TODO: Map nationality to country dict code
    NULL AS issue_date,
    passportexpirydate AS expiry_date,  -- OLD: passportexpirydate
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
    4 AS identifier_type_dict_code,  -- TODO: Map to "Visa" dict code
    visanumber AS identifier_number,  -- OLD: visanumber
    NULL AS issuing_country_dict_code,  -- TODO: Saudi Arabia dict code
    NULL AS issue_date,
    visaexpirydate AS expiry_date,  -- OLD: visaexpirydate
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
    employeeid AS employee_id,  -- OLD: employeeid, NEW: employee_id
    employeeid AS person_id,  -- Same as employee_id (person already created)
    userid AS user_id,  -- OLD: userid
    employeecode AS employee_code,  -- OLD: employeecode
    hiredate AS hire_date,  -- OLD: hiredate
    terminationdate AS termination_date,  -- OLD: terminationdate
    CASE
        WHEN LOWER(employmenttype) = 'full-time' OR LOWER(employmenttype) = 'full time' THEN 1  -- OLD: employmenttype
        WHEN LOWER(employmenttype) = 'part-time' OR LOWER(employmenttype) = 'part time' THEN 2
        WHEN LOWER(employmenttype) = 'contract' THEN 3
        WHEN LOWER(employmenttype) = 'temporary' THEN 4
        ELSE NULL
    END AS employment_type_dict_code,  -- TODO: Map to actual employment type dict codes
    CASE
        WHEN LOWER(status) = 'active' THEN true  -- OLD: status
        ELSE false
    END AS is_active,
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
    employeeid AS employee_id,  -- OLD: employeeid
    NULL AS contract_type_dict_code,  -- TODO: Derive from employment_type or set default
    contractstartdate AS contract_start_date,  -- OLD: contractstartdate
    contractenddate AS contract_end_date,  -- OLD: contractenddate
    NULL AS probation_end_date,  -- TODO: Calculate if needed (e.g., hire_date + 90 days)
    COALESCE(contractstartdate, hiredate, create_time) AS effective_from,
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
WHERE contractstartdate IS NOT NULL OR hiredate IS NOT NULL;

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
    employeeid AS employee_id,  -- OLD: employeeid
    departmentid AS department_id,  -- OLD: departmentid
    positionid AS position_id,  -- OLD: positionid
    managerid AS manager_employee_id,  -- OLD: managerid
    NULL AS work_location_id,  -- TODO: Create work_location lookup table and map worklocation string
    COALESCE(hiredate, create_time) AS effective_from,
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
    employeeid AS employee_id,  -- OLD: employeeid
    CASE
        WHEN LOWER(status) = 'active' THEN 1  -- OLD: status
        WHEN LOWER(status) = 'inactive' THEN 2
        WHEN LOWER(status) = 'terminated' THEN 3
        WHEN LOWER(status) = 'suspended' THEN 4
        ELSE 1
    END AS employment_status_dict_code,  -- TODO: Map to actual status dict codes
    NULL AS status_reason_dict_code,  -- TODO: Add if termination/suspension reasons exist
    COALESCE(hiredate, create_time) AS effective_from,
    terminationdate AS effective_to,  -- OLD: terminationdate
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee;

-- =====================================================================
-- Step 11: Create HrEmployeeBankAccount records
-- =====================================================================
-- NOTE: This assumes encrypted fields exist in old table
-- If they don't exist, you'll need to migrate plain text fields first
-- and encrypt them separately, or skip encrypted fields entirely

-- First, check if we have bank account data to migrate
-- We'll try to migrate both encrypted and plain text fields
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
    employeeid AS employee_id,  -- OLD: employeeid
    bankname AS bank_name,  -- OLD: bankname
    -- Try to get encrypted fields if they exist, otherwise NULL
    -- NOTE: If old table doesn't have encrypted fields, these will be NULL
    -- You may need to encrypt plain text fields separately after migration
    CASE
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_name = 'hr_employee' AND column_name = 'iban_encrypted'
        ) THEN (SELECT iban_encrypted FROM hr_employee e2 WHERE e2.employeeid = hr_employee.employeeid)
        ELSE NULL
    END AS iban_encrypted,
    CASE
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_name = 'hr_employee' AND column_name = 'accountnumber_encrypted'
        ) THEN (SELECT accountnumber_encrypted FROM hr_employee e2 WHERE e2.employeeid = hr_employee.employeeid)
        ELSE NULL
    END AS account_number_encrypted,
    CASE
        WHEN EXISTS (
            SELECT 1 FROM information_schema.columns
            WHERE table_name = 'hr_employee' AND column_name = 'bankaccount_encrypted'
        ) THEN (SELECT bankaccount_encrypted FROM hr_employee e2 WHERE e2.employeeid = hr_employee.employeeid)
        ELSE NULL
    END AS bank_account_encrypted,
    NULL AS currency_id,  -- TODO: Map to default currency (e.g., SAR)
    COALESCE(hiredate, create_time) AS effective_from,
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
-- Step 12: Create HrEmployeeCompensationPackage records
-- =====================================================================
INSERT INTO hr_employee_compensation_package (
    employee_id,
    currency_id,
    effective_from,
    effective_to,
    comp_package_status_dict_code,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    employeeid AS employee_id,  -- OLD: employeeid
    NULL AS currency_id,  -- TODO: Map to default currency (e.g., SAR)
    COALESCE(hiredate, create_time) AS effective_from,
    NULL AS effective_to,
    1 AS comp_package_status_dict_code,  -- TODO: Map to "Active" status
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee
WHERE basicsalary IS NOT NULL  -- OLD: basicsalary
   OR housingallowance IS NOT NULL  -- OLD: housingallowance
   OR transportallowance IS NOT NULL  -- OLD: transportallowance
   OR foodallowance IS NOT NULL  -- OLD: foodallowance
   OR otherallowances IS NOT NULL;  -- OLD: otherallowances

-- =====================================================================
-- Step 13: Create HrEmployeeCompensationComponent records
-- =====================================================================
-- 13a: Basic Salary
INSERT INTO hr_employee_compensation_component (
    comp_package_id,
    component_type_dict_code,
    component_code_dict_code,
    amount,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    cp.comp_package_id,
    1 AS component_type_dict_code,  -- TODO: Map to "Earning" or "Base Pay"
    1 AS component_code_dict_code,  -- TODO: Map to "Basic Salary"
    e.basicsalary AS amount,  -- OLD: basicsalary
    e.create_by,
    e.create_time,
    e.update_by,
    e.update_time
FROM hr_employee e
JOIN hr_employee_compensation_package cp ON cp.employee_id = e.employeeid  -- OLD: employeeid
WHERE e.basicsalary IS NOT NULL AND e.basicsalary > 0;

-- 13b: Housing Allowance
INSERT INTO hr_employee_compensation_component (
    comp_package_id,
    component_type_dict_code,
    component_code_dict_code,
    amount,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    cp.comp_package_id,
    1 AS component_type_dict_code,  -- TODO: Map to "Earning" or "Allowance"
    2 AS component_code_dict_code,  -- TODO: Map to "Housing Allowance"
    e.housingallowance AS amount,  -- OLD: housingallowance
    e.create_by,
    e.create_time,
    e.update_by,
    e.update_time
FROM hr_employee e
JOIN hr_employee_compensation_package cp ON cp.employee_id = e.employeeid
WHERE e.housingallowance IS NOT NULL AND e.housingallowance > 0;

-- 13c: Transport Allowance
INSERT INTO hr_employee_compensation_component (
    comp_package_id,
    component_type_dict_code,
    component_code_dict_code,
    amount,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    cp.comp_package_id,
    1 AS component_type_dict_code,  -- TODO: Map to "Earning" or "Allowance"
    3 AS component_code_dict_code,  -- TODO: Map to "Transport Allowance"
    e.transportallowance AS amount,  -- OLD: transportallowance
    e.create_by,
    e.create_time,
    e.update_by,
    e.update_time
FROM hr_employee e
JOIN hr_employee_compensation_package cp ON cp.employee_id = e.employeeid
WHERE e.transportallowance IS NOT NULL AND e.transportallowance > 0;

-- 13d: Food Allowance
INSERT INTO hr_employee_compensation_component (
    comp_package_id,
    component_type_dict_code,
    component_code_dict_code,
    amount,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    cp.comp_package_id,
    1 AS component_type_dict_code,  -- TODO: Map to "Earning" or "Allowance"
    4 AS component_code_dict_code,  -- TODO: Map to "Food Allowance"
    e.foodallowance AS amount,  -- OLD: foodallowance
    e.create_by,
    e.create_time,
    e.update_by,
    e.update_time
FROM hr_employee e
JOIN hr_employee_compensation_package cp ON cp.employee_id = e.employeeid
WHERE e.foodallowance IS NOT NULL AND e.foodallowance > 0;

-- 13e: Other Allowances
INSERT INTO hr_employee_compensation_component (
    comp_package_id,
    component_type_dict_code,
    component_code_dict_code,
    amount,
    create_by,
    create_time,
    update_by,
    update_time
)
SELECT
    cp.comp_package_id,
    1 AS component_type_dict_code,  -- TODO: Map to "Earning" or "Allowance"
    5 AS component_code_dict_code,  -- TODO: Map to "Other Allowances"
    e.otherallowances AS amount,  -- OLD: otherallowances
    e.create_by,
    e.create_time,
    e.update_by,
    e.update_time
FROM hr_employee e
JOIN hr_employee_compensation_package cp ON cp.employee_id = e.employeeid
WHERE e.otherallowances IS NOT NULL AND e.otherallowances > 0;

-- =====================================================================
-- Step 14: Create HrEmployeeStatutory records
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
    employeeid AS employee_id,  -- OLD: employeeid
    NULL AS country_dict_code,  -- TODO: Map to Saudi Arabia dict code
    CASE
        WHEN gosinumber IS NOT NULL AND gosinumber != '' THEN true  -- OLD: gosinumber
        ELSE false
    END AS has_gosi,
    gosinumber AS gosi_number,  -- OLD: gosinumber
    issaudi AS is_saudi,  -- OLD: issaudi
    medicalinsurancenumber AS medical_insurance_number,  -- OLD: medicalinsurancenumber
    COALESCE(hiredate, create_time) AS effective_from,
    NULL AS effective_to,
    create_by,
    create_time,
    update_by,
    update_time
FROM hr_employee;

-- =====================================================================
-- Step 15: Create HrEmployeeEmergencyContact records
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
    employeeid AS employee_id,  -- OLD: employeeid
    emergencycontact AS contact_name,  -- OLD: emergencycontact
    NULL AS relationship_dict_code,  -- TODO: Parse or default to "Other"
    emergencyphone AS phone,  -- OLD: emergencyphone
    emergencycontactphone AS alternate_phone,  -- OLD: emergencycontactphone
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
-- Run these to verify migration results:

SELECT 'hr_person' AS table_name, COUNT(*) AS record_count FROM hr_person
UNION ALL
SELECT 'hr_employee (source)' AS table_name, COUNT(*) AS record_count FROM hr_employee;

SELECT 'hr_person_name' AS table_name, COUNT(*) AS record_count FROM hr_person_name;
SELECT 'hr_person_contact' AS table_name, COUNT(*) AS record_count FROM hr_person_contact;
SELECT 'hr_person_address' AS table_name, COUNT(*) AS record_count FROM hr_person_address;
SELECT 'hr_person_demographic' AS table_name, COUNT(*) AS record_count FROM hr_person_demographic;
SELECT 'hr_person_identifier' AS table_name, COUNT(*) AS record_count FROM hr_person_identifier;
SELECT 'hr_employee_core' AS table_name, COUNT(*) AS record_count FROM hr_employee_core;
SELECT 'hr_employee_contract' AS table_name, COUNT(*) AS record_count FROM hr_employee_contract;
SELECT 'hr_employee_assignment' AS table_name, COUNT(*) AS record_count FROM hr_employee_assignment;
SELECT 'hr_employee_status' AS table_name, COUNT(*) AS record_count FROM hr_employee_status;
SELECT 'hr_employee_bank_account' AS table_name, COUNT(*) AS record_count FROM hr_employee_bank_account;
SELECT 'hr_employee_compensation_package' AS table_name, COUNT(*) AS record_count FROM hr_employee_compensation_package;
SELECT 'hr_employee_compensation_component' AS table_name, COUNT(*) AS record_count FROM hr_employee_compensation_component;
SELECT 'hr_employee_statutory' AS table_name, COUNT(*) AS record_count FROM hr_employee_statutory;
SELECT 'hr_employee_emergency_contact' AS table_name, COUNT(*) AS record_count FROM hr_employee_emergency_contact;

-- Sample data comparison (for first employee)
SELECT
    e.employeeid AS old_id,
    e.employeecode AS old_code,
    ec.employee_code AS new_code,
    e.firstname || ' ' || e.lastname AS old_name,
    pn.given_name || ' ' || pn.family_name AS new_name
FROM hr_employee e
LEFT JOIN hr_employee_core ec ON ec.employee_id = e.employeeid
LEFT JOIN hr_person_name pn ON pn.person_id = e.employeeid
WHERE e.employeeid = (SELECT MIN(employeeid) FROM hr_employee)
LIMIT 1;

COMMIT;

-- =====================================================================
-- POST-MIGRATION TASKS (TODO)
-- =====================================================================
-- Same as original script - see MIGRATION_GUIDE.md
-- =====================================================================
