using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Treasury GL Account Mapping Repository - مستودع تعيين حسابات دفتر الأستاذ للخزينة
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class TreasuryGLAccountMappingRepository : BaseRepository<TreasuryGLAccountMapping>
    {
    }
}
