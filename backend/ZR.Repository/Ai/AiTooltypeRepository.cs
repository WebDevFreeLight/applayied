using Infrastructure.Attribute;
using ZR.Model.Ai;

namespace ZR.Repository
{
    /// <summary>
    /// Tool Type Table仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class AiTooltypeRepository : BaseRepository<AiTooltype>
    {
    }
}
