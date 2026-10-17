using Microsoft.AspNetCore.Mvc;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Service.Operations;

namespace ZR.Admin.WebApi.Controllers.Operations
{
    /// <summary>
    /// Work Categories - تصنيفات العمل
    /// </summary>
    [Route("operations/work-categories")]
    [ApiExplorerSettings(GroupName = "operations")]
    public class OpsWorkCategoryController : BaseController
    {
        private readonly IOpsWorkCategoryService _service;

        public OpsWorkCategoryController(IOpsWorkCategoryService service)
        {
            _service = service;
        }

        /// <summary>
        /// List work categories
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "operations:workcategory:list")]
        public IActionResult QueryList([FromQuery] OpsWorkCategoryQueryDto parm)
        {
            var response = _service.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get all active work categories (for dropdowns)
        /// </summary>
        [HttpGet("all-active")]
        [ActionPermissionFilter(Permission = "operations:workcategory:list")]
        public IActionResult GetAllActive()
        {
            var response = _service.GetAllActive();
            return SUCCESS(response);
        }

        /// <summary>
        /// Get work category by ID
        /// </summary>
        [HttpGet("{WorkCategoryId}")]
        [ActionPermissionFilter(Permission = "operations:workcategory:query")]
        public IActionResult GetInfo(long WorkCategoryId)
        {
            var response = _service.GetInfo(WorkCategoryId);
            var info = response.Adapt<OpsWorkCategoryDto>();
            return SUCCESS(info);
        }

        /// <summary>
        /// Add work category
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "operations:workcategory:add")]
        [Log(Title = "Work Category", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] OpsWorkCategoryDto parm)
        {
            var modal = parm.Adapt<OpsWorkCategory>().ToCreate(HttpContext);
            var response = _service.AddWorkCategory(modal);
            return SUCCESS(response);
        }

        /// <summary>
        /// Update work category
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "operations:workcategory:edit")]
        [Log(Title = "Work Category", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] OpsWorkCategoryDto parm)
        {
            var modal = parm.Adapt<OpsWorkCategory>().ToUpdate(HttpContext);
            var response = _service.UpdateWorkCategory(modal);
            return ToResponse(response);
        }

        /// <summary>
        /// Delete work categories
        /// </summary>
        [HttpPost("delete/{ids}")]
        [ActionPermissionFilter(Permission = "operations:workcategory:delete")]
        [Log(Title = "Work Category", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete([FromRoute] string ids)
        {
            var idArr = Tools.SplitAndConvert<long>(ids);
            return ToResponse(_service.Delete(idArr, "Delete work categories"));
        }

        /// <summary>
        /// Export work categories
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "operations:workcategory:export")]
        [Log(Title = "Work Category", BusinessType = BusinessType.EXPORT, IsSaveResponseData = false)]
        public IActionResult Export([FromQuery] OpsWorkCategoryQueryDto parm)
        {
            var list = _service.ExportList(parm).Result;
            if (list == null || list.Count <= 0)
            {
                return ToResponse(ResultCode.FAIL, "No data to export");
            }
            var result = ExportExcelMini(list, "WorkCategories", "WorkCategories");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}
