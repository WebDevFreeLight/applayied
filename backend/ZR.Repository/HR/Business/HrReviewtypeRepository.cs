using Infrastructure.Attribute;
using ZR.Model.HR.Business;

namespace ZR.Repository
{
    /// <summary>
    /// Reviewtype Information Table仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrReviewtypeRepository : BaseRepository<HrReviewtype>
    {
    }
}
