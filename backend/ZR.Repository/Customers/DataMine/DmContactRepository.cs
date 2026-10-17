using Infrastructure.Attribute;
using ZR.Model.Customers.DataMine;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class DmContactRepository : BaseRepository<DmContact>
    {
    }
}
