using Infrastructure.Attribute;
using ZR.Model.HR.Payroll;

namespace ZR.Repository
{
    /// <summary>
    /// Salary Compliance - امتثال الرواتب仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrSalaryComplianceRepository : BaseRepository<HrSalaryCompliance>
    {
    }
}
