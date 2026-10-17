using Infrastructure.Attribute;
using ZR.Model.Boulevard;

namespace ZR.Repository
{
    /// <summary>
    /// CustomerRegistrationForm仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BlvCustomerregistrationformRepository : BaseRepository<BlvCustomerregistrationform>
    {
    }
}
