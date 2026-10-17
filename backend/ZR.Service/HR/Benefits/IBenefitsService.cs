using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR;
using ZR.Model.HR.Dto;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Benefits Service Interface
    /// </summary>
    public interface IBenefitsService
    {
        // Benefit Plans
        Task<PagedInfo<HrBenefitPlan>> GetBenefitPlansAsync(Expression<Func<HrBenefitPlan, bool>> predicate, PagerInfo pager);
        Task<List<HrBenefitPlan>> GetActiveBenefitPlansAsync();
        Task<HrBenefitPlan> GetBenefitPlanByIdAsync(long planId);
        Task<int> AddBenefitPlanAsync(HrBenefitPlan plan);
        Task<int> UpdateBenefitPlanAsync(HrBenefitPlan plan);
        Task<int> DeleteBenefitPlanAsync(long planId);
        
        // Benefit Enrollment
        Task<PagedInfo<HrBenefitEnrollment>> GetEnrollmentsAsync(Expression<Func<HrBenefitEnrollment, bool>> predicate, PagerInfo pager);
        Task<List<HrBenefitEnrollment>> GetEmployeeEnrollmentsAsync(long employeeId);
        Task<int> EnrollEmployeeAsync(HrBenefitEnrollment enrollment);
        Task<long> EnrollEmployee(long employeeId, long planId);
        Task<int> UpdateEnrollmentAsync(HrBenefitEnrollment enrollment);
        Task<int> TerminateEnrollmentAsync(long enrollmentId, string reason);
        
        // Benefit Claims
        Task<PagedInfo<HrBenefitClaim>> GetBenefitClaimsAsync(Expression<Func<HrBenefitClaim, bool>> predicate, PagerInfo pager);
        Task<List<HrBenefitClaim>> GetEmployeeClaimsAsync(long employeeId);
        Task<int> SubmitClaimAsync(HrBenefitClaim claim);
        Task<int> UpdateClaimStatusAsync(long claimId, string status, string comments);
        Task<int> ApproveClaimAsync(long claimId, decimal approvedAmount, string approvedBy);
        Task<int> RejectClaimAsync(long claimId, string reason, string rejectedBy);
        Task<int> ProcessClaimPaymentAsync(long claimId, string paymentReference);
        
        // Employee Dependents
        Task<List<HrEmployeeDependent>> GetEmployeeDependentsAsync(long employeeId);
        Task<int> AddDependentAsync(HrEmployeeDependent dependent);
        Task<int> UpdateDependentAsync(HrEmployeeDependent dependent);
        Task<int> RemoveDependentAsync(long dependentId);
        
        // Retirement Plans
        Task<HrRetirementPlan> GetEmployeeRetirementPlanAsync(long employeeId);
        Task<int> EnrollInRetirementPlanAsync(HrRetirementPlan plan);
        Task<int> UpdateRetirementPlanAsync(HrRetirementPlan plan);
        Task<decimal> CalculateRetirementBalanceAsync(long employeeId);
        
        // Wellness Programs
        Task<List<HrWellnessProgram>> GetActiveWellnessProgramsAsync();
        Task<int> CreateWellnessProgramAsync(HrWellnessProgram program);
        Task<int> EnrollInWellnessProgramAsync(HrWellnessParticipation participation);
        Task<List<HrWellnessParticipation>> GetEmployeeWellnessParticipationAsync(long employeeId);
        Task<int> UpdateWellnessProgressAsync(long participationId, decimal progress);
        
        // Benefits Analytics
        Task<object> GetBenefitsUtilizationAsync(DateTime startDate, DateTime endDate);
        Task<object> GetBenefitsCostAnalysisAsync(int year);
        Task<object> GetEmployeeBenefitsSummaryAsync(long employeeId);
        Task<decimal> CalculateTotalBenefitsCostAsync(long employeeId);
        
        // Open Enrollment
        Task<bool> IsOpenEnrollmentPeriodAsync();
        Task<List<HrBenefitPlan>> GetEligiblePlansAsync(long employeeId);
        Task<int> ProcessOpenEnrollmentAsync(long employeeId, List<long> selectedPlanIds);
    }
}
