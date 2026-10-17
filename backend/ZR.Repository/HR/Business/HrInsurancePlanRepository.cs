using Infrastructure.Attribute;
using ZR.Model.HR.Business;

namespace ZR.Repository
{
    /// <summary>
    /// Insurance Plans Table仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrInsurancePlanRepository : BaseRepository<HrInsurancePlan>
    {
    }
}
