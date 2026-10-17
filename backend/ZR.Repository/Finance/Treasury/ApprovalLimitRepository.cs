using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Approval Limit Repository - مستودع حدود الموافقة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class ApprovalLimitRepository : BaseRepository<ApprovalLimit>
    {
    }
}
