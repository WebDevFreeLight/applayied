using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Bank Parsing Profile Repository - مستودع ملف تعريف تحليل البنك
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BankParsingProfileRepository : BaseRepository<BankParsingProfile>
    {
    }
}
