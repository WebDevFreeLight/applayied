using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Cash Account Repository - مستودع الحساب النقدي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class CashAccountRepository : BaseRepository<CashAccount>
    {
    }
}


