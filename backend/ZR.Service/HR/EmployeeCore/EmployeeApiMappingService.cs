using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;
using ZR.ServiceCore.Services;
using ZR.ServiceCore.Security;
using SqlSugar;

namespace ZR.Service.HR.EmployeeCore
{
    /// <summary>
    /// Employee API Mapping Service - Converts between EmployeeCore (normalized) and API DTOs (flat)
    /// Modern clean mapping without legacy adapter overhead
    /// </summary>
    [AppService(ServiceType = typeof(IEmployeeApiMappingService), ServiceLifetime = LifeTime.Transient)]
    public class EmployeeApiMappingService : BaseService<HrEmployeeCore>, IEmployeeApiMappingService
    {
        private readonly IHrPersonService _personService;
        private readonly IHrPersonNameService _personNameService;
        private readonly IHrPersonDemographicService _demographicService;
        private readonly IHrPersonIdentifierService _identifierService;
        private readonly IHrPersonContactService _contactService;
        private readonly IHrPersonAddressService _addressService;
        private readonly IHrEmployeeCoreService _employeeService;
        private readonly IHrEmployeeAssignmentService _assignmentService;
        private readonly IHrEmployeeStatusService _statusService;
        private readonly IHrEmployeeContractService _contractService;
        private readonly IHrEmployeeStatutoryService _statutoryService;
        private readonly IHrEmployeeBankAccountService _bankAccountService;
        private readonly IHrEmployeeEmergencyContactService _emergencyContactService;
        private readonly ISensitiveDataProtector _dataProtector;
        private readonly ISysDictDataService _dictDataService;
        private readonly ISysDeptService _deptService;
        private readonly ISysPostService _postService;

        public EmployeeApiMappingService(
            IHrPersonService personService,
            IHrPersonNameService personNameService,
            IHrPersonDemographicService demographicService,
            IHrPersonIdentifierService identifierService,
            IHrPersonContactService contactService,
            IHrPersonAddressService addressService,
            IHrEmployeeCoreService employeeService,
            IHrEmployeeAssignmentService assignmentService,
            IHrEmployeeStatusService statusService,
            IHrEmployeeContractService contractService,
            IHrEmployeeStatutoryService statutoryService,
            IHrEmployeeBankAccountService bankAccountService,
            IHrEmployeeEmergencyContactService emergencyContactService,
            ISensitiveDataProtector dataProtector,
            ISysDictDataService dictDataService,
            ISysDeptService deptService,
            ISysPostService postService)
        {
            _personService = personService;
            _personNameService = personNameService;
            _demographicService = demographicService;
            _identifierService = identifierService;
            _contactService = contactService;
            _addressService = addressService;
            _employeeService = employeeService;
            _assignmentService = assignmentService;
            _statusService = statusService;
            _contractService = contractService;
            _statutoryService = statutoryService;
            _bankAccountService = bankAccountService;
            _emergencyContactService = emergencyContactService;
            _dataProtector = dataProtector;
            _dictDataService = dictDataService;
            _deptService = deptService;
            _postService = postService;
        }

