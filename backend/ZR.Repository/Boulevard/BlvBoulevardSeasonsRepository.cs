using Infrastructure.Attribute;
using ZR.Model.Boulevard;

namespace ZR.Repository
{
    /// <summary>
    /// boulevard_seasons仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BlvBoulevardSeasonsRepository : BaseRepository<BlvBoulevardSeasons>
    {
    }
}
