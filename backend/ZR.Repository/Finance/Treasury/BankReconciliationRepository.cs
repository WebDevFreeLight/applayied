using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Bank Reconciliation Repository - مستودع التسوية البنكية
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BankReconciliationRepository : BaseRepository<BankReconciliation>
    {
    }
}


