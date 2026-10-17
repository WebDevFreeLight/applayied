using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Bank Statement Repository - مستودع كشف الحساب البنكي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BankStatementRepository : BaseRepository<BankStatement>
    {
    }
}


