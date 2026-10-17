using Infrastructure.Attribute;
using ZR.Model.BusinessPartners;

namespace ZR.Repository
{
    /// <summary>
    /// Bp_PartnerDocsments仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpPartnerdocsmentsRepository : BaseRepository<BpPartnerdocsments>
    {
    }
}
