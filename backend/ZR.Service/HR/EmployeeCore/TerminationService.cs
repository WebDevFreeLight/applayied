using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.EmployeeCore;
using ZR.Repository;
using ZR.Service.HR.IService;

namespace ZR.Service.HR
{
    /// <summary>
    /// Employee Termination / Exit Workflow Service
    /// Handles: Resignation → Approval → ESB → Final Settlement → Checklist → Deactivation
    /// </summary>
    [AppService(ServiceType = typeof(ITerminationService), ServiceLifetime = LifeTime.Transient)]
    public class TerminationService : BaseService<HrTermination>, ITerminationService
    {
        private readonly IPayrollService _payrollService;
        private readonly ILeaveBalanceService _leaveBalanceService;

        public TerminationService(
            IPayrollService payrollService,
            ILeaveBalanceService leaveBalanceService)
        {
            _payrollService = payrollService;
            _leaveBalanceService = leaveBalanceService;
        }

        public async Task<PagedInfo<HrTermination>> GetListAsync(TerminationQueryDto query)
        {
            var q = Queryable()
                .WhereIF(query.EmployeeId.HasValue, t => t.EmployeeId == query.EmployeeId)
                .WhereIF(!string.IsNullOrEmpty(query.Status), t => t.Status == query.Status)
                .WhereIF(!string.IsNullOrEmpty(query.TerminationType), t => t.TerminationType == query.TerminationType)
                .WhereIF(query.FromDate.HasValue, t => t.RequestDate >= query.FromDate)
                .WhereIF(query.ToDate.HasValue, t => t.RequestDate <= query.ToDate)
                .OrderBy(t => t.RequestDate, OrderByType.Desc);

            return await q.ToPageAsync(query);
        }

        public async Task<HrTermination> GetByIdAsync(long terminationId)
        {
            return await GetFirstAsync(t => t.TerminationId == terminationId);
        }

        public async Task<HrTermination> InitiateTerminationAsync(TerminationRequestDto request, string initiatedBy)
        {
            // Check no active termination exists for this employee
            var existing = await GetFirstAsync(t =>
                t.EmployeeId == request.EmployeeId &&
                t.Status != "completed" && t.Status != "cancelled");

            if (existing != null)
                throw new Exception("An active termination process already exists for this employee.");

            var termination = new HrTermination
            {
                EmployeeId = request.EmployeeId,
                TerminationType = request.TerminationType,
                Status = "pending_approval",
                RequestDate = DateTime.Now,
                LastWorkingDate = request.LastWorkingDate ?? DateTime.Now.AddDays(request.NoticePeriodDays),
                NoticePeriodDays = request.NoticePeriodDays,
                Reason = request.Reason,
                ReasonCategory = request.ReasonCategory,
                Create_time = DateTime.Now,
                Create_by = initiatedBy
            };

            // For terminations by employer, set effective date immediately
            if (request.TerminationType == "termination")
            {
                termination.EffectiveDate = request.LastWorkingDate ?? DateTime.Now;
            }

            await InsertAsync(termination);
            return termination;
        }

        public async Task<bool> ApproveTerminationAsync(TerminationApprovalDto approval, long approverId)
        {
            var termination = await GetByIdAsync(approval.TerminationId);
            if (termination == null || termination.Status != "pending_approval")
                return false;

            if (approval.Approved)
            {
                termination.Status = "approved";
                termination.ApprovedBy = approverId;
                termination.ApprovedDate = DateTime.Now;
                termination.ApprovalComments = approval.Comments;

                // Set effective date if not already set
                if (!termination.EffectiveDate.HasValue)
                {
                    termination.EffectiveDate = termination.LastWorkingDate;
                }
            }
            else
            {
                termination.Status = "cancelled";
                termination.ApprovalComments = approval.Comments;
            }

            termination.Update_time = DateTime.Now;
            return await UpdateAsync(termination);
        }

