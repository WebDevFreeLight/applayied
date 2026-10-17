using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository.EventManagement
{
    /// <summary>
    /// Event Zone - المناطق داخل الفعالية
    /// Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventZoneRepository : BaseRepository<EventZone>
    {
    }
}

