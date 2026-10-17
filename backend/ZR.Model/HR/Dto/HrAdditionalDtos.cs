 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model;

namespace ZR.Model.HR.Dto
{
    #region Analytics DTOs

    /// <summary>
    /// Analytics Report Query DTO
    /// </summary>
    public class AnalyticsReportQueryDto : PagerInfo
    {
        public string ReportType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public List<string> Metrics { get; set; }
        public string GroupBy { get; set; }
        public string Format { get; set; } // json, excel, pdf
    }

    /// <summary>
    /// Export Report DTO
    /// </summary>
    public class ExportReportDto
    {
        [Required(ErrorMessage = "Report type is required")]
        public string ReportType { get; set; }
        
        [Required(ErrorMessage = "Export format is required")]
        public string Format { get; set; } // excel, pdf, csv
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<long> DepartmentIds { get; set; }
        public List<long> EmployeeIds { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public bool IncludeCharts { get; set; }
        public string FileName { get; set; }
    }

    /// <summary>
    /// Training Metrics DTO
    /// </summary>
    public class TrainingMetricsDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalTrainingHours { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageScore { get; set; }
        public decimal TrainingCostPerEmployee { get; set; }
        public int CertificatesIssued { get; set; }
        public Dictionary<string, int> TrainingByCategory { get; set; }
        public Dictionary<string, decimal> CompletionRateByDepartment { get; set; }
        public List<TopTrainingPerformersDto> TopPerformers { get; set; }
        public int TrainingBudgetUtilization { get; set; }
    }

    /// <summary>
    /// Top Training Performers DTO
    /// </summary>
    public class TopTrainingPerformersDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public int CompletedTrainings { get; set; }
        public decimal AverageScore { get; set; }
        public int CertificatesEarned { get; set; }
    }

    #endregion

    #region KPI DTOs

    /// <summary>
    /// KPI Query DTO
    /// </summary>
    public class KpiQueryDto : PagerInfo
    {
        public string KpiName { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Period { get; set; }
        public bool? IsActive { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PositionId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// KPI DTO
    /// </summary>
    public class KpiDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }
        
    
        
        public string NameAr { get; set; }
        
        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }
        
