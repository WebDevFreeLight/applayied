using Infrastructure.Attribute;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Tax Configuration Repository - مستودع إعدادات الضرائب
    /// Repository for managing Tax Configuration settings
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class TaxConfigurationRepository : BaseRepository<TaxConfiguration>
    {
    }
}
