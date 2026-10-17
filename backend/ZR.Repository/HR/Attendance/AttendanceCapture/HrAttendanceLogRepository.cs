using Infrastructure.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.AttendanceCapture;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrAttendanceLogRepository : BaseRepository<HrAttendanceLog>
    {
        public async Task<List<HrAttendanceLog>> GetLogsByEmployeeAndDate(long employeeId, DateTime date)
        {
            return await Context.Queryable<HrAttendanceLog>()
                .Where(log => log.EmployeeId == employeeId &&
                       log.LogTime.Date == date.Date)
                .OrderBy(log => log.LogTime)
                .ToListAsync();
        }
    }
}
