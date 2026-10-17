using Microsoft.AspNetCore.Mvc;
using System;
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
    /// Daily Activity Report - تقرير النشاط اليومي
    /// Aggregates employee activities from multiple sources into a unified timeline
    /// </summary>
    [Verify]
    [Route("api/reports/daily-activity")]
    [ApiExplorerSettings(GroupName = "marketing")]
    public class DailyActivityReportController : BaseController
    {
        private readonly IDailyActivityReportService _reportService;
        private readonly IHrEmployeeCoreService _employeeCoreService;

        public DailyActivityReportController(
            IDailyActivityReportService reportService,
            IHrEmployeeCoreService employeeCoreService)
        {
            _reportService = reportService;
            _employeeCoreService = employeeCoreService;
        }

        /// <summary>
        /// Get full daily activity report (summary + timeline + team performance)
        /// </summary>
        [HttpGet("report")]
        [ActionPermissionFilter("marketing:reports:view")]
        public async Task<IActionResult> GetReport([FromQuery] EmployeeActivityQueryDto query)
        {
            await ApplyEmployeeFilter(query);
            var result = await _reportService.GetDailyReportAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get KPI summary only
        /// </summary>
        [HttpGet("summary")]
        [ActionPermissionFilter("marketing:reports:view")]
        public async Task<IActionResult> GetSummary([FromQuery] EmployeeActivityQueryDto query)
        {
            await ApplyEmployeeFilter(query);
            var result = await _reportService.GetSummaryAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get activity timeline only
        /// </summary>
        [HttpGet("timeline")]
        [ActionPermissionFilter("marketing:reports:view")]
        public async Task<IActionResult> GetTimeline([FromQuery] EmployeeActivityQueryDto query)
        {
            await ApplyEmployeeFilter(query);
            var result = await _reportService.GetTimelineAsync(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get team performance comparison table
        /// </summary>
        [HttpGet("team-performance")]
        [ActionPermissionFilter("marketing:reports:view")]
        public async Task<IActionResult> GetTeamPerformance([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _reportService.GetTeamPerformanceAsync(startDate, endDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// If EmployeeId is null and user is not admin or does not have reports permission,
        /// auto-set to current user's employee ID.
        /// Users with marketing:reports:view permission can see all report data
        /// without needing dataScope=1 (which would also expose all leads).
        /// </summary>
        private async Task ApplyEmployeeFilter(EmployeeActivityQueryDto query)
        {
            if (!query.EmployeeId.HasValue && !HttpContextExtension.IsAdmin(HttpContext))
            {
                // Users with reports permission can view aggregated data for all employees
                var permissions = JwtUtil.GetLoginUser(HttpContext)?.Permissions ?? new List<string>();
                bool hasReportsAccess = permissions.Contains("*:*:*") || permissions.Contains("marketing:reports:view");
                if (hasReportsAccess) return;

                var currentUserId = HttpContext.GetUId();
                if (currentUserId > 0)
                {
                    var employeeId = await _employeeCoreService.GetEmployeeIdByUserIdAsync(currentUserId);
                    if (employeeId.HasValue && employeeId.Value > 0)
                    {
                        query.EmployeeId = employeeId.Value;
                    }
                }
            }
        }
    }
}
