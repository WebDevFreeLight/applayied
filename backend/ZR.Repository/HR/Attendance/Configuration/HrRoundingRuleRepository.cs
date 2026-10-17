using Infrastructure.Attribute;
using ZR.Model.HR.Attendance.Configuration;

namespace ZR.Repository
{
    /// <summary>
    /// Repository for HR Rounding Rules
    /// Manages time rounding rules for attendance calculation
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrRoundingRuleRepository : BaseRepository<HrRoundingRule>
    {
    }
}
