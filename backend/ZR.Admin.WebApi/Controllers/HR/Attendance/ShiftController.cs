using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR.Dto.Attendance;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Attendance
{
    /// <summary>
    /// Shift Management Controller - Shift definitions, assignments, schedules
    /// </summary>
    [Verify]
    [Route("api/hr/attendance/shifts")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class ShiftController : BaseController
    {
        private readonly IShiftManagementService _shiftService;
        private readonly ICalendarAssignmentService _calendarService;

        public ShiftController(
            IShiftManagementService shiftService,
            ICalendarAssignmentService calendarService)
        {
            _shiftService = shiftService;
            _calendarService = calendarService;
        }

        /// <summary>
        /// Get all shifts
        /// </summary>
        [HttpGet]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetShifts()
        {
            var shifts = await _shiftService.GetAllShiftsAsync();
            return SUCCESS(shifts);
        }

        /// <summary>
        /// Get shift by ID
        /// </summary>
        [HttpGet("{shiftId}")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetShift(long shiftId)
        {
            var shift = await _shiftService.GetShiftByIdAsync(shiftId);
            if (shift == null)
                return ToResponse(ResultCode.FAIL, "Shift not found");

            return SUCCESS(shift);
        }

        /// <summary>
        /// Create shift
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("hr:attendance:add")]
        [Log(Title = "Shift Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateShift([FromBody] CreateHrShiftDto dto)
        {
            var result = await _shiftService.CreateShiftAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update shift
        /// </summary>
        [HttpPut("{shiftId}")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Shift Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateShift(long shiftId, [FromBody] UpdateHrShiftDto dto)
        {
            var result = await _shiftService.UpdateShiftAsync(shiftId, dto);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Delete shift
        /// </summary>
        [HttpDelete("{shiftId}")]
        [ActionPermissionFilter("hr:attendance:delete")]
        [Log(Title = "Shift Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteShift(long shiftId)
        {
            var result = await _shiftService.DeleteShiftAsync(shiftId);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get employee current shift
        /// </summary>
        [HttpGet("employee/{employeeId}/current")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetEmployeeCurrentShift(long employeeId, [FromQuery] DateTime? date)
        {
            var shift = await _shiftService.GetEmployeeCurrentShiftAsync(employeeId, date ?? DateTime.Today);
            return SUCCESS(shift);
        }

        /// <summary>
        /// Get employee shift assignments
        /// </summary>
        [HttpGet("employee/{employeeId}/assignments")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetEmployeeShiftAssignments(long employeeId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var assignments = await _shiftService.GetEmployeeShiftAssignmentsAsync(
                employeeId,
                startDate ?? DateTime.Today.AddMonths(-1),
                endDate ?? DateTime.Today.AddMonths(1));
            return SUCCESS(assignments);
        }

        /// <summary>
        /// Assign shift to employee
        /// </summary>
        [HttpPost("assign")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Shift Assignment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> AssignShift([FromBody] CreateHrEmployeeShiftAssignmentDto dto)
        {
            var result = await _shiftService.AssignShiftToEmployeeAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Bulk assign shifts
        /// </summary>
        [HttpPost("bulk-assign")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Bulk Shift Assignment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> BulkAssignShifts([FromBody] BulkShiftAssignmentDto dto)
        {
            var result = await _shiftService.BulkAssignShiftsAsync(dto);
            return SUCCESS(new { AssignedCount = result });
        }

        /// <summary>
        /// Get shift schedules
        /// </summary>
        [HttpGet("schedules")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetShiftSchedules([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var schedules = await _shiftService.GetShiftSchedulesAsync(startDate, endDate);
            return SUCCESS(schedules);
        }

        /// <summary>
        /// Create shift calendar override
        /// </summary>
        [HttpPost("overrides")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Shift Override", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateShiftOverride([FromBody] CreateHrShiftCalendarOverrideDto dto)
        {
            var result = await _shiftService.CreateShiftOverrideAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get shift overrides for date range
        /// </summary>
        [HttpGet("overrides")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetShiftOverrides([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var overrides = await _shiftService.GetShiftOverridesAsync(startDate, endDate);
            return SUCCESS(overrides);
        }

        /// <summary>
        /// Assign calendar to employee
        /// </summary>
        [HttpPost("calendar/assign")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Calendar Assignment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> AssignCalendar([FromBody] AssignCalendarDto dto)
        {
            var result = await _calendarService.AssignCalendarToEmployeeAsync(dto.EmployeeId, dto.CalendarId, dto.EffectiveDate);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get employee calendar assignment
        /// </summary>
        [HttpGet("employee/{employeeId}/calendar")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetEmployeeCalendar(long employeeId)
        {
            var calendar = await _calendarService.GetEmployeeCalendarAsync(employeeId);
            return SUCCESS(calendar);
        }
    }

    public class BulkShiftAssignmentDto
    {
        public List<long> EmployeeIds { get; set; }
        public long ShiftId { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string Reason { get; set; }
    }

    public class AssignCalendarDto
    {
        public long EmployeeId { get; set; }
        public long CalendarId { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
