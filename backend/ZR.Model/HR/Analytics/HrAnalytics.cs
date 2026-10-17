 
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;
using Newtonsoft.Json;

namespace ZR.Model.HR
{
    /// <summary>
    /// Analytics Report Table
    /// </summary>
    [SugarTable("hr_analytics_report")]
    public class HrAnalyticsReport : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        [JsonConverter(typeof(ValueToStringConverter))]
         
        public long ReportId { get; set; }

        [SugarColumn(Length = 100)]
        public string ReportType { get; set; }

        [SugarColumn(Length = 200)]
        public string ReportName { get; set; }

        
        public string ReportData { get; set; }

        
        public string Parameters { get; set; }

        public DateTime GeneratedDate { get; set; }

        [SugarColumn(Length = 500)]
        public string FilePath { get; set; }

        [SugarColumn(Length = 50)]
        public string Format { get; set; }

     }

    /// <summary>
    /// Workforce Metrics Table
    /// </summary>
    [SugarTable("hr_workforce_metrics")]
    public class HrWorkforceMetrics : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        [JsonConverter(typeof(ValueToStringConverter))]
         
        public long MetricsId { get; set; }

        public DateTime MetricsDate { get; set; }

        public int TotalEmployees { get; set; }

        public int NewHires { get; set; }

        public int Terminations { get; set; }

        public decimal TurnoverRate { get; set; }

        public decimal AverageTenure { get; set; }

        
        public string DepartmentDistribution { get; set; }

        
        public string GenderDistribution { get; set; }

        
        public string AgeDistribution { get; set; }

        
        public string LocationDistribution { get; set; } 
    }

    /// <summary>
    /// Performance Metrics Table
    /// </summary>
    [SugarTable("hr_performance_metrics")]
    public class HrPerformanceMetrics : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]


        [JsonConverter(typeof(ValueToStringConverter))]
         
        public long MetricsId { get; set; }

        public int Year { get; set; }

        public DateTime MetricsDate { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalReviews { get; set; }

        public decimal GoalCompletionRate { get; set; }

        
        public string RatingDistribution { get; set; }

        
        public string TopPerformers { get; set; }

        
        public string LowPerformers { get; set; }

        
        public string ImprovementAreas { get; set; } 
    }

    /// <summary>
    /// Compensation Metrics Table
    /// </summary>
    [SugarTable("hr_compensation_metrics")]
    public class HrCompensationMetrics : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        [JsonConverter(typeof(ValueToStringConverter))]
         
        public long MetricsId { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public DateTime MetricsDate { get; set; }

        public decimal TotalPayrollCost { get; set; }

        public decimal AverageSalary { get; set; }

        public decimal MedianSalary { get; set; }

        public decimal TotalOvertime { get; set; }

        public decimal TotalBonuses { get; set; }

        public decimal TotalDeductions { get; set; }

        
        public string DepartmentSalaryDistribution { get; set; }

        
        public string PositionSalaryDistribution { get; set; }

        
        public string CompaRatioAnalysis { get; set; }

        /// <summary>
        /// Finance GL Journal ID used as the source-of-truth for derived metrics (FK -> gl_journal.id)
        /// </summary>
        public Guid? GLJournalId { get; set; }
         
    }

    /// <summary>
    /// Leave Metrics Table
    /// </summary>
    [SugarTable("hr_leave_metrics")]
    public class HrLeaveMetrics : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        [JsonConverter(typeof(ValueToStringConverter))]
         
        public long MetricsId { get; set; }

        public int Year { get; set; }

        public DateTime MetricsDate { get; set; }

        public int TotalLeaveRequests { get; set; }

        public int ApprovedLeaves { get; set; }

        public int RejectedLeaves { get; set; }

        public int PendingLeaves { get; set; }

        public decimal ApprovalRate { get; set; }

        public decimal AverageLeaveDays { get; set; }

        public decimal TotalEntitlement { get; set; }

        public decimal TotalUsed { get; set; }

        public decimal TotalRemaining { get; set; }

        public decimal UtilizationRate { get; set; }

        public string LeaveTypeDistribution { get; set; }

        public string MonthlyTrends { get; set; } 
    }

    /// <summary>
    /// Training Metrics Table
    /// </summary>
    [SugarTable("hr_training_metrics")]
    public class HrTrainingMetrics : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
         
         
        public long MetricsId { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public DateTime MetricsDate { get; set; }

        public int TotalSessions { get; set; }

        public int TotalEnrollments { get; set; }

        public int CompletedEnrollments { get; set; }

        public decimal CompletionRate { get; set; }

        public decimal AverageScore { get; set; }

        public decimal AverageSatisfaction { get; set; }

        public decimal TotalCost { get; set; }

        public decimal CostPerEmployee { get; set; }

        public decimal ROI { get; set; }
         
        public string CourseDistribution { get; set; } 
        public string SkillsImprovement { get; set; }
         
    }

    /// <summary>
    /// Predictive Insights Table
    /// </summary>
    [SugarTable("hr_predictive_insights")]
    public class HrPredictiveInsights : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        [JsonConverter(typeof(ValueToStringConverter))] 
        public long InsightId { get; set; }

        public DateTime GeneratedDate { get; set; }

        public decimal PredictedTurnoverRate { get; set; }
         
        public string FlightRiskEmployees { get; set; }

        
        public string PredictedHiringNeeds { get; set; }

        
        public string SkillGaps { get; set; }

        
        public string RecommendedTrainings { get; set; }

        
        public string SalaryAdjustmentRecommendations { get; set; }

        
        public string PerformanceTrends { get; set; }

        
        public string RetentionStrategies { get; set; }

        public decimal ConfidenceScore { get; set; }
         
    }

    /// <summary>
    /// Job Position Table for Analytics
    /// </summary>
    [SugarTable("hr_job_position")]
    public class HrJobPosition : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
         
        public long PositionId { get; set; }

        [SugarColumn(Length = 100)]
        public string PositionCode { get; set; }

        [SugarColumn(Length = 200)]
        public string PositionTitle { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [SugarColumn(Length = 20)]
        public string Level { get; set; }

        public decimal MinSalary { get; set; }

        public decimal MaxSalary { get; set; }

        
        public string RequiredSkills { get; set; }

        
        public string JobDescription { get; set; }

        public int HeadCount { get; set; }

        public int CurrentCount { get; set; }

        public bool IsActive { get; set; } 
    }

    /// <summary>
    /// Payroll History Table for Analytics
    /// </summary>
    [SugarTable("hr_payroll_history")]
    public class HrPayrollHistory : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public decimal BaseSalary { get; set; }

        public decimal TotalCompensation { get; set; }

        public DateTime EffectiveDate { get; set; }

        [SugarColumn(Length = 50)]
        public string ChangeType { get; set; }

        public decimal ChangePercentage { get; set; }

        [SugarColumn(Length = 500)]
        public string Reason { get; set; }

        [SugarColumn(Length = 50)]
        public string ApprovedBy { get; set; } 
    }

    /// <summary>
    /// Dashboard Metrics Table
    /// </summary>
    [SugarTable("hr_dashboard_metrics")]
    public class HrDashboardMetrics : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        [JsonConverter(typeof(ValueToStringConverter))]
        public long MetricsId { get; set; }

        [SugarColumn(Length = 50)]
        public string DashboardType { get; set; } // employee, manager, hr, executive

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? UserId { get; set; }

        [SugarColumn(Length = 100)]
        public string MetricName { get; set; }

        public decimal MetricValue { get; set; }

        [SugarColumn(Length = 20)]
        public string MetricUnit { get; set; }

        [SugarColumn(Length = 20)]
        public string Trend { get; set; } // up, down, stable

        public decimal ChangePercentage { get; set; }

        public DateTime CalculatedDate { get; set; }

        
        public string Details { get; set; }
 
    }

    /// <summary>
    /// KPI Dashboard Table
    /// </summary>
    [SugarTable("hr_kpi_dashboard")]
    public class HrKpiDashboard : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        [JsonConverter(typeof(ValueToStringConverter))]
        public long KpiId { get; set; }

        [SugarColumn(Length = 100)]
        public string KpiName { get; set; }

        [SugarColumn(Length = 50)]
        public string Category { get; set; }

        public decimal Target { get; set; }

        public decimal Actual { get; set; }

        public decimal Achievement { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // on_track, at_risk, off_track

        [SugarColumn(Length = 20)]
        public string Period { get; set; }

        public int Year { get; set; }

        public int? Month { get; set; }

        public int? Quarter { get; set; }

        
        public string Breakdown { get; set; }
 

    }
}

