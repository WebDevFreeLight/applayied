using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

using ZR.Admin.WebApi.Filters;
using ZR.Service.HR.IHrService;

using SqlSugar;

namespace ZR.Admin.WebApi.Controllers.HR.Dashboard
{
    /// <summary>
    /// Manager Dashboard Controller
    /// </summary>
    [Verify]
    [Route("api/hr/manager")]
    [ApiExplorerSettings(GroupName = "hr")]
    [ApiController]
    [Authorize]
    public class ManagerDashboardController : BaseController
    {
        private readonly IManagerDashboardService _managerDashboardService;

        public ManagerDashboardController(IManagerDashboardService managerDashboardService)
        {
            _managerDashboardService = managerDashboardService;
        }

        /// <summary>
        /// Get manager dashboard overview
        /// </summary>
        [HttpGet("dashboard")]
        [ActionPermissionFilter("hr:manager:dashboard")]
        public async Task<IActionResult> GetManagerDashboard()
        {
            var managerId = HttpContext.GetUId();
            var dashboard = await _managerDashboardService.GetDashboardOverviewAsync(managerId);
            return SUCCESS(dashboard);
        }

        /// <summary>
        /// Get team members
        /// </summary>
        [HttpGet("team")]
        [ActionPermissionFilter("hr:manager:team")]
        public async Task<IActionResult> GetTeamMembers()
        {
            var managerId = HttpContext.GetUId();
            var team = await _managerDashboardService.GetTeamMembersAsync(managerId);
            return SUCCESS(team);
        }

        /// <summary>
        /// Get team attendance
        /// </summary>
        [HttpGet("team/attendance")]
        [ActionPermissionFilter("hr:manager:attendance")]
        public async Task<IActionResult> GetTeamAttendance([FromQuery] DateTime? date = null)
        {
            var managerId = HttpContext.GetUId();
            var targetDate = date ?? DateTime.Today;
            var attendance = await _managerDashboardService.GetTeamAttendanceTodayAsync(managerId);
            return SUCCESS(attendance);
        }

        /// <summary>
        /// Get pending approvals
        /// </summary>
        [HttpGet("approvals")]
        [ActionPermissionFilter("hr:manager:approvals")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            var managerId = HttpContext.GetUId();
            
            var approvals = new
            {
                LeaveRequests = await _managerDashboardService.GetPendingLeaveRequestsAsync(managerId),
                PerformanceReviews = await _managerDashboardService.GetPendingPerformanceReviewsAsync(managerId),
                ExpenseRequests = await _managerDashboardService.GetPendingExpenseRequestsAsync(managerId)
            };
            
            return SUCCESS(approvals);
        }

