
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Performance
{
    /// <summary>
    /// Performance Review Table
    /// </summary>
    [SugarTable("hr_performance_review")]
    public class HrPerformanceReview : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = false)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewerId { get; set; }

        public DateTime ReviewPeriodStart { get; set; }

        public DateTime ReviewPeriodEnd { get; set; }

        public decimal? OverallRating { get; set; } // 1.00 to 5.00

        public decimal? ProductivityRating { get; set; }

        public decimal? QualityRating { get; set; }

        public decimal? AttendanceRating { get; set; }

        public decimal? TeamworkRating { get; set; }

        public decimal? LeadershipRating { get; set; }

        [SugarColumn(Length = 1000)]
        public string Strengths { get; set; }

        [SugarColumn(Length = 1000)]
        public string Improvements { get; set; }

        [SugarColumn(Length = 1000)]
        public string Goals { get; set; }

        [SugarColumn(Length = 1000)]
        public string Comments { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // draft, submitted, approved

        public DateTime? ReviewDate { get; set; }

        public DateTime? DueDate { get; set; }


        public int ReviewYear { get; set; }
        public int ReviewQuarter { get; set; }
        public string ReviewType { get; set; }
        public string ReviewPeriod { get; set; }
        public decimal? KnowledgeRating { get; set; }
        public decimal? CommunicationRating { get; set; }
        public decimal? InitiativeRating { get; set; }
        public string AreasForImprovement { get; set; }
        public string GoalsAchieved { get; set; }
        public string TrainingNeeds { get; set; }
        public string CareerAspirations { get; set; }
        public string ManagerComments { get; set; }
        public string EmployeeComments { get; set; }
        public DateTime SubmittedDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime SelfReviewDate { get; set; }
        public string DevelopmentAreas { get; set; }
        public string Category { get; set; }














































          

            /// <summary>
            /// Duedate 
            /// </summary>
            public DateTime? Duedate { get; set; }

            /// <summary>
            /// Reviewyear 
            /// </summary>
            public int? Reviewyear { get; set; }

            /// <summary>
            /// Reviewdate 
            /// </summary>
            public DateTime? Reviewdate { get; set; }

            /// <summary>
            /// Reviewquarter 
            /// </summary>
            public int? Reviewquarter { get; set; }

            /// <summary>
            /// Reviewtype 
            /// </summary>
            public string Reviewtype { get; set; }

            /// <summary>
            /// Reviewperiod 
            /// </summary>
            public string Reviewperiod { get; set; }

            /// <summary>
            /// Knowledgerating 
            /// </summary>
            public float Knowledgerating { get; set; }


            /// <summary>
            /// Employeeid 
            /// </summary>
            public long? Employeeid { get; set; }

            /// <summary>
            /// Reviewerid 
            /// </summary>
            public long? Reviewerid { get; set; }

            /// <summary>
            /// Reviewperiodstart 
            /// </summary>
            public DateTime? Reviewperiodstart { get; set; }

            /// <summary>
            /// Reviewperiodend 
            /// </summary>
            public DateTime? Reviewperiodend { get; set; }

            /// <summary>
            /// Overallrating 
            /// </summary>
            public float Overallrating { get; set; }

            /// <summary>
            /// Productivityrating 
            /// </summary>
            public float Productivityrating { get; set; }

            /// <summary>
            /// Qualityrating 
            /// </summary>
            public float Qualityrating { get; set; }

            /// <summary>
            /// Attendancerating 
            /// </summary>
            public float Attendancerating { get; set; }

            /// <summary>
            /// Teamworkrating 
            /// </summary>
            public float Teamworkrating { get; set; }

            /// <summary>
            /// Leadershiprating 
            /// </summary>
            public float Leadershiprating { get; set; }
 

            /// <summary>
            /// Communicationrating 
            /// </summary>
            public float Communicationrating { get; set; }

            /// <summary>
            /// Initiativerating 
            /// </summary>
            public float Initiativerating { get; set; }

            /// <summary>
            /// Areasforimprovement 
            /// </summary>
            public string Areasforimprovement { get; set; }

            /// <summary>
            /// Goalsachieved 
            /// </summary>
            public string Goalsachieved { get; set; }

            /// <summary>
            /// Trainingneeds 
            /// </summary>
            public string Trainingneeds { get; set; }

            /// <summary>
            /// Careeraspirations 
            /// </summary>
            public string Careeraspirations { get; set; }

            /// <summary>
            /// Managercomments 
            /// </summary>
            public string Managercomments { get; set; }

            /// <summary>
            /// Employeecomments 
            /// </summary>
            public string Employeecomments { get; set; }

            /// <summary>
            /// Submitteddate 
            /// </summary>
            public DateTime? Submitteddate { get; set; }

            /// <summary>
            /// Approveddate 
            /// </summary>
            public DateTime? Approveddate { get; set; }

            /// <summary>
            /// Approvedby 
            /// </summary>
            public string Approvedby { get; set; }

            /// <summary>
            /// Selfreviewdate 
            /// </summary>
            public DateTime? Selfreviewdate { get; set; }

            /// <summary>
            /// Developmentareas 
            /// </summary>
            public string Developmentareas { get; set; }

         


        
    }

    /// <summary>
    /// Performance Review Goals Table (alias for HrPerformanceGoal)
    /// </summary>
    [SugarTable("hr_performance_goal")]
    public class HrPerformanceReviewGoal : HrPerformanceGoal
    {
    }

    /// <summary>
    /// Performance Goals Table
    /// </summary>
    [SugarTable("hr_performance_goal")]
    public class HrPerformanceGoal : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long GoalId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 200)]
        public string GoalTitle { get; set; }

        [SugarColumn(Length = 1000)]
        public string GoalDescription { get; set; }

        public DateTime? TargetDate { get; set; }

        public int CompletionPercentage { get; set; } = 0;

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // active, completed, cancelled

        [SugarColumn(Length = 20)]
        public string Priority { get; set; } // high, medium, low

        [SugarColumn(Length = 500)]
        public string Metrics { get; set; }

        [SugarColumn(Length = 500)]
        public string Achievements { get; set; }

 
         
        public DateTime? StartDate { get; set; }
        public int Year { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int? Quarter { get; set; }
        public decimal TargetValue { get; set; }
        public string TargetUnit { get; set; }
        public int CurrentProgress { get; set; }
        public decimal Weight { get; set; }
        public DateTime CompletedDate { get; set; }
        public decimal AchievedValue { get; set; }
        public decimal AchievementScore { get; set; }
        public string Comments { get; set; }
        public int Progress { get; set; }
        public DateTime DueDate { get; set; }
        public string SetBy { get; set; }
        public DateTime CompletionDate { get; set; }
        public string EmployeeNotes { get; set; }
         
        public string ProgressNotes { get; set; }
        public DateTime? LastProgressUpdate { get; set; }
        public string CompletionNotes { get; set; }
         







    }

    /// <summary>
    /// KPI Metrics Table
    /// </summary>
    [SugarTable("hr_kpi_metric")]
    public class HrKpiMetric : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MetricId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 100)]
        public string MetricName { get; set; }

        [SugarColumn(Length = 20)]
        public string MetricType { get; set; } // quantity, quality, efficiency, behavior

        public decimal Target { get; set; }

        public decimal Actual { get; set; }

        [SugarColumn(Length = 20)]
        public string Unit { get; set; }

        public DateTime MeasurementDate { get; set; }

        public decimal Score { get; set; }

        [SugarColumn(Length = 500)]
        public string Comments { get; set; }

     
        public bool IsActive { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DepartmentId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PositionId { get; set; }
        public string MetricNameAr { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string MeasurementUnit { get; set; }
        public string Frequency { get; set; }
        public decimal TargetValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public decimal Weight { get; set; }
     
    }

    /// <summary>
    /// Performance Improvement Plan Table
    /// </summary>
    [SugarTable("hr_performance_improvement_plan")]
    public class HrPerformanceImprovementPlan : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PipId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [SugarColumn(Length = 500)]
        public string Reason { get; set; }
        
        public string ImprovementAreas { get; set; }
        
        public string SpecificGoals { get; set; }
        
        public string SupportProvided { get; set; }
        
        public string MilestoneChecks { get; set; }
        
        public string ConsequencesOfFailure { get; set; }
        public decimal CompletionPercentage { get; set; }
        
        public string ProgressUpdates { get; set; }
        [SugarColumn(Length = 20)]
        public string Status { get; set; }
        public DateTime? CompletedDate { get; set; }
        
        public string FinalOutcome { get; set; }
 





        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }
        public string PlanTitle { get; set; }
        public string Areas { get; set; }
        public string Goals { get; set; }
        public string Progress { get; set; }


    }

    /// <summary>
    /// KPI Record Table
    /// </summary>
    [SugarTable("hr_kpi_record")]
    public class HrKpiRecord : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RecordId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long MetricId { get; set; }
        public DateTime Period { get; set; }
        public decimal Value { get; set; }
        [SugarColumn(Length = 500)]
        public string Comments { get; set; }
      
    }

    /// <summary>
    /// 360 Feedback Table
    /// </summary>
    [SugarTable("hr_360_feedback")]
    public class Hr360Feedback : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long FeedbackId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewerId { get; set; }
        [SugarColumn(Length = 50)]
        public string ReviewPeriod { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        [SugarColumn(Length = 20)]
        public string Relationship { get; set; }
        public decimal? OverallRating { get; set; }
        public decimal? TechnicalSkillsRating { get; set; }
        public decimal? CommunicationRating { get; set; }
        public decimal? TeamworkRating { get; set; }
        public decimal? LeadershipRating { get; set; }
        public decimal? ProblemSolvingRating { get; set; }
        [SugarColumn(Length = 1000)]
        public string Strengths { get; set; }
        [SugarColumn(Length = 1000)]
        public string ImprovementAreas { get; set; }
        [SugarColumn(Length = 1000)]
        public string Comments { get; set; }
        [SugarColumn(Length = 20)]
        public string Status { get; set; }
        [SugarColumn(Length = 50)]
        public string Create_by { get; set; }
        public DateTime Create_time { get; set; }
        public DateTime? Update_time { get; set; }


        [JsonConverter(typeof(ValueToStringConverter))]
        public long ReviewId { get; set; }
        public decimal Rating { get; set; }
        public string Feedback { get; set; }



    }
}

