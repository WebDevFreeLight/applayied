using Infrastructure.Attribute;
using ZR.Model.BusinessPartners;

namespace ZR.Repository
{
    /// <summary>
    /// ContactMethod仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpContactmethodRepository : BaseRepository<BpContactmethod>
    {
    }
}
