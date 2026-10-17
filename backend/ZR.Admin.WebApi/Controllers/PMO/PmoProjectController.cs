using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Project portfolio — create/view/edit projects, search &amp; filter, project detail.
    /// </summary>
    [Route("pmo/project")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoProjectController : PmoBaseController
    {
        private readonly IPmoProjectService _projectService;

        public PmoProjectController(IPmoProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>List projects (scoped to the caller), with search/filter/paging.</summary>
        [HttpGet("list")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult List([FromQuery] PmoProjectQueryDto parm)
        {
            var page = _projectService.GetList(parm, CurrentActor);
            return SUCCESS(page);
        }

        /// <summary>Project detail + derived delivery status.</summary>
        [HttpGet("{projectId}")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult Get(long projectId)
        {
            return SUCCESS(_projectService.GetInfo(projectId, CurrentActor));
        }

        /// <summary>Create a project (Project Manager or Administrator).</summary>
        [HttpPost]
        [PmoAuthorize(PmoRoleKeys.ProjectManager)]
        [Log(Title = "PMO Project", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] PmoProjectInputDto dto)
        {
            return SUCCESS(_projectService.AddProject(dto, CurrentActor));
        }

        /// <summary>Update a project (owner or Administrator).</summary>
        [HttpPut]
        [PmoAuthorize(PmoRoleKeys.ProjectManager)]
        [Log(Title = "PMO Project", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] PmoProjectInputDto dto)
        {
            return ToResponse(_projectService.UpdateProject(dto, CurrentActor));
        }
    }
}
