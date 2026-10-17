using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Payroll;

namespace ZR.Service.HR.IHrService
{
    /// <summary>
    /// HR Payroll Service Interface
    /// </summary>
    public interface IHrPayrollService
    {
        // Payroll Processing
        Task<HrPayrollDetail> CalculatePayrollAsync(long employeeId, DateTime payPeriodStart, DateTime payPeriodEnd);
        Task<List<HrPayrollDetail>> ProcessBatchPayrollAsync(DateTime payPeriodStart, DateTime payPeriodEnd);
        Task<bool> ApprovePayrollAsync(long payrollId, long approvedBy);
        Task<bool> RejectPayrollAsync(long payrollId, long rejectedBy, string reason);
        Task<bool> FinalizePayrollAsync(long payrollId);
        Task<bool> ReversePayrollAsync(long payrollId, string reason);
        
        // Payroll Details
        Task<HrPayrollDetail> GetPayrollDetailAsync(long payrollId);
        Task<List<HrPayrollDetail>> GetEmployeePayrollHistoryAsync(long employeeId, int year);
        Task<List<HrPayrollDetail>> GetPayrollByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<bool> UpdatePayrollDetailAsync(HrPayrollDetail detail);
        
        // Salary Components
        Task<decimal> CalculateBasicSalaryAsync(long employeeId, int workingDays, int actualDays);
        Task<decimal> CalculateOvertimeAsync(long employeeId, decimal overtimeHours);
        Task<decimal> CalculateAllowancesAsync(long employeeId);
        Task<decimal> CalculateBonusAsync(long employeeId, DateTime period);
        Task<decimal> CalculateDeductionsAsync(long employeeId);
        Task<decimal> CalculateGOSIAsync(decimal salary, bool isSaudi);
        Task<decimal> CalculateTaxAsync(decimal grossSalary);
        
        // Loan Management
        Task<List<HrLoan>> GetEmployeeLoansAsync(long employeeId);
        Task<int> AddLoanAsync(HrLoan loan);
        Task<bool> UpdateLoanAsync(HrLoan loan);
        Task<decimal> CalculateLoanDeductionAsync(long employeeId);
        Task<bool> ProcessLoanPaymentAsync(long loanId, decimal amount);
        
        // Bank Transfer
        Task<bool> GenerateBankFileAsync(List<long> payrollIds);
        Task<bool> MarkAsTransferredAsync(List<long> payrollIds, string referenceNumber);
        
        // Reports
        Task<byte[]> GeneratePayslipAsync(long payrollId);
        Task<byte[]> GenerateSalaryReportAsync(DateTime period);
        Task<byte[]> GenerateGOSIReportAsync(DateTime period);
        Task<byte[]> GenerateTaxReportAsync(int year);
    }
}