        public async Task<TerminationSettlementDto> CalculateSettlementAsync(long terminationId)
        {
            var termination = await GetByIdAsync(terminationId);
            if (termination == null)
                throw new Exception("Termination record not found.");

            // Calculate ESB using existing PayrollService
            var esbResult = await _payrollService.CalculateESBAsync(
                termination.EmployeeId,
                termination.TerminationType);

            // Calculate leave encashment (unused annual leave days × daily salary)
            decimal leaveEncashment = 0;
            try
            {
                var balanceSummary = await _leaveBalanceService.GetBalanceSummaryAsync(termination.EmployeeId);
                // Get total available days from balance summary
                var summaryType = balanceSummary.GetType();
                var totalBalanceProp = summaryType.GetProperty("TotalBalance");
                if (totalBalanceProp != null)
                {
                    var totalBalance = Convert.ToDecimal(totalBalanceProp.GetValue(balanceSummary));
                    var dailySalary = esbResult.BasicSalary / 30m;
                    leaveEncashment = totalBalance * dailySalary;
                }
            }
            catch { /* Balance may not exist */ }

            // Calculate pending loans
            decimal pendingLoans = 0;
            try
            {
                var loans = await Context.Queryable<ZR.Model.HR.Payroll.HrLoan>()
                    .Where(l => l.EmployeeId == termination.EmployeeId &&
                                (l.Status == "active" || l.Status == "approved"))
                    .ToListAsync();

                pendingLoans = loans.Sum(l => l.RemainingAmount);
            }
            catch { /* Loan table may not have data */ }

            // Final settlement = ESB + Leave Encashment - Loans - Other Deductions
            var finalSettlement = esbResult.FinalESBAmount + leaveEncashment - pendingLoans - (termination.OtherDeductions ?? 0);

            // Update termination record
            termination.ESBAmount = esbResult.FinalESBAmount;
            termination.LeaveEncashmentAmount = leaveEncashment;
            termination.PendingLoanAmount = pendingLoans;
            termination.FinalSettlementAmount = finalSettlement;
            termination.Status = "pending_settlement";
            termination.Update_time = DateTime.Now;
            await UpdateAsync(termination);

            return new TerminationSettlementDto
            {
                TerminationId = terminationId,
                ESBAmount = esbResult.FinalESBAmount,
                LeaveEncashmentAmount = leaveEncashment,
                PendingLoanAmount = pendingLoans,
                OtherDeductions = termination.OtherDeductions ?? 0,
                FinalSettlementAmount = finalSettlement,
                BasicSalary = esbResult.BasicSalary,
                TotalServiceYears = esbResult.TotalServiceYears,
                EligibilityNote = esbResult.EligibilityNote
            };
        }

        public async Task<bool> UpdateChecklistAsync(TerminationChecklistDto checklist)
        {
            var termination = await GetByIdAsync(checklist.TerminationId);
            if (termination == null) return false;

            termination.AssetReturnCompleted = checklist.AssetReturnCompleted;
            termination.ITAccessRevoked = checklist.ITAccessRevoked;
            termination.ExitInterviewCompleted = checklist.ExitInterviewCompleted;
            termination.ClearanceCompleted = checklist.ClearanceCompleted;
            termination.ExperienceLetterIssued = checklist.ExperienceLetterIssued;
            termination.Update_time = DateTime.Now;

            return await UpdateAsync(termination);
        }

        public async Task<bool> CompleteTerminationAsync(long terminationId)
        {
            var termination = await GetByIdAsync(terminationId);
            if (termination == null) return false;

            // Verify checklist items
            if (!termination.ClearanceCompleted)
                throw new Exception("Clearance must be completed before finalizing termination.");

            // Deactivate the employee
            var employee = await Context.Queryable<HrEmployeeCore>()
                .FirstAsync(e => e.EmployeeId == termination.EmployeeId);

            if (employee != null)
            {
                employee.IsActive = false;
                employee.TerminationDate = termination.EffectiveDate ?? DateTime.Now;
                await Context.Updateable(employee)
                    .UpdateColumns(e => new { e.IsActive, e.TerminationDate })
                    .ExecuteCommandAsync();
            }

            // Create employee status record
            var statusRecord = new HrEmployeeStatus
            {
                EmployeeId = termination.EmployeeId,
                EffectiveFrom = termination.EffectiveDate ?? DateTime.Now,
                Create_time = DateTime.Now,
                Create_by = "System"
            };
            await Context.Insertable(statusRecord).ExecuteCommandAsync();

            // Mark termination as completed
            termination.Status = "completed";
            termination.Update_time = DateTime.Now;
            return await UpdateAsync(termination);
        }

        public async Task<bool> CancelTerminationAsync(long terminationId, string reason)
        {
            var termination = await GetByIdAsync(terminationId);
            if (termination == null || termination.Status == "completed")
                return false;

            termination.Status = "cancelled";
            termination.Notes = reason;
            termination.Update_time = DateTime.Now;
            return await UpdateAsync(termination);
        }
    }
}
