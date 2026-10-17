using Infrastructure.Attribute;
using ZR.Model.PMO;

namespace ZR.Repository
{
    /// <summary>PMO Risk/Issue repository.</summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoRiskRepository : BaseRepository<PmoRisk>
    {
    }
}
