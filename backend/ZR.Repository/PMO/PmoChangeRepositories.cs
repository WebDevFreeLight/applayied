using Infrastructure.Attribute;
using ZR.Model.PMO;

namespace ZR.Repository
{
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoChangeRequestRepository : BaseRepository<PmoChangeRequest> { }

    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class PmoChangeHistoryRepository : BaseRepository<PmoChangeHistory> { }
}
