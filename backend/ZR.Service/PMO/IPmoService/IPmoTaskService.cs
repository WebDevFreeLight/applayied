using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoTaskService
    {
        /// <summary>Scope-aware, filtered, paged task list enriched with assignee/project/overdue.</summary>
        PagedInfo<PmoTask> GetList(PmoTaskQueryDto parm, PmoActor actor);

        /// <summary>Task detail; enforces read scope for contributors.</summary>
        PmoTask GetInfo(long taskId, PmoActor actor);

        /// <summary>Create a task (PM/Admin). Validates title/weight/progress/dates.</summary>
        PmoTask AddTask(PmoTaskInputDto dto, PmoActor actor);

        /// <summary>Update a task (PM/Admin).</summary>
        int UpdateTask(PmoTaskInputDto dto, PmoActor actor);

        /// <summary>Contributor path: update progress/status on an assigned task (own-task rule enforced).</summary>
        int UpdateProgress(PmoTaskProgressDto dto, PmoActor actor);

        /// <summary>Weighted project progress = Σ(weight × progress) ÷ Σ(weight); 0 if no tasks.</summary>
        decimal GetProjectProgress(long projectId);
    }
}
