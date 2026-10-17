using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Event Type - أنواع الفعاليات仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventTypeRepository : BaseRepository<EventType>
    {
    }
}
