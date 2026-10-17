using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Service.HR.IService;
using ZR.Admin.WebApi.Extensions;
using System;

namespace ZR.Admin.WebApi.Controllers.HR.EmployeeCore
{
    /// <summary>
    /// Employee Core - Assignment Controller
    /// Handles ONLY: EmployeeAssignment (department, position, manager, location)
    /// </summary>
    [Verify]
    [Route("api/hr/employeecore/assignment")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class HrEmployeeAssignmentController : BaseController
    {
        private readonly IHrEmployeeAssignmentService _assignmentService;

        public HrEmployeeAssignmentController(IHrEmployeeAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        /// <summary>
        /// Get current assignment for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Current assignment record with calculated fields</returns>
        [HttpGet("{employeeId:long}/current")]
        [ActionPermissionFilter("hr:employeecore:assignment:query")]
        public async Task<IActionResult> GetCurrentAssignment(long employeeId)
        {
            var assignments = await _assignmentService.GetListAsync(x => x.EmployeeId == employeeId);

            // Get current assignment (IsPrimary flag or effective dating)
            var currentAssignment = assignments
                .Where(a => a.IsPrimary)
                .OrderByDescending(a => a.EffectiveFrom)
                .FirstOrDefault();

            if (currentAssignment == null)
            {
                // Fallback to effective dating
                var now = DateTime.Now;
                currentAssignment = assignments
                    .Where(a => a.EffectiveFrom <= now && (a.EffectiveTo == null || a.EffectiveTo > now))
                    .OrderByDescending(a => a.EffectiveFrom)
                    .FirstOrDefault();
            }

            if (currentAssignment == null)
            {
                return SUCCESS(new
                {
                    EmployeeId = employeeId,
                    HasAssignment = false,
                    Assignment = (object)null
                });
            }

            return SUCCESS(new
            {
                EmployeeId = employeeId,
                HasAssignment = true,
                Assignment = currentAssignment
            });
        }

        /// <summary>
        /// Get assignment history for an employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>All assignment records ordered by effective date</returns>
        [HttpGet("{employeeId:long}/history")]
        [ActionPermissionFilter("hr:employeecore:assignment:query")]
        public async Task<IActionResult> GetAssignmentHistory(long employeeId)
        {
            var assignments = await _assignmentService.GetListAsync(x => x.EmployeeId == employeeId);
            var orderedAssignments = assignments.OrderByDescending(a => a.EffectiveFrom).ToList();

            return SUCCESS(orderedAssignments);
        }

        /// <summary>
        /// Create new assignment (marks previous as not current)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Assignment data</param>
        /// <returns>Created assignment ID</returns>
        [HttpPost("{employeeId:long}")]
        [ActionPermissionFilter("hr:employeecore:assignment:add")]
        [Log(Title = "Create Assignment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateAssignment(long employeeId, [FromBody] CreateAssignmentDto dto)
        {
            // Mark existing assignments as not current
            var existingAssignments = await _assignmentService.GetListAsync(a => a.EmployeeId == employeeId && a.IsPrimary);

            DateTime effectiveFrom = dto.EffectiveFrom ?? DateTime.Now;

            foreach (var existing in existingAssignments)
            {
                existing.IsPrimary = false;
                existing.EffectiveTo = effectiveFrom.AddDays(-1);
                existing.Update_by = HttpContext.GetName();
                existing.Update_time = DateTime.Now;
                await _assignmentService.UpdateAsync(existing);
            }

            // Create new assignment
            var newAssignment = new Model.HR.EmployeeCore.HrEmployeeAssignment
            {
                EmployeeId = employeeId,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                ManagerEmployeeId = dto.ManagerEmployeeId,
                WorkLocationId = dto.WorkLocationId,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = null,
                IsPrimary = true,
                Create_by = HttpContext.GetName(),
                Create_time = DateTime.Now
            };

            var assignmentId = await _assignmentService.AddAsync(newAssignment);

            return SUCCESS(new { EmployeeAssignmentId = assignmentId });
        }

        /// <summary>
        /// Update assignment
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="assignmentId">Assignment ID</param>
        /// <param name="dto">Updated data</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/{assignmentId:long}")]
        [ActionPermissionFilter("hr:employeecore:assignment:edit")]
        [Log(Title = "Update Assignment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateAssignment(long employeeId, long assignmentId, [FromBody] UpdateAssignmentDto dto)
        {
            var assignment = await _assignmentService.GetByIdAsync(assignmentId);
            if (assignment == null || assignment.EmployeeId != employeeId)
            {
                return ToResponse(ApiResult.Error("Assignment not found"));
            }

            if (dto.DepartmentId.HasValue)
                assignment.DepartmentId = dto.DepartmentId;

            if (dto.PositionId.HasValue)
                assignment.PositionId = dto.PositionId;

            if (dto.ManagerEmployeeId.HasValue)
                assignment.ManagerEmployeeId = dto.ManagerEmployeeId;

            if (dto.WorkLocationId.HasValue)
                assignment.WorkLocationId = dto.WorkLocationId;

            if (dto.EffectiveTo.HasValue)
                assignment.EffectiveTo = dto.EffectiveTo;

            assignment.Update_by = HttpContext.GetName();
            assignment.Update_time = DateTime.Now;

            await _assignmentService.UpdateAsync(assignment);

            return SUCCESS(assignment);
        }

        /// <summary>
        /// Update current assignment (simple update for current assignment fields)
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="dto">Simple assignment update</param>
        /// <returns>Success result</returns>
        [HttpPut("{employeeId:long}/current")]
        [ActionPermissionFilter("hr:employeecore:assignment:edit")]
        [Log(Title = "Update Current Assignment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateCurrentAssignment(long employeeId, [FromBody] UpdateCurrentAssignmentDto dto)
        {
            var assignments = await _assignmentService.GetListAsync(x => x.EmployeeId == employeeId);
            var currentAssignment = assignments.FirstOrDefault(a => a.IsPrimary);

            var now = DateTime.Now;
            var userName = HttpContext.GetName();

            if (currentAssignment != null)
            {
                // Update existing current assignment
                if (dto.DepartmentId.HasValue)
                    currentAssignment.DepartmentId = dto.DepartmentId;

                if (dto.PositionId.HasValue)
                    currentAssignment.PositionId = dto.PositionId;

                if (dto.ManagerId.HasValue)
                    currentAssignment.ManagerEmployeeId = dto.ManagerId;

                currentAssignment.Update_by = userName;
                currentAssignment.Update_time = now;
                await _assignmentService.UpdateAsync(currentAssignment);
            }
            else
            {
                // Create new current assignment
                var newAssignment = new Model.HR.EmployeeCore.HrEmployeeAssignment
                {
                    EmployeeId = employeeId,
                    DepartmentId = dto.DepartmentId,
                    PositionId = dto.PositionId,
                    ManagerEmployeeId = dto.ManagerId,
                    IsPrimary = true,
                    EffectiveFrom = now,
                    Create_by = userName,
                    Create_time = now
                };
                await _assignmentService.AddAsync(newAssignment);
            }

            return SUCCESS(new { message = "Assignment updated successfully" });
        }

        /// <summary>
        /// Close/deactivate current assignment
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="assignmentId">Assignment ID</param>
        /// <returns>Success result</returns>
        [HttpDelete("{employeeId:long}/{assignmentId:long}")]
        [ActionPermissionFilter("hr:employeecore:assignment:delete")]
        [Log(Title = "Close Assignment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CloseAssignment(long employeeId, long assignmentId)
        {
            var assignment = await _assignmentService.GetByIdAsync(assignmentId);
            if (assignment == null || assignment.EmployeeId != employeeId)
            {
                return ToResponse(ApiResult.Error("Assignment not found"));
            }

            // Close the assignment
            assignment.IsPrimary = false;
            assignment.EffectiveTo = DateTime.Now;
            assignment.Update_by = HttpContext.GetName();
            assignment.Update_time = DateTime.Now;

            await _assignmentService.UpdateAsync(assignment);

            return SUCCESS(assignment);
        }
    }
}
