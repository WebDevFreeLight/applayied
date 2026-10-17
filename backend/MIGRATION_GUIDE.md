# Employee Migration Guide: hr_employee → EmployeeCore

## Overview

This guide explains the migration from the legacy `hr_employee` table to the new normalized EmployeeCore structure. The new structure provides:

- **Separation of Person and Employee concepts** - A person can exist without being an employee
- **Effective dating** - Track historical changes (names, addresses, assignments, compensation)
- **Better normalization** - Reduced data redundancy
- **Type safety** - Dictionary codes instead of strings
- **Security** - Consistent encryption for sensitive data

## Migration Architecture

### Data Flow Mapping

```
hr_employee (Legacy Single Table)
    ↓
    ├─→ hr_person (Core identity)
    ├─→ hr_person_name (Effective-dated names)
    ├─→ hr_person_contact (Effective-dated emails/phones)
    ├─→ hr_person_address (Effective-dated addresses)
    ├─→ hr_person_demographic (Demographics)
    ├─→ hr_person_identifier (IDs, passports, visas)
    ├─→ hr_employee_core (Employment anchor)
    ├─→ hr_employee_contract (Effective-dated contracts)
    ├─→ hr_employee_assignment (Effective-dated job assignments)
    ├─→ hr_employee_status (Effective-dated employment status)
    ├─→ hr_employee_bank_account (Effective-dated bank info)
    ├─→ hr_employee_compensation_package (Effective-dated packages)
    ├─→ hr_employee_compensation_component (Salary components)
    ├─→ hr_employee_statutory (GOSI, insurance)
    └─→ hr_employee_emergency_contact (Emergency contacts)
```

## Detailed Field Mapping

### 1. HrPerson (Core Identity)

| Old Field | New Field | Notes |
|-----------|-----------|-------|
| employee_id | person_id | Same ID used for person |
| status | is_active | Converted to boolean |

### 2. HrPersonName

| Old Field | New Field | Notes |
|-----------|-----------|-------|
| firstname | given_name | First name |
| lastname | family_name | Last name |
| namear | full_name_ar | Arabic name |
| - | middle_name | Not in old structure |
| - | effective_from | Uses hire_date |
| - | is_primary | Set to true |

### 3. HrPersonContact

Creates multiple records per employee:

| Old Field | New Field | Contact Type |
|-----------|-----------|--------------|
| email | contact_value | Work Email (type=1) |
| personalemail | contact_value | Personal Email (type=2) |
| phone | contact_value | Work Phone (type=3) |
| mobilenumber | contact_value | Mobile (type=4) |

### 4. HrPersonAddress

| Old Field | New Field | Notes |
|-----------|-----------|-------|
| address | address_line_1 | Full address as single line |
| - | city | TODO: Parse from address |
| - | postal_code | TODO: Parse from address |

### 5. HrPersonDemographic

| Old Field | New Field | Conversion |
|-----------|-----------|------------|
| dateofbirth | date_of_birth | Direct copy |
| gender | gender_dict_code | "male"→1, "female"→2 |
| maritalstatus | marital_status_dict_code | "single"→1, "married"→2, etc. |
| nationality | nationality_dict_code | TODO: Lookup dictionary |

### 6. HrPersonIdentifier

Creates multiple records per employee:

| Old Field | New Field | Identifier Type |
|-----------|-----------|-----------------|
| idnumber | identifier_number | National ID/Iqama (based on is_saudi) |
| passportnumber | identifier_number | Passport (type=3) |
| passportexpirydate | expiry_date | For passport |
| visanumber | identifier_number | Visa (type=4) |
| visaexpirydate | expiry_date | For visa |

### 7. HrEmployeeCore

| Old Field | New Field | Notes |
|-----------|-----------|-------|
| employee_id | employee_id | Direct copy |
| employee_id | person_id | Links to hr_person |
| userid | user_id | System user link |
| employeecode | employee_code | Employee code |
| hiredate | hire_date | Hiring date |
| terminationdate | termination_date | Termination date |
| employmenttype | employment_type_dict_code | "full-time"→1, etc. |
| status | is_active | Boolean conversion |

### 8. HrEmployeeContract

