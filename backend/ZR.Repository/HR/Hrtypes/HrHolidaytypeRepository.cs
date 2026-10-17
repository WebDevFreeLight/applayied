using Infrastructure.Attribute;
using ZR.Model.HR.Hrtypes;

namespace ZR.Repository
{
    /// <summary>
    /// HolidayType Table仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrHolidaytypeRepository : BaseRepository<HrHolidaytype>
    {
    }
}
