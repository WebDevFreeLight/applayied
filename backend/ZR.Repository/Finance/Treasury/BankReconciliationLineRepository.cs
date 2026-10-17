using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Bank Reconciliation Line Repository - مستودع بند التسوية البنكية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BankReconciliationLineRepository : BaseRepository<BankReconciliationLine>
    {
    }
}


