using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Event Location Information - معلومات موقع الفعالية仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventLocationRepository : BaseRepository<EventLocation>
    {
    }
}
