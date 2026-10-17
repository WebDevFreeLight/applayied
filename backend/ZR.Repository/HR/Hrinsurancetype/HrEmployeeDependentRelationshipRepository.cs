using Infrastructure.Attribute;
using ZR.Model.HR.Hrinsurancetype;

namespace ZR.Repository
{
    /// <summary>
    /// employeeependentrelationship仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeDependentRelationshipRepository : BaseRepository<HrEmployeeDependentRelationship>
    {
    }
}
