using Microsoft.AspNetCore.Mvc;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Work Types - أنواع العمل
    /// </summary>
    [Route("operations/work-types")]
    [ApiExplorerSettings(GroupName = "operations")]
    public class OpsWorkTypeController : BaseController
    {
        private readonly IOpsWorkTypeService _service;

        public OpsWorkTypeController(IOpsWorkTypeService service)
        {
            _service = service;
        }

        /// <summary>
        /// List work types
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "operations:worktype:list")]
        public IActionResult QueryList([FromQuery] OpsWorkTypeQueryDto parm)
        {
            var response = _service.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get all active work types (for dropdowns)
        /// </summary>
        [HttpGet("all-active")]
        [ActionPermissionFilter(Permission = "operations:worktype:list")]
        public IActionResult GetAllActive()
        {
            var response = _service.GetAllActive();
            return SUCCESS(response);
        }

        /// <summary>
        /// Get work type by ID
        /// </summary>
        [HttpGet("{WorkTypeId}")]
        [ActionPermissionFilter(Permission = "operations:worktype:query")]
        public IActionResult GetInfo(long WorkTypeId)
        {
            var response = _service.GetInfo(WorkTypeId);
            var info = response.Adapt<OpsWorkTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add work type
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "operations:worktype:add")]
        [Log(Title = "Work Type", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] OpsWorkTypeDto parm)
        {
            var modal = parm.Adapt<OpsWorkType>().ToCreate(HttpContext);
            var response = _service.AddWorkType(modal);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update work type
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "operations:worktype:edit")]
        [Log(Title = "Work Type", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] OpsWorkTypeDto parm)
        {
            var modal = parm.Adapt<OpsWorkType>().ToUpdate(HttpContext);
            var response = _service.UpdateWorkType(modal);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete work types
        /// </summary>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "operations:worktype:delete")]
        [Log(Title = "Work Type", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);
            return ToResponse(_service.Delete(idArr, "Delete work types"));
        }

        /// <summary>
        /// Export work types
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "operations:worktype:export")]
        [Log(Title = "Work Type", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        public IActionResult Export([FromQuery] OpsWorkTypeQueryDto parm)
        {
            var list = _service.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "WorkTypes", "WorkTypes");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
