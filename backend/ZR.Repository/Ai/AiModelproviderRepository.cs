using Infrastructure.Attribute;
using ZR.Model.Ai;

namespace ZR.Repository
{
    /// <summary>
    /// Model Provider Table仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class AiModelproviderRepository : BaseRepository<AiModelprovider>
    {
    }
}
