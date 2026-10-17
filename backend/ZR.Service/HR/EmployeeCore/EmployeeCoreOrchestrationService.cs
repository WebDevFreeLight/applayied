using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Model.System;
using ZR.Service.HR.IService;
using ZR.ServiceCore.Security;
using ZR.ServiceCore.Services;

namespace ZR.Service.HR.EmployeeCore
{
    /// <summary>
    /// Employee Core Orchestration Service - Handles complex multi-entity operations
    /// </summary>
    [AppService(ServiceType = typeof(IEmployeeCoreOrchestrationService), ServiceLifetime = LifeTime.Transient)]
    public class EmployeeCoreOrchestrationService : BaseService<HrEmployeeCore>, IEmployeeCoreOrchestrationService
    {
        private readonly ISensitiveDataProtector _sensitiveDataProtector;
        private readonly IHrPersonService _personService;
        private readonly IHrEmployeeCoreService _employeeService;
        private readonly ISysUserService _userService;

        public EmployeeCoreOrchestrationService(
            ISensitiveDataProtector sensitiveDataProtector,
            IHrPersonService personService,
            IHrEmployeeCoreService employeeService,
            ISysUserService userService)
        {
            _sensitiveDataProtector = sensitiveDataProtector;
            _personService = personService;
            _employeeService = employeeService;
            _userService = userService;
        }

        #region Create Complete Employee

        /// <summary>
        /// Create complete employee (Person + Employee + all related entities) in one transaction
        /// </summary>
        public async Task<EmployeeCreationResult> CreateCompleteEmployeeAsync(CreateEmployeeCompleteDto dto)
        {
            var result = new EmployeeCreationResult();

            try
            {
                await Context.Ado.BeginTranAsync();

                // Step 1: Create Person
                result.PersonId = await CreatePersonAsync(dto.Person.Person);

                // Step 2: Create Person Names
                if (dto.Person?.Names != null && dto.Person.Names.Any())
                {
                    foreach (var nameDto in dto.Person.Names)
                    {
                        nameDto.PersonId = result.PersonId;
                        await CreatePersonNameAsync(nameDto);
                    }
                }

                // Step 3: Create Person Demographic
                if (dto.Person?.Demographic != null)
                {
                    dto.Person.Demographic.PersonId = result.PersonId;
                    await CreatePersonDemographicAsync(dto.Person.Demographic);
                }

                // Step 4: Create Person Identifiers
                if (dto.Person?.Identifiers != null && dto.Person.Identifiers.Any())
                {
                    foreach (var idDto in dto.Person.Identifiers)
                    {
                        idDto.PersonId = result.PersonId;
                        await CreatePersonIdentifierAsync(idDto);
                    }
                }

                // Step 5: Create Person Contacts
                if (dto.Person?.Contacts != null && dto.Person.Contacts.Any())
                {
                    foreach (var contactDto in dto.Person.Contacts)
                    {
                        contactDto.PersonId = result.PersonId;
                        await CreatePersonContactAsync(contactDto);
                    }
                }

                // Step 6: Create Person Addresses
                if (dto.Person?.Addresses != null && dto.Person.Addresses.Any())
                {
                    foreach (var addressDto in dto.Person.Addresses)
                    {
                        addressDto.PersonId = result.PersonId;
                        await CreatePersonAddressAsync(addressDto);
                    }
                }

                // Step 7: Create Employee Core
                dto.Employee.PersonId = result.PersonId;

                // Generate employee code if not provided
                if (string.IsNullOrWhiteSpace(dto.Employee.EmployeeCode))
                {
                    dto.Employee.EmployeeCode = await GenerateEmployeeCodeAsync();
                }

                // Try to link user account (based on primary email contact)
                var primaryEmail = await GetPrimaryEmailContactAsync(result.PersonId);
                if (!string.IsNullOrWhiteSpace(primaryEmail))
                {
                    dto.Employee.UserId = await TryLinkUserAccountAsync(primaryEmail, dto.Employee.EmployeeCode);
                }

                result.EmployeeId = await CreateEmployeeCoreAsync(dto.Employee);

                // Step 8: Create Employee Assignments
                if (dto.Assignments != null && dto.Assignments.Any())
                {
                    foreach (var assignmentDto in dto.Assignments)
                    {
                        assignmentDto.EmployeeId = result.EmployeeId;
                        await CreateEmployeeAssignmentAsync(assignmentDto);
                    }
                }

                // Step 9: Create Employee Statuses
                if (dto.Statuses != null && dto.Statuses.Any())
                {
                    foreach (var statusDto in dto.Statuses)
                    {
                        statusDto.EmployeeId = result.EmployeeId;
                        await CreateEmployeeStatusAsync(statusDto);
                    }
                }
                else
                {
                    // Create default "Active" status if none provided
                    await CreateDefaultActiveStatusAsync(result.EmployeeId);
                }

                // Step 10: Create Employee Contracts
                if (dto.Contracts != null && dto.Contracts.Any())
                {
                    foreach (var contractDto in dto.Contracts)
                    {
                        contractDto.EmployeeId = result.EmployeeId;
                        await CreateEmployeeContractAsync(contractDto);
                    }
                }

                // Step 11: Create Employee Statutory
                if (dto.Statutory != null && dto.Statutory.Any())
                {
                    foreach (var statutoryDto in dto.Statutory)
                    {
                        statutoryDto.EmployeeId = result.EmployeeId;
                        await CreateEmployeeStatutoryAsync(statutoryDto);
                    }
                }

                // Step 12: Create Bank Accounts
                if (dto.BankAccounts != null && dto.BankAccounts.Any())
                {
                    foreach (var bankDto in dto.BankAccounts)
                    {
                        bankDto.EmployeeId = result.EmployeeId;
                        await CreateEmployeeBankAccountAsync(bankDto);
                    }
                }

                // Step 13: Create Compensation Packages
                if (dto.CompensationPackages != null && dto.CompensationPackages.Any())
                {
                    foreach (var compPackageDto in dto.CompensationPackages)
                    {
                        compPackageDto.EmployeeId = result.EmployeeId;
                        await CreateEmployeeCompensationPackageAsync(compPackageDto);
                    }
                }

                // Step 14: Create Emergency Contacts
                if (dto.EmergencyContacts != null && dto.EmergencyContacts.Any())
                {
                    foreach (var emergencyDto in dto.EmergencyContacts)
                    {
                        emergencyDto.EmployeeId = result.EmployeeId;
                        await CreateEmployeeEmergencyContactAsync(emergencyDto);
                    }
                }

                // Step 15: Create Dependent Links
                if (dto.Dependents != null && dto.Dependents.Any())
                {
                    foreach (var dependentDto in dto.Dependents)
                    {
                        dependentDto.EmployeeId = result.EmployeeId;
                        await CreateEmployeeDependentLinkAsync(dependentDto);
                    }
                }

                await Context.Ado.CommitTranAsync();
                result.Success = true;
                result.Message = "Employee created successfully";

                return result;
            }
            catch (Exception ex)
            {
                await Context.Ado.RollbackTranAsync();
                result.Success = false;
                result.Message = $"Failed to create employee: {ex.Message}";
                result.ErrorDetails = ex.ToString();
                // IMPORTANT: Do not rethrow here; controller expects a result object and will
                // translate failures to a consistent API response (instead of 500 COMMON_INTERNAL_ERROR).
                return result;
            }
        }

