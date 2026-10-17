using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Booth Type - أنواع الأكشاك仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class BoothTypeRepository : BaseRepository<BoothType>
    {
    }
}