| Old Field | New Field | Notes |
|-----------|-----------|-------|
| contractstartdate | contract_start_date | Contract start |
| contractenddate | contract_end_date | Contract end |
| - | probation_end_date | TODO: Calculate if needed |
| contractstartdate | effective_from | Effective dating |
| - | is_current | Based on end date |

### 9. HrEmployeeAssignment

| Old Field | New Field | Notes |
|-----------|-----------|-------|
| departmentid | department_id | Department assignment |
| positionid | position_id | Position/job title |
| managerid | manager_employee_id | Reports to |
| worklocation | work_location_id | TODO: Create lookup |
| hiredate | effective_from | Initial assignment date |

### 10. HrEmployeeStatus

| Old Field | New Field | Conversion |
|-----------|-----------|------------|
| status | employment_status_dict_code | "active"→1, "inactive"→2, "terminated"→3 |
| hiredate | effective_from | Status start |
| terminationdate | effective_to | Status end (if terminated) |

### 11. HrEmployeeBankAccount

| Old Field | New Field | Notes |
|-----------|-----------|-------|
| bankname | bank_name | Bank name |
| iban_encrypted | iban_encrypted | Already encrypted |
| accountnumber_encrypted | account_number_encrypted | Already encrypted |
| bankaccount_encrypted | bank_account_encrypted | Already encrypted |
| - | currency_id | TODO: Default to SAR |

### 12. HrEmployeeCompensationPackage & Components

Creates one package per employee, then multiple components:

**Package:**
- One record per employee with compensation

**Components (separate records):**
- basicsalary → Component (type=1, code=1)
- housingallowance → Component (type=1, code=2)
- transportallowance → Component (type=1, code=3)
- foodallowance → Component (type=1, code=4)
- otherallowances → Component (type=1, code=5)

### 13. HrEmployeeStatutory

| Old Field | New Field | Notes |
|-----------|-----------|-------|
| gosinumber | gosi_number | GOSI number |
| - | has_gosi | Derived from gosi_number presence |
| issaudi | is_saudi | Boolean flag |
| medicalinsurancenumber | medical_insurance_number | Insurance number |

### 14. HrEmployeeEmergencyContact

| Old Field | New Field | Notes |
|-----------|-----------|-------|
| emergencycontact | contact_name | Contact name |
| emergencyphone | phone | Primary phone |
| emergencycontactphone | alternate_phone | Secondary phone |

## Fields NOT Migrated

These fields exist in the old structure but are not included in the new EmployeeCore tables:

1. **BiometricId** - Consider adding to HrEmployeeCore or create separate table
2. **Education Fields:**
   - EducationLevel
   - FieldOfStudy
   - University
   - GraduationYear
   - *Recommendation: Create `hr_person_education` table*

3. **Experience Fields:**
   - PreviousCompany
   - YearsOfExperience
   - *Recommendation: Create `hr_person_work_history` table*

4. **RoleIds** (JSON array) - *Recommendation: Create `hr_employee_role` junction table*

5. **ProfilePicture** - *Recommendation: Add to HrPerson or create `hr_person_document` table*

6. **GLAccountId** - Consider adding to HrEmployeeCore for payroll GL posting

7. **ShiftId** - Consider adding to HrEmployeeAssignment

8. **JoiningDate vs HireDate** - Script uses HireDate; verify if different

## Pre-Migration Checklist

### 1. Create Dictionary Entries

Before running the migration, ensure these dictionaries exist:

**Contact Types:**
- [ ] 1 = "Work Email"
- [ ] 2 = "Personal Email"
- [ ] 3 = "Work Phone"
- [ ] 4 = "Mobile Phone"

**Identifier Types:**
- [ ] 1 = "National ID" (Saudi)
- [ ] 2 = "Iqama" (Non-Saudi)
- [ ] 3 = "Passport"
- [ ] 4 = "Visa"

**Gender:**
- [ ] 1 = "Male"
- [ ] 2 = "Female"

**Marital Status:**
- [ ] 1 = "Single"
- [ ] 2 = "Married"
- [ ] 3 = "Divorced"
- [ ] 4 = "Widowed"

