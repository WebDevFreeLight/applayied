using Infrastructure.Attribute;
using ZR.Model.Customers.Leads;

namespace ZR.Repository.Customers
{
    /// <summary>
    /// Lead Assignment Log Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BpLeadAssignmentLogRepository : BaseRepository<BpLeadAssignmentLog>
    {
    }
}
