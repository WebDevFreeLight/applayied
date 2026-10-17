using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Overtime;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Overtime Segments
    /// Manages overtime work segments and time tracking
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrOvertimeSegmentRepository : BaseRepository<HrOvertimeSegment>
    {
        /// <summary>
        /// Get overtime segments for an employee by date range
        /// </summary>
        public async Task<List<HrOvertimeSegment>> GetSegmentsByDateRange(long employeeId, DateTime fromDate, DateTime toDate)
        {
            return await Context.Queryable<HrOvertimeSegment>()
                .Where(s => s.EmployeeId == employeeId
                    && s.WorkDate >= fromDate
                    && s.WorkDate <= toDate)
                .OrderBy(s => s.WorkDate)
                .OrderBy(s => s.OvertimeStartTime)
                .ToListAsync();
        }

        /// <summary>
        /// Get overtime segments for a specific work date
        /// </summary>
        public async Task<List<HrOvertimeSegment>> GetSegmentsByWorkDate(long employeeId, DateTime workDate)
        {
            return await Context.Queryable<HrOvertimeSegment>()
                .Where(s => s.EmployeeId == employeeId && s.WorkDate == workDate.Date)
                .OrderBy(s => s.OvertimeStartTime)
                .ToListAsync();
        }

        /// <summary>
        /// Get unverified overtime segments
        /// </summary>
        public async Task<List<HrOvertimeSegment>> GetUnverifiedSegments(long? employeeId = null)
        {
            var query = Context.Queryable<HrOvertimeSegment>()
                .LeftJoin<HrOvertimeVerification>((seg, ver) => seg.SegmentId == ver.SegmentId)
                .Where((seg, ver) => ver.SegmentId == null || ver.IsVerified == false);

            if (employeeId.HasValue)
            {
                query = query.Where((seg, ver) => seg.EmployeeId == employeeId.Value);
            }

            return await query.Select((seg, ver) => seg).ToListAsync();
        }
    }
}
