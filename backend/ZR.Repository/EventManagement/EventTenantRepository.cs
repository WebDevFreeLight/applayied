using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository.EventManagement
{
    /// <summary>
    /// Event Tenant - المستأجرون
    /// Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventTenantRepository : BaseRepository<EventTenant>
    {
    }
}

