using Infrastructure.Attribute;
using ZR.Model.Boulevard;

namespace ZR.Repository
{
    /// <summary>
    /// boulevard_season_zones仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BlvBoulevardSeasonZonesRepository : BaseRepository<BlvBoulevardSeasonZones>
    {
    }
}
