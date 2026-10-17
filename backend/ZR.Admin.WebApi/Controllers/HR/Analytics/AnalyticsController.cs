using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using SqlSugar;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Analytics
{
    /// <summary>
    /// Analytics Management Controller
///hhhahahandiuf
    /// </summary>
    [Verify]
    [Route("api/hr/analytics")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class AnalyticsController : BaseController
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        #region Analytics Reports

        /// <summary>
        /// Get analytics reports
        /// </summary>
        [HttpGet("reports")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetAnalyticsReports([FromQuery] AnalyticsReportQueryDto dto)
        {
            var predicate = Expressionable.Create<HrAnalyticsReport>();
            
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.ReportType), 
                it => it.ReportType == dto.ReportType);
            predicate = predicate.AndIF(dto.StartDate != null, 
                it => it.Create_time >= dto.StartDate);
            predicate = predicate.AndIF(dto.EndDate != null, 
                it => it.Create_time <= dto.EndDate);
            
            var response = await _analyticsService.GetAnalyticsReportsAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Generate analytics report
        /// </summary>
        [HttpPost("reports/generate")]
        [ActionPermissionFilter("hr:analytics:generate")]
        [Log(Title = "Generate Analytics Report", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> GenerateReport([FromBody] GenerateReportDto dto)
        {
            var report = await _analyticsService.GenerateReportAsync(dto.ReportType, dto.Parameters);
            return SUCCESS(report);
        }

        /// <summary>
        /// Save analytics report
        /// </summary>
        [HttpPost("reports")]
        [ActionPermissionFilter("hr:analytics:add")]
        [Log(Title = "Analytics Report", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SaveReport([FromBody] HrAnalyticsReport report)
        {
            report.Create_by = HttpContext.GetName();
            report.Create_time = DateTime.Now;
            
            var result = await _analyticsService.SaveReportAsync(report);
            return SUCCESS(result);
        }

        /// <summary>
        /// Export analytics report
        /// </summary>
        [HttpPost("reports/export")]
        [ActionPermissionFilter("hr:analytics:export")]
        [Log(Title = "Export Analytics Report", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportReport([FromBody] ExportReportDto dto)
        {
            var reportData = await _analyticsService.ExportAnalyticsReportAsync(dto.ReportType, dto.Format, dto.Parameters);
            
            var fileName = $"{dto.ReportType}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{dto.Format.ToLower()}";
            var contentType = dto.Format.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            
            return File(reportData, contentType, fileName);
        }

        #endregion

        #region Workforce Metrics

        /// <summary>
        /// Get workforce metrics
        /// </summary>
        [HttpGet("workforce/metrics")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetWorkforceMetrics([FromQuery] DateTime? date)
        {
            var metricsDate = date ?? DateTime.Now;
            var metrics = await _analyticsService.GetWorkforceMetricsAsync(metricsDate);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Calculate workforce metrics
        /// </summary>
        [HttpPost("workforce/calculate")]
        [ActionPermissionFilter("hr:analytics:calculate")]
        [Log(Title = "Calculate Workforce Metrics", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> CalculateWorkforceMetrics([FromBody] DateTime date)
        {
            var metrics = await _analyticsService.CalculateWorkforceMetricsAsync(date);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Get workforce metrics trend
        /// </summary>
        [HttpGet("workforce/trend")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetWorkforceMetricsTrend([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var trend = await _analyticsService.GetWorkforceMetricsTrendAsync(startDate, endDate);
            return SUCCESS(trend);
        }

        #endregion

        #region Performance Metrics

        /// <summary>
        /// Get performance metrics
        /// </summary>
        [HttpGet("performance/metrics")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetPerformanceMetrics([FromQuery] int year, [FromQuery] DateTime? date)
        {
            var metricsDate = date ?? DateTime.Now;
            var metrics = await _analyticsService.GetPerformanceMetricsAsync(year, metricsDate);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Calculate performance metrics
        /// </summary>
        [HttpPost("performance/calculate")]
        [ActionPermissionFilter("hr:analytics:calculate")]
        [Log(Title = "Calculate Performance Metrics", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> CalculatePerformanceMetrics([FromBody] DateTime date)
        {
            var metrics = await _analyticsService.CalculatePerformanceMetricsAsync(date);
            return SUCCESS(metrics);
        }

        #endregion

        #region Compensation Metrics

        /// <summary>
        /// Get compensation metrics
        /// </summary>
        [HttpGet("compensation/metrics")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetCompensationMetrics([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
        {
            var metrics = await _analyticsService.GetCompensationMetricsAsync(periodStart, periodEnd);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Calculate compensation metrics
        /// </summary>
        [HttpPost("compensation/calculate")]
        [ActionPermissionFilter("hr:analytics:calculate")]
        [Log(Title = "Calculate Compensation Metrics", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> CalculateCompensationMetrics([FromBody] DateTime date)
        {
            var metrics = await _analyticsService.CalculateCompensationMetricsAsync(date);
            return SUCCESS(metrics);
        }

        #endregion

        #region Leave Metrics

        /// <summary>
        /// Get leave metrics
        /// </summary>
        [HttpGet("leave/metrics")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetLeaveMetrics([FromQuery] int year)
        {
            var metrics = await _analyticsService.GetLeaveMetricsAsync(year);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Calculate leave metrics
        /// </summary>
        [HttpPost("leave/calculate")]
        [ActionPermissionFilter("hr:analytics:calculate")]
        [Log(Title = "Calculate Leave Metrics", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> CalculateLeaveMetrics([FromBody] int year)
        {
            var metrics = await _analyticsService.CalculateLeaveMetricsAsync(year);
            return SUCCESS(metrics);
        }

        #endregion

        #region Training Metrics

        /// <summary>
        /// Get training metrics
        /// </summary>
        [HttpGet("training/metrics")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetTrainingMetrics([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
        {
            var metrics = await _analyticsService.GetTrainingMetricsAsync(periodStart, periodEnd);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Calculate training metrics
        /// </summary>
        [HttpPost("training/calculate")]
        [ActionPermissionFilter("hr:analytics:calculate")]
        [Log(Title = "Calculate Training Metrics", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> CalculateTrainingMetrics([FromBody] TrainingMetricsDto dto)
        {
            var metrics = await _analyticsService.CalculateTrainingMetricsAsync(dto.PeriodStart, dto.PeriodEnd);
            return SUCCESS(metrics);
        }

        #endregion

        #region Predictive Insights

        /// <summary>
        /// Generate predictive insights
        /// </summary>
        [HttpGet("insights/predictive")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetPredictiveInsights([FromQuery] DateTime? date)
        {
            var insightDate = date ?? DateTime.Now;
            var insights = await _analyticsService.GeneratePredictiveInsightsAsync(insightDate);
            return SUCCESS(insights);
        }

        /// <summary>
        /// Get predictive insights history
        /// </summary>
        [HttpGet("insights/history")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetPredictiveInsightsHistory([FromQuery] int months = 12)
        {
            var history = await _analyticsService.GetPredictiveInsightsHistoryAsync(months);
            return SUCCESS(history);
        }

        #endregion

        #region KPI Dashboard

        /// <summary>
        /// Get KPI dashboard
        /// </summary>
        [HttpGet("kpi/dashboard")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetKpiDashboard([FromQuery] int year, [FromQuery] int? month, [FromQuery] int? quarter)
        {
            var dashboard = await _analyticsService.GetKpiDashboardAsync(year, month, quarter);
            return SUCCESS(dashboard);
        }

        /// <summary>
        /// Update KPI dashboard
        /// </summary>
        [HttpPut("kpi/dashboard")]
        [ActionPermissionFilter("hr:analytics:edit")]
        [Log(Title = "Update KPI Dashboard", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateKpiDashboard([FromBody] HrKpiDashboard kpi)
        {
            var result = await _analyticsService.UpdateKpiDashboardAsync(kpi);
            return SUCCESS(result);
        }

        #endregion

        #region Dashboard Metrics

        /// <summary>
        /// Get dashboard metrics
        /// </summary>
        [HttpGet("dashboard/metrics")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetDashboardMetrics([FromQuery] string dashboardType, [FromQuery] long? userId)
        {
            var metrics = await _analyticsService.GetDashboardMetricsAsync(dashboardType, userId);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Update dashboard metrics
        /// </summary>
        [HttpPut("dashboard/metrics")]
        [ActionPermissionFilter("hr:analytics:edit")]
        [Log(Title = "Update Dashboard Metrics", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateDashboardMetrics([FromBody] List<HrDashboardMetrics> metrics)
        {
            var result = await _analyticsService.UpdateDashboardMetricsAsync(metrics);
            return SUCCESS(result);
        }

        #endregion

        #region Job Position Analytics

        /// <summary>
        /// Get job position analytics
        /// </summary>
        [HttpGet("positions/analytics")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetJobPositionAnalytics()
        {
            var analytics = await _analyticsService.GetJobPositionAnalyticsAsync();
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Get job position by ID
        /// </summary>
        [HttpGet("positions/{positionId}")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetJobPositionById(long positionId)
        {
            var position = await _analyticsService.GetJobPositionByIdAsync(positionId);
            return SUCCESS(position);
        }

        /// <summary>
        /// Update job position
        /// </summary>
        [HttpPut("positions/{positionId}")]
        [ActionPermissionFilter("hr:analytics:edit")]
        [Log(Title = "Update Job Position", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateJobPosition(long positionId, [FromBody] HrJobPosition position)
        {
            position.PositionId = positionId;
            var result = await _analyticsService.UpdateJobPositionAsync(position);
            return SUCCESS(result);
        }

        #endregion

        #region Payroll History Analytics

        /// <summary>
        /// Get payroll history
        /// </summary>
        [HttpGet("payroll/history/{employeeId}")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetPayrollHistory(long employeeId)
        {
            var history = await _analyticsService.GetPayrollHistoryAsync(employeeId);
            return SUCCESS(history);
        }

        /// <summary>
        /// Get payroll trends
        /// </summary>
        [HttpGet("payroll/trends/{employeeId}")]
        [ActionPermissionFilter("hr:analytics:list")]
        public async Task<IActionResult> GetPayrollTrends(long employeeId, [FromQuery] int months = 12)
        {
            var trends = await _analyticsService.GetPayrollTrendsAsync(employeeId, months);
            return SUCCESS(trends);
        }

        #endregion
    }
}
