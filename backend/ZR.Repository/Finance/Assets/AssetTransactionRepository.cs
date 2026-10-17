using Infrastructure.Attribute;
using ZR.Model.Finance.Assets;

namespace ZR.Repository.Finance.Assets
{
    /// <summary>
    /// Asset Transaction Repository - مستودع معاملات الأصول
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class AssetTransactionRepository : BaseRepository<AssetTransaction>
    {
    }
}

