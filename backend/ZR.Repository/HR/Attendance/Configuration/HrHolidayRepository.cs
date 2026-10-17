using Infrastructure.Attribute;
using System;
using System.Threading.Tasks;
using ZR.Model.HR.Attendance.Configuration;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrHolidayRepository : BaseRepository<HrHoliday>
    {
        public async Task<bool> IsHoliday(DateTime date)
        {
            return await Context.Queryable<HrHoliday>()
                .AnyAsync(h => h.Holidaydate.Date == date.Date);
        }
    }
}
