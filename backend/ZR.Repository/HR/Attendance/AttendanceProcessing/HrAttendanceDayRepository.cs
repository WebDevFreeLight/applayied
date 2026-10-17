using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.AttendanceProcessing;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Attendance Day
    /// Manages daily processed attendance records
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrAttendanceDayRepository : BaseRepository<HrAttendanceDay>
    {
        /// <summary>
        /// Get attendance records for an employee within a date range
        /// </summary>
        public async Task<List<HrAttendanceDay>> GetAttendanceByEmployeeAndDateRange(long employeeId, DateTime fromDate, DateTime toDate)
        {
            return await Context.Queryable<HrAttendanceDay>()
                .Where(att => att.EmployeeId == employeeId
                    && att.AttendanceDate >= fromDate
                    && att.AttendanceDate <= toDate)
                .OrderBy(att => att.AttendanceDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get attendance records for a payroll period
        /// </summary>
        public async Task<List<HrAttendanceDay>> GetAttendanceByPayrollPeriod(long employeeId, string payrollPeriod)
        {
            // Parse payroll period (e.g., "2025-01")
            var parts = payrollPeriod.Split('-');
            if (parts.Length != 2) return new List<HrAttendanceDay>();

            var year = int.Parse(parts[0]);
            var month = int.Parse(parts[1]);

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await GetAttendanceByEmployeeAndDateRange(employeeId, startDate, endDate);
        }

        /// <summary>
        /// Get attendance for a specific employee and date
        /// </summary>
        public async Task<HrAttendanceDay> GetAttendanceByEmployeeAndDate(long employeeId, DateTime date)
        {
            return await Context.Queryable<HrAttendanceDay>()
                .Where(att => att.EmployeeId == employeeId && att.AttendanceDate == date.Date)
                .FirstAsync();
        }
    }
}
