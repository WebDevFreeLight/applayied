 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model;

namespace ZR.Model.HR.Dto
{
    #region Notification DTOs

    /// <summary>
    /// Leave Notification DTO
    /// </summary>
    public class LeaveNotificationDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveId { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
    }

    /// <summary>
    /// Leave Approval Notification DTO
    /// </summary>
    public class LeaveApprovalNotificationDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LeaveId { get; set; }
        
        [Required]
        public string Status { get; set; }
    }

    /// <summary>
    /// Payslip Notification DTO
    /// </summary>
    public class PayslipNotificationDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PayrollId { get; set; }
        
        [Required]
        public DateTime PayPeriod { get; set; }
    }

    /// <summary>
    /// Performance Review Notification DTO
    /// </summary>
    public class PerformanceReviewNotificationDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewId { get; set; }
        
        [Required]
        public string ReviewType { get; set; }
    }

    /// <summary>
    /// Training Notification DTO
    /// </summary>
    public class TrainingNotificationDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SessionId { get; set; }
        
        [Required]
        public string CourseName { get; set; }
    }

    /// <summary>
    /// Document Expiry Notification DTO
    /// </summary>
    public class DocumentExpiryNotificationDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        public string DocumentType { get; set; }
        
        [Required]
        public DateTime ExpiryDate { get; set; }
    }

    /// <summary>
    /// Anniversary Notification DTO
    /// </summary>
    public class AnniversaryNotificationDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        public int Years { get; set; }
    }

    // NotificationTemplateDto already exists in WorkflowDto.cs

    /// <summary>
    /// Template Notification DTO
    /// </summary>
    public class TemplateNotificationDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RecipientId { get; set; }
        
        public Dictionary<string, string> Parameters { get; set; }
    }

    #endregion

    #region Benefits DTOs

    /// <summary>
    /// Update Claim Status DTO
    /// </summary>
    public class UpdateClaimStatusDto
    {
        [Required]
        public string Status { get; set; }
        
        public string Comments { get; set; }
        public string ProcessedBy { get; set; }
        public DateTime? ProcessedDate { get; set; }
    }

    /// <summary>
    /// Approve Claim DTO
    /// </summary>
    public class ApproveClaimDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ClaimId { get; set; }
        
        public string ApprovalComments { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string ApprovedBy { get; set; }
    }

    /// <summary>
    /// Reject Claim DTO
    /// </summary>
    public class RejectClaimDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ClaimId { get; set; }
        
        [Required]
        public string RejectionReason { get; set; }
        public string Reason { get; set; } // Alias for RejectionReason
        
        public string RejectedBy { get; set; }
    }

    /// <summary>
    /// Process Payment DTO
    /// </summary>
    public class ProcessPaymentDto
    {
       
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ClaimId { get; set; }
        
     
        public decimal PaymentAmount { get; set; }
        
        public string PaymentMethod { get; set; }
        public string BankReference { get; set; }
        public string PaymentReference { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string ProcessedBy { get; set; }





        [JsonConverter(typeof(ValueToStringConverter))]
        public long payrollId { get; set; } 


    }

    /// <summary>
    /// Employee Dependent DTO
    /// </summary>
    public class EmployeeDependentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DependentId { get; set; }
        
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        public string DependentName { get; set; }
        
        [Required]
        public string Relationship { get; set; }
        
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string IdNumber { get; set; }
        public bool IsEligible { get; set; }
        public DateTime? EligibilityStartDate { get; set; }
        public DateTime? EligibilityEndDate { get; set; }
    }

    /// <summary>
    /// Update Progress DTO
    /// </summary>
    public class UpdateProgressDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ItemId { get; set; }
        
        [Required]
        public decimal ProgressPercentage { get; set; }
        public decimal Progress { get; set; } // Alias
        
        public string Status { get; set; }
        public string Comments { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// Open Enrollment DTO
    /// </summary>
    public class OpenEnrollmentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public List<long> SelectedPlanIds { get; set; }
        
        [Required]
        public string EnrollmentPeriod { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public List<long> EligibleEmployees { get; set; }
        public List<long> AvailablePlans { get; set; }
        public bool AutoEnroll { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Benefit Plan Query DTO
    /// </summary>
    public class BenefitPlanQueryDto : PagerInfo
    {
        public string PlanName { get; set; }
        public string PlanType { get; set; }
        public string Status { get; set; }
        public bool? IsActive { get; set; }
    }

    #endregion

    #region Analytics DTOs

    /// <summary>
    /// Analytics Report DTO
    /// </summary>
    public class AnalyticsReportDto
    {
        [Required]
        public string ReportType { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Metrics { get; set; }
        public List<string> Dimensions { get; set; }
        public string GroupBy { get; set; }
        public List<long> DepartmentIds { get; set; }
    }

    /// <summary>
    /// Export Analytics DTO
    /// </summary>
    public class ExportAnalyticsDto
    {
        [Required]
        public string ReportType { get; set; }
        
        [Required]
        public string ExportFormat { get; set; } // Excel, PDF, CSV
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Columns { get; set; }
        public bool IncludeCharts { get; set; }
    }

    #endregion

    #region KPI DTOs

    /// <summary>
    /// KPI Dashboard DTO
    /// </summary>
    public class KpiDashboardDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public string Period { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Generate Report DTO
    /// </summary>
    public class GenerateReportDto
    {
        [Required]
        public string ReportType { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<long> EmployeeIds { get; set; }
        public List<long> DepartmentIds { get; set; }
        public string Format { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    /// <summary>
    /// Assign KPI DTO
    /// </summary>
    public class AssignKpiDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }
        
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        public decimal TargetValue { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Weight { get; set; }
        public string Comments { get; set; }
    }

    /// <summary>
    /// Record Achievement DTO
    /// </summary>
    public class RecordAchievementDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiAssignmentId { get; set; }
        
        [Required]
        public decimal ActualValue { get; set; }
        
        public DateTime? AchievementDate { get; set; }
        public string Comments { get; set; }
        public string Evidence { get; set; }
    }

    /// <summary>
    /// Calculate Score DTO
    /// </summary>
    public class CalculateScoreDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        public DateTime PeriodDate { get; set; }
        
        public string Period { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Process KPI DTO
    /// </summary>
    public class ProcessKpiDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }
        
        public string Action { get; set; } // ACTIVATE, DEACTIVATE, CALCULATE
        public DateTime? ProcessDate { get; set; }
        public string ProcessedBy { get; set; }
    }

    /// <summary>
    /// Export KPI DTO
    /// </summary>
    public class ExportKpiDto
    {
        public List<long> EmployeeIds { get; set; }
        public List<long> KpiIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        [Required]
        public string ExportFormat { get; set; }
        
        public string Format { get; set; }
        public string Category { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public bool? IsActive { get; set; }
        public bool IncludeAchievements { get; set; }
    }

    /// <summary>
    /// Bulk Assign KPI DTO
    /// </summary>
    public class BulkAssignKpiDto
    {
        [Required]
        public List<long> EmployeeIds { get; set; }
        
        [Required]
        public List<KpiAssignment> KpiAssignments { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class KpiAssignment
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }
        public decimal TargetValue { get; set; }
        public decimal Weight { get; set; }
    }

    #endregion

    #region Loan DTOs

    /// <summary>
    /// Approve Loan DTO
    /// </summary>
    public class ApproveLoanDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        
        public decimal? ApprovedAmount { get; set; }
        public string Comments { get; set; }
        public string ApprovalComments { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int? InstallmentMonths { get; set; }
        public decimal? InterestRate { get; set; }
    }

    /// <summary>
    /// Reject Loan DTO
    /// </summary>
    public class RejectLoanDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        
        public string Reason { get; set; }
        
        public string RejectionReason { get; set; }
        
        
        public string RejectedBy { get; set; }
        
        public DateTime? RejectionDate { get; set; }

         
    }

    /// <summary>
    /// Process Repayment DTO
    /// </summary>
    public class ProcessRepaymentDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        
        [Required]
        public decimal RepaymentAmount { get; set; }
        
        public DateTime? RepaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Reference { get; set; }
    }

    /// <summary>
    /// Early Settlement DTO
    /// </summary>
    public class EarlySettlementDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public decimal SettlementAmount { get; set; }
        
        public string PaymentMethod { get; set; }
        public DateTime? SettlementDate { get; set; }
        public decimal? InterestWaived { get; set; }
        public string Reason { get; set; }
    }

    /// <summary>
    /// Attach Document DTO
    /// </summary>
    public class AttachDocumentDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EntityId { get; set; }
        
        [Required]
        public string EntityType { get; set; }
        
        [Required]
        public string DocumentType { get; set; }
        
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string DocumentPath { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        public string Description { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long FileSize { get; set; }
    }

    /// <summary>
    /// Export Loans DTO
    /// </summary>
    public class ExportLoansDto
    {
        public List<long> LoanIds { get; set; }
        public List<long> EmployeeIds { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        public string LoanType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        
        [Required]
        public string Format { get; set; }
        
        public bool IncludeRepaymentHistory { get; set; }
    }

    /// <summary>
    /// Bulk Repayment DTO
    /// </summary>
    public class BulkRepaymentDto
    {
        [Required]
        public List<RepaymentItem> RepaymentItems { get; set; }
        
        public DateTime? ProcessDate { get; set; }
        public string PaymentMethod { get; set; }
        public string ProcessedBy { get; set; }
    }

    public class RepaymentItem
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
    }

    #endregion

    #region Training DTOs

    /// <summary>
    /// Export Training DTO
    /// </summary>
    public class ExportTrainingDto
    {
        public List<long> CourseIds { get; set; }
        public List<long> EmployeeIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IncludeDetails { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsMandatory { get; set; }
        
        [Required]
        public string Format { get; set; }
        
        public bool IncludeProgressData { get; set; }
        public bool IncludeCertificates { get; set; }
    }

    /// <summary>
    /// Enrollment Progress DTO
    /// </summary>
    public class EnrollmentProgressDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        [Required]
        public decimal ProgressPercentage { get; set; }
        
        public DateTime? LastAccessed { get; set; }
        public int CompletedModules { get; set; }
        public int TotalModules { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Update Budget DTO
    /// </summary>
    public class UpdateBudgetDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BudgetId { get; set; }
        
        [Required]
        public decimal AllocatedAmount { get; set; }
        
        public decimal? SpentAmount { get; set; }
        public string Period { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Development Plan DTO
    /// </summary>
    public class DevelopmentPlanDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }
        
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        public string PlanName { get; set; }
        
        public List<long> CourseIds { get; set; }
        public List<string> SkillsToImprove { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string Goals { get; set; }
    }

    /// <summary>
    /// Submit Feedback DTO
    /// </summary>
    public class SubmitFeedbackDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CourseId { get; set; }
        
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required]
        public int Rating { get; set; } // 1-5 stars
        
        public string Comments { get; set; }
        public string InstructorFeedback { get; set; }
        public string ContentFeedback { get; set; }
        public List<string> Suggestions { get; set; }
    }

    /// <summary>
    /// Export Data DTO
    /// </summary>
    public class ExportDataDto
    {
        [Required]
        public string DataType { get; set; }
        
        [Required]
        public string ExportFormat { get; set; }
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Columns { get; set; }
        public Dictionary<string, object> Filters { get; set; }
    }

    /// <summary>
    /// Record Module Completion DTO
    /// </summary>
    public class RecordModuleCompletionDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ModuleId { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        public decimal? Score { get; set; }
        public int TimeSpent { get; set; } // in minutes
        public string Status { get; set; }
    }

    /// <summary>
    /// Mentor Feedback DTO
    /// </summary>
    public class MentorFeedbackDto
    {
        [Required]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MentorshipId { get; set; }
        
        [Required]
        public string FeedbackType { get; set; }
        
        [Required]
        public string Feedback { get; set; }
        
        public int? Rating { get; set; }
        public DateTime? FeedbackDate { get; set; }
        public List<string> AreasOfImprovement { get; set; }
        public List<string> Strengths { get; set; }
    }

    #endregion
}

