using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Project milestones — list/view (contributor), PM create/edit. Dashboard uses due-soon/missed.
    /// </summary>
    [Route("pmo/milestone")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoMilestoneController : PmoBaseController
    {
        private readonly IPmoMilestoneService _milestoneService;

        public PmoMilestoneController(IPmoMilestoneService milestoneService)
        {
            _milestoneService = milestoneService;
        }

        /// <summary>List milestones with filter/paging.</summary>
        [HttpGet("list")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult List([FromQuery] PmoMilestoneQueryDto parm)
        {
            var page = _milestoneService.GetList(parm, CurrentActor);
            return SUCCESS(page);
        }

        /// <summary>Milestone detail.</summary>
        [HttpGet("{milestoneId}")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult Get(long milestoneId)
        {
            return SUCCESS(_milestoneService.GetInfo(milestoneId, CurrentActor));
        }

        /// <summary>Create a milestone (Project Manager or Administrator).</summary>
        [HttpPost]
        [PmoAuthorize(PmoRoleKeys.ProjectManager)]
        [Log(Title = "PMO Milestone", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] PmoMilestoneInputDto dto)
        {
            return SUCCESS(_milestoneService.AddMilestone(dto, CurrentActor));
        }

        /// <summary>Update a milestone (Project Manager or Administrator).</summary>
        [HttpPut]
        [PmoAuthorize(PmoRoleKeys.ProjectManager)]
        [Log(Title = "PMO Milestone", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] PmoMilestoneInputDto dto)
        {
            return ToResponse(_milestoneService.UpdateMilestone(dto, CurrentActor));
        }
    }
}
