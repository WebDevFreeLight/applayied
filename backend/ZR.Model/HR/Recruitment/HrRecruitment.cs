
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;

namespace ZR.Model.HR.Recruitment
{
    /// <summary>
    /// Job Requisition Table
    /// </summary>
    [SugarTable("hr_job_requisition")]
    public class HrJobRequisition : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequisitionId { get; set; }

        [SugarColumn(Length = 200)]
        public string JobTitle { get; set; }

        [SugarColumn(Length = 200)]
        public string JobTitleAr { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? DepartmentId { get; set; }

        public int PositionsRequired { get; set; } = 1;

        
        public string JobDescription { get; set; }

        
        public string Requirements { get; set; }

        public decimal? SalaryRangeMin { get; set; }

        public decimal? SalaryRangeMax { get; set; }

        [SugarColumn(Length = 20)]
        public string EmploymentType { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // open, closed, on_hold

        public DateTime? PostingDate { get; set; }

        public DateTime? ClosingDate { get; set; }

        [SugarColumn(Length = 100)]
        public string Location { get; set; }

        [SugarColumn(Length = 20)]
        public string ExperienceLevel { get; set; } // entry, mid, senior, executive

        [SugarColumn(Length = 500)]
        public string Benefits { get; set; }
 
    }

    /// <summary>
    /// Candidates Table
    /// </summary>
    [SugarTable("hr_candidate")]
    public class HrCandidate :  SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }





        [SugarColumn(Length = 100)]
        public string CurrentCompany { get; set; }

        [SugarColumn(Length = 100)]
        public string CurrentPosition { get; set; }

        public int? TotalExperience { get; set; }

        public int? NoticePeriod { get; set; }

        [SugarColumn(Length = 500)]
        public string ResumePath { get; set; }

        [SugarColumn(Length = 20)]
        public string Stage { get; set; } // screening, phone_interview, technical_interview, hr_interview, offer, hired

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EmployeeId { get; set; } // Set when candidate is hired






        [JsonConverter(typeof(ValueToStringConverter))]
        public long? RequisitionId { get; set; }

        [SugarColumn(Length = 100)]
        public string FirstName { get; set; }

        [SugarColumn(Length = 100)]
        public string LastName { get; set; }

        [SugarColumn(Length = 100)]
        public string Email { get; set; }

        [SugarColumn(Length = 20)]
        public string Phone { get; set; }

        [SugarColumn(Length = 50)]
        public string Nationality { get; set; }

        [SugarColumn(Length = 200)]
        public string CurrentLocation { get; set; }

        public int? YearsExperience { get; set; }

        public decimal? CurrentSalary { get; set; }

        public decimal? ExpectedSalary { get; set; }

        public int? NoticePeriodDays { get; set; }

        [SugarColumn(Length = 500)]
        public string CvFilePath { get; set; }

        
        public string CoverLetter { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // new, screening, interview, offer, hired, rejected

        public int? Rating { get; set; }

        
        public string Notes { get; set; }

        [SugarColumn(Length = 50)]
        public string Source { get; set; } // website, referral, agency, linkedin

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ReferralEmployeeId { get; set; }

        [SugarColumn(Length = 100)]
        public string LinkedInProfile { get; set; }

        [SugarColumn(Length = 500)]
        public string Skills { get; set; }

        [SugarColumn(Length = 500)]
        public string Education { get; set; }

    
    }

    /// <summary>
    /// Interview Schedule Table
    /// </summary>
    [SugarTable("hr_interview")]
    public class HrInterview : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long InterviewId { get; set; }


        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequisitionId { get; set; }

        public DateTime InterviewDate { get; set; }

        [SugarColumn(Length = 20)]
        public string InterviewType { get; set; } // phone, video, in-person, technical

        [SugarColumn(Length = 20)]
        public string InterviewRound { get; set; } // first, second, final

        [SugarColumn(Length = 500)]
        public string Interviewers { get; set; }

        [SugarColumn(Length = 200)]
        public string Location { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // scheduled, completed, cancelled, no-show

        public int? Rating { get; set; }

        
        public string Feedback { get; set; }

        [SugarColumn(Length = 20)]
        public string Result { get; set; } // pass, fail, hold

        


        [JsonConverter(typeof(ValueToStringConverter))]
        public long? InterviewerId { get; set; }

        public int? Duration { get; set; } // in minutes

        [SugarColumn(Length = 20)]
        public string Recommendation { get; set; } // hire, no_hire, maybe

        [SugarColumn(Length = 500)]
        public string CancelReason { get; set; }



    }

    /// <summary>
    /// Job Offer Table
    /// </summary>
    [SugarTable("hr_job_offer")]
    public class HrJobOffer : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long OfferId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long CandidateId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long RequisitionId { get; set; }

        public decimal OfferedSalary { get; set; }

        public DateTime? JoiningDate { get; set; }

        [SugarColumn(Length = 20)]
        public string EmploymentType { get; set; }

        [SugarColumn(Length = 500)]
        public string Benefits { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // draft, sent, accepted, rejected, expired

        public DateTime? OfferDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime? ResponseDate { get; set; }

        [SugarColumn(Length = 500)]
        public string Terms { get; set; }

        [SugarColumn(Length = 500)]
        public string RejectionReason { get; set; }

     








        [SugarColumn(Length = 100)]
        public string PositionTitle { get; set; }

        [SugarColumn(Length = 100)]
        public string Department { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal? HousingAllowance { get; set; }

        public decimal? TransportAllowance { get; set; }

        public decimal? OtherAllowances { get; set; }

        public decimal? TotalPackage { get; set; }

        public DateTime? ValidUntil { get; set; }

        public DateTime? SentDate { get; set; }

        public DateTime? AcceptedDate { get; set; }

        public DateTime? RejectedDate { get; set; }

        [SugarColumn(Length = 100)]
        public string PaymentMethod { get; set; }

        [SugarColumn(Length = 100)]
        public string BankReference { get; set; }


    }
}

