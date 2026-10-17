using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Bank Account Repository - مستودع الحساب البنكي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BankAccountRepository : BaseRepository<BankAccount>
    {
    }
}


