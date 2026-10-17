 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Performance Query DTO
    /// </summary>
    public class PerformanceQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReviewerId { get; set; }
        public int? ReviewYear { get; set; }
        public int? ReviewQuarter { get; set; }
        public string Status { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public string ReviewType { get; set; }
        public string ReviewPeriod { get; set; }
    }

    /// <summary>
    /// Performance Review DTO
    /// </summary>
    public class PerformanceReviewDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewId { get; set; }
        
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Reviewer is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewerId { get; set; }
        
        [Required(ErrorMessage = "Review period is required")]
        public string ReviewPeriod { get; set; }
        
        [Required(ErrorMessage = "Review type is required")]
        public string ReviewType { get; set; } // annual, quarterly, probation, project
        
        public DateTime ReviewDate { get; set; }
        
        // Ratings
        public decimal? QualityRating { get; set; }
        public decimal? ProductivityRating { get; set; }
        public decimal? KnowledgeRating { get; set; }
        public decimal? CommunicationRating { get; set; }
        public decimal? TeamworkRating { get; set; }
        public decimal? InitiativeRating { get; set; }
        public decimal? AttendanceRating { get; set; }
        public decimal OverallRating { get; set; }
        
        // Comments
        public string Strengths { get; set; }
        public string AreasForImprovement { get; set; }
        public string GoalsAchieved { get; set; }
        public string TrainingNeeds { get; set; }
        public string CareerAspirations { get; set; }
        public string ManagerComments { get; set; }
        public string EmployeeComments { get; set; }
        
        public string Status { get; set; } // draft, submitted, approved, rejected
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }
    }

    /// <summary>
    /// Performance Goal DTO
    /// </summary>
    public class PerformanceGoalDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GoalId { get; set; }
        
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Goal title is required")]
        public string GoalTitle { get; set; }
        
        public string Description { get; set; }
        public string Category { get; set; } // performance, development, project
        public int Year { get; set; }
        public int? Quarter { get; set; }
        
        [Required(ErrorMessage = "Target date is required")]
        public DateTime? TargetDate { get; set; }
        
        public decimal TargetValue { get; set; }
        public string TargetUnit { get; set; }
        public decimal CurrentProgress { get; set; }
        public decimal? AchievedValue { get; set; }
        public decimal Weight { get; set; } // Importance weight (percentage)
        
        public string Status { get; set; } // pending, in_progress, completed, cancelled
        public DateTime? CompletedDate { get; set; }
        public decimal? AchievementScore { get; set; }
        public string Comments { get; set; }
    }

    /// <summary>
    /// KPI Metric DTO
    /// </summary>
    public class KpiMetricDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MetricId { get; set; }
        
        [Required(ErrorMessage = "Metric name is required")]
        public string MetricName { get; set; }
        
        public string MetricNameAr { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }
        public string MeasurementUnit { get; set; }
        public string Frequency { get; set; } // daily, weekly, monthly, quarterly
        public decimal TargetValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public decimal Weight { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Feedback Request DTO
    /// </summary>
    public class FeedbackRequestDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequestId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewPeriod { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } // pending, completed
    }

    /// <summary>
    /// Feedback Response DTO
    /// </summary>
    public class FeedbackResponseDto
    {
        [Required(ErrorMessage = "Overall rating is required")]
        public decimal OverallRating { get; set; }
        
        public decimal? TechnicalSkillsRating { get; set; }
        public decimal? CommunicationRating { get; set; }
        public decimal? TeamworkRating { get; set; }
        public decimal? LeadershipRating { get; set; }
        public decimal? ProblemSolvingRating { get; set; }
        
        [MaxLength(1000)]
        public string Strengths { get; set; }
        
        [MaxLength(1000)]
        public string ImprovementAreas { get; set; }
        
        [MaxLength(1000)]
        public string AdditionalComments { get; set; }
        
        public string Relationship { get; set; } // manager, peer, subordinate, client
    }

    /// <summary>
    /// Performance Analytics DTO
    /// </summary>
    public class PerformanceAnalyticsDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int Year { get; set; }
        public decimal AverageRating { get; set; }
        public decimal RatingTrend { get; set; } // Percentage change from previous year
        public int GoalsCompleted { get; set; }
        public int TotalGoals { get; set; }
        public decimal GoalCompletionRate { get; set; }
        public Dictionary<string, decimal> RatingsByCategory { get; set; }
        public List<decimal> QuarterlyRatings { get; set; }
        public List<KpiScore> KpiScores { get; set; }
        public string PerformanceLevel { get; set; } // exceptional, exceeds, meets, needs_improvement
        public decimal PercentileRank { get; set; } // Among peers
    }

    public class KpiScore
    {
        public string MetricName { get; set; }
        public decimal Target { get; set; }
        public decimal Achieved { get; set; }
        public decimal Score { get; set; }
    }

    /// <summary>
    /// Department Performance DTO
    /// </summary>
    public class DepartmentPerformanceDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public int TotalEmployees { get; set; }
        public int ReviewsCompleted { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<string, int> RatingDistribution { get; set; }
        public decimal GoalCompletionRate { get; set; }
        public List<TopPerformerDto> TopPerformers { get; set; }
        public List<string> CommonStrengths { get; set; }
        public List<string> CommonImprovementAreas { get; set; }
    }

    /// <summary>
    /// Top Performer DTO
    /// </summary>
    public class TopPerformerDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public decimal Rating { get; set; }
        public decimal GoalCompletionRate { get; set; }
        public int Rank { get; set; }
    }

    /// <summary>
    /// Performance Comparison DTO
    /// </summary>
    public class PerformanceComparisonDto
    {
        public int Year { get; set; }
        public List<EmployeeComparison> Employees { get; set; }
        public Dictionary<string, List<decimal>> CategoryComparisons { get; set; }
        public decimal TeamAverage { get; set; }
        public decimal CompanyAverage { get; set; }
    }

    public class EmployeeComparison
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal OverallRating { get; set; }
        public Dictionary<string, decimal> CategoryRatings { get; set; }
    }

    /// <summary>
    /// Performance Improvement Plan DTO
    /// </summary>
    public class PerformanceImprovementPlanDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PipId { get; set; }
        
        [Required(ErrorMessage = "Employee is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
        
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
        
        public string ImprovementAreas { get; set; }
        public string SpecificGoals { get; set; }
        public string SupportProvided { get; set; }
        public string MilestoneChecks { get; set; }
        public string ConsequencesOfFailure { get; set; }
        public decimal CompletionPercentage { get; set; }
        public string ProgressUpdates { get; set; }
        public string Status { get; set; } // active, completed_success, completed_failure, cancelled
        public DateTime? CompletedDate { get; set; }
        public string FinalOutcome { get; set; }
    }

    /// <summary>
    /// Calibration Session DTO
    /// </summary>
    public class CalibrationSessionDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long SessionId { get; set; }
        
        [Required(ErrorMessage = "Session name is required")]
        public string SessionName { get; set; }
        
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DepartmentId { get; set; }
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public DateTime SessionDate { get; set; }
        public List<long> ParticipantIds { get; set; }
        public List<CalibrationEmployee> Employees { get; set; }
        public string Status { get; set; } // draft, in_progress, finalized
        public DateTime? FinalizedDate { get; set; }
        public string FinalizedBy { get; set; }
    }

    public class CalibrationEmployee
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal InitialRating { get; set; }
        public decimal? CalibratedRating { get; set; }
        public string Justification { get; set; }
        public string FinalBand { get; set; } // A, B, C, D
    }

    /// <summary>
    /// Review Cycle Status DTO
    /// </summary>
    public class ReviewCycleStatusDto
    {
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public int TotalEmployees { get; set; }
        public int ReviewsScheduled { get; set; }
        public int ReviewsCompleted { get; set; }
        public int ReviewsPending { get; set; }
        public int ReviewsOverdue { get; set; }
        public decimal CompletionPercentage { get; set; }
        public List<DepartmentReviewStatus> DepartmentStatuses { get; set; }
    }

    public class DepartmentReviewStatus
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int TotalReviews { get; set; }
        public int Completed { get; set; }
        public decimal CompletionRate { get; set; }
    }
}

