using ZR.Model.HR.EmployeeCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.Customers;
using ZR.Model.Finance.Budget;
using ZR.Model.Finance.Configuration;
using ZR.Model.HR;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Finance;
using ZR.Model.HR.Finance.Enums;
using ZR.Repository;
using ZR.Service.HR.IService;
using ZR.ServiceCore;

namespace ZR.Service.HR
{
    /// <summary>
    /// Benefits Service Implementation
    /// Provides comprehensive employee benefits management including benefit plans, enrollments, claims, and wellness programs
    /// </summary>
    [AppService(ServiceType = typeof(IBenefitsService), ServiceLifetime = LifeTime.Transient)]
    public class BenefitsService : BaseService<HrBenefitPlan>, IBenefitsService
    {
        // Benefit Plans
        public async Task<PagedInfo<HrBenefitPlan>> GetBenefitPlansAsync(Expression<Func<HrBenefitPlan, bool>> predicate, PagerInfo pager)
        {
            var query = Context.Queryable<HrBenefitPlan>()
                .Where(predicate)
                .OrderBy(p => p.Create_time, OrderByType.Desc);

            return await query.ToPageAsync(pager);
        }

        public async Task<List<HrBenefitPlan>> GetActiveBenefitPlansAsync()
        {
            return await Context.Queryable<HrBenefitPlan>()
                .Where(p => p.IsActive && (p.ExpiryDate == null || p.ExpiryDate >= DateTime.Now))
                .OrderBy(p => p.PlanName)
                .ToListAsync();
        }

        public async Task<HrBenefitPlan> GetBenefitPlanByIdAsync(long planId)
        {
            return await Context.Queryable<HrBenefitPlan>()
                .FirstAsync(p => p.PlanId == planId);
        }

