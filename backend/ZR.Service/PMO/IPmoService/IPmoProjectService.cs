using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoProjectService
    {
        /// <summary>Scope-aware, filtered, paged project list enriched with owner/dept/progress.</summary>
        PagedInfo<PmoProject> GetList(PmoProjectQueryDto parm, PmoActor actor);

        /// <summary>Project detail with derived delivery status; enforces read scope.</summary>
        PmoProject GetInfo(long projectId, PmoActor actor);

        /// <summary>Create a project (PM/Admin). Validates code uniqueness and date/budget rules.</summary>
        PmoProject AddProject(PmoProjectInputDto dto, PmoActor actor);

        /// <summary>Update a project; only the owner or an admin may edit.</summary>
        int UpdateProject(PmoProjectInputDto dto, PmoActor actor);
    }
}
