using Infrastructure.Attribute;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrPersonNameRepository : BaseRepository<HrPersonName>
    {
    }
}














