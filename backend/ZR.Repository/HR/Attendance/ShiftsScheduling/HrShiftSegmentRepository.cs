using Infrastructure.Attribute;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Shifts_Scheduling;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Shift Segments
    /// Manages time segments within a shift (for split shifts)
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrShiftSegmentRepository : BaseRepository<HrShiftSegment>
    {
        /// <summary>
        /// Get all segments for a specific shift
        /// </summary>
        public async Task<List<HrShiftSegment>> GetSegmentsByShift(long shiftId)
        {
            return await Context.Queryable<HrShiftSegment>()
                .Where(seg => seg.ShiftId == shiftId)
                .OrderBy(seg => seg.SegmentOrder)
                .ToListAsync();
        }
    }
}
