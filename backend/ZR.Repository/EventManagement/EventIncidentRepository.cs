using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository.EventManagement
{
    /// <summary>
    /// Event Incident - الحوادث والبلاغات
    /// Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class EventIncidentRepository : BaseRepository<EventIncident>
    {
    }
}

