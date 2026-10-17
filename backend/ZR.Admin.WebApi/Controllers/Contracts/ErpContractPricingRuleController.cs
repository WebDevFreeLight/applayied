using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Contracts.Dto;
using ZR.Service.Contracts.IService;

namespace ZR.Admin.WebApi.Controllers.Contracts
{
    /// <summary>
    /// Contract Pricing Rule Controller - قواعد تسعير العقد
    /// </summary>
    [Authorize]
    [Route("erp/contracts/pricing-rules")]
    [ApiController]
    public class ErpContractPricingRuleController : BaseController
    {
        private readonly IContractPricingRuleService _pricingRuleService;

        public ErpContractPricingRuleController(IContractPricingRuleService pricingRuleService)
        {
            _pricingRuleService = pricingRuleService;
        }

        /// <summary>
        /// Get pricing rules by contract ID
        /// الحصول على قواعد التسعير بمعرف العقد
        /// </summary>
        [HttpGet("contract/{contractId}")]
        public IActionResult GetByContractId(Guid contractId, [FromQuery] bool? isActive = null)
        {
            var result = _pricingRuleService.GetByContractId(contractId, isActive);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get pricing rule by ID
        /// الحصول على قاعدة التسعير بالمعرف
        /// </summary>
        [HttpGet("{ruleId}")]
        public IActionResult GetById(Guid ruleId)
        {
            var result = _pricingRuleService.GetById(ruleId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Pricing rule not found");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create pricing rule
        /// إنشاء قاعدة تسعير
        /// </summary>
        [HttpPost]
        [Log(Title = "Contract Pricing Rule", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateErpContractPricingRuleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var ruleId = _pricingRuleService.Create(dto, HttpContext.GetName());
                return SUCCESS(new { RuleId = ruleId });
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update pricing rule
        /// تحديث قاعدة التسعير
        /// </summary>
        [HttpPut]
        [Log(Title = "Contract Pricing Rule", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateErpContractPricingRuleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR);
            }

            try
            {
                var result = _pricingRuleService.Update(dto, HttpContext.GetName());
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete pricing rule
        /// حذف قاعدة التسعير
        /// </summary>
        [HttpDelete("{ruleId}")]
        [Log(Title = "Contract Pricing Rule", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid ruleId)
        {
            try
            {
                var result = _pricingRuleService.Delete(ruleId);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Toggle rule active status
        /// تبديل حالة نشاط القاعدة
        /// </summary>
        [HttpPost("toggle-active/{ruleId}")]
        [Log(Title = "Contract Pricing Rule", BusinessType = BusinessType.UPDATE)]
        public IActionResult ToggleActive(Guid ruleId)
        {
            try
            {
                var result = _pricingRuleService.ToggleActive(ruleId);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Calculate amount with pricing rules
        /// حساب المبلغ مع قواعد التسعير
        /// </summary>
        [HttpGet("calculate")]
        public IActionResult CalculateAmount([FromQuery] Guid contractId, [FromQuery] decimal baseAmount, [FromQuery] DateTime billingDate)
        {
            var result = _pricingRuleService.CalculateAmount(contractId, baseAmount, billingDate);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get active rules for date
        /// الحصول على القواعد النشطة للتاريخ
        /// </summary>
        [HttpGet("active")]
        public IActionResult GetActiveRulesForDate([FromQuery] Guid contractId, [FromQuery] DateTime date)
        {
            var result = _pricingRuleService.GetActiveRulesForDate(contractId, date);
            return SUCCESS(result);
        }
    }
}
