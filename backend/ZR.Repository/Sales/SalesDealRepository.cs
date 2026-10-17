using Infrastructure.Attribute;
using ZR.Model.Sales;

namespace ZR.Repository.Sales
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class SalesDealRepository : BaseRepository<SalesDeal>
    {
    }
}
