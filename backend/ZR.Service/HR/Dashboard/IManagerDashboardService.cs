using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Performance;
using ZR.Model.HR.EmployeeCore;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR.IHrService
{
    /// <summary>
    /// Manager Dashboard Service Interface
    /// </summary>
    public interface IManagerDashboardService
    {
        // Dashboard Overview
        Task<HrManagerDashboard> GetDashboardOverviewAsync(long managerId);

        // Team Management
        Task<List<EmployeeFullDto>> GetTeamMembersAsync(long managerId, bool includeIndirect = false);
        Task<HrTeamStatistics> GetTeamStatisticsAsync(long managerId);
        Task<List<HrEmployeeDetail>> GetTeamDetailsAsync(long managerId);

        // Attendance Management
        Task<HrTeamAttendanceSummary> GetTeamAttendanceTodayAsync(long managerId);
        Task<List<HrAttendanceReport>> GetTeamAttendanceReportAsync(long managerId, DateTime startDate, DateTime endDate);

        // Leave Management
        Task<List<HrLeaveRequest>> GetPendingLeaveRequestsAsync(long managerId);
        Task<int> ApproveLeaveRequestAsync(long managerId, long leaveId, string comments = null);
        Task<int> RejectLeaveRequestAsync(long managerId, long leaveId, string reason);
        Task<HrLeaveAnalytics> GetTeamLeaveAnalyticsAsync(long managerId, int year);

        // Performance Management
        Task<HrTeamPerformanceMetrics> GetTeamPerformanceMetricsAsync(long managerId);
        Task<List<HrPerformanceReview>> GetPendingPerformanceReviewsAsync(long managerId);
        Task<int> SubmitPerformanceReviewAsync(long managerId, HrPerformanceReview review);
        Task<int> SetTeamGoalsAsync(long managerId, List<HrPerformanceGoal> goals);

        // Expense Management
        Task<List<HrExpenseRequest>> GetPendingExpenseRequestsAsync(long managerId);
        Task<int> ApproveExpenseRequestAsync(long managerId, long expenseId, string comments = null);

        // Analytics and Reports
        Task<HrManagerReport> GenerateTeamReportAsync(long managerId, string reportType, DateTime startDate, DateTime endDate);

        // Notifications and Alerts
        Task<List<HrManagerAlert>> GenerateManagerAlertsAsync(long managerId);
        
        // Additional Methods for Controllers
        Task<object> GetTeamPerformanceMetricsAsync(long managerId, int year);
        Task<object> GetTeamLeaveSummaryAsync(long managerId, int year);
        Task<object> GetTeamTrainingStatusAsync(long managerId);
        Task<bool> ApproveRequestAsync(long managerId, string requestType, long requestId, string comments);
        Task<bool> RejectRequestAsync(long managerId, string requestType, long requestId, string reason);
        Task<int> ScheduleTeamMeetingAsync(object request);
        Task<int> AssignTaskAsync(object request);
        Task<object> GetTeamWorkloadAsync(long managerId);
        Task<int> SubmitPerformanceReviewAsync(object request);
    }
}
