using Microsoft.AspNetCore.Mvc;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Safety Precautions - احتياطات السلامة
    /// </summary>
    [Route("operations/safety-precautions")]
    [ApiExplorerSettings(GroupName = "operations")]
    public class OpsSafetyPrecautionController : BaseController
    {
        private readonly IOpsSafetyPrecautionService _service;

        public OpsSafetyPrecautionController(IOpsSafetyPrecautionService service)
        {
            _service = service;
        }

        /// <summary>
        /// List safety precautions
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "operations:safetyprecaution:list")]
        public IActionResult QueryList([FromQuery] OpsSafetyPrecautionQueryDto parm)
        {
            var response = _service.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get all active safety precautions (for checklists)
        /// </summary>
        [HttpGet("all-active")]
        [ActionPermissionFilter(Permission = "operations:safetyprecaution:list")]
        public IActionResult GetAllActive()
        {
            var response = _service.GetAllActive();
            return SUCCESS(response);
        }

        /// <summary>
        /// Get safety precaution by ID
        /// </summary>
        [HttpGet("{PrecautionId}")]
        [ActionPermissionFilter(Permission = "operations:safetyprecaution:query")]
        public IActionResult GetInfo(long PrecautionId)
        {
            var response = _service.GetInfo(PrecautionId);
            var info = response.Adapt<OpsSafetyPrecautionDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add safety precaution
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "operations:safetyprecaution:add")]
        [Log(Title = "Safety Precaution", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] OpsSafetyPrecautionDto parm)
        {
            var modal = parm.Adapt<OpsSafetyPrecaution>().ToCreate(HttpContext);
            var response = _service.AddPrecaution(modal);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update safety precaution
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "operations:safetyprecaution:edit")]
        [Log(Title = "Safety Precaution", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] OpsSafetyPrecautionDto parm)
        {
            var modal = parm.Adapt<OpsSafetyPrecaution>().ToUpdate(HttpContext);
            var response = _service.UpdatePrecaution(modal);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete safety precautions
        /// </summary>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "operations:safetyprecaution:delete")]
        [Log(Title = "Safety Precaution", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);
            return ToResponse(_service.Delete(idArr, "Delete safety precautions"));
        }

        /// <summary>
        /// Export safety precautions
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "operations:safetyprecaution:export")]
        [Log(Title = "Safety Precaution", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        public IActionResult Export([FromQuery] OpsSafetyPrecautionQueryDto parm)
        {
            var list = _service.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "SafetyPrecautions", "SafetyPrecautions");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