        public string Description { get; set; }
        public string MeasurementUnit { get; set; }
        public string CalculationMethod { get; set; }
        public decimal? TargetValue { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string Period { get; set; } // monthly, quarterly, yearly
        public int Weight { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }





         

        [SugarColumn(Length = 100)]
        public string KpiCode { get; set; }

        [SugarColumn(Length = 200)]
        public string KpiName { get; set; }

        [SugarColumn(Length = 200)]
        public string KpiNameAr { get; set; }


        [SugarColumn(Length = 50)]
        public string MeasurementType { get; set; } // percentage, number, currency, ratio

        [SugarColumn(Length = 50)]
        public string Unit { get; set; }
         

        [SugarColumn(Length = 20)]
        public string Frequency { get; set; } // daily, weekly, monthly, quarterly, yearly

 

        [SugarColumn(Length = 500)]
        public string Formula { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }
         

        public bool IsCompanyWide { get; set; } = false;

        [SugarColumn(Length = 20)]
        public string Trend { get; set; } // higher_better, lower_better, range

        [SugarColumn(Length = 500)]
        public string DataSource { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal ActualValue { get; set; }







    }

    /// <summary>
    /// Employee KPI DTO
    /// </summary>
    public class EmployeeKpiDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeKpiId { get; set; }
        
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "KPI ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }
        
        public decimal? TargetValue { get; set; }
        public decimal? ActualValue { get; set; }
        public decimal? AchievementPercentage { get; set; }
        public string Status { get; set; } // not_started, in_progress, achieved, not_achieved
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string Comments { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string UpdatedBy { get; set; }
    }

    /// <summary>
    /// Bulk KPI Assignment DTO
    /// </summary>
    public class BulkKpiAssignmentDto
    {
        [Required(ErrorMessage = "Employee IDs are required")]
        public List<long> EmployeeIds { get; set; }
        
        [Required(ErrorMessage = "KPI IDs are required")]
        public List<long> KpiIds { get; set; }
        
        [Required(ErrorMessage = "KPI ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }
        
        public decimal? TargetValue { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public string Comments { get; set; }
    }

    /// <summary>
    /// KPI Achievement DTO
    /// </summary>
    public class KpiAchievementDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeKpiId { get; set; }
        
        [Required(ErrorMessage = "Actual value is required")]
        public decimal ActualValue { get; set; }
        
        public string Evidence { get; set; }
        public string Comments { get; set; }
        public DateTime AchievementDate { get; set; }
        public List<string> AttachmentUrls { get; set; }
    }

    /// <summary>
    /// Quarterly KPI DTO
    /// </summary>
    public class QuarterlyKpiDto
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public List<KpiPerformanceDto> KpiPerformances { get; set; }
        public decimal OverallAchievementPercentage { get; set; }
        public string OverallGrade { get; set; }
    }

    /// <summary>
    /// KPI Performance DTO
    /// </summary>
    public class KpiPerformanceDto
    {
        public string KpiName { get; set; }
        public string Category { get; set; }
        public decimal TargetValue { get; set; }
        public decimal ActualValue { get; set; }
        public decimal AchievementPercentage { get; set; }
        public int Weight { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// KPI Report DTO
    /// </summary>
    public class KpiReportDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ReportDate { get; set; }
        public string Period { get; set; }
        public int TotalKpis { get; set; }
        public int AchievedKpis { get; set; }
        public decimal OverallAchievementRate { get; set; }
        public Dictionary<string, decimal> AchievementByCategory { get; set; }
        public Dictionary<string, decimal> AchievementByDepartment { get; set; }
        public List<TopKpiPerformersDto> TopPerformers { get; set; }
        public List<UnderperformingKpiDto> UnderperformingKpis { get; set; }
    }

    /// <summary>
    /// Top KPI Performers DTO
    /// </summary>
    public class TopKpiPerformersDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public decimal AchievementPercentage { get; set; }
        public int AchievedKpis { get; set; }
        public int TotalKpis { get; set; }
    }

    /// <summary>
    /// Underperforming KPI DTO
    /// </summary>
    public class UnderperformingKpiDto
    {
        public string KpiName { get; set; }
        public string Category { get; set; }
        public decimal AverageAchievement { get; set; }
        public int EmployeesAssigned { get; set; }
        public int UnderperformingEmployees { get; set; }
    }

    /// <summary>
    /// Team KPI Report DTO
    /// </summary>
    public class TeamKpiReportDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ManagerId { get; set; }
        public DateTime PeriodDate { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Period { get; set; }
        public decimal TeamAchievementRate { get; set; }
        public List<EmployeeKpiSummaryDto> EmployeePerformances { get; set; }
        public List<KpiCategoryPerformanceDto> CategoryPerformances { get; set; }
        public string ManagerComments { get; set; }
    }

    /// <summary>
    /// Employee KPI Summary DTO
    /// </summary>
    public class EmployeeKpiSummaryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal AchievementPercentage { get; set; }
        public int CompletedKpis { get; set; }
        public int TotalKpis { get; set; }
        public string PerformanceGrade { get; set; }
    }

    /// <summary>
    /// KPI Category Performance DTO
    /// </summary>
    public class KpiCategoryPerformanceDto
    {
        public string Category { get; set; }
        public decimal AchievementRate { get; set; }
        public int TotalKpis { get; set; }
        public int AchievedKpis { get; set; }
    }

    /// <summary>
    /// Create From Template DTO
    /// </summary>
    public class CreateFromTemplateDto
    {
        [Required(ErrorMessage = "Template ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TemplateId { get; set; }
        
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        public List<long> EmployeeIds { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public Dictionary<long, decimal> KpiTargetOverrides { get; set; }
    }

    #endregion

    #region Benefits DTOs

    /// <summary>
    /// Benefits DTO (General)
    /// </summary>
    public class BenefitsDto
    {



        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }


        public string BenefitType { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
        public string ValueType { get; set; } // fixed, percentage
        public string EligibilityCriteria { get; set; }
        public bool IsTaxable { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }








        [SugarColumn(Length = 50)]
        public string PlanCode { get; set; }

        [SugarColumn(Length = 200)]
        public string PlanName { get; set; }

        [SugarColumn(Length = 200)]
        public string PlanNameAr { get; set; }

        [SugarColumn(Length = 50)]
        public string PlanType { get; set; } // health, dental, vision, life, disability, retirement

        [SugarColumn(Length = 200)]
        public string Provider { get; set; }

         


        public string Coverage { get; set; }

        public decimal EmployerContribution { get; set; }

        public decimal EmployeeContribution { get; set; }

        public decimal MaxCoverage { get; set; }

        public decimal Deductible { get; set; }

        [SugarColumn(Length = 20)]
        public string EligibilityPeriod { get; set; }

        public int WaitingPeriodDays { get; set; }

        public bool IsMandatory { get; set; }
         

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // active, inactive, suspended 
    }

    /// <summary>
    /// Benefit Enrollment Query DTO
    /// </summary>
    public class BenefitEnrollmentQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PlanId { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EnrollmentStartDate { get; set; }
        public DateTime? EnrollmentEndDate { get; set; }
        public string PlanType { get; set; }
    }

    /// <summary>
    /// Benefit Enrollment DTO
    /// </summary>
    public class BenefitEnrollmentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Benefit ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BenefitId { get; set; }
        
        public DateTime EnrollmentDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string Status { get; set; }
        public decimal CoverageAmount { get; set; }
        public List<BeneficiaryDto> Beneficiaries { get; set; }
        public string EnrollmentReason { get; set; }
    }

    /// <summary>
    /// Beneficiary DTO
    /// </summary>
    public class BeneficiaryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BeneficiaryId { get; set; }
        public string Name { get; set; }
        public string Relationship { get; set; }
        public DateTime BirthDate { get; set; }
        public decimal Percentage { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
    }

    /// <summary>
    /// Terminate Enrollment DTO
    /// </summary>
    public class TerminateEnrollmentDto
    {
        [Required(ErrorMessage = "Enrollment ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        [Required(ErrorMessage = "Termination date is required")]
        public DateTime TerminationDate { get; set; }
        
        [Required(ErrorMessage = "Termination reason is required")]
        public string TerminationReason { get; set; }
        public string Reason { get; set; } // Alias for TerminationReason
        
        public string Comments { get; set; }
    }

    /// <summary>
    /// Benefit Claim Query DTO
    /// </summary>
    public class BenefitClaimQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PlanId { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ClaimStartDate { get; set; }
        public DateTime? ClaimEndDate { get; set; }
        public string ClaimType { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }

    #endregion

    #region Training DTOs

    /// <summary>
    /// Training Course Query DTO
    /// </summary>
    public class TrainingCourseQueryDto : PagerInfo
    {
        public string CourseName { get; set; }
        public string Category { get; set; }
        public string Provider { get; set; }
        public string Level { get; set; }
        public string DeliveryMethod { get; set; }
        public bool? IsMandatory { get; set; }
        public bool? IsActive { get; set; }
        public decimal? MinCost { get; set; }
        public decimal? MaxCost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Training DTO (General)
    /// </summary>
    public class TrainingDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TrainingId { get; set; }
        
        //[Required(ErrorMessage = "Training name is required")]
        public string TrainingName { get; set; }
        
        public string TrainingNameAr { get; set; }
        public string Category { get; set; }
        public string Provider { get; set; }
        public string Level { get; set; } // beginner, intermediate, advanced
        public string DeliveryMethod { get; set; } // online, classroom, hybrid
        public int Duration { get; set; } // in hours
        public string Description { get; set; }
        public string Objectives { get; set; }
        public string Prerequisites { get; set; }
        public decimal Cost { get; set; }
        public string Currency { get; set; }
        public int MaxParticipants { get; set; }
        public bool IsMandatory { get; set; }
        public bool RequiresCertification { get; set; }
        public string Materials { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public bool IsActive { get; set; }


        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CourseId { get; set; }

        [SugarColumn(Length = 100)]
        public string CourseCode { get; set; }

        [SugarColumn(Length = 200)]
        public string CourseName { get; set; }

        [SugarColumn(Length = 200)]
        public string CourseNameAr { get; set; } 

        [SugarColumn(Length = 20)]
        public string CourseType { get; set; } // online, classroom, hybrid

        public int DurationHours { get; set; } 

        [SugarColumn(Length = 200)]
        public string Instructor { get; set; } 

        [SugarColumn(Length = 500)]
        public string LearningObjectives { get; set; } 

        public int MinParticipants { get; set; }

         

        [SugarColumn(Length = 20)]
        public string CertificationType { get; set; }
         

        public int? ValidityMonths { get; set; }

    }

    /// <summary>
    /// Cancel Session DTO
    /// </summary>
    public class CancelSessionDto
    {
        [Required(ErrorMessage = "Session ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SessionId { get; set; }
        
        [Required(ErrorMessage = "Cancellation reason is required")]
        public string CancellationReason { get; set; }
        public string Reason { get; set; } // Alias for CancellationReason
        
        public DateTime? NewSessionDate { get; set; }
        public bool NotifyParticipants { get; set; } = true;
        public string NotificationMessage { get; set; }
    }

    /// <summary>
    /// Cancel Enrollment DTO
    /// </summary>
    public class CancelEnrollmentDto
    {
        [Required(ErrorMessage = "Enrollment ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        [Required(ErrorMessage = "Cancellation reason is required")]
        public string CancellationReason { get; set; }
        public string Reason { get; set; } // Alias for CancellationReason
        
        public bool RefundFees { get; set; }
        public string Comments { get; set; }
    }

    /// <summary>
    /// Complete Enrollment DTO
    /// </summary>
    public class CompleteEnrollmentDto
    {
        [Required(ErrorMessage = "Enrollment ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        public DateTime CompletionDate { get; set; }
        public decimal? Score { get; set; }
        public bool PassedCertification { get; set; }
        public string CertificateNumber { get; set; }
        public string Comments { get; set; }
        public List<string> AttachmentUrls { get; set; }
    }

    /// <summary>
    /// Bulk Enrollment DTO
    /// </summary>
    public class BulkEnrollmentDto
    {
        [Required(ErrorMessage = "Session ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SessionId { get; set; }
        
        [Required(ErrorMessage = "Employee IDs are required")]
        public List<long> EmployeeIds { get; set; }
        
        public string EnrollmentReason { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime? CompletionDeadline { get; set; }
        public string EnrolledBy { get; set; }
    }

    /// <summary>
    /// Training Progress DTO
    /// </summary>
    public class TrainingProgressDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TrainingId { get; set; }
        public string TrainingName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public decimal ProgressPercentage { get; set; }
        public List<ModuleProgressDto> ModuleProgress { get; set; }
        public int CompletedHours { get; set; }
        public int TotalHours { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Module Progress DTO
    /// </summary>
    public class ModuleProgressDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal? Score { get; set; }
        public int TimeSpent { get; set; } // in minutes
    }

    /// <summary>
    /// Module Completion DTO
    /// </summary>
    public class ModuleCompletionDto
    {
        [Required(ErrorMessage = "Enrollment ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        [Required(ErrorMessage = "Module ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ModuleId { get; set; }
        
        public string ModuleName { get; set; }
        public DateTime CompletionDate { get; set; }
        public decimal? Score { get; set; }
        public int TimeSpent { get; set; } // in minutes
        public string Comments { get; set; }
    }

    /// <summary>
    /// Training Budget DTO
    /// </summary>
    public class TrainingBudgetDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BudgetId { get; set; }
        public int Year { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public decimal AllocatedBudget { get; set; }
        public decimal UsedBudget { get; set; }
        public decimal RemainingBudget { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public List<BudgetBreakdownDto> Breakdown { get; set; }
    }

    /// <summary>
    /// Budget Breakdown DTO
    /// </summary>
    public class BudgetBreakdownDto
    {
        public string Category { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }

    /// <summary>
    /// Update Budget Usage DTO
    /// </summary>
    public class UpdateBudgetUsageDto
    {
        [Required(ErrorMessage = "Budget ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BudgetId { get; set; }
        
        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }
        
        public string Category { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TrainingId { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    /// <summary>
    /// Update Plan Progress DTO
    /// </summary>
    public class UpdatePlanProgressDto
    {
        [Required(ErrorMessage = "Plan ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }
        
        public decimal Progress { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public List<long> CompletedTrainingIds { get; set; }
    }

    /// <summary>
    /// Training Feedback DTO
    /// </summary>
    public class TrainingFeedbackDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long FeedbackId { get; set; }
        
        [Required(ErrorMessage = "Enrollment ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        [Required(ErrorMessage = "Overall rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int OverallRating { get; set; }
        public int Rating { get; set; } // Alias
        public string Feedback { get; set; } // Additional feedback text
        
        public int ContentQuality { get; set; }
        public int InstructorRating { get; set; }
        public int MaterialsRating { get; set; }
        public int FacilitiesRating { get; set; }
        public string WhatWentWell { get; set; }
        public string ImprovementSuggestions { get; set; }
        public string AdditionalComments { get; set; }
        public bool WouldRecommend { get; set; }
        public DateTime FeedbackDate { get; set; }
    }

    /// <summary>
    /// Training Report DTO
    /// </summary>
    public class TrainingReportDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ReportDate { get; set; }
        public string Period { get; set; }
        public int TotalTrainings { get; set; }
        public int CompletedTrainings { get; set; }
        public int OngoingTrainings { get; set; }
        public int CancelledTrainings { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageScore { get; set; }
        public decimal AverageRating { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CostPerEmployee { get; set; }
        public Dictionary<string, int> TrainingsByCategory { get; set; }
        public Dictionary<string, int> TrainingsByDepartment { get; set; }
        public List<PopularTrainingDto> PopularTrainings { get; set; }
        public List<TrainingROIDto> TrainingROI { get; set; }
    }

    /// <summary>
    /// Popular Training DTO
    /// </summary>
    public class PopularTrainingDto
    {
        public string TrainingName { get; set; }
        public string Category { get; set; }
        public int EnrollmentCount { get; set; }
        public decimal AverageRating { get; set; }
        public decimal CompletionRate { get; set; }
    }

    /// <summary>
    /// Training ROI DTO
    /// </summary>
    public class TrainingROIDto
    {
        public string TrainingName { get; set; }
        public decimal Cost { get; set; }
        public int ParticipantCount { get; set; }
        public decimal CostPerParticipant { get; set; }
        public decimal PerformanceImprovement { get; set; }
        public decimal ROIPercentage { get; set; }
    }

    #endregion

    #region Loan DTOs

    /// <summary>
    /// Loan Query DTO
    /// </summary>
    public class LoanQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        public string LoanType { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RequestStartDate { get; set; }
        public DateTime? RequestEndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
    }

    /// <summary>
    /// Loan Application DTO
    /// </summary>
    public class LoanApplicationDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Loan type is required")]
        public string LoanType { get; set; } // personal, housing, emergency, education
        
        [Required(ErrorMessage = "Requested amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal RequestedAmount { get; set; }
        
        [Required(ErrorMessage = "Repayment months is required")]
        [Range(1, 120, ErrorMessage = "Repayment period must be between 1 and 120 months")]
        public int RequestedMonths { get; set; }
        
        [Required(ErrorMessage = "Purpose is required")]
        public string Purpose { get; set; }
        
        public string Justification { get; set; }
        public decimal CurrentSalary { get; set; }
        public decimal ExistingLoans { get; set; }
        public string Guarantor1Name { get; set; }
        public string Guarantor1Contact { get; set; }
        public string Guarantor2Name { get; set; }
        public string Guarantor2Contact { get; set; }
        public List<string> SupportingDocuments { get; set; }
        public DateTime ApplicationDate { get; set; }
    }

    /// <summary>
    /// Loan DTO (General)
    /// </summary>
    public class LoanDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string LoanType { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal ApprovedAmount { get; set; }
        public int RepaymentMonths { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MonthlyDeduction { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; } // pending, approved, rejected, active, completed, defaulted
        public DateTime ApplicationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public decimal TotalRepaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime? NextPaymentDate { get; set; }
        public string ApprovalComments { get; set; }
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// Cancel Loan DTO
    /// </summary>
    public class CancelLoanDto
    {
        [Required(ErrorMessage = "Loan ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
        
        [Required(ErrorMessage = "Cancellation reason is required")]
        public string CancellationReason { get; set; }
        
        public string Comments { get; set; }
        public DateTime CancellationDate { get; set; }
        public bool RefundProcessingFees { get; set; }
    }

    /// <summary>
    /// Loan Repayment DTO
    /// </summary>
    public class LoanRepaymentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RepaymentId { get; set; }
        
        [Required(ErrorMessage = "Loan ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }
        
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } // salary_deduction, bank_transfer, cash
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public string TransactionReference { get; set; }
        public string Status { get; set; } // pending, completed, failed
        public string Comments { get; set; }
    }

    /// <summary>
    /// Process Monthly Repayment DTO
    /// </summary>
    public class ProcessMonthlyRepaymentDto
    {
        [Required(ErrorMessage = "Pay period is required")]
        public string PayPeriod { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public List<long> LoanIds { get; set; }
        public DateTime ProcessingDate { get; set; }
        public bool AutoProcessFailedPayments { get; set; }
        public string ProcessedBy { get; set; }
    }

    /// <summary>
    /// Bulk Status Update DTO
    /// </summary>
    public class BulkStatusUpdateDto
    {
        [Required(ErrorMessage = "Loan IDs are required")]
        public List<long> LoanIds { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
        
        [Required(ErrorMessage = "New status is required")]
        public string NewStatus { get; set; }
        
        public string Comments { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string UpdatedBy { get; set; }
        public bool NotifyEmployees { get; set; }
    }

    /// <summary>
    /// Loan Summary DTO
    /// </summary>
    public class LoanSummaryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public List<ActiveLoanDto> ActiveLoans { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal MonthlyDeduction { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public int EligibilityScore { get; set; }
        public decimal MaxEligibleAmount { get; set; }
    }

    /// <summary>
    /// Active Loan DTO
    /// </summary>
    public class ActiveLoanDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        public string LoanType { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal MonthlyDeduction { get; set; }
        public DateTime? NextPaymentDate { get; set; }
        public int RemainingMonths { get; set; }
        public string Status { get; set; }
    }

    #endregion
}