        #endregion

        #region Update Complete Employee

        /// <summary>
        /// Update complete employee (all fields that are provided)
        /// </summary>
        public async Task<bool> UpdateCompleteEmployeeAsync(UpdateEmployeeCompleteDto dto)
        {
            try
            {
                await Context.Ado.BeginTranAsync();

                // Update Employee Core
                if (dto.Employee != null)
                {
                    await UpdateEmployeeCoreAsync(dto.Employee);
                }

                // Update Person
                if (dto.Person != null)
                {
                    await UpdatePersonAsync(dto.Person);
                }

                // Update Names
                if (dto.Names != null && dto.Names.Any())
                {
                    foreach (var nameDto in dto.Names)
                    {
                        await UpdatePersonNameAsync(nameDto);
                    }
                }

                // Update Demographic
                if (dto.Demographic != null)
                {
                    await UpsertPersonDemographicAsync(dto.Demographic);
                }

                // Update Identifiers
                if (dto.Identifiers != null && dto.Identifiers.Any())
                {
                    foreach (var idDto in dto.Identifiers)
                    {
                        await UpdatePersonIdentifierAsync(idDto);
                    }
                }

                // Update Contacts
                if (dto.Contacts != null && dto.Contacts.Any())
                {
                    foreach (var contactDto in dto.Contacts)
                    {
                        await UpdatePersonContactAsync(contactDto);
                    }
                }

                // Update Addresses
                if (dto.Addresses != null && dto.Addresses.Any())
                {
                    foreach (var addressDto in dto.Addresses)
                    {
                        await UpdatePersonAddressAsync(addressDto);
                    }
                }

                // Update Assignments
                if (dto.Assignments != null && dto.Assignments.Any())
                {
                    foreach (var assignmentDto in dto.Assignments)
                    {
                        await UpdateEmployeeAssignmentAsync(assignmentDto);
                    }
                }

                // Update Statuses
                if (dto.Statuses != null && dto.Statuses.Any())
                {
                    foreach (var statusDto in dto.Statuses)
                    {
                        await UpdateEmployeeStatusAsync(statusDto);
                    }
                }

                // Update Contracts
                if (dto.Contracts != null && dto.Contracts.Any())
                {
                    foreach (var contractDto in dto.Contracts)
                    {
                        await UpdateEmployeeContractAsync(contractDto);
                    }
                }

                // Update Statutory
                if (dto.Statutory != null && dto.Statutory.Any())
                {
                    foreach (var statutoryDto in dto.Statutory)
                    {
                        await UpdateEmployeeStatutoryAsync(statutoryDto);
                    }
                }

                // Update Bank Accounts
                if (dto.BankAccounts != null && dto.BankAccounts.Any())
                {
                    foreach (var bankDto in dto.BankAccounts)
                    {
                        await UpdateEmployeeBankAccountAsync(bankDto);
                    }
                }

                // Update Compensation Packages
                if (dto.CompensationPackages != null && dto.CompensationPackages.Any())
                {
                    foreach (var compDto in dto.CompensationPackages)
                    {
                        await UpdateEmployeeCompensationPackageAsync(compDto);
                    }
                }

                // Update Emergency Contacts
                if (dto.EmergencyContacts != null && dto.EmergencyContacts.Any())
                {
                    foreach (var emergencyDto in dto.EmergencyContacts)
                    {
                        await UpdateEmployeeEmergencyContactAsync(emergencyDto);
                    }
                }

                // Update Dependents
                if (dto.Dependents != null && dto.Dependents.Any())
                {
                    foreach (var dependentDto in dto.Dependents)
                    {
                        await UpdateEmployeeDependentLinkAsync(dependentDto);
                    }
                }

                await Context.Ado.CommitTranAsync();
                return true;
            }
            catch
            {
                await Context.Ado.RollbackTranAsync();
                throw;
            }
        }

