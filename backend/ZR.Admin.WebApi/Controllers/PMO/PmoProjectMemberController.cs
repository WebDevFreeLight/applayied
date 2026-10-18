using Microsoft.AspNetCore.Mvc;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;
using ZR.ServiceCore.Middleware;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>Project membership — who works on a project (drives Contributor scoped access).</summary>
    [Route("pmo/project-member")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoProjectMemberController : PmoBaseController
    {
        private readonly IPmoProjectMemberService _service;
        public PmoProjectMemberController(IPmoProjectMemberService service) => _service = service;

        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "pmo:member:list")]
        public IActionResult List([FromQuery] PmoMemberQueryDto parm) => SUCCESS(_service.GetList(parm, CurrentActor));

        [HttpPost]
        [ActionPermissionFilter(Permission = "pmo:member:add")]
        public IActionResult Add([FromBody] PmoMemberInputDto dto) => SUCCESS(_service.AddMember(dto, CurrentActor));

        [HttpDelete("{memberId}")]
        [ActionPermissionFilter(Permission = "pmo:member:remove")]
        public IActionResult Remove(long memberId) => ToResponse(_service.RemoveMember(memberId, CurrentActor));
    }
}
