using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Performance;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR.IHrService
{
    /// <summary>
    /// HR Performance Service Interface
    /// </summary>
    public interface IHrPerformanceService
    {
        // Performance Review Management
        Task<List<HrPerformanceReview>> GetEmployeeReviewsAsync(long employeeId);
        Task<HrPerformanceReview> GetReviewByIdAsync(long reviewId);
        Task<int> CreateReviewAsync(HrPerformanceReview review);
        Task<bool> UpdateReviewAsync(HrPerformanceReview review);
        Task<bool> DeleteReviewAsync(long reviewId);
        Task<int> SubmitPerformanceReviewAsync(HrPerformanceReview review);
        Task<bool> ApproveReviewAsync(long reviewId, long approvedBy);
        Task<List<HrPerformanceReview>> GetPendingReviewsAsync(long managerId);
        Task<List<HrPerformanceReview>> GetTeamReviewsAsync(long managerId);
        
        // Goal Management
        Task<List<HrPerformanceGoal>> GetEmployeeGoalsAsync(long employeeId);
        Task<HrPerformanceGoal> GetGoalByIdAsync(long goalId);
        Task<int> CreateGoalAsync(HrPerformanceGoal goal);
        Task<bool> UpdateGoalAsync(HrPerformanceGoal goal);
        Task<bool> DeleteGoalAsync(long goalId);
        Task<bool> UpdateGoalProgressAsync(long goalId, decimal progress, string notes);
        Task<bool> CompleteGoalAsync(long goalId, string completionNotes);
        Task<List<HrPerformanceGoal>> GetTeamGoalsAsync(long managerId);
        
        // KPI Management
        Task<List<HrKpi>> GetKpisAsync(long employeeId);
        Task<bool> UpdateKpiAsync(HrKpi kpi);
        Task<decimal> CalculateKpiScoreAsync(long employeeId, DateTime period);
        
        // Performance Improvement Plan
        Task<int> CreateImprovementPlanAsync(HrPerformanceImprovementPlan plan);
        Task<bool> UpdateImprovementPlanAsync(HrPerformanceImprovementPlan plan);
        Task<List<HrPerformanceImprovementPlan>> GetImprovementPlansAsync(long employeeId);
        
        // 360 Feedback
        Task<int> Submit360FeedbackAsync(Hr360Feedback feedback);
        Task<List<Hr360Feedback>> Get360FeedbackAsync(long employeeId, long reviewId);
        
        // Metrics and Analytics
        Task<decimal> GetAverageRatingAsync(long employeeId, int year);
        Task<List<HrEmployeeCore>> GetTopPerformersAsync(int count, int year);
        Task<List<HrEmployeeCore>> GetUnderperformersAsync(decimal threshold, int year);
        Task<Dictionary<string, decimal>> GetDepartmentPerformanceAsync(long departmentId, int year);
        
        // Reports
        Task<byte[]> GeneratePerformanceReportAsync(long employeeId, int year);
        Task<byte[]> GenerateTeamPerformanceReportAsync(long managerId, int year);
    }
}