        public async Task<int> AddBenefitPlanAsync(HrBenefitPlan plan)
        {
            // Validate plan data
            if (string.IsNullOrEmpty(plan.PlanName))
                throw new ArgumentException("Plan name is required");

            if (string.IsNullOrEmpty(plan.PlanCode))
                throw new ArgumentException("Plan code is required");

            // Check for duplicate plan codes
            var exists = await Context.Queryable<HrBenefitPlan>()
                .AnyAsync(p => p.PlanCode == plan.PlanCode);
            if (exists)
                throw new InvalidOperationException("Plan code already exists");

            plan.Create_time = DateTime.Now;
            return await Context.Insertable(plan).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateBenefitPlanAsync(HrBenefitPlan plan)
        {
            // Validate plan data
            if (string.IsNullOrEmpty(plan.PlanName))
                throw new ArgumentException("Plan name is required");

            // Check for duplicate plan codes (excluding current plan)
            var exists = await Context.Queryable<HrBenefitPlan>()
                .AnyAsync(p => p.PlanCode == plan.PlanCode && p.PlanId != plan.PlanId);
            if (exists)
                throw new InvalidOperationException("Plan code already exists");

            plan.Update_time = DateTime.Now;
            return await Context.Updateable(plan)
                .IgnoreColumns(p => new { p.Create_time, p.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<int> DeleteBenefitPlanAsync(long planId)
        {
            // Check if plan is in use
            var enrollmentExists = await Context.Queryable<HrBenefitEnrollment>()
                .AnyAsync(e => e.PlanId == planId && e.Status == "active");
            if (enrollmentExists)
                throw new InvalidOperationException("Cannot delete plan with active enrollments");

            return await Context.Deleteable<HrBenefitPlan>()
                .Where(p => p.PlanId == planId)
                .ExecuteCommandAsync();
        }

        // Benefit Enrollment
        public async Task<PagedInfo<HrBenefitEnrollment>> GetEnrollmentsAsync(Expression<Func<HrBenefitEnrollment, bool>> predicate, PagerInfo pager)
        {
            var query = Context.Queryable<HrBenefitEnrollment>()
                .LeftJoin<HrEmployeeCore>((e, emp) => e.EmployeeId == emp.EmployeeId)
                .LeftJoin<HrBenefitPlan>((e, emp, p) => e.PlanId == p.PlanId)
                .Where(predicate)
                .Select((e, emp, p) => new HrBenefitEnrollment
                {
                    EnrollmentId = e.EnrollmentId,
                    EmployeeId = e.EmployeeId,
                    PlanId = e.PlanId,
                    EnrollmentDate = e.EnrollmentDate,
                    CoverageStartDate = e.CoverageStartDate,
                    CoverageEndDate = e.CoverageEndDate,
                    Status = e.Status,
                    EmployeeContribution = e.EmployeeContribution,
                    EmployerContribution = e.EmployerContribution,
                    PaymentFrequency = e.PaymentFrequency,
                    Beneficiaries = e.Beneficiaries,
                    Dependents = e.Dependents,
                    TerminationReason = e.TerminationReason,
                    TerminationDate = e.TerminationDate
                })
                .OrderBy(e => e.Create_time, OrderByType.Desc);

            return await query.ToPageAsync(pager);
        }

        public async Task<List<HrBenefitEnrollment>> GetEmployeeEnrollmentsAsync(long employeeId)
        {
            return await Context.Queryable<HrBenefitEnrollment>()
                .LeftJoin<HrBenefitPlan>((e, p) => e.PlanId == p.PlanId)
                .Where((e, p) => e.EmployeeId == employeeId)
                .Select((e, p) => new HrBenefitEnrollment
                {
                    EnrollmentId = e.EnrollmentId,
                    EmployeeId = e.EmployeeId,
                    PlanId = e.PlanId,
                    EnrollmentDate = e.EnrollmentDate,
                    CoverageStartDate = e.CoverageStartDate,
                    CoverageEndDate = e.CoverageEndDate,
                    Status = e.Status,
                    EmployeeContribution = e.EmployeeContribution,
                    EmployerContribution = e.EmployerContribution,
                    PaymentFrequency = e.PaymentFrequency,
                    Beneficiaries = e.Beneficiaries,
                    Dependents = e.Dependents,
                    TerminationReason = e.TerminationReason,
                    TerminationDate = e.TerminationDate
                })
                .OrderBy(e => e.Status == "active" ? 0 : 1)
                .ToListAsync();
        }

        public async Task<int> EnrollEmployeeAsync(HrBenefitEnrollment enrollment)
        {
            // Validate enrollment data
            if (enrollment.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (enrollment.PlanId <= 0)
                throw new ArgumentException("Plan ID is required");

            // Check if employee is already enrolled in this plan
            var existingEnrollment = await Context.Queryable<HrBenefitEnrollment>()
                .AnyAsync(e => e.EmployeeId == enrollment.EmployeeId 
                    && e.PlanId == enrollment.PlanId 
                    && e.Status == "active");

            if (existingEnrollment)
                throw new InvalidOperationException("Employee is already enrolled in this plan");

            // Get plan details for contribution calculation
            var plan = await GetBenefitPlanByIdAsync(enrollment.PlanId);
            if (plan == null)
                throw new ArgumentException("Invalid plan ID");

            enrollment.EmployeeContribution = plan.EmployeeContribution;
            enrollment.EmployerContribution = plan.EmployerContribution;
            enrollment.Status = "active";
            enrollment.Create_time = DateTime.Now;

            return await Context.Insertable(enrollment).ExecuteReturnIdentityAsync();
        }

        public async Task<long> EnrollEmployee(long employeeId, long planId)
        {
            var result = await Context.Ado.UseTranAsync(async () =>
            {
                var plan = await Context.Queryable<HrBenefitPlan>()
                    .FirstAsync(p => p.PlanId == planId && p.IsActive);

                if (plan == null)
                    throw new ArgumentException($"Benefit plan not found or inactive: {planId}");

                var enrollment = new HrBenefitEnrollment
                {
                    EmployeeId = employeeId,
                    PlanId = planId,
                    EnrollmentDate = DateTime.Now,
                    CoverageStartDate = DateTime.Now.Date,
                    CoverageEndDate = plan.ExpiryDate,
                    Status = "pending",
                    EmployeeContribution = plan.EmployeeContribution,
                    EmployerContribution = plan.EmployerContribution,
                    PaymentFrequency = "monthly",
                    BudgetAllocationId = plan.BudgetAllocationId,
                    ARInvoiceId = null,
                    Create_time = DateTime.Now
                };

                // Budget validation (company portion)
                if (enrollment.BudgetAllocationId.HasValue)
                {
                    var available = await Context.Queryable<BudgetLine>()
                        .Where(b => b.Id == enrollment.BudgetAllocationId.Value)
                        .Select(b => (SqlFunc.IIF(b.RevisedAmount > 0, b.RevisedAmount, b.BudgetedAmount) - b.ActualAmount - b.CommittedAmount))
                        .FirstAsync();

                    if (available < enrollment.EmployerContribution)
                        throw new InvalidOperationException("Budget exceeded for this enrollment.");
                }

                // Ensure Benefits Clearing customer exists (employee receivable)
                var customer = await Context.Queryable<Customer>()
                    .FirstAsync(c => c.CustomerCode == "EMPLOYEE_BENEFITS");

                if (customer == null)
                {
                    customer = new Customer
                    {
                        Id = Guid.NewGuid(),
                        CustomerCode = "EMPLOYEE_BENEFITS",
                        NameEn = "Employee Benefits Receivable",
                        NameAr = "ذمم منافع الموظفين",
                        IsActive = true,
                        CreditLimit = 0,
                        Create_time = DateTime.Now
                    };
                    await Context.Insertable(customer).ExecuteCommandAsync();
                }

                var baseCurrency = await Context.Queryable<Currency>()
                    .FirstAsync(c => c.IsBaseCurrency && c.IsActive);

                // Create AR invoice for employee contribution (if any)
                if (enrollment.EmployeeContribution > 0)
                {
                    var arInvoice = new ARInvoice
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customer.Id,
                        InvoiceNumber = $"HRBEN-{employeeId}-{planId}-{DateTime.Now:yyyyMMddHHmm}",
                        InvoiceDate = DateTime.Now,
                        DueDate = DateTime.Now,
                        Currency = baseCurrency?.Code ?? "SAR",
                        ExchangeRate = 1,
                        Subtotal = enrollment.EmployeeContribution,
                        TaxAmount = 0,
                        TotalAmount = enrollment.EmployeeContribution,
                        PaidAmount = 0,
                        RemainingAmount = enrollment.EmployeeContribution,
                        Status = InvoiceStatus.Draft,
                        IsTaxInclusive = false,
                        Description = $"Benefit enrollment employee contribution (Employee {employeeId}, Plan {planId})",
                        ReferenceNumber = $"HRBEN:{employeeId}:{planId}"
                    };

                    await Context.Insertable(arInvoice).ExecuteCommandAsync();
                    enrollment.ARInvoiceId = arInvoice.Id;
                }

                enrollment.Status = "active";
                var enrollmentId = await Context.Insertable(enrollment).ExecuteReturnIdentityAsync();

                // Audit log
                var audit = new HrFinancialAuditLog
                {
                    LogId = Guid.NewGuid(),
                    Action = "EnrollEmployeeBenefit",
                    UserId = 0,
                    Timestamp = DateTime.Now,
                    Changes = $"{{\"employeeId\":{employeeId},\"planId\":{planId},\"enrollmentId\":{enrollmentId},\"budgetAllocationId\":{(enrollment.BudgetAllocationId.HasValue ? $"\\\"{enrollment.BudgetAllocationId.Value}\\\"" : "null")},\"arInvoiceId\":{(enrollment.ARInvoiceId.HasValue ? $"\\\"{enrollment.ARInvoiceId.Value}\\\"" : "null")}}}",
                    Create_time = DateTime.Now
                };
                await Context.Insertable(audit).ExecuteCommandAsync();

                return (long)enrollmentId;
            });

