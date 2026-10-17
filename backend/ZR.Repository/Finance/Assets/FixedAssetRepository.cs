using Infrastructure.Attribute;
using ZR.Model.Finance.Assets;

namespace ZR.Repository.Finance.Assets
{
    /// <summary>
    /// Fixed Asset Repository - مستودع الأصول الثابتة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class FixedAssetRepository : BaseRepository<FixedAsset>
    {
    }
}

