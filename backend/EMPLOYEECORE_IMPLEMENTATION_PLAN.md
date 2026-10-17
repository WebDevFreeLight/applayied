# EmployeeCore Refactoring - Implementation Plan

## Overview
Complete the EmployeeCore refactoring by adding missing write operations, business logic, orchestration, and migration capabilities.

## Phase 1: Write DTOs (Create/Update)

### Person DTOs
- [x] `HrPersonQueryDto` (existing)
- [ ] `CreatePersonDto` - Create person with all related entities
- [ ] `UpdatePersonDto` - Update person
- [ ] `CreatePersonNameDto`
- [ ] `UpdatePersonNameDto`
- [ ] `CreatePersonDemographicDto`
- [ ] `UpdatePersonDemographicDto`
- [ ] `CreatePersonIdentifierDto`
- [ ] `UpdatePersonIdentifierDto`
- [ ] `CreatePersonContactDto`
- [ ] `UpdatePersonContactDto`
- [ ] `CreatePersonAddressDto`
- [ ] `UpdatePersonAddressDto`

### Employee DTOs
- [x] `HrEmployeeCoreQueryDto` (existing)
- [ ] `CreateEmployeeCoreDto`
- [ ] `UpdateEmployeeCoreDto`
- [ ] `CreateEmployeeAssignmentDto`
- [ ] `UpdateEmployeeAssignmentDto`
- [ ] `CreateEmployeeStatusDto`
- [ ] `UpdateEmployeeStatusDto`
- [ ] `CreateEmployeeContractDto`
- [ ] `UpdateEmployeeContractDto`
- [ ] `CreateEmployeeStatutoryDto`
- [ ] `UpdateEmployeeStatutoryDto`
- [ ] `CreateEmployeeBankAccountDto`
- [ ] `UpdateEmployeeBankAccountDto`
- [ ] `CreateEmployeeCompensationPackageDto`
- [ ] `UpdateEmployeeCompensationPackageDto`
- [ ] `CreateEmployeeCompensationComponentDto`
- [ ] `UpdateEmployeeCompensationComponentDto`
- [ ] `CreateEmployeeEmergencyContactDto`
- [ ] `UpdateEmployeeEmergencyContactDto`
- [ ] `CreateEmployeeDependentLinkDto`
- [ ] `UpdateEmployeeDependentLinkDto`

### Composite DTOs
- [ ] `CreateEmployeeCompleteDto` - Create entire employee hierarchy in one call
- [ ] `UpdateEmployeeCompleteDto` - Update entire employee hierarchy

## Phase 2: Enhanced Service Layer

### Business Logic to Add
1. **Validation Logic**
   - Required field validation
   - Email format validation
   - Employee code uniqueness
   - Effective date validation (effective_from < effective_to)
   - Primary flag validation (only one primary per type)

2. **Encryption Logic**
   - Bank account encryption/decryption
   - IBAN encryption/decryption
   - Sensitive data masking for API responses

3. **Effective Date Logic**
   - Auto-calculate effective_to when new record becomes primary
   - Ensure no overlapping effective date ranges
   - Get current effective record by date

4. **Primary Flag Management**
   - When setting a new primary, unset old primary
   - Ensure at least one primary exists for required types

5. **Transaction Management**
   - Multi-entity operations in single transaction
   - Rollback on failure

6. **User Account Integration**
   - Auto-create system user when employee is created
   - Link UserId to employee
   - Sync user status with employee status

### Services to Enhance
- [ ] `HrPersonService` - Add validation, transaction support
- [ ] `HrPersonNameService` - Add effective date logic, primary flag management
- [ ] `HrPersonContactService` - Add validation, effective dates, primary flags
- [ ] `HrPersonAddressService` - Add validation, effective dates, primary flags
- [ ] `HrPersonIdentifierService` - Add validation, expiry tracking
- [ ] `HrEmployeeCoreService` - Add employee code generation, user linking
- [ ] `HrEmployeeAssignmentService` - Add effective date logic, primary flags
- [ ] `HrEmployeeStatusService` - Add status transition validation
- [ ] `HrEmployeeContractService` - Add contract validation, effective dates
- [ ] `HrEmployeeBankAccountService` - Add encryption/decryption, masking
- [ ] `HrEmployeeCompensationPackageService` - Add effective date logic
- [ ] `HrEmployeeCompensationComponentService` - Add package validation

## Phase 3: Write Operations in Controllers