**Employment Type:**
- [ ] 1 = "Full-Time"
- [ ] 2 = "Part-Time"
- [ ] 3 = "Contract"
- [ ] 4 = "Temporary"

**Employment Status:**
- [ ] 1 = "Active"
- [ ] 2 = "Inactive"
- [ ] 3 = "Terminated"
- [ ] 4 = "Suspended"

**Compensation Component Types:**
- [ ] 1 = "Earning/Base Pay"
- [ ] 2 = "Allowance"

**Compensation Component Codes:**
- [ ] 1 = "Basic Salary"
- [ ] 2 = "Housing Allowance"
- [ ] 3 = "Transport Allowance"
- [ ] 4 = "Food Allowance"
- [ ] 5 = "Other Allowances"

**Other Dictionaries:**
- [ ] Nationality codes
- [ ] Country codes (especially Saudi Arabia)
- [ ] Address types
- [ ] Relationship types (for emergency contacts)
- [ ] Contract types

### 2. Database Preparation

```sql
-- Create backup of hr_employee table
CREATE TABLE hr_employee_backup_20251225 AS SELECT * FROM hr_employee;

-- Verify all EmployeeCore tables exist
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'hr_%'
ORDER BY table_name;

-- Check for existing data (should be empty before migration)
SELECT 'hr_person' AS tbl, COUNT(*) FROM hr_person
UNION ALL SELECT 'hr_employee_core', COUNT(*) FROM hr_employee_core;
```

### 3. Review Data Quality

```sql
-- Check for NULL employee codes
SELECT COUNT(*) FROM hr_employee WHERE employeecode IS NULL;

-- Check for NULL hire dates
SELECT COUNT(*) FROM hr_employee WHERE hiredate IS NULL;

-- Check for invalid statuses
SELECT DISTINCT status FROM hr_employee;

-- Check for invalid employment types
SELECT DISTINCT employmenttype FROM hr_employee;
```

## Running the Migration

### Step 1: Test Migration (Recommended)

```sql
-- Start a transaction
BEGIN;

-- Run the migration script
\i Migration_HrEmployee_To_EmployeeCore.sql

-- Review verification queries at the end of the script

-- If everything looks good, COMMIT
-- If issues found, ROLLBACK
ROLLBACK;  -- or COMMIT;
```

### Step 2: Production Migration

```bash
# Backup database first
pg_dump -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB > backup_before_migration.sql

# Run migration
psql -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB -f Migration_HrEmployee_To_EmployeeCore.sql
```

## Post-Migration Tasks

### 1. Update Dictionary Mappings

Search for `TODO` comments in the migration script and update with actual dictionary codes:

```sql
-- Example: Update gender dict codes
UPDATE hr_person_demographic pd
SET gender_dict_code = (
    SELECT dict_code FROM sys_dict_data
    WHERE dict_type = 'gender' AND dict_value = 'male'
)
WHERE pd.gender_dict_code = 1;
```

### 2. Create Missing Tables

Consider creating these tables for unmigrated data:

```sql
-- Education history
CREATE TABLE hr_person_education (
    education_id BIGSERIAL PRIMARY KEY,
    person_id BIGINT NOT NULL REFERENCES hr_person(person_id),
    education_level_dict_code BIGINT,
    field_of_study VARCHAR(100),
    institution VARCHAR(200),
    graduation_year INT,
    effective_from TIMESTAMP,
    effective_to TIMESTAMP,
    is_primary BOOLEAN DEFAULT false
);

-- Work history
CREATE TABLE hr_person_work_history (
    work_history_id BIGSERIAL PRIMARY KEY,
    person_id BIGINT NOT NULL REFERENCES hr_person(person_id),
    company_name VARCHAR(200),
    position VARCHAR(100),
    start_date DATE,
    end_date DATE,
    years_of_experience INT
);

-- Biometric data
ALTER TABLE hr_employee_core ADD COLUMN biometric_id VARCHAR(50);

-- Shift assignment (add to assignment table)
ALTER TABLE hr_employee_assignment ADD COLUMN shift_id BIGINT;
```

### 3. Migrate Unmigrated Fields

