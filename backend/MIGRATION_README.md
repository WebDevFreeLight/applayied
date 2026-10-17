# Employee Migration Package

## Overview

This package contains all necessary scripts and documentation to migrate data from the legacy `hr_employee` table to the new normalized EmployeeCore structure.

## Package Contents

### 📄 Documentation Files

1. **MIGRATION_README.md** (this file)
   - Quick start guide
   - File descriptions
   - Execution order

2. **MIGRATION_GUIDE.md**
   - Comprehensive migration guide
   - Detailed field mappings
   - Pre/post migration checklists
   - Troubleshooting guide
   - Estimated timelines

### 🔧 SQL Scripts

3. **Migration_Setup_Dictionaries.sql**
   - Creates all required dictionary entries
   - **Run FIRST** before main migration
   - Sets up: contact types, identifier types, employment types, etc.

4. **Migration_HrEmployee_To_EmployeeCore.sql**
   - Main migration script
   - Migrates all employee data to new structure
   - **Run SECOND** after dictionaries are set up
   - Includes inline verification queries

5. **Migration_Verification.sql**
   - Comprehensive verification suite
   - **Run THIRD** after migration completes
   - Validates data integrity and accuracy

## Quick Start

### Step 1: Preparation (30-60 minutes)

```bash
# 1. Backup your database
pg_dump -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB > backup_$(date +%Y%m%d).sql

# 2. Review the MIGRATION_GUIDE.md
# Read sections:
# - Pre-Migration Checklist
# - Detailed Field Mapping
# - What's NOT migrated
```

### Step 2: Set Up Dictionaries (10-15 minutes)

```bash
# Run the dictionary setup script
psql -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB -f Migration_Setup_Dictionaries.sql

# Verify dictionaries were created
psql -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB -c "
SELECT dict_type, COUNT(*) FROM sys_dict_data
WHERE dict_type IN ('contact_type', 'identifier_type', 'gender',
                     'marital_status', 'employment_type')
GROUP BY dict_type;"
```

### Step 3: Test Migration (30-45 minutes)

```bash
# Run migration in a transaction (can rollback if issues)
psql -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB

-- In psql:
BEGIN;
\i Migration_HrEmployee_To_EmployeeCore.sql
-- Review the verification output at the end
-- If everything looks good:
COMMIT;
-- If issues found:
ROLLBACK;
```

### Step 4: Verify Migration (15-20 minutes)

```bash
# Run comprehensive verification
psql -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB -f Migration_Verification.sql > verification_report.txt

# Review the report
cat verification_report.txt | less
```

### Step 5: Post-Migration Tasks

See **MIGRATION_GUIDE.md** → Post-Migration Tasks section for:
- Dictionary code mapping updates
- Creating indexes
- Migrating unmigrated fields
- Application code updates

## Execution Order

```
┌─────────────────────────────────────┐
│  1. Read MIGRATION_GUIDE.md         │
│     - Understand what will happen   │
│     - Review field mappings         │
│     - Check pre-migration list      │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  2. Backup Database                 │
│     pg_dump > backup.sql            │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  3. Run Dictionary Setup            │
│     Migration_Setup_Dictionaries    │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  4. Run Main Migration (in txn)     │
│     Migration_HrEmployee_To_        │
│     EmployeeCore.sql                │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  5. Run Verification                │
│     Migration_Verification.sql      │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  6. Review Verification Report      │
│     - Fix any FAIL statuses         │
│     - Address WARNING items         │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  7. Post-Migration Tasks            │
│     - Update dictionary mappings    │
│     - Create indexes                │
│     - Migrate unmigrated fields     │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│  8. Application Updates             │
│     - Update services/repositories  │
│     - Test application              │
└─────────────────────────────────────┘
```

## What Gets Migrated

### ✅ Fully Migrated

| Old Table | New Tables |
|-----------|------------|
| hr_employee | → hr_person |
| | → hr_person_name |
| | → hr_person_contact (email, phone, mobile) |
| | → hr_person_address |
| | → hr_person_demographic |
| | → hr_person_identifier (ID, passport, visa) |
| | → hr_employee_core |
| | → hr_employee_contract |
| | → hr_employee_assignment |
| | → hr_employee_status |
| | → hr_employee_bank_account |
| | → hr_employee_compensation_package |
| | → hr_employee_compensation_component |
| | → hr_employee_statutory |
| | → hr_employee_emergency_contact |

### ⚠️ NOT Migrated (Manual Action Required)

- **BiometricId** - Add to HrEmployeeCore or create separate table
- **Education fields** - Create `hr_person_education` table
- **Work history** - Create `hr_person_work_history` table
- **RoleIds** (JSON) - Create `hr_employee_role` junction table
- **ProfilePicture** - Add to HrPerson or create document table
- **ShiftId** - Add to HrEmployeeAssignment

