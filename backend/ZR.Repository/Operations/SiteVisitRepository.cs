using Infrastructure.Attribute;
using ZR.Model.Operations;

namespace ZR.Repository.Operations
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class SiteVisitRepository : BaseRepository<SiteVisit>
    {
    }
}
