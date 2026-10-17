using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;
using SqlSugar;
using Mapster;
using ZR.Model.HR.Performance;

namespace ZR.Admin.WebApi.Controllers.HR.Performance
{
    /// <summary>
    /// Performance Management Controller
    /// </summary>
    [Verify]
    [Route("api/hr/performance")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class PerformanceController : BaseController
    {
        private readonly IPerformanceService _performanceService;

        public PerformanceController(IPerformanceService performanceService)
        {
            _performanceService = performanceService;
        }

        /// <summary>
        /// Get performance review list
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:performance:list")]
        public async Task<IActionResult> GetPerformanceList([FromQuery] PerformanceQueryDto query)
        {
            var predicate = Expressionable.Create<HrPerformanceReview>();
            
            if (query.EmployeeId.HasValue)
                predicate = predicate.And(p => p.EmployeeId == query.EmployeeId.Value);
            
            if (!string.IsNullOrEmpty(query.ReviewPeriod))
                predicate = predicate.And(p => p.ReviewPeriod == query.ReviewPeriod);
            
            if (!string.IsNullOrEmpty(query.Status))
                predicate = predicate.And(p => p.Status == query.Status);

            var result = await _performanceService.GetPageAsync(predicate.ToExpression(), query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get performance review by ID
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter("hr:performance:query")]
        public async Task<IActionResult> GetPerformanceReview(long id)
        {
            var result = await _performanceService.GetByIdAsync(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Performance review not found");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create performance review
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter("hr:performance:add")]
        [Log(Title = "Performance Review", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreatePerformanceReview([FromBody] PerformanceReviewDto dto)
        {
            var review = dto.Adapt<HrPerformanceReview>();
            review.Create_by = HttpContext.GetName();
            review.Create_time = DateTime.Now;
            review.Status = "draft";
            
            var result = await _performanceService.AddAsync(review);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update performance review
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter("hr:performance:edit")]
        [Log(Title = "Performance Review", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdatePerformanceReview([FromBody] PerformanceReviewDto dto)
        {
            var review = dto.Adapt<HrPerformanceReview>();
            review.Update_by = HttpContext.GetName();
            review.Update_time = DateTime.Now;
            
            var result = await _performanceService.UpdateAsync(review);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete performance review
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter("hr:performance:delete")]
        [Log(Title = "Performance Review", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeletePerformanceReview(string ids)
        {
            var idArray = Tools.SplitAndConvert<long>(ids);
            var result = await _performanceService.DeleteAsync(idArray);
            return SUCCESS(result);
        }

        /// <summary>
        /// Submit performance review
        /// </summary>
        [HttpPost("{reviewId}/submit")]
        [ActionPermissionFilter("hr:performance:submit")]
        [Log(Title = "Submit Performance Review", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> SubmitReview(long reviewId)
        {
            var result = await _performanceService.SubmitReviewAsync(reviewId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Approve performance review
        /// </summary>
        [HttpPost("{reviewId}/approve")]
        [ActionPermissionFilter("hr:performance:approve")]
        [Log(Title = "Approve Performance Review", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApproveReview(long reviewId, [FromBody] ApprovalDto approval)
        {
            var approvedBy = HttpContext.GetName();
            var result = await _performanceService.ApproveReviewAsync(reviewId, approval.Approved, approval.Comments, approvedBy);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get employee performance history
        /// </summary>
        [HttpGet("employee/{employeeId}/history")]
        [ActionPermissionFilter("hr:performance:view")]
        public async Task<IActionResult> GetEmployeePerformanceHistory(long employeeId)
        {
            var result = await _performanceService.GetEmployeeHistoryAsync(employeeId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get performance goals
        /// </summary>
        [HttpGet("goals/{employeeId}")]
        [ActionPermissionFilter("hr:performance:view")]
        public async Task<IActionResult> GetPerformanceGoals(long employeeId)
        {
            var result = await _performanceService.GetEmployeeGoalsAsync(employeeId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Set performance goals
        /// </summary>
        [HttpPost("goals")]
        [ActionPermissionFilter("hr:performance:goals")]
        [Log(Title = "Set Performance Goals", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SetPerformanceGoals([FromBody] PerformanceGoalDto dto)
        {
            var goal = dto.Adapt<HrPerformanceReviewGoal>();
            goal.Create_by = HttpContext.GetName();
            goal.Create_time = DateTime.Now;
            
            var result = await _performanceService.SetGoalAsync(goal);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update goal progress
        /// </summary>
        [HttpPut("goals/{goalId}/progress")]
        [ActionPermissionFilter("hr:performance:goals")]
        [Log(Title = "Update Goal Progress", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateGoalProgress(long goalId, [FromBody] GoalProgressDto progress)
        {
            var result = await _performanceService.UpdateGoalProgressAsync(goalId, progress.Progress, progress.Notes);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get performance metrics
        /// </summary>
        [HttpGet("metrics")]
        [ActionPermissionFilter("hr:performance:view")]
        public async Task<IActionResult> GetPerformanceMetrics([FromQuery] long employeeId, [FromQuery] string period)
        {
            var result = await _performanceService.GetPerformanceMetricsAsync(employeeId, period);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get team performance
        /// </summary>
        [HttpGet("team/{managerId}")]
        [ActionPermissionFilter("hr:performance:view")]
        public async Task<IActionResult> GetTeamPerformance(long managerId)
        {
            var result = await _performanceService.GetTeamPerformanceAsync(managerId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Export performance report
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter("hr:performance:export")]
        [Log(Title = "Export Performance Report", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportPerformanceReport([FromQuery] PerformanceQueryDto query)
        {
            var predicate = Expressionable.Create<HrPerformanceReview>();
            
            if (query.EmployeeId.HasValue)
                predicate = predicate.And(p => p.EmployeeId == query.EmployeeId.Value);
            
            var list = await _performanceService.GetListAsync(predicate.ToExpression());
            var result = ExportExcelMini(list, "performance", "Performance Report");
            return ExportExcel(result.Item2, result.Item1);
        }
    }

    public class ApprovalDto
    {
        public bool Approved { get; set; }
        public string Comments { get; set; }
    }

    public class GoalProgressDto
    {
        public decimal Progress { get; set; }
        public string Notes { get; set; }
    }
}
