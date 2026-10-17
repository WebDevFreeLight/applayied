using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System;
using System.Linq;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Model.System.Dto;
using ZR.Service.HR.EmployeeCore;
using ZR.Service.HR.IService;
using ZR.Service.Auth;
using ZR.ServiceCore.Services;
using Infrastructure;

namespace ZR.Admin.WebApi.Controllers.HR.EmployeeCore
{
    /// <summary>
    /// Employee Management Controller - Modern Clean Architecture
    /// Uses EmployeeCore (normalized backend) with clean API DTOs
    /// </summary>
    [Verify]
    [Route("api/hr/employees")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class EmployeesController : BaseController
    {
        private readonly IEmployeeCoreOrchestrationService _orchestrationService;
        private readonly IEmployeeApiMappingService _mappingService;
        private readonly IHrEmployeeCoreService _employeeService;
        private readonly IEmployeeInvitationService _invitationService;

        public EmployeesController(
            IEmployeeCoreOrchestrationService orchestrationService,
            IEmployeeApiMappingService mappingService,
            IHrEmployeeCoreService employeeService,
            IEmployeeInvitationService invitationService)
        {
            _orchestrationService = orchestrationService;
            _mappingService = mappingService;
            _employeeService = employeeService;
            _invitationService = invitationService;
        }

        /// <summary>
        /// Get employee lookup list (minimal info for dropdowns)
        /// PUBLIC ENDPOINT - No permission required, accessible to all authenticated users
        /// Used for: Lead assignment, Meeting participants, etc.
        /// </summary>
        /// <param name="departmentId">Optional department filter</param>
        /// <param name="activeOnly">Only return active employees (default: true)</param>
        /// <returns>List of employees with minimal info (id, name)</returns>
        [HttpGet("lookup")]
        public async Task<IActionResult> GetEmployeeLookup([FromQuery] long? departmentId = null, [FromQuery] bool activeOnly = true)
        {
            try
            {
                // Build predicate for active employees
                var predicate = Expressionable.Create<ZR.Model.HR.EmployeeCore.HrEmployeeCore>();

                if (activeOnly)
                {
                    predicate = predicate.And(e => e.IsActive);
                }

                // Get all matching employees
                var employees = await _employeeService.GetListAsync(predicate.ToExpression());
                var employeeIds = employees.Select(e => e.EmployeeId).ToList();

                // Get employee details with names
                var dtoList = await _mappingService.ToListDtoAsync(employeeIds);

                // Apply department filter if provided (requires join data)
                if (departmentId.HasValue)
                {
                    dtoList = dtoList.Where(e => e.DepartmentId == departmentId.Value).ToList();
                }

                // Return minimal lookup data (id, name, email, department)
                var lookupData = dtoList.Select(e => new {
                    value = e.EmployeeId.ToString(), // String to prevent JS precision loss
                    label = e.FullName ?? $"{e.FirstName} {e.LastName}".Trim(),
                    employeeId = e.EmployeeId.ToString(),
                    employeeCode = e.EmployeeCode,
                    fullName = e.FullName,
                    firstName = e.FirstName,
                    lastName = e.LastName,
                    email = e.Email,
                    departmentId = e.DepartmentId?.ToString(),
                    departmentName = e.DepartmentName,
                    isActive = e.IsActive
                }).ToList();

                return SUCCESS(lookupData);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to get employee lookup: {ex.Message}");
            }
        }

        #region Employee Invitation

        /// <summary>
        /// Invite an employee to create a system account
        /// </summary>
        /// <param name="dto">Invitation data including employee ID, email, and roles</param>
        /// <returns>Invitation result</returns>
        [HttpPost("invite")]
        [ActionPermissionFilter("hr:employees:invite")]
        [Log(Title = "Employee Invitation", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> InviteEmployee([FromBody] InviteEmployeeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ToResponse(ResultCode.PARAM_ERROR, "Invalid request parameters");
                }

                var result = await _invitationService.InviteEmployeeAsync(dto);
                return SUCCESS(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to send invitation: {ex.Message}");
            }
        }

        /// <summary>
        /// Resend invitation email with new token
        /// </summary>
        /// <param name="dto">Employee ID to resend invitation</param>
        /// <returns>Invitation result</returns>
        [HttpPost("resend-invitation")]
        [ActionPermissionFilter("hr:employees:invite")]
        [Log(Title = "Resend Invitation", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ResendInvitation([FromBody] ResendInvitationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ToResponse(ResultCode.PARAM_ERROR, "Invalid request parameters");
                }

                var result = await _invitationService.ResendInvitationAsync(dto.EmployeeId);
                return SUCCESS(result);
            }
            catch (CustomException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to resend invitation: {ex.Message}");
            }
        }

        /// <summary>
        /// Get employee's invitation status and preview access info
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Invitation status and access preview</returns>
        [HttpGet("{employeeId}/invitation-status")]
        [ActionPermissionFilter("hr:employees:query")]
        public async Task<IActionResult> GetInvitationStatus(long employeeId)
        {
            try
            {
                var employee = await _employeeService.GetByIdAsync(employeeId);
                if (employee == null)
                {
                    return ToResponse(ResultCode.FAIL, "Employee not found");
                }

                // Get employee email and name
                string employeeEmail = await _invitationService.GetEmployeeEmailAsync(employeeId);
                string employeeName = await _invitationService.GetEmployeeNameAsync(employeeId);

                bool hasSystemAccess = employee.UserId.HasValue;
                long? existingUserId = employee.UserId;
                bool canActivate = true;
                var activationBlockers = new List<string>();

                // Check activation blockers
                if (string.IsNullOrEmpty(employeeEmail))
                {
                    canActivate = false;
                    activationBlockers.Add("Employee has no email address");
                }

                if (hasSystemAccess)
                {
                    var userService = App.GetService<ISysUserService>();
                    var user = userService.SelectUserById(employee.UserId.Value);
                    if (user != null && user.IsActivated == 1)
                    {
                        canActivate = false;
                        activationBlockers.Add("Employee already has an active account");
                    }
                }

                return SUCCESS(new
                {
                    employeeId,
                    employeeCode = employee.EmployeeCode,
                    employeeName = employeeName ?? "Unknown",
                    email = employeeEmail ?? "",
                    positionId = (long?)null,
                    positionName = (string)null,
                    departmentId = (long?)null,
                    departmentName = (string)null,
                    hasSystemAccess,
                    existingUserId,
                    derivedRoleIds = new long[] { },
                    derivedRoleNames = new string[] { "Employee" }, // Default role
                    canActivate,
                    activationBlockers
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to get invitation status: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// Get employee list (paginated with filters)
        /// </summary>
        /// <param name="request">Query parameters</param>
        /// <returns>Paged list of employees</returns>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:employees:list")]
        public async Task<IActionResult> GetEmployeeList([FromQuery] EmployeeQueryRequest request)
        {
            try
            {
                // Build predicate
                var predicate = Expressionable.Create<ZR.Model.HR.EmployeeCore.HrEmployeeCore>();

                if (!string.IsNullOrEmpty(request.EmployeeCode))
                {
                    predicate = predicate.And(e => e.EmployeeCode.Contains(request.EmployeeCode));
                }

                if (request.IsActive.HasValue)
                {
                    predicate = predicate.And(e => e.IsActive == request.IsActive.Value);
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    var isActive = request.Status.ToLower() == "active";
                    predicate = predicate.And(e => e.IsActive == isActive);
                }

                if (request.HireDateFrom.HasValue)
                {
                    predicate = predicate.And(e => e.HireDate >= request.HireDateFrom.Value);
                }

                if (request.HireDateTo.HasValue)
                {
                    predicate = predicate.And(e => e.HireDate <= request.HireDateTo.Value);
                }

                if (!string.IsNullOrEmpty(request.EmploymentType))
                {
                    // EmploymentType is stored as dict code in employee table
                    // Filter is applied in mapping service - skip for now
                }

                // Filters requiring joins - these will be applied in mapping service
                if (request.DepartmentId.HasValue || request.PositionId.HasValue ||
                    !string.IsNullOrEmpty(request.Name) || !string.IsNullOrEmpty(request.Email))
                {
                    // Note: Assignment table has DepartmentId and PositionId
                    // PersonName table has names
                    // PersonContact table has email
                    // These filters need to be applied through joins
                    // For now, we'll filter after mapping
                }

                // Get paginated employee IDs
                // EmployeeQueryRequest already extends PagerInfo, so use it directly
                var pagedResult = await _employeeService.GetPageListAsync(predicate.ToExpression(), request);

                // Convert to API DTOs
                var employeeIds = pagedResult.Result.Select(e => e.EmployeeId).ToList();
                var dtoList = await _mappingService.ToListDtoAsync(employeeIds);

                // Apply filters that require joined data
                var filteredList = dtoList.AsEnumerable();

                if (request.DepartmentId.HasValue)
                {
                    filteredList = filteredList.Where(e => e.DepartmentId == request.DepartmentId.Value);
                }

                if (request.PositionId.HasValue)
                {
                    filteredList = filteredList.Where(e => e.PositionId == request.PositionId.Value);
                }

                if (!string.IsNullOrEmpty(request.Name))
                {
                    filteredList = filteredList.Where(e =>
                        (e.FirstName?.Contains(request.Name, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.LastName?.Contains(request.Name, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.FullName?.Contains(request.Name, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                if (!string.IsNullOrEmpty(request.Email))
                {
                    filteredList = filteredList.Where(e =>
                        e.Email?.Contains(request.Email, StringComparison.OrdinalIgnoreCase) ?? false);
                }

                if (!string.IsNullOrEmpty(request.EmploymentType))
                {
                    filteredList = filteredList.Where(e => e.EmploymentType == request.EmploymentType);
                }

                var finalList = filteredList.ToList();

                // Return paginated response
                return SUCCESS(new PagedInfo<EmployeeListDto>
                {
                    PageIndex = pagedResult.PageIndex,
                    PageSize = pagedResult.PageSize,
                    TotalNum = finalList.Count, // Update total count after filtering
                    Result = finalList
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to get employee list: {ex.Message}");
            }
        }

        /// <summary>
        /// Get employee details by ID
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>Full employee details</returns>
        [HttpGet("{id}")]
        [ActionPermissionFilter("hr:employees:query")]
        public async Task<IActionResult> GetEmployee(long id)
        {
            try
            {
                var employee = await _mappingService.ToApiDtoAsync(id);
                if (employee == null)
                {
                    return ToResponse(ResultCode.FAIL, "Employee not found");
                }

                return SUCCESS(employee);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to get employee: {ex.Message}");
            }
        }

        /// <summary>
        /// Create new employee
        /// </summary>
        /// <param name="request">Employee creation data</param>
        /// <returns>Created employee details</returns>
        [HttpPost]
        [ActionPermissionFilter("hr:employees:add")]
        [Log(Title = "Employee Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.FirstName))
                {
                    return ToResponse(ResultCode.CUSTOM_ERROR, "First name is required");
                }

                if (string.IsNullOrWhiteSpace(request.LastName))
                {
                    return ToResponse(ResultCode.CUSTOM_ERROR, "Last name is required");
                }

                // Convert request to create DTO
                var createDto = MapToCreateDto(request);

                // Audit fields (Create_by, Create_time) are set automatically by the orchestration service

                // Create employee using orchestration service
                var result = await _orchestrationService.CreateCompleteEmployeeAsync(createDto);

                if (!result.Success)
                {
                    return ToResponse(ResultCode.FAIL, result.Message);
                }

                // Return created employee
                var createdEmployee = await _mappingService.ToApiDtoAsync(result.EmployeeId);
                return SUCCESS(createdEmployee);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to create employee: {ex.Message}");
            }
        }

        /// <summary>
        /// Update employee
        /// </summary>
        /// <param name="request">Employee update data</param>
        /// <returns>Updated employee details</returns>
        [HttpPut]
        [ActionPermissionFilter("hr:employees:edit")]
        [Log(Title = "Employee Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateEmployee([FromBody] UpdateEmployeeRequest request)
        {
            try
            {
                if (request.EmployeeId <= 0)
                {
                    return ToResponse(ResultCode.CUSTOM_ERROR, "Invalid employee ID");
                }

                // Check if employee exists
                var existingEmployee = await _employeeService.GetByIdAsync(request.EmployeeId);
                if (existingEmployee == null)
                {
                    return ToResponse(ResultCode.FAIL, "Employee not found");
                }

                // Convert request to update DTO
                var updateDto = await MapToUpdateDtoAsync(request, existingEmployee.PersonId);

                // Audit fields (Update_by, Update_time) are set automatically by the orchestration service

                // Update employee using orchestration service
                var success = await _orchestrationService.UpdateCompleteEmployeeAsync(updateDto);

                if (!success)
                {
                    return ToResponse(ResultCode.FAIL, "Failed to update employee");
                }

                // Return updated employee
                var updatedEmployee = await _mappingService.ToApiDtoAsync(request.EmployeeId);
                return SUCCESS(updatedEmployee);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to update employee: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete employee(s)
        /// </summary>
        /// <param name="ids">Comma-separated employee IDs</param>
        /// <returns>Success or failure</returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter("hr:employees:delete")]
        [Log(Title = "Employee Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteEmployee(string ids)
        {
            try
            {
                var idArray = Tools.SplitAndConvert<long>(ids);

                foreach (var id in idArray)
                {
                    var employee = await _employeeService.GetByIdAsync(id);
                    if (employee != null)
                    {
                        // Soft delete - mark as inactive
                        employee.IsActive = false;
                        employee.TerminationDate = DateTime.Now;
                        employee.Update_by = HttpContext.GetName();
                        employee.Update_time = DateTime.Now;
                        await _employeeService.UpdateAsync(employee);
                    }
                }

                return SUCCESS(idArray.Length);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to delete employee(s): {ex.Message}");
            }
        }

        /// <summary>
        /// Get employee statistics
        /// </summary>
        /// <returns>Statistics summary</returns>
        [HttpGet("statistics")]
        [ActionPermissionFilter("hr:employees:list")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var allEmployees = await _employeeService.GetListAsync(e => true);

                var stats = new EmployeeStatisticsResponse
                {
                    TotalEmployees = allEmployees.Count,
                    ActiveEmployees = allEmployees.Count(e => e.IsActive),
                    InactiveEmployees = allEmployees.Count(e => !e.IsActive),
                    TerminatedEmployees = allEmployees.Count(e => e.TerminationDate.HasValue),
                    NewHiresThisMonth = allEmployees.Count(e => e.HireDate.HasValue &&
                        e.HireDate.Value.Year == DateTime.Now.Year &&
                        e.HireDate.Value.Month == DateTime.Now.Month),
                    TerminationsThisMonth = allEmployees.Count(e => e.TerminationDate.HasValue &&
                        e.TerminationDate.Value.Year == DateTime.Now.Year &&
                        e.TerminationDate.Value.Month == DateTime.Now.Month),
                    ByDepartment = new Dictionary<string, int>(),
                    ByEmploymentType = new Dictionary<string, int>(),
                    ByNationality = new Dictionary<string, int>()
                };

                return SUCCESS(stats);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to get statistics: {ex.Message}");
            }
        }

        /// <summary>
        /// Export employees to Excel
        /// </summary>
        /// <param name="request">Filter criteria</param>
        /// <returns>Excel file</returns>
        [HttpGet("export")]
        [ActionPermissionFilter("hr:employees:export")]
        [Log(Title = "Employee Management", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportEmployees([FromQuery] EmployeeQueryRequest request)
        {
            try
            {
                var predicate = Expressionable.Create<ZR.Model.HR.EmployeeCore.HrEmployeeCore>();

                if (!string.IsNullOrEmpty(request.EmployeeCode))
                {
                    predicate = predicate.And(e => e.EmployeeCode.Contains(request.EmployeeCode));
                }

                var employees = await _employeeService.GetListAsync(predicate.ToExpression());
                var employeeIds = employees.Select(e => e.EmployeeId).ToList();
                var dtoList = await _mappingService.ToListDtoAsync(employeeIds);

                var result = ExportExcelMini(dtoList, "employees", "Employee List");
                return ExportExcel(result.Item2, result.Item1);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Failed to export employees: {ex.Message}");
            }
        }

        #region Helper Methods

        private CreateEmployeeCompleteDto MapToCreateDto(CreateEmployeeRequest request)
        {
            var dto = new CreateEmployeeCompleteDto
            {
                Person = new CreatePersonCompleteDto
                {
                    Person = new CreatePersonDto { IsActive = true },
                    Names = new List<CreatePersonNameDto>
                    {
                        new CreatePersonNameDto
                        {
                            GivenName = request.FirstName,
                            FamilyName = request.LastName,
                            FullNameAr = request.NameAr,
                            IsPrimary = true,
                            EffectiveFrom = request.HireDate
                        }
                    },
                    Demographic = new CreatePersonDemographicDto
                    {
                        DateOfBirth = request.DateOfBirth ?? DateTime.MinValue,
                        GenderDictCode = ParseDictCode(request.Gender),
                        MaritalStatusDictCode = ParseDictCode(request.MaritalStatus),
                        NationalityDictCode = ParseDictCode(request.Nationality)
                    },
                    Identifiers = new List<CreatePersonIdentifierDto>(),
                    Contacts = new List<CreatePersonContactDto>(),
                    Addresses = new List<CreatePersonAddressDto>()
                },
                Employee = new CreateEmployeeCoreDto
                {
                    HireDate = request.HireDate,
                    EmploymentTypeDictCode = GetDictCodeByValue("sys_employment_type", request.EmploymentType),
                    IsActive = true
                },
                Assignments = new List<CreateEmployeeAssignmentDto>(),
                Statuses = new List<CreateEmployeeStatusDto>(),
                Contracts = new List<CreateEmployeeContractDto>(),
                Statutory = new List<CreateEmployeeStatutoryDto>(),
                BankAccounts = new List<CreateEmployeeBankAccountDto>(),
                EmergencyContacts = new List<CreateEmployeeEmergencyContactDto>(),
                CompensationPackages = new List<CreateEmployeeCompensationPackageDto>(),
                Dependents = new List<CreateEmployeeDependentLinkDto>()
            };

            // Add Identifiers
            if (!string.IsNullOrEmpty(request.IdNumber))
            {
                dto.Person.Identifiers.Add(new CreatePersonIdentifierDto
                {
                    IdentifierTypeDictCode = 1, // National ID/Iqama
                    IdentifierNumber = request.IdNumber,
                    IsPrimary = true,
                    IsVerified = false
                });
            }

            if (!string.IsNullOrEmpty(request.PassportNumber))
            {
                dto.Person.Identifiers.Add(new CreatePersonIdentifierDto
                {
                    IdentifierTypeDictCode = 2, // Passport
                    IdentifierNumber = request.PassportNumber,
                    ExpiryDate = request.PassportExpiryDate,
                    IsPrimary = false,
                    IsVerified = false
                });
            }

            // Add Contacts
            if (!string.IsNullOrEmpty(request.Email))
            {
                dto.Person.Contacts.Add(new CreatePersonContactDto
                {
                    ContactTypeDictCode = 1, // Work Email
                    ContactValue = request.Email,
                    IsPrimary = true,
                    IsVerified = false,
                    EffectiveFrom = request.HireDate
                });
            }

            if (!string.IsNullOrEmpty(request.PersonalEmail))
            {
                dto.Person.Contacts.Add(new CreatePersonContactDto
                {
                    ContactTypeDictCode = 2, // Personal Email
                    ContactValue = request.PersonalEmail,
                    IsPrimary = false,
                    IsVerified = false,
                    EffectiveFrom = request.HireDate
                });
            }

            if (!string.IsNullOrEmpty(request.Phone))
            {
                dto.Person.Contacts.Add(new CreatePersonContactDto
                {
                    ContactTypeDictCode = 3, // Work Phone
                    ContactValue = request.Phone,
                    IsPrimary = true,
                    IsVerified = false,
                    EffectiveFrom = request.HireDate
                });
            }

            if (!string.IsNullOrEmpty(request.MobileNumber))
            {
                dto.Person.Contacts.Add(new CreatePersonContactDto
                {
                    ContactTypeDictCode = 4, // Mobile
                    ContactValue = request.MobileNumber,
                    IsPrimary = false,
                    IsVerified = false,
                    EffectiveFrom = request.HireDate
                });
            }

            // Add Address
            if (!string.IsNullOrEmpty(request.Address))
            {
                dto.Person.Addresses.Add(new CreatePersonAddressDto
                {
                    AddressTypeDictCode = 1, // Home Address
                    AddressLine1 = request.Address,
                    IsPrimary = true,
                    EffectiveFrom = request.HireDate
                });
            }

            // Add Assignment (Department, Position, Manager)
            if (request.DepartmentId.HasValue || request.PositionId.HasValue || request.ManagerId.HasValue)
            {
                dto.Assignments.Add(new CreateEmployeeAssignmentDto
                {
                    DepartmentId = request.DepartmentId,
                    PositionId = request.PositionId,
                    ManagerEmployeeId = request.ManagerId,
                    IsPrimary = true,
                    EffectiveFrom = request.HireDate
                });
            }

            // Add Initial Status
            dto.Statuses.Add(new CreateEmployeeStatusDto
            {
                EmploymentStatusDictCode = 1, // Active
                EffectiveFrom = request.HireDate
            });

            // Add Contract
            if (request.ContractStartDate.HasValue || request.ContractEndDate.HasValue)
            {
                dto.Contracts.Add(new CreateEmployeeContractDto
                {
                    ContractStartDate = request.ContractStartDate,
                    ContractEndDate = request.ContractEndDate,
                    IsCurrent = true,
                    EffectiveFrom = request.HireDate
                });
            }

            // Add Statutory
            if (!string.IsNullOrEmpty(request.GosiNumber) || !string.IsNullOrEmpty(request.MedicalInsuranceNumber) || request.IsSaudi)
            {
                dto.Statutory.Add(new CreateEmployeeStatutoryDto
                {
                    GosiNumber = request.GosiNumber,
                    MedicalInsuranceNumber = request.MedicalInsuranceNumber,
                    IsSaudi = request.IsSaudi,
                    HasGosi = !string.IsNullOrEmpty(request.GosiNumber),
                    EffectiveFrom = request.HireDate
                });
            }

            // Add Bank Account
            if (!string.IsNullOrEmpty(request.BankName) || !string.IsNullOrEmpty(request.Iban) || !string.IsNullOrEmpty(request.AccountNumber))
            {
                dto.BankAccounts.Add(new CreateEmployeeBankAccountDto
                {
                    BankName = request.BankName,
                    Iban = request.Iban,
                    AccountNumber = request.AccountNumber,
                    IsPrimary = true,
                    EffectiveFrom = request.HireDate
                });
            }

            // Add Emergency Contact
            if (!string.IsNullOrEmpty(request.EmergencyContactName))
            {
                dto.EmergencyContacts.Add(new CreateEmployeeEmergencyContactDto
                {
                    ContactName = request.EmergencyContactName,
                    Phone = request.EmergencyContactPhone,
                    IsPrimary = true,
                    IsActive = true
                });
            }

            return dto;
        }

        private async Task<UpdateEmployeeCompleteDto> MapToUpdateDtoAsync(UpdateEmployeeRequest request, long personId)
        {
            // Parse status once to use in multiple places
            var isActive = ParseStatus(request.Status);
            var isTerminated = request.Status?.Trim() == "2"; // Status "2" = TERMINATED

            var dto = new UpdateEmployeeCompleteDto
            {
                EmployeeId = request.EmployeeId,
                PersonId = personId,
                Employee = new UpdateEmployeeCoreDto
                {
                    EmployeeId = request.EmployeeId,
                    HireDate = request.HireDate,
                    EmploymentTypeDictCode = GetDictCodeByValue("sys_employment_type", request.EmploymentType),
                    IsActive = isActive,
                    TerminationDate = isTerminated ? DateTime.Now : (DateTime?)null  // Set termination date if status is TERMINATED
                },
                Names = new List<UpdatePersonNameDto>(),
                Contacts = new List<UpdatePersonContactDto>(),
                Identifiers = new List<UpdatePersonIdentifierDto>(),
                Addresses = new List<UpdatePersonAddressDto>(),
                Assignments = new List<UpdateEmployeeAssignmentDto>(),
                Statuses = new List<UpdateEmployeeStatusDto>(),
                Contracts = new List<UpdateEmployeeContractDto>(),
                Statutory = new List<UpdateEmployeeStatutoryDto>(),
                BankAccounts = new List<UpdateEmployeeBankAccountDto>()
            };

            // Update Name (if provided) - support both naming conventions
            if (!string.IsNullOrEmpty(request.GivenName) || !string.IsNullOrEmpty(request.FamilyName) ||
                !string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName) ||
                !string.IsNullOrEmpty(request.MiddleName) || !string.IsNullOrEmpty(request.FullNameAr) || !string.IsNullOrEmpty(request.NameAr))
            {
                var existingName = await _mappingService.GetPrimaryNameAsync(personId);
                if (existingName != null)
                {
                    dto.Names.Add(new UpdatePersonNameDto
                    {
                        PersonNameId = existingName.PersonNameId,
                        // Prefer new naming convention, fallback to old
                        GivenName = request.GivenName ?? request.FirstName ?? existingName.GivenName,
                        FamilyName = request.FamilyName ?? request.LastName ?? existingName.FamilyName,
                        MiddleName = request.MiddleName ?? existingName.MiddleName,
                        FullNameAr = request.FullNameAr ?? request.NameAr ?? existingName.FullNameAr,
                        IsPrimary = true,
                        EffectiveFrom = existingName.EffectiveFrom
                    });
                }
                else
                {
                    // Create new name if none exists
                    dto.Names.Add(new UpdatePersonNameDto
                    {
                        PersonNameId = 0, // New record
                        PersonId = personId, // REQUIRED for INSERT
                        GivenName = request.GivenName ?? request.FirstName,
                        FamilyName = request.FamilyName ?? request.LastName,
                        MiddleName = request.MiddleName,
                        FullNameAr = request.FullNameAr ?? request.NameAr,
                        IsPrimary = true,
                        EffectiveFrom = DateTime.Now
                    });
                }
            }

            // Update Demographic (if provided) - support both dictCode and string
            if (request.DateOfBirth.HasValue || request.GenderDictCode.HasValue || request.MaritalStatusDictCode.HasValue ||
                request.NationalityDictCode.HasValue || !string.IsNullOrEmpty(request.Gender) ||
                !string.IsNullOrEmpty(request.MaritalStatus) || !string.IsNullOrEmpty(request.Nationality))
            {
                dto.Demographic = new UpdatePersonDemographicDto
                {
                    PersonId = personId,
                    DateOfBirth = request.DateOfBirth ?? DateTime.MinValue,
                    // Prefer DictCode, fallback to parsing string
                    GenderDictCode = request.GenderDictCode ?? ParseDictCode(request.Gender),
                    MaritalStatusDictCode = request.MaritalStatusDictCode ?? ParseDictCode(request.MaritalStatus),
                    NationalityDictCode = request.NationalityDictCode ?? ParseDictCode(request.Nationality)
                };
            }

            // Update Contacts (if provided)
            if (!string.IsNullOrEmpty(request.Email))
            {
                var contact = await _mappingService.GetContactAsync(personId, 1); // Work Email
                if (contact != null)
                {
                    dto.Contacts.Add(new UpdatePersonContactDto
                    {
                        PersonContactId = contact.PersonContactId,
                        ContactTypeDictCode = 1,
                        ContactValue = request.Email,
                        IsPrimary = true,
                        EffectiveFrom = contact.EffectiveFrom
                    });
                }
                else
                {
                    // Create new work email if none exists
                    dto.Contacts.Add(new UpdatePersonContactDto
                    {
                        PersonContactId = 0, // New record
                        PersonId = personId, // REQUIRED for INSERT
                        ContactTypeDictCode = 1,
                        ContactValue = request.Email,
                        IsPrimary = true,
                        EffectiveFrom = DateTime.Now
                    });
                }
            }

            if (!string.IsNullOrEmpty(request.PersonalEmail))
            {
                var contact = await _mappingService.GetContactAsync(personId, 2); // Personal Email
                if (contact != null)
                {
                    dto.Contacts.Add(new UpdatePersonContactDto
                    {
                        PersonContactId = contact.PersonContactId,
                        ContactTypeDictCode = 2,
                        ContactValue = request.PersonalEmail,
                        IsPrimary = false,
                        EffectiveFrom = contact.EffectiveFrom
                    });
                }
                else
                {
                    // Create new personal email if none exists
                    dto.Contacts.Add(new UpdatePersonContactDto
                    {
                        PersonContactId = 0, // New record
                        PersonId = personId, // REQUIRED for INSERT
                        ContactTypeDictCode = 2,
                        ContactValue = request.PersonalEmail,
                        IsPrimary = false,
                        EffectiveFrom = DateTime.Now
                    });
                }
            }

            if (!string.IsNullOrEmpty(request.Phone))
            {
                var contact = await _mappingService.GetContactAsync(personId, 3); // Work Phone
                if (contact != null)
                {
                    dto.Contacts.Add(new UpdatePersonContactDto
                    {
                        PersonContactId = contact.PersonContactId,
                        ContactTypeDictCode = 3,
                        ContactValue = request.Phone,
                        IsPrimary = true,
                        EffectiveFrom = contact.EffectiveFrom
                    });
                }
                else
                {
                    // Create new work phone if none exists
                    dto.Contacts.Add(new UpdatePersonContactDto
                    {
                        PersonContactId = 0, // New record
                        PersonId = personId, // REQUIRED for INSERT
                        ContactTypeDictCode = 3,
                        ContactValue = request.Phone,
                        IsPrimary = true,
                        EffectiveFrom = DateTime.Now
                    });
                }
            }

            if (!string.IsNullOrEmpty(request.MobileNumber))
            {
                var contact = await _mappingService.GetContactAsync(personId, 4); // Mobile
                if (contact != null)
                {
                    dto.Contacts.Add(new UpdatePersonContactDto
                    {
                        PersonContactId = contact.PersonContactId,
                        ContactTypeDictCode = 4,
                        ContactValue = request.MobileNumber,
                        IsPrimary = false,
                        EffectiveFrom = contact.EffectiveFrom
                    });
                }
                else
                {
                    // Create new mobile if none exists
                    dto.Contacts.Add(new UpdatePersonContactDto
                    {
                        PersonContactId = 0, // New record
                        PersonId = personId, // REQUIRED for INSERT
                        ContactTypeDictCode = 4,
                        ContactValue = request.MobileNumber,
                        IsPrimary = false,
                        EffectiveFrom = DateTime.Now
                    });
                }
            }

            // Update Identifiers (if provided)
            // Type 1: National ID (Primary)
            if (!string.IsNullOrEmpty(request.IdNumber))
            {
                var identifier = await _mappingService.GetIdentifierByTypeAsync(personId, 1);
                if (identifier != null)
                {
                    dto.Identifiers.Add(new UpdatePersonIdentifierDto
                    {
                        PersonIdentifierId = identifier.PersonIdentifierId,
                        IdentifierTypeDictCode = 1,
                        IdentifierNumber = request.IdNumber,
                        ExpiryDate = request.IdExpiryDate,
                        IsPrimary = true
                    });
                }
                else
                {
                    // Create new identifier if none exists
                    dto.Identifiers.Add(new UpdatePersonIdentifierDto
                    {
                        PersonIdentifierId = 0, // New record
                        PersonId = personId, // REQUIRED for INSERT
                        IdentifierTypeDictCode = 1,
                        IdentifierNumber = request.IdNumber,
                        ExpiryDate = request.IdExpiryDate,
                        IsPrimary = true
                    });
                }
            }

            // Type 3: Iqama Number
            if (!string.IsNullOrEmpty(request.IqamaNumber))
            {
                var iqama = await _mappingService.GetIdentifierByTypeAsync(personId, 3);
                if (iqama != null)
                {
                    dto.Identifiers.Add(new UpdatePersonIdentifierDto
                    {
                        PersonIdentifierId = iqama.PersonIdentifierId,
                        IdentifierTypeDictCode = 3,
                        IdentifierNumber = request.IqamaNumber,
                        ExpiryDate = request.IqamaExpiryDate,
                        IsPrimary = false
                    });
                }
                else
                {
                    // Create new iqama if none exists
                    dto.Identifiers.Add(new UpdatePersonIdentifierDto
                    {
                        PersonIdentifierId = 0, // New record
                        PersonId = personId, // REQUIRED for INSERT
                        IdentifierTypeDictCode = 3,
                        IdentifierNumber = request.IqamaNumber,
                        ExpiryDate = request.IqamaExpiryDate,
                        IsPrimary = false
                    });
                }
            }

            // Type 2: Passport
            if (!string.IsNullOrEmpty(request.PassportNumber))
            {
                var passport = await _mappingService.GetIdentifierByTypeAsync(personId, 2);
                if (passport != null)
                {
                    dto.Identifiers.Add(new UpdatePersonIdentifierDto
                    {
                        PersonIdentifierId = passport.PersonIdentifierId,
                        IdentifierTypeDictCode = 2,
                        IdentifierNumber = request.PassportNumber,
                        ExpiryDate = request.PassportExpiryDate,
                        IsPrimary = false
                    });
                }
                else
                {
                    // Create new passport if none exists
                    dto.Identifiers.Add(new UpdatePersonIdentifierDto
                    {
                        PersonIdentifierId = 0, // New record
                        PersonId = personId, // REQUIRED for INSERT
                        IdentifierTypeDictCode = 2,
                        IdentifierNumber = request.PassportNumber,
                        ExpiryDate = request.PassportExpiryDate,
                        IsPrimary = false
                    });
                }
            }

            // Update Address (if provided)
            if (!string.IsNullOrEmpty(request.Address))
            {
                var address = await _mappingService.GetPrimaryAddressAsync(personId);
                if (address != null)
                {
                    dto.Addresses.Add(new UpdatePersonAddressDto
                    {
                        PersonAddressId = address.PersonAddressId,
                        AddressTypeDictCode = 1,
                        AddressLine1 = request.Address,
                        IsPrimary = true,
                        EffectiveFrom = address.EffectiveFrom
                    });
                }
                else
                {
                    // Create new address if none exists
                    dto.Addresses.Add(new UpdatePersonAddressDto
                    {
                        PersonAddressId = 0, // New record
                        PersonId = personId, // REQUIRED for INSERT
                        AddressTypeDictCode = 1,
                        AddressLine1 = request.Address,
                        IsPrimary = true,
                        EffectiveFrom = DateTime.Now
                    });
                }
            }

            // Update Assignment (if provided)
            if (request.DepartmentId.HasValue || request.PositionId.HasValue || request.ManagerId.HasValue)
            {
                var assignment = await _mappingService.GetCurrentAssignmentAsync(request.EmployeeId);
                if (assignment != null)
                {
                    dto.Assignments.Add(new UpdateEmployeeAssignmentDto
                    {
                        EmployeeAssignmentId = assignment.EmployeeAssignmentId,
                        DepartmentId = request.DepartmentId ?? assignment.DepartmentId,
                        PositionId = request.PositionId ?? assignment.PositionId,
                        ManagerEmployeeId = request.ManagerId ?? assignment.ManagerEmployeeId,
                        IsPrimary = true,
                        EffectiveFrom = assignment.EffectiveFrom
                    });
                }
                else
                {
                    // Create new assignment if none exists
                    dto.Assignments.Add(new UpdateEmployeeAssignmentDto
                    {
                        EmployeeAssignmentId = 0, // New record
                        EmployeeId = request.EmployeeId, // REQUIRED for INSERT
                        DepartmentId = request.DepartmentId,
                        PositionId = request.PositionId,
                        ManagerEmployeeId = request.ManagerId,
                        IsPrimary = true,
                        EffectiveFrom = DateTime.Now
                    });
                }
            }

            // Update Contract (if provided)
            if (request.ContractStartDate.HasValue || request.ContractEndDate.HasValue)
            {
                var contract = await _mappingService.GetCurrentContractAsync(request.EmployeeId);
                if (contract != null)
                {
                    dto.Contracts.Add(new UpdateEmployeeContractDto
                    {
                        EmployeeContractId = contract.EmployeeContractId,
                        ContractStartDate = request.ContractStartDate ?? contract.ContractStartDate,
                        ContractEndDate = request.ContractEndDate ?? contract.ContractEndDate,
                        IsCurrent = true,
                        EffectiveFrom = contract.EffectiveFrom
                    });
                }
            }

            // Update Statutory (if provided)
            if (!string.IsNullOrEmpty(request.GosiNumber) || !string.IsNullOrEmpty(request.MedicalInsuranceNumber))
            {
                var statutory = await _mappingService.GetCurrentStatutoryAsync(request.EmployeeId);
                if (statutory != null)
                {
                    dto.Statutory.Add(new UpdateEmployeeStatutoryDto
                    {
                        EmployeeStatutoryId = statutory.EmployeeStatutoryId,
                        GosiNumber = request.GosiNumber ?? statutory.GosiNumber,
                        MedicalInsuranceNumber = request.MedicalInsuranceNumber ?? statutory.MedicalInsuranceNumber,
                        IsSaudi = statutory.IsSaudi,
                        HasGosi = !string.IsNullOrEmpty(request.GosiNumber ?? statutory.GosiNumber),
                        EffectiveFrom = statutory.EffectiveFrom
                    });
                }
            }

            // Update Bank Account (if provided)
            if (!string.IsNullOrEmpty(request.BankName) || !string.IsNullOrEmpty(request.Iban) || !string.IsNullOrEmpty(request.AccountNumber))
            {
                var bankAccount = await _mappingService.GetPrimaryBankAccountAsync(request.EmployeeId);
                if (bankAccount != null)
                {
                    // UPDATE existing bank account
                    dto.BankAccounts.Add(new UpdateEmployeeBankAccountDto
                    {
                        EmployeeBankAccountId = bankAccount.EmployeeBankAccountId,
                        BankName = request.BankName ?? bankAccount.BankName,
                        Iban = request.Iban,
                        AccountNumber = request.AccountNumber,
                        IsPrimary = true,
                        EffectiveFrom = bankAccount.EffectiveFrom
                    });
                }
                else
                {
                    // CREATE new bank account (ID=0 pattern)
                    dto.BankAccounts.Add(new UpdateEmployeeBankAccountDto
                    {
                        EmployeeBankAccountId = 0, // Signal for new record
                        EmployeeId = request.EmployeeId, // Required for CREATE
                        BankName = request.BankName,
                        Iban = request.Iban,
                        AccountNumber = request.AccountNumber,
                        IsPrimary = true,
                        EffectiveFrom = DateTime.Now
                    });
                }
            }

            // Update Emergency Contact (if provided)
            if (!string.IsNullOrEmpty(request.EmergencyContactName) || !string.IsNullOrEmpty(request.EmergencyContactPhone))
            {
                var emergencyContact = await _mappingService.GetPrimaryEmergencyContactAsync(request.EmployeeId);
                if (emergencyContact != null)
                {
                    // UPDATE existing emergency contact
                    dto.EmergencyContacts = new List<UpdateEmployeeEmergencyContactDto>
                    {
                        new UpdateEmployeeEmergencyContactDto
                        {
                            EmergencyContactId = emergencyContact.EmergencyContactId,
                            ContactName = request.EmergencyContactName ?? emergencyContact.ContactName,
                            Phone = request.EmergencyContactPhone ?? emergencyContact.Phone,
                            IsPrimary = true,
                            IsActive = true
                        }
                    };
                }
                else
                {
                    // CREATE new emergency contact (ID=0 pattern)
                    dto.EmergencyContacts = new List<UpdateEmployeeEmergencyContactDto>
                    {
                        new UpdateEmployeeEmergencyContactDto
                        {
                            EmergencyContactId = 0, // Signal for new record
                            EmployeeId = request.EmployeeId, // Required for CREATE
                            ContactName = request.EmergencyContactName,
                            Phone = request.EmergencyContactPhone,
                            IsPrimary = true,
                            IsActive = true
                        }
                    };
                }
            }

            return dto;
        }

        private long? ParseDictCode(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (long.TryParse(value, out var code)) return code;
            return null;
        }

        /// <summary>
        /// Get dict_code by looking up dict_value in a specific dict_type
        /// Used for employment_type, gender, etc. where frontend sends dictValue
        /// </summary>
        private long? GetDictCodeByValue(string dictType, string dictValue)
        {
            if (string.IsNullOrEmpty(dictValue)) return null;

            try
            {
                var dictDataService = App.GetService<ISysDictDataService>();
                var dictList = dictDataService.SelectDictDataByType(dictType);
                var dictEntry = dictList?.FirstOrDefault(d => d.DictValue == dictValue);
                return dictEntry?.DictCode;
            }
            catch
            {
                return null;
            }
        }

        private bool ParseStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return false;

            var statusTrimmed = status.Trim();

            // Map dictionary values to boolean IsActive
            // Dictionary: sys_hr_employee_status
            // "0" = ACTIVE → IsActive = true
            // "1" = SUSPENDED → IsActive = false
            // "2" = TERMINATED → IsActive = false
            // "3" = ON_LEAVE → IsActive = true (employee still active, just on leave)
            if (statusTrimmed == "0" || statusTrimmed == "3")
            {
                return true; // ACTIVE or ON_LEAVE
            }
            if (statusTrimmed == "1" || statusTrimmed == "2")
            {
                return false; // SUSPENDED or TERMINATED
            }

            // Fallback: Map string literals (backwards compatibility)
            var statusLower = statusTrimmed.ToLower();
            return statusLower == "active" || statusLower == "on_leave";

            // "active" → true
            // "inactive" or "terminated" or "suspended" → false
        }

        #endregion
    }
}
