using Infrastructure.Attribute;
using ZR.Model.PMO;

namespace ZR.Repository
{
    /// <summary>PMO Project repository.</summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoProjectRepository : BaseRepository<PmoProject>
    {
    }
}
