using Microsoft.AspNetCore.Mvc;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Permit Types - أنواع التصاريح
    /// </summary>
    [Route("operations/permit-types")]
    [ApiExplorerSettings(GroupName = "operations")]
    public class OpsPermitTypeController : BaseController
    {
        private readonly IOpsPermitTypeService _service;

        public OpsPermitTypeController(IOpsPermitTypeService service)
        {
            _service = service;
        }

        /// <summary>
        /// List permit types
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "operations:permittype:list")]
        public IActionResult QueryList([FromQuery] OpsPermitTypeQueryDto parm)
        {
            var response = _service.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get permit type by ID
        /// </summary>
        [HttpGet("{PermitTypeId}")]
        [ActionPermissionFilter(Permission = "operations:permittype:query")]
        public IActionResult GetInfo(long PermitTypeId)
        {
            var response = _service.GetInfo(PermitTypeId);
            var info = response.Adapt<OpsPermitTypeDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add permit type
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "operations:permittype:add")]
        [Log(Title = "Permit Type", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] OpsPermitTypeDto parm)
        {
            var modal = parm.Adapt<OpsPermitType>().ToCreate(HttpContext);
            var response = _service.AddOpsPermitType(modal);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update permit type
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "operations:permittype:edit")]
        [Log(Title = "Permit Type", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] OpsPermitTypeDto parm)
        {
            var modal = parm.Adapt<OpsPermitType>().ToUpdate(HttpContext);
            var response = _service.UpdateOpsPermitType(modal);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete permit types
        /// </summary>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "operations:permittype:delete")]
        [Log(Title = "Permit Type", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);
            return ToResponse(_service.Delete(idArr, "Delete permit types"));
        }

        /// <summary>
        /// Export permit types
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "operations:permittype:export")]
        [Log(Title = "Permit Type", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        public IActionResult Export([FromQuery] OpsPermitTypeQueryDto parm)
        {
            var list = _service.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "PermitTypes", "PermitTypes");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
