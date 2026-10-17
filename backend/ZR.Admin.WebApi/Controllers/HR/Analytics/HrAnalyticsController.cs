using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Admin.WebApi.Filters;
using ZR.Service.HR.IService;
using Microsoft.AspNetCore.Authorization;
using ZR.Admin.WebApi.Extensions;
using ZR.Common;

namespace ZR.Admin.WebApi.Controllers.HR.Analytics
{
    /// <summary>
    /// HR Analytics and Reports Controller
    /// </summary>
    [Verify]
    [Route("api/hr/analytics")]
    [ApiExplorerSettings(GroupName = "hr")]
    [ApiController]
    [Authorize]
    public class HrAnalyticsController : BaseController
    {
        private readonly IAnalyticsService _analyticsService;

        public HrAnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get workforce metrics
        /// </summary>
        [HttpGet("workforce")]
        [ActionPermissionFilter("hr:analytics:workforce")]
        public async Task<IActionResult> GetWorkforceMetrics()
        {
            var metrics = await _analyticsService.GetWorkforceMetricsAsync();
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Get performance metrics
        /// </summary>
        [HttpGet("performance")]
        [ActionPermissionFilter("hr:analytics:performance")]
        public async Task<IActionResult> GetPerformanceMetrics([FromQuery] int year)
        {
            var metrics = await _analyticsService.GetPerformanceMetricsAsync(year);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Get compensation metrics
        /// </summary>
        [HttpGet("compensation")]
        [ActionPermissionFilter("hr:analytics:compensation")]
        public async Task<IActionResult> GetCompensationMetrics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var metrics = await _analyticsService.GetCompensationMetricsAsync(startDate, endDate);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Generate analytics report
        /// </summary>
        [HttpPost("report")]
        [ActionPermissionFilter("hr:analytics:report")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequest request)
        {
            var report = await _analyticsService.GenerateReportAsync(request.ReportType, request.Parameters);
            return SUCCESS(report);
        }

        /// <summary>
        /// Get predictive insights
        /// </summary>
        [HttpGet("insights")]
        [ActionPermissionFilter("hr:analytics:insights")]
        public async Task<IActionResult> GetPredictiveInsights()
        {
            var insights = await _analyticsService.GeneratePredictiveInsightsAsync();
            return SUCCESS(insights);
        }

        /// <summary>
        /// Get turnover analysis
        /// </summary>
        [HttpGet("turnover")]
        [ActionPermissionFilter("hr:analytics:turnover")]
        public async Task<IActionResult> GetTurnoverAnalysis([FromQuery] int year)
        {
            var analysis = await _analyticsService.GetTurnoverAnalysisAsync(year);
            return SUCCESS(analysis);
        }

        /// <summary>
        /// Get attendance analytics
        /// </summary>
        [HttpGet("attendance")]
        [ActionPermissionFilter("hr:analytics:attendance")]
        public async Task<IActionResult> GetAttendanceAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var analytics = await _analyticsService.GetAttendanceAnalyticsAsync(startDate, endDate);
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Get leave analytics
        /// </summary>
        [HttpGet("leave")]
        [ActionPermissionFilter("hr:analytics:leave")]
        public async Task<IActionResult> GetLeaveAnalytics([FromQuery] int year)
        {
            var analytics = await _analyticsService.GetLeaveAnalyticsAsync(year);
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Get training analytics
        /// </summary>
        [HttpGet("training")]
        [ActionPermissionFilter("hr:analytics:training")]
        public async Task<IActionResult> GetTrainingAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var analytics = await _analyticsService.GetTrainingAnalyticsAsync(startDate, endDate);
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Get recruitment analytics
        /// </summary>
        [HttpGet("recruitment")]
        [ActionPermissionFilter("hr:analytics:recruitment")]
        public async Task<IActionResult> GetRecruitmentAnalytics([FromQuery] int year)
        {
            var analytics = await _analyticsService.GetRecruitmentAnalyticsAsync(year);
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Get diversity metrics
        /// </summary>
        [HttpGet("diversity")]
        [ActionPermissionFilter("hr:analytics:diversity")]
        public async Task<IActionResult> GetDiversityMetrics()
        {
            var metrics = await _analyticsService.GetDiversityMetricsAsync();
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Get employee satisfaction metrics
        /// </summary>
        [HttpGet("satisfaction")]
        [ActionPermissionFilter("hr:analytics:satisfaction")]
        public async Task<IActionResult> GetEmployeeSatisfactionMetrics([FromQuery] int year)
        {
            var metrics = await _analyticsService.GetEmployeeSatisfactionMetricsAsync(year);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Get department comparison
        /// </summary>
        [HttpGet("department-comparison")]
        [ActionPermissionFilter("hr:analytics:department")]
        public async Task<IActionResult> GetDepartmentComparison()
        {
            var comparison = await _analyticsService.GetDepartmentComparisonAsync();
            return SUCCESS(comparison);
        }

        /// <summary>
        /// Get cost analysis
        /// </summary>
        [HttpGet("cost-analysis")]
        [ActionPermissionFilter("hr:analytics:cost")]
        public async Task<IActionResult> GetCostAnalysis([FromQuery] int year)
        {
            var analysis = await _analyticsService.GetCostAnalysisAsync(year);
            return SUCCESS(analysis);
        }

        /// <summary>
        /// Export analytics dashboard
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter("hr:analytics:export")]
        public async Task<IActionResult> ExportAnalyticsDashboard([FromQuery] ExportRequest request)
        {
            var export = await _analyticsService.ExportDashboardAsync(request.Format, request.StartDate, request.EndDate);
            return File(export, "application/octet-stream", $"hr_analytics_{DateTime.Now:yyyyMMdd}.{request.Format}");
        }

        /// <summary>
        /// Get KPI dashboard
        /// </summary>
        [HttpGet("kpi")]
        [ActionPermissionFilter("hr:analytics:kpi")]
        public async Task<IActionResult> GetKpiDashboard()
        {
            var dashboard = await _analyticsService.GetKpiDashboardAsync();
            return SUCCESS(dashboard);
        }

        /// <summary>
        /// Get trend analysis
        /// </summary>
        [HttpGet("trends")]
        [ActionPermissionFilter("hr:analytics:trends")]
        public async Task<IActionResult> GetTrendAnalysis([FromQuery] string metric, [FromQuery] int months = 12)
        {
            var trends = await _analyticsService.GetTrendAnalysisAsync(metric, months);
            return SUCCESS(trends);
        }

        /// <summary>
        /// Get benchmark comparison
        /// </summary>
        [HttpGet("benchmark")]
        [ActionPermissionFilter("hr:analytics:benchmark")]
        public async Task<IActionResult> GetBenchmarkComparison()
        {
            var benchmark = await _analyticsService.GetBenchmarkComparisonAsync();
            return SUCCESS(benchmark);
        }

        /// <summary>
        /// Get retention analysis
        /// </summary>
        [HttpGet("retention")]
        [ActionPermissionFilter("hr:analytics:retention")]
        public async Task<IActionResult> GetRetentionAnalysis([FromQuery] int year)
        {
            var analysis = await _analyticsService.GetRetentionAnalysisAsync(year);
            return SUCCESS(analysis);
        }
    }

    public class ReportRequest
    {
        public string ReportType { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    public class ExportRequest
    {
        public string Format { get; set; } = "pdf";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
