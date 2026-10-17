using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Admin.WebApi.Controllers.Finance.Configuration
{
    /// <summary>
    /// Auto Posting Controller - متحكم الترحيل التلقائي
    /// </summary>
    [Verify]
    [Route("finance/configuration/auto-posting")]
    [ApiController]
    public class AutoPostingController : BaseController
    {
        private readonly IAutoPostingService _autoPostingService;

        public AutoPostingController(IAutoPostingService autoPostingService)
        {
            _autoPostingService = autoPostingService;
        }

        /// <summary>
        /// Get list of auto posting rules - الحصول على قائمة قواعد الترحيل التلقائي
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:configuration:auto-posting:list")]
        public IActionResult GetList([FromQuery] AutoPostingRuleQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_autoPostingService.GetList(query, pager));
        }

        /// <summary>
        /// Get rule by ID - الحصول على القاعدة بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:configuration:auto-posting:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _autoPostingService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Auto posting rule not found - قاعدة الترحيل التلقائي غير موجودة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create rule - إنشاء قاعدة
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:configuration:auto-posting:add")]
        [Log(Title = "Auto Posting Rule", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateAutoPostingRuleDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _autoPostingService.Create(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update rule - تحديث قاعدة
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:configuration:auto-posting:edit")]
        [Log(Title = "Auto Posting Rule", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateAutoPostingRuleDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _autoPostingService.Update(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete rules - حذف قواعد
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:configuration:auto-posting:delete")]
        [Log(Title = "Auto Posting Rule", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = Array.ConvertAll(ids.Split(','), Guid.Parse);
            var result = _autoPostingService.Delete(idArray);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get rules for transaction type - الحصول على القواعد لنوع المعاملة
        /// </summary>
        [HttpGet("by-transaction-type/{transactionType}")]
        [ActionPermissionFilter(Permission = "finance:configuration:auto-posting:query")]
        public IActionResult GetRulesForTransactionType(TransactionType transactionType)
        {
            return SUCCESS(_autoPostingService.GetRulesForTransactionType(transactionType));
        }

        /// <summary>
        /// Apply auto posting rule - تطبيق قاعدة الترحيل التلقائي
        /// </summary>
        [HttpPost("apply")]
        [ActionPermissionFilter(Permission = "finance:configuration:auto-posting:query")]
        public IActionResult ApplyAutoPostingRule([FromQuery] TransactionType transactionType, [FromQuery] decimal amount, [FromQuery] string description = null)
        {
            try
            {
                var result = _autoPostingService.ApplyAutoPostingRule(transactionType, amount, description);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }
    }
}


