 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Workflow Template Query DTO
    /// </summary>
    public class WorkflowTemplateQueryDto : PagerInfo
    {
        public string WorkflowName { get; set; }
        public string WorkflowType { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Workflow Template DTO
    /// </summary>
    public class WorkflowTemplateDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TemplateId { get; set; }
        
        [Required(ErrorMessage = "Workflow name is required")]
        public string WorkflowName { get; set; }
        
        [Required(ErrorMessage = "Workflow type is required")]
        public string WorkflowType { get; set; } // leave, expense, loan, promotion, etc.
        
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string TriggerConditions { get; set; }
        public List<WorkflowStepDto> Steps { get; set; }
    }

    /// <summary>
    /// Workflow Step DTO
    /// </summary>
    public class WorkflowStepDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long StepId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TemplateId { get; set; }
        public int StepOrder { get; set; }
        
        [Required(ErrorMessage = "Step name is required")]
        public string StepName { get; set; }
        
        public string StepType { get; set; } // approval, notification, action
        public string ApproverRole { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? SpecificApproverId { get; set; }
        public int SLAHours { get; set; }
        public string EscalationTo { get; set; }
        public bool IsParallel { get; set; }
        public string Conditions { get; set; }
        public string Actions { get; set; }
    }

    /// <summary>
    /// Workflow Instance DTO
    /// </summary>
    public class WorkflowInstanceDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InstanceId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TemplateId { get; set; }
        public string WorkflowType { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InitiatorId { get; set; }
        public string InitiatorName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } // pending, in_progress, approved, rejected, cancelled
        public string CurrentStep { get; set; }
        public string RequestData { get; set; }
        public List<WorkflowHistoryDto> History { get; set; }
    }

    /// <summary>
    /// Workflow History DTO
    /// </summary>
    public class WorkflowHistoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InstanceId { get; set; }
        public int StepNumber { get; set; }
        public string StepName { get; set; }
        public string Action { get; set; } // approved, rejected, returned, escalated
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ActorId { get; set; }
        public string ActorName { get; set; }
        public DateTime ActionDate { get; set; }
        public string Comments { get; set; }
        public int TimeSpentHours { get; set; }
    }

    /// <summary>
    /// Pending Approval DTO
    /// </summary>
    public class PendingApprovalDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InstanceId { get; set; }
        public string WorkflowType { get; set; }
        public string RequestType { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequesterId { get; set; }
        public string RequesterName { get; set; }
        public string RequesterDepartment { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string Summary { get; set; }
        public string Priority { get; set; }
        public int DaysPending { get; set; }
        public string RequestDetails { get; set; }
        public List<WorkflowHistoryDto> ApprovalHistory { get; set; }
    }

    /// <summary>
    /// Approval Action DTO
    /// </summary>
    public class ApprovalActionDto
    {
        [Required(ErrorMessage = "Instance ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InstanceId { get; set; }
        
        [Required(ErrorMessage = "Action is required")]
        public string Action { get; set; } // approve, reject, return
        
        public string Comments { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DelegateTo { get; set; }
        public string Attachments { get; set; }
    }

    /// <summary>
    /// Workflow Dashboard DTO
    /// </summary>
    public class WorkflowDashboardDto
    {
        public int PendingApprovals { get; set; }
        public int MyRequests { get; set; }
        public int Delegated { get; set; }
        public int Overdue { get; set; }
        public List<PendingApprovalDto> RecentPending { get; set; }
        public List<WorkflowInstanceDto> MyRecentRequests { get; set; }
        public Dictionary<string, int> ApprovalsByType { get; set; }
        public Dictionary<string, decimal> AverageProcessingTime { get; set; }
    }

    /// <summary>
    /// Workflow Analytics DTO
    /// </summary>
    public class WorkflowAnalyticsDto
    {
        public int TotalWorkflows { get; set; }
        public int ActiveInstances { get; set; }
        public int CompletedThisMonth { get; set; }
        public decimal AverageCompletionTime { get; set; }
        public decimal ApprovalRate { get; set; }
        public Dictionary<string, int> InstancesByType { get; set; }
        public Dictionary<string, decimal> AverageTimeByType { get; set; }
        public List<BottleneckDto> Bottlenecks { get; set; }
        public List<TopApproversDto> TopApprovers { get; set; }
    }

    /// <summary>
    /// Bottleneck DTO
    /// </summary>
    public class BottleneckDto
    {
        public string StepName { get; set; }
        public string WorkflowType { get; set; }
        public decimal AverageDelayHours { get; set; }
        public int PendingCount { get; set; }
    }

    /// <summary>
    /// Top Approvers DTO
    /// </summary>
    public class TopApproversDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ApproverId { get; set; }
        public string ApproverName { get; set; }
        public int TotalApprovals { get; set; }
        public decimal AverageResponseTime { get; set; }
    }

    /// <summary>
    /// Onboarding Workflow DTO
    /// </summary>
    public class OnboardingWorkflowDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkflowId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Status { get; set; }
        public List<OnboardingTaskDto> Tasks { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string CurrentStep { get; set; }
        public DateTime? ExpectedCompletionDate { get; set; }
        public decimal Progress { get; set; }
    }

    /// <summary>
    /// Onboarding Task DTO
    /// </summary>
    public class OnboardingTaskDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }
        public string TaskName { get; set; }
        public string TaskCategory { get; set; }
        public string AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Description { get; set; }
        public bool Completed { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Leave Workflow DTO
    /// </summary>
    public class LeaveWorkflowDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkflowId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveRequestId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Days { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string CurrentApprover { get; set; }
        public List<WorkflowHistoryDto> ApprovalHistory { get; set; }
        public decimal AvailableBalance { get; set; }
        public List<ApprovalStepDto> ApprovalSteps { get; set; }
    }

    /// <summary>
    /// Performance Workflow DTO
    /// </summary>
    public class PerformanceWorkflowDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkflowId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string ReviewPeriod { get; set; }
        public string ReviewType { get; set; }
        public decimal? OverallRating { get; set; }
        public string Status { get; set; }
        public string CurrentStep { get; set; }
        public DateTime? DueDate { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public int TotalEmployees { get; set; }
        public int WorkflowsCreated { get; set; }
        public List<PerformanceStepDto> Steps { get; set; }
    }

    /// <summary>
    /// Performance Step DTO
    /// </summary>
    public class PerformanceStepDto
    {
        public string StepName { get; set; }
        public string StepType { get; set; }
        public string Responsible { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
    }

    /// <summary>
    /// Workflow Status DTO
    /// </summary>
    public class WorkflowStatusDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkflowId { get; set; }
        public string WorkflowType { get; set; }
        public string Status { get; set; }
        public string CurrentStep { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CurrentApproverId { get; set; }
        public string CurrentApproverName { get; set; }
        public DateTime? LastActionDate { get; set; }
        public int DaysPending { get; set; }
        public bool IsOverdue { get; set; }
        public string Priority { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EntityId { get; set; }
        public DateTime? DueDate { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal Progress { get; set; }
        public DateTime InitiatedDate { get; set; }
    }

    /// <summary>
    /// Notification Template DTO
    /// </summary>
    public class NotificationTemplateDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TemplateId { get; set; }
        
        [Required(ErrorMessage = "Template name is required")]
        public string TemplateName { get; set; }
        
        [Required(ErrorMessage = "Notification type is required")]
        public string NotificationType { get; set; } // email, sms, push, in-app
        
        [Required(ErrorMessage = "Event trigger is required")]
        public string EventTrigger { get; set; }
        
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Recipients { get; set; } // role-based or specific users
        public bool IsActive { get; set; }
        public string Variables { get; set; } // JSON of available variables
    }
}

