using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Change requests &amp; approvals — submit, review and approve/reject/return with durable history.
    /// </summary>
    [Route("pmo/change-request")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoChangeRequestController : PmoBaseController
    {
        private readonly IPmoChangeRequestService _service;
        public PmoChangeRequestController(IPmoChangeRequestService service) => _service = service;

        [HttpGet("list")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult List([FromQuery] PmoChangeRequestQueryDto parm) => SUCCESS(_service.GetList(parm, CurrentActor));

        [HttpGet("{changeRequestId}")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult Get(long changeRequestId) => SUCCESS(_service.GetInfo(changeRequestId, CurrentActor));

        [HttpGet("{changeRequestId}/history")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult History(long changeRequestId) => SUCCESS(_service.GetHistory(changeRequestId));

        /// <summary>Create a draft change request (Contributor and above).</summary>
        [HttpPost]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        [Log(Title = "PMO Change Request", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] PmoChangeRequestInputDto dto) => SUCCESS(_service.Create(dto, CurrentActor));

        [HttpPut]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        [Log(Title = "PMO Change Request", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] PmoChangeRequestInputDto dto) => ToResponse(_service.Update(dto, CurrentActor));

        /// <summary>Submit a draft/returned request for review (requester only).</summary>
        [HttpPost("{changeRequestId}/submit")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        [Log(Title = "PMO Change Request Submit", BusinessType = BusinessType.UPDATE)]
        public IActionResult Submit(long changeRequestId) => SUCCESS(_service.Submit(changeRequestId, CurrentActor));

        /// <summary>Decide a request (Project Manager / Admin). The service blocks self-approval.</summary>
        [HttpPost("decision")]
        [PmoAuthorize(PmoRoleKeys.ProjectManager)]
        [Log(Title = "PMO Change Request Decision", BusinessType = BusinessType.UPDATE)]
        public IActionResult Decide([FromBody] PmoChangeRequestDecisionDto dto) => SUCCESS(_service.Decide(dto, CurrentActor));
    }
}
