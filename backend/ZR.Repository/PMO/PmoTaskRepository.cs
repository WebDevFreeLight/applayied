using Infrastructure.Attribute;
using ZR.Model.PMO;

namespace ZR.Repository
{
    /// <summary>PMO Task repository.</summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoTaskRepository : BaseRepository<PmoTask>
    {
    }
}
