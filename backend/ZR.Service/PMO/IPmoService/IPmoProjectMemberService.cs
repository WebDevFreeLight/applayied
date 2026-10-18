using ZR.Model;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;

namespace ZR.Service.PMO.IPmoService
{
    public interface IPmoProjectMemberService
    {
        /// <summary>Members of a project, enriched with the user's display name.</summary>
        PagedInfo<PmoProjectMember> GetList(PmoMemberQueryDto parm, PmoActor actor);

        /// <summary>Add a member to a project (unique per project+user).</summary>
        PmoProjectMember AddMember(PmoMemberInputDto dto, PmoActor actor);

        /// <summary>Remove a member from a project.</summary>
        int RemoveMember(long memberId, PmoActor actor);
    }
}
