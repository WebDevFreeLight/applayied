using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository.EventManagement
{
    /// <summary>
    /// Event Booth - الأكشاك
    /// Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventBoothRepository : BaseRepository<EventBooth>
    {
    }
}

