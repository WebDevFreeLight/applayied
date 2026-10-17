using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Incident Type - أنواع الحوادث仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class IncidentTypeRepository : BaseRepository<IncidentType>
    {
    }
}
