using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using Infrastructure;
using ZR.Model;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Admin.WebApi.Controllers.Finance.Configuration
{
    /// <summary>
    /// Tax Configuration Controller - تحكم إعدادات الضرائب
    /// Controller for managing VAT Input/Output and AR/AP account configurations
    /// </summary>
    [Verify]
    [Route("erp/finance/configuration/[controller]")]
    [ApiController]
    public class TaxConfigurationController : BaseController
    {
        private readonly ITaxConfigurationService _taxConfigService;

        public TaxConfigurationController(ITaxConfigurationService taxConfigService)
        {
            _taxConfigService = taxConfigService;
        }

        /// <summary>
        /// Get tax configuration for company - الحصول على إعدادات الضريبة للشركة
        /// </summary>
        /// <param name="companyId">Company ID (optional)</param>
        [HttpGet("company")]
        public IActionResult GetByCompany([FromQuery] long? companyId = null)
        {
            var response = _taxConfigService.GetByCompanyId(companyId);
            if (response == null)
            {
                // If companyId is not provided, try to reflect the active scoped company (dev middleware / request scope)
                if (!companyId.HasValue && App.HttpContext?.Items != null
                    && App.HttpContext.Items.TryGetValue("CompanyId", out var scopedCompanyObj)
                    && scopedCompanyObj != null)
                {
                    if (scopedCompanyObj is long l) companyId = l;
                    else if (scopedCompanyObj is int i) companyId = i;
                }

                // Return empty configuration with status
                return SUCCESS(new TaxConfigurationDetailDto
                {
                    CompanyId = companyId,
                    IsConfigured = false
                });
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Get tax configuration by ID - الحصول على إعدادات الضريبة بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _taxConfigService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Configuration not found - الإعداد غير موجود");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Get configuration status - الحصول على حالة الإعدادات
        /// Returns detailed status about what's configured and what's missing
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus([FromQuery] long? companyId = null)
        {
            var response = _taxConfigService.GetConfigurationStatus(companyId);
            return SUCCESS(response);
        }

        /// <summary>
        /// Save tax configuration - حفظ إعدادات الضريبة
        /// Creates new configuration or updates existing one
        /// </summary>
        [HttpPost]
        [Log(Title = "Save Tax Configuration", BusinessType = BusinessType.INSERT)]
        public IActionResult Save([FromBody] SaveTaxConfigurationDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            var userId = HttpContext.GetName();
            var result = _taxConfigService.SaveConfiguration(dto, userId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get VAT Input Account ID - الحصول على معرف حساب ضريبة المدخلات
        /// Used internally by invoice posting services
        /// </summary>
        [HttpGet("vat-input-account")]
        public IActionResult GetVATInputAccount([FromQuery] long? companyId = null)
        {
            var accountId = _taxConfigService.GetVATInputAccountId(companyId);
            if (!accountId.HasValue)
            {
                return ToResponse(ResultCode.FAIL, "VAT Input Account not configured - حساب ضريبة المدخلات غير مكون");
            }
            return SUCCESS(new { AccountId = accountId.Value });
        }

        /// <summary>
        /// Get VAT Output Account ID - الحصول على معرف حساب ضريبة المخرجات
        /// Used internally by invoice posting services
        /// </summary>
        [HttpGet("vat-output-account")]
        public IActionResult GetVATOutputAccount([FromQuery] long? companyId = null)
        {
            var accountId = _taxConfigService.GetVATOutputAccountId(companyId);
            if (!accountId.HasValue)
            {
                return ToResponse(ResultCode.FAIL, "VAT Output Account not configured - حساب ضريبة المخرجات غير مكون");
            }
            return SUCCESS(new { AccountId = accountId.Value });
        }

        /// <summary>
        /// Ensure configuration exists - التأكد من وجود الإعدادات
        /// Creates default empty configuration if none exists
        /// </summary>
        [HttpPost("ensure")]
        public IActionResult EnsureExists([FromQuery] long? companyId = null)
        {
            _taxConfigService.EnsureConfigurationExists(companyId);
            return SUCCESS("Configuration exists - الإعداد موجود");
        }
    }
}
