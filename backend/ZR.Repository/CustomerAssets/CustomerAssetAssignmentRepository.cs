using Infrastructure.Attribute;
using ZR.Model.CustomerAssets;

namespace ZR.Repository.CustomerAssets
{
    [AppService(ServiceType = typeof(CustomerAssetAssignmentRepository), ServiceLifetime = LifeTime.Transient)]
    public class CustomerAssetAssignmentRepository : BaseRepository<CustomerAssetAssignment>
    {
    }
}
