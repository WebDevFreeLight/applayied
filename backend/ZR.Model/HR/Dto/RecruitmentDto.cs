 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Requisition Query DTO
    /// </summary>
    public class RequisitionQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string JobTitle { get; set; }
    }

    /// <summary>
    /// Job Requisition DTO
    /// </summary>
    public class JobRequisitionDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequisitionId { get; set; }
        
        [Required(ErrorMessage = "Position is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? PositionId { get; set; }
        
        [Required(ErrorMessage = "Department is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        
        [Required(ErrorMessage = "Number of openings is required")]
        [Range(1, 100)]
        public int NumberOfOpenings { get; set; }
        
        [Required(ErrorMessage = "Job title is required")]
        public string JobTitle { get; set; }
        
        public string JobDescription { get; set; }
        public string Requirements { get; set; }
        public string PreferredQualifications { get; set; }
        public string EmploymentType { get; set; } // full_time, part_time, contract
        public string ExperienceLevel { get; set; } // entry, mid, senior, executive
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public string Location { get; set; }
        public bool IsRemote { get; set; }
        public string Priority { get; set; } // low, medium, high, urgent
        public DateTime? TargetStartDate { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequestedBy { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // draft, pending_approval, approved, published, closed
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string ClosureReason { get; set; }
        
        // Additional properties
        public string JobCode { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReportingTo { get; set; }
        public int NumberOfPositions { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string ExperienceRequired { get; set; }
        public string EducationLevel { get; set; }
        public string Skills { get; set; }
        public string Create_by { get; set; }
    }

    /// <summary>
    /// Candidate Query DTO
    /// </summary>
    public class CandidateQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RequisitionId { get; set; }
        public string Status { get; set; }
        public string Stage { get; set; }
        public string Source { get; set; }
        public DateTime? AppliedFrom { get; set; }
        public DateTime? AppliedTo { get; set; }
        public string Skills { get; set; }
        public int? MinExperience { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// Candidate DTO
    /// </summary>
    public class CandidateDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }
        
        public string AlternatePhone { get; set; }
        public string CurrentCompany { get; set; }
        public string CurrentPosition { get; set; }
        public int YearsOfExperience { get; set; }
        public string HighestEducation { get; set; }
        public string University { get; set; }
        public string Major { get; set; }
        public string Skills { get; set; }
        public decimal? ExpectedSalary { get; set; }
        public int? NoticePeriodDays { get; set; }
        public string Source { get; set; } // website, referral, agency, linkedin, etc
        public string ReferredBy { get; set; }
        public string ResumeUrl { get; set; }
        public string LinkedInProfile { get; set; }
        public string PortfolioUrl { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } // new, screening, interview, offer, hired, rejected
        public string Stage { get; set; } // applied, phone_screen, technical, hr, final, offer
        public decimal? Rating { get; set; }
        public string Notes { get; set; }
        public bool IsInTalentPool { get; set; }
        public string TalentPoolCategory { get; set; }
        public IFormFile ResumeFile { get; set; }
        
        // Additional properties
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RequisitionId { get; set; }
        public int? TotalExperience { get; set; }
        public decimal? CurrentSalary { get; set; }
        public int? NoticePeriod { get; set; }
    }

    /// <summary>
    /// Interview Query DTO
    /// </summary>
    public class InterviewQueryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? CandidateId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InterviewerId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public string InterviewType { get; set; }
    }

    /// <summary>
    /// Interview DTO
    /// </summary>
    public class InterviewDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InterviewId { get; set; }
        
        [Required(ErrorMessage = "Candidate is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }
        
        [Required(ErrorMessage = "Requisition is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequisitionId { get; set; }
        
        [Required(ErrorMessage = "Interview date is required")]
        public DateTime InterviewDate { get; set; }
        
        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }
        
        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }
        
        [Required(ErrorMessage = "Interview type is required")]
        public string InterviewType { get; set; } // phone, video, in_person, technical, hr
        
        public string InterviewRound { get; set; } // first, second, final
        public List<long> InterviewerIds { get; set; }
        public string Location { get; set; }
        public string MeetingLink { get; set; }
        public string Agenda { get; set; }
        public string Instructions { get; set; }
        public string Status { get; set; } // scheduled, completed, cancelled, no_show
        public string CandidateName { get; set; }
        public string InterviewerNames { get; set; }
        public string JobTitle { get; set; }
        
        // Additional properties for feedback
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InterviewerId { get; set; }
        public int? Duration { get; set; }
        public string Feedback { get; set; }
        public decimal? Rating { get; set; }
        public string Recommendation { get; set; }
    }

    /// <summary>
    /// Interview Feedback DTO
    /// </summary>
    public class InterviewFeedbackDto
    {
        [Required(ErrorMessage = "Overall rating is required")]
        [Range(1, 5)]
        public int OverallRating { get; set; }
        
        public int? TechnicalSkills { get; set; }
        public int? CommunicationSkills { get; set; }
        public int? ProblemSolving { get; set; }
        public int? CulturalFit { get; set; }
        public int? Leadership { get; set; }
        public int? Teamwork { get; set; }
        
        [Required(ErrorMessage = "Recommendation is required")]
        public string Recommendation { get; set; } // strong_hire, hire, no_hire, strong_no_hire
        
        public string Strengths { get; set; }
        public string Weaknesses { get; set; }
        public string Comments { get; set; }
        public string QuestionsAsked { get; set; }
        public string CandidateQuestions { get; set; }
        public string Feedback { get; set; }
        public int? Rating { get; set; }
    }

    /// <summary>
    /// Job Offer DTO
    /// </summary>
    public class JobOfferDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OfferId { get; set; }
        
        [Required(ErrorMessage = "Candidate is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }
        
        [Required(ErrorMessage = "Requisition is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequisitionId { get; set; }
        
        [Required(ErrorMessage = "Position is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PositionId { get; set; }
        
        [Required(ErrorMessage = "Department is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DepartmentId { get; set; }
        
        [Required(ErrorMessage = "Basic salary is required")]
        public decimal BasicSalary { get; set; }
        
        public decimal? HousingAllowance { get; set; }
        public decimal? TransportAllowance { get; set; }
        public decimal? OtherAllowances { get; set; }
        public decimal? SigningBonus { get; set; }
        public string Benefits { get; set; }
        public int ProbationPeriodMonths { get; set; }
        
        [Required(ErrorMessage = "Start date is required")]
        public DateTime ProposedStartDate { get; set; }
        
        public DateTime OfferDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; } // draft, sent, accepted, rejected, negotiating, expired
        public DateTime? ResponseDate { get; set; }
        public string ResponseComments { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string Terms { get; set; }
        public string OfferLetterUrl { get; set; }
        
        // Additional properties
        public string PositionTitle { get; set; }
        public string Department { get; set; }
        public decimal? TotalPackage { get; set; }
        public DateTime? ValidUntil { get; set; }
    }

    /// <summary>
    /// Recruitment Pipeline DTO
    /// </summary>
    public class RecruitmentPipelineDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequisitionId { get; set; }
        public string JobTitle { get; set; }
        public int TotalCandidates { get; set; }
        public Dictionary<string, List<PipelineCandidate>> StageBreakdown { get; set; }
        public int Hired { get; set; }
        public int Rejected { get; set; }
        public int InProcess { get; set; }
    }

    public class PipelineCandidate
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }
        public string CandidateName { get; set; }
        public DateTime StageDate { get; set; }
        public int DaysInStage { get; set; }
    }

    /// <summary>
    /// Candidate Stage History DTO
    /// </summary>
    public class CandidateStageHistoryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long HistoryId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequisitionId { get; set; }
        public string Stage { get; set; }
        public DateTime EnteredDate { get; set; }
        public DateTime? ExitedDate { get; set; }
        public string Notes { get; set; }
        public string UpdatedBy { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Recruitment Metrics DTO
    /// </summary>
    public class RecruitmentMetricsDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalRequisitions { get; set; }
        public int OpenPositions { get; set; }
        public int TotalApplications { get; set; }
        public int TotalInterviews { get; set; }
        public int TotalOffers { get; set; }
        public int TotalHires { get; set; }
        public decimal OfferAcceptanceRate { get; set; }
        public decimal AverageTimeToHire { get; set; }
        public decimal AverageCostPerHire { get; set; }
        public Dictionary<string, int> HiresByDepartment { get; set; }
        public Dictionary<string, int> HiresBySource { get; set; }
        public int TotalCandidates { get; set; }
        public int AcceptedOffers { get; set; }
        public int RejectedOffers { get; set; }
    }

    /// <summary>
    /// Source Analytics DTO
    /// </summary>
    public class SourceAnalyticsDto
    {
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public List<SourcePerformance> Sources { get; set; }
        public string BestSource { get; set; }
        public decimal BestSourceConversionRate { get; set; }
    }

    public class SourcePerformance
    {
        public string Source { get; set; }
        public int Applications { get; set; }
        public int Interviews { get; set; }
        public int Offers { get; set; }
        public int Hires { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal QualityScore { get; set; }
    }

    /// <summary>
    /// Time to Hire DTO
    /// </summary>
    public class TimeToHireDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int Months { get; set; }
        public decimal AverageDays { get; set; }
        public decimal MedianDays { get; set; }
        public Dictionary<string, decimal> MonthlyAverage { get; set; }
        public Dictionary<string, decimal> ByPosition { get; set; }
        public Dictionary<string, decimal> ByLevel { get; set; }
        public int MinDays { get; set; }
        public int MaxDays { get; set; }
    }

    /// <summary>
    /// Interviewer Performance DTO
    /// </summary>
    public class InterviewerPerformanceDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InterviewerId { get; set; }
        public string InterviewerName { get; set; }
        public int TotalInterviews { get; set; }
        public int CandidatesHired { get; set; }
        public int CandidatesRejected { get; set; }
        public decimal AccuracyRate { get; set; } // How often their recommendation matched final decision
        public decimal AverageRating { get; set; } // Average rating given to candidates
        public Dictionary<string, int> RecommendationBreakdown { get; set; }
        public List<string> TopSkillsEvaluated { get; set; }
        public decimal FeedbackCompletionRate { get; set; }
        public decimal AverageFeedbackTime { get; set; } // Hours to submit feedback
        public int CompletedInterviews { get; set; }
        public int ActualHires { get; set; }
        public int RecommendedHires { get; set; }
    }

    /// <summary>
    /// Onboarding Checklist DTO
    /// </summary>
    public class OnboardingChecklistDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }
        public string CandidateName { get; set; }
        public DateTime JoiningDate { get; set; }
        public List<OnboardingTask> Tasks { get; set; } = new() { };

        public decimal CompletionPercentage { get; set; }
        //public decimal CompletionRate => CompletionPercentage;

        public decimal CompletionRate { get; set; } // pending, in_progress, completed
        //public decimal CompletionRate => CompletionPercentage;




        public string Status { get; set; } // pending, in_progress, completed
    }

    public class OnboardingTask
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TaskId { get; set; }
        public string TaskName { get; set; }
        public string Category { get; set; } // documents, it_setup, training, orientation
        public string AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool Completed => IsCompleted;
        public string Notes { get; set; }
    }
}

