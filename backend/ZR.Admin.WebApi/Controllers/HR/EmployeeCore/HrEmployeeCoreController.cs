using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using SqlSugar;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;
using ZR.Service.HR.EmployeeCore;

namespace ZR.Admin.WebApi.Controllers.HR
{
    /// <summary>
    /// Employee Core - Employee Controller
    /// Handles: EmployeeCore + Assignment + Status + Contract + Statutory
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/employee")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrEmployeeCoreController : BaseController
    {
        private readonly IHrEmployeeCoreService _employeeCoreService;
        private readonly IHrEmployeeAssignmentService _assignmentService;
        private readonly IHrEmployeeStatusService _statusService;
        private readonly IHrEmployeeContractService _contractService;
        private readonly IHrEmployeeStatutoryService _statutoryService;
        private readonly IEmployeeApiMappingService _mappingService;

        public HrEmployeeCoreController(
            IHrEmployeeCoreService employeeCoreService,
            IHrEmployeeAssignmentService assignmentService,
            IHrEmployeeStatusService statusService,
            IHrEmployeeContractService contractService,
            IHrEmployeeStatutoryService statutoryService,
            IEmployeeApiMappingService mappingService)
        {
            _employeeCoreService = employeeCoreService;
            _assignmentService = assignmentService;
            _statusService = statusService;
            _contractService = contractService;
            _statutoryService = statutoryService;
            _mappingService = mappingService;
        }

        /// <summary>
        /// List employees (paged)
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:employeecore:employee:list")]
        public async Task<IActionResult> List([FromQuery] HrEmployeeCoreQueryDto dto)
        {
            var predicate = Expressionable.Create<ZR.Model.HR.EmployeeCore.HrEmployeeCore>();
            predicate = predicate.AndIF(dto.EmployeeId.HasValue && dto.EmployeeId > 0, it => it.EmployeeId == dto.EmployeeId);
            predicate = predicate.AndIF(dto.PersonId.HasValue && dto.PersonId > 0, it => it.PersonId == dto.PersonId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.EmployeeCode), it => it.EmployeeCode.Contains(dto.EmployeeCode));
            predicate = predicate.AndIF(dto.IsActive.HasValue, it => it.IsActive == dto.IsActive.Value);

            var response = await _employeeCoreService.GetPageListAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get employee lookup list (for dropdowns) - PUBLIC endpoint for all authenticated users
        /// Returns minimal data: EmployeeId, EmployeeCode, FullName
        /// </summary>
        [HttpGet("lookup")]
        [LookupPermission("hr:employeecore:employee:lookup", isPublic: true)]
        public async Task<IActionResult> GetLookup([FromQuery] string search = null)
        {
            try
            {
                // Get active employees only, with minimal data
                var employees = await _employeeCoreService.GetLookupList(search);

                // Format for dropdown: value + label
                var result = employees.Select(e => new
                {
                    value = e.EmployeeId,
                    label = $"{e.EmployeeCode} - {e.FirstName} {e.LastName}",
                    employeeCode = e.EmployeeCode,
                    fullName = $"{e.FirstName} {e.LastName}"
                }).ToList();

                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error retrieving employee lookup: {ex.Message}");
            }
        }

        /// <summary>
        /// Get employee full details (simple composition, no complex joins)
        /// </summary>
        [HttpGet("{id:long}")]
        [ActionPermissionFilter("hr:employeecore:employee:query")]
        public async Task<IActionResult> GetInfo(long id)
        {
            var employee = await _employeeCoreService.GetByIdAsync(id);
            if (employee == null)
                return ToResponse(ResultCode.FAIL, "Employee not found");

            var assignments = await _assignmentService.GetListAsync(x => x.EmployeeId == id);
            var statuses = await _statusService.GetListAsync(x => x.EmployeeId == id);
            var contracts = await _contractService.GetListAsync(x => x.EmployeeId == id);
            var statutory = await _statutoryService.GetListAsync(x => x.EmployeeId == id);

            return SUCCESS(new
            {
                Employee = employee,
                Assignments = assignments,
                Statuses = statuses,
                Contracts = contracts,
                Statutory = statutory
            });
        }

        /// <summary>
        /// Export employees to Excel with all resolved labels
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter("hr:employeecore:employee:export")]
        [Log(Title = "Employee Export", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> Export()
        {
            try
            {
                // Get all employees
                var employees = await _employeeCoreService.GetListAsync(x => true);
                if (employees == null || !employees.Any())
                {
                    return ToResponse(ResultCode.FAIL, "No employees found to export");
                }

                // Map each employee through the API mapping service (resolves all dict codes, decrypts bank data, etc.)
                var exportList = new List<EmployeeExportDto>();
                foreach (var emp in employees)
                {
                    var apiDto = await _mappingService.ToApiDtoAsync(emp.EmployeeId);
                    if (apiDto == null) continue;

                    exportList.Add(new EmployeeExportDto
                    {
                        EmployeeCode = apiDto.EmployeeCode,
                        FullName = apiDto.FullName,
                        FullNameAr = apiDto.FullNameAr ?? apiDto.NameAr,
                        Gender = apiDto.GenderLabel,
                        DateOfBirth = apiDto.DateOfBirth,
                        Nationality = apiDto.NationalityLabel,
                        MaritalStatus = apiDto.MaritalStatusLabel,
                        Email = apiDto.Email,
                        Phone = apiDto.Phone,
                        MobileNumber = apiDto.MobileNumber,
                        PersonalEmail = apiDto.PersonalEmail,
                        IdNumber = apiDto.IdNumber,
                        IdExpiryDate = apiDto.IdExpiryDate,
                        IqamaNumber = apiDto.IqamaNumber,
                        IqamaExpiryDate = apiDto.IqamaExpiryDate,
                        PassportNumber = apiDto.PassportNumber,
                        PassportExpiryDate = apiDto.PassportExpiryDate,
                        Address = apiDto.Address,
                        DepartmentName = apiDto.DepartmentName,
                        PositionName = apiDto.PositionName,
                        ManagerName = apiDto.ManagerName,
                        HireDate = apiDto.HireDate,
                        EmploymentType = apiDto.EmploymentTypeLabel,
                        Status = apiDto.Status,
                        IsActive = apiDto.IsActive ? "Yes" : "No",
                        WorkLocation = apiDto.WorkLocation,
                        ContractStartDate = apiDto.ContractStartDate,
                        ContractEndDate = apiDto.ContractEndDate,
                        GosiNumber = apiDto.GosiNumber,
                        MedicalInsuranceNumber = apiDto.MedicalInsuranceNumber,
                        IsSaudi = apiDto.IsSaudi ? "Yes" : "No",
                        BankName = apiDto.BankName,
                        Iban = apiDto.Iban,
                        AccountNumber = apiDto.AccountNumber,
                        EmergencyContactName = apiDto.EmergencyContactName,
                        EmergencyContactPhone = apiDto.EmergencyContactPhone
                    });
                }

                var result = await ExportExcelMiniAsync(exportList, "Employees", "Employee_Export");
                return ExportExcel(result.Item2, result.Item1);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Export failed: {ex.Message}");
            }
        }
    }
}














