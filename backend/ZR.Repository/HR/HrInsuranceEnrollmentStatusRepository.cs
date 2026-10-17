using Infrastructure.Attribute;
using ZR.Model.Hr;

namespace ZR.Repository
{
    /// <summary>
    /// hrinsuranceenrollmentstatus仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrInsuranceEnrollmentStatusRepository : BaseRepository<HrInsuranceEnrollmentStatus>
    {
    }
}
