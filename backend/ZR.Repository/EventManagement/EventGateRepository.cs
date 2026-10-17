using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Event Gate - بوابات الدخول仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventGateRepository : BaseRepository<EventGate>
    {
    }
}
