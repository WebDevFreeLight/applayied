
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Training
{
    /// <summary>
    /// Training Course Table
    /// </summary>
    [SugarTable("hr_training_course")]
    public class HrTrainingCourse : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CourseId { get; set; }

        [SugarColumn(Length = 100)]
        public string CourseCode { get; set; }

        [SugarColumn(Length = 200)]
        public string CourseName { get; set; }

        [SugarColumn(Length = 200)]
        public string CourseNameAr { get; set; }

        
        public string Description { get; set; }

        [SugarColumn(Length = 50)]
        public string Category { get; set; }

        [SugarColumn(Length = 20)]
        public string CourseType { get; set; } // online, classroom, hybrid

        public int DurationHours { get; set; }

        public decimal Cost { get; set; }

        [SugarColumn(Length = 200)]
        public string Provider { get; set; }

        [SugarColumn(Length = 200)]
        public string Instructor { get; set; }

        [SugarColumn(Length = 500)]
        public string Prerequisites { get; set; }

        [SugarColumn(Length = 500)]
        public string LearningObjectives { get; set; }

        public int MaxParticipants { get; set; }

        public int MinParticipants { get; set; }

     
        public string Level { get; set; } // beginner, intermediate, advanced

        [SugarColumn(Length = 20)]
        public string CertificationType { get; set; }

        public bool IsActive { get; set; }

        public bool IsMandatory { get; set; }

        public int? ValidityMonths { get; set; }


    }

    /// <summary>
    /// Training Session Table
    /// </summary>
    [SugarTable("hr_training_session")]
    public class HrTrainingSession : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SessionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CourseId { get; set; }

        [SugarColumn(Length = 100)]
        public string SessionCode { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [SugarColumn(Length = 200)]
        public string Location { get; set; }

        [SugarColumn(Length = 20)]
        public string DeliveryMode { get; set; }

        public int EnrolledCount { get; set; }
        
        [SugarColumn(Length = 200)]
        public string Instructor { get; set; }

        public int CompletedCount { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // scheduled, in_progress, completed, cancelled

        public decimal ActualCost { get; set; }

        [SugarColumn(Length = 500)]
        public string Materials { get; set; }
    }

    /// <summary>
    /// Training Enrollment Table
    /// </summary>
    [SugarTable("hr_training_enrollment")]
    public class HrTrainingEnrollment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long SessionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public DateTime EnrollmentDate { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // enrolled, in_progress, completed, cancelled, failed

        public decimal? Score { get; set; }

        public decimal? AttendancePercentage { get; set; }

        public DateTime? CompletionDate { get; set; }

        [SugarColumn(Length = 100)]
        public string CertificateNumber { get; set; }

        public DateTime? CertificateIssueDate { get; set; }

        public DateTime? CertificateExpiryDate { get; set; }

        [SugarColumn(Length = 500)]
        public string Feedback { get; set; }

        public int? Rating { get; set; }

        [SugarColumn(Length = 50)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

    }

    /// <summary>
    /// Training Progress Table
    /// </summary>
    [SugarTable("hr_training_progress")]
    public class HrTrainingProgress : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProgressId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }

        [SugarColumn(Length = 100)]
        public string ModuleName { get; set; }

        public decimal ProgressPercentage { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        public decimal? QuizScore { get; set; }

        public int? Attempts { get; set; }

        public TimeSpan? TimeSpent { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; }

        public DateTime LastAccessDate { get; set; }

    }

    /// <summary>
    /// Training Budget Table
    /// </summary>
    [SugarTable("hr_training_budget")]
    public class HrTrainingBudget : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BudgetId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        public int Year { get; set; }

        public decimal AllocatedBudget { get; set; }

        public decimal UsedBudget { get; set; }

        public decimal RemainingBudget { get; set; }

        [SugarColumn(Length = 500)]
        public string Notes { get; set; }

        [SugarColumn(Length = 50)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

  
    }

    /// <summary>
    /// Development Plan Table
    /// </summary>
    [SugarTable("hr_development_plan")]
    public class HrDevelopmentPlan : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 200)]
        public string PlanTitle { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        
        public string Objectives { get; set; }

        
        public string DevelopmentAreas { get; set; }

        
        public string ActionPlan { get; set; }

        
        public string SuccessCriteria { get; set; }

        public decimal ProgressPercentage { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // draft, active, on_hold, completed

        [SugarColumn(Length = 50)]
        public string MentorId { get; set; }

        
        public string MentorFeedback { get; set; }

        
        public string EmployeeFeedback { get; set; }

        public DateTime? ReviewDate { get; set; }

   

    }
}

