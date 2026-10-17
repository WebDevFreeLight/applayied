using Microsoft.AspNetCore.Mvc;
using ZR.Model.Subscriptions;
using ZR.Service.Subscriptions.IService;
using ZR.Admin.WebApi.Filters;

namespace ZR.Admin.WebApi.Controllers.Subscriptions
{
    /// <summary>
    /// Resource Controller - متحكم الموارد (الغرف/المعدات)
    /// </summary>
    [Verify]
    [Route("api/erp/resources")]
    [ApiController]
    public class ErpResourceController : BaseController
    {
        private readonly IErpResourceService _resourceService;

        public ErpResourceController(IErpResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        /// <summary>
        /// Get resource list - قائمة الموارد
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:resource:list")]
        public IActionResult GetList([FromQuery] ErpResource parm, [FromQuery] Model.PagerInfo pager)
        {
            var response = _resourceService.GetList(parm, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get resource by ID - الحصول على مورد
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "erp:resource:query")]
        public IActionResult GetInfo(long id)
        {
            var response = _resourceService.GetInfo(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Resource not found - المورد غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Add resource - إضافة مورد
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "erp:resource:add")]
        [Log(Title = "Resource", BusinessType = BusinessType.INSERT)]
        public IActionResult Add([FromBody] ErpResource model)
        {
            var result = _resourceService.AddResource(model);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update resource - تعديل مورد
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "erp:resource:edit")]
        [Log(Title = "Resource", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] ErpResource model)
        {
            var result = _resourceService.UpdateResource(model);
            return ToResponse(result);
        }

        /// <summary>
        /// Delete resources - حذف موارد
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "erp:resource:delete")]
        [Log(Title = "Resource", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) return ToResponse(ResultCode.FAIL);
            return ToResponse(_resourceService.Delete(idsArr));
        }

        /// <summary>
        /// Get active resources - الموارد النشطة
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "erp:resource:list")]
        public IActionResult GetActiveResources([FromQuery] long? BranchID)
        {
            var result = _resourceService.GetActiveResources(BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get resources by type - الموارد حسب النوع
        /// </summary>
        [HttpGet("type/{resourceType}")]
        [ActionPermissionFilter(Permission = "erp:resource:list")]
        public IActionResult GetResourcesByType(string resourceType)
        {
            var result = _resourceService.GetResourcesByType(resourceType);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get available resources - الموارد المتاحة
        /// </summary>
        [HttpGet("available")]
        [ActionPermissionFilter(Permission = "erp:resource:list")]
        public IActionResult GetAvailableResources([FromQuery] DateTime startTime, [FromQuery] DateTime endTime, [FromQuery] long? BranchID)
        {
            var result = _resourceService.GetAvailableResources(startTime, endTime, BranchID);
            return SUCCESS(result);
        }

        /// <summary>
        /// Check resource availability - التحقق من توفر المورد
        /// </summary>
        [HttpGet("{id}/availability")]
        [ActionPermissionFilter(Permission = "erp:resource:query")]
        public IActionResult CheckAvailability(long id, [FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
        {
            var isAvailable = _resourceService.IsResourceAvailable(id, startTime, endTime);
            return SUCCESS(new { ResourceId = id, IsAvailable = isAvailable, StartTime = startTime, EndTime = endTime });
        }

        /// <summary>
        /// Get resource schedule - جدول المورد
        /// </summary>
        [HttpGet("{id}/schedule")]
        [ActionPermissionFilter(Permission = "erp:resource:query")]
        public IActionResult GetSchedule(long id, [FromQuery] DateTime date)
        {
            var result = _resourceService.GetResourceSchedule(id, date);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get resource utilization - استخدام المورد
        /// </summary>
        [HttpGet("{id}/utilization")]
        [ActionPermissionFilter(Permission = "erp:resource:report")]
        public IActionResult GetUtilization(long id, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var result = _resourceService.GetResourceUtilization(id, fromDate, toDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update resource status - تحديث حالة المورد
        /// </summary>
        [HttpPost("{id}/status")]
        [ActionPermissionFilter(Permission = "erp:resource:status")]
        [Log(Title = "Resource Status", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateStatus(long id, [FromQuery] bool isBookable)
        {
            var result = _resourceService.UpdateResourceStatus(id, isBookable);
            return ToResponse(result);
        }

        /// <summary>
        /// Get resource revenue - إيرادات المورد
        /// </summary>
        [HttpGet("{id}/revenue")]
        [ActionPermissionFilter(Permission = "erp:resource:report")]
        public IActionResult GetRevenue(long id, [FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var revenue = _resourceService.GetResourceRevenue(id, fromDate, toDate);
            return SUCCESS(new { ResourceId = id, Revenue = revenue, FromDate = fromDate, ToDate = toDate });
        }

        /// <summary>
        /// Export resources - تصدير الموارد
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "erp:resource:export")]
        [Log(Title = "Resource", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] ErpResource parm, [FromQuery] Model.PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _resourceService.GetList(parm, pager).Result;
            var result = ExportExcelMini(list, "Resources", "Resources - الموارد");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}

