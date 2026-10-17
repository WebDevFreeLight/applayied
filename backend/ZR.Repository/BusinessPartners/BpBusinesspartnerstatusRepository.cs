using Infrastructure.Attribute;
using ZR.Model.BusinessPartners;

namespace ZR.Repository
{
    /// <summary>
    /// BusinessPartnerStatus仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpBusinesspartnerstatusRepository : BaseRepository<BpBusinesspartnerstatus>
    {
    }
}
