 
using Infrastructure.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ZR.Model.System;

namespace ZR.Model.HR.Dto
{
    /// <summary>
    /// Benefits Plan Query DTO
    /// </summary>
    public class BenefitsPlanQueryDto : PagerInfo
    {
        public string PlanName { get; set; }
        public string PlanType { get; set; }
        public string Provider { get; set; }
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Benefits Plan DTO
    /// </summary>
    public class BenefitsPlanDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }
        
        [Required(ErrorMessage = "Plan code is required")]
        public string PlanCode { get; set; }
        
        [Required(ErrorMessage = "Plan name is required")]
        public string PlanName { get; set; }
        
        public string PlanNameAr { get; set; }
        
        [Required(ErrorMessage = "Plan type is required")]
        public string PlanType { get; set; } // medical, dental, life, disability
        
        public string Provider { get; set; }
        public string Description { get; set; }
        public string CoverageDetails { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal EmployeeContribution { get; set; }
        public string EligibilityCriteria { get; set; }
        public int WaitingPeriodDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// Employee Benefits Enrollment DTO
    /// </summary>
    public class BenefitsEnrollmentDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Plan ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }
        
        public DateTime EnrollmentDate { get; set; }
        public DateTime? TerminationDate { get; set; }
        public string Status { get; set; } // active, terminated, pending
        public decimal CoverageAmount { get; set; }
        public decimal MonthlyPremium { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryRelation { get; set; }
        public string BeneficiaryContact { get; set; }
    }

    /// <summary>
    /// Employee Benefits Summary DTO
    /// </summary>
    public class EmployeeBenefitsSummaryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public List<EnrolledBenefitDto> EnrolledBenefits { get; set; }
        public decimal TotalMonthlyPremium { get; set; }
        public decimal EmployerContribution { get; set; }
        public decimal EmployeeContribution { get; set; }
        public List<BenefitClaimDto> RecentClaims { get; set; }
    }

    /// <summary>
    /// Enrolled Benefit DTO
    /// </summary>
    public class EnrolledBenefitDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EnrollmentId { get; set; }
        public string PlanName { get; set; }
        public string PlanType { get; set; }
        public string Provider { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; }
        public decimal CoverageAmount { get; set; }
        public decimal MonthlyPremium { get; set; }
    }

    /// <summary>
    /// Benefit Claim DTO
    /// </summary>
    public class BenefitClaimDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long ClaimId { get; set; }
        
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Plan ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long PlanId { get; set; }
        
        [Required(ErrorMessage = "Claim amount is required")]
        public decimal ClaimAmount { get; set; }
        
        public DateTime ClaimDate { get; set; }
        public string ClaimType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // pending, approved, rejected, paid
        public decimal? ApprovedAmount { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string RejectionReason { get; set; }
        public string SupportingDocuments { get; set; }
    }

    /// <summary>
    /// Loan Request DTO
    /// </summary>
    public class LoanRequestDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long LoanId { get; set; }
        
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Loan type is required")]
        public string LoanType { get; set; } // personal, housing, emergency
        
        [Required(ErrorMessage = "Loan amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Loan amount must be positive")]
        public decimal LoanAmount { get; set; }
        
        [Required(ErrorMessage = "Repayment months is required")]
        [Range(1, 120, ErrorMessage = "Repayment period must be between 1 and 120 months")]
        public int RepaymentMonths { get; set; }
        
        public decimal InterestRate { get; set; }
        public string Purpose { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // pending, approved, rejected, active, completed
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? DisbursementDate { get; set; }
        public decimal MonthlyDeduction { get; set; }
        public decimal TotalRepaid { get; set; }
        public decimal OutstandingBalance { get; set; }
    }

    /// <summary>
    /// Advance Salary Request DTO
    /// </summary>
    public class AdvanceSalaryDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long AdvanceId { get; set; }
        
        [Required(ErrorMessage = "Employee ID is required")]
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EmployeeId { get; set; }
        
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }
        
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
        
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // pending, approved, rejected, paid
        public DateTime? ApprovalDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// Benefits Analytics DTO
    /// </summary>
    public class BenefitsAnalyticsDto
    {
        public int TotalPlans { get; set; }
        public int ActiveEnrollments { get; set; }
        public decimal TotalMonthlyPremiums { get; set; }
        public decimal TotalEmployerContribution { get; set; }
        public decimal TotalEmployeeContribution { get; set; }
        public Dictionary<string, int> EnrollmentsByPlanType { get; set; }
        public Dictionary<string, decimal> CostByPlanType { get; set; }
        public decimal AverageClaimAmount { get; set; }
        public int PendingClaims { get; set; }
        public decimal ClaimApprovalRate { get; set; }
        public List<PlanUtilizationDto> PlanUtilization { get; set; }
    }

    /// <summary>
    /// Plan Utilization DTO
    /// </summary>
    public class PlanUtilizationDto
    {
        public string PlanName { get; set; }
        public int EnrolledEmployees { get; set; }
        public decimal UtilizationRate { get; set; }
        public decimal TotalClaims { get; set; }
    }
}

