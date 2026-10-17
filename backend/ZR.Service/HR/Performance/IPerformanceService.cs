using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Performance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Performance Management Service Interface
    /// </summary>
    public interface IPerformanceService
    {
        // Base CRUD operations
        Task<PagedInfo<HrPerformanceReview>> GetPageAsync(Expression<Func<HrPerformanceReview, bool>> predicate, PagerInfo pager);
        Task<HrPerformanceReview> GetByIdAsync(long id);
        Task<int> AddAsync(HrPerformanceReview review);
        Task<int> UpdateAsync(HrPerformanceReview review);
        Task<int> DeleteAsync(long[] ids);
        Task<List<HrPerformanceReview>> GetListAsync(Expression<Func<HrPerformanceReview, bool>> predicate);
        
        // Performance Reviews
        Task<PagedInfo<PerformanceReviewDto>> GetReviewsAsync(PerformanceQueryDto query);
        Task<PerformanceReviewDto> GetReviewByIdAsync(long reviewId);
        Task<PerformanceReviewDto> CreateReviewAsync(PerformanceReviewDto review);
        Task<bool> UpdateReviewAsync(PerformanceReviewDto review);
        Task<bool> SubmitReviewAsync(long reviewId);
        Task<bool> ApproveReviewAsync(long reviewId, bool approved, string comments, string approvedBy);
        Task<bool> SubmitReviewAsync(long reviewId, string submittedBy);
        Task<bool> ApproveReviewAsync(long reviewId, string approvedBy, string comments);
        Task<bool> RejectReviewAsync(long reviewId, string rejectedBy, string reason);
        Task<List<HrPerformanceReview>> GetEmployeeHistoryAsync(long employeeId);
        
        // Goals Management
        Task<List<HrPerformanceReviewGoal>> GetEmployeeGoalsAsync(long employeeId);
        Task<List<PerformanceGoalDto>> GetEmployeeGoalsAsync(long employeeId, int? year);
        Task<int> SetGoalAsync(HrPerformanceReviewGoal goal);
        Task<PerformanceGoalDto> CreateGoalAsync(PerformanceGoalDto goal);
        Task<bool> UpdateGoalAsync(PerformanceGoalDto goal);
        Task<bool> UpdateGoalProgressAsync(long goalId, decimal progress, string notes);
        Task<bool> CompleteGoalAsync(long goalId, decimal achievementScore, string comments);
        
        // KPI Management
        Task<List<KpiMetricDto>> GetKpiMetricsAsync(long? departmentId = null, long? positionId = null);
        Task<KpiMetricDto> CreateKpiMetricAsync(KpiMetricDto metric);
        Task<bool> UpdateKpiMetricAsync(KpiMetricDto metric);
        Task<bool> RecordKpiValueAsync(long employeeId, long metricId, decimal value, DateTime period);
        
        // 360 Feedback
        Task<List<FeedbackRequestDto>> GetPendingFeedbacksAsync(long reviewerId);
        Task<bool> SubmitFeedbackAsync(long requestId, FeedbackResponseDto feedback);
        Task<bool> Request360FeedbackAsync(long employeeId, List<long> reviewerIds, string reviewPeriod);
        
        // Performance Analytics
        Task<object> GetPerformanceMetricsAsync(long employeeId, string period);
        Task<object> GetTeamPerformanceAsync(long managerId);
        Task<PerformanceAnalyticsDto> GetPerformanceAnalyticsAsync(long employeeId, int year);
        Task<DepartmentPerformanceDto> GetDepartmentPerformanceAsync(long departmentId, int year, int? quarter = null);
        Task<List<TopPerformerDto>> GetTopPerformersAsync(int year, int? quarter = null, int topCount = 10);
        Task<PerformanceComparisonDto> ComparePerformanceAsync(List<long> employeeIds, int year);
        
        // Performance Improvement Plan (PIP)
        Task<List<PerformanceImprovementPlanDto>> GetActivePIPsAsync(long? employeeId = null);
        Task<PerformanceImprovementPlanDto> CreatePIPAsync(PerformanceImprovementPlanDto pip);
        Task<bool> UpdatePIPProgressAsync(long pipId, string progressUpdate, decimal completionPercentage);
        Task<bool> CompletePIPAsync(long pipId, bool successful, string finalComments);
        
        // Calibration Sessions
        Task<CalibrationSessionDto> CreateCalibrationSessionAsync(CalibrationSessionDto session);
        Task<bool> UpdateEmployeeRatingAsync(long sessionId, long employeeId, decimal rating, string justification);
        Task<bool> FinalizeCalibrationAsync(long sessionId, string finalizedBy);
        
        // Reports
        Task<byte[]> GeneratePerformanceReportAsync(long employeeId, int year);
        Task<byte[]> GenerateTeamPerformanceReportAsync(long managerId, int year, int? quarter = null);
        Task<ReviewCycleStatusDto> GetReviewCycleStatusAsync(int year, int? quarter = null);
    }
}
