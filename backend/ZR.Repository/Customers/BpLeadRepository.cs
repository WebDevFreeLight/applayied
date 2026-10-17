using Infrastructure.Attribute;
using ZR.Model.Customers.Leads;

namespace ZR.Repository.Customers
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpLeadRepository : BaseRepository<BpLead>
    {
    }
}
