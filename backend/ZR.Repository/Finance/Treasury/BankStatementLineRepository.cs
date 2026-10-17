using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Bank Statement Line Repository - مستودع بند كشف الحساب البنكي
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BankStatementLineRepository : BaseRepository<BankStatementLine>
    {
    }
}