        #endregion

        #region Employee Lifecycle Operations

        /// <summary>
        /// Terminate employee
        /// </summary>
        public async Task<bool> TerminateEmployeeAsync(TerminateEmployeeDto dto)
        {
            try
            {
                await Context.Ado.BeginTranAsync();

                // Update employee core with termination date
                var employee = await Context.Queryable<HrEmployeeCore>()
                    .InSingleAsync(dto.EmployeeId);

                if (employee == null)
                {
                    throw new AppException(ExceptionCode.HR_EMPLOYEE_NOT_FOUND)
                        .WithParameter("EmployeeId", dto.EmployeeId);
                }

                employee.TerminationDate = dto.TerminationDate;
                employee.IsActive = false;
                employee.Update_time = DateTime.Now;

                await Context.Updateable(employee).ExecuteCommandAsync();

                // Add termination status
                var terminationStatus = new HrEmployeeStatus
                {
                    EmployeeId = dto.EmployeeId,
                    EmploymentStatusDictCode = dto.TerminationReasonDictCode ?? 0, // Default to "Terminated" status
                    EffectiveFrom = dto.TerminationDate,
                    EffectiveTo = null,
                    Create_time = DateTime.Now,
                    Remark = dto.Reason
                };

                await Context.Insertable(terminationStatus).ExecuteCommandAsync();

                // End all current assignments
                var currentAssignments = await Context.Queryable<HrEmployeeAssignment>()
                    .Where(a => a.EmployeeId == dto.EmployeeId)
                    .Where(a => a.EffectiveTo == null || a.EffectiveTo > dto.TerminationDate)
                    .ToListAsync();

                foreach (var assignment in currentAssignments)
                {
                    assignment.EffectiveTo = dto.TerminationDate;
                    assignment.Update_time = DateTime.Now;
                }

                if (currentAssignments.Any())
                {
                    await Context.Updateable(currentAssignments).ExecuteCommandAsync();
                }

                // Optionally deactivate linked user account
                if (employee.UserId.HasValue)
                {
                    await Context.Updateable<SysUser>()
                        .SetColumns(u => u.Status == 1) // 1 = Inactive
                        .Where(u => u.UserId == employee.UserId.Value)
                        .ExecuteCommandAsync();
                }

                await Context.Ado.CommitTranAsync();
                return true;
            }
            catch
            {
                await Context.Ado.RollbackTranAsync();
                throw;
            }
        }

        /// <summary>
        /// Activate employee
        /// </summary>
        public async Task<bool> ActivateEmployeeAsync(long employeeId)
        {
            try
            {
                await Context.Ado.BeginTranAsync();

                var employee = await Context.Queryable<HrEmployeeCore>()
                    .InSingleAsync(employeeId);

                if (employee == null)
                {
                    throw new AppException(ExceptionCode.HR_EMPLOYEE_NOT_FOUND)
                        .WithParameter("EmployeeId", employeeId);
                }

                employee.TerminationDate = null;
                employee.IsActive = true;
                employee.Update_time = DateTime.Now;

                await Context.Updateable(employee).ExecuteCommandAsync();

                // Add active status
                await CreateDefaultActiveStatusAsync(employeeId);

                // Reactivate linked user account
                if (employee.UserId.HasValue)
                {
                    await Context.Updateable<SysUser>()
                        .SetColumns(u => u.Status == 0) // 0 = Active
                        .Where(u => u.UserId == employee.UserId.Value)
                        .ExecuteCommandAsync();
                }

                await Context.Ado.CommitTranAsync();
                return true;
            }
            catch
            {
                await Context.Ado.RollbackTranAsync();
                throw;
            }
        }

        /// <summary>
        /// Transfer employee to new department/position
        /// </summary>
        public async Task<bool> TransferEmployeeAsync(TransferEmployeeDto dto)
        {
            try
            {
                await Context.Ado.BeginTranAsync();

                // Ensure employee exists
                var exists = await Context.Queryable<HrEmployeeCore>()
                    .AnyAsync(e => e.EmployeeId == dto.EmployeeId);

                if (!exists)
                {
                    throw new AppException(ExceptionCode.HR_EMPLOYEE_NOT_FOUND)
                        .WithParameter("EmployeeId", dto.EmployeeId);
                }

                // End current primary assignment
                var currentAssignments = await Context.Queryable<HrEmployeeAssignment>()
                    .Where(a => a.EmployeeId == dto.EmployeeId)
                    .Where(a => a.IsPrimary == true)
                    .Where(a => a.EffectiveTo == null || a.EffectiveTo > dto.EffectiveDate)
                    .ToListAsync();

                foreach (var assignment in currentAssignments)
                {
                    assignment.EffectiveTo = dto.EffectiveDate.AddDays(-1);
                    assignment.IsPrimary = false;
                    assignment.Update_time = DateTime.Now;
                }

                if (currentAssignments.Any())
                {
                    await Context.Updateable(currentAssignments).ExecuteCommandAsync();
                }

                // Create new assignment
                var newAssignment = new HrEmployeeAssignment
                {
                    EmployeeId = dto.EmployeeId,
                    DepartmentId = dto.NewDepartmentId,
                    PositionId = dto.NewPositionId,
                    ManagerEmployeeId = dto.NewManagerEmployeeId,
                    WorkLocationId = dto.NewWorkLocationId,
                    EffectiveFrom = dto.EffectiveDate,
                    EffectiveTo = null,
                    IsPrimary = true,
                    Create_time = DateTime.Now,
                    Remark = dto.Reason
                };

                await Context.Insertable(newAssignment).ExecuteCommandAsync();

                await Context.Ado.CommitTranAsync();
                return true;
            }
            catch
            {
                await Context.Ado.RollbackTranAsync();
                throw;
            }
        }