### Add to HrPersonController
- [ ] `POST /api/hr/employeecore/person` - Create person
- [ ] `PUT /api/hr/employeecore/person` - Update person
- [ ] `DELETE /api/hr/employeecore/person/{id}` - Delete person
- [ ] `POST /api/hr/employeecore/person/{id}/names` - Add name
- [ ] `PUT /api/hr/employeecore/person/names/{id}` - Update name
- [ ] `DELETE /api/hr/employeecore/person/names/{id}` - Delete name
- [ ] `POST /api/hr/employeecore/person/{id}/contacts` - Add contact
- [ ] `PUT /api/hr/employeecore/person/contacts/{id}` - Update contact
- [ ] `DELETE /api/hr/employeecore/person/contacts/{id}` - Delete contact
- [ ] `POST /api/hr/employeecore/person/{id}/addresses` - Add address
- [ ] `PUT /api/hr/employeecore/person/addresses/{id}` - Update address
- [ ] `DELETE /api/hr/employeecore/person/addresses/{id}` - Delete address
- [ ] `POST /api/hr/employeecore/person/{id}/identifiers` - Add identifier
- [ ] `PUT /api/hr/employeecore/person/identifiers/{id}` - Update identifier
- [ ] `DELETE /api/hr/employeecore/person/identifiers/{id}` - Delete identifier

### Add to HrEmployeeCoreController
- [ ] `POST /api/hr/employeecore/employee` - Create employee
- [ ] `PUT /api/hr/employeecore/employee` - Update employee
- [ ] `DELETE /api/hr/employeecore/employee/{id}` - Delete employee
- [ ] `POST /api/hr/employeecore/employee/{id}/assignments` - Add assignment
- [ ] `PUT /api/hr/employeecore/employee/assignments/{id}` - Update assignment
- [ ] `POST /api/hr/employeecore/employee/{id}/statuses` - Add status
- [ ] `POST /api/hr/employeecore/employee/{id}/contracts` - Add contract
- [ ] `PUT /api/hr/employeecore/employee/contracts/{id}` - Update contract

### Add to HrEmployeeCompensationController
- [ ] `POST /api/hr/employeecore/compensation/packages` - Create package
- [ ] `PUT /api/hr/employeecore/compensation/packages/{id}` - Update package
- [ ] `DELETE /api/hr/employeecore/compensation/packages/{id}` - Delete package
- [ ] `POST /api/hr/employeecore/compensation/packages/{id}/components` - Add component
- [ ] `PUT /api/hr/employeecore/compensation/components/{id}` - Update component
- [ ] `DELETE /api/hr/employeecore/compensation/components/{id}` - Delete component

### Add to HrEmployeeFamilyController
- [ ] `POST /api/hr/employeecore/family/emergency-contacts` - Add emergency contact
- [ ] `PUT /api/hr/employeecore/family/emergency-contacts/{id}` - Update emergency contact
- [ ] `DELETE /api/hr/employeecore/family/emergency-contacts/{id}` - Delete emergency contact
- [ ] `POST /api/hr/employeecore/family/dependents` - Link dependent
- [ ] `DELETE /api/hr/employeecore/family/dependents/{id}` - Unlink dependent

## Phase 4: Orchestration Service

### Create EmployeeCoreOrchestrationService
- [ ] `CreateCompleteEmployeeAsync(CreateEmployeeCompleteDto)` - Create person + employee + all related entities in one transaction
- [ ] `UpdateCompleteEmployeeAsync(UpdateEmployeeCompleteDto)` - Update entire employee hierarchy
- [ ] `TerminateEmployeeAsync(long employeeId, DateTime terminationDate, string reason)` - Terminate with all related updates
- [ ] `ActivateEmployeeAsync(long employeeId)` - Activate employee
- [ ] `TransferEmployeeAsync(long employeeId, long newDepartmentId, long newPositionId, DateTime effectiveDate)` - Transfer with new assignment
- [ ] `PromoteEmployeeAsync(long employeeId, long newPositionId, decimal newSalary, DateTime effectiveDate)` - Promotion with new compensation
- [ ] `UpdateEmployeeStatusAsync(long employeeId, long statusDictCode, DateTime effectiveDate, string reason)` - Status change with history

