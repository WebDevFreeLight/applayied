using Infrastructure.Attribute;
using ZR.Model.HR.Performance;

namespace ZR.Repository
{
    /// <summary>
    /// Performance Review Table仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrPerformanceReviewRepository : BaseRepository<HrPerformanceReview>
    {
    }
}
