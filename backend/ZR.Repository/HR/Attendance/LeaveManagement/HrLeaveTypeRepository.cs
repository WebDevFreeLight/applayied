using Infrastructure.Attribute;
using ZR.Model.HR.Attendance.LeaveManagement;

namespace ZR.Repository.HR.Attendance.LeaveManagement
{
    /// <summary>
    /// HR Leave Type Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrLeaveTypeRepository : BaseRepository<HrLeaveType>
    {
        // Inherits all CRUD operations from BaseRepository
    }
}
