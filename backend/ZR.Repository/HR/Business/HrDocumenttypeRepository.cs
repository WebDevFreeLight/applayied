using Infrastructure.Attribute;
using ZR.Model.HR.Business;

namespace ZR.Repository
{
    /// <summary>
    /// DocumentType仓储
    /// </summary>
    [AppService(ServiceLifetime = LifeTime.Transient)]
    public class HrDocumenttypeRepository : BaseRepository<HrDocumenttype>
    {
    }
}
