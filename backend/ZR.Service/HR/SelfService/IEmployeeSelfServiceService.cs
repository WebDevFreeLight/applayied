using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR;
using ZR.Model.HR.Attendance;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.Payroll;
using ZR.Model.HR.Performance;
using ZR.Model.HR.Training;
using ZR.Model.HR.Dto.EmployeeCore;

namespace ZR.Service.HR.IHrService
{
    /// <summary>
    /// Employee Self-Service Interface
    /// </summary>
    public interface IEmployeeSelfServiceService
    {
        // Profile Management
        Task<EmployeeApiDto> GetMyProfileAsync(long employeeId);
        Task<int> UpdateMyProfileAsync(long employeeId, HrEmployeeProfile profile);
        Task<int> UpdateEmergencyContactAsync(long employeeId, HrEmergencyContact contact);
        Task<List<HrEmergencyContact>> GetEmergencyContactsAsync(long employeeId);

        // Leave Management
        Task<List<HrLeaveBalance>> GetMyLeaveBalanceAsync(long employeeId);
        Task<List<HrLeaveRequest>> GetMyLeaveRequestsAsync(long employeeId, DateTime? startDate = null, DateTime? endDate = null);
        Task<int> SubmitLeaveRequestAsync(HrLeaveRequest request);
        Task<int> CancelLeaveRequestAsync(long employeeId, long leaveId);

        // Attendance Management
        Task<List<ZR.Model.HR.Dto.Attendance.HrAttendanceDayDto>> GetMyAttendanceAsync(long employeeId, DateTime startDate, DateTime endDate);
        Task<HrAttendanceSummary> GetMyAttendanceSummaryAsync(long employeeId, int year, int month);
        Task<int> CheckInAsync(long employeeId, string location = null);
        Task<int> CheckOutAsync(long employeeId, string location = null);

        // Payroll Management
        Task<List<HrPayrollDetail>> GetMyPayslipsAsync(long employeeId, int year);
        Task<HrPayrollDetail> GetPayslipDetailAsync(long employeeId, long payrollId);
        Task<HrCompensationSummary> GetCompensationSummaryAsync(long employeeId);

        // Document Management
        Task<List<object>> GetMyDocumentsAsync(long employeeId);
        Task<int> UploadDocumentAsync(object document);
        Task<object> DownloadDocumentAsync(long employeeId, long documentId);

        // Training & Development
        Task<List<HrTrainingEnrollment>> GetMyTrainingsAsync(long employeeId);
        Task<List<HrTrainingCourse>> GetAvailableCoursesAsync(long employeeId);
        Task<int> EnrollInTrainingAsync(long employeeId, long sessionId);
        Task<HrSkillsMatrix> GetMySkillsAsync(long employeeId);
        Task<int> UpdateMySkillsAsync(long employeeId, List<HrSkillsMatrix> skills);

        // Performance Management
        Task<List<HrPerformanceReview>> GetMyPerformanceReviewsAsync(long employeeId);
        Task<List<HrPerformanceGoal>> GetMyGoalsAsync(long employeeId);
        Task<int> UpdateGoalProgressAsync(long employeeId, long goalId, decimal progress, string notes);
        Task<int> SubmitSelfReviewAsync(HrPerformanceReview review);

        // Team Management (for managers)
        Task<List<EmployeeListDto>> GetMyTeamAsync(long managerId);
        Task<List<HrLeaveRequest>> GetTeamLeaveRequestsAsync(long managerId);
        Task<HrTeamAttendanceSummary> GetTeamAttendanceSummaryAsync(long managerId, DateTime date);

        // Notifications
        Task<List<HrNotification>> GetMyNotificationsAsync(long employeeId, bool unreadOnly = false);
        Task<int> MarkNotificationAsReadAsync(long employeeId, long notificationId);
        
        // Additional Methods for Controllers
        Task<int> UploadMyDocumentAsync(long employeeId, object request);
        Task<object> GetMyBenefitsAsync(long employeeId);
        Task<object> GetMyTrainingHistoryAsync(long employeeId);
        Task<int> SubmitTimeOffRequestAsync(object request);
    }
}
