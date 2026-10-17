using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Admin.WebApi.Controllers.Finance.Configuration
{
    /// <summary>
    /// ERP Settings Controller - متحكم إعدادات النظام
    /// </summary>
    [Verify]
    [Route("finance/configuration/erp-settings")]
    [ApiController]
    public class ERPSettingsController : BaseController
    {
        private readonly IERPSettingsService _erpSettingsService;

        public ERPSettingsController(IERPSettingsService erpSettingsService)
        {
            _erpSettingsService = erpSettingsService;
        }

        /// <summary>
        /// Get list of ERP settings - الحصول على قائمة إعدادات النظام
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:configuration:erp-settings:list")]
        public IActionResult GetList([FromQuery] ERPSettingsQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_erpSettingsService.GetList(query, pager));
        }

        /// <summary>
        /// Get settings by ID - الحصول على الإعدادات بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:configuration:erp-settings:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _erpSettingsService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "ERP settings not found - إعدادات النظام غير موجودة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Get settings by company - الحصول على إعدادات الشركة
        /// </summary>
        [HttpGet("by-company")]
        [ActionPermissionFilter(Permission = "finance:configuration:erp-settings:query")]
        public IActionResult GetByCompany([FromQuery] long? companyId = null)
        {
            var response = _erpSettingsService.GetByCompanyId(companyId);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "ERP settings not found for company - إعدادات النظام غير موجودة للشركة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Get settings by company (path parameter) - الحصول على إعدادات الشركة
        /// </summary>
        [HttpGet("by-company/{companyId}")]
        [ActionPermissionFilter(Permission = "finance:configuration:erp-settings:query")]
        public IActionResult GetByCompanyId(long companyId)
        {
            var response = _erpSettingsService.GetByCompanyId(companyId);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "ERP settings not found for company - إعدادات النظام غير موجودة للشركة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Get or Create settings for company - الحصول على أو إنشاء إعدادات الشركة
        /// Returns existing settings if found, creates default settings if not
        /// </summary>
        [HttpGet("get-or-create")]
        [ActionPermissionFilter(Permission = "finance:configuration:erp-settings:query")]
        public IActionResult GetOrCreate([FromQuery] long? companyId = null)
        {
            var response = _erpSettingsService.GetOrCreate(companyId, HttpContext.GetName());
            return SUCCESS(response);
        }

        /// <summary>
        /// Create settings - إنشاء إعدادات
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:configuration:erp-settings:add")]
        [Log(Title = "ERP Settings", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateERPSettingsDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _erpSettingsService.Create(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update settings - تحديث إعدادات
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:configuration:erp-settings:edit")]
        [Log(Title = "ERP Settings", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateERPSettingsDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _erpSettingsService.Update(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete settings - حذف إعدادات
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:configuration:erp-settings:delete")]
        [Log(Title = "ERP Settings", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = Array.ConvertAll(ids.Split(','), Guid.Parse);
            var result = _erpSettingsService.Delete(idArray);
            return SUCCESS(result);
        }
    }
}


