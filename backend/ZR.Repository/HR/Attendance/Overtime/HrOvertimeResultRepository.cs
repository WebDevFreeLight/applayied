using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Overtime;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Overtime Results
    /// Manages calculated overtime results for payroll
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrOvertimeResultRepository : BaseRepository<HrOvertimeResult>
    {
        /// <summary>
        /// Get overtime results for a payroll period
        /// </summary>
        public async Task<List<HrOvertimeResult>> GetOvertimeByPayrollPeriod(long employeeId, string payrollPeriod)
        {
            // Parse payroll period (e.g., "2025-01")
            var parts = payrollPeriod.Split('-');
            if (parts.Length != 2) return new List<HrOvertimeResult>();

            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await Context.Queryable<HrOvertimeResult>()
                .Where(ot => ot.EmployeeId == employeeId
                    && ot.WorkDate >= startDate
                    && ot.WorkDate <= endDate)
                .OrderBy(ot => ot.WorkDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get overtime results for an employee within a date range
        /// </summary>
        public async Task<List<HrOvertimeResult>> GetOvertimeByDateRange(long employeeId, DateTime fromDate, DateTime toDate)
        {
            return await Context.Queryable<HrOvertimeResult>()
                .Where(ot => ot.EmployeeId == employeeId
                    && ot.WorkDate >= fromDate
                    && ot.WorkDate <= toDate)
                .OrderBy(ot => ot.WorkDate)
                .ToListAsync();
        }
    }
}
