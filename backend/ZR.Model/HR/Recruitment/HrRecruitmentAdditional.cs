
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Recruitment
{
    /// <summary>
    /// Recruitment Main Table (using existing model with additional properties)
    /// </summary>
    [SugarTable("hr_recruitment")]
    public class HrRecruitment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RecruitmentId { get; set; }

        [SugarColumn(Length = 200)]
        public string JobTitle { get; set; }

        [SugarColumn(Length = 50)]
        public string JobCode { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReportingTo { get; set; }

        public int NumberOfPositions { get; set; } = 1;

        
        public string JobDescription { get; set; }

        
        public string Requirements { get; set; }

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        [SugarColumn(Length = 50)]
        public string EmploymentType { get; set; }

        [SugarColumn(Length = 200)]
        public string Location { get; set; }

        [SugarColumn(Length = 50)]
        public string ExperienceRequired { get; set; }

        [SugarColumn(Length = 100)]
        public string EducationLevel { get; set; }

        [SugarColumn(Length = 500)]
        public string Skills { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // draft, approved, published, closed

        [SugarColumn(Length = 20)]
        public string Priority { get; set; } // low, medium, high, critical

        public DateTime? TargetStartDate { get; set; }

        [SugarColumn(Length = 50)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? PublishedDate { get; set; }

        [SugarColumn(Length = 500)]
        public string JobBoards { get; set; }

        public DateTime? ClosedDate { get; set; }

        [SugarColumn(Length = 500)]
        public string ClosedReason { get; set; }

     
    }

   
 

    /// <summary>
    /// Candidate Stage History Table
    /// </summary>
    [SugarTable("hr_candidate_history")]
    public class HrCandidateHistory : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }

        [SugarColumn(Length = 50)]
        public string Stage { get; set; }

        [SugarColumn(Length = 50)]
        public string Status { get; set; }

        [SugarColumn(Length = 500)]
        public string Notes { get; set; }

       
    }

    /// <summary>
    /// Offer Negotiation Table
    /// </summary>
    [SugarTable("hr_offer_negotiation")]
    public class HrOfferNegotiation : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long NegotiationId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long OfferId { get; set; }

        public decimal ProposedSalary { get; set; }

        [SugarColumn(Length = 500)]
        public string Comments { get; set; }

     
    }

    /// <summary>
    /// Onboarding Task Table
    /// </summary>
    [SugarTable("hr_onboarding_task")]
    public class HrOnboardingTask : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }

        [SugarColumn(Length = 200)]
        public string TaskName { get; set; }

        [SugarColumn(Length = 500)]
        public string Description { get; set; }

        [SugarColumn(Length = 100)]
        public string AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }

        public bool Completed { get; set; }

        public DateTime? CompletedDate { get; set; }

        [SugarColumn(Length = 500)]
        public string Notes { get; set; }

        public int Sequence { get; set; }

  
    }

    /// <summary>
    /// Talent Pool Table
    /// </summary>
    [SugarTable("hr_talent_pool")]
    public class HrTalentPool : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PoolId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }

        [SugarColumn(Length = 50)]
        public string Category { get; set; }

        [SugarColumn(Length = 500)]
        public string Notes { get; set; }

        public DateTime AddedDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime? LastContactDate { get; set; }

   
    }

    /// <summary>
    /// Candidate Communication Table
    /// </summary>
    [SugarTable("hr_candidate_communication")]
    public class HrCandidateCommunication : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CommunicationId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }

        [SugarColumn(Length = 20)]
        public string Type { get; set; } // email, phone, sms

        [SugarColumn(Length = 200)]
        public string Subject { get; set; }

        
        public string Message { get; set; }

        public DateTime SentDate { get; set; }

        [SugarColumn(Length = 50)]
        public string SentBy { get; set; } 
    }
}

