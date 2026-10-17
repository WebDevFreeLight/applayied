using Infrastructure.Attribute;
using ZR.Model.Boulevard;

namespace ZR.Repository
{
    /// <summary>
    /// boulevard_seasons_Shops仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BlvBoulevardSeasonsShopsRepository : BaseRepository<BlvBoulevardSeasonsShops>
    {
    }
}
