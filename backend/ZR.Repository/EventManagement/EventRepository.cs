using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Event Information - معلومات الفعالية仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventRepository : BaseRepository<Event>
    {
    }
}