            if (!result.IsSuccess)
                throw result.ErrorException;

            return (long)result.Data;
        }

        public async Task<int> UpdateEnrollmentAsync(HrBenefitEnrollment enrollment)
        {
            enrollment.Update_time = DateTime.Now;
            return await Context.Updateable(enrollment)
                .IgnoreColumns(e => new { e.Create_time, e.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<int> TerminateEnrollmentAsync(long enrollmentId, string reason)
        {
            return await Context.Updateable<HrBenefitEnrollment>()
                .SetColumns(e => new HrBenefitEnrollment
                {
                    Status = "terminated",
                    TerminationDate = DateTime.Now,
                    TerminationReason = reason,
                    CoverageEndDate = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(e => e.EnrollmentId == enrollmentId)
                .ExecuteCommandAsync();
        }

        // Benefit Claims
        public async Task<PagedInfo<HrBenefitClaim>> GetBenefitClaimsAsync(Expression<Func<HrBenefitClaim, bool>> predicate, PagerInfo pager)
        {
            var query = Context.Queryable<HrBenefitClaim>()
                .LeftJoin<HrEmployeeCore>((c, e) => c.EmployeeId == e.EmployeeId)
                .LeftJoin<HrBenefitEnrollment>((c, e, en) => c.EnrollmentId == en.EnrollmentId)
                .MergeTable()
                .Where(predicate)
                .OrderBy(c => c.Create_time, OrderByType.Desc);

            return await query.ToPageAsync(pager);
        }

        public async Task<List<HrBenefitClaim>> GetEmployeeClaimsAsync(long employeeId)
        {
            return await Context.Queryable<HrBenefitClaim>()
                .Where(c => c.EmployeeId == employeeId)
                .OrderBy(c => c.Create_time, OrderByType.Desc)
                .ToListAsync();
        }

        public async Task<int> SubmitClaimAsync(HrBenefitClaim claim)
        {
            // Validate claim data
            if (claim.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (claim.EnrollmentId <= 0)
                throw new ArgumentException("Enrollment ID is required");

            if (claim.ClaimAmount <= 0)
                throw new ArgumentException("Claim amount must be greater than zero");

            // Generate claim number
            var claimCount = await Context.Queryable<HrBenefitClaim>().CountAsync();
            claim.ClaimNumber = $"CLM{DateTime.Now:yyyyMM}{(claimCount + 1):D4}";
            claim.Status = "submitted";
            claim.Create_time = DateTime.Now;

            return await Context.Insertable(claim).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateClaimStatusAsync(long claimId, string status, string comments)
        {
            return await Context.Updateable<HrBenefitClaim>()
                .SetColumns(c => new HrBenefitClaim
                {
                    Status = status,
                    Update_time = DateTime.Now
                })
                .Where(c => c.ClaimId == claimId)
                .ExecuteCommandAsync();
        }

        public async Task<int> ApproveClaimAsync(long claimId, decimal approvedAmount, string approvedBy)
        {
            return await Context.Updateable<HrBenefitClaim>()
                .SetColumns(c => new HrBenefitClaim
                {
                    Status = "approved",
                    ApprovedAmount = approvedAmount,
                    ApprovedBy = approvedBy,
                    ApprovalDate = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(c => c.ClaimId == claimId)
                .ExecuteCommandAsync();
        }

        public async Task<int> RejectClaimAsync(long claimId, string reason, string rejectedBy)
        {
            return await Context.Updateable<HrBenefitClaim>()
                .SetColumns(c => new HrBenefitClaim
                {
                    Status = "rejected",
                    RejectionReason = reason,
                    ReviewedBy = rejectedBy,
                    ReviewDate = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(c => c.ClaimId == claimId)
                .ExecuteCommandAsync();
        }

        public async Task<int> ProcessClaimPaymentAsync(long claimId, string paymentReference)
        {
            return await Context.Updateable<HrBenefitClaim>()
                .SetColumns(c => new HrBenefitClaim
                {
                    Status = "paid",
                    PaymentReference = paymentReference,
                    PaymentDate = DateTime.Now,
                    Update_time = DateTime.Now
                })
                .Where(c => c.ClaimId == claimId)
                .ExecuteCommandAsync();
        }

        // Employee Dependents
        public async Task<List<HrEmployeeDependent>> GetEmployeeDependentsAsync(long employeeId)
        {
            return await Context.Queryable<HrEmployeeDependent>()
                .Where(d => d.EmployeeId == employeeId && d.IsActive)
                .OrderBy(d => d.Relationship)
                .ToListAsync();
        }

        public async Task<int> AddDependentAsync(HrEmployeeDependent dependent)
        {
            // Validate dependent data
            if (dependent.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (string.IsNullOrEmpty(dependent.FirstName))
                throw new ArgumentException("First name is required");

            if (string.IsNullOrEmpty(dependent.Relationship))
                throw new ArgumentException("Relationship is required");

            dependent.IsActive = true;
            dependent.Create_time = DateTime.Now;

            return await Context.Insertable(dependent).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateDependentAsync(HrEmployeeDependent dependent)
        {
            dependent.Update_time = DateTime.Now;
            return await Context.Updateable(dependent)
                .IgnoreColumns(d => new { d.Create_time, d.Create_by })
                .ExecuteCommandAsync();
        }

        public async Task<int> RemoveDependentAsync(long dependentId)
        {
            return await Context.Updateable<HrEmployeeDependent>()
                .SetColumns(d => new HrEmployeeDependent
                {
                    IsActive = false,
                    Update_time = DateTime.Now
                })
                .Where(d => d.DependentId == dependentId)
                .ExecuteCommandAsync();
        }

        // Retirement Plans
        public async Task<HrRetirementPlan> GetEmployeeRetirementPlanAsync(long employeeId)
        {
            return await Context.Queryable<HrRetirementPlan>()
                .FirstAsync(r => r.EmployeeId == employeeId && r.Status == "active");
        }

        public async Task<int> EnrollInRetirementPlanAsync(HrRetirementPlan plan)
        {
            // Validate retirement plan data
            if (plan.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (string.IsNullOrEmpty(plan.PlanType))
                throw new ArgumentException("Plan type is required");

            // Check if employee already has an active retirement plan
            var existingPlan = await Context.Queryable<HrRetirementPlan>()
                .AnyAsync(r => r.EmployeeId == plan.EmployeeId && r.Status == "active");

            if (existingPlan)
                throw new InvalidOperationException("Employee already has an active retirement plan");

            plan.Status = "active";
            plan.EnrollmentDate = DateTime.Now;
            plan.Create_time = DateTime.Now;

            return await Context.Insertable(plan).ExecuteReturnIdentityAsync();
        }

        public async Task<int> UpdateRetirementPlanAsync(HrRetirementPlan plan)
        {
            plan.Update_time = DateTime.Now;
            return await Context.Updateable(plan)
                .IgnoreColumns(r => new { r.Create_time, r.Create_by, r.EnrollmentDate })
                .ExecuteCommandAsync();
        }

        public async Task<decimal> CalculateRetirementBalanceAsync(long employeeId)
        {
            var plan = await GetEmployeeRetirementPlanAsync(employeeId);
            if (plan == null)
                return 0;

            // This is a simplified calculation - in practice, you would consider
            // contributions, employer matching, investment returns, etc.
            return plan.CurrentBalance + plan.YearToDateContribution;
        }

        // Wellness Programs
        public async Task<List<HrWellnessProgram>> GetActiveWellnessProgramsAsync()
        {
            return await Context.Queryable<HrWellnessProgram>()
                .Where(p => p.IsActive && p.StartDate <= DateTime.Now && 
                           (p.EndDate == null || p.EndDate >= DateTime.Now))
                .OrderBy(p => p.ProgramName)
                .ToListAsync();
        }

        public async Task<int> CreateWellnessProgramAsync(HrWellnessProgram program)
        {
            // Validate program data
            if (string.IsNullOrEmpty(program.ProgramName))
                throw new ArgumentException("Program name is required");

            if (program.StartDate < DateTime.Now.Date)
                throw new ArgumentException("Start date cannot be in the past");

            program.EnrolledCount = 0;
            program.IsActive = true;
            program.Create_time = DateTime.Now;

            return await Context.Insertable(program).ExecuteReturnIdentityAsync();
        }

        public async Task<int> EnrollInWellnessProgramAsync(HrWellnessParticipation participation)
        {
            // Validate participation data
            if (participation.EmployeeId <= 0)
                throw new ArgumentException("Employee ID is required");

            if (participation.ProgramId <= 0)
                throw new ArgumentException("Program ID is required");

            // Check if employee is already enrolled in this program
            var existingParticipation = await Context.Queryable<HrWellnessParticipation>()
                .AnyAsync(p => p.EmployeeId == participation.EmployeeId 
                    && p.ProgramId == participation.ProgramId 
                    && p.Status == "active");

            if (existingParticipation)
                throw new InvalidOperationException("Employee is already enrolled in this program");

            // Check program capacity
            var program = await Context.Queryable<HrWellnessProgram>()
                .FirstAsync(p => p.ProgramId == participation.ProgramId);

            if (program != null && program.MaxParticipants > 0 && program.EnrolledCount >= program.MaxParticipants)
                throw new InvalidOperationException("Program has reached maximum capacity");

            participation.Status = "active";
            participation.EnrollmentDate = DateTime.Now;
            participation.ProgressPercentage = 0;
            participation.PointsEarned = 0;
            participation.Create_time = DateTime.Now;

            var result = await Context.Insertable(participation).ExecuteReturnIdentityAsync();

            // Update enrolled count
            await Context.Updateable<HrWellnessProgram>()
                .SetColumns(p => p.EnrolledCount == SqlFunc.Subqueryable<HrWellnessParticipation>()
                    .Where(pt => pt.ProgramId == p.ProgramId && pt.Status == "active").Count())
                .Where(p => p.ProgramId == participation.ProgramId)
                .ExecuteCommandAsync();

            return result;
        }

        public async Task<List<HrWellnessParticipation>> GetEmployeeWellnessParticipationAsync(long employeeId)
        {
            return await Context.Queryable<HrWellnessParticipation>()
                .LeftJoin<HrWellnessProgram>((p, w) => p.ProgramId == w.ProgramId)
                .Where((p, w) => p.EmployeeId == employeeId)
                .OrderBy(p => p.Status == "active" ? 0 : 1)
                .Select(p => p)
                .ToListAsync();
        }

        public async Task<int> UpdateWellnessProgressAsync(long participationId, decimal progress)
        {
            return await Context.Updateable<HrWellnessParticipation>()
                .SetColumns(p => new HrWellnessParticipation
                {
                    ProgressPercentage = progress,
                    Update_time = DateTime.Now
                })
                .Where(p => p.ParticipationId == participationId)
                .ExecuteCommandAsync();
        }

        // Benefits Analytics
        public async Task<object> GetBenefitsUtilizationAsync(DateTime startDate, DateTime endDate)
        {
            var enrollments = await Context.Queryable<HrBenefitEnrollment>()
                .LeftJoin<HrBenefitPlan>((e, p) => e.PlanId == p.PlanId)
                .Where((e, p) => e.EnrollmentDate >= startDate && e.EnrollmentDate <= endDate)
                .GroupBy((e, p) => p.PlanType)
                .Select((e, p) => new
                {
                    PlanType = p.PlanType,
                    EnrollmentCount = SqlFunc.AggregateCount(e.EnrollmentId),
                    TotalEmployeeContribution = SqlFunc.AggregateSum(e.EmployeeContribution),
                    TotalEmployerContribution = SqlFunc.AggregateSum(e.EmployerContribution)
                })
                .ToListAsync();

            return new
            {
                Period = new { StartDate = startDate, EndDate = endDate },
                Utilization = enrollments
            };
        }

        public async Task<object> GetBenefitsCostAnalysisAsync(int year)
        {
            var costs = await Context.Queryable<HrBenefitEnrollment>()
                .LeftJoin<HrBenefitPlan>((e, p) => e.PlanId == p.PlanId)
                .Where((e, p) => e.EnrollmentDate.Year == year)
                .GroupBy((e, p) => new { p.PlanType, Month = e.EnrollmentDate.Month })
                .Select((e, p) => new
                {
                    PlanType = p.PlanType,
                    Month = SqlFunc.AggregateMin(e.EnrollmentDate.Month),
                    EmployeeCost = SqlFunc.AggregateSum(e.EmployeeContribution),
                    EmployerCost = SqlFunc.AggregateSum(e.EmployerContribution),
                    TotalCost = SqlFunc.AggregateSum(e.EmployeeContribution + e.EmployerContribution)
                })
                .ToListAsync();

            return new
            {
                Year = year,
                CostAnalysis = costs,
                Summary = new
                {
                    TotalEmployeeCost = costs.Sum(c => c.EmployeeCost),
                    TotalEmployerCost = costs.Sum(c => c.EmployerCost),
                    GrandTotal = costs.Sum(c => c.TotalCost)
                }
            };
        }

        public async Task<object> GetEmployeeBenefitsSummaryAsync(long employeeId)
        {
            var enrollments = await GetEmployeeEnrollmentsAsync(employeeId);
            var claims = await GetEmployeeClaimsAsync(employeeId);
            var dependents = await GetEmployeeDependentsAsync(employeeId);
            var retirementPlan = await GetEmployeeRetirementPlanAsync(employeeId);
            var wellnessParticipation = await GetEmployeeWellnessParticipationAsync(employeeId);

            return new
            {
                EmployeeId = employeeId,
                ActiveEnrollments = enrollments.Where(e => e.Status == "active").Count(),
                TotalMonthlyContribution = enrollments.Where(e => e.Status == "active")
                    .Sum(e => e.EmployeeContribution),
                TotalClaims = claims.Count,
                TotalClaimAmount = claims.Sum(c => c.ClaimAmount),
                ApprovedClaimAmount = claims.Where(c => c.Status == "approved" || c.Status == "paid")
                    .Sum(c => c.ApprovedAmount),
                DependentsCount = dependents.Count,
                HasRetirementPlan = retirementPlan != null,
                RetirementBalance = retirementPlan?.CurrentBalance ?? 0,
                WellnessProgramsCount = wellnessParticipation.Count(p => p.Status == "active")
            };
        }

        public async Task<decimal> CalculateTotalBenefitsCostAsync(long employeeId)
        {
            var enrollments = await GetEmployeeEnrollmentsAsync(employeeId);
            var activeCost = enrollments.Where(e => e.Status == "active")
                .Sum(e => e.EmployeeContribution + e.EmployerContribution);

            var retirementPlan = await GetEmployeeRetirementPlanAsync(employeeId);
            var retirementCost = retirementPlan?.YearToDateContribution ?? 0;

            return activeCost + retirementCost;
        }

        // Open Enrollment
        public async Task<bool> IsOpenEnrollmentPeriodAsync()
        {
            // This would typically be configured in system settings
            // For now, we'll assume November is open enrollment month
            var now = DateTime.Now;
            return now.Month == 11; // November
        }

        public async Task<List<HrBenefitPlan>> GetEligiblePlansAsync(long employeeId)
        {
            // Get employee details for eligibility check
            var employee = await Context.Queryable<HrEmployeeCore>()
                .FirstAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return new List<HrBenefitPlan>();

            // Calculate tenure for eligibility
            var tenure = employee.HireDate.HasValue ? (DateTime.Now - employee.HireDate.Value).Days : 0;

            return await Context.Queryable<HrBenefitPlan>()
                .Where(p => p.IsActive && 
                           (p.ExpiryDate == null || p.ExpiryDate >= DateTime.Now) &&
                           p.WaitingPeriodDays <= tenure)
                .OrderBy(p => p.PlanName)
                .ToListAsync();
        }

        public async Task<int> ProcessOpenEnrollmentAsync(long employeeId, List<long> selectedPlanIds)
        {
            if (!await IsOpenEnrollmentPeriodAsync())
                throw new InvalidOperationException("Open enrollment is not currently active");

            // Terminate existing enrollments (except those not affected by open enrollment)
            await Context.Updateable<HrBenefitEnrollment>()
                .SetColumns(e => new HrBenefitEnrollment
                {
                    Status = "terminated",
                    TerminationDate = DateTime.Now.AddYears(1).AddDays(-1), // End of current benefit year
                    TerminationReason = "Open enrollment change",
                    Update_time = DateTime.Now
                })
                .Where(e => e.EmployeeId == employeeId && e.Status == "active")
                .ExecuteCommandAsync();

            // Enroll in selected plans
            var enrollmentCount = 0;
            var nextYear = DateTime.Now.AddYears(1);
            var benefitYearStart = new DateTime(nextYear.Year, 1, 1);

            foreach (var planId in selectedPlanIds)
            {
                var plan = await GetBenefitPlanByIdAsync(planId);
                if (plan != null)
                {
                    var enrollment = new HrBenefitEnrollment
                    {
                        EmployeeId = employeeId,
                        PlanId = planId,
                        EnrollmentDate = DateTime.Now,
                        CoverageStartDate = benefitYearStart,
                        Status = "active",
                        EmployeeContribution = plan.EmployeeContribution,
                        EmployerContribution = plan.EmployerContribution,
                        PaymentFrequency = "monthly"
                    };

                    await EnrollEmployeeAsync(enrollment);
                    enrollmentCount++;
                }
            }

            return enrollmentCount;
        }
    }
}
