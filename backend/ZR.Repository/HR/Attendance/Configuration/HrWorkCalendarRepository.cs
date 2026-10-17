using Infrastructure.Attribute;
using SqlSugar;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Configuration;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Work Calendars
    /// Manages work calendars defining working days and hours
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrWorkCalendarRepository : BaseRepository<HrWorkCalendar>
    {
        /// <summary>
        /// Get the default work calendar
        /// </summary>
        public async Task<HrWorkCalendar> GetDefaultCalendar()
        {
            return await Context.Queryable<HrWorkCalendar>()
                .Where(cal => cal.IsDefault && cal.IsActive)
                .FirstAsync();
        }
    }
}
