using Infrastructure.Attribute;
using ZR.Model.CustomerAssets;

namespace ZR.Repository.CustomerAssets
{
    [AppService(ServiceType = typeof(CustomerContractFileRepository), ServiceLifetime = LifeTime.Transient)]
    public class CustomerContractFileRepository : BaseRepository<CustomerContractFile>
    {
    }
}