See **MIGRATION_GUIDE.md** → "Fields NOT Migrated" for details.

## Key Migration Concepts

### 1. Person vs Employee Separation

```
OLD: Everything in one table (hr_employee)

NEW: Separated into:
  - Person: Who someone is (name, demographics, contacts)
  - Employee: Their employment relationship (job, salary, contract)
```

### 2. Effective Dating

Many tables support historical tracking:

```sql
-- Example: Track salary changes over time
hr_employee_compensation_package
  - Package from 2023-01-01 to 2023-12-31 (old salary)
  - Package from 2024-01-01 to NULL (current salary)
```

### 3. Dictionary Codes

String values converted to dictionary codes:

```
OLD: status = "active"
NEW: employment_status_dict_code = 1

OLD: gender = "male"
NEW: gender_dict_code = 1
```

## Common Issues & Solutions

### Issue: "Dictionary codes not found"

**Solution:** Run `Migration_Setup_Dictionaries.sql` first

### Issue: "Duplicate employee codes"

**Solution:**
```sql
-- Find duplicates
SELECT employee_code, COUNT(*)
FROM hr_employee
GROUP BY employee_code
HAVING COUNT(*) > 1;

-- Clean up before migration
```

### Issue: "Compensation totals don't match"

**Check:**
- NULL values in old table (treated as 0)
- Decimal precision differences
- Missing compensation package records

### Issue: "Performance is slow after migration"

**Solution:** Create indexes (see MIGRATION_GUIDE.md → Post-Migration Tasks #5)

## Testing Checklist

After migration, verify:

- [ ] Record counts match (employees in = employees out)
- [ ] Sample employees spot-checked for accuracy
- [ ] All compensation amounts match
- [ ] Encrypted fields still encrypted
- [ ] No orphaned records (referential integrity)
- [ ] Application can read employee data
- [ ] Application can create new employees
- [ ] Application can update existing employees
- [ ] Reports show correct data

## Rollback Procedure

If you need to rollback:

```sql
-- 1. During migration (if in transaction)
ROLLBACK;

-- 2. After migration (if committed)
-- Truncate all new tables
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

-- 3. Restore from backup if needed
psql -h srv792437.hstgr.cloud -p 5433 -U your_user -d MahamDB < backup.sql
```

## Support

For issues or questions:

1. Review **MIGRATION_GUIDE.md** → Troubleshooting section
2. Check verification report for specific error details
3. Review inline TODO comments in migration script
4. Test queries in separate transaction first

## Timeline

**Typical migration timeline:**

| Phase | Duration | Notes |
|-------|----------|-------|
| Preparation | 2-4 hours | Read docs, review data, setup dictionaries |
| Test Migration | 1-2 hours | Run in test environment |
| Production Migration | 30-60 min | Actual data migration |
| Verification | 1-2 hours | Run checks, validate data |
| Post-Migration | 2-4 hours | Indexes, unmigrated fields |
| Application Updates | 4-16 hours | Update code to use new structure |
| Testing | 4-8 hours | Full application testing |

**Total: 14.5 - 36 hours** (depending on data volume and complexity)

## Important Notes

⚠️ **CRITICAL:**
- Always backup before migration
- Test in non-production environment first
- Run migration in a transaction so you can rollback
- Don't drop old table until verified and in production for 30+ days

📝 **REMEMBER:**
- Dictionary codes are placeholders - update with actual values
- Some fields need manual migration (education, work history, etc.)
- Encrypted fields are copied as-is (already encrypted)
- All effective-dated records use HireDate as starting date

🎯 **SUCCESS CRITERIA:**
- All employee counts match
- All compensation totals match (within 0.01 difference)
- No orphaned records
- Application works correctly
- Performance acceptable

## File Quick Reference

| File | Purpose | When to Use |
|------|---------|-------------|
| MIGRATION_GUIDE.md | Detailed documentation | Read FIRST for understanding |
| Migration_Setup_Dictionaries.sql | Create dictionary entries | Run 1st - before migration |
| Migration_HrEmployee_To_EmployeeCore.sql | Main migration | Run 2nd - migrates data |
| Migration_Verification.sql | Verify migration | Run 3rd - after migration |
| MIGRATION_README.md | Quick start guide | This file - overview |

---

**Ready to migrate?**

1. ✅ Read MIGRATION_GUIDE.md
2. ✅ Backup database
3. ✅ Run Migration_Setup_Dictionaries.sql
4. ✅ Run Migration_HrEmployee_To_EmployeeCore.sql (in transaction)
5. ✅ Run Migration_Verification.sql
6. ✅ Review and fix any issues
7. ✅ Complete post-migration tasks
8. ✅ Update application code
9. ✅ Test thoroughly

**Good luck! 🚀**
