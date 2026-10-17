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
using ZR.Model.Finance.Enums;
using ZR.Service.IService.Finance.Tax;

namespace ZR.Admin.WebApi.Controllers.Finance.Tax
{
    /// <summary>
    /// Tax Code API Controller
    /// وحدة تحكم API رموز الضرائب
    /// </summary>
    [Verify]
    [Route("finance/tax/codes")]
    [ApiController]
    [Authorize]
    public class TaxCodeController : BaseController
    {
        private readonly ITaxCodeService _taxCodeService;

        public TaxCodeController(ITaxCodeService taxCodeService)
        {
            _taxCodeService = taxCodeService;
        }

        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax codes
        /// الحصول على قائمة مرقمة من رموز الضرائب
        /// </summary>
        [HttpGet]
        public IActionResult GetList([FromQuery] TaxCodeQueryDto query)
        {
            var result = _taxCodeService.GetList(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax code by ID
        /// الحصول على رمز الضريبة بواسطة المعرف
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _taxCodeService.GetByIdAsync(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Tax code not found - رمز الضريبة غير موجود");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax code by code
        /// الحصول على رمز الضريبة بواسطة الرمز
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<IActionResult> GetByCode(string code, [FromQuery] long? companyId = null)
        {
            var result = await _taxCodeService.GetByCodeAsync(code, companyId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Tax code not found - رمز الضريبة غير موجود");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create tax code
        /// إنشاء رمز ضريبة جديد
        /// </summary>
        [HttpPost]
        [Log(Title = "Tax Code", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> Create([FromBody] CreateTaxCodeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var result = await _taxCodeService.CreateAsync(dto);
                return SUCCESS(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Update tax code
        /// تحديث رمز الضريبة
        /// </summary>
        [HttpPut]
        [Log(Title = "Tax Code", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Update([FromBody] UpdateTaxCodeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var result = await _taxCodeService.UpdateAsync(dto);
                return SUCCESS(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Delete tax codes
        /// حذف رموز الضرائب
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Tax Code", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();

            try
            {
                var count = await _taxCodeService.DeleteAsync(idArray);
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
        /// Get active tax codes for dropdown
        /// الحصول على رموز الضرائب النشطة للقائمة المنسدلة
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTaxCodes([FromQuery] long? companyId = null)
        {
            var result = await _taxCodeService.GetActiveTaxCodesAsync(companyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get default tax code
        /// الحصول على رمز الضريبة الافتراضي
        /// </summary>
        [HttpGet("default")]
        public async Task<IActionResult> GetDefaultTaxCode([FromQuery] long? companyId = null)
        {
            var result = await _taxCodeService.GetDefaultTaxCodeAsync(companyId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "No default tax code configured - لم يتم تكوين رمز ضريبة افتراضي");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax codes by type
        /// الحصول على رموز الضرائب حسب النوع
        /// </summary>
        [HttpGet("by-type/{taxType}")]
        public IActionResult GetByTaxType(TaxType taxType, [FromQuery] long? companyId = null)
        {
            var result = _taxCodeService.GetByTaxType(taxType, companyId);
            return SUCCESS(result);
        }

        #endregion

        #region Rate Operations

        /// <summary>
        /// Get effective rate for tax code on a date
        /// الحصول على المعدل الساري لرمز الضريبة في تاريخ معين
        /// </summary>
        [HttpGet("{id}/rate")]
        public async Task<IActionResult> GetEffectiveRate(Guid id, [FromQuery] DateTime? date = null)
        {
            var effectiveDate = date ?? DateTime.Today;
            var rate = await _taxCodeService.GetEffectiveRateAsync(id, effectiveDate);
            return SUCCESS(new { TaxCodeId = id, Date = effectiveDate, Rate = rate });
        }

        /// <summary>
        /// Get effective rate by code on a date
        /// الحصول على المعدل الساري بواسطة الرمز في تاريخ معين
        /// </summary>
        [HttpGet("rate-by-code/{code}")]
        public async Task<IActionResult> GetEffectiveRateByCode(string code, [FromQuery] DateTime? date = null, [FromQuery] long? companyId = null)
        {
            var effectiveDate = date ?? DateTime.Today;
            var rate = await _taxCodeService.GetEffectiveRateByCodeAsync(code, effectiveDate, companyId);
            return SUCCESS(new { Code = code, Date = effectiveDate, Rate = rate });
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Check if code exists
        /// التحقق من وجود الرمز
        /// </summary>
        [HttpGet("exists/{code}")]
        public async Task<IActionResult> CodeExists(string code, [FromQuery] long? companyId = null, [FromQuery] Guid? excludeId = null)
        {
            var exists = await _taxCodeService.CodeExistsAsync(code, companyId, excludeId);
            return SUCCESS(new { Code = code, Exists = exists });
        }

        /// <summary>
        /// Check if tax code can be deleted
        /// التحقق من إمكانية حذف رمز الضريبة
        /// </summary>
        [HttpGet("{id}/can-delete")]
        public async Task<IActionResult> CanDelete(Guid id)
        {
            var canDelete = await _taxCodeService.CanDeleteAsync(id);
            return SUCCESS(new { Id = id, CanDelete = canDelete });
        }

        #endregion
    }
}
