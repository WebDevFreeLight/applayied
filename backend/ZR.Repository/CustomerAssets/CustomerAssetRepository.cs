using Infrastructure.Attribute;
using ZR.Model.CustomerAssets;

namespace ZR.Repository.CustomerAssets
{
    [AppService(ServiceType = typeof(CustomerAssetRepository), ServiceLifetime = LifeTime.Transient)]
    public class CustomerAssetRepository : BaseRepository<CustomerAsset>
    {
    }
}
