using Infrastructure.Attribute;
using ZR.Model.PMO;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoCostCategoryRepository : BaseRepository<PmoCostCategory> { }

    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoBudgetRevisionRepository : BaseRepository<PmoBudgetRevision> { }

    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoCostTransactionRepository : BaseRepository<PmoCostTransaction> { }

    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoFinancialForecastRepository : BaseRepository<PmoFinancialForecast> { }

    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoAuditEventRepository : BaseRepository<PmoAuditEvent> { }
}
