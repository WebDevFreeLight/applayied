## Employee Core (Phase 1) — Normalization Progress Report

### What was done (so far)

- **Designed and created new normalized HR tables (Employee Core domain only)** as additive DDL.
  - No existing tables were modified or deleted.
  - No triggers, views, or foreign keys were added (Phase 1 safe).
- **Aligned the new schema with existing ERP conventions**:
  - Added audit/multi-company fields (`create_by`, `create_time`, `update_by`, `update_time`, `remark`, `companyid`, `branchid`).
  - Added semantic comments:
    - `hr_person.is_active` = person entity existence/visibility (identity-level)
    - `hr_employee_core.is_active` = employment relationship activity (employment-level)
  - Added TODO comments (not enforced yet) for “single primary/current/active” rules.
- **Generated new SqlSugar models for every new Employee Core table**:
  - Inherit from `SysBase`
  - Use `[SugarTable]` and `[SugarColumn(ColumnName=...)]` mappings
  - Use `ValueToStringConverter` for BIGINT identifiers (to keep JSON/Excel behavior consistent)
  - No services/repositories/controllers were changed or created.

### Files added/updated

| Type | Path | Purpose |
|---|---|---|
| DDL (domain) | `backend/sql/20251220_0200_HR_Normalization_Phase1_employee_core.sql` | Creates new Employee Core tables + indexes + non-enforced checks + comments/TODOs |
| Models | `backend/ZR.Model/HR/EmployeeCore/*.cs` | SqlSugar entities for the new Employee Core tables |

### Employee Core — New tables and fields (as implemented in DDL)

| Table | Fields |
|---|---|
| `hr_person` | `person_id`, `is_active`, `companyid`, `branchid`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_person_name` | `person_name_id`, `person_id`, `name_type_dict_code`, `given_name`, `middle_name`, `family_name`, `full_name_ar`, `effective_from`, `effective_to`, `is_primary`, `companyid`, `branchid`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_person_demographic` | `person_id`, `date_of_birth`, `gender_dict_code`, `marital_status_dict_code`, `nationality_dict_code`, `companyid`, `branchid`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_person_identifier` | `person_identifier_id`, `person_id`, `identifier_type_dict_code`, `identifier_number`, `issuing_country_dict_code`, `issue_date`, `expiry_date`, `is_primary`, `is_verified`, `verified_time`, `companyid`, `branchid`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_person_contact` | `person_contact_id`, `person_id`, `contact_type_dict_code`, `contact_value`, `effective_from`, `effective_to`, `is_primary`, `is_verified`, `verified_time`, `companyid`, `branchid`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_person_address` | `person_address_id`, `person_id`, `address_type_dict_code`, `country_dict_code`, `city`, `postal_code`, `address_line_1`, `address_line_2`, `effective_from`, `effective_to`, `is_primary`, `companyid`, `branchid`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_core` | `employee_id`, `companyid`, `branchid`, `person_id`, `user_id`, `employee_code`, `hire_date`, `termination_date`, `employment_type_dict_code`, `is_active`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_assignment` | `employee_assignment_id`, `companyid`, `branchid`, `employee_id`, `department_id`, `position_id`, `manager_employee_id`, `work_location_id`, `effective_from`, `effective_to`, `is_primary`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_status` | `employee_status_id`, `companyid`, `branchid`, `employee_id`, `employment_status_dict_code`, `status_reason_dict_code`, `effective_from`, `effective_to`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_contract` | `employee_contract_id`, `companyid`, `branchid`, `employee_id`, `contract_type_dict_code`, `contract_start_date`, `contract_end_date`, `probation_end_date`, `effective_from`, `effective_to`, `is_current`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_statutory` | `employee_statutory_id`, `companyid`, `branchid`, `employee_id`, `country_dict_code`, `has_gosi`, `gosi_number`, `is_saudi`, `medical_insurance_number`, `effective_from`, `effective_to`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_bank_account` | `employee_bank_account_id`, `companyid`, `branchid`, `employee_id`, `bank_name`, `iban_encrypted`, `account_number_encrypted`, `bank_account_encrypted`, `currency_id`, `effective_from`, `effective_to`, `is_primary`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_compensation_package` | `comp_package_id`, `companyid`, `branchid`, `employee_id`, `currency_id`, `effective_from`, `effective_to`, `comp_package_status_dict_code`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_compensation_component` | `comp_component_id`, `comp_package_id`, `component_type_dict_code`, `component_code_dict_code`, `amount`, `companyid`, `branchid`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_emergency_contact` | `emergency_contact_id`, `companyid`, `branchid`, `employee_id`, `contact_name`, `relationship_dict_code`, `phone`, `alternate_phone`, `email`, `address`, `is_primary`, `is_active`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |
| `hr_employee_dependent_link` | `employee_dependent_link_id`, `companyid`, `branchid`, `employee_id`, `dependent_person_id`, `relationship_dict_code`, `effective_from`, `effective_to`, `is_active`, `create_by`, `create_time`, `update_by`, `update_time`, `remark` |

### Employee Core — New SqlSugar models (matching the new tables)

| Model class | Maps to table |
|---|---|
| `HrPerson` | `hr_person` |
| `HrPersonName` | `hr_person_name` |
| `HrPersonDemographic` | `hr_person_demographic` |
| `HrPersonIdentifier` | `hr_person_identifier` |
| `HrPersonContact` | `hr_person_contact` |
| `HrPersonAddress` | `hr_person_address` |
| `HrEmployeeCore` | `hr_employee_core` |
| `HrEmployeeAssignment` | `hr_employee_assignment` |
| `HrEmployeeStatus` | `hr_employee_status` |
| `HrEmployeeContract` | `hr_employee_contract` |
| `HrEmployeeStatutory` | `hr_employee_statutory` |
| `HrEmployeeBankAccount` | `hr_employee_bank_account` |
| `HrEmployeeCompensationPackage` | `hr_employee_compensation_package` |
| `HrEmployeeCompensationComponent` | `hr_employee_compensation_component` |
| `HrEmployeeEmergencyContact` | `hr_employee_emergency_contact` |
| `HrEmployeeDependentLink` | `hr_employee_dependent_link` |














