using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>Cost categories (administered; also used as the finance category lookup).</summary>
    [Route("pmo/cost-category")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoCostCategoryController : PmoBaseController
    {
        private readonly IPmoCostCategoryService _service;
        public PmoCostCategoryController(IPmoCostCategoryService service) => _service = service;

        /// <summary>All categories (any signed-in PMO user, for dropdowns/filters).</summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "pmo:category:list")]
        public IActionResult List() => SUCCESS(_service.GetAll());

        [HttpPost]
        [ActionPermissionFilter(Permission = "pmo:category:add")]
        [Log(Title = "PMO Cost Category", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] PmoCostCategoryInputDto dto) => SUCCESS(_service.Add(dto, CurrentActor));

        [HttpPut]
        [ActionPermissionFilter(Permission = "pmo:category:edit")]
        [Log(Title = "PMO Cost Category", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] PmoCostCategoryInputDto dto) => ToResponse(_service.Update(dto, CurrentActor));
    }
}
