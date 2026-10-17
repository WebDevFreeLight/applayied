using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoMilestoneService
    {
        /// <summary>Filtered, paged milestone list enriched with project name.</summary>
        PagedInfo<PmoMilestone> GetList(PmoMilestoneQueryDto parm, PmoActor actor);

        /// <summary>Milestone detail.</summary>
        PmoMilestone GetInfo(long milestoneId, PmoActor actor);

        /// <summary>Create a milestone (PM/Admin). Validates name and status.</summary>
        PmoMilestone AddMilestone(PmoMilestoneInputDto dto, PmoActor actor);

        /// <summary>Update a milestone (PM/Admin).</summary>
        int UpdateMilestone(PmoMilestoneInputDto dto, PmoActor actor);
    }
}
