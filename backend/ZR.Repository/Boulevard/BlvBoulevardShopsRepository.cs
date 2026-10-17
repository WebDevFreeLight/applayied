using Infrastructure.Attribute;
using ZR.Model.Boulevard;

namespace ZR.Repository
{
    /// <summary>
    /// shops仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BlvBoulevardShopsRepository : BaseRepository<BlvBoulevardShops>
    {
    }
}
