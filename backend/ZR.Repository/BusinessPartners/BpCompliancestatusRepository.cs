using Infrastructure.Attribute;
using ZR.Model.BusinessPartners;

namespace ZR.Repository
{
    /// <summary>
    /// compliancestatus仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpCompliancestatusRepository : BaseRepository<BpCompliancestatus>
    {
    }
}
