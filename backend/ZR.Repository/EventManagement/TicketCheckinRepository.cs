using Infrastructure.Attribute;
using Infrastructure.Extensions;
using ZR.Model.EventManagement;

namespace ZR.Repository.EventManagement
{
    /// <summary>
    /// Ticket Check-in/Checkout Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class TicketCheckinRepository : BaseRepository<TicketCheckin>
    {
    }
}

