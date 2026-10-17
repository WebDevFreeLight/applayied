using Infrastructure.Attribute;
using ZR.Model.Business;

namespace ZR.Repository
{
    /// <summary>
    /// Organizations Information Table仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class OrganizationsRepository : BaseRepository<Organizations>
    {
    }
}
