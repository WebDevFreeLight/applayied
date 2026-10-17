using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository.EventManagement
{
    /// <summary>
    /// Event Booth History - سجل الأكشاك
    /// Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventBoothHistoryRepository : BaseRepository<EventBoothHistory>
    {
    }
}

