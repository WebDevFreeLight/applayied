using Infrastructure.Attribute;
using ZR.Model.Customers.Leads;

namespace ZR.Repository.Customers
{
    /// <summary>
    /// Lead Activity Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpLeadActivityRepository : BaseRepository<BpLeadActivity>
    {
    }
}
