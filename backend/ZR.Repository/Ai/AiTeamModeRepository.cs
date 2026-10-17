using Infrastructure.Attribute;
using ZR.Model.AI;

namespace ZR.Repository
{
    /// <summary>
    /// Team Mode Table仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class AiTeamModeRepository : BaseRepository<AiTeamMode>
    {
    }
}
