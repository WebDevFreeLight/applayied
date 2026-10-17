using Infrastructure.Attribute;
using ZR.Model.Finance.Treasury;

namespace ZR.Repository.Finance.Treasury
{
    /// <summary>
    /// Cheque Repository - مستودع الشيك
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class ChequeRepository : BaseRepository<Cheque>
    {
    }
}


