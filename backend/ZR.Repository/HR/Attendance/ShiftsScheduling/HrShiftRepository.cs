using Infrastructure.Attribute;
using ZR.Model.HR.Attendance.Shifts_Scheduling;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Shifts
    /// Manages shift definitions and configurations
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrShiftRepository : BaseRepository<HrShift>
    {
    }
}