        /// <summary>
        /// Convert EmployeeCore to API DTO (full details)
        /// </summary>
        public async Task<EmployeeApiDto> ToApiDtoAsync(long employeeId)
        {
            try
            {
                // Get employee core
                var employee = await _employeeService.GetByIdAsync(employeeId);
                if (employee == null) return null;

                var dto = new EmployeeApiDto
                {
                    EmployeeId = employee.EmployeeId,
                    PersonId = employee.PersonId,
                    EmployeeCode = employee.EmployeeCode,
                    UserId = employee.UserId,
                    HireDate = employee.HireDate,
                    TerminationDate = employee.TerminationDate,
                    IsActive = employee.IsActive,
                    Status = MapEmployeeToStatusString(employee)
                };

                // Get primary name
                var name = await GetPrimaryNameAsync(employee.PersonId);
                if (name != null)
                {
                    // Populate both naming conventions
                    dto.FirstName = name.GivenName;
                    dto.LastName = name.FamilyName;
                    dto.NameAr = name.FullNameAr;

                    // Also populate normalized person name fields
                    dto.GivenName = name.GivenName;
                    dto.FamilyName = name.FamilyName;
                    dto.MiddleName = name.MiddleName;
                    dto.FullNameAr = name.FullNameAr;
                }

                // Get demographic
                var demographic = await _demographicService.GetByIdAsync(employee.PersonId);
                if (demographic != null)
                {
                    dto.DateOfBirth = demographic.DateOfBirth;
                    dto.Gender = demographic.GenderDictCode?.ToString();
                    dto.GenderLabel = GetDictLabel("sys_gender", demographic.GenderDictCode);
                    dto.MaritalStatus = demographic.MaritalStatusDictCode?.ToString();
                    dto.MaritalStatusLabel = GetDictLabel("sys_marital_status", demographic.MaritalStatusDictCode);
                    dto.Nationality = demographic.NationalityDictCode?.ToString();
                    dto.NationalityLabel = GetDictLabel("sys_user_nationality", demographic.NationalityDictCode);
                }

                // Get identifiers
                // Type 1: National ID (Primary)
                var primaryId = await GetIdentifierByTypeAsync(employee.PersonId, 1);
                if (primaryId != null)
                {
                    dto.IdNumber = primaryId.IdentifierNumber;
                    dto.IdExpiryDate = primaryId.ExpiryDate;
                }

                // Type 2: Passport
                var passport = await GetIdentifierByTypeAsync(employee.PersonId, 2);
                if (passport != null)
                {
                    dto.PassportNumber = passport.IdentifierNumber;
                    dto.PassportExpiryDate = passport.ExpiryDate;
                }

                // Type 3: Iqama
                var iqama = await GetIdentifierByTypeAsync(employee.PersonId, 3);
                if (iqama != null)
                {
                    dto.IqamaNumber = iqama.IdentifierNumber;
                    dto.IqamaExpiryDate = iqama.ExpiryDate;
                }

                // Get contacts
                var workEmail = await GetContactAsync(employee.PersonId, 1); // Work Email
                if (workEmail != null) dto.Email = workEmail.ContactValue;

                var personalEmail = await GetContactAsync(employee.PersonId, 2); // Personal Email
                if (personalEmail != null) dto.PersonalEmail = personalEmail.ContactValue;

                var workPhone = await GetContactAsync(employee.PersonId, 3); // Work Phone
                if (workPhone != null) dto.Phone = workPhone.ContactValue;

                var mobile = await GetContactAsync(employee.PersonId, 4); // Mobile
                if (mobile != null) dto.MobileNumber = mobile.ContactValue;

                // Get address
                var address = await GetPrimaryAddressAsync(employee.PersonId);
                if (address != null)
                {
                    dto.Address = $"{address.AddressLine1} {address.AddressLine2}".Trim();
                }

                // Get current assignment
                var assignment = await GetCurrentAssignmentAsync(employeeId);
                if (assignment != null)
                {
                    dto.DepartmentId = assignment.DepartmentId;
                    dto.DepartmentName = await GetDepartmentNameAsync(assignment.DepartmentId);
                    dto.PositionId = assignment.PositionId;
                    dto.PositionName = await GetPositionNameAsync(assignment.PositionId);
                    dto.ManagerId = assignment.ManagerEmployeeId;
                    if (assignment.ManagerEmployeeId.HasValue)
                    {
                        dto.ManagerName = await GetEmployeeNameAsync(assignment.ManagerEmployeeId.Value);
                    }
                    dto.WorkLocation = GetDictLabel("work_location", assignment.WorkLocationId);
                }

                // Get employment type
                dto.EmploymentType = GetDictValue(employee.EmploymentTypeDictCode);  // Return DictValue for frontend
                dto.EmploymentTypeLabel = GetDictLabel("employment_type", employee.EmploymentTypeDictCode);

                // Get current contract
                var contract = await GetCurrentContractAsync(employeeId);
                if (contract != null)
                {
                    dto.ContractStartDate = contract.ContractStartDate;
                    dto.ContractEndDate = contract.ContractEndDate;
                }

                // Get statutory
                var statutory = await GetCurrentStatutoryAsync(employeeId);
                if (statutory != null)
                {
                    dto.GosiNumber = statutory.GosiNumber;
                    dto.MedicalInsuranceNumber = statutory.MedicalInsuranceNumber;
                    dto.IsSaudi = statutory.IsSaudi;
                }

                // Get bank account (masked)
                var bankAccount = await GetPrimaryBankAccountAsync(employeeId);
                if (bankAccount != null)
                {
                    dto.BankName = bankAccount.BankName;
                    // Decrypt and mask sensitive data
                    if (!string.IsNullOrEmpty(bankAccount.IbanEncrypted))
                    {
                        var iban = _dataProtector.Unprotect(bankAccount.IbanEncrypted);
                        dto.Iban = _dataProtector.MaskIban(iban);
                    }
                    if (!string.IsNullOrEmpty(bankAccount.AccountNumberEncrypted))
                    {
                        var accNum = _dataProtector.Unprotect(bankAccount.AccountNumberEncrypted);
                        dto.AccountNumber = _dataProtector.MaskAccountNumber(accNum);
                    }
                }

                // Get emergency contact
                var emergencyContact = await GetPrimaryEmergencyContactAsync(employeeId);
                if (emergencyContact != null)
                {
                    dto.EmergencyContactName = emergencyContact.ContactName;
                    dto.EmergencyContactPhone = emergencyContact.Phone;
                }

                // Audit fields
                dto.CreateBy = employee.Create_by;
                dto.CreateTime = employee.Create_time;
                dto.UpdateBy = employee.Update_by;
                dto.UpdateTime = employee.Update_time;
                dto.Remark = employee.Remark;

                return dto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to map employee {employeeId} to API DTO", ex);
            }
        }

