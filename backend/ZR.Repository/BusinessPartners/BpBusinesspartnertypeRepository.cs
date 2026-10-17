using Infrastructure.Attribute;
using ZR.Model.BusinessPartners;

namespace ZR.Repository
{
    /// <summary>
    /// BusinessPartnerType仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpBusinesspartnertypeRepository : BaseRepository<BpBusinesspartnertype>
    {
    }
}
