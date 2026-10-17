using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Admin.WebApi.Controllers.Finance.Configuration
{
    /// <summary>
    /// Tax Rule Controller - متحكم قواعد الضريبة
    /// </summary>
    [Verify]
    [Route("finance/configuration/tax-rules")]
    [ApiController]
    public class TaxRuleController : BaseController
    {
        private readonly ITaxRuleService _taxRuleService;

        public TaxRuleController(ITaxRuleService taxRuleService)
        {
            _taxRuleService = taxRuleService;
        }

        /// <summary>
        /// Get list of tax rules - الحصول على قائمة قواعد الضريبة
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:configuration:tax-rule:list")]
        public IActionResult GetList([FromQuery] TaxRuleQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_taxRuleService.GetList(query, pager));
        }

        /// <summary>
        /// Get tax rule by ID - الحصول على قاعدة الضريبة بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:configuration:tax-rule:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _taxRuleService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Tax rule not found - قاعدة الضريبة غير موجودة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create tax rule - إنشاء قاعدة ضريبة
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:configuration:tax-rule:add")]
        [Log(Title = "Tax Rule", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateTaxRuleDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _taxRuleService.Create(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update tax rule - تحديث قاعدة ضريبة
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:configuration:tax-rule:edit")]
        [Log(Title = "Tax Rule", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateTaxRuleDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _taxRuleService.Update(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete tax rules - حذف قواعد ضريبة
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:configuration:tax-rule:delete")]
        [Log(Title = "Tax Rule", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = Array.ConvertAll(ids.Split(','), Guid.Parse);
            var result = _taxRuleService.Delete(idArray);
            return SUCCESS(result);
        }

        /// <summary>
        /// Calculate tax - حساب الضريبة
        /// </summary>
        [HttpPost("calculate")]
        [ActionPermissionFilter(Permission = "finance:configuration:tax-rule:query")]
        public IActionResult CalculateTax([FromBody] CalculateTaxDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _taxRuleService.CalculateTax(dto);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get active tax rules - الحصول على قواعد الضريبة النشطة
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "finance:configuration:tax-rule:list")]
        public IActionResult GetActiveTaxRules()
        {
            return SUCCESS(_taxRuleService.GetActiveTaxRules());
        }
    }
}


