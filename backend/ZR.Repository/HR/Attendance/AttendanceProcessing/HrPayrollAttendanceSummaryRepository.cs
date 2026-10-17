using Infrastructure.Attribute;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.AttendanceProcessing;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Payroll Attendance Summary
    /// Manages payroll period attendance summaries - CRITICAL for payroll
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrPayrollAttendanceSummaryRepository : BaseRepository<HrPayrollAttendanceSummary>
    {
        /// <summary>
        /// Get summary for a specific employee and payroll period
        /// </summary>
        public async Task<HrPayrollAttendanceSummary> GetSummaryByEmployeeAndPeriod(long employeeId, string payrollPeriod)
        {
            return await Context.Queryable<HrPayrollAttendanceSummary>()
                .Where(sum => sum.EmployeeId == employeeId && sum.PayrollPeriod == payrollPeriod)
                .FirstAsync();
        }

        /// <summary>
        /// Get all summaries for a payroll period
        /// </summary>
        public async Task<List<HrPayrollAttendanceSummary>> GetSummariesByPeriod(string payrollPeriod)
        {
            return await Context.Queryable<HrPayrollAttendanceSummary>()
                .Where(sum => sum.PayrollPeriod == payrollPeriod)
                .OrderBy(sum => sum.EmployeeId)
                .ToListAsync();
        }

        /// <summary>
        /// Get all finalized summaries for a payroll period
        /// </summary>
        public async Task<List<HrPayrollAttendanceSummary>> GetFinalizedSummariesByPeriod(string payrollPeriod)
        {
            return await Context.Queryable<HrPayrollAttendanceSummary>()
                .Where(sum => sum.PayrollPeriod == payrollPeriod && sum.IsFinalized)
                .OrderBy(sum => sum.EmployeeId)
                .ToListAsync();
        }

        /// <summary>
        /// Check if summary exists for employee and period
        /// </summary>
        public async Task<bool> SummaryExists(long employeeId, string payrollPeriod)
        {
            return await Context.Queryable<HrPayrollAttendanceSummary>()
                .Where(sum => sum.EmployeeId == employeeId && sum.PayrollPeriod == payrollPeriod)
                .AnyAsync();
        }
    }
}
