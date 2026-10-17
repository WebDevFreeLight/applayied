using Infrastructure.Attribute;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrEmployeeStatusRepository : BaseRepository<HrEmployeeStatus>
    {
    }
}














