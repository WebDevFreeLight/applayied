using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using SqlSugar;
using ZR.Admin.WebApi.Extensions;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model.HR;
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;

namespace ZR.Admin.WebApi.Controllers.HR.Benefits
{
    /// <summary>
    /// Benefits Management Controller
    /// </summary>
    [Verify]
    [Route("api/hr/benefits")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class BenefitsController : BaseController
    {
        private readonly IBenefitsService _benefitsService;

        public BenefitsController(IBenefitsService benefitsService)
        {
            _benefitsService = benefitsService;
        }

        #region Benefit Plans

        /// <summary>
        /// Get benefit plans
        /// </summary>
        [HttpGet("plans")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetBenefitPlans([FromQuery] BenefitPlanQueryDto dto)
        {
            var predicate = Expressionable.Create<HrBenefitPlan>();
            
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.PlanName), 
                it => it.PlanName.Contains(dto.PlanName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.PlanType), 
                it => it.PlanType == dto.PlanType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status), 
                it => it.Status == dto.Status);
            predicate = predicate.AndIF(dto.IsActive.HasValue, 
                it => it.IsActive == dto.IsActive);
            
            var response = await _benefitsService.GetBenefitPlansAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get active benefit plans
        /// </summary>
        [HttpGet("plans/active")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetActiveBenefitPlans()
        {
            var plans = await _benefitsService.GetActiveBenefitPlansAsync();
            return SUCCESS(plans);
        }

        /// <summary>
        /// Get benefit plan by ID
        /// </summary>
        [HttpGet("plans/{planId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetBenefitPlanById(long planId)
        {
            var plan = await _benefitsService.GetBenefitPlanByIdAsync(planId);
            return SUCCESS(plan);
        }

        /// <summary>
        /// Add benefit plan
        /// </summary>
        [HttpPost("plans")]
        [ActionPermissionFilter("hr:benefits:add")]
        [Log(Title = "Benefit Plan Management", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddBenefitPlan([FromBody] BenefitsDto dto)
        {
            var plan = dto.Adapt<HrBenefitPlan>();
            plan.Create_by = HttpContext.GetName();
            plan.Create_time = DateTime.Now;
            
            var result = await _benefitsService.AddBenefitPlanAsync(plan);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update benefit plan
        /// </summary>
        [HttpPut("plans/{planId}")]
        [ActionPermissionFilter("hr:benefits:edit")]
        [Log(Title = "Benefit Plan Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateBenefitPlan(long planId, [FromBody] BenefitsDto dto)
        {
            var plan = dto.Adapt<HrBenefitPlan>();
            plan.PlanId = planId;
            plan.Update_by = HttpContext.GetName();
            plan.Update_time = DateTime.Now;
            
            var result = await _benefitsService.UpdateBenefitPlanAsync(plan);
            return SUCCESS(result);
        }

        /// <summary>
        /// Delete benefit plan
        /// </summary>
        [HttpDelete("plans/{planId}")]
        [ActionPermissionFilter("hr:benefits:delete")]
        [Log(Title = "Benefit Plan Management", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteBenefitPlan(long planId)
        {
            var result = await _benefitsService.DeleteBenefitPlanAsync(planId);
            return SUCCESS(result);
        }

        #endregion

        #region Benefit Enrollment

        /// <summary>
        /// Get enrollments
        /// </summary>
        [HttpGet("enrollments")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetEnrollments([FromQuery] BenefitEnrollmentQueryDto dto)
        {
            var predicate = Expressionable.Create<HrBenefitEnrollment>();
            
            predicate = predicate.AndIF(dto.EmployeeId > 0, 
                it => it.EmployeeId == dto.EmployeeId);
            predicate = predicate.AndIF(dto.PlanId > 0, 
                it => it.PlanId == dto.PlanId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status), 
                it => it.Status == dto.Status);
            predicate = predicate.AndIF(dto.StartDate != null, 
                it => it.EnrollmentDate >= dto.StartDate);
            predicate = predicate.AndIF(dto.EndDate != null, 
                it => it.EnrollmentDate <= dto.EndDate);
            
            var response = await _benefitsService.GetEnrollmentsAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get employee enrollments
        /// </summary>
        [HttpGet("enrollments/employee/{employeeId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetEmployeeEnrollments(long employeeId)
        {
            var enrollments = await _benefitsService.GetEmployeeEnrollmentsAsync(employeeId);
            return SUCCESS(enrollments);
        }

        /// <summary>
        /// Enroll employee in benefit plan
        /// </summary>
        [HttpPost("enrollments")]
        [ActionPermissionFilter("hr:benefits:enroll")]
        [Log(Title = "Benefit Enrollment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> EnrollEmployee([FromBody] BenefitEnrollmentDto dto)
        {
            var enrollment = dto.Adapt<HrBenefitEnrollment>();
            enrollment.Create_by = HttpContext.GetName();
            enrollment.Create_time = DateTime.Now;
            enrollment.EnrollmentDate = DateTime.Now;
            
            var result = await _benefitsService.EnrollEmployeeAsync(enrollment);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update enrollment
        /// </summary>
        [HttpPut("enrollments/{enrollmentId}")]
        [ActionPermissionFilter("hr:benefits:edit")]
        [Log(Title = "Benefit Enrollment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateEnrollment(long enrollmentId, [FromBody] BenefitEnrollmentDto dto)
        {
            var enrollment = dto.Adapt<HrBenefitEnrollment>();
            enrollment.EnrollmentId = enrollmentId;
            enrollment.Update_by = HttpContext.GetName();
            enrollment.Update_time = DateTime.Now;
            
            var result = await _benefitsService.UpdateEnrollmentAsync(enrollment);
            return SUCCESS(result);
        }

        /// <summary>
        /// Terminate enrollment
        /// </summary>
        [HttpPost("enrollments/{enrollmentId}/terminate")]
        [ActionPermissionFilter("hr:benefits:terminate")]
        [Log(Title = "Benefit Enrollment Termination", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> TerminateEnrollment(long enrollmentId, [FromBody] TerminateEnrollmentDto dto)
        {
            var result = await _benefitsService.TerminateEnrollmentAsync(enrollmentId, dto.Reason);
            return SUCCESS(result);
        }

        #endregion

        #region Benefit Claims

        /// <summary>
        /// Get benefit claims
        /// </summary>
        [HttpGet("claims")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetBenefitClaims([FromQuery] BenefitClaimQueryDto dto)
        {
            var predicate = Expressionable.Create<HrBenefitClaim>();
            
            predicate = predicate.AndIF(dto.EmployeeId > 0, 
                it => it.EmployeeId == dto.EmployeeId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status), 
                it => it.Status == dto.Status);
            predicate = predicate.AndIF(dto.StartDate != null, 
                it => it.ClaimDate >= dto.StartDate);
            predicate = predicate.AndIF(dto.EndDate != null, 
                it => it.ClaimDate <= dto.EndDate);
            
            var response = await _benefitsService.GetBenefitClaimsAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get employee claims
        /// </summary>
        [HttpGet("claims/employee/{employeeId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetEmployeeClaims(long employeeId)
        {
            var claims = await _benefitsService.GetEmployeeClaimsAsync(employeeId);
            return SUCCESS(claims);
        }

        /// <summary>
        /// Submit benefit claim
        /// </summary>
        [HttpPost("claims")]
        [ActionPermissionFilter("hr:benefits:claim")]
        [Log(Title = "Benefit Claim", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> SubmitClaim([FromBody] BenefitClaimDto dto)
        {
            var claim = dto.Adapt<HrBenefitClaim>();
            claim.Create_by = HttpContext.GetName();
            claim.Create_time = DateTime.Now;
            claim.ClaimDate = DateTime.Now;
            claim.Status = "PENDING";
            
            var result = await _benefitsService.SubmitClaimAsync(claim);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update claim status
        /// </summary>
        [HttpPost("claims/{claimId}/status")]
        [ActionPermissionFilter("hr:benefits:approve")]
        [Log(Title = "Benefit Claim Status", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateClaimStatus(long claimId, [FromBody] UpdateClaimStatusDto dto)
        {
            var result = await _benefitsService.UpdateClaimStatusAsync(claimId, dto.Status, dto.Comments);
            return SUCCESS(result);
        }

        /// <summary>
        /// Approve benefit claim
        /// </summary>
        [HttpPost("claims/{claimId}/approve")]
        [ActionPermissionFilter("hr:benefits:approve")]
        [Log(Title = "Benefit Claim Approval", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApproveClaim(long claimId, [FromBody] ApproveClaimDto dto)
        {
            var approvedBy = HttpContext.GetName();
            var result = await _benefitsService.ApproveClaimAsync(claimId, (decimal)dto.ApprovedAmount, approvedBy);
            return SUCCESS(result);
        }

        /// <summary>
        /// Reject benefit claim
        /// </summary>
        [HttpPost("claims/{claimId}/reject")]
        [ActionPermissionFilter("hr:benefits:approve")]
        [Log(Title = "Benefit Claim Rejection", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RejectClaim(long claimId, [FromBody] RejectClaimDto dto)
        {
            var rejectedBy = HttpContext.GetName();
            var result = await _benefitsService.RejectClaimAsync(claimId, dto.Reason, rejectedBy);
            return SUCCESS(result);
        }

        /// <summary>
        /// Process claim payment
        /// </summary>
        [HttpPost("claims/{claimId}/payment")]
        [ActionPermissionFilter("hr:benefits:payment")]
        [Log(Title = "Benefit Claim Payment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ProcessClaimPayment(long claimId, [FromBody] ProcessPaymentDto dto)
        {
            var result = await _benefitsService.ProcessClaimPaymentAsync(claimId, dto.PaymentReference);
            return SUCCESS(result);
        }

        #endregion

        #region Employee Dependents

        /// <summary>
        /// Get employee dependents
        /// </summary>
        [HttpGet("dependents/employee/{employeeId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetEmployeeDependents(long employeeId)
        {
            var dependents = await _benefitsService.GetEmployeeDependentsAsync(employeeId);
            return SUCCESS(dependents);
        }

        /// <summary>
        /// Add dependent
        /// </summary>
        [HttpPost("dependents")]
        [ActionPermissionFilter("hr:benefits:add")]
        [Log(Title = "Employee Dependent", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddDependent([FromBody] EmployeeDependentDto dto)
        {
            var dependent = dto.Adapt<HrEmployeeDependent>();
            dependent.Create_by = HttpContext.GetName();
            dependent.Create_time = DateTime.Now;
            
            var result = await _benefitsService.AddDependentAsync(dependent);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update dependent
        /// </summary>
        [HttpPut("dependents/{dependentId}")]
        [ActionPermissionFilter("hr:benefits:edit")]
        [Log(Title = "Employee Dependent", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateDependent(long dependentId, [FromBody] EmployeeDependentDto dto)
        {
            var dependent = dto.Adapt<HrEmployeeDependent>();
            dependent.DependentId = dependentId;
            dependent.Update_by = HttpContext.GetName();
            dependent.Update_time = DateTime.Now;
            
            var result = await _benefitsService.UpdateDependentAsync(dependent);
            return SUCCESS(result);
        }

        /// <summary>
        /// Remove dependent
        /// </summary>
        [HttpDelete("dependents/{dependentId}")]
        [ActionPermissionFilter("hr:benefits:delete")]
        [Log(Title = "Employee Dependent", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> RemoveDependent(long dependentId)
        {
            var result = await _benefitsService.RemoveDependentAsync(dependentId);
            return SUCCESS(result);
        }

        #endregion

        #region Retirement Plans

        /// <summary>
        /// Get employee retirement plan
        /// </summary>
        [HttpGet("retirement/employee/{employeeId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetEmployeeRetirementPlan(long employeeId)
        {
            var plan = await _benefitsService.GetEmployeeRetirementPlanAsync(employeeId);
            return SUCCESS(plan);
        }

        /// <summary>
        /// Enroll in retirement plan
        /// </summary>
        [HttpPost("retirement/enroll")]
        [ActionPermissionFilter("hr:benefits:enroll")]
        [Log(Title = "Retirement Plan Enrollment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> EnrollInRetirementPlan([FromBody] HrRetirementPlan plan)
        {
            plan.Create_by = HttpContext.GetName();
            plan.Create_time = DateTime.Now;
            
            var result = await _benefitsService.EnrollInRetirementPlanAsync(plan);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update retirement plan
        /// </summary>
        [HttpPut("retirement/{planId}")]
        [ActionPermissionFilter("hr:benefits:edit")]
        [Log(Title = "Retirement Plan", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateRetirementPlan(long planId, [FromBody] HrRetirementPlan plan)
        {
            plan.PlanId = planId;
            plan.Update_by = HttpContext.GetName();
            plan.Update_time = DateTime.Now;
            
            var result = await _benefitsService.UpdateRetirementPlanAsync(plan);
            return SUCCESS(result);
        }

        /// <summary>
        /// Calculate retirement balance
        /// </summary>
        [HttpGet("retirement/balance/{employeeId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> CalculateRetirementBalance(long employeeId)
        {
            var balance = await _benefitsService.CalculateRetirementBalanceAsync(employeeId);
            return SUCCESS(new { Balance = balance });
        }

        #endregion

        #region Wellness Programs

        /// <summary>
        /// Get active wellness programs
        /// </summary>
        [HttpGet("wellness/programs")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetActiveWellnessPrograms()
        {
            var programs = await _benefitsService.GetActiveWellnessProgramsAsync();
            return SUCCESS(programs);
        }

        /// <summary>
        /// Create wellness program
        /// </summary>
        [HttpPost("wellness/programs")]
        [ActionPermissionFilter("hr:benefits:add")]
        [Log(Title = "Wellness Program", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateWellnessProgram([FromBody] HrWellnessProgram program)
        {
            program.Create_by = HttpContext.GetName();
            program.Create_time = DateTime.Now;
            
            var result = await _benefitsService.CreateWellnessProgramAsync(program);
            return SUCCESS(result);
        }

        /// <summary>
        /// Enroll in wellness program
        /// </summary>
        [HttpPost("wellness/enroll")]
        [ActionPermissionFilter("hr:benefits:enroll")]
        [Log(Title = "Wellness Program Enrollment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> EnrollInWellnessProgram([FromBody] HrWellnessParticipation participation)
        {
            participation.Create_by = HttpContext.GetName();
            participation.Create_time = DateTime.Now;
            participation.EnrollmentDate = DateTime.Now;
            
            var result = await _benefitsService.EnrollInWellnessProgramAsync(participation);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get employee wellness participation
        /// </summary>
        [HttpGet("wellness/participation/employee/{employeeId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetEmployeeWellnessParticipation(long employeeId)
        {
            var participation = await _benefitsService.GetEmployeeWellnessParticipationAsync(employeeId);
            return SUCCESS(participation);
        }

        /// <summary>
        /// Update wellness progress
        /// </summary>
        [HttpPut("wellness/participation/{participationId}/progress")]
        [ActionPermissionFilter("hr:benefits:edit")]
        [Log(Title = "Wellness Progress", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateWellnessProgress(long participationId, [FromBody] UpdateProgressDto dto)
        {
            var result = await _benefitsService.UpdateWellnessProgressAsync(participationId, dto.Progress);
            return SUCCESS(result);
        }

        #endregion

        #region Benefits Analytics

        /// <summary>
        /// Get benefits utilization
        /// </summary>
        [HttpGet("analytics/utilization")]
        [ActionPermissionFilter("hr:benefits:analytics")]
        public async Task<IActionResult> GetBenefitsUtilization([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var utilization = await _benefitsService.GetBenefitsUtilizationAsync(startDate, endDate);
            return SUCCESS(utilization);
        }

        /// <summary>
        /// Get benefits cost analysis
        /// </summary>
        [HttpGet("analytics/cost-analysis")]
        [ActionPermissionFilter("hr:benefits:analytics")]
        public async Task<IActionResult> GetBenefitsCostAnalysis([FromQuery] int year)
        {
            var analysis = await _benefitsService.GetBenefitsCostAnalysisAsync(year);
            return SUCCESS(analysis);
        }

        /// <summary>
        /// Get employee benefits summary
        /// </summary>
        [HttpGet("analytics/employee-summary/{employeeId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetEmployeeBenefitsSummary(long employeeId)
        {
            var summary = await _benefitsService.GetEmployeeBenefitsSummaryAsync(employeeId);
            return SUCCESS(summary);
        }

        /// <summary>
        /// Calculate total benefits cost
        /// </summary>
        [HttpGet("analytics/total-cost/{employeeId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> CalculateTotalBenefitsCost(long employeeId)
        {
            var cost = await _benefitsService.CalculateTotalBenefitsCostAsync(employeeId);
            return SUCCESS(new { TotalCost = cost });
        }

        #endregion

        #region Open Enrollment

        /// <summary>
        /// Check if open enrollment period is active
        /// </summary>
        [HttpGet("open-enrollment/status")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> IsOpenEnrollmentPeriod()
        {
            var isOpen = await _benefitsService.IsOpenEnrollmentPeriodAsync();
            return SUCCESS(new { IsOpenEnrollment = isOpen });
        }

        /// <summary>
        /// Get eligible plans for employee
        /// </summary>
        [HttpGet("open-enrollment/eligible-plans/{employeeId}")]
        [ActionPermissionFilter("hr:benefits:list")]
        public async Task<IActionResult> GetEligiblePlans(long employeeId)
        {
            var plans = await _benefitsService.GetEligiblePlansAsync(employeeId);
            return SUCCESS(plans);
        }

        /// <summary>
        /// Process open enrollment
        /// </summary>
        [HttpPost("open-enrollment/process")]
        [ActionPermissionFilter("hr:benefits:enroll")]
        [Log(Title = "Open Enrollment", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ProcessOpenEnrollment([FromBody] OpenEnrollmentDto dto)
        {
            var result = await _benefitsService.ProcessOpenEnrollmentAsync(dto.EmployeeId, dto.SelectedPlanIds);
            return SUCCESS(result);
        }

        #endregion
    }
}
