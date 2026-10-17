using Microsoft.AspNetCore.Mvc;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.System
{
    /// <summary>
    /// Employee System Access Management
    /// Handles activation of system access for employees with position-based roles
    /// </summary>
    [Route("system/employeeAccess")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class EmployeeSystemAccessController : BaseController
    {
        private readonly IEmployeeUserAccountService _employeeUserAccountService;

        public EmployeeSystemAccessController(IEmployeeUserAccountService employeeUserAccountService)
        {
            _employeeUserAccountService = employeeUserAccountService;
        }

        /// <summary>
        /// Preview system access for an employee
        /// Shows what roles would be assigned based on their position
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        [HttpGet("preview/{employeeId}")]
        [ActionPermissionFilter(Permission = "system:employeeAccess:query")]
        public async Task<IActionResult> Preview(long employeeId)
        {
            var preview = await _employeeUserAccountService.PreviewSystemAccessAsync(employeeId);

            if (preview == null)
            {
                return ToResponse(ResultCode.FAIL, "Employee not found");
            }

            return SUCCESS(preview);
        }

        /// <summary>
        /// Activate system access for an employee
        /// Creates a user account with roles derived from their position
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        [HttpPost("activate/{employeeId}")]
        [ActionPermissionFilter(Permission = "system:employeeAccess:activate")]
        [Log(Title = "Employee System Access Activation", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> Activate(long employeeId)
        {
            var result = await _employeeUserAccountService.ActivateSystemAccessAsync(employeeId);

            if (!result.Success)
            {
                return ToResponse(ResultCode.FAIL, result.Message);
            }

            return SUCCESS(result, result.Message);
        }

        /// <summary>
        /// Batch preview system access for multiple employees
        /// </summary>
        /// <param name="employeeIds">Array of Employee IDs</param>
        [HttpPost("preview/batch")]
        [ActionPermissionFilter(Permission = "system:employeeAccess:query")]
        public async Task<IActionResult> BatchPreview([FromBody] long[] employeeIds)
        {
            if (employeeIds == null || employeeIds.Length == 0)
            {
                return ToResponse(ResultCode.FAIL, "No employees specified");
            }

            var previews = new List<EmployeeAccessPreviewDto>();

            foreach (var employeeId in employeeIds)
            {
                var preview = await _employeeUserAccountService.PreviewSystemAccessAsync(employeeId);
                if (preview != null)
                {
                    previews.Add(preview);
                }
            }

            return SUCCESS(previews);
        }

        /// <summary>
        /// Batch activate system access for multiple employees
        /// </summary>
        /// <param name="employeeIds">Array of Employee IDs</param>
        [HttpPost("activate/batch")]
        [ActionPermissionFilter(Permission = "system:employeeAccess:activate")]
        [Log(Title = "Batch Employee System Access Activation", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> BatchActivate([FromBody] long[] employeeIds)
        {
            if (employeeIds == null || employeeIds.Length == 0)
            {
                return ToResponse(ResultCode.FAIL, "No employees specified");
            }

            var results = new List<EmployeeSystemAccessResult>();
            var successCount = 0;
            var failCount = 0;

            foreach (var employeeId in employeeIds)
            {
                var result = await _employeeUserAccountService.ActivateSystemAccessAsync(employeeId);
                results.Add(result);

                if (result.Success)
                    successCount++;
                else
                    failCount++;
            }

            var message = $"Activated {successCount} employee(s), {failCount} failed";

            return SUCCESS(new
            {
                successCount,
                failCount,
                results
            }, message);
        }
    }
}
