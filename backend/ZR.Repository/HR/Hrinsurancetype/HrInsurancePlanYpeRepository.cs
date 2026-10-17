using Infrastructure.Attribute;
using ZR.Model.HR.Hrinsurancetype;

namespace ZR.Repository
{
    /// <summary>
    /// hrinsuranceplanype仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrInsurancePlanYpeRepository : BaseRepository<HrInsurancePlanYpe>
    {
    }
}
