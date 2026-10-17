using Infrastructure.Attribute;
using ZR.Model.Hr;

namespace ZR.Repository
{
    /// <summary>
    /// insuranceenrollment仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrInsuranceEnrollmentRepository : BaseRepository<HrInsuranceEnrollment>
    {
    }
}