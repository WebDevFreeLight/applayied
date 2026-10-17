using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Training;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Training Service Interface
    /// </summary>
    public interface ITrainingService
    {
        // Training Course Management
        Task<PagedInfo<HrTrainingCourse>> GetCoursesAsync(Expression<Func<HrTrainingCourse, bool>> predicate, PagerInfo pager);
        Task<List<HrTrainingCourse>> GetActiveCoursesAsync();
        Task<HrTrainingCourse> GetCourseByIdAsync(long courseId);
        Task<int> CreateCourseAsync(HrTrainingCourse course);
        Task<int> UpdateCourseAsync(HrTrainingCourse course);
        Task<int> DeleteCourseAsync(long courseId);
        Task<List<HrTrainingCourse>> GetCoursesByCategoryAsync(string category);
        Task<List<HrTrainingCourse>> GetMandatoryCoursesAsync();
        
        // Training Session Management
        Task<PagedInfo<HrTrainingSession>> GetSessionsAsync(Expression<Func<HrTrainingSession, bool>> predicate, PagerInfo pager);
        Task<List<HrTrainingSession>> GetUpcomingSessionsAsync();
        Task<HrTrainingSession> GetSessionByIdAsync(long sessionId);
        Task<int> ScheduleSessionAsync(HrTrainingSession session);
        Task<int> UpdateSessionAsync(HrTrainingSession session);
        Task<int> CancelSessionAsync(long sessionId, string reason);
        Task<List<HrTrainingSession>> GetSessionsByCourseAsync(long courseId);
        
        // Training Enrollment
        Task<List<HrTrainingEnrollment>> GetEnrollmentsAsync(long sessionId);
        Task<List<HrTrainingEnrollment>> GetEmployeeEnrollmentsAsync(long employeeId);
        Task<int> EnrollEmployeeAsync(HrTrainingEnrollment enrollment);
        Task<int> UpdateEnrollmentAsync(HrTrainingEnrollment enrollment);
        Task<int> CancelEnrollmentAsync(long enrollmentId, string reason);
        Task<int> CompleteEnrollmentAsync(long enrollmentId, decimal? score);
        Task<int> BulkEnrollAsync(long sessionId, List<long> employeeIds);
        Task<bool> CheckEnrollmentEligibilityAsync(long employeeId, long courseId);
        
        // Training Progress
        Task<List<HrTrainingProgress>> GetProgressAsync(long enrollmentId);
        Task<int> UpdateProgressAsync(HrTrainingProgress progress);
        Task<decimal> GetOverallProgressAsync(long enrollmentId);
        Task<int> RecordModuleCompletionAsync(long enrollmentId, string moduleName, decimal score);
        
        // Training Budget
        Task<HrTrainingBudget> GetBudgetAsync(long? departmentId, int year);
        Task<int> AllocateBudgetAsync(HrTrainingBudget budget);
        Task<int> UpdateBudgetUsageAsync(long budgetId, decimal amount);
        Task<decimal> GetRemainingBudgetAsync(long? departmentId, int year);
        Task<object> GetBudgetUtilizationAsync(int year);
        
        // Development Plans
        Task<List<HrDevelopmentPlan>> GetDevelopmentPlansAsync(long employeeId);
        Task<HrDevelopmentPlan> GetDevelopmentPlanByIdAsync(long planId);
        Task<int> CreateDevelopmentPlanAsync(HrDevelopmentPlan plan);
        Task<int> UpdateDevelopmentPlanAsync(HrDevelopmentPlan plan);
        Task<int> UpdatePlanProgressAsync(long planId, decimal progress);
        Task<int> ProvideMentorFeedbackAsync(long planId, string feedback);
        Task<List<HrDevelopmentPlan>> GetPlansForMentorAsync(string mentorId);
        
        // Training Analytics
        Task<object> GetTrainingMetricsAsync(DateTime startDate, DateTime endDate);
        Task<object> GetEmployeeTrainingHistoryAsync(long employeeId);
        Task<object> GetCourseEffectivenessAsync(long courseId);
        Task<object> GetTrainingCompletionRatesAsync(DateTime startDate, DateTime endDate);
        Task<object> GetSkillGapAnalysisAsync(long? departmentId);
        Task<object> GetTrainingROIAsync(long courseId);
        Task<List<object>> GetTopPerformingTraineesAsync(long sessionId);
        
        // Certification Management
        Task<int> IssueCertificateAsync(long enrollmentId);
        Task<object> GetCertificateDetailsAsync(string certificateNumber);
        Task<List<object>> GetExpiringCertificatesAsync(int daysAhead);
        Task<int> RenewCertificationAsync(long enrollmentId);
        
        // Training Feedback
        Task<int> SubmitTrainingFeedbackAsync(long enrollmentId, string feedback, int rating);
        Task<object> GetCourseFeedbackAsync(long courseId);
        Task<object> GetInstructorRatingsAsync(string instructor);
        
        // Training Reports
        Task<byte[]> GenerateTrainingReportAsync(long employeeId, DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateSessionReportAsync(long sessionId);
        Task<byte[]> GenerateCertificateAsync(long enrollmentId);
        Task<byte[]> ExportTrainingDataAsync(Expression<Func<HrTrainingCourse, bool>> predicate, string format);
        
        // Training Recommendations
        Task<List<HrTrainingCourse>> GetRecommendedCoursesAsync(long employeeId);
        Task<List<HrTrainingCourse>> GetCareerPathCoursesAsync(long employeeId);
        Task<List<HrTrainingCourse>> GetSkillBasedRecommendationsAsync(long employeeId);
    }
}
