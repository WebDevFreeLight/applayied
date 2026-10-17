using Infrastructure.Attribute;
using ZR.Model.BusinessPartners;

namespace ZR.Repository
{
    /// <summary>
    /// businesspartner仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpBusinesspartnerRepository : BaseRepository<BpBusinesspartner>
    {
    }
}
