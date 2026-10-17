using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Controllers;
using Infrastructure.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model;
using ZR.Model.Finance.Tax.Dto;
using ZR.Service.IService.Finance.Tax;

namespace ZR.Admin.WebApi.Controllers.Finance.Tax
{
    /// <summary>
    /// Tax Rate API Controller
    /// وحدة تحكم API معدلات الضرائب
    /// </summary>
    [Verify]
    [Route("finance/tax/rates")]
    [ApiController]
    [Authorize]
    public class TaxRateController : BaseController
    {
        private readonly ITaxRateService _taxRateService;

        public TaxRateController(ITaxRateService taxRateService)
        {
            _taxRateService = taxRateService;
        }

        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax rates
        /// الحصول على قائمة مرقمة من معدلات الضرائب
        /// </summary>
        [HttpGet]
        public IActionResult GetList([FromQuery] TaxRateQueryDto query)
        {
            var result = _taxRateService.GetList(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax rate by ID
        /// الحصول على معدل الضريبة بواسطة المعرف
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _taxRateService.GetByIdAsync(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Tax rate not found - معدل الضريبة غير موجود");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create tax rate
        /// إنشاء معدل ضريبة جديد
        /// </summary>
        [HttpPost]
        [Log(Title = "Tax Rate", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> Create([FromBody] CreateTaxRateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var result = await _taxRateService.CreateAsync(dto);
                return SUCCESS(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Update tax rate
        /// تحديث معدل الضريبة
        /// </summary>
        [HttpPut]
        [Log(Title = "Tax Rate", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Update([FromBody] UpdateTaxRateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var result = await _taxRateService.UpdateAsync(dto);
                return SUCCESS(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// End a tax rate (set effective_to date)
        /// إنهاء معدل الضريبة (تحديد تاريخ الانتهاء)
        /// </summary>
        [HttpPost("end")]
        [Log(Title = "Tax Rate", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> EndRate([FromBody] EndTaxRateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var result = await _taxRateService.EndRateAsync(dto);
                return SUCCESS(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Delete tax rates
        /// حذف معدلات الضرائب
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Tax Rate", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();

            try
            {
                var count = await _taxRateService.DeleteAsync(idArray);
                return ToResponse(count);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Get all rates for a tax code
        /// الحصول على جميع المعدلات لرمز ضريبة
        /// </summary>
        [HttpGet("by-tax-code/{taxCodeId}")]
        public async Task<IActionResult> GetRatesByTaxCode(Guid taxCodeId)
        {
            var result = await _taxRateService.GetRatesByTaxCodeAsync(taxCodeId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get effective rate for a specific date
        /// الحصول على المعدل الساري لتاريخ محدد
        /// </summary>
        [HttpPost("rate-for-date")]
        public async Task<IActionResult> GetRateForDate([FromBody] GetRateForDateRequest request)
        {
            var result = await _taxRateService.GetRateForDateAsync(request);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get currently effective rates
        /// الحصول على المعدلات السارية حالياً
        /// </summary>
        [HttpGet("effective")]
        public IActionResult GetCurrentlyEffectiveRates()
        {
            var result = _taxRateService.GetCurrentlyEffectiveRates();
            return SUCCESS(result);
        }

        /// <summary>
        /// Get future rates
        /// الحصول على المعدلات المستقبلية
        /// </summary>
        [HttpGet("future")]
        public IActionResult GetFutureRates()
        {
            var result = _taxRateService.GetFutureRates();
            return SUCCESS(result);
        }

        /// <summary>
        /// Get expired rates
        /// الحصول على المعدلات المنتهية
        /// </summary>
        [HttpGet("expired")]
        public IActionResult GetExpiredRates()
        {
            var result = _taxRateService.GetExpiredRates();
            return SUCCESS(result);
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check for overlapping rates
        /// التحقق من تداخل المعدلات
        /// </summary>
        [HttpGet("check-overlap")]
        public async Task<IActionResult> HasOverlappingRates(
            [FromQuery] Guid taxCodeId,
            [FromQuery] DateTime effectiveFrom,
            [FromQuery] DateTime? effectiveTo = null,
            [FromQuery] Guid? excludeId = null)
        {
            var hasOverlap = await _taxRateService.HasOverlappingRatesAsync(taxCodeId, effectiveFrom, effectiveTo, excludeId);
            return SUCCESS(new { HasOverlap = hasOverlap });
        }

        /// <summary>
        /// Check if rate can be deleted
        /// التحقق من إمكانية حذف المعدل
        /// </summary>
        [HttpGet("{id}/can-delete")]
        public async Task<IActionResult> CanDelete(Guid id)
        {
            var canDelete = await _taxRateService.CanDeleteAsync(id);
            return SUCCESS(new { Id = id, CanDelete = canDelete });
        }

        #endregion

        #region Approval Operations

        /// <summary>
        /// Approve a tax rate
        /// الموافقة على معدل الضريبة
        /// </summary>
        [HttpPost("{id}/approve")]
        [Log(Title = "Tax Rate", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Approve(Guid id)
        {
            var userName = HttpContext.User.Identity?.Name ?? "System";
            var result = await _taxRateService.ApproveAsync(id, userName);
            return SUCCESS(result);
        }

        #endregion
    }
}