        /// <summary>
        /// Promote employee with new compensation
        /// </summary>
        public async Task<bool> PromoteEmployeeAsync(PromoteEmployeeDto dto)
        {
            try
            {
                await Context.Ado.BeginTranAsync();

                // Transfer to new position/department
                var transferDto = new TransferEmployeeDto
                {
                    EmployeeId = dto.EmployeeId,
                    NewDepartmentId = dto.NewDepartmentId ?? 0, // Will use current if not provided
                    NewPositionId = dto.NewPositionId,
                    EffectiveDate = dto.EffectiveDate,
                    Reason = $"Promotion: {dto.Reason}"
                };

                // Get current assignment to preserve department if not changing
                if (!dto.NewDepartmentId.HasValue)
                {
                    var currentAssignment = await Context.Queryable<HrEmployeeAssignment>()
                        .Where(a => a.EmployeeId == dto.EmployeeId && a.IsPrimary == true)
                        .OrderByDescending(a => a.EffectiveFrom)
                        .FirstAsync();

                    if (currentAssignment != null)
                    {
                        transferDto.NewDepartmentId = currentAssignment.DepartmentId ?? 0;
                    }
                }

                await TransferEmployeeAsync(transferDto);

                // Update compensation if new salary provided
                if (dto.NewBaseSalary.HasValue)
                {
                    // TODO: Create new compensation package with new salary
                    // This requires the compensation service implementation
                }

                await Context.Ado.CommitTranAsync();
                return true;
            }
            catch
            {
                await Context.Ado.RollbackTranAsync();
                throw;
            }
        }

        #endregion

        #region Helper Methods - Create Operations

        private async Task<long> CreatePersonAsync(CreatePersonDto dto)
        {
            var entity = new HrPerson
            {
                IsActive = dto.IsActive,
                Create_time = DateTime.Now
            };

            return await Context.Insertable(entity).ExecuteReturnBigIdentityAsync();
        }

        private async Task<long> CreatePersonNameAsync(CreatePersonNameDto dto)
        {
            var entity = dto.Adapt<HrPersonName>();
            entity.Create_time = DateTime.Now;
            return await Context.Insertable(entity).ExecuteReturnBigIdentityAsync();
        }

