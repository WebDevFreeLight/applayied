using Infrastructure.Attribute;
using ZR.Model.BusinessPartners;

namespace ZR.Repository
{
    /// <summary>
    /// BusinessPartnerkind仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpBusinesspartnerkindRepository : BaseRepository<BpBusinesspartnerkind>
    {
    }
}
