using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Admin.WebApi.Controllers.Finance.Configuration
{
    /// <summary>
    /// System Configuration Controller - متحكم إعدادات النظام
    /// </summary>
    [Verify]
    [Route("api/erp/finance/configuration")]
    [ApiController]
    public class SystemConfigurationController : BaseController
    {
        private readonly ISystemConfigurationService _configService;

        public SystemConfigurationController(ISystemConfigurationService configService)
        {
            _configService = configService;
        }

        /// <summary>
        /// Get list of configurations - الحصول على قائمة الإعدادات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:configuration:list")]
        public IActionResult GetList([FromQuery] SystemConfigurationQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_configService.GetList(query, pager));
        }

        /// <summary>
        /// Get configuration by ID - الحصول على الإعداد بالمعرف
        /// </summary>
        [HttpGet("get/{id}")]
        [ActionPermissionFilter(Permission = "finance:configuration:query")]
        public IActionResult GetById(Guid id)
        {
            var response = _configService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Configuration not found - الإعداد غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Get configuration by key - الحصول على الإعداد بالمفتاح
        /// </summary>
        [HttpGet("getByKey/{key}")]
        [ActionPermissionFilter(Permission = "finance:configuration:query")]
        public IActionResult GetByKey(string key, [FromQuery] long? companyId = null)
        {
            var response = _configService.GetByKey(key, companyId);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Configuration not found - الإعداد غير موجود");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create configuration - إنشاء إعداد
        /// </summary>
        [HttpPost("create")]
        [ActionPermissionFilter(Permission = "finance:configuration:add")]
        [Log(Title = "System Configuration", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateConfigurationDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _configService.Create(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update configuration - تحديث إعداد
        /// </summary>
        [HttpPut("update/{id}")]
        [ActionPermissionFilter(Permission = "finance:configuration:edit")]
        [Log(Title = "System Configuration", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update(Guid id, [FromBody] UpdateConfigurationDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            if (id != dto.Id)
                return ToResponse(ResultCode.PARAM_ERROR, "ID mismatch - عدم تطابق المعرف");

            try
            {
                var result = _configService.Update(dto, HttpContext.GetName());
                if (result == 0)
                    return ToResponse(ResultCode.FAIL, "Update failed - فشل التحديث");

                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete configuration - حذف إعداد
        /// </summary>
        [HttpDelete("delete/{id}")]
        [ActionPermissionFilter(Permission = "finance:configuration:delete")]
        [Log(Title = "System Configuration", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid id)
        {
            var result = _configService.Delete(id);
            if (result == 0)
                return ToResponse(ResultCode.FAIL, "Delete failed or configuration not found - فشل الحذف أو الإعداد غير موجود");

            return ToResponse(result);
        }

        /// <summary>
        /// Export configurations - تصدير الإعدادات
        /// </summary>
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "finance:configuration:export")]
        [Log(Title = "System Configuration", BusinessType = BusinessType.EXPORT)]
        public IActionResult Export([FromQuery] SystemConfigurationQueryDto query, [FromQuery] PagerInfo pager)
        {
            pager.PageSize = 10000;
            var list = _configService.GetList(query, pager).Result;
            var result = ExportExcelMini(list, "SystemConfiguration", "System Configurations - إعدادات النظام");
            return ExportExcel(result.Item2, result.Item1);
        }
    }
}

