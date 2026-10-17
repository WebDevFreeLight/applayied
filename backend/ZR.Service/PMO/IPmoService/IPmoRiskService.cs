using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoRiskService
    {
        /// <summary>Scope-aware, filtered, paged risk/issue list ordered by exposure (score desc).</summary>
        PagedInfo<PmoRisk> GetList(PmoRiskQueryDto parm, PmoActor actor);

        /// <summary>Risk/issue detail; enforces read scope.</summary>
        PmoRisk GetInfo(long riskId, PmoActor actor);

        /// <summary>Create a risk/issue (PM/Admin). Score is computed server-side.</summary>
        PmoRisk AddRisk(PmoRiskInputDto dto, PmoActor actor);

        /// <summary>Update a risk/issue (PM/Admin). Score is recomputed server-side.</summary>
        int UpdateRisk(PmoRiskInputDto dto, PmoActor actor);
    }
}
