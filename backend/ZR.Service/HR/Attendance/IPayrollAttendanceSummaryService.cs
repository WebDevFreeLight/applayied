using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Dto.Attendance;

namespace ZR.Service.HR.IService
{
    /// <summary>
    /// Interface for Payroll Attendance Summary Service
    /// CRITICAL SERVICE - Generates attendance summaries for payroll processing
    /// </summary>
    public interface IPayrollAttendanceSummaryService
    {
        /// <summary>
        /// Generate payroll attendance summary for a specific employee and period
        /// Days-based calculation per user preference
        /// </summary>
        Task<PayrollAttendanceSummaryDto> GenerateSummaryForEmployee(long employeeId, string payrollPeriod);

        /// <summary>
        /// Generate payroll attendance summaries for all employees for a period
        /// </summary>
        Task<List<PayrollAttendanceSummaryDto>> GenerateSummariesForPeriod(string payrollPeriod);

        /// <summary>
        /// Get existing summary for an employee and period
        /// </summary>
        Task<PayrollAttendanceSummaryDto> GetSummary(long employeeId, string payrollPeriod);

        /// <summary>
        /// Get all summaries for a payroll period
        /// </summary>
        Task<List<PayrollAttendanceSummaryDto>> GetSummariesByPeriod(string payrollPeriod);

        /// <summary>
        /// Finalize a summary (lock it for payroll processing)
        /// </summary>
        Task<bool> FinalizeSummary(long summaryId, long finalizedBy);

        /// <summary>
        /// Finalize all summaries for a payroll period
        /// </summary>
        Task<int> FinalizeAllSummariesForPeriod(string payrollPeriod, long finalizedBy);

        /// <summary>
        /// Recalculate summary for an employee and period
        /// Useful if attendance data was corrected after summary generation
        /// </summary>
        Task<PayrollAttendanceSummaryDto> RecalculateSummary(long employeeId, string payrollPeriod);

        /// <summary>
        /// Validate summary data for a period
        /// Returns list of validation errors
        /// </summary>
        Task<List<string>> ValidateSummaries(string payrollPeriod);
    }
}
