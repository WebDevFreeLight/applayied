 
using Infrastructure.Converter;
using SqlSugar;
using System;
using ZR.Model.System;
using Newtonsoft.Json;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Model.HR
{
    /// <summary>
    /// Benefit Plan Table
    /// </summary>
    [SugarTable("hr_benefit_plan")]
    public class HrBenefitPlan : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }



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

        
        public string Description { get; set; }

        
        public string Coverage { get; set; }

        public decimal EmployerContribution { get; set; }

        public decimal EmployeeContribution { get; set; }

        public decimal MaxCoverage { get; set; }

        public decimal Deductible { get; set; }

        [SugarColumn(Length = 20)]
        public string EligibilityPeriod { get; set; }

        public int WaitingPeriodDays { get; set; }

        public bool IsMandatory { get; set; }

        public bool IsActive { get; set; }
        
        [SugarColumn(Length = 20)]
        public string Status { get; set; } // active, inactive, suspended

        public DateTime EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Budget Allocation (FK -> finance_budget_lines.id)
        /// </summary>
        public Guid? BudgetAllocationId { get; set; }

        /// <summary>
        /// Finance AR Invoice (FK -> arap_ar_invoice.id)
        /// </summary>
        public Guid? ARInvoiceId { get; set; }

    }

    /// <summary>
    /// Benefit Enrollment Table
    /// </summary>
    [SugarTable("hr_benefit_enrollment")]
    public class HrBenefitEnrollment : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }

        public DateTime EnrollmentDate { get; set; }

        public DateTime CoverageStartDate { get; set; }

        public DateTime? CoverageEndDate { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // active, pending, terminated, suspended

        public decimal EmployeeContribution { get; set; }

        public decimal EmployerContribution { get; set; }

        [SugarColumn(Length = 20)]
        public string PaymentFrequency { get; set; } // monthly, quarterly, annually

        
        public string Beneficiaries { get; set; } // JSON string of beneficiaries

        
        public string Dependents { get; set; } // JSON string of dependents

        [SugarColumn(Length = 500)]
        public string TerminationReason { get; set; }

        public DateTime? TerminationDate { get; set; }

        /// <summary>
        /// Budget Allocation (FK -> finance_budget_lines.id)
        /// </summary>
        public Guid? BudgetAllocationId { get; set; }

        /// <summary>
        /// Finance AR Invoice (FK -> arap_ar_invoice.id)
        /// </summary>
        public Guid? ARInvoiceId { get; set; }

    }

    /// <summary>
    /// Benefit Claim Table
    /// </summary>
    [SugarTable("hr_benefit_claim")]
    public class HrBenefitClaim : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ClaimId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 100)]
        public string ClaimNumber { get; set; }

        [SugarColumn(Length = 50)]
        public string ClaimType { get; set; }

        public DateTime ClaimDate { get; set; }

        public DateTime? ServiceDate { get; set; }

        public decimal ClaimAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public decimal RejectedAmount { get; set; }

        public decimal PaidAmount { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; } // submitted, under_review, approved, rejected, paid

        [SugarColumn(Length = 500)]
        public string Description { get; set; }

        [SugarColumn(Length = 500)]
        public string Documents { get; set; } // File paths

        [SugarColumn(Length = 500)]
        public string RejectionReason { get; set; }

        [SugarColumn(Length = 50)]
        public string ReviewedBy { get; set; }

        public DateTime? ReviewDate { get; set; }

        [SugarColumn(Length = 50)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public DateTime? PaymentDate { get; set; }

        [SugarColumn(Length = 100)]
        public string PaymentReference { get; set; }

        /// <summary>
        /// Treasury Payment ID (FK -> treasury_payment.id)
        /// </summary>
        public Guid? TreasuryPaymentId { get; set; }

        /// <summary>
        /// ZATCA submission status for claim payment (enum)
        /// </summary>
        public ZatcaSubmissionStatus ZatcaStatus { get; set; } = ZatcaSubmissionStatus.NotSubmitted;

    }


  


        /// <summary>
        /// Employee Dependent Table
        /// </summary>
        [SugarTable("hr_employee_dependent")]
    public class HrEmployeeDependent : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long DependentId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 100)]
        public string FirstName { get; set; }

        [SugarColumn(Length = 100)]
        public string LastName { get; set; }

        [SugarColumn(Length = 20)]
        public string Relationship { get; set; } // spouse, child, parent

        public DateTime DateOfBirth { get; set; }

        [SugarColumn(Length = 10)]
        public string Gender { get; set; }

        [SugarColumn(Length = 50)]
        public string NationalId { get; set; }

        [SugarColumn(Length = 20)]
        public string MaritalStatus { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsStudent { get; set; }

        public bool IsCoveredByBenefits { get; set; }

        [SugarColumn(Length = 500)]
        public string MedicalConditions { get; set; }

        [SugarColumn(Length = 500)]
        public string Documents { get; set; }

        public bool IsActive { get; set; }

    }

    /// <summary>
    /// Retirement Plan Table
    /// </summary>
    [SugarTable("hr_retirement_plan")]
    public class HrRetirementPlan : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long RetirementId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        [SugarColumn(Length = 50)]
        public string PlanType { get; set; } // 401k, pension, savings

        public DateTime EnrollmentDate { get; set; }

        public decimal EmployeeContributionPercentage { get; set; }

        public decimal EmployerMatchPercentage { get; set; }

        public decimal VestingPercentage { get; set; }

        public DateTime? VestingDate { get; set; }

        public decimal CurrentBalance { get; set; }

        public decimal YearToDateContribution { get; set; }

        public decimal LifetimeContribution { get; set; }

        [SugarColumn(Length = 500)]
        public string InvestmentOptions { get; set; }

        [SugarColumn(Length = 500)]
        public string Beneficiaries { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; }
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }

        /// <summary>
        /// Finance GL Account (FK -> gl_account.account_id)
        /// </summary>
        public Guid? GLAccountId { get; set; }
    }

    /// <summary>
    /// Wellness Program Table
    /// </summary>
    [SugarTable("hr_wellness_program")]
    public class HrWellnessProgram : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProgramId { get; set; }



        [SugarColumn(Length = 200)]
        public string ProgramName { get; set; }

        [SugarColumn(Length = 50)]
        public string ProgramType { get; set; } // fitness, mental_health, nutrition

        
        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal Budget { get; set; }

        public int MaxParticipants { get; set; }

        public int EnrolledCount { get; set; }

        [SugarColumn(Length = 200)]
        public string Provider { get; set; }

        [SugarColumn(Length = 500)]
        public string Benefits { get; set; }

        [SugarColumn(Length = 500)]
        public string Eligibility { get; set; }

        public bool IsActive { get; set; }


    }

    /// <summary>
    /// Wellness Participation Table
    /// </summary>
    [SugarTable("hr_wellness_participation")]
    public class HrWellnessParticipation : SysBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ParticipationId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ProgramId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }

        public DateTime EnrollmentDate { get; set; }

        public DateTime? CompletionDate { get; set; }

        public decimal ProgressPercentage { get; set; }

        public int PointsEarned { get; set; }

        [SugarColumn(Length = 20)]
        public string Status { get; set; }

        [SugarColumn(Length = 500)]
        public string Achievements { get; set; }

        [SugarColumn(Length = 500)]
        public string Feedback { get; set; }

   
    }
}

