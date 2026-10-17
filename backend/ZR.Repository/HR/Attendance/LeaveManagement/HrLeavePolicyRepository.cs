using Infrastructure.Attribute;
using ZR.Model.HR.Attendance.LeaveManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Leave Policies
    /// Manages leave policy rules and configurations
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrLeavePolicyRepository : BaseRepository<HrLeavePolicy>
    {
    }
}
