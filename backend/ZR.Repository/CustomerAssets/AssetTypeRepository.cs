using Infrastructure.Attribute;
using ZR.Model.CustomerAssets;

namespace ZR.Repository.CustomerAssets
{
    [AppService(ServiceType = typeof(AssetTypeRepository), ServiceLifetime = LifeTime.Transient)]
    public class AssetTypeRepository : BaseRepository<AssetType>
    {
    }
}
