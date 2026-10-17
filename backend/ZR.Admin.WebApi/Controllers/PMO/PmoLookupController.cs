using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>Reference-data lookups for dropdowns and dashboard filters.</summary>
    [Route("pmo/lookup")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoLookupController : PmoBaseController
    {
        private readonly IPmoLookupService _service;
        public PmoLookupController(IPmoLookupService service) => _service = service;

        [HttpGet("departments")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult Departments() => SUCCESS(_service.Departments());

        [HttpGet("users")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult Users() => SUCCESS(_service.Users());

        [HttpGet("managers")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult Managers() => SUCCESS(_service.Managers());

        [HttpGet("projects")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult Projects() => SUCCESS(_service.Projects());
    }
}
