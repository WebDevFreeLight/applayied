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
        [ActionPermissionFilter(Permission = "pmo:lookup:list")]
        public IActionResult Departments() => SUCCESS(_service.Departments());

        [HttpGet("users")]
        [ActionPermissionFilter(Permission = "pmo:lookup:list")]
        public IActionResult Users() => SUCCESS(_service.Users());

        [HttpGet("managers")]
        [ActionPermissionFilter(Permission = "pmo:lookup:list")]
        public IActionResult Managers() => SUCCESS(_service.Managers());

        [HttpGet("projects")]
        [ActionPermissionFilter(Permission = "pmo:lookup:list")]
        public IActionResult Projects() => SUCCESS(_service.Projects());
    }
}
