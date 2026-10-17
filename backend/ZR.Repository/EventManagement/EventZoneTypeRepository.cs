using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Zone Type - أنواع المناطق仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventZoneTypeRepository : BaseRepository<EventZoneType>
    {
    }
}
