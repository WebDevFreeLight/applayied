using Infrastructure.Attribute;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.AttendanceProcessing;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Attendance Segment Results
    /// Manages segment-level attendance for split shifts
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrAttendanceSegmentResultRepository : BaseRepository<HrAttendanceSegmentResult>
    {
        /// <summary>
        /// Get segment results for an employee and date
        /// </summary>
        public async Task<List<HrAttendanceSegmentResult>> GetSegmentResultsByEmployeeAndDate(long employeeId, DateTime date)
        {
            return await Context.Queryable<HrAttendanceSegmentResult>()
                .Where(seg => seg.EmployeeId == employeeId && seg.AttendanceDate == date.Date)
                .OrderBy(seg => seg.SegmentId)
                .ToListAsync();
        }
    }
}
