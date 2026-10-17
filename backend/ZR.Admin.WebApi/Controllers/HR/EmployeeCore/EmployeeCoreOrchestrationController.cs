using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.EmployeeCore
{
    /// <summary>
    /// Employee Core Orchestration Controller
    /// Handles composite employee operations (create/update complete employee, lifecycle events)
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/orchestration")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class EmployeeCoreOrchestrationController : BaseController
    {
        private readonly IEmployeeCoreOrchestrationService _orchestrationService;

        public EmployeeCoreOrchestrationController(IEmployeeCoreOrchestrationService orchestrationService)
        {
            _orchestrationService = orchestrationService;
        }

        #region Complete Employee Operations

        /// <summary>
        /// Create complete employee (Person + Employee + all related entities) in one transaction
        /// </summary>
        /// <remarks>
        /// This endpoint creates a complete employee record including:
        /// - Person (identity)
        /// - Person names, demographic, identifiers, contacts, addresses
        /// - Employee core
        /// - Employee assignments, statuses, contracts, statutory
        /// - Compensation packages and bank accounts
        /// - Emergency contacts and dependents
        ///
        /// All operations are performed in a single transaction - if any step fails, everything rolls back.
        /// </remarks>
        [HttpPost("complete")]
        [ActionPermissionFilter("hr:employeecore:create")]
        [Log(Title = "Create Complete Employee", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateCompleteEmployee([FromBody] CreateEmployeeCompleteDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Request body cannot be null");
            }

            var result = await _orchestrationService.CreateCompleteEmployeeAsync(dto);

            if (result.Success)
            {
                return SUCCESS(new
                {
                    result.PersonId,
                    result.EmployeeId,
                    result.Message
                });
            }
            else
            {
                return ToResponse(ResultCode.FAIL, result.Message);
            }
        }

        /// <summary>
        /// Update complete employee (all provided fields)
        /// </summary>
        /// <remarks>
        /// This endpoint updates an employee and all related entities provided in the request.
        /// Only the fields/entities included in the request will be updated.
        ///
        /// All operations are performed in a single transaction.
        /// </remarks>
        [HttpPut("complete/{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:update")]
        [Log(Title = "Update Complete Employee", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateCompleteEmployee(long employeeId, [FromBody] UpdateEmployeeCompleteDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Request body cannot be null");
            }

            if (dto.EmployeeId != employeeId)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Employee ID mismatch");
            }

            var success = await _orchestrationService.UpdateCompleteEmployeeAsync(dto);

            if (success)
            {
                return SUCCESS("Employee updated successfully");
            }
            else
            {
                return ToResponse(ResultCode.FAIL, "Failed to update employee");
            }
        }

        #endregion

        #region Employee Lifecycle Operations

        /// <summary>
        /// Terminate employee
        /// </summary>
        /// <remarks>
        /// This operation:
        /// - Sets the termination date on the employee record
        /// - Marks the employee as inactive
        /// - Creates a termination status entry
        /// - Ends all current assignments
        /// - Optionally deactivates the linked user account
        ///
        /// All operations are performed in a single transaction.
        /// </remarks>
        [HttpPost("terminate/{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:terminate")]
        [Log(Title = "Terminate Employee", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> TerminateEmployee(long employeeId, [FromBody] TerminateEmployeeDto dto)
        {
            if (dto == null || dto.EmployeeId != employeeId)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Invalid request");
            }

            var success = await _orchestrationService.TerminateEmployeeAsync(dto);

            if (success)
            {
                return SUCCESS("Employee terminated successfully");
            }
            else
            {
                return ToResponse(ResultCode.FAIL, "Failed to terminate employee");
            }
        }

        /// <summary>
        /// Activate (or reactivate) employee
        /// </summary>
        /// <remarks>
        /// This operation:
        /// - Clears the termination date
        /// - Marks the employee as active
        /// - Creates an active status entry
        /// - Reactivates the linked user account
        ///
        /// All operations are performed in a single transaction.
        /// </remarks>
        [HttpPost("activate/{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:activate")]
        [Log(Title = "Activate Employee", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ActivateEmployee(long employeeId)
        {
            var success = await _orchestrationService.ActivateEmployeeAsync(employeeId);

            if (success)
            {
                return SUCCESS("Employee activated successfully");
            }
            else
            {
                return ToResponse(ResultCode.FAIL, "Failed to activate employee");
            }
        }

        /// <summary>
        /// Transfer employee to new department/position/location
        /// </summary>
        /// <remarks>
        /// This operation:
        /// - Ends the current primary assignment
        /// - Creates a new assignment with the new department/position/location
        /// - Maintains complete assignment history
        ///
        /// All operations are performed in a single transaction.
        /// </remarks>
        [HttpPost("transfer/{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:transfer")]
        [Log(Title = "Transfer Employee", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> TransferEmployee(long employeeId, [FromBody] TransferEmployeeDto dto)
        {
            if (dto == null || dto.EmployeeId != employeeId)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Invalid request");
            }

            var success = await _orchestrationService.TransferEmployeeAsync(dto);

            if (success)
            {
                return SUCCESS("Employee transferred successfully");
            }
            else
            {
                return ToResponse(ResultCode.FAIL, "Failed to transfer employee");
            }
        }

        /// <summary>
        /// Promote employee (new position and optionally new salary)
        /// </summary>
        /// <remarks>
        /// This operation:
        /// - Transfers the employee to the new position
        /// - Optionally creates a new compensation package with the new salary
        /// - Maintains complete assignment and compensation history
        ///
        /// All operations are performed in a single transaction.
        /// </remarks>
        [HttpPost("promote/{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:promote")]
        [Log(Title = "Promote Employee", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> PromoteEmployee(long employeeId, [FromBody] PromoteEmployeeDto dto)
        {
            if (dto == null || dto.EmployeeId != employeeId)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Invalid request");
            }

            var success = await _orchestrationService.PromoteEmployeeAsync(dto);

            if (success)
            {
                return SUCCESS("Employee promoted successfully");
            }
            else
            {
                return ToResponse(ResultCode.FAIL, "Failed to promote employee");
            }
        }

        #endregion

        #region Helper Endpoints

        /// <summary>
        /// Get complete employee data (all related entities)
        /// </summary>
        [HttpGet("complete/{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:query")]
        public async Task<IActionResult> GetCompleteEmployee(long employeeId)
        {
            // This would combine data from Person, Employee, and all related services
            // For now, return a placeholder
            return SUCCESS(new { Message = "Complete employee data endpoint - to be implemented" });
        }

        #endregion
    }
}
