using Infrastructure.Attribute;
using ZR.Model.Finance.Assets;

namespace ZR.Repository.Finance.Assets
{
    /// <summary>
    /// Asset Category Repository - مستودع فئات الأصول
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class AssetCategoryRepository : BaseRepository<AssetCategory>
    {
    }
}

