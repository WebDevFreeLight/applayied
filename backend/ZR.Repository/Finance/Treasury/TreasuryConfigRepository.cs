using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Treasury Config Repository - مستودع إعدادات الخزينة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class TreasuryConfigRepository : BaseRepository<TreasuryConfig>
    {
    }
}


