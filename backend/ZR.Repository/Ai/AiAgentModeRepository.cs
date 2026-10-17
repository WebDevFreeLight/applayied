using Infrastructure.Attribute;
using ZR.Model.Ai;

namespace ZR.Repository
{
    /// <summary>
    /// Agent Mode Table仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class AiAgentModeRepository : BaseRepository<AiAgentMode>
    {
    }
}
