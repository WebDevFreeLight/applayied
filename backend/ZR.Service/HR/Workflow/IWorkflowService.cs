using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Dto;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Workflow Service Interface
    /// </summary>
    public interface IWorkflowService
    {
        // Onboarding Workflow
        Task<OnboardingWorkflowDto> InitiateOnboardingWorkflowAsync(long employeeId);
        Task<OnboardingWorkflowDto> GetOnboardingWorkflowAsync(long workflowId);
        Task<List<OnboardingWorkflowDto>> GetActiveOnboardingWorkflowsAsync();
        Task<bool> UpdateOnboardingTaskAsync(long taskId, bool isCompleted, string comments);
        Task<bool> CompleteOnboardingStepAsync(long workflowId, string stepName);
        Task<byte[]> GenerateOnboardingChecklistAsync(long employeeId);

        // Leave Workflow
        Task<LeaveWorkflowDto> InitiateLeaveWorkflowAsync(long leaveRequestId);
        Task<LeaveWorkflowDto> GetLeaveWorkflowAsync(long workflowId);
        Task<List<LeaveWorkflowDto>> GetPendingLeaveWorkflowsAsync(long approverId);
        Task<bool> ApproveLeaveWorkflowAsync(long workflowId, long approverId, string comments);
        Task<bool> RejectLeaveWorkflowAsync(long workflowId, long approverId, string reason);
        Task<bool> EscalateLeaveWorkflowAsync(long workflowId, string reason);
        Task<List<LeaveWorkflowDto>> GetLeaveWorkflowHistoryAsync(long employeeId, DateTime? startDate, DateTime? endDate);

        // Performance Workflow
        Task<PerformanceWorkflowDto> InitiatePerformanceReviewAsync(long employeeId, string reviewType);
        Task<PerformanceWorkflowDto> GetPerformanceWorkflowAsync(long workflowId);
        Task<List<PerformanceWorkflowDto>> GetActivePerformanceWorkflowsAsync(long managerId);
        Task<bool> SubmitSelfAssessmentAsync(long workflowId, Dictionary<string, object> assessmentData);
        Task<bool> SubmitManagerReviewAsync(long workflowId, long managerId, Dictionary<string, object> reviewData);
        Task<bool> Submit360FeedbackAsync(long workflowId, long reviewerId, Dictionary<string, object> feedbackData);
        Task<bool> FinalizePerformanceReviewAsync(long workflowId, decimal finalRating, string comments);
        Task<List<PerformanceWorkflowDto>> GetPerformanceWorkflowsByPeriodAsync(string period);

        // General Workflow Operations
        Task<WorkflowStatusDto> GetWorkflowStatusAsync(long workflowId);
        Task<List<WorkflowStatusDto>> GetMyWorkflowsAsync(long employeeId);
        Task<List<WorkflowStatusDto>> GetPendingApprovalsAsync(long approverId);
        Task<bool> DelegateWorkflowAsync(long workflowId, long fromUserId, long toUserId, string reason);
        Task<bool> CancelWorkflowAsync(long workflowId, string reason);
        Task<List<WorkflowHistoryDto>> GetWorkflowHistoryAsync(long workflowId);

        // Workflow Templates
        Task<List<WorkflowTemplateDto>> GetWorkflowTemplatesAsync(string workflowType = null);
        Task<WorkflowTemplateDto> GetWorkflowTemplateAsync(long templateId);
        Task<long> CreateWorkflowTemplateAsync(WorkflowTemplateDto template);
        Task<bool> UpdateWorkflowTemplateAsync(WorkflowTemplateDto template);
        Task<bool> DeleteWorkflowTemplateAsync(long templateId);
        Task<bool> ActivateWorkflowTemplateAsync(long templateId);
        Task<bool> DeactivateWorkflowTemplateAsync(long templateId);

        // Analytics and Reporting
        Task<WorkflowAnalyticsDto> GetWorkflowAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<List<BottleneckDto>> IdentifyBottlenecksAsync(string workflowType = null);
        Task<List<TopApproversDto>> GetTopApproversAsync(int top = 10);
        Task<byte[]> GenerateWorkflowReportAsync(string workflowType, DateTime startDate, DateTime endDate);

        // Notifications
        Task<bool> SendWorkflowNotificationAsync(long workflowId, string notificationType, long recipientId);
        Task<bool> SendReminderAsync(long workflowId);
        Task<List<NotificationTemplateDto>> GetNotificationTemplatesAsync(string eventTrigger = null);
        Task<bool> UpdateNotificationTemplateAsync(NotificationTemplateDto template);
    }
}