### Add Orchestration Controller
- [ ] `POST /api/hr/employeecore/orchestration/complete` - Create complete employee
- [ ] `PUT /api/hr/employeecore/orchestration/complete/{id}` - Update complete employee
- [ ] `POST /api/hr/employeecore/orchestration/terminate/{id}` - Terminate employee
- [ ] `POST /api/hr/employeecore/orchestration/activate/{id}` - Activate employee
- [ ] `POST /api/hr/employeecore/orchestration/transfer/{id}` - Transfer employee
- [ ] `POST /api/hr/employeecore/orchestration/promote/{id}` - Promote employee

## Phase 5: Data Migration

### Create EmployeeMigrationService
- [ ] `MigrateEmployeeToEmployeeCore(long oldEmployeeId)` - Migrate single employee from old to new
- [ ] `MigrateBatchAsync(long[] employeeIds)` - Batch migration
- [ ] `MigrateAllAsync()` - Migrate all employees
- [ ] `RollbackMigration(long personId)` - Rollback if needed
- [ ] `ValidateMigration(long oldEmployeeId, long newPersonId)` - Validate migration correctness

### Mapping Logic
```
HrEmployee → HrPerson + HrEmployeeCore + Related Entities

Fields Mapping:
- FirstName, LastName, NameAr → HrPersonName
- Email, Phone, PersonalEmail, MobileNumber → HrPersonContact
- Address → HrPersonAddress
- DateOfBirth, Gender, MaritalStatus, Nationality → HrPersonDemographic
- IdNumber, PassportNumber → HrPersonIdentifier
- EmployeeCode, HireDate, TerminationDate, EmploymentType → HrEmployeeCore
- DepartmentId, PositionId, ManagerId, WorkLocation → HrEmployeeAssignment
- Status → HrEmployeeStatus
- ContractStartDate, ContractEndDate → HrEmployeeContract
- GOSINumber, MedicalInsuranceNumber, IsSaudi → HrEmployeeStatutory
- BankAccount, IBAN, AccountNumber, BankName → HrEmployeeBankAccount
- BasicSalary, HousingAllowance, TransportAllowance, etc. → HrEmployeeCompensationPackage + Components
- EmergencyContact, EmergencyPhone → HrEmployeeEmergencyContact
```

## Phase 6: Error Handling & Validation

### Custom Exception Classes
- [ ] `EmployeeCoreException` - Base exception
- [ ] `EmployeeValidationException` - Validation errors
- [ ] `EmployeeNotFoundException` - Entity not found
- [ ] `EffectiveDateConflictException` - Date range conflicts
- [ ] `PrimaryFlagException` - Primary flag issues

### Validation Rules
- [ ] Required fields validation
- [ ] Format validation (email, phone, IBAN)
- [ ] Range validation (dates, amounts)
- [ ] Business rule validation (effective dates, primary flags)
- [ ] Referential integrity (person_id, employee_id exist)

## Phase 7: Testing & Validation

### Unit Tests
- [ ] Service layer tests (validation, business logic)
- [ ] Orchestration service tests
- [ ] Migration service tests

### Integration Tests
- [ ] End-to-end employee creation
- [ ] End-to-end employee update
- [ ] Effective date management
- [ ] Primary flag management
- [ ] Migration accuracy

### Manual Testing Checklist
- [ ] Create complete employee via orchestration endpoint
- [ ] Update employee details
- [ ] Add/update names, contacts, addresses
- [ ] Manage compensation packages
- [ ] Terminate and reactivate employee
- [ ] Migrate old employee to new structure
- [ ] Validate data integrity

## Implementation Order

1. **Day 1-2: DTOs** - Create all Write DTOs
2. **Day 3-4: Service Enhancement** - Add business logic to services
3. **Day 5-6: Controller Write Operations** - Add POST/PUT/DELETE endpoints
4. **Day 7-8: Orchestration** - Build orchestration service and composite endpoints
5. **Day 9: Migration** - Build migration service
6. **Day 10: Testing & Refinement** - Test everything, fix bugs

## Success Criteria

- ✅ All CRUD operations available for all entities
- ✅ Business logic properly implemented (validation, encryption, effective dates)
- ✅ Single endpoint to create/update complete employee
- ✅ Migration service can convert old to new structure
- ✅ No breaking changes to existing old HrEmployee API
- ✅ Proper error handling and validation messages
- ✅ Transaction safety (all-or-nothing operations)
- ✅ Documentation complete (API docs, migration guide)

## Notes

- Old `HrEmployee` system remains operational during implementation
- No breaking changes until migration is complete
- Run both systems in parallel during transition
- Frontend can choose to use old or new endpoints
- Gradual migration: critical employees first, then batch migration
