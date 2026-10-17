using Infrastructure.Attribute;
using ZR.Model.HR.Attendance.AttendanceCapture;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Attendance Devices
    /// Manages biometric devices and attendance capture hardware
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrAttendanceDeviceRepository : BaseRepository<HrAttendanceDevice>
    {
    }
}
