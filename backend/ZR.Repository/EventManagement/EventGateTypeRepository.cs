using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Gate Type - أنواع البوابات仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventGateTypeRepository : BaseRepository<EventGateType>
    {
    }
}