        /// <summary>
        /// Convert EmployeeCore to List DTO (lightweight)
        /// </summary>
        public async Task<EmployeeListDto> ToListDtoAsync(long employeeId)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null) return null;

            var dto = new EmployeeListDto
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                IsActive = employee.IsActive,
                Status = MapEmployeeToStatusString(employee),
                HireDate = employee.HireDate,
                EmploymentType = GetDictValue(employee.EmploymentTypeDictCode)  // Return DictValue for frontend
            };

            // Get name
            var name = await GetPrimaryNameAsync(employee.PersonId);
            if (name != null)
            {
                dto.FirstName = name.GivenName;
                dto.LastName = name.FamilyName;
            }

            // Get contacts
            var email = await GetContactAsync(employee.PersonId, 1);
            if (email != null) dto.Email = email.ContactValue;

            var phone = await GetContactAsync(employee.PersonId, 3);
            if (phone != null) dto.Phone = phone.ContactValue;

            // Get assignment
            var assignment = await GetCurrentAssignmentAsync(employeeId);
            if (assignment != null)
            {
                dto.DepartmentId = assignment.DepartmentId;
                dto.DepartmentName = await GetDepartmentNameAsync(assignment.DepartmentId);
                dto.PositionId = assignment.PositionId;
                dto.PositionName = await GetPositionNameAsync(assignment.PositionId);
            }

            return dto;
        }

        /// <summary>
        /// Convert list of employees to List DTOs
        /// </summary>
        public async Task<List<EmployeeListDto>> ToListDtoAsync(List<long> employeeIds)
        {
            var results = new List<EmployeeListDto>();
            foreach (var id in employeeIds)
            {
                var dto = await ToListDtoAsync(id);
                if (dto != null) results.Add(dto);
            }
            return results;
        }

        #region Helper Methods

        public async Task<HrPersonName> GetPrimaryNameAsync(long personId)
        {
            var now = DateTime.Now;
            return await Context.Queryable<HrPersonName>()
                .Where(n => n.PersonId == personId && n.IsPrimary == true)
                // Include both current and future-dated records (for employees not yet started)
                .Where(n => n.EffectiveTo == null || n.EffectiveTo > now)
                .OrderByDescending(n => n.EffectiveFrom)
                .FirstAsync();
        }

        public async Task<HrPersonIdentifier> GetPrimaryIdentifierAsync(long personId)
        {
            return await Context.Queryable<HrPersonIdentifier>()
                .Where(i => i.PersonId == personId && i.IsPrimary == true)
                .FirstAsync();
        }

        public async Task<HrPersonIdentifier> GetPassportAsync(long personId)
        {
            return await Context.Queryable<HrPersonIdentifier>()
                .Where(i => i.PersonId == personId && i.IdentifierTypeDictCode == 2) // Passport
                .FirstAsync();
        }

        public async Task<HrPersonIdentifier> GetIdentifierByTypeAsync(long personId, long identifierTypeCode)
        {
            return await Context.Queryable<HrPersonIdentifier>()
                .Where(i => i.PersonId == personId && i.IdentifierTypeDictCode == identifierTypeCode)
                .FirstAsync();
        }

        public async Task<HrPersonContact> GetContactAsync(long personId, long contactTypeCode)
        {
            var now = DateTime.Now;
            return await Context.Queryable<HrPersonContact>()
                .Where(c => c.PersonId == personId && c.ContactTypeDictCode == contactTypeCode)
                // Include both current and future-dated records (for employees not yet started)
                .Where(c => c.EffectiveTo == null || c.EffectiveTo > now)
                .OrderBy(c => c.IsPrimary, OrderByType.Desc)
                .OrderBy(c => c.EffectiveFrom, OrderByType.Desc)
                .FirstAsync();
        }

        public async Task<HrPersonAddress> GetPrimaryAddressAsync(long personId)
        {
            var now = DateTime.Now;
            return await Context.Queryable<HrPersonAddress>()
                .Where(a => a.PersonId == personId && a.IsPrimary == true)
                // Include both current and future-dated records (for employees not yet started)
                .Where(a => a.EffectiveTo == null || a.EffectiveTo > now)
                .OrderByDescending(a => a.EffectiveFrom)
                .FirstAsync();
        }

        public async Task<HrEmployeeAssignment> GetCurrentAssignmentAsync(long employeeId)
        {
            var now = DateTime.Now;
            return await Context.Queryable<HrEmployeeAssignment>()
                .Where(a => a.EmployeeId == employeeId && a.IsPrimary == true)
                // Include both current and future-dated records (for employees not yet started)
                .Where(a => a.EffectiveTo == null || a.EffectiveTo > now)
                .OrderByDescending(a => a.EffectiveFrom)
                .FirstAsync();
        }

        public async Task<HrEmployeeContract> GetCurrentContractAsync(long employeeId)
        {
            return await Context.Queryable<HrEmployeeContract>()
                .Where(c => c.EmployeeId == employeeId && c.IsCurrent == true)
                .OrderByDescending(c => c.EffectiveFrom)
                .FirstAsync();
        }

        public async Task<HrEmployeeStatutory> GetCurrentStatutoryAsync(long employeeId)
        {
            var now = DateTime.Now;
            return await Context.Queryable<HrEmployeeStatutory>()
                .Where(s => s.EmployeeId == employeeId)
                // Include both current and future-dated records (for employees not yet started)
                .Where(s => s.EffectiveTo == null || s.EffectiveTo > now)
                .OrderByDescending(s => s.EffectiveFrom)
                .FirstAsync();
        }

        public async Task<HrEmployeeBankAccount> GetPrimaryBankAccountAsync(long employeeId)
        {
            var now = DateTime.Now;
            return await Context.Queryable<HrEmployeeBankAccount>()
                .Where(b => b.EmployeeId == employeeId && b.IsPrimary == true)
                // Include both current and future-dated records (for employees not yet started)
                .Where(b => b.EffectiveTo == null || b.EffectiveTo > now)
                .OrderByDescending(b => b.EffectiveFrom)
                .FirstAsync();
        }

        public async Task<HrEmployeeEmergencyContact> GetPrimaryEmergencyContactAsync(long employeeId)
        {
            return await Context.Queryable<HrEmployeeEmergencyContact>()
                .Where(e => e.EmployeeId == employeeId && e.IsPrimary == true && e.IsActive == true)
                .FirstAsync();
        }

        private async Task<string> GetDepartmentNameAsync(long? deptId)
        {
            if (!deptId.HasValue) return null;
            var dept = await _deptService.GetByIdAsync(deptId.Value);
            return dept?.DeptName;
        }

        private async Task<string> GetPositionNameAsync(long? postId)
        {
            if (!postId.HasValue) return null;
            var post = await _postService.GetByIdAsync(postId.Value);
            return post?.PostName;
        }

        private async Task<string> GetEmployeeNameAsync(long employeeId)
        {
            var emp = await _employeeService.GetByIdAsync(employeeId);
            if (emp == null) return null;
            var name = await GetPrimaryNameAsync(emp.PersonId);
            return name != null ? $"{name.GivenName} {name.FamilyName}".Trim() : null;
        }

        private string GetDictLabel(string dictType, long? dictCode)
        {
            if (!dictCode.HasValue) return null;
            try
            {
                var data = _dictDataService?.SelectDictDataById(dictCode.Value);
                return data?.DictLabel ?? dictCode.Value.ToString();
            }
            catch
            {
                return dictCode.Value.ToString();
            }
        }

        /// <summary>
        /// Get DictValue (not DictLabel) for frontend compatibility
        /// Frontend uses DictValue for translations and select matching
        /// </summary>
        private string GetDictValue(long? dictCode)
        {
            if (!dictCode.HasValue) return null;
            try
            {
                var data = _dictDataService?.SelectDictDataById(dictCode.Value);
                return data?.DictValue ?? dictCode.Value.ToString();
            }
            catch
            {
                return dictCode.Value.ToString();
            }
        }

        /// <summary>
        /// Map employee status to dictValue (not string label) based on IsActive flag and TerminationDate
        /// Returns dictValue for frontend compatibility (frontend uses dictValue for translations)
        /// Dictionary: sys_hr_employee_status
        /// 0 = ACTIVE, 1 = SUSPENDED, 2 = TERMINATED, 3 = ON_LEAVE
        /// </summary>
        private string MapEmployeeToStatusString(HrEmployeeCore employee)
        {
            if (employee == null) return "1";  // Default to suspended

            // Check termination date first
            if (employee.TerminationDate.HasValue && employee.TerminationDate.Value <= DateTime.Now)
            {
                return "2";  // TERMINATED (dictValue)
            }

            // Check active status
            if (employee.IsActive)
            {
                return "0";  // ACTIVE (dictValue)
            }
            else
            {
                // Inactive but not terminated = suspended
                return "1";  // SUSPENDED (dictValue)
            }
        }

        #endregion
    }

    /// <summary>
    /// Interface for Employee API Mapping Service
    /// </summary>
    public interface IEmployeeApiMappingService
    {
        Task<EmployeeApiDto> ToApiDtoAsync(long employeeId);
        Task<EmployeeListDto> ToListDtoAsync(long employeeId);
        Task<List<EmployeeListDto>> ToListDtoAsync(List<long> employeeIds);

        // Helper methods for controller mapping
        Task<HrPersonName> GetPrimaryNameAsync(long personId);
        Task<HrPersonContact> GetContactAsync(long personId, long contactTypeCode);
        Task<HrPersonIdentifier> GetPrimaryIdentifierAsync(long personId);
        Task<HrPersonIdentifier> GetPassportAsync(long personId);
        Task<HrPersonIdentifier> GetIdentifierByTypeAsync(long personId, long identifierTypeCode);
        Task<HrPersonAddress> GetPrimaryAddressAsync(long personId);
        Task<HrEmployeeAssignment> GetCurrentAssignmentAsync(long employeeId);
        Task<HrEmployeeContract> GetCurrentContractAsync(long employeeId);
        Task<HrEmployeeStatutory> GetCurrentStatutoryAsync(long employeeId);
        Task<HrEmployeeBankAccount> GetPrimaryBankAccountAsync(long employeeId);
        Task<HrEmployeeEmergencyContact> GetPrimaryEmergencyContactAsync(long employeeId);
    }
}
