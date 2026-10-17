using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository.EventManagement
{
    /// <summary>
    /// Event Ticket - التذاكر
    /// Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventTicketRepository : BaseRepository<EventTicket>
    {
    }
}

