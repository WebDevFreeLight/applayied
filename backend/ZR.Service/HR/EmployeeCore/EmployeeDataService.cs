using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Model.HR.EmployeeCore;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR.EmployeeCore
{
    /// <summary>
    /// SINGLE SOURCE OF TRUTH for employee data access
    /// ALL complex joins happen HERE - other services just call these methods
    ///
    /// IMPORTANT: This uses ACTUAL model properties:
    /// - HrPersonName has GivenName/FamilyName (NOT FirstName/LastName!)
    /// - HrPersonContact uses ContactValue + dict codes (NOT Email/Phone properties!)
    /// - HrPersonDemographic uses dict codes (GenderDictCode, NationalityDictCode, etc.)
    /// - HrEmployeeStatus uses EmploymentStatusDictCode (NOT CurrentStatus!)
    /// - HrEmployeeBankAccount has encrypted fields (IbanEncrypted, AccountNumberEncrypted)
    /// - HrEmployeeStatutory has GosiNumber, IsSaudi, MedicalInsuranceNumber
    /// </summary>
    [AppService(ServiceType = typeof(IEmployeeDataService), ServiceLifetime = LifeTime.Transient)]
    public class EmployeeDataService : BaseService<HrEmployeeCore>, IEmployeeDataService
    {
        public EmployeeDataService() : base() { }

        /// <summary>
        /// Get complete employee data - THIS IS THE METHOD EVERYONE SHOULD USE
        /// All joins happen here, once, correctly
        /// </summary>
        public async Task<EmployeeFullDto> GetEmployeeFullDataAsync(long employeeId)
        {
            var now = DateTime.Now;

            var result = await Context.Queryable<HrEmployeeCore>()
                // Join with Person Name (required)
                .LeftJoin<HrPersonName>((core, name) =>
                    core.PersonId == name.PersonId &&
                    name.EffectiveFrom <= now &&
                    (name.EffectiveTo == null || name.EffectiveTo > now) &&
                    name.IsPrimary)
                // Join with Person Demographics (optional)
                .LeftJoin<HrPersonDemographic>((core, name, demo) =>
                    core.PersonId == demo.PersonId)
                // Join with Employee Assignment (optional - current)
                .LeftJoin<HrEmployeeAssignment>((core, name, demo, assign) =>
                    core.EmployeeId == assign.EmployeeId &&
                    assign.EffectiveFrom <= now &&
                    (assign.EffectiveTo == null || assign.EffectiveTo > now) &&
                    assign.IsPrimary)
                // Join with Employee Status (optional - current)
                .LeftJoin<HrEmployeeStatus>((core, name, demo, assign, status) =>
                    core.EmployeeId == status.EmployeeId &&
                    status.EffectiveFrom <= now &&
                    (status.EffectiveTo == null || status.EffectiveTo > now))
                // Join with Employee Contract (optional - current)
                .LeftJoin<HrEmployeeContract>((core, name, demo, assign, status, contract) =>
                    core.EmployeeId == contract.EmployeeId &&
                    contract.EffectiveFrom <= now &&
                    (contract.EffectiveTo == null || contract.EffectiveTo > now))
                // Join with Employee Statutory (optional)
                .LeftJoin<HrEmployeeStatutory>((core, name, demo, assign, status, contract, statutory) =>
                    core.EmployeeId == statutory.EmployeeId)
                // Join with Employee Bank Account (optional - primary)
                .LeftJoin<HrEmployeeBankAccount>((core, name, demo, assign, status, contract, statutory, bank) =>
                    core.EmployeeId == bank.EmployeeId && bank.IsPrimary)
                .Where((core) => core.EmployeeId == employeeId)
                .Select((core, name, demo, assign, status, contract, statutory, bank) => new EmployeeFullDto
                {
                    // Core
                    EmployeeId = core.EmployeeId,
                    PersonId = core.PersonId,
                    UserId = core.UserId,
                    EmployeeCode = core.EmployeeCode,
                    HireDate = core.HireDate,
                    IsActive = core.IsActive,

                    // Name - ACTUAL properties from HrPersonName
                    GivenName = name.GivenName ?? "",
                    FamilyName = name.FamilyName ?? "",
                    MiddleName = name.MiddleName ?? "",
                    FullNameAr = name.FullNameAr ?? "",
                    // Note: FirstName/LastName are computed properties in DTO (aliases)

                    // Contact - NOTE: HrPersonContact uses ContactValue + dict codes
                    // Will be populated in Phase 2 with proper dict lookups
                    Email = "",  // TODO: Query HrPersonContact with ContactTypeDictCode
                    Phone = "",  // TODO: Query HrPersonContact with ContactTypeDictCode
                    MobileNumber = "",  // TODO: Query HrPersonContact with ContactTypeDictCode

                    // Demographics - DICT CODES from HrPersonDemographic
                    GenderDictCode = demo.GenderDictCode,
                    MaritalStatusDictCode = demo.MaritalStatusDictCode,
                    NationalityDictCode = demo.NationalityDictCode,
                    DateOfBirth = demo.DateOfBirth,
                    // String properties left empty for now (can be filled with dict lookups later)
                    Gender = "",  // TODO: Lookup from DictCode
                    MaritalStatus = "",  // TODO: Lookup from DictCode
                    Nationality = "",  // TODO: Lookup from DictCode

                    // Assignment - ACTUAL properties from HrEmployeeAssignment
                    DepartmentId = assign.DepartmentId,
                    PositionId = assign.PositionId,
                    ManagerEmployeeId = assign.ManagerEmployeeId,
                    WorkLocationId = assign.WorkLocationId,

                    // Status - DICT CODE from HrEmployeeStatus
                    EmploymentStatusDictCode = status.EmploymentStatusDictCode,
                    Status = "",  // TODO: Lookup from DictCode

                    // Contract - ACTUAL properties from HrEmployeeContract
                    EmployeeContractId = contract.EmployeeContractId,
                    ContractTypeDictCode = contract.ContractTypeDictCode,
                    ContractStartDate = contract.ContractStartDate,
                    ContractEndDate = contract.ContractEndDate,
                    ContractType = "",  // TODO: Lookup from DictCode

                    // Bank - ACTUAL properties from HrEmployeeBankAccount
                    BankName = bank.BankName ?? "",
                    BankAccountEncrypted = bank.BankAccountEncrypted ?? "",
                    IbanEncrypted = bank.IbanEncrypted ?? "",
                    AccountNumberEncrypted = bank.AccountNumberEncrypted ?? "",
                    // Decrypted properties left empty for now (will be decrypted when needed)
                    BankAccountNumber = "",  // TODO: Decrypt from BankAccountEncrypted
                    IBAN = "",  // TODO: Decrypt from IbanEncrypted

                    // Statutory - ACTUAL properties from HrEmployeeStatutory
                    GosiNumber = statutory.GosiNumber ?? "",
                    IsSaudi = statutory.IsSaudi,
                    MedicalInsuranceNumber = statutory.MedicalInsuranceNumber ?? ""
                })
                .FirstAsync();

            return result;
        }

        /// <summary>
        /// Get multiple employees efficiently
        /// </summary>
        public async Task<List<EmployeeFullDto>> GetEmployeesFullDataAsync(List<long> employeeIds)
        {
            if (employeeIds == null || !employeeIds.Any())
                return new List<EmployeeFullDto>();

            var now = DateTime.Now;

            var results = await Context.Queryable<HrEmployeeCore>()
                .LeftJoin<HrPersonName>((core, name) =>
                    core.PersonId == name.PersonId &&
                    name.EffectiveFrom <= now &&
                    (name.EffectiveTo == null || name.EffectiveTo > now) &&
                    name.IsPrimary)
                .LeftJoin<HrPersonDemographic>((core, name, demo) =>
                    core.PersonId == demo.PersonId)
                .LeftJoin<HrEmployeeAssignment>((core, name, demo, assign) =>
                    core.EmployeeId == assign.EmployeeId &&
                    assign.EffectiveFrom <= now &&
                    (assign.EffectiveTo == null || assign.EffectiveTo > now) &&
                    assign.IsPrimary)
                .LeftJoin<HrEmployeeStatus>((core, name, demo, assign, status) =>
                    core.EmployeeId == status.EmployeeId &&
                    status.EffectiveFrom <= now &&
                    (status.EffectiveTo == null || status.EffectiveTo > now))
                .LeftJoin<HrEmployeeContract>((core, name, demo, assign, status, contract) =>
                    core.EmployeeId == contract.EmployeeId &&
                    contract.EffectiveFrom <= now &&
                    (contract.EffectiveTo == null || contract.EffectiveTo > now))
                .LeftJoin<HrEmployeeStatutory>((core, name, demo, assign, status, contract, statutory) =>
                    core.EmployeeId == statutory.EmployeeId)
                .LeftJoin<HrEmployeeBankAccount>((core, name, demo, assign, status, contract, statutory, bank) =>
                    core.EmployeeId == bank.EmployeeId && bank.IsPrimary)
                .Where((core) => employeeIds.Contains(core.EmployeeId))
                .Select((core, name, demo, assign, status, contract, statutory, bank) => new EmployeeFullDto
                {
                    EmployeeId = core.EmployeeId,
                    PersonId = core.PersonId,
                    UserId = core.UserId,
                    EmployeeCode = core.EmployeeCode,
                    HireDate = core.HireDate,
                    IsActive = core.IsActive,
                    GivenName = name.GivenName ?? "",
                    FamilyName = name.FamilyName ?? "",
                    MiddleName = name.MiddleName ?? "",
                    FullNameAr = name.FullNameAr ?? "",
                    Email = "",
                    Phone = "",
                    MobileNumber = "",
                    GenderDictCode = demo.GenderDictCode,
                    MaritalStatusDictCode = demo.MaritalStatusDictCode,
                    NationalityDictCode = demo.NationalityDictCode,
                    DateOfBirth = demo.DateOfBirth,
                    Gender = "",
                    MaritalStatus = "",
                    Nationality = "",
                    DepartmentId = assign.DepartmentId,
                    PositionId = assign.PositionId,
                    ManagerEmployeeId = assign.ManagerEmployeeId,
                    WorkLocationId = assign.WorkLocationId,
                    EmploymentStatusDictCode = status.EmploymentStatusDictCode,
                    Status = "",
                    EmployeeContractId = contract.EmployeeContractId,
                    ContractTypeDictCode = contract.ContractTypeDictCode,
                    ContractStartDate = contract.ContractStartDate,
                    ContractEndDate = contract.ContractEndDate,
                    ContractType = "",
                    BankName = bank.BankName ?? "",
                    BankAccountEncrypted = bank.BankAccountEncrypted ?? "",
                    IbanEncrypted = bank.IbanEncrypted ?? "",
                    AccountNumberEncrypted = bank.AccountNumberEncrypted ?? "",
                    BankAccountNumber = "",
                    IBAN = "",
                    GosiNumber = statutory.GosiNumber ?? "",
                    IsSaudi = statutory.IsSaudi,
                    MedicalInsuranceNumber = statutory.MedicalInsuranceNumber ?? ""
                })
                .ToListAsync();

            return results;
        }

        /// <summary>
        /// Get all active employees
        /// </summary>
        public async Task<List<EmployeeFullDto>> GetAllActiveEmployeesAsync()
        {
            var now = DateTime.Now;

            var results = await Context.Queryable<HrEmployeeCore>()
                .LeftJoin<HrPersonName>((core, name) =>
                    core.PersonId == name.PersonId &&
                    name.EffectiveFrom <= now &&
                    (name.EffectiveTo == null || name.EffectiveTo > now) &&
                    name.IsPrimary)
                .LeftJoin<HrPersonDemographic>((core, name, demo) =>
                    core.PersonId == demo.PersonId)
                .LeftJoin<HrEmployeeAssignment>((core, name, demo, assign) =>
                    core.EmployeeId == assign.EmployeeId &&
                    assign.EffectiveFrom <= now &&
                    (assign.EffectiveTo == null || assign.EffectiveTo > now) &&
                    assign.IsPrimary)
                .LeftJoin<HrEmployeeStatus>((core, name, demo, assign, status) =>
                    core.EmployeeId == status.EmployeeId &&
                    status.EffectiveFrom <= now &&
                    (status.EffectiveTo == null || status.EffectiveTo > now))
                .Where((core) => core.IsActive)
                .Select((core, name, demo, assign, status) => new EmployeeFullDto
                {
                    EmployeeId = core.EmployeeId,
                    PersonId = core.PersonId,
                    UserId = core.UserId,
                    EmployeeCode = core.EmployeeCode,
                    HireDate = core.HireDate,
                    IsActive = core.IsActive,
                    GivenName = name.GivenName ?? "",
                    FamilyName = name.FamilyName ?? "",
                    MiddleName = name.MiddleName ?? "",
                    FullNameAr = name.FullNameAr ?? "",
                    Email = "",
                    Phone = "",
                    MobileNumber = "",
                    GenderDictCode = demo.GenderDictCode,
                    MaritalStatusDictCode = demo.MaritalStatusDictCode,
                    NationalityDictCode = demo.NationalityDictCode,
                    DateOfBirth = demo.DateOfBirth,
                    Gender = "",
                    MaritalStatus = "",
                    Nationality = "",
                    DepartmentId = assign.DepartmentId,
                    PositionId = assign.PositionId,
                    ManagerEmployeeId = assign.ManagerEmployeeId,
                    WorkLocationId = assign.WorkLocationId,
                    EmploymentStatusDictCode = status.EmploymentStatusDictCode,
                    Status = ""
                })
                .ToListAsync();

            return results;
        }

        /// <summary>
        /// Get employees by department
        /// </summary>
        public async Task<List<EmployeeFullDto>> GetEmployeesByDepartmentAsync(long departmentId)
        {
            var now = DateTime.Now;

            var results = await Context.Queryable<HrEmployeeCore>()
                .LeftJoin<HrPersonName>((core, name) =>
                    core.PersonId == name.PersonId &&
                    name.EffectiveFrom <= now &&
                    (name.EffectiveTo == null || name.EffectiveTo > now) &&
                    name.IsPrimary)
                .LeftJoin<HrEmployeeAssignment>((core, name, assign) =>
                    core.EmployeeId == assign.EmployeeId &&
                    assign.EffectiveFrom <= now &&
                    (assign.EffectiveTo == null || assign.EffectiveTo > now) &&
                    assign.IsPrimary)
                .LeftJoin<HrEmployeeStatus>((core, name, assign, status) =>
                    core.EmployeeId == status.EmployeeId &&
                    status.EffectiveFrom <= now &&
                    (status.EffectiveTo == null || status.EffectiveTo > now))
                .Where((core, name, assign, status) =>
                    assign.DepartmentId == departmentId &&
                    core.IsActive)
                .Select((core, name, assign, status) => new EmployeeFullDto
                {
                    EmployeeId = core.EmployeeId,
                    PersonId = core.PersonId,
                    EmployeeCode = core.EmployeeCode,
                    HireDate = core.HireDate,
                    IsActive = core.IsActive,
                    GivenName = name.GivenName ?? "",
                    FamilyName = name.FamilyName ?? "",
                    FullNameAr = name.FullNameAr ?? "",
                    DepartmentId = assign.DepartmentId,
                    PositionId = assign.PositionId,
                    ManagerEmployeeId = assign.ManagerEmployeeId,
                    EmploymentStatusDictCode = status.EmploymentStatusDictCode,
                    Status = ""
                })
                .ToListAsync();

            return results;
        }

        /// <summary>
        /// Get team members for a manager
        /// </summary>
        public async Task<List<EmployeeFullDto>> GetTeamMembersAsync(long managerEmployeeId)
        {
            var now = DateTime.Now;

            var results = await Context.Queryable<HrEmployeeCore>()
                .LeftJoin<HrPersonName>((core, name) =>
                    core.PersonId == name.PersonId &&
                    name.EffectiveFrom <= now &&
                    (name.EffectiveTo == null || name.EffectiveTo > now) &&
                    name.IsPrimary)
                .LeftJoin<HrEmployeeAssignment>((core, name, assign) =>
                    core.EmployeeId == assign.EmployeeId &&
                    assign.EffectiveFrom <= now &&
                    (assign.EffectiveTo == null || assign.EffectiveTo > now) &&
                    assign.IsPrimary)
                .LeftJoin<HrEmployeeStatus>((core, name, assign, status) =>
                    core.EmployeeId == status.EmployeeId &&
                    status.EffectiveFrom <= now &&
                    (status.EffectiveTo == null || status.EffectiveTo > now))
                .Where((core, name, assign, status) =>
                    assign.ManagerEmployeeId == managerEmployeeId &&
                    core.IsActive)
                .Select((core, name, assign, status) => new EmployeeFullDto
                {
                    EmployeeId = core.EmployeeId,
                    PersonId = core.PersonId,
                    EmployeeCode = core.EmployeeCode,
                    HireDate = core.HireDate,
                    IsActive = core.IsActive,
                    GivenName = name.GivenName ?? "",
                    FamilyName = name.FamilyName ?? "",
                    FullNameAr = name.FullNameAr ?? "",
                    DepartmentId = assign.DepartmentId,
                    PositionId = assign.PositionId,
                    ManagerEmployeeId = assign.ManagerEmployeeId,
                    EmploymentStatusDictCode = status.EmploymentStatusDictCode,
                    Status = ""
                })
                .ToListAsync();

            return results;
        }

        /// <summary>
        /// Search employees by name or code
        /// </summary>
        public async Task<List<EmployeeFullDto>> SearchEmployeesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<EmployeeFullDto>();

            var now = DateTime.Now;
            searchTerm = searchTerm.ToLower();

            var results = await Context.Queryable<HrEmployeeCore>()
                .LeftJoin<HrPersonName>((core, name) =>
                    core.PersonId == name.PersonId &&
                    name.EffectiveFrom <= now &&
                    (name.EffectiveTo == null || name.EffectiveTo > now) &&
                    name.IsPrimary)
                .Where((core, name) =>
                    core.EmployeeCode.ToLower().Contains(searchTerm) ||
                    name.GivenName.ToLower().Contains(searchTerm) ||
                    name.FamilyName.ToLower().Contains(searchTerm) ||
                    name.FullNameAr.Contains(searchTerm))
                .Select((core, name) => new EmployeeFullDto
                {
                    EmployeeId = core.EmployeeId,
                    PersonId = core.PersonId,
                    EmployeeCode = core.EmployeeCode,
                    HireDate = core.HireDate,
                    IsActive = core.IsActive,
                    GivenName = name.GivenName ?? "",
                    FamilyName = name.FamilyName ?? "",
                    FullNameAr = name.FullNameAr ?? ""
                })
                .ToListAsync();

            return results;
        }
    }
}