```sql
-- Migrate biometric IDs
UPDATE hr_employee_core ec
SET biometric_id = e.biometricid
FROM hr_employee e
WHERE ec.employee_id = e.employee_id;

-- Migrate education data
INSERT INTO hr_person_education (person_id, education_level_dict_code, field_of_study, institution, graduation_year)
SELECT employee_id, NULL, fieldofstudy, university, graduationyear
FROM hr_employee
WHERE university IS NOT NULL;

-- Migrate shift IDs
UPDATE hr_employee_assignment ea
SET shift_id = e.shiftid
FROM hr_employee e
WHERE ea.employee_id = e.employee_id;
```

### 4. Update Sequences

```sql
-- Update auto-increment sequences to avoid ID conflicts
SELECT setval('hr_person_person_id_seq', (SELECT MAX(person_id) FROM hr_person));
SELECT setval('hr_employee_core_employee_id_seq', (SELECT MAX(employee_id) FROM hr_employee_core));
-- Repeat for all tables with auto-increment IDs
```

### 5. Create Indexes

```sql
-- Person indexes
CREATE INDEX idx_person_name_person_id ON hr_person_name(person_id);
CREATE INDEX idx_person_contact_person_id ON hr_person_contact(person_id);
CREATE INDEX idx_person_address_person_id ON hr_person_address(person_id);
CREATE INDEX idx_person_identifier_person_id ON hr_person_identifier(person_id);

-- Employee indexes
CREATE INDEX idx_employee_core_person_id ON hr_employee_core(person_id);
CREATE INDEX idx_employee_core_user_id ON hr_employee_core(user_id);
CREATE INDEX idx_employee_core_employee_code ON hr_employee_core(employee_code);
CREATE INDEX idx_employee_assignment_employee_id ON hr_employee_assignment(employee_id);
CREATE INDEX idx_employee_assignment_department_id ON hr_employee_assignment(department_id);
CREATE INDEX idx_employee_assignment_position_id ON hr_employee_assignment(position_id);
CREATE INDEX idx_employee_bank_employee_id ON hr_employee_bank_account(employee_id);
CREATE INDEX idx_employee_comp_package_employee_id ON hr_employee_compensation_package(employee_id);
CREATE INDEX idx_employee_comp_component_package_id ON hr_employee_compensation_component(comp_package_id);
```

### 6. Verify Data Integrity

```sql
-- Check all employees have person records
SELECT COUNT(*) FROM hr_employee_core ec
LEFT JOIN hr_person p ON p.person_id = ec.person_id
WHERE p.person_id IS NULL;

-- Check all employees have names
SELECT COUNT(*) FROM hr_employee_core ec
LEFT JOIN hr_person_name pn ON pn.person_id = ec.person_id
WHERE pn.person_name_id IS NULL;

-- Check compensation package integrity
SELECT COUNT(*) FROM hr_employee_compensation_component cc
LEFT JOIN hr_employee_compensation_package cp ON cp.comp_package_id = cc.comp_package_id
WHERE cp.comp_package_id IS NULL;

-- Verify total compensation matches
SELECT
    e.employee_id,
    e.employeecode,
    (COALESCE(e.basicsalary, 0) + COALESCE(e.housingallowance, 0) +
     COALESCE(e.transportallowance, 0) + COALESCE(e.foodallowance, 0) +
     COALESCE(e.otherallowances, 0)) AS old_total,
    (SELECT SUM(amount) FROM hr_employee_compensation_component cc
     JOIN hr_employee_compensation_package cp ON cp.comp_package_id = cc.comp_package_id
     WHERE cp.employee_id = e.employee_id) AS new_total
FROM hr_employee e
WHERE ABS(
    (COALESCE(e.basicsalary, 0) + COALESCE(e.housingallowance, 0) +
     COALESCE(e.transportallowance, 0) + COALESCE(e.foodallowance, 0) +
     COALESCE(e.otherallowances, 0)) -
    COALESCE((SELECT SUM(amount) FROM hr_employee_compensation_component cc
     JOIN hr_employee_compensation_package cp ON cp.comp_package_id = cc.comp_package_id
     WHERE cp.employee_id = e.employee_id), 0)
) > 0.01;
```

## Application Updates Required

### 1. Update Repository/Service Layer

Update services to query new tables:

