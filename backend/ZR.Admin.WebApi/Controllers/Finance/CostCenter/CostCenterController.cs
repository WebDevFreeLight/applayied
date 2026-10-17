using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.CostCenter.Dto;
using ZR.Service.IService.Finance;
using ZR.Common;
using Infrastructure.Enums;

namespace ZR.Admin.WebApi.Controllers.Finance.CostCenter
{
    /// <summary>
    /// Cost Center Management - إدارة مراكز التكلفة
    /// </summary>
    [Verify]
    [Route("finance/cost-centers")]
    [ApiController]
    public class CostCenterController : BaseController
    {
        private readonly ICostCenterService _costCenterService;

        public CostCenterController(ICostCenterService costCenterService)
        {
            _costCenterService = costCenterService;
        }

        /// <summary>
        /// Get cost center list - قائمة مراكز التكلفة
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:costcenter:list")]
        public IActionResult GetList([FromQuery] CostCenterQueryDto query, [FromQuery] PagerInfo pager)
        {
            var response = _costCenterService.GetList(query, pager);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get cost center by ID - الحصول على مركز تكلفة بالمعرف
        /// </summary>
        [HttpGet("{costCenterId}")]
        [ActionPermissionFilter(Permission = "finance:costcenter:query")]
        public IActionResult GetInfo(long costCenterId)
        {
            var response = _costCenterService.GetById(costCenterId);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Cost center not found - مركز التكلفة غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create cost center - إنشاء مركز تكلفة
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:costcenter:add")]
        [Log(Title = "Create Cost Center", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateCostCenterDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var response = _costCenterService.Create(dto, HttpContext.GetName());
                return SUCCESS(response);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update cost center - تحديث مركز تكلفة
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:costcenter:edit")]
        [Log(Title = "Update Cost Center", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateCostCenterDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var response = _costCenterService.Update(dto, HttpContext.GetName());
                return ToResponse(response);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete cost centers - حذف مراكز التكلفة
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:costcenter:delete")]
        [Log(Title = "Delete Cost Centers", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            try
            {
                long[] idArr = Tools.SpitLongArrary(ids);
                if (idArr.Length == 0)
                    return ToResponse(ResultCode.FAIL, "No valid IDs provided - لم يتم تقديم معرفات صالحة");

                var response = _costCenterService.Delete(idArr);
                return ToResponse(response);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get active cost centers for dropdown - مراكز التكلفة النشطة للقائمة المنسدلة
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "finance:costcenter:query")]
        public IActionResult GetActiveCostCenters([FromQuery] long? branchId)
        {
            var response = _costCenterService.GetActiveCostCenters(branchId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get cost centers by type - مراكز التكلفة حسب النوع
        /// </summary>
        [HttpGet("by-type/{costCenterType}")]
        [ActionPermissionFilter(Permission = "finance:costcenter:query")]
        public IActionResult GetByType(string costCenterType)
        {
            var response = _costCenterService.GetCostCentersByType(costCenterType);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get cost center tree - شجرة مراكز التكلفة
        /// </summary>
        [HttpGet("tree")]
        [ActionPermissionFilter(Permission = "finance:costcenter:query")]
        public IActionResult GetCostCenterTree()
        {
            var response = _costCenterService.GetCostCenterTree();
            return SUCCESS(response);
        }

        /// <summary>
        /// Get cost center balance - رصيد مركز التكلفة
        /// </summary>
        [HttpGet("balance/{costCenterId}")]
        [ActionPermissionFilter(Permission = "finance:costcenter:query")]
        public IActionResult GetBalance(long costCenterId, [FromQuery] DateTime? asOfDate)
        {
            try
            {
                var response = _costCenterService.GetCostCenterBalance(costCenterId, asOfDate);
                return SUCCESS(response);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get cost center report - تقرير مراكز التكلفة
        /// </summary>
        [HttpGet("report")]
        [ActionPermissionFilter(Permission = "finance:costcenter:query")]
        public IActionResult GetReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var response = _costCenterService.GetCostCenterReport(fromDate, toDate);
            return SUCCESS(response);
        }

        /// <summary>
        /// Check if code exists - التحقق من وجود الكود
        /// </summary>
        [HttpGet("check-code")]
        [ActionPermissionFilter(Permission = "finance:costcenter:query")]
        public IActionResult CheckCodeExists([FromQuery] string code, [FromQuery] long? excludeId)
        {
            var exists = _costCenterService.CodeExists(code, excludeId);
            return SUCCESS(new { Exists = exists });
        }

        /// <summary>
        /// Get next cost center code - الحصول على الكود التالي
        /// </summary>
        [HttpGet("next-code")]
        [ActionPermissionFilter(Permission = "finance:costcenter:query")]
        public IActionResult GetNextCode([FromQuery] string prefix = "CC")
        {
            var nextCode = _costCenterService.GetNextCode(prefix);
            return SUCCESS(new { Code = nextCode });
        }
    }
}
