using Infrastructure.Attribute;
using ZR.Model.HR.Attendance.Overtime;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Overtime Policies
    /// Manages overtime calculation policies and rates
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrOvertimePolicyRepository : BaseRepository<HrOvertimePolicy>
    {
    }
}
