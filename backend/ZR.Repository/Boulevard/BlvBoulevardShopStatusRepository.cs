using Infrastructure.Attribute;
using ZR.Model.Boulevard;

namespace ZR.Repository
{
    /// <summary>
    /// shop_status仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BlvBoulevardShopStatusRepository : BaseRepository<BlvBoulevardShopStatus>
    {
    }
}
