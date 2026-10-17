using Infrastructure.Attribute;
using ZR.Model.Finance.Assets;

namespace ZR.Repository.Finance.Assets
{
    /// <summary>
    /// Asset Depreciation Repository - مستودع إهلاك الأصول
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class AssetDepreciationRepository : BaseRepository<AssetDepreciation>
    {
    }
}

