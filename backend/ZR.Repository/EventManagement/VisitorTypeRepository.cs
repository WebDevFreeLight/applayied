using Infrastructure.Attribute;
using ZR.Model.EventManagement;

namespace ZR.Repository
{
    /// <summary>
    /// Visitor Type - أنواع الزوار仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class VisitorTypeRepository : BaseRepository<VisitorType>
    {
    }
}
