using Infrastructure.Attribute;
using ZR.Model.BusinessPartners;

namespace ZR.Repository
{
    /// <summary>
    /// BusinessPartnerDocType仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpBusinesspartnerdoctypeRepository : BaseRepository<BpBusinesspartnerdoctype>
    {
    }
}
