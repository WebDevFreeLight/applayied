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
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;
using ZR.Model.HR.Performance;

namespace ZR.Admin.WebApi.Controllers.HR.Performance
{
    /// <summary>
    /// KPI Management Controller
    /// </summary>
    [Verify]
    [Route("api/hr/kpi")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class KpiController : BaseController
    {
        private readonly IKpiService _kpiService;

        public KpiController(IKpiService kpiService)
        {
            _kpiService = kpiService;
        }

        #region KPI Management

        /// <summary>
        /// Get KPIs
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpis([FromQuery] KpiQueryDto dto)
        {
            var predicate = Expressionable.Create<HrKpi>();
            
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.KpiName), 
                it => it.KpiName.Contains(dto.KpiName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Category), 
                it => it.Category == dto.Category);
            predicate = predicate.AndIF(dto.DepartmentId > 0, 
                it => it.DepartmentId == dto.DepartmentId);
            predicate = predicate.AndIF(dto.PositionId > 0, 
                it => it.PositionId == dto.PositionId);
            predicate = predicate.AndIF(dto.IsActive.HasValue, 
                it => it.IsActive == dto.IsActive);
            
            var response = await _kpiService.GetKpisAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get active KPIs
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetActiveKpis()
        {
            var kpis = await _kpiService.GetActiveKpisAsync();
            return SUCCESS(kpis);
        }

        /// <summary>
        /// Get KPI by ID
        /// </summary>
        [HttpGet("{kpiId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpiById(long kpiId)
        {
            var kpi = await _kpiService.GetKpiByIdAsync(kpiId);
            return SUCCESS(kpi);
        }

        /// <summary>
        /// Add KPI
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("hr:kpi:add")]
        [Log(Title = "KPI Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddKpi([FromBody] KpiDto dto)
        {
            var kpi = dto.Adapt<HrKpi>();
            kpi.Create_by = HttpContext.GetName();
            kpi.Create_time = DateTime.Now;
            
            var result = await _kpiService.AddKpiAsync(kpi);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update KPI
        /// </summary>
        [HttpPut("{kpiId}")]
        [ActionPermissionFilter("hr:kpi:edit")]
        [Log(Title = "KPI Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateKpi(long kpiId, [FromBody] KpiDto dto)
        {
            var kpi = dto.Adapt<HrKpi>();
            kpi.KpiId = kpiId;
            kpi.Update_by = HttpContext.GetName();
            kpi.Update_time = DateTime.Now;
            
            var result = await _kpiService.UpdateKpiAsync(kpi);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete KPI
        /// </summary>
        [HttpDelete("{kpiId}")]
        [ActionPermissionFilter("hr:kpi:delete")]
        [Log(Title = "KPI Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteKpi(long kpiId)
        {
            var result = await _kpiService.DeleteKpiAsync(kpiId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get KPIs by category
        /// </summary>
        [HttpGet("category/{category}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpisByCategory(string category)
        {
            var kpis = await _kpiService.GetKpisByCategoryAsync(category);
            return SUCCESS(kpis);
        }

        /// <summary>
        /// Get department KPIs
        /// </summary>
        [HttpGet("department/{departmentId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetDepartmentKpis(long departmentId)
        {
            var kpis = await _kpiService.GetDepartmentKpisAsync(departmentId);
            return SUCCESS(kpis);
        }

        /// <summary>
        /// Get position KPIs
        /// </summary>
        [HttpGet("position/{positionId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetPositionKpis(long positionId)
        {
            var kpis = await _kpiService.GetPositionKpisAsync(positionId);
            return SUCCESS(kpis);
        }

        #endregion

        #region Employee KPI Assignment

        /// <summary>
        /// Get employee KPIs
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetEmployeeKpis(long employeeId)
        {
            var assignments = await _kpiService.GetEmployeeKpisAsync(employeeId);
            return SUCCESS(assignments);
        }

        /// <summary>
        /// Assign KPI to employee
        /// </summary>
        [HttpPost("assignment")]
        [ActionPermissionFilter("hr:kpi:assign")]
        [Log(Title = "KPI Assignment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AssignKpiToEmployee([FromBody] EmployeeKpiDto dto)
        {
            var assignment = dto.Adapt<HrEmployeeKpi>();
            assignment.Create_by = HttpContext.GetName();
            assignment.Create_time = DateTime.Now;
            assignment.AssignmentDate = DateTime.Now;
            
            var result = await _kpiService.AssignKpiToEmployeeAsync(assignment);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update employee KPI
        /// </summary>
        [HttpPut("assignment/{assignmentId}")]
        [ActionPermissionFilter("hr:kpi:edit")]
        [Log(Title = "KPI Assignment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateEmployeeKpi(long assignmentId, [FromBody] EmployeeKpiDto dto)
        {
            var assignment = dto.Adapt<HrEmployeeKpi>();
            assignment.AssignmentId = assignmentId;
            assignment.Update_by = HttpContext.GetName();
            assignment.Update_time = DateTime.Now;
            
            var result = await _kpiService.UpdateEmployeeKpiAsync(assignment);
            return SUCCESS(result);
        }

        /// <summary>
        /// Remove employee KPI
        /// </summary>
        [HttpDelete("assignment/{assignmentId}")]
        [ActionPermissionFilter("hr:kpi:delete")]
        [Log(Title = "KPI Assignment", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> RemoveEmployeeKpi(long assignmentId)
        {
            var result = await _kpiService.RemoveEmployeeKpiAsync(assignmentId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get KPI assignments by period
        /// </summary>
        [HttpGet("assignments/period")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpiAssignmentsByPeriod([FromQuery] int year, [FromQuery] int? quarter)
        {
            var assignments = await _kpiService.GetKpiAssignmentsByPeriodAsync(year, quarter);
            return SUCCESS(assignments);
        }

        /// <summary>
        /// Bulk assign KPIs
        /// </summary>
        [HttpPost("assignment/bulk")]
        [ActionPermissionFilter("hr:kpi:assign")]
        [Log(Title = "Bulk KPI Assignment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> BulkAssignKpis([FromBody] BulkKpiAssignmentDto dto)
        {
            var result = await _kpiService.BulkAssignKpisAsync(dto.EmployeeIds, dto.KpiIds);
            return SUCCESS(result);
        }

        #endregion

        #region KPI Achievement

        /// <summary>
        /// Get KPI achievements
        /// </summary>
        [HttpGet("achievements/{employeeId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpiAchievements(long employeeId, [FromQuery] DateTime periodDate)
        {
            var achievements = await _kpiService.GetKpiAchievementsAsync(employeeId, periodDate);
            return SUCCESS(achievements);
        }

        /// <summary>
        /// Record KPI achievement
        /// </summary>
        [HttpPost("achievements")]
        [ActionPermissionFilter("hr:kpi:record")]
        [Log(Title = "KPI Achievement", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> RecordKpiAchievement([FromBody] KpiAchievementDto dto)
        {
            var achievement = dto.Adapt<HrKpiAchievement>();
            achievement.Create_by = HttpContext.GetName();
            achievement.Create_time = DateTime.Now;
            achievement.AchievementDate = DateTime.Now;
            
            var result = await _kpiService.RecordKpiAchievementAsync(achievement);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update KPI achievement
        /// </summary>
        [HttpPut("achievements/{achievementId}")]
        [ActionPermissionFilter("hr:kpi:edit")]
        [Log(Title = "KPI Achievement", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateKpiAchievement(long achievementId, [FromBody] KpiAchievementDto dto)
        {
            var achievement = dto.Adapt<HrKpiAchievement>();
            achievement.AchievementId = achievementId;
            achievement.Update_by = HttpContext.GetName();
            achievement.Update_time = DateTime.Now;
            
            var result = await _kpiService.UpdateKpiAchievementAsync(achievement);
            return SUCCESS(result);
        }

        /// <summary>
        /// Calculate KPI score
        /// </summary>
        [HttpGet("score/{employeeId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> CalculateKpiScore(long employeeId, [FromQuery] DateTime periodDate)
        {
            var score = await _kpiService.CalculateKpiScoreAsync(employeeId, periodDate);
            return SUCCESS(new { Score = score });
        }

        /// <summary>
        /// Verify KPI achievement
        /// </summary>
        [HttpPost("achievements/{achievementId}/verify")]
        [ActionPermissionFilter("hr:kpi:verify")]
        [Log(Title = "KPI Achievement Verification", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> VerifyKpiAchievement(long achievementId)
        {
            var verifiedBy = Convert.ToInt64(HttpContext.GetUId());
            var result = await _kpiService.VerifyKpiAchievementAsync(achievementId, verifiedBy);
            return SUCCESS(result);
        }

        #endregion

        #region KPI Metrics (Legacy Support)

        /// <summary>
        /// Get KPI metrics
        /// </summary>
        [HttpGet("metrics/{employeeId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpiMetrics(long employeeId)
        {
            var metrics = await _kpiService.GetKpiMetricsAsync(employeeId);
            return SUCCESS(metrics);
        }

        /// <summary>
        /// Add KPI metric
        /// </summary>
        [HttpPost("metrics")]
        [ActionPermissionFilter("hr:kpi:add")]
        [Log(Title = "KPI Metric", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddKpiMetric([FromBody] HrKpiMetric metric)
        {
            metric.Create_by = HttpContext.GetName();
            metric.Create_time = DateTime.Now;
            
            var result = await _kpiService.AddKpiMetricAsync(metric);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update KPI metric
        /// </summary>
        [HttpPut("metrics/{metricId}")]
        [ActionPermissionFilter("hr:kpi:edit")]
        [Log(Title = "KPI Metric", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateKpiMetric(long metricId, [FromBody] HrKpiMetric metric)
        {
            metric.MetricId = metricId;
            metric.Update_by = HttpContext.GetName();
            metric.Update_time = DateTime.Now;
            
            var result = await _kpiService.UpdateKpiMetricAsync(metric);
            return SUCCESS(result);
        }

        #endregion

        #region KPI Records

        /// <summary>
        /// Get KPI records
        /// </summary>
        [HttpGet("records/{employeeId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpiRecords(long employeeId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var records = await _kpiService.GetKpiRecordsAsync(employeeId, startDate, endDate);
            return SUCCESS(records);
        }

        /// <summary>
        /// Add KPI record
        /// </summary>
        [HttpPost("records")]
        [ActionPermissionFilter("hr:kpi:add")]
        [Log(Title = "KPI Record", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddKpiRecord([FromBody] HrKpiRecord record)
        {
            record.Create_by = HttpContext.GetName();
            record.Create_time = DateTime.Now;
            
            var result = await _kpiService.AddKpiRecordAsync(record);
            return SUCCESS(result);
        }

        #endregion

        #region KPI Analytics

        /// <summary>
        /// Get KPI dashboard
        /// </summary>
        [HttpGet("dashboard/{employeeId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpiDashboard([FromQuery] long? employeeId, [FromQuery] int year)
        {
            var dashboard = await _kpiService.GetKpiDashboardAsync(employeeId, year);
            return SUCCESS(dashboard);
        }

        /// <summary>
        /// Get team KPI performance
        /// </summary>
        [HttpGet("team-performance/{managerId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetTeamKpiPerformance(long managerId, [FromQuery] DateTime periodDate)
        {
            var performance = await _kpiService.GetTeamKpiPerformanceAsync(managerId, periodDate);
            return SUCCESS(performance);
        }

        /// <summary>
        /// Get department KPI analytics
        /// </summary>
        [HttpGet("department-analytics/{departmentId}")]
        [ActionPermissionFilter("hr:kpi:analytics")]
        public async Task<IActionResult> GetDepartmentKpiAnalytics(long departmentId, [FromQuery] int year)
        {
            var analytics = await _kpiService.GetDepartmentKpiAnalyticsAsync(departmentId, year);
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Get KPI trends
        /// </summary>
        [HttpGet("trends/{employeeId}")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpiTrends(long employeeId, [FromQuery] int months = 12)
        {
            var trends = await _kpiService.GetKpiTrendsAsync(employeeId, months);
            return SUCCESS(trends);
        }

        /// <summary>
        /// Get top performers
        /// </summary>
        [HttpGet("top-performers")]
        [ActionPermissionFilter("hr:kpi:analytics")]
        public async Task<IActionResult> GetTopPerformers([FromQuery] DateTime periodDate, [FromQuery] int topN = 10)
        {
            var performers = await _kpiService.GetTopPerformersAsync(periodDate, topN);
            return SUCCESS(performers);
        }

        /// <summary>
        /// Get underperformers
        /// </summary>
        [HttpGet("underperformers")]
        [ActionPermissionFilter("hr:kpi:analytics")]
        public async Task<IActionResult> GetUnderperformers([FromQuery] DateTime periodDate, [FromQuery] int bottomN = 10)
        {
            var performers = await _kpiService.GetUnderperformersAsync(periodDate, bottomN);
            return SUCCESS(performers);
        }

        #endregion

        #region KPI Calculation

        /// <summary>
        /// Calculate weighted score
        /// </summary>
        [HttpPost("calculate/weighted-score")]
        [ActionPermissionFilter("hr:kpi:calculate")]
        [Log(Title = "Calculate Weighted KPI Score", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> CalculateWeightedScore([FromBody] CalculateScoreDto dto)
        {
            var score = await _kpiService.CalculateWeightedScoreAsync(dto.EmployeeId, dto.PeriodDate);
            return SUCCESS(new { WeightedScore = score });
        }

        /// <summary>
        /// Process monthly KPIs
        /// </summary>
        [HttpPost("process/monthly")]
        [ActionPermissionFilter("hr:kpi:process")]
        [Log(Title = "Process Monthly KPIs", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> ProcessMonthlyKpis([FromBody] DateTime month)
        {
            var result = await _kpiService.ProcessMonthlyKpisAsync(month);
            return SUCCESS(new { ProcessedCount = result });
        }

        /// <summary>
        /// Process quarterly KPIs
        /// </summary>
        [HttpPost("process/quarterly")]
        [ActionPermissionFilter("hr:kpi:process")]
        [Log(Title = "Process Quarterly KPIs", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> ProcessQuarterlyKpis([FromBody] QuarterlyKpiDto dto)
        {
            var result = await _kpiService.ProcessQuarterlyKpisAsync(dto.Year, dto.Quarter);
            return SUCCESS(new { ProcessedCount = result });
        }

        /// <summary>
        /// Process annual KPIs
        /// </summary>
        [HttpPost("process/annual")]
        [ActionPermissionFilter("hr:kpi:process")]
        [Log(Title = "Process Annual KPIs", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> ProcessAnnualKpis([FromBody] int year)
        {
            var result = await _kpiService.ProcessAnnualKpisAsync(year);
            return SUCCESS(new { ProcessedCount = result });
        }

        #endregion

        #region KPI Reports

        /// <summary>
        /// Generate KPI report
        /// </summary>
        [HttpPost("reports/individual")]
        [ActionPermissionFilter("hr:kpi:export")]
        [Log(Title = "Generate KPI Report", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> GenerateKpiReport([FromBody] KpiReportDto dto)
        {
            var reportData = await _kpiService.GenerateKpiReportAsync(dto.EmployeeId, dto.StartDate, dto.EndDate);
            var fileName = $"KPI_Report_{dto.EmployeeId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            return File(reportData, "application/pdf", fileName);
        }

        /// <summary>
        /// Generate team KPI report
        /// </summary>
        [HttpPost("reports/team")]
        [ActionPermissionFilter("hr:kpi:export")]
        [Log(Title = "Generate Team KPI Report", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> GenerateTeamKpiReport([FromBody] TeamKpiReportDto dto)
        {
            var reportData = await _kpiService.GenerateTeamKpiReportAsync(dto.ManagerId, dto.PeriodDate);
            var fileName = $"Team_KPI_Report_{dto.ManagerId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            return File(reportData, "application/pdf", fileName);
        }

        /// <summary>
        /// Export KPI data
        /// </summary>
        [HttpPost("export")]
        [ActionPermissionFilter("hr:kpi:export")]
        [Log(Title = "Export KPI Data", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportKpiData([FromBody] ExportKpiDto dto)
        {
            var predicate = Expressionable.Create<HrKpi>();
            
            if (!string.IsNullOrEmpty(dto.Category))
                predicate = predicate.And(it => it.Category == dto.Category);
            if (dto.DepartmentId > 0)
                predicate = predicate.And(it => it.DepartmentId == dto.DepartmentId);
            if (dto.IsActive.HasValue)
                predicate = predicate.And(it => it.IsActive == dto.IsActive);
            
            var exportData = await _kpiService.ExportKpiDataAsync(predicate.ToExpression(), dto.Format);
            var fileName = $"KPI_Data_{DateTime.Now:yyyyMMdd_HHmmss}.{dto.Format.ToLower()}";
            var contentType = dto.Format.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            
            return File(exportData, contentType, fileName);
        }

        #endregion

        #region KPI Templates

        /// <summary>
        /// Get KPI templates
        /// </summary>
        [HttpGet("templates")]
        [ActionPermissionFilter("hr:kpi:list")]
        public async Task<IActionResult> GetKpiTemplates([FromQuery] string category)
        {
            var templates = await _kpiService.GetKpiTemplatesAsync(category);
            return SUCCESS(templates);
        }

        /// <summary>
        /// Create KPI from template
        /// </summary>
        [HttpPost("templates/{templateId}/create")]
        [ActionPermissionFilter("hr:kpi:add")]
        [Log(Title = "Create KPI from Template", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateKpiFromTemplate(long templateId, [FromBody] CreateFromTemplateDto dto)
        {
            var result = await _kpiService.CreateKpiFromTemplateAsync(templateId, dto.EmployeeId);
            return SUCCESS(result);
        }

        #endregion
    }
}
