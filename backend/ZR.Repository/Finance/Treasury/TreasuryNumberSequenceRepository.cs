using ZR.Model.Finance.Treasury;
using Infrastructure.Attribute;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Treasury Number Sequence Repository - مستودع تسلسل أرقام الخزينة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class TreasuryNumberSequenceRepository : BaseRepository<TreasuryNumberSequence>
    {
    }
}
