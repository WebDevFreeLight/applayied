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
using ZR.Model.HR.Dto;
using ZR.Service.HR.IService;
using ZR.Model.HR.Payroll;

namespace ZR.Admin.WebApi.Controllers.HR.Finance
{
    /// <summary>
    /// Loan Management Controller
    /// </summary>
    [Verify]
    [Route("api/hr/loans")]
    [ApiExplorerSettings(GroupName = "hr")]
    public class LoanController : BaseController
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        #region Loan Management

        /// <summary>
        /// Get loans
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetLoans([FromQuery] LoanQueryDto dto)
        {
            var predicate = Expressionable.Create<HrLoan>();
            
            predicate = predicate.AndIF(dto.EmployeeId > 0, 
                it => it.EmployeeId == dto.EmployeeId);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.LoanType), 
                it => it.LoanType == dto.LoanType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(dto.Status), 
                it => it.Status == dto.Status);
            predicate = predicate.AndIF(dto.StartDate != null, 
                it => it.ApplicationDate >= dto.StartDate);
            predicate = predicate.AndIF(dto.EndDate != null, 
                it => it.ApplicationDate <= dto.EndDate);
            predicate = predicate.AndIF(dto.MinAmount.HasValue, 
                it => it.LoanAmount >= dto.MinAmount);
            predicate = predicate.AndIF(dto.MaxAmount.HasValue, 
                it => it.LoanAmount <= dto.MaxAmount);
            
            var response = await _loanService.GetLoansAsync(predicate.ToExpression(), dto);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get employee loans
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetEmployeeLoans(long employeeId)
        {
            var loans = await _loanService.GetEmployeeLoansAsync(employeeId);
            return SUCCESS(loans);
        }

        /// <summary>
        /// Get active loans
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetActiveLoans()
        {
            var loans = await _loanService.GetActiveLoansAsync();
            return SUCCESS(loans);
        }

        /// <summary>
        /// Get loan by ID
        /// </summary>
        [HttpGet("{loanId}")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetLoanById(long loanId)
        {
            var loan = await _loanService.GetLoanByIdAsync(loanId);
            return SUCCESS(loan);
        }

        /// <summary>
        /// Apply for loan
        /// </summary>
        [HttpPost("apply")]
        [ActionPermissionFilter("hr:loans:apply")]
        [Log(Title = "Loan Application", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> ApplyForLoan([FromBody] LoanApplicationDto dto)
        {
            var loan = dto.Adapt<HrLoan>();
            loan.Create_by = HttpContext.GetName();
            loan.Create_time = DateTime.Now;
            loan.ApplicationDate = DateTime.Now;
            loan.Status = "pending";
            
            var result = await _loanService.ApplyForLoanAsync(loan);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update loan
        /// </summary>
        [HttpPut("{loanId}")]
        [ActionPermissionFilter("hr:loans:edit")]
        [Log(Title = "Loan Management", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateLoan(long loanId, [FromBody] LoanDto dto)
        {
            var loan = dto.Adapt<HrLoan>();
            loan.LoanId = loanId;
            loan.Update_by = HttpContext.GetName();
            loan.Update_time = DateTime.Now;
            
            var result = await _loanService.UpdateLoanAsync(loan);
            return SUCCESS(result);
        }

        /// <summary>
        /// Cancel loan
        /// </summary>
        [HttpPost("{loanId}/cancel")]
        [ActionPermissionFilter("hr:loans:cancel")]
        [Log(Title = "Loan Cancellation", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> CancelLoan(long loanId, [FromBody] CancelLoanDto dto)
        {
            var result = await _loanService.CancelLoanAsync(loanId, dto.Reason);
            return SUCCESS(result);
        }

        #endregion

        #region Loan Approval

        /// <summary>
        /// Approve loan 0
        /// </summary>
        [HttpPost("approve")]
        [ActionPermissionFilter("hr:loans:approve")]
        [Log(Title = "Loan Approval", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApproveLoan(  [FromBody] ApproveLoanDto dto)
        {
            var approvedBy = HttpContext.GetUId();
            var result = await _loanService.ApproveLoanAsync(dto.LoanId, approvedBy, dto.Comments);
            return SUCCESS(result);
        }

        /// <summary>
        /// Approve loan (path-based compatibility for frontend)
        /// </summary>
        [HttpPost("{loanId}/approve")]
        [ActionPermissionFilter("hr:loans:approve")]
        [Log(Title = "Loan Approval", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ApproveLoanById(long loanId, [FromBody] ApproveLoanDto dto)
        {
            var approvedBy = HttpContext.GetUId();
            var comments = dto?.Comments ?? dto?.ApprovalComments;
            var result = await _loanService.ApproveLoanAsync(loanId, approvedBy, comments);
            return SUCCESS(result);
        }


        /// <summary>
        /// Reject loan
        /// </summary>
        [HttpPost("reject")]
        [ActionPermissionFilter("hr:loans:approve")]
        [Log(Title = "Loan RejectionAio", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RejectLoan([FromBody] RejectLoanDto dto)
        {
            var rejectedBy = HttpContext.GetUId();
            var result = await _loanService.RejectLoanAsync(dto.LoanId, rejectedBy, dto.Reason);
            return SUCCESS(result);
        }

        /// <summary>
        /// Reject loan (path-based compatibility for frontend)
        /// </summary>
        [HttpPost("{loanId}/reject")]
        [ActionPermissionFilter("hr:loans:approve")]
        [Log(Title = "Loan Rejection", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RejectLoanById(long loanId, [FromBody] RejectLoanDto dto)
        {
            var rejectedBy = HttpContext.GetUId();
            var reason = dto?.Reason ?? dto?.RejectionReason;
            var result = await _loanService.RejectLoanAsync(loanId, rejectedBy, reason);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get pending loans
        /// </summary>
        [HttpGet("pending")]
        [ActionPermissionFilter("hr:loans:approve")]
        public async Task<IActionResult> GetPendingLoans()
        {
            var loans = await _loanService.GetPendingLoansAsync();
            return SUCCESS(loans);
        }

        /// <summary>
        /// Get loans for approval
        /// </summary>
        [HttpGet("for-approval/{approverId}")]
        [ActionPermissionFilter("hr:loans:approve")]
        public async Task<IActionResult> GetLoansForApproval(long approverId)
        {
            var loans = await _loanService.GetLoansForApprovalAsync(approverId);
            return SUCCESS(loans);
        }

        #endregion

        #region Loan Repayment

        /// <summary>
        /// Get loan repayments
        /// </summary>
        [HttpGet("{loanId}/repayments")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetLoanRepayments(long loanId)
        {
            var repayments = await _loanService.GetLoanRepaymentsAsync(loanId);
            return SUCCESS(repayments);
        }

        /// <summary>
        /// Get employee repayments
        /// </summary>
        [HttpGet("repayments/employee/{employeeId}")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetEmployeeRepayments(long employeeId, [FromQuery] string payPeriod)
        {
            var repayments = await _loanService.GetEmployeeRepaymentsAsync(employeeId, payPeriod);
            return SUCCESS(repayments);
        }

        /// <summary>
        /// Process repayment
        /// </summary>
        [HttpPost("repayments")]
        [ActionPermissionFilter("hr:loans:repay")]
        [Log(Title = "Loan Repayment", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> ProcessRepayment([FromBody] LoanRepaymentDto dto)
        {
            var repayment = dto.Adapt<HrLoanRepayment>();
            repayment.Create_by = HttpContext.GetName();
            repayment.Create_time = DateTime.Now;
            repayment.PaymentDate = DateTime.Now;
            
            var result = await _loanService.ProcessRepaymentAsync(repayment);
            return SUCCESS(result);
        }

        /// <summary>
        /// Process monthly repayments
        /// </summary>
        [HttpPost("repayments/process-monthly")]
        [ActionPermissionFilter("hr:loans:process")]
        [Log(Title = "Monthly Loan Repayments", BusinessType = BusinessType.OTHER)]
        public async Task<IActionResult> ProcessMonthlyRepayment([FromBody] ProcessMonthlyRepaymentDto dto)
        {
            var result = await _loanService.ProcessMonthlyRepaymentAsync(dto.PayPeriod);
            return SUCCESS(new { ProcessedCount = result });
        }

        /// <summary>
        /// Calculate monthly repayment
        /// </summary>
        [HttpGet("{loanId}/monthly-repayment")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> CalculateMonthlyRepayment(long loanId)
        {
            var amount = await _loanService.CalculateMonthlyRepaymentAsync(loanId);
            return SUCCESS(new { MonthlyRepayment = amount });
        }

        /// <summary>
        /// Get outstanding balance
        /// </summary>
        [HttpGet("{loanId}/outstanding-balance")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetOutstandingBalance(long loanId)
        {
            var balance = await _loanService.GetOutstandingBalanceAsync(loanId);
            return SUCCESS(new { OutstandingBalance = balance });
        }

        /// <summary>
        /// Recalculate repayment schedule
        /// </summary>
        [HttpPost("{loanId}/recalculate-schedule")]
        [ActionPermissionFilter("hr:loans:edit")]
        [Log(Title = "Recalculate Repayment Schedule", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> RecalculateRepaymentSchedule(long loanId)
        {
            var result = await _loanService.RecalculateRepaymentScheduleAsync(loanId);
            return SUCCESS(result);
        }

        #endregion

        #region Early Settlement

        /// <summary>
        /// Calculate early settlement amount
        /// </summary>
        [HttpGet("{loanId}/early-settlement-amount")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> CalculateEarlySettlementAmount(long loanId)
        {
            var amount = await _loanService.CalculateEarlySettlementAmountAsync(loanId);
            return SUCCESS(new { EarlySettlementAmount = amount });
        }

        /// <summary>
        /// Process early settlement
        /// </summary>
        [HttpPost("{loanId}/early-settlement")]
        [ActionPermissionFilter("hr:loans:settle")]
        [Log(Title = "Early Loan Settlement", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ProcessEarlySettlement(long loanId, [FromBody] EarlySettlementDto dto)
        {
            var result = await _loanService.ProcessEarlySettlementAsync(loanId, dto.Amount, dto.PaymentMethod);
            return SUCCESS(result);
        }

        #endregion

        #region Loan Analytics

        /// <summary>
        /// Get loan statistics
        /// </summary>
        [HttpGet("statistics")]
        [ActionPermissionFilter("hr:loans:analytics")]
        public async Task<IActionResult> GetLoanStatistics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var statistics = await _loanService.GetLoanStatisticsAsync(startDate, endDate);
            return SUCCESS(statistics);
        }

        /// <summary>
        /// Get employee loan summary
        /// </summary>
        [HttpGet("summary/employee/{employeeId}")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetEmployeeLoanSummary(long employeeId)
        {
            var summary = await _loanService.GetEmployeeLoanSummaryAsync(employeeId);
            return SUCCESS(summary);
        }

        /// <summary>
        /// Get total outstanding loans
        /// </summary>
        [HttpGet("total-outstanding")]
        [ActionPermissionFilter("hr:loans:analytics")]
        public async Task<IActionResult> GetTotalOutstandingLoans()
        {
            var total = await _loanService.GetTotalOutstandingLoansAsync();
            return SUCCESS(new { TotalOutstanding = total });
        }

        /// <summary>
        /// Get loans by type
        /// </summary>
        [HttpGet("by-type")]
        [ActionPermissionFilter("hr:loans:analytics")]
        public async Task<IActionResult> GetLoansByType()
        {
            var loansByType = await _loanService.GetLoansByTypeAsync();
            return SUCCESS(loansByType);
        }

        /// <summary>
        /// Get repayment analytics
        /// </summary>
        [HttpGet("repayment-analytics")]
        [ActionPermissionFilter("hr:loans:analytics")]
        public async Task<IActionResult> GetRepaymentAnalytics([FromQuery] int year)
        {
            var analytics = await _loanService.GetRepaymentAnalyticsAsync(year);
            return SUCCESS(analytics);
        }

        /// <summary>
        /// Get defaulted loans
        /// </summary>
        [HttpGet("defaulted")]
        [ActionPermissionFilter("hr:loans:analytics")]
        public async Task<IActionResult> GetDefaultedLoans()
        {
            var defaultedLoans = await _loanService.GetDefaultedLoansAsync();
            return SUCCESS(defaultedLoans);
        }

        #endregion

        #region Loan Eligibility

        /// <summary>
        /// Check loan eligibility
        /// </summary>
        [HttpGet("eligibility/{employeeId}")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> CheckLoanEligibility(long employeeId, [FromQuery] string loanType)
        {
            var eligible = await _loanService.CheckLoanEligibilityAsync(employeeId, loanType);
            return SUCCESS(new { IsEligible = eligible });
        }

        /// <summary>
        /// Get maximum loan amount
        /// </summary>
        [HttpGet("max-amount/{employeeId}")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetMaxLoanAmount(long employeeId, [FromQuery] string loanType)
        {
            var maxAmount = await _loanService.GetMaxLoanAmountAsync(employeeId, loanType);
            return SUCCESS(new { MaxAmount = maxAmount });
        }

        /// <summary>
        /// Get eligibility criteria
        /// </summary>
        [HttpGet("eligibility-criteria")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetEligibilityCriteria([FromQuery] string loanType)
        {
            var criteria = await _loanService.GetEligibilityCriteriaAsync(loanType);
            return SUCCESS(criteria);
        }

        #endregion

        #region Loan Documents

        /// <summary>
        /// Attach loan document
        /// </summary>
        [HttpPost("{loanId}/documents")]
        [ActionPermissionFilter("hr:loans:documents")]
        [Log(Title = "Attach Loan Document", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AttachLoanDocument(long loanId, [FromBody] AttachDocumentDto dto)
        {
            var result = await _loanService.AttachLoanDocumentAsync(loanId, dto.DocumentPath);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get loan documents
        /// </summary>
        [HttpGet("{loanId}/documents")]
        [ActionPermissionFilter("hr:loans:list")]
        public async Task<IActionResult> GetLoanDocuments(long loanId)
        {
            var documents = await _loanService.GetLoanDocumentsAsync(loanId);
            return SUCCESS(documents);
        }

        #endregion

        #region Reports

        /// <summary>
        /// Generate loan statement
        /// </summary>
        [HttpGet("{loanId}/statement")]
        [ActionPermissionFilter("hr:loans:reports")]
        [Log(Title = "Generate Loan Statement", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> GenerateLoanStatement(long loanId)
        {
            var statementData = await _loanService.GenerateLoanStatementAsync(loanId);
            var fileName = $"Loan_Statement_{loanId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            return File(statementData, "application/pdf", fileName);
        }

        /// <summary>
        /// Generate repayment schedule
        /// </summary>
        [HttpGet("{loanId}/repayment-schedule")]
        [ActionPermissionFilter("hr:loans:reports")]
        [Log(Title = "Generate Repayment Schedule", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> GenerateRepaymentSchedule(long loanId)
        {
            var scheduleData = await _loanService.GenerateRepaymentScheduleAsync(loanId);
            var fileName = $"Repayment_Schedule_{loanId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            
            return File(scheduleData, "application/pdf", fileName);
        }

        /// <summary>
        /// Export loans report
        /// </summary>
        [HttpPost("export")]
        [ActionPermissionFilter("hr:loans:export")]
        [Log(Title = "Export Loans Report", BusinessType = BusinessType.EXPORT)]
        public async Task<IActionResult> ExportLoansReport([FromBody] ExportLoansDto dto)
        {
            var predicate = Expressionable.Create<HrLoan>();
            
            if (dto.EmployeeId > 0)
                predicate = predicate.And(it => it.EmployeeId == dto.EmployeeId);
            if (!string.IsNullOrEmpty(dto.LoanType))
                predicate = predicate.And(it => it.LoanType == dto.LoanType);
            if (!string.IsNullOrEmpty(dto.Status))
                predicate = predicate.And(it => it.Status == dto.Status);
            if (dto.StartDate != null)
                predicate = predicate.And(it => it.ApplicationDate >= dto.StartDate);
            if (dto.EndDate != null)
                predicate = predicate.And(it => it.ApplicationDate <= dto.EndDate);
            
            var reportData = await _loanService.ExportLoansReportAsync(predicate.ToExpression(), dto.Format);
            var fileName = $"Loans_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{dto.Format.ToLower()}";
            var contentType = dto.Format.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            
            return File(reportData, contentType, fileName);
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Process bulk repayments
        /// </summary>
        [HttpPost("repayments/bulk")]
        [ActionPermissionFilter("hr:loans:bulk")]
        [Log(Title = "Bulk Loan Repayments", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ProcessBulkRepayment([FromBody] List<LoanRepaymentDto> repaymentDtos)
        {
            var repayments = repaymentDtos.Adapt<List<HrLoanRepayment>>();
            foreach (var repayment in repayments)
            {
                repayment.Create_by = HttpContext.GetName();
                repayment.Create_time = DateTime.Now;
                repayment.PaymentDate = DateTime.Now;
            }
            
            var result = await _loanService.ProcessBulkRepaymentAsync(repayments);
            return SUCCESS(new { ProcessedCount = result });
        }

        /// <summary>
        /// Update multiple loans status
        /// </summary>
        [HttpPost("bulk-status-update")]
        [ActionPermissionFilter("hr:loans:bulk")]
        [Log(Title = "Bulk Loan Status Update", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateMultipleLoansStatus([FromBody] BulkStatusUpdateDto dto)
        {
            var result = await _loanService.UpdateMultipleLoansStatusAsync(dto.LoanIds, dto.Status);
            return SUCCESS(new { UpdatedCount = result });
        }

        #endregion
    }
}