```csharp
// Old approach
var employee = await _db.Queryable<HrEmployee>()
    .Where(e => e.EmployeeCode == code)
    .FirstAsync();

// New approach - join multiple tables
var employee = await _db.Queryable<HrEmployeeCore>()
    .LeftJoin<HrPerson>((ec, p) => ec.PersonId == p.PersonId)
    .LeftJoin<HrPersonName>((ec, p, pn) => ec.PersonId == pn.PersonId && pn.IsPrimary)
    .LeftJoin<HrEmployeeAssignment>((ec, p, pn, ea) => ec.EmployeeId == ea.EmployeeId && ea.IsPrimary)
    .Where((ec, p, pn, ea) => ec.EmployeeCode == code)
    .Select((ec, p, pn, ea) => new EmployeeDto {
        EmployeeId = ec.EmployeeId,
        EmployeeCode = ec.EmployeeCode,
        FirstName = pn.GivenName,
        LastName = pn.FamilyName,
        DepartmentId = ea.DepartmentId,
        // ... other fields
    })
    .FirstAsync();
```

### 2. Create DTOs and View Models

Create comprehensive DTOs that aggregate data from multiple tables.

### 3. Update API Endpoints

Update controllers to work with new structure while maintaining backward compatibility if needed.

## Rollback Plan

If migration fails or issues are discovered:

```sql
-- 1. Drop all migrated data
TRUNCATE TABLE hr_employee_emergency_contact CASCADE;
TRUNCATE TABLE hr_employee_compensation_component CASCADE;
TRUNCATE TABLE hr_employee_compensation_package CASCADE;
TRUNCATE TABLE hr_employee_statutory CASCADE;
TRUNCATE TABLE hr_employee_bank_account CASCADE;
TRUNCATE TABLE hr_employee_status CASCADE;
TRUNCATE TABLE hr_employee_assignment CASCADE;
TRUNCATE TABLE hr_employee_contract CASCADE;
TRUNCATE TABLE hr_employee_core CASCADE;
TRUNCATE TABLE hr_person_identifier CASCADE;
TRUNCATE TABLE hr_person_demographic CASCADE;
TRUNCATE TABLE hr_person_address CASCADE;
TRUNCATE TABLE hr_person_contact CASCADE;
TRUNCATE TABLE hr_person_name CASCADE;
TRUNCATE TABLE hr_person CASCADE;

-- 2. Restore from backup if needed
-- psql -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB < backup_before_migration.sql
```

## Support and Troubleshooting

### Common Issues

**Issue: Dictionary codes not found**
- Solution: Create missing dictionary entries before migration

**Issue: Duplicate employee codes**
- Solution: Clean up duplicates in source data first

**Issue: NULL hire dates**
- Solution: Use create_time as fallback or set a default date

**Issue: Performance degradation**
- Solution: Ensure all indexes are created (see Post-Migration Tasks #5)

### Testing Checklist

- [ ] All employee records migrated (count matches)
- [ ] Sample employees spot-checked for accuracy
- [ ] Dictionary codes properly mapped
- [ ] Encrypted fields still encrypted
- [ ] Effective dating setup correctly
- [ ] Primary flags set appropriately
- [ ] Foreign key relationships valid
- [ ] Application can read new structure
- [ ] Application can write to new structure
- [ ] Reporting queries updated and working

## Timeline Estimate

- **Preparation**: 2-4 hours (dictionaries, data cleanup)
- **Test migration**: 1-2 hours
- **Production migration**: 30-60 minutes
- **Post-migration tasks**: 2-4 hours
- **Application updates**: Varies (4-16 hours depending on complexity)
- **Testing**: 4-8 hours

**Total estimated time**: 13.5 - 34 hours

## Success Criteria

✅ All employees from `hr_employee` have corresponding records in new tables
✅ No data loss (all fields either migrated or documented as not applicable)
✅ Dictionary codes properly mapped
✅ Referential integrity maintained
✅ Application functions correctly with new structure
✅ Performance acceptable (queries < 2x old performance)
✅ Old table backed up and ready for archival

---

**Document Version**: 1.0
**Last Updated**: 2025-12-25
**Author**: Claude Code Migration Assistant
