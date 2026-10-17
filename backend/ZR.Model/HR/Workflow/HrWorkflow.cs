 
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR
{
    /// <summary>
    /// Workflow Table
    /// </summary>
    [SugarTable("hr_workflow")]
    
    [SqlDict(
        dictType: "workflow",
        dictName: "Hr Workflow",
        valueColumn: "WorkflowId",
        labelColumns: "workflowtype",
        RemarkColumn = "notes",
        OrderBy = "WorkflowId ASC",
        Description = "Auto-generated from HrWorkflow table"
    )]
    public class HrWorkflow : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkflowId { get; set; }

        [SugarColumn(Length = 50)]
        public string WorkflowType { get; set; } // onboarding, leave, performance, termination

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EntityId { get; set; } // Reference to employee, leave request, etc.

        [SugarColumn(Length = 50)]
        public string EntityType { get; set; } // employee, leave_request, performance_review

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // initiated, in_progress, completed, cancelled

        [SugarColumn(Length = 100)]
        public string CurrentStep { get; set; }

        public int CurrentStepNumber { get; set; } = 1;

        public int TotalSteps { get; set; }

        [SugarColumn(Length = 50)]
        public string InitiatedBy { get; set; }

        public DateTime InitiatedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [SugarColumn(Length = 500)]
        public string Notes { get; set; }

        
        public string WorkflowData { get; set; } // JSON data for workflow state

        public DateTime? DueDate { get; set; }

    }

    /// <summary>
    /// Workflow Task Table
    /// </summary>
    [SugarTable("hr_workflow_task")]
    public class HrWorkflowTask : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkflowId { get; set; }

        [SugarColumn(Length = 200)]
        public string TaskName { get; set; }

        [SugarColumn(Length = 50)]
        public string TaskType { get; set; } // approval, verification, system, manual

        [SugarColumn(Length = 100)]
        public string AssignedTo { get; set; } // Role or specific user

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? AssignedUserId { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // pending, in_progress, completed, skipped

        [SugarColumn(Length = 20)]
        public string Priority { get; set; } // high, medium, low

        public int Sequence { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool IsParallel { get; set; } = false;

        [SugarColumn(Length = 500)]
        public string Comments { get; set; }

        [SugarColumn(Length = 500)]
        public string AttachmentUrl { get; set; }

        [SugarColumn(Length = 50)]
        public string CompletedBy { get; set; }
        public string Notes { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AssignedToId { get; set; }
    }

    /// <summary>
    /// Workflow Template Table
    /// </summary>
    [SugarTable("hr_workflow_template")]
    public class HrWorkflowTemplate : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TemplateId { get; set; }

        [SugarColumn(Length = 100)]
        public string TemplateName { get; set; }

        [SugarColumn(Length = 50)]
        public string WorkflowType { get; set; }

        [SugarColumn(Length = 500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        
        public string TemplateData { get; set; } // JSON template configuration

        
        public string TriggerConditions { get; set; } // JSON conditions

        public int Version { get; set; } = 1;

        
    }

    /// <summary>
    /// Workflow History Table
    /// </summary>
    [SugarTable("hr_workflow_history")]
    public class HrWorkflowHistory : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkflowId { get; set; }

        [SugarColumn(Length = 100)]
        public string StepName { get; set; }

        [SugarColumn(Length = 50)]
        public string Action { get; set; } // approved, rejected, completed, escalated

        [SugarColumn(Length = 50)]
        public string ActionBy { get; set; }

        public DateTime ActionDate { get; set; }

        [SugarColumn(Length = 1000)]
        public string Comments { get; set; }

        
        public string BeforeData { get; set; } // JSON before state

        
        public string AfterData { get; set; } // JSON after state

        public int TimeSpentMinutes { get; set; }

     
    }

    /// <summary>
    /// Workflow Notification Table
    /// </summary>
    [SugarTable("hr_workflow_notification")]
    public class HrWorkflowNotification : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long NotificationId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkflowId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long RecipientId { get; set; }

        [SugarColumn(Length = 50)]
        public string NotificationType { get; set; } // email, sms, push, in_app

        [SugarColumn(Length = 200)]
        public string Subject { get; set; }

        
        public string Body { get; set; }

        public DateTime ScheduledDate { get; set; }

        public DateTime? SentDate { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // pending, sent, failed

        public int RetryCount { get; set; } = 0;

        public bool IsEscalated { get; set; } = false;

        public DateTime? EscalationDate { get; set; }

        [SugarColumn(Length = 500)]
        public string ErrorMessage { get; set; }

    
    }

    /// <summary>
    /// Workflow Approval Table
    /// </summary>
    [SugarTable("hr_workflow_approval")]
    public class HrWorkflowApproval : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ApprovalId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long WorkflowId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ApproverId { get; set; }

        [SugarColumn(Length = 50)]
        public string ApproverRole { get; set; }

        public int ApprovalLevel { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // pending, approved, rejected, skipped

        public DateTime? ApprovalDate { get; set; }

        [SugarColumn(Length = 500)]
        public string Comments { get; set; }

        public bool IsEscalated { get; set; } = false;

        public DateTime? EscalationDate { get; set; }

  
    }
}

