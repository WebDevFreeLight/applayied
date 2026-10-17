using ZR.Model.Finance.Treasury;
using Infrastructure.Attribute;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Treasury Audit Log Repository - مستودع سجل تدقيق الخزينة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class TreasuryAuditLogRepository : BaseRepository<TreasuryAuditLog>
    {
    }
}
