using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model.HR.Performance;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Repository;
using ZR.Service.HR.IHrService;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    /// <summary>
    /// HR Performance Service Implementation
    /// </summary>
    [AppService(ServiceType = typeof(IHrPerformanceService), ServiceLifetime = LifeTime.Transient)]
    public class HrPerformanceService : BaseService<HrPerformanceReview>, IHrPerformanceService
    {
        private readonly ISqlSugarClient Context;
        private readonly IEmployeeDataService _employeeDataService;

        public HrPerformanceService(IEmployeeDataService employeeDataService)
        {
            Context = Context;
            _employeeDataService = employeeDataService;
        }

        #region Performance Review Management

        /// <summary>
        /// Get employee reviews
        /// </summary>
        public async Task<List<HrPerformanceReview>> GetEmployeeReviewsAsync(long employeeId)
        {
            return await Context.Queryable<HrPerformanceReview>()
                .Where(r => r.EmployeeId == employeeId)
                .OrderBy(r => r.ReviewDate, OrderByType.Desc)
                .ToListAsync();
        }

        /// <summary>
        /// Get review by ID
        /// </summary>
        public async Task<HrPerformanceReview> GetReviewByIdAsync(long reviewId)
        {
            return await Context.Queryable<HrPerformanceReview>()
                .FirstAsync(r => r.ReviewId == reviewId);
        }

        /// <summary>
        /// Create review
        /// </summary>
        public async Task<int> CreateReviewAsync(HrPerformanceReview review)
        {
            review.Status = "Draft";
            review.Create_time = DateTime.Now;
            return await Context.Insertable(review).ExecuteCommandAsync();
            return await Context.Insertable(review).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update review
        /// </summary>
        public async Task<bool> UpdateReviewAsync(HrPerformanceReview review)
        {
            review.Update_time = DateTime.Now;
            return await Context.Updateable(review).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Delete review
        /// </summary>
        public async Task<bool> DeleteReviewAsync(long reviewId)
        {
            return await Context.Deleteable<HrPerformanceReview>()
                .Where(r => r.ReviewId == reviewId)
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Submit performance review
        /// </summary>
        public async Task<int> SubmitPerformanceReviewAsync(HrPerformanceReview review)
        {
            review.Status = "Submitted";
            review.SubmittedDate = DateTime.Now;
            review.Create_time = DateTime.Now;
            return await Context.Insertable(review).ExecuteCommandAsync();
        }

        /// <summary>
        /// Approve review
        /// </summary>
        public async Task<bool> ApproveReviewAsync(long reviewId, long approvedBy)
        {
            var review = await GetReviewByIdAsync(reviewId);
            if (review == null) return false;

            review.Status = "Approved";
            review.ApprovedBy = approvedBy.ToString();
            review.ApprovedDate = DateTime.Now;
            review.Update_time = DateTime.Now;

            return await Context.Updateable(review).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Get pending reviews
        /// </summary>
        public async Task<List<HrPerformanceReview>> GetPendingReviewsAsync(long managerId)
        {
            // Get team members using EmployeeDataService
            var teamMembers = await _employeeDataService.GetTeamMembersAsync(managerId);
            var teamMemberIds = teamMembers.Select(e => e.EmployeeId).ToList();

            // Get pending reviews for team members
            return await Context.Queryable<HrPerformanceReview>()
                .Where(r => teamMemberIds.Contains(r.EmployeeId) && r.Status == "Submitted")
                .ToListAsync();
        }

        /// <summary>
        /// Get team reviews
        /// </summary>
        public async Task<List<HrPerformanceReview>> GetTeamReviewsAsync(long managerId)
        {
            // Get team members using EmployeeDataService
            var teamMembers = await _employeeDataService.GetTeamMembersAsync(managerId);
            var teamMemberIds = teamMembers.Select(e => e.EmployeeId).ToList();

            // Get all reviews for team members
            return await Context.Queryable<HrPerformanceReview>()
                .Where(r => teamMemberIds.Contains(r.EmployeeId))
                .ToListAsync();
        }

        #endregion

        #region Goal Management

        /// <summary>
        /// Get employee goals
        /// </summary>
        public async Task<List<HrPerformanceGoal>> GetEmployeeGoalsAsync(long employeeId)
        {
            return await Context.Queryable<HrPerformanceGoal>()
                .Where(g => g.EmployeeId == employeeId)
                .OrderBy(g => g.DueDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get goal by ID
        /// </summary>
        public async Task<HrPerformanceGoal> GetGoalByIdAsync(long goalId)
        {
            return await Context.Queryable<HrPerformanceGoal>()
                .FirstAsync(g => g.GoalId == goalId);
        }

        /// <summary>
        /// Create goal
        /// </summary>
        public async Task<int> CreateGoalAsync(HrPerformanceGoal goal)
        {
            goal.Status = "Active";
            goal.Progress = 0;
            goal.Create_time = DateTime.Now;
            return await Context.Insertable(goal).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update goal
        /// </summary>
        public async Task<bool> UpdateGoalAsync(HrPerformanceGoal goal)
        {
            goal.Update_time = DateTime.Now;
            return await Context.Updateable(goal).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Delete goal
        /// </summary>
        public async Task<bool> DeleteGoalAsync(long goalId)
        {
            return await Context.Deleteable<HrPerformanceGoal>()
                .Where(g => g.GoalId == goalId)
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Update goal progress
        /// </summary>
        public async Task<bool> UpdateGoalProgressAsync(long goalId, decimal progress, string notes)
        {
            var goal = await GetGoalByIdAsync(goalId);
            if (goal == null) return false;

            goal.Progress =(int) progress;
            goal.ProgressNotes = notes;
            goal.LastProgressUpdate = DateTime.Now;
            goal.Update_time = DateTime.Now;

            if (progress >= 100)
            {
                goal.Status = "Completed";
                goal.CompletedDate = DateTime.Now;
            }

            return await Context.Updateable(goal).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Complete goal
        /// </summary>
        public async Task<bool> CompleteGoalAsync(long goalId, string completionNotes)
        {
            var goal = await GetGoalByIdAsync(goalId);
            if (goal == null) return false;

            goal.Status = "Completed";
            goal.Progress = 100;
            goal.CompletionNotes = completionNotes;
            goal.CompletedDate = DateTime.Now;
            goal.Update_time = DateTime.Now;

            return await Context.Updateable(goal).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Get team goals
        /// </summary>
        public async Task<List<HrPerformanceGoal>> GetTeamGoalsAsync(long managerId)
        {
            // Get team members using EmployeeDataService
            var teamMembers = await _employeeDataService.GetTeamMembersAsync(managerId);
            var teamMemberIds = teamMembers.Select(e => e.EmployeeId).ToList();

            // Get all goals for team members
            return await Context.Queryable<HrPerformanceGoal>()
                .Where(g => teamMemberIds.Contains(g.EmployeeId))
                .ToListAsync();
        }

        #endregion

        #region KPI Management

        /// <summary>
        /// Get KPIs
        /// </summary>
        public async Task<List<HrKpi>> GetKpisAsync(long employeeId)
        {
            return await Context.Queryable<HrKpi>()
                .Where(k => k.EmployeeId == employeeId)
                .ToListAsync();
        }

        /// <summary>
        /// Update KPI
        /// </summary>
        public async Task<bool> UpdateKpiAsync(HrKpi kpi)
        {
            kpi.Update_time = DateTime.Now;
            return await Context.Updateable(kpi).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Calculate KPI score
        /// </summary>
        public async Task<decimal> CalculateKpiScoreAsync(long employeeId, DateTime period)
        {
            var kpis = await Context.Queryable<HrKpi>()
                .Where(k => k.EmployeeId == employeeId && 
                           k.PeriodStart <= period && 
                           k.PeriodEnd >= period)
                .ToListAsync();

            if (!kpis.Any()) return 0;

            return kpis.Average(k => (k.ActualValue / k.TargetValue) * k.Weight);
        }

        #endregion

        #region Performance Improvement Plan

        /// <summary>
        /// Create improvement plan
        /// </summary>
        public async Task<int> CreateImprovementPlanAsync(HrPerformanceImprovementPlan plan)
        {
            plan.Status = "Active";
            plan.Create_time = DateTime.Now;
            return await Context.Insertable(plan).ExecuteCommandAsync();
        }

        /// <summary>
        /// Update improvement plan
        /// </summary>
        public async Task<bool> UpdateImprovementPlanAsync(HrPerformanceImprovementPlan plan)
        {
            plan.Update_time = DateTime.Now;
            return await Context.Updateable(plan).ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Get improvement plans
        /// </summary>
        public async Task<List<HrPerformanceImprovementPlan>> GetImprovementPlansAsync(long employeeId)
        {
            return await Context.Queryable<HrPerformanceImprovementPlan>()
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync();
        }

        #endregion

        #region 360 Feedback

        /// <summary>
        /// Submit 360 feedback
        /// </summary>
        public async Task<int> Submit360FeedbackAsync(Hr360Feedback feedback)
        {
            feedback.SubmittedDate = DateTime.Now;
            feedback.Create_time = DateTime.Now;
            return await Context.Insertable(feedback).ExecuteCommandAsync();
        }

        /// <summary>
        /// Get 360 feedback
        /// </summary>
        public async Task<List<Hr360Feedback>> Get360FeedbackAsync(long employeeId, long reviewId)
        {
            return await Context.Queryable<Hr360Feedback>()
                .Where(f => f.EmployeeId == employeeId && f.ReviewId == reviewId)
                .ToListAsync();
        }

        #endregion

        #region Metrics and Analytics

        /// <summary>
        /// Get average rating
        /// </summary>
        public async Task<decimal> GetAverageRatingAsync(long employeeId, int year)
        {
            var reviews = await Context.Queryable<HrPerformanceReview>()
                .Where(r => r.EmployeeId == employeeId && r.ReviewDate.Value.Year == year)
                .ToListAsync();

            if (!reviews.Any()) return 0;

            return reviews.Average(r => r.OverallRating??0);
        }

        /// <summary>
        /// Get top performers
        /// </summary>
        public async Task<List<HrEmployeeCore>> GetTopPerformersAsync(int count, int year)
        {
            var topPerformers = await Context.Queryable<HrPerformanceReview, HrEmployeeCore>((r, e) => r.EmployeeId == e.EmployeeId)
                .Where((r, e) => r.ReviewDate.Value.Year == year)
                .GroupBy((r, e) => e.EmployeeId)
                .Select((r, e) => new { Employee = e, AvgRating = SqlFunc.AggregateAvg(r.OverallRating) })
                .OrderBy(x => x.AvgRating, OrderByType.Desc)
                .Take(count)
                .Select(x => x.Employee)
                .ToListAsync();

            return topPerformers;
        }

        /// <summary>
        /// Get underperformers
        /// </summary>
        public async Task<List<HrEmployeeCore>> GetUnderperformersAsync(decimal threshold, int year)
        {
            var underperformers = await Context.Queryable<HrPerformanceReview, HrEmployeeCore>((r, e) => r.EmployeeId == e.EmployeeId)
                .Where((r, e) => r.ReviewDate.Value.Year == year)
                .GroupBy((r, e) => e.EmployeeId)
                .Having((r, e) => SqlFunc.AggregateAvg(r.OverallRating) < threshold)
                .Select((r, e) => e)
                .ToListAsync();

            return underperformers;
        }

        /// <summary>
        /// Get department performance
        /// </summary>
        public async Task<Dictionary<string, decimal>> GetDepartmentPerformanceAsync(long departmentId, int year)
        {
            // Get employees by department using EmployeeDataService
            var departmentEmployees = await _employeeDataService.GetEmployeesByDepartmentAsync(departmentId);
            var employeeIds = departmentEmployees.Select(e => e.EmployeeId).ToList();

            // Get performance reviews for department employees
            var performance = await Context.Queryable<HrPerformanceReview>()
                .Where(r => employeeIds.Contains(r.EmployeeId) && r.ReviewDate.Value.Year == year)
                .Select(r => new { r.OverallRating, r.Category })
                .ToListAsync();

            var result = new Dictionary<string, decimal>();

            foreach (var category in performance.GroupBy(p => p.Category))
            {
                result[category.Key] = category.Average(p => p.OverallRating??0);
            }

            return result;
        }

        #endregion

        #region Reports

        /// <summary>
        /// Generate performance report
        /// </summary>
        public async Task<byte[]> GeneratePerformanceReportAsync(long employeeId, int year)
        {
            // Generate PDF performance report
            return await Task.FromResult(new byte[0]);
        }

        /// <summary>
        /// Generate team performance report
        /// </summary>
        public async Task<byte[]> GenerateTeamPerformanceReportAsync(long managerId, int year)
        {
            // Generate PDF team performance report
            return await Task.FromResult(new byte[0]);
        }

        #endregion
    }

    // Supporting classes
    // HrPerformanceReview is defined in HrPerformance.cs model file

  

  

  
}
