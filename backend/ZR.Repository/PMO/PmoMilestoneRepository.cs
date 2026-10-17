using Infrastructure.Attribute;
using ZR.Model.PMO;

namespace ZR.Repository
{
    /// <summary>PMO Milestone repository.</summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoMilestoneRepository : BaseRepository<PmoMilestone>
    {
    }
}
