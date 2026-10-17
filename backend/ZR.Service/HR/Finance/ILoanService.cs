using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Payroll;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Loan Service Interface
    /// </summary>
    public interface ILoanService
    {
        // Loan Management
        Task<PagedInfo<HrLoan>> GetLoansAsync(Expression<Func<HrLoan, bool>> predicate, PagerInfo pager);
        Task<List<HrLoan>> GetEmployeeLoansAsync(long employeeId);
        Task<List<HrLoan>> GetActiveLoansAsync();
        Task<HrLoan> GetLoanByIdAsync(long loanId);
        Task<int> ApplyForLoanAsync(HrLoan loan);
        Task<int> UpdateLoanAsync(HrLoan loan);
        Task<int> CancelLoanAsync(long loanId, string reason);
        
        // Loan Approval
        Task<int> ApproveLoanAsync(long loanId, long approvedBy, string comments);
        Task<int> RejectLoanAsync(long loanId, long rejectedBy, string reason);
        Task<List<HrLoan>> GetPendingLoansAsync();
        Task<List<HrLoan>> GetLoansForApprovalAsync(long approverId);
        
        // Loan Repayment
        Task<List<HrLoanRepayment>> GetLoanRepaymentsAsync(long loanId);
        Task<List<HrLoanRepayment>> GetEmployeeRepaymentsAsync(long employeeId, string payPeriod);
        Task<int> ProcessRepaymentAsync(HrLoanRepayment repayment);
        Task<int> ProcessRepayment(long repaymentId);
        Task<int> ProcessMonthlyRepaymentAsync(string payPeriod);
        Task<decimal> CalculateMonthlyRepaymentAsync(long loanId);
        Task<decimal> GetOutstandingBalanceAsync(long loanId);
        Task<int> RecalculateRepaymentScheduleAsync(long loanId);
        
        // Early Settlement
        Task<decimal> CalculateEarlySettlementAmountAsync(long loanId);
        Task<int> ProcessEarlySettlementAsync(long loanId, decimal amount, string paymentMethod);
        

        // Loan Analytics
        Task<object> GetLoanStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<object> GetEmployeeLoanSummaryAsync(long employeeId);
        Task<decimal> GetTotalOutstandingLoansAsync();
        Task<List<LoanTypeSummaryDto>> GetLoansByTypeAsync();
        Task<object> GetRepaymentAnalyticsAsync(int year);
        Task<List<DefaultedLoanDto>> GetDefaultedLoansAsync();
        
        // Loan Eligibility
        Task<bool> CheckLoanEligibilityAsync(long employeeId, string loanType);
        Task<decimal> GetMaxLoanAmountAsync(long employeeId, string loanType);
        Task<object> GetEligibilityCriteriaAsync(string loanType);
        
        // Loan Documents
        Task<int> AttachLoanDocumentAsync(long loanId, string documentPath);
        Task<List<string>> GetLoanDocumentsAsync(long loanId);
        
        // Reports
        Task<byte[]> GenerateLoanStatementAsync(long loanId);
        Task<byte[]> GenerateRepaymentScheduleAsync(long loanId);
        Task<byte[]> ExportLoansReportAsync(Expression<Func<HrLoan, bool>> predicate, string format);
        
        // Bulk Operations
        Task<int> ProcessBulkRepaymentAsync(List<HrLoanRepayment> repayments);
        Task<int> UpdateMultipleLoansStatusAsync(List<long> loanIds, string status);
    }
}
