using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.HR.Dto.Attendance;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Attendance
{
    /// <summary>
    /// Attendance Management Controller - Daily attendance, check-in/out, processing
    /// </summary>
    [Verify]
    [Route("api/hr/attendance")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class AttendanceController : BaseController
    {
        private readonly IAttendanceProcessingService _attendanceService;
        private readonly IAttendanceLogService _logService;

        public AttendanceController(
            IAttendanceProcessingService attendanceService,
            IAttendanceLogService logService)
        {
            _attendanceService = attendanceService;
            _logService = logService;
        }

        /// <summary>
        /// Get today's attendance
        /// </summary>
        [HttpGet("today")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetTodayAttendance()
        {
            var attendance = await _attendanceService.GetDailyAttendanceAsync(DateTime.Today);
            return SUCCESS(attendance);
        }

        /// <summary>
        /// Get employee attendance history
        /// </summary>
        [HttpGet("employee/{employeeId}/history")]
        [ActionPermissionFilter("hr:attendance:view")]
        public async Task<IActionResult> GetEmployeeAttendance(long employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var attendance = await _attendanceService.GetEmployeeAttendanceAsync(employeeId, startDate, endDate);
            return SUCCESS(attendance);
        }

        /// <summary>
        /// Check in
        /// </summary>
        [HttpPost("check-in")]
        [ActionPermissionFilter("hr:attendance:checkin")]
        [Log(Title = "Employee Check In", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CheckIn([FromBody] CreateHrAttendanceDayDto dto)
        {
            dto.CheckInTime = DateTime.Now;
            var result = await _attendanceService.CheckInAsync(dto);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Check out
        /// </summary>
        [HttpPost("check-out")]
        [ActionPermissionFilter("hr:attendance:checkout")]
        [Log(Title = "Employee Check Out", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CheckOut([FromBody] UpdateHrAttendanceDayDto dto)
        {
            dto.CheckOutTime = DateTime.Now;
            var result = await _attendanceService.CheckOutAsync(dto);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get attendance summary statistics
        /// </summary>
        [HttpGet("summary")]
        [ActionPermissionFilter("hr:attendance:statistics")]
        public async Task<IActionResult> GetAttendanceSummary([FromQuery] DateTime? date)
        {
            var today = date ?? DateTime.Today;
            var dailyAttendance = await _attendanceService.GetDailyAttendanceAsync(today);

            var statistics = new
            {
                Date = today,
                TotalPresent = dailyAttendance.Count,
                AttendanceDetails = dailyAttendance
            };

            return SUCCESS(statistics);
        }

        /// <summary>
        /// Get attendance records with pagination
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetAttendanceList([FromQuery] HrAttendanceDayQueryDto dto)
        {
            var response = await _attendanceService.GetPagedAttendanceAsync(dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get daily summary
        /// </summary>
        [HttpGet("daily-summary")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetDailySummary([FromQuery] DateTime date)
        {
            var summary = await _attendanceService.GetDailySummaryAsync(date);
            return SUCCESS(summary);
        }

        /// <summary>
        /// Get monthly report
        /// </summary>
        [HttpGet("monthly-report")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] long employeeId, [FromQuery] int year, [FromQuery] int month)
        {
            var report = await _attendanceService.GetMonthlyReportAsync(employeeId, year, month);
            return SUCCESS(report);
        }

        /// <summary>
        /// Process attendance logs for a specific date
        /// </summary>
        [HttpPost("process-logs")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Process Attendance Logs", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ProcessAttendanceLogs([FromQuery] DateTime date)
        {
            var result = await _attendanceService.ProcessAttendanceForDateAsync(date);
            return SUCCESS(new { ProcessedCount = result });
        }

        /// <summary>
        /// Get attendance dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        [ActionPermissionFilter("hr:attendance:dashboard")]
        public async Task<IActionResult> GetAttendanceDashboard([FromQuery] DateTime? date)
        {
            var today = date ?? DateTime.Today;
            var dailyAttendance = await _attendanceService.GetDailyAttendanceAsync(today);

            var dashboard = new
            {
                Date = today,
                TotalPresent = dailyAttendance.Count,
                Attendance = dailyAttendance
            };

            return SUCCESS(dashboard);
        }

        /// <summary>
        /// Get attendance analytics
        /// </summary>
        [HttpGet("analytics")]
        [ActionPermissionFilter("hr:attendance:analytics")]
        public async Task<IActionResult> GetAttendanceAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var analytics = await _attendanceService.GetAttendanceAnalyticsAsync(startDate, endDate);
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Get department attendance summary
        /// </summary>
        [HttpGet("department/{departmentId}/summary")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetDepartmentAttendanceSummary(long departmentId, [FromQuery] DateTime? date)
        {
            var summary = await _attendanceService.GetDepartmentSummaryAsync(departmentId, date ?? DateTime.Today);
            return SUCCESS(summary);
        }

        /// <summary>
        /// Add attendance adjustment
        /// </summary>
        [HttpPost("adjustments")]
        [ActionPermissionFilter("hr:attendance:edit")]
        [Log(Title = "Attendance Adjustment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddAdjustment([FromBody] CreateHrAttendanceAdjustmentDto dto)
        {
            var result = await _attendanceService.CreateAdjustmentAsync(dto);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get pending adjustments
        /// </summary>
        [HttpGet("adjustments/pending")]
        [ActionPermissionFilter("hr:attendance:query")]
        public async Task<IActionResult> GetPendingAdjustments()
        {
            var adjustments = await _attendanceService.GetPendingAdjustmentsAsync();
            return SUCCESS(adjustments);
        }

        /// <summary>
        /// Approve adjustment
        /// </summary>
        [HttpPost("adjustments/{adjustmentId}/approve")]
        [ActionPermissionFilter("hr:attendance:approve")]
        [Log(Title = "Approve Adjustment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApproveAdjustment(long adjustmentId, [FromBody] ApproveAdjustmentDto dto)
        {
            var result = await _attendanceService.ApproveAdjustmentAsync(adjustmentId, dto);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Reject adjustment
        /// </summary>
        [HttpPost("adjustments/{adjustmentId}/reject")]
        [ActionPermissionFilter("hr:attendance:approve")]
        [Log(Title = "Reject Adjustment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RejectAdjustment(long adjustmentId, [FromBody] RejectAdjustmentDto dto)
        {
            var result = await _attendanceService.RejectAdjustmentAsync(adjustmentId, dto);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }
    }

    public class ApproveAdjustmentDto
    {
        public string ApprovalNotes { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedAt { get; set; }
    }

    public class RejectAdjustmentDto
    {
        public string RejectionReason { get; set; }
        public string RejectedBy { get; set; }
        public DateTime RejectedAt { get; set; }
    }
}
