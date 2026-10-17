using Infrastructure.Attribute;
using ZR.Model.HR.Attendance.AttendanceProcessing;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Attendance Adjustments
    /// Manages manual adjustments to attendance records
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrAttendanceAdjustmentRepository : BaseRepository<HrAttendanceAdjustment>
    {
    }
}
