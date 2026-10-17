using Infrastructure.Attribute;
using ZR.Model.HR.Hrinsurancetype;

namespace ZR.Repository
{
    /// <summary>
    /// insurancetype仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrInsuranceCoveragelevelRepository : BaseRepository<HrInsuranceCoveragelevel>
    {
    }
}
