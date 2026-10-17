using Infrastructure.Attribute;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Repository
{
    /// <summary>
    /// Employee Document Repository
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeDocumentRepository : BaseRepository<HrEmployeeDocument>
    {
    }
}
