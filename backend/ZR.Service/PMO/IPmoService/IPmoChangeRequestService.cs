using System.Collections.Generic;
using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoChangeRequestService
    {
        PagedInfo<PmoChangeRequest> GetList(PmoChangeRequestQueryDto parm, PmoActor actor);
        PmoChangeRequest GetInfo(long changeRequestId, PmoActor actor);
        List<PmoChangeHistory> GetHistory(long changeRequestId);

        PmoChangeRequest Create(PmoChangeRequestInputDto dto, PmoActor actor);
        int Update(PmoChangeRequestInputDto dto, PmoActor actor);
        PmoChangeRequest Submit(long changeRequestId, PmoActor actor);
        PmoChangeRequest Decide(PmoChangeRequestDecisionDto dto, PmoActor actor);
    }
}
