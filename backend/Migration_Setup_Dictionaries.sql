-- =====================================================================
-- Dictionary Setup for Employee Migration
-- =====================================================================
-- Purpose: Create all required dictionary entries for EmployeeCore migration
-- Run this BEFORE running the main migration script
-- =====================================================================

-- Note: Adjust dict_type values to match your existing dictionary structure
-- These are example values - you may need to customize

BEGIN;

-- =====================================================================
-- 1. Contact Type Dictionary
-- =====================================================================
-- Create dictionary type if doesn't exist
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Contact Type',
    'contact_type',
    '0',  -- Active
    'Types of contact information (email, phone, etc.)',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

-- Create dictionary data
INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Work Email', 'work_email', 'contact_type', '', 'default', 'Y', '0', 'Primary work email address', NOW()),
    (2, 2, 'Personal Email', 'personal_email', 'contact_type', '', 'default', 'N', '0', 'Personal email address', NOW()),
    (3, 3, 'Work Phone', 'work_phone', 'contact_type', '', 'default', 'N', '0', 'Work phone number', NOW()),
    (4, 4, 'Mobile Phone', 'mobile_phone', 'contact_type', '', 'default', 'N', '0', 'Mobile/cell phone number', NOW()),
    (5, 5, 'Home Phone', 'home_phone', 'contact_type', '', 'default', 'N', '0', 'Home phone number', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 2. Identifier Type Dictionary
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Identifier Type',
    'identifier_type',
    '0',
    'Types of personal identifiers (ID, passport, visa)',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'National ID', 'national_id', 'identifier_type', '', 'primary', 'Y', '0', 'National ID for Saudi citizens', NOW()),
    (2, 2, 'Iqama', 'iqama', 'identifier_type', '', 'info', 'N', '0', 'Residency permit for non-Saudis', NOW()),
    (3, 3, 'Passport', 'passport', 'identifier_type', '', 'default', 'N', '0', 'International passport', NOW()),
    (4, 4, 'Visa', 'visa', 'identifier_type', '', 'default', 'N', '0', 'Entry/work visa', NOW()),
    (5, 5, 'Border Number', 'border_number', 'identifier_type', '', 'default', 'N', '0', 'Border crossing number', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 3. Gender Dictionary (if not exists)
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Gender',
    'gender',
    '0',
    'Gender types',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Male', 'male', 'gender', '', 'primary', 'Y', '0', 'Male', NOW()),
    (2, 2, 'Female', 'female', 'gender', '', 'danger', 'N', '0', 'Female', NOW()),
    (3, 3, 'Other', 'other', 'gender', '', 'default', 'N', '0', 'Other/Prefer not to say', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 4. Marital Status Dictionary
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Marital Status',
    'marital_status',
    '0',
    'Marital status types',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Single', 'single', 'marital_status', '', 'info', 'Y', '0', 'Never married', NOW()),
    (2, 2, 'Married', 'married', 'marital_status', '', 'success', 'N', '0', 'Currently married', NOW()),
    (3, 3, 'Divorced', 'divorced', 'marital_status', '', 'warning', 'N', '0', 'Divorced', NOW()),
    (4, 4, 'Widowed', 'widowed', 'marital_status', '', 'default', 'N', '0', 'Widowed', NOW()),
    (5, 5, 'Separated', 'separated', 'marital_status', '', 'default', 'N', '0', 'Legally separated', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 5. Employment Type Dictionary
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Employment Type',
    'employment_type',
    '0',
    'Types of employment (full-time, part-time, etc.)',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Full-Time', 'full_time', 'employment_type', '', 'primary', 'Y', '0', 'Full-time permanent employee', NOW()),
    (2, 2, 'Part-Time', 'part_time', 'employment_type', '', 'info', 'N', '0', 'Part-time employee', NOW()),
    (3, 3, 'Contract', 'contract', 'employment_type', '', 'warning', 'N', '0', 'Fixed-term contract', NOW()),
    (4, 4, 'Temporary', 'temporary', 'employment_type', '', 'default', 'N', '0', 'Temporary/seasonal employee', NOW()),
    (5, 5, 'Intern', 'intern', 'employment_type', '', 'default', 'N', '0', 'Internship/trainee', NOW()),
    (6, 6, 'Consultant', 'consultant', 'employment_type', '', 'default', 'N', '0', 'External consultant', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 6. Employment Status Dictionary
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Employment Status',
    'employment_status',
    '0',
    'Current employment status',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Active', 'active', 'employment_status', '', 'success', 'Y', '0', 'Currently employed and active', NOW()),
    (2, 2, 'Inactive', 'inactive', 'employment_status', '', 'warning', 'N', '0', 'Employed but inactive (leave, etc.)', NOW()),
    (3, 3, 'Terminated', 'terminated', 'employment_status', '', 'danger', 'N', '0', 'Employment terminated', NOW()),
    (4, 4, 'Suspended', 'suspended', 'employment_status', '', 'danger', 'N', '0', 'Suspended', NOW()),
    (5, 5, 'On Leave', 'on_leave', 'employment_status', '', 'info', 'N', '0', 'On extended leave', NOW()),
    (6, 6, 'Resigned', 'resigned', 'employment_status', '', 'default', 'N', '0', 'Resigned voluntarily', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 7. Compensation Component Type Dictionary
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Compensation Component Type',
    'comp_component_type',
    '0',
    'Types of compensation components (earning, deduction, etc.)',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Earning', 'earning', 'comp_component_type', '', 'success', 'Y', '0', 'Earnings/payments to employee', NOW()),
    (2, 2, 'Deduction', 'deduction', 'comp_component_type', '', 'danger', 'N', '0', 'Deductions from salary', NOW()),
    (3, 3, 'Benefit', 'benefit', 'comp_component_type', '', 'info', 'N', '0', 'Non-cash benefits', NOW()),
    (4, 4, 'Employer Contribution', 'employer_contribution', 'comp_component_type', '', 'primary', 'N', '0', 'Employer-paid contributions', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 8. Compensation Component Code Dictionary
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Compensation Component Code',
    'comp_component_code',
    '0',
    'Specific compensation component codes',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    -- Earnings
    (1, 1, 'Basic Salary', 'basic_salary', 'comp_component_code', '', 'primary', 'Y', '0', 'Base salary component', NOW()),
    (2, 2, 'Housing Allowance', 'housing_allowance', 'comp_component_code', '', 'success', 'N', '0', 'Housing allowance', NOW()),
    (3, 3, 'Transport Allowance', 'transport_allowance', 'comp_component_code', '', 'success', 'N', '0', 'Transportation allowance', NOW()),
    (4, 4, 'Food Allowance', 'food_allowance', 'comp_component_code', '', 'success', 'N', '0', 'Food/meal allowance', NOW()),
    (5, 5, 'Other Allowances', 'other_allowances', 'comp_component_code', '', 'success', 'N', '0', 'Miscellaneous allowances', NOW()),
    (6, 6, 'Overtime', 'overtime', 'comp_component_code', '', 'success', 'N', '0', 'Overtime pay', NOW()),
    (7, 7, 'Bonus', 'bonus', 'comp_component_code', '', 'success', 'N', '0', 'Performance bonus', NOW()),
    (8, 8, 'Commission', 'commission', 'comp_component_code', '', 'success', 'N', '0', 'Sales commission', NOW()),
    -- Deductions
    (101, 101, 'GOSI Employee', 'gosi_employee', 'comp_component_code', '', 'danger', 'N', '0', 'GOSI employee contribution', NOW()),
    (102, 102, 'Income Tax', 'income_tax', 'comp_component_code', '', 'danger', 'N', '0', 'Income tax deduction', NOW()),
    (103, 103, 'Loan Repayment', 'loan_repayment', 'comp_component_code', '', 'danger', 'N', '0', 'Employee loan repayment', NOW()),
    (104, 104, 'Insurance Premium', 'insurance_premium', 'comp_component_code', '', 'danger', 'N', '0', 'Insurance premium deduction', NOW()),
    (105, 105, 'Absence', 'absence', 'comp_component_code', '', 'danger', 'N', '0', 'Deduction for absence', NOW()),
    -- Employer Contributions
    (201, 201, 'GOSI Employer', 'gosi_employer', 'comp_component_code', '', 'info', 'N', '0', 'GOSI employer contribution', NOW()),
    (202, 202, 'Medical Insurance', 'medical_insurance', 'comp_component_code', '', 'info', 'N', '0', 'Employer-paid medical insurance', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 9. Address Type Dictionary
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Address Type',
    'address_type',
    '0',
    'Types of addresses',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Residential', 'residential', 'address_type', '', 'primary', 'Y', '0', 'Primary residential address', NOW()),
    (2, 2, 'Mailing', 'mailing', 'address_type', '', 'info', 'N', '0', 'Mailing/postal address', NOW()),
    (3, 3, 'Work', 'work', 'address_type', '', 'default', 'N', '0', 'Work/office address', NOW()),
    (4, 4, 'Emergency', 'emergency', 'address_type', '', 'warning', 'N', '0', 'Emergency contact address', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 10. Relationship Type Dictionary (for emergency contacts/dependents)
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Relationship Type',
    'relationship_type',
    '0',
    'Types of relationships (for emergency contacts, dependents)',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Spouse', 'spouse', 'relationship_type', '', 'primary', 'Y', '0', 'Husband/Wife', NOW()),
    (2, 2, 'Parent', 'parent', 'relationship_type', '', 'info', 'N', '0', 'Mother/Father', NOW()),
    (3, 3, 'Child', 'child', 'relationship_type', '', 'success', 'N', '0', 'Son/Daughter', NOW()),
    (4, 4, 'Sibling', 'sibling', 'relationship_type', '', 'default', 'N', '0', 'Brother/Sister', NOW()),
    (5, 5, 'Grandparent', 'grandparent', 'relationship_type', '', 'default', 'N', '0', 'Grandfather/Grandmother', NOW()),
    (6, 6, 'Grandchild', 'grandchild', 'relationship_type', '', 'default', 'N', '0', 'Grandson/Granddaughter', NOW()),
    (7, 7, 'Friend', 'friend', 'relationship_type', '', 'default', 'N', '0', 'Friend', NOW()),
    (8, 8, 'Other', 'other', 'relationship_type', '', 'default', 'N', '0', 'Other relationship', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 11. Contract Type Dictionary
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Contract Type',
    'contract_type',
    '0',
    'Types of employment contracts',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Permanent', 'permanent', 'contract_type', '', 'success', 'Y', '0', 'Permanent/indefinite contract', NOW()),
    (2, 2, 'Fixed-Term', 'fixed_term', 'contract_type', '', 'warning', 'N', '0', 'Fixed-term contract', NOW()),
    (3, 3, 'Probation', 'probation', 'contract_type', '', 'info', 'N', '0', 'Probationary contract', NOW()),
    (4, 4, 'Seasonal', 'seasonal', 'contract_type', '', 'default', 'N', '0', 'Seasonal contract', NOW()),
    (5, 5, 'Project-Based', 'project_based', 'contract_type', '', 'default', 'N', '0', 'Project-specific contract', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- 12. Compensation Package Status Dictionary
-- =====================================================================
INSERT INTO sys_dict_type (dict_id, dict_name, dict_type, status, remark, create_time)
VALUES (
    nextval('sys_dict_type_dict_id_seq'),
    'Compensation Package Status',
    'comp_package_status',
    '0',
    'Status of compensation packages',
    NOW()
)
ON CONFLICT (dict_type) DO NOTHING;

INSERT INTO sys_dict_data (dict_code, dict_sort, dict_label, dict_value, dict_type, css_class, list_class, is_default, status, remark, create_time)
VALUES
    (1, 1, 'Active', 'active', 'comp_package_status', '', 'success', 'Y', '0', 'Currently active package', NOW()),
    (2, 2, 'Pending Approval', 'pending_approval', 'comp_package_status', '', 'warning', 'N', '0', 'Awaiting approval', NOW()),
    (3, 3, 'Approved', 'approved', 'comp_package_status', '', 'info', 'N', '0', 'Approved but not yet active', NOW()),
    (4, 4, 'Expired', 'expired', 'comp_package_status', '', 'default', 'N', '0', 'No longer active', NOW()),
    (5, 5, 'Rejected', 'rejected', 'comp_package_status', '', 'danger', 'N', '0', 'Rejected/cancelled', NOW())
ON CONFLICT (dict_code) DO NOTHING;

-- =====================================================================
-- Verification Queries
-- =====================================================================

SELECT 'Dictionary Types Created:' AS info;
SELECT dict_type, dict_name, COUNT(*) AS entry_count
FROM sys_dict_data d
JOIN sys_dict_type t ON t.dict_type = d.dict_type
WHERE t.dict_type IN (
    'contact_type', 'identifier_type', 'gender', 'marital_status',
    'employment_type', 'employment_status', 'comp_component_type',
    'comp_component_code', 'address_type', 'relationship_type',
    'contract_type', 'comp_package_status'
)
GROUP BY d.dict_type, t.dict_name
ORDER BY d.dict_type;

COMMIT;

-- =====================================================================
-- Next Steps:
-- 1. Review the created dictionary entries
-- 2. Adjust dict_code values if they conflict with existing codes
-- 3. Add Arabic translations if needed
-- 4. Run the main migration script: Migration_HrEmployee_To_EmployeeCore.sql
-- =====================================================================
