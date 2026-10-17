using Infrastructure.Attribute;
using ZR.Model.HR.Attendance.Configuration;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Attendance Policies
    /// Manages attendance calculation rules and thresholds
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrAttendancePolicyRepository : BaseRepository<HrAttendancePolicy>
    {
    }
}
