using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Model;
using Infrastructure.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Model.Marketing.Dto;
using ZR.Service.Marketing.IMarketingService;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.Marketing
{
    /// <summary>
    /// Marketing Employee Controller - Get marketing team employees
    /// </summary>
    [Verify]
    [Route("api/marketing/employees")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class MarketingEmployeeController : BaseController
    {
        private readonly IMarketingEmployeeService _marketingEmployeeService;
        private readonly IHrEmployeeCoreService _employeeCoreService;

        public MarketingEmployeeController(
            IMarketingEmployeeService marketingEmployeeService,
            IHrEmployeeCoreService employeeCoreService)
        {
            _marketingEmployeeService = marketingEmployeeService;
            _employeeCoreService = employeeCoreService;
        }

        /// <summary>
        /// Get employees by department (for marketing team)
        /// الحصول على الموظفين حسب القسم (لفريق التسويق)
        /// </summary>
        /// <param name="queryDto">Query parameters including department, search, status filters</param>
        [HttpGet]
        [ActionPermissionFilter("marketing:employees:list")]
        public IActionResult GetMarketingEmployees([FromQuery] MarketingEmployeeQueryDto queryDto)
        {
            try
            {
                var result = _marketingEmployeeService.GetMarketingEmployeesAsync(queryDto).Result;

                return SUCCESS(new
                {
                    result = result.Result,
                    totalNum = result.TotalNum,
                    pageIndex = result.PageIndex,
                    pageSize = result.PageSize
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error retrieving marketing employees: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current logged-in user's employee data (no special permission required)
        /// الحصول على بيانات الموظف للمستخدم الحالي
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMyEmployeeData()
        {
            try
            {
                // Get the current user ID from the token
                var currentUserId = HttpContext.GetUId();

                if (currentUserId == 0)
                {
                    return ToResponse(ResultCode.FAIL, "User not authenticated");
                }

                // Look up employee ID from user ID
                var employeeId = await _employeeCoreService.GetEmployeeIdByUserIdAsync(currentUserId);

                if (employeeId == null || employeeId == 0)
                {
                    return ToResponse(ResultCode.FAIL, "Your account is not linked to an employee profile");
                }

                var employee = await _marketingEmployeeService.GetEmployeeByIdAsync(employeeId.Value);

                if (employee == null)
                {
                    return ToResponse(ResultCode.FAIL, "Employee profile not found");
                }

                return SUCCESS(employee);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error retrieving your employee data: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current logged-in user's KPIs (no special permission required)
        /// الحصول على مؤشرات الأداء للمستخدم الحالي
        /// </summary>
        /// <param name="periodDays">Number of days to calculate KPIs for (default: 30)</param>
        [HttpGet("me/kpis")]
        public async Task<IActionResult> GetMyKpis([FromQuery] int periodDays = 30)
        {
            try
            {
                // Get the current user ID from the token
                var currentUserId = HttpContext.GetUId();

                if (currentUserId == 0)
                {
                    return ToResponse(ResultCode.FAIL, "User not authenticated");
                }

                // Look up employee ID from user ID
                var employeeId = await _employeeCoreService.GetEmployeeIdByUserIdAsync(currentUserId);

                if (employeeId == null || employeeId == 0)
                {
                    return ToResponse(ResultCode.FAIL, "Your account is not linked to an employee profile");
                }

                // Validate period days
                if (periodDays < 1 || periodDays > 365)
                {
                    return ToResponse(ResultCode.FAIL, "Period days must be between 1 and 365");
                }

                var kpis = await _marketingEmployeeService.GetEmployeeKpisAsync(employeeId.Value, periodDays);

                if (kpis == null)
                {
                    return ToResponse(ResultCode.FAIL, "Failed to retrieve your KPIs");
                }

                return SUCCESS(kpis);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error retrieving your KPIs: {ex.Message}");
            }
        }

        /// <summary>
        /// Get employee by ID with details
        /// الحصول على تفاصيل الموظف
        /// </summary>
        /// <param name="id">Employee ID (can be string to prevent JavaScript precision loss)</param>
        [HttpGet("{id}")]
        [ActionPermissionFilter("marketing:employees:query")]
        public IActionResult GetEmployeeById(string id)
        {
            try
            {
                // Parse string ID to long to handle JavaScript precision loss for large Snowflake IDs
                if (!long.TryParse(id, out long employeeId))
                {
                    return ToResponse(ResultCode.FAIL, "Invalid employee ID format");
                }

                var employee = _marketingEmployeeService.GetEmployeeByIdAsync(employeeId).Result;

                if (employee == null)
                {
                    return ToResponse(ResultCode.FAIL, "Employee not found");
                }

                return SUCCESS(employee);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error retrieving employee: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all departments (for filtering)
        /// الحصول على جميع الأقسام
        /// </summary>
        [HttpGet("departments")]
        [ActionPermissionFilter("marketing:employees:list")]
        public IActionResult GetDepartments()
        {
            try
            {
                var departments = _marketingEmployeeService.GetDepartments()
                    .Select(d => new
                    {
                        // Convert to string to prevent JavaScript precision loss with large Snowflake IDs
                        DeptId = d.DeptId.ToString(),
                        DeptName = d.DeptName,
                        // Convert to string to prevent JavaScript precision loss with large Snowflake IDs
                        ParentId = d.ParentId.ToString()
                    })
                    .ToList();

                return SUCCESS(departments);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error retrieving departments: {ex.Message}");
            }
        }

        /// <summary>
        /// Get employee KPIs (Key Performance Indicators)
        /// الحصول على مؤشرات الأداء الرئيسية للموظف
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <param name="periodDays">Number of days to calculate KPIs for (default: 30)</param>
        [HttpGet("{id}/kpis")]
        [ActionPermissionFilter("marketing:employees:query")]
        public IActionResult GetEmployeeKpis(string id, [FromQuery] int periodDays = 30)
        {
            try
            {
                // Parse string ID to long to handle JavaScript precision loss for large Snowflake IDs
                if (!long.TryParse(id, out long employeeId))
                {
                    return ToResponse(ResultCode.FAIL, "Invalid employee ID format");
                }

                // Validate period days
                if (periodDays < 1 || periodDays > 365)
                {
                    return ToResponse(ResultCode.FAIL, "Period days must be between 1 and 365");
                }

                var kpis = _marketingEmployeeService.GetEmployeeKpisAsync(employeeId, periodDays).Result;

                if (kpis == null)
                {
                    return ToResponse(ResultCode.FAIL, "Failed to retrieve employee KPIs");
                }

                return SUCCESS(kpis);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error retrieving employee KPIs: {ex.Message}");
            }
        }
    }
}
