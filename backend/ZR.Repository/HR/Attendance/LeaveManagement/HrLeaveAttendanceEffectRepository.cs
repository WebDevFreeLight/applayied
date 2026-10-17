using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.LeaveManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Leave Attendance Effects
    /// Manages leave impact on attendance records
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrLeaveAttendanceEffectRepository : BaseRepository<HrLeaveAttendanceEffect>
    {
        /// <summary>
        /// Get leave effects for an employee within a date range
        /// </summary>
        public async Task<List<HrLeaveAttendanceEffect>> GetEffectsByEmployeeAndDateRange(long employeeId, DateTime fromDate, DateTime toDate)
        {
            return await Context.Queryable<HrLeaveAttendanceEffect>()
                .Where(eff => eff.EmployeeId == employeeId
                    && eff.AffectedDate >= fromDate
                    && eff.AffectedDate <= toDate)
                .OrderBy(eff => eff.AffectedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get leave effects for a specific leave request
        /// </summary>
        public async Task<List<HrLeaveAttendanceEffect>> GetEffectsByLeaveRequest(long leaveRequestId)
        {
            return await Context.Queryable<HrLeaveAttendanceEffect>()
                .Where(eff => eff.LeaveRequestId == leaveRequestId)
                .OrderBy(eff => eff.AffectedDate)
                .ToListAsync();
        }
    }
}
