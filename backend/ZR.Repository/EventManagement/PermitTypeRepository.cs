using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Permit Type - أنواع التصاريح仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PermitTypeRepository : BaseRepository<PermitType>
    {
    }
}
