using Infrastructure.Attribute;
using System;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Shifts_Scheduling;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrShiftCalendarOverrideRepository : BaseRepository<HrShiftCalendarOverride>
    {
        public async Task<HrShiftCalendarOverride> GetOverrideByEmployeeAndDate(long employeeId, DateTime date)
        {
            return await Context.Queryable<HrShiftCalendarOverride>()
                .Where(o => o.EmployeeId == employeeId && o.CalendarDate.Date == date.Date)
                .FirstAsync();
        }
    }
}
