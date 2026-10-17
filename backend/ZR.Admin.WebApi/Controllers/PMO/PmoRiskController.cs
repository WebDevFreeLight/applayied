using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model.PMO;
using ZR.Model.PMO.Dto;
using ZR.Service.PMO.IPmoService;

namespace ZR.Admin.WebApi.Controllers.PMO
{
    /// <summary>
    /// Risks &amp; Issues register — create/view/edit risks, search &amp; filter, high-exposure focus.
    /// </summary>
    [Route("pmo/risk")]
    [ApiExplorerSettings(GroupName = "pmo")]
    public class PmoRiskController : PmoBaseController
    {
        private readonly IPmoRiskService _riskService;

        public PmoRiskController(IPmoRiskService riskService)
        {
            _riskService = riskService;
        }

        /// <summary>List risks/issues (scoped to the caller), with search/filter/paging; ordered by exposure.</summary>
        [HttpGet("list")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult List([FromQuery] PmoRiskQueryDto parm)
        {
            var page = _riskService.GetList(parm, CurrentActor);
            return SUCCESS(page);
        }

        /// <summary>Risk/issue detail.</summary>
        [HttpGet("{riskId}")]
        [PmoAuthorize(PmoRoleKeys.Contributor)]
        public IActionResult Get(long riskId)
        {
            return SUCCESS(_riskService.GetInfo(riskId, CurrentActor));
        }

        /// <summary>Create a risk/issue (Project Manager or Administrator).</summary>
        [HttpPost]
        [PmoAuthorize(PmoRoleKeys.ProjectManager)]
        [Log(Title = "PMO Risk", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] PmoRiskInputDto dto)
        {
            return SUCCESS(_riskService.AddRisk(dto, CurrentActor));
        }

        /// <summary>Update a risk/issue (Project Manager or Administrator).</summary>
        [HttpPut]
        [PmoAuthorize(PmoRoleKeys.ProjectManager)]
        [Log(Title = "PMO Risk", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] PmoRiskInputDto dto)
        {
            return ToResponse(_riskService.UpdateRisk(dto, CurrentActor));
        }
    }
}
