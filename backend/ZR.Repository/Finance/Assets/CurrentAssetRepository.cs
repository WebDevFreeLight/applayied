using Infrastructure.Attribute;
using ZR.Model.Finance.Assets;

namespace ZR.Repository.Finance.Assets
{
    /// <summary>
    /// Current Asset Repository - مستودع الأصول المتداولة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class CurrentAssetRepository : BaseRepository<CurrentAsset>
    {
    }
}