        private async Task CreatePersonDemographicAsync(CreatePersonDemographicDto dto)
        {
            var entity = dto.Adapt<HrPersonDemographic>();
            // Treat default/min DOB as "not provided"
            if (dto.DateOfBirth == default || dto.DateOfBirth == DateTime.MinValue || dto.DateOfBirth.Year <= 1900)
            {
                entity.DateOfBirth = null;
            }
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task CreatePersonIdentifierAsync(CreatePersonIdentifierDto dto)
        {
            var entity = dto.Adapt<HrPersonIdentifier>();
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task CreatePersonContactAsync(CreatePersonContactDto dto)
        {
            var entity = dto.Adapt<HrPersonContact>();
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task CreatePersonAddressAsync(CreatePersonAddressDto dto)
        {
            var entity = dto.Adapt<HrPersonAddress>();
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task<long> CreateEmployeeCoreAsync(CreateEmployeeCoreDto dto)
        {
            var entity = dto.Adapt<HrEmployeeCore>();
            entity.Create_time = DateTime.Now;
            return await Context.Insertable(entity).ExecuteReturnBigIdentityAsync();
        }

        private async Task CreateEmployeeAssignmentAsync(CreateEmployeeAssignmentDto dto)
        {
            var entity = dto.Adapt<HrEmployeeAssignment>();
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task CreateEmployeeStatusAsync(CreateEmployeeStatusDto dto)
        {
            var entity = dto.Adapt<HrEmployeeStatus>();
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task CreateDefaultActiveStatusAsync(long employeeId)
        {
            var status = new HrEmployeeStatus
            {
                EmployeeId = employeeId,
                EmploymentStatusDictCode = 1, // Assume 1 = Active (should use dictionary)
                EffectiveFrom = DateTime.Now,
                EffectiveTo = null,
                Create_time = DateTime.Now
            };

            await Context.Insertable(status).ExecuteCommandAsync();
        }

        private async Task CreateEmployeeContractAsync(CreateEmployeeContractDto dto)
        {
            var entity = dto.Adapt<HrEmployeeContract>();
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task CreateEmployeeStatutoryAsync(CreateEmployeeStatutoryDto dto)
        {
            var entity = dto.Adapt<HrEmployeeStatutory>();
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task CreateEmployeeBankAccountAsync(CreateEmployeeBankAccountDto dto)
        {
            var entity = new HrEmployeeBankAccount
            {
                EmployeeId = dto.EmployeeId,
                BankName = dto.BankName,
                CurrencyId = dto.CurrencyId,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                IsPrimary = dto.IsPrimary,
                Create_time = DateTime.Now
            };

            // Encrypt sensitive data
            if (!string.IsNullOrWhiteSpace(dto.Iban))
            {
                entity.IbanEncrypted = _sensitiveDataProtector.Protect(dto.Iban);
            }
            if (!string.IsNullOrWhiteSpace(dto.AccountNumber))
            {
                entity.AccountNumberEncrypted = _sensitiveDataProtector.Protect(dto.AccountNumber);
            }
            if (!string.IsNullOrWhiteSpace(dto.BankAccount))
            {
                entity.BankAccountEncrypted = _sensitiveDataProtector.Protect(dto.BankAccount);
            }

            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task CreateEmployeeCompensationPackageAsync(CreateEmployeeCompensationPackageDto dto)
        {
            var package = new HrEmployeeCompensationPackage
            {
                EmployeeId = dto.EmployeeId,
                CurrencyId = dto.CurrencyId,
                CompPackageStatusDictCode = dto.CompPackageStatusDictCode,
                EffectiveFrom = dto.EffectiveFrom,
                EffectiveTo = dto.EffectiveTo,
                Create_time = DateTime.Now
            };

            var packageId = await Context.Insertable(package).ExecuteReturnBigIdentityAsync();

            // Create components
            if (dto.Components != null && dto.Components.Any())
            {
                foreach (var componentDto in dto.Components)
                {
                    var component = new HrEmployeeCompensationComponent
                    {
                        CompPackageId = packageId,
                        ComponentTypeDictCode = componentDto.ComponentTypeDictCode,
                        ComponentCodeDictCode = componentDto.ComponentCodeDictCode,
                        Amount = componentDto.Amount,
                        Create_time = DateTime.Now
                    };

                    await Context.Insertable(component).ExecuteCommandAsync();
                }
            }
        }

        private async Task CreateEmployeeEmergencyContactAsync(CreateEmployeeEmergencyContactDto dto)
        {
            var entity = dto.Adapt<HrEmployeeEmergencyContact>();
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        private async Task CreateEmployeeDependentLinkAsync(CreateEmployeeDependentLinkDto dto)
        {
            var entity = dto.Adapt<HrEmployeeDependentLink>();
            entity.Create_time = DateTime.Now;
            await Context.Insertable(entity).ExecuteCommandAsync();
        }

        #endregion

        #region Helper Methods - Update Operations

        private async Task UpdatePersonAsync(UpdatePersonDto dto)
        {
            var entity = await Context.Queryable<HrPerson>().InSingleAsync(dto.PersonId);
            if (entity == null) return;

            entity.IsActive = dto.IsActive;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdatePersonNameAsync(UpdatePersonNameDto dto)
        {
            // ID = 0 means CREATE new record
            if (dto.PersonNameId == 0)
            {
                if (!dto.PersonId.HasValue || dto.PersonId.Value == 0)
                    return; // Cannot create without PersonId

                var newEntity = new HrPersonName
                {
                    PersonId = dto.PersonId.Value,
                    GivenName = dto.GivenName,
                    MiddleName = dto.MiddleName,
                    FamilyName = dto.FamilyName,
                    FullNameAr = dto.FullNameAr,
                    EffectiveFrom = dto.EffectiveFrom ?? DateTime.Now,
                    IsPrimary = dto.IsPrimary,
                    Create_time = DateTime.Now
                };
                await Context.Insertable(newEntity).ExecuteCommandAsync();
                return;
            }

            var entity = await Context.Queryable<HrPersonName>().InSingleAsync(dto.PersonNameId);
            if (entity == null) return;

            entity.GivenName = dto.GivenName;
            entity.MiddleName = dto.MiddleName;
            entity.FamilyName = dto.FamilyName;
            entity.FullNameAr = dto.FullNameAr;
            entity.EffectiveFrom = dto.EffectiveFrom ?? entity.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.IsPrimary = dto.IsPrimary;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdatePersonDemographicAsync(UpdatePersonDemographicDto dto)
        {
            var entity = await Context.Queryable<HrPersonDemographic>().InSingleAsync(dto.PersonId);
            if (entity == null) return;

            // Only update DOB if a meaningful value is provided
            if (dto.DateOfBirth != default && dto.DateOfBirth != DateTime.MinValue && dto.DateOfBirth.Year > 1900)
            {
                entity.DateOfBirth = dto.DateOfBirth;
            }
            entity.GenderDictCode = dto.GenderDictCode;
            entity.MaritalStatusDictCode = dto.MaritalStatusDictCode;
            entity.NationalityDictCode = dto.NationalityDictCode;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// Upsert person demographics (insert if missing, else update).
        /// Some legacy employees were created without a demographic row, which made updates silently skip.
        /// </summary>
        private async Task UpsertPersonDemographicAsync(UpdatePersonDemographicDto dto)
        {
            var entity = await Context.Queryable<HrPersonDemographic>().InSingleAsync(dto.PersonId);
            if (entity == null)
            {
                var insert = new HrPersonDemographic
                {
                    PersonId = dto.PersonId,
                    GenderDictCode = dto.GenderDictCode,
                    MaritalStatusDictCode = dto.MaritalStatusDictCode,
                    NationalityDictCode = dto.NationalityDictCode,
                    Create_time = DateTime.Now
                };

                if (dto.DateOfBirth != default && dto.DateOfBirth != DateTime.MinValue && dto.DateOfBirth.Year > 1900)
                {
                    insert.DateOfBirth = dto.DateOfBirth;
                }
                else
                {
                    insert.DateOfBirth = null;
                }

                await Context.Insertable(insert).ExecuteCommandAsync();
                return;
            }

            await UpdatePersonDemographicAsync(dto);
        }

        private async Task UpdatePersonIdentifierAsync(UpdatePersonIdentifierDto dto)
        {
            // ID = 0 means CREATE new record
            if (dto.PersonIdentifierId == 0)
            {
                var newEntity = new HrPersonIdentifier
                {
                    PersonId = (long)dto.PersonId,
                    IdentifierTypeDictCode = dto.IdentifierTypeDictCode,
                    IdentifierNumber = dto.IdentifierNumber,
                    IssuingCountryDictCode = dto.IssuingCountryDictCode,
                    IssueDate = dto.IssueDate,
                    ExpiryDate = dto.ExpiryDate,
                    IsPrimary = dto.IsPrimary,
                    IsVerified = dto.IsVerified,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(newEntity).ExecuteCommandAsync();
                return;
            }

            // UPDATE existing record
            var entity = await Context.Queryable<HrPersonIdentifier>().InSingleAsync(dto.PersonIdentifierId);
            if (entity == null) return;

            entity.IdentifierTypeDictCode = dto.IdentifierTypeDictCode;
            entity.IdentifierNumber = dto.IdentifierNumber;
            entity.IssuingCountryDictCode = dto.IssuingCountryDictCode;
            entity.IssueDate = dto.IssueDate;
            entity.ExpiryDate = dto.ExpiryDate;
            entity.IsPrimary = dto.IsPrimary;
            entity.IsVerified = dto.IsVerified;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdatePersonContactAsync(UpdatePersonContactDto dto)
        {
            // ID = 0 means CREATE new record
            if (dto.PersonContactId == 0)
            {
                var newEntity = new HrPersonContact
                {
                    PersonId = dto.PersonId.Value,
                    ContactTypeDictCode = dto.ContactTypeDictCode,
                    ContactValue = dto.ContactValue,
                    EffectiveFrom = dto.EffectiveFrom,
                    EffectiveTo = dto.EffectiveTo,
                    IsPrimary = dto.IsPrimary,
                    IsVerified = dto.IsVerified,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(newEntity).ExecuteCommandAsync();
                return;
            }

            // UPDATE existing record
            var entity = await Context.Queryable<HrPersonContact>().InSingleAsync(dto.PersonContactId);
            if (entity == null) return;

            entity.ContactTypeDictCode = dto.ContactTypeDictCode;
            entity.ContactValue = dto.ContactValue;
            entity.EffectiveFrom = dto.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.IsPrimary = dto.IsPrimary;
            entity.IsVerified = dto.IsVerified;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdatePersonAddressAsync(UpdatePersonAddressDto dto)
        {
            // ID = 0 means CREATE new record
            if (dto.PersonAddressId == 0)
            {
                var newEntity = new HrPersonAddress
                {
                    PersonId = dto.PersonId.Value,
                    AddressTypeDictCode = dto.AddressTypeDictCode,
                    CountryDictCode = dto.CountryDictCode,
                    City = dto.City,
                    PostalCode = dto.PostalCode,
                    AddressLine1 = dto.AddressLine1,
                    AddressLine2 = dto.AddressLine2,
                    EffectiveFrom = dto.EffectiveFrom,
                    EffectiveTo = dto.EffectiveTo,
                    IsPrimary = dto.IsPrimary,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(newEntity).ExecuteCommandAsync();
                return;
            }

            // UPDATE existing record
            var entity = await Context.Queryable<HrPersonAddress>().InSingleAsync(dto.PersonAddressId);
            if (entity == null) return;

            entity.AddressTypeDictCode = dto.AddressTypeDictCode;
            entity.CountryDictCode = dto.CountryDictCode;
            entity.City = dto.City;
            entity.PostalCode = dto.PostalCode;
            entity.AddressLine1 = dto.AddressLine1;
            entity.AddressLine2 = dto.AddressLine2;
            entity.EffectiveFrom = dto.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.IsPrimary = dto.IsPrimary;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdateEmployeeCoreAsync(UpdateEmployeeCoreDto dto)
        {
            var entity = await Context.Queryable<HrEmployeeCore>().InSingleAsync(dto.EmployeeId);
            if (entity == null) return;

            entity.EmployeeCode = dto.EmployeeCode;
            entity.HireDate = dto.HireDate;
            entity.TerminationDate = dto.TerminationDate;
            entity.EmploymentTypeDictCode = dto.EmploymentTypeDictCode;
            entity.IsActive = dto.IsActive;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdateEmployeeAssignmentAsync(UpdateEmployeeAssignmentDto dto)
        {
            // ID = 0 means CREATE new record
            if (dto.EmployeeAssignmentId == 0)
            {
                if (!dto.EmployeeId.HasValue || dto.EmployeeId.Value == 0)
                    return; // Cannot create without EmployeeId

                var newEntity = new HrEmployeeAssignment
                {
                    EmployeeId = dto.EmployeeId.Value,
                    DepartmentId = dto.DepartmentId,
                    PositionId = dto.PositionId,
                    ManagerEmployeeId = dto.ManagerEmployeeId,
                    WorkLocationId = dto.WorkLocationId,
                    EffectiveFrom = dto.EffectiveFrom ?? DateTime.Now,
                    IsPrimary = dto.IsPrimary,
                    Create_time = DateTime.Now
                };
                await Context.Insertable(newEntity).ExecuteCommandAsync();
                return;
            }

            var entity = await Context.Queryable<HrEmployeeAssignment>().InSingleAsync(dto.EmployeeAssignmentId);
            if (entity == null) return;

            entity.DepartmentId = dto.DepartmentId;
            entity.PositionId = dto.PositionId;
            entity.ManagerEmployeeId = dto.ManagerEmployeeId;
            entity.WorkLocationId = dto.WorkLocationId;
            entity.EffectiveFrom = dto.EffectiveFrom ?? entity.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.IsPrimary = dto.IsPrimary;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdateEmployeeStatusAsync(UpdateEmployeeStatusDto dto)
        {
            var entity = await Context.Queryable<HrEmployeeStatus>().InSingleAsync(dto.EmployeeStatusId);
            if (entity == null) return;

            entity.EmploymentStatusDictCode = dto.EmploymentStatusDictCode;
            entity.StatusReasonDictCode = dto.StatusReasonDictCode;
            entity.EffectiveFrom = dto.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdateEmployeeContractAsync(UpdateEmployeeContractDto dto)
        {
            var entity = await Context.Queryable<HrEmployeeContract>().InSingleAsync(dto.EmployeeContractId);
            if (entity == null) return;

            entity.ContractTypeDictCode = dto.ContractTypeDictCode;
            entity.ContractStartDate = dto.ContractStartDate;
            entity.ContractEndDate = dto.ContractEndDate;
            entity.ProbationEndDate = dto.ProbationEndDate;
            entity.IsCurrent = dto.IsCurrent;
            entity.EffectiveFrom = dto.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdateEmployeeStatutoryAsync(UpdateEmployeeStatutoryDto dto)
        {
            var entity = await Context.Queryable<HrEmployeeStatutory>().InSingleAsync(dto.EmployeeStatutoryId);
            if (entity == null) return;

            entity.CountryDictCode = dto.CountryDictCode;
            entity.HasGosi = dto.HasGosi;
            entity.GosiNumber = dto.GosiNumber;
            entity.IsSaudi = dto.IsSaudi;
            entity.MedicalInsuranceNumber = dto.MedicalInsuranceNumber;
            entity.EffectiveFrom = dto.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdateEmployeeBankAccountAsync(UpdateEmployeeBankAccountDto dto)
        {
            // Check if this is a CREATE operation (ID=0 pattern)
            if (dto.EmployeeBankAccountId == 0)
            {
                // Validate EmployeeId is provided
                if (!dto.EmployeeId.HasValue || dto.EmployeeId.Value == 0)
                {
                    throw new Exception("EmployeeId is required when creating a new bank account");
                }

                // Convert to CreateEmployeeBankAccountDto and delegate to Create method
                var createDto = new CreateEmployeeBankAccountDto
                {
                    EmployeeId = dto.EmployeeId.Value,
                    BankName = dto.BankName,
                    Iban = dto.Iban,
                    AccountNumber = dto.AccountNumber,
                    BankAccount = dto.BankAccount,
                    CurrencyId = dto.CurrencyId,
                    EffectiveFrom = dto.EffectiveFrom,
                    EffectiveTo = dto.EffectiveTo,
                    IsPrimary = dto.IsPrimary
                };
                await CreateEmployeeBankAccountAsync(createDto);
                return;
            }

            // Original UPDATE logic
            var entity = await Context.Queryable<HrEmployeeBankAccount>().InSingleAsync(dto.EmployeeBankAccountId);
            if (entity == null) return;

            entity.BankName = dto.BankName;
            entity.CurrencyId = dto.CurrencyId;
            entity.EffectiveFrom = dto.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.IsPrimary = dto.IsPrimary;
            entity.Update_time = DateTime.Now;

            // Re-encrypt if changed
            if (!string.IsNullOrWhiteSpace(dto.Iban))
            {
                entity.IbanEncrypted = _sensitiveDataProtector.Protect(dto.Iban);
            }
            if (!string.IsNullOrWhiteSpace(dto.AccountNumber))
            {
                entity.AccountNumberEncrypted = _sensitiveDataProtector.Protect(dto.AccountNumber);
            }
            if (!string.IsNullOrWhiteSpace(dto.BankAccount))
            {
                entity.BankAccountEncrypted = _sensitiveDataProtector.Protect(dto.BankAccount);
            }

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdateEmployeeCompensationPackageAsync(UpdateEmployeeCompensationPackageDto dto)
        {
            var entity = await Context.Queryable<HrEmployeeCompensationPackage>().InSingleAsync(dto.CompPackageId);
            if (entity == null) return;

            entity.CurrencyId = dto.CurrencyId;
            entity.CompPackageStatusDictCode = dto.CompPackageStatusDictCode;
            entity.EffectiveFrom = dto.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdateEmployeeEmergencyContactAsync(UpdateEmployeeEmergencyContactDto dto)
        {
            // ID = 0 means CREATE new record
            if (dto.EmergencyContactId == 0)
            {
                var newEntity = new HrEmployeeEmergencyContact
                {
                    EmployeeId = dto.EmployeeId.Value,
                    ContactName = dto.ContactName,
                    RelationshipDictCode = dto.RelationshipDictCode,
                    Phone = dto.Phone,
                    AlternatePhone = dto.AlternatePhone,
                    Email = dto.Email,
                    Address = dto.Address,
                    IsPrimary = dto.IsPrimary,
                    IsActive = dto.IsActive,
                    Create_time = DateTime.Now
                };

                await Context.Insertable(newEntity).ExecuteCommandAsync();
                return;
            }

            // UPDATE existing record
            var entity = await Context.Queryable<HrEmployeeEmergencyContact>().InSingleAsync(dto.EmergencyContactId);
            if (entity == null) return;

            entity.ContactName = dto.ContactName;
            entity.RelationshipDictCode = dto.RelationshipDictCode;
            entity.Phone = dto.Phone;
            entity.AlternatePhone = dto.AlternatePhone;
            entity.Email = dto.Email;
            entity.Address = dto.Address;
            entity.IsPrimary = dto.IsPrimary;
            entity.IsActive = dto.IsActive;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        private async Task UpdateEmployeeDependentLinkAsync(UpdateEmployeeDependentLinkDto dto)
        {
            var entity = await Context.Queryable<HrEmployeeDependentLink>().InSingleAsync(dto.EmployeeDependentLinkId);
            if (entity == null) return;

            entity.RelationshipDictCode = dto.RelationshipDictCode;
            entity.EffectiveFrom = dto.EffectiveFrom;
            entity.EffectiveTo = dto.EffectiveTo;
            entity.IsActive = dto.IsActive;
            entity.Update_time = DateTime.Now;

            await Context.Updateable(entity).ExecuteCommandAsync();
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Generate unique employee code
        /// </summary>
        private async Task<string> GenerateEmployeeCodeAsync()
        {
            const int maxAttempts = 30;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var datePart = DateTime.Now.ToString("yyyyMMdd");
                var rand = new Random().Next(1000, 9999);
                var code = $"EMP-{datePart}-{rand}";

                var exists = await Context.Queryable<HrEmployeeCore>()
                    .AnyAsync(e => e.EmployeeCode == code);

                if (!exists)
                {
                    return code;
                }
            }

            // Fallback
            return $"EMP-{DateTime.Now:yyyyMMdd}-{DateTime.Now.Ticks % 1000000:D6}";
        }

        /// <summary>
        /// Get primary email contact for a person
        /// </summary>
        private async Task<string> GetPrimaryEmailContactAsync(long personId)
        {
            // Assume email contact type dict code = 1 (should be configurable)
            var now = DateTime.Now;
            var emailContact = await Context.Queryable<HrPersonContact>()
                .Where(c => c.PersonId == personId)
                .Where(c => c.ContactTypeDictCode == 1) // Email type
                .Where(c => c.IsPrimary == true)
                .Where(c => c.EffectiveFrom <= now)
                .Where(c => c.EffectiveTo == null || c.EffectiveTo > now)
                .FirstAsync();

            return emailContact?.ContactValue;
        }

        /// <summary>
        /// Try to link or create user account for employee
        /// </summary>
        private async Task<long?> TryLinkUserAccountAsync(string email, string employeeCode)
        {
            // NOTE:
            // - In PostgreSQL, *any* failed statement inside an open transaction aborts the whole transaction.
            //   Swallowing errors here causes the later inserts (e.g., hr_employee_core) to fail even if valid.
            // - sys_user has NOT NULL fields (e.g., NickName, Password). We must populate them.
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(employeeCode))
            {
                return null;
            }

            // Check if user with this email already exists (ignore soft-deleted users)
            var existingUser = await Context.Queryable<SysUser>()
                .Where(u => u.Email == email)
                .Where(u => u.DelFlag == 0)
                .FirstAsync();

            if (existingUser != null)
            {
                return existingUser.UserId;
            }

            // Create new user account (minimal required fields)
            var rawPassword = GenerateInitialEmployeePassword(email, employeeCode);
            var hashedPassword = NETCore.Encrypt.EncryptProvider.Md5(rawPassword);
            var nickname = GetNickNameFromEmail(email) ?? employeeCode;

            var newUser = new SysUser
            {
                UserName = employeeCode,
                NickName = nickname,
                Email = email,
                Password = hashedPassword,
                Status = 0,   // 0 = Active
                DelFlag = 0,  // 0 = Exists
                Sex = 2,      // 2 = Unknown
                Create_time = DateTime.Now,
            };

            // Use Snowflake ID (SysUser primary key is not identity)
            var userId = await Context.Insertable(newUser).ExecuteReturnSnowflakeIdAsync();
            return userId;
        }

        private static string GetNickNameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var at = email.IndexOf('@');
            if (at <= 0) return email;
            return email.Substring(0, at);
        }

        private static string GenerateInitialEmployeePassword(string email, string employeeCode)
        {
            try
            {
                var nick = GetNickNameFromEmail(email) ?? "emp";
                var a = nick.Length >= 3 ? nick.Substring(0, 3) : nick;
                var codePart = employeeCode.Length >= 3 ? employeeCode.Substring(employeeCode.Length - 3) : employeeCode;

                var password = $"{a.ToLower()}{codePart}@2024";
                if (password.Length < 8)
                {
                    password += "Emp123!";
                }
                return password;
            }
            catch
            {
                return "Employee123!";
            }
        }

        #endregion
    }

    #region Result Classes

    /// <summary>
    /// Employee Creation Result
    /// </summary>
    public class EmployeeCreationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long PersonId { get; set; }
        public long EmployeeId { get; set; }
        public string ErrorDetails { get; set; }
    }

    #endregion
}
