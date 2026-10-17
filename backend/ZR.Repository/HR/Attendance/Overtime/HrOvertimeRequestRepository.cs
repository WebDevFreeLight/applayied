using Infrastructure.Attribute;
using ZR.Model.HR.Attendance.Overtime;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrOvertimeRequestRepository : BaseRepository<HrOvertimeRequest>
    {
    }
}
