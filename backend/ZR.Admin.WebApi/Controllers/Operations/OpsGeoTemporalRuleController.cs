using Microsoft.AspNetCore.Mvc;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Geo-Temporal Rules - قواعد الوصول الجغرافية/الزمنية
    /// </summary>
    [Route("operations/geo-temporal-rules")]
    [ApiExplorerSettings(GroupName = "operations")]
    public class OpsGeoTemporalRuleController : BaseController
    {
        private readonly IOpsGeoTemporalRuleService _service;

        public OpsGeoTemporalRuleController(IOpsGeoTemporalRuleService service)
        {
            _service = service;
        }

        /// <summary>
        /// List geo-temporal rules
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "operations:georule:list")]
        public IActionResult QueryList([FromQuery] OpsGeoTemporalRuleQueryDto parm)
        {
            var response = _service.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get rule by ID
        /// </summary>
        [HttpGet("{RuleId}")]
        [ActionPermissionFilter(Permission = "operations:georule:query")]
        public IActionResult GetInfo(long RuleId)
        {
            var response = _service.GetInfo(RuleId);
            var info = response.Adapt<OpsGeoTemporalRuleDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add rule
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "operations:georule:add")]
        [Log(Title = "Geo-Temporal Rule", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] OpsGeoTemporalRuleDto parm)
        {
            var modal = parm.Adapt<OpsGeoTemporalRule>().ToCreate(HttpContext);
            var response = _service.AddOpsGeoTemporalRule(modal);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update rule
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "operations:georule:edit")]
        [Log(Title = "Geo-Temporal Rule", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] OpsGeoTemporalRuleDto parm)
        {
            var modal = parm.Adapt<OpsGeoTemporalRule>().ToUpdate(HttpContext);
            var response = _service.UpdateOpsGeoTemporalRule(modal);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete rules
        /// </summary>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "operations:georule:delete")]
        [Log(Title = "Geo-Temporal Rule", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);
            return ToResponse(_service.Delete(idArr, "Delete geo-temporal rules"));
        }

        /// <summary>
        /// Export rules
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "operations:georule:export")]
        [Log(Title = "Geo-Temporal Rule", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        public IActionResult Export([FromQuery] OpsGeoTemporalRuleQueryDto parm)
        {
            var list = _service.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "GeoTemporalRules", "GeoTemporalRules");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
