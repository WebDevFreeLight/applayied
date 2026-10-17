using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Event Status - أنواع حالة الفعاليات仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventStatusRepository : BaseRepository<EventStatus>
    {
    }
}
