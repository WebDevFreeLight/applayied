using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Admin.WebApi.Controllers.Finance.Configuration
{
    /// <summary>
    /// Currency Controller - متحكم العملات
    /// </summary>
    [Verify]
    [Route("finance/configuration/currencies")]
    [ApiController]
    public class CurrencyController : BaseController
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        /// <summary>
        /// Get list of currencies - الحصول على قائمة العملات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:configuration:currency:list")]
        public IActionResult GetList([FromQuery] CurrencyQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_currencyService.GetList(query, pager));
        }

        /// <summary>
        /// Get currency by ID - الحصول على العملة بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:configuration:currency:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _currencyService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Currency not found - العملة غير موجودة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create currency - إنشاء عملة
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:configuration:currency:add")]
        [Log(Title = "Currency", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateCurrencyDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _currencyService.Create(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update currency - تحديث عملة
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:configuration:currency:edit")]
        [Log(Title = "Currency", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateCurrencyDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _currencyService.Update(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete currencies - حذف عملات
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:configuration:currency:delete")]
        [Log(Title = "Currency", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = Array.ConvertAll(ids.Split(','), Guid.Parse);
            var result = _currencyService.Delete(idArray);
            return SUCCESS(result);
        }

        /// <summary>
        /// Set base currency - تعيين العملة الأساسية
        /// </summary>
        [HttpPost("{id}/set-base")]
        [ActionPermissionFilter(Permission = "finance:configuration:currency:edit")]
        [Log(Title = "Set Base Currency", BusinessType = BusinessType.UPDATE)]
        public IActionResult SetBaseCurrency(Guid id)
        {
            try
            {
                var result = _currencyService.SetBaseCurrency(id, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get base currency - الحصول على العملة الأساسية
        /// </summary>
        [HttpGet("base")]
        [ActionPermissionFilter(Permission = "finance:configuration:currency:query")]
        public IActionResult GetBaseCurrency()
        {
            var response = _currencyService.GetBaseCurrency();
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Base currency not found - العملة الأساسية غير موجودة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Get active currencies - الحصول على العملات النشطة
        /// </summary>
        [HttpGet("active")]
        [ActionPermissionFilter(Permission = "finance:configuration:currency:list")]
        public IActionResult GetActiveCurrencies()
        {
            return SUCCESS(_currencyService.GetActiveCurrencies());
        }

        /// <summary>
        /// Check if currency code exists - التحقق من وجود رمز العملة
        /// </summary>
        [HttpGet("check-code")]
        [ActionPermissionFilter(Permission = "finance:configuration:currency:query")]
        public IActionResult CheckCodeExists([FromQuery] string code, [FromQuery] Guid? excludeId = null)
        {
            return SUCCESS(_currencyService.CodeExists(code, excludeId));
        }
    }
}


