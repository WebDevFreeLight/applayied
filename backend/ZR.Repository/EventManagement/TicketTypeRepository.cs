using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Ticket Type - أنواع التذاكر仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class TicketTypeRepository : BaseRepository<TicketType>
    {
    }
}