        /// <summary>
        /// Get team performance metrics
        /// </summary>
        [HttpGet("team/performance")]
        [ActionPermissionFilter("hr:manager:performance")]
        public async Task<IActionResult> GetTeamPerformanceMetrics([FromQuery] int year)
        {
            var managerId = HttpContext.GetUId();
            var metrics = await _managerDashboardService.GetTeamPerformanceMetricsAsync(managerId, year);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Get team leave summary
        /// </summary>
        [HttpGet("team/leave-summary")]
        [ActionPermissionFilter("hr:manager:leave")]
        public async Task<IActionResult> GetTeamLeaveSummary([FromQuery] int year)
        {
            var managerId = HttpContext.GetUId();
            var summary = await _managerDashboardService.GetTeamLeaveSummaryAsync(managerId, year);
            return SUCCESS(summary);
        }

        /// <summary>
        /// Get team training status
        /// </summary>
        [HttpGet("team/training")]
        [ActionPermissionFilter("hr:manager:training")]
        public async Task<IActionResult> GetTeamTrainingStatus()
        {
            var managerId = HttpContext.GetUId();
            var training = await _managerDashboardService.GetTeamTrainingStatusAsync(managerId);
            return SUCCESS(training);
        }

        /// <summary>
        /// Approve team member request
        /// </summary>
        [HttpPost("approve/{requestType}/{requestId}")]
        [ActionPermissionFilter("hr:manager:approve")]
        public async Task<IActionResult> ApproveRequest(string requestType, long requestId, [FromBody] ManagerApprovalRequest request)
        {
            var managerId = HttpContext.GetUId();
            var result = await _managerDashboardService.ApproveRequestAsync(managerId, requestType, requestId, request.Comments);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Reject team member request
        /// </summary>
        [HttpPost("reject/{requestType}/{requestId}")]
        [ActionPermissionFilter("hr:manager:reject")]
        public async Task<IActionResult> RejectRequest(string requestType, long requestId, [FromBody] ManagerRejectionRequest request)
        {
            var managerId = HttpContext.GetUId();
            var result = await _managerDashboardService.RejectRequestAsync(managerId, requestType, requestId, request.Reason);
            return result ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get team statistics
        /// </summary>
        [HttpGet("team/statistics")]
        [ActionPermissionFilter("hr:manager:statistics")]
        public async Task<IActionResult> GetTeamStatistics()
        {
            var managerId = HttpContext.GetUId();
            var stats = await _managerDashboardService.GetTeamStatisticsAsync(managerId);
            return SUCCESS(stats);
        }

        /// <summary>
        /// Schedule team meeting
        /// </summary>
        [HttpPost("team/meeting")]
        [ActionPermissionFilter("hr:manager:meeting")]
        public async Task<IActionResult> ScheduleTeamMeeting([FromBody] TeamMeetingRequest request)
        {
            var managerId = HttpContext.GetUId();
            request.OrganizerId = managerId;
            request.Create_by = HttpContext.GetName();
            request.Create_time = DateTime.Now;
            
            var result = await _managerDashboardService.ScheduleTeamMeetingAsync(request);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Assign task to team member
        /// </summary>
        [HttpPost("team/assign-task")]
        [ActionPermissionFilter("hr:manager:task")]
        public async Task<IActionResult> AssignTask([FromBody] TaskAssignmentRequest request)
        {
            var managerId = HttpContext.GetUId();
            request.AssignedBy = managerId;
            request.Create_by = HttpContext.GetName();
            request.Create_time = DateTime.Now;
            
            var result = await _managerDashboardService.AssignTaskAsync(request);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }

        /// <summary>
        /// Get team workload
        /// </summary>
        [HttpGet("team/workload")]
        [ActionPermissionFilter("hr:manager:workload")]
        public async Task<IActionResult> GetTeamWorkload()
        {
            var managerId = HttpContext.GetUId();
            var workload = await _managerDashboardService.GetTeamWorkloadAsync(managerId);
            return SUCCESS(workload);
        }

        /// <summary>
        /// Submit team performance review
        /// </summary>
        [HttpPost("team/performance-review")]
        [ActionPermissionFilter("hr:manager:review")]
        public async Task<IActionResult> SubmitPerformanceReview([FromBody] PerformanceReviewRequest request)
        {
            var managerId = HttpContext.GetUId();
            request.ReviewerId = managerId;
            request.Create_by = HttpContext.GetName();
            request.Create_time = DateTime.Now;
            
            var result = await _managerDashboardService.SubmitPerformanceReviewAsync(request);
            return result > 0 ? SUCCESS(result) : ToResponse(ResultCode.FAIL);
        }
    }

    public class ManagerApprovalRequest
    {
        public string Comments { get; set; }
    }

    public class ManagerRejectionRequest
    {
        public string Reason { get; set; }
    }

    public class TeamMeetingRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OrganizerId { get; set; }
        public string Subject { get; set; }
        public DateTime MeetingDate { get; set; }
        public string Location { get; set; }
        public string Agenda { get; set; }
        public string Create_by { get; set; }
        public DateTime Create_time { get; set; }
    }

    public class TaskAssignmentRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignedBy { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignedTo { get; set; }
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }
        public string Create_by { get; set; }
        public DateTime Create_time { get; set; }
    }

    public class PerformanceReviewRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewerId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public int ReviewPeriodYear { get; set; }
        public int ReviewPeriodQuarter { get; set; }
        public decimal OverallRating { get; set; }
        public string Strengths { get; set; }
        public string AreasForImprovement { get; set; }
        public string GoalsAchieved { get; set; }
        public string FutureGoals { get; set; }
        public string Comments { get; set; }
        public string Create_by { get; set; }
        public DateTime Create_time { get; set; }
    }
}

