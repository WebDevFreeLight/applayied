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
    /// Tax Group API Controller
    /// وحدة تحكم API مجموعات الضرائب
    /// </summary>
    [Verify]
    [Route("finance/tax/groups")]
    [ApiController]
    [Authorize]
    public class TaxGroupController : BaseController
    {
        private readonly ITaxGroupService _taxGroupService;

        public TaxGroupController(ITaxGroupService taxGroupService)
        {
            _taxGroupService = taxGroupService;
        }

        #region CRUD Operations

        /// <summary>
        /// Get paginated list of tax groups
        /// الحصول على قائمة مرقمة من مجموعات الضرائب
        /// </summary>
        [HttpGet]
        public IActionResult GetList([FromQuery] TaxGroupQueryDto query)
        {
            var result = _taxGroupService.GetList(query);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax group by ID
        /// الحصول على مجموعة الضريبة بواسطة المعرف
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _taxGroupService.GetByIdAsync(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Tax group not found - مجموعة الضريبة غير موجودة");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax group by code
        /// الحصول على مجموعة الضريبة بواسطة الرمز
        /// </summary>
        [HttpGet("by-code/{code}")]
        public async Task<IActionResult> GetByCode(string code, [FromQuery] long? companyId = null)
        {
            var result = await _taxGroupService.GetByCodeAsync(code, companyId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Tax group not found - مجموعة الضريبة غير موجودة");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Create tax group
        /// إنشاء مجموعة ضريبة جديدة
        /// </summary>
        [HttpPost]
        [Log(Title = "Tax Group", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> Create([FromBody] CreateTaxGroupDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return ToResponse(ResultCode.PARAM_ERROR, $"Invalid data - بيانات غير صالحة: {errors}");
            }

            try
            {
                var result = await _taxGroupService.CreateAsync(dto);
                return SUCCESS(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update tax group
        /// تحديث مجموعة الضريبة
        /// </summary>
        [HttpPut]
        [Log(Title = "Tax Group", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Update([FromBody] UpdateTaxGroupDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var result = await _taxGroupService.UpdateAsync(dto);
                return SUCCESS(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Delete tax groups
        /// حذف مجموعات الضرائب
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "Tax Group", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();

            try
            {
                var count = await _taxGroupService.DeleteAsync(idArray);
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
        /// Get active tax groups for dropdown
        /// الحصول على مجموعات الضرائب النشطة للقائمة المنسدلة
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveTaxGroups([FromQuery] long? companyId = null)
        {
            var result = await _taxGroupService.GetActiveTaxGroupsAsync(companyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get tax groups containing a tax code
        /// الحصول على مجموعات الضرائب التي تحتوي على رمز ضريبة معين
        /// </summary>
        [HttpGet("by-tax-code/{taxCodeId}")]
        public IActionResult GetGroupsByTaxCode(Guid taxCodeId)
        {
            var result = _taxGroupService.GetGroupsByTaxCode(taxCodeId);
            return SUCCESS(result);
        }

        #endregion

        #region Line Operations

        /// <summary>
        /// Add a tax code to a group
        /// إضافة رمز ضريبة إلى مجموعة
        /// </summary>
        [HttpPost("{groupId}/lines")]
        [Log(Title = "Tax Group Line", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> AddTaxCode(Guid groupId, [FromBody] TaxGroupLineDto line)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var result = await _taxGroupService.AddTaxCodeAsync(groupId, line);
                return SUCCESS(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Remove a tax code from a group
        /// إزالة رمز ضريبة من مجموعة
        /// </summary>
        [HttpDelete("{groupId}/lines/{lineId}")]
        [Log(Title = "Tax Group Line", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> RemoveTaxCode(Guid groupId, Guid lineId)
        {
            var result = await _taxGroupService.RemoveTaxCodeAsync(groupId, lineId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Update line in a group
        /// تحديث سطر في المجموعة
        /// </summary>
        [HttpPut("{groupId}/lines")]
        [Log(Title = "Tax Group Line", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> UpdateLine(Guid groupId, [FromBody] TaxGroupLineDto line)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var result = await _taxGroupService.UpdateLineAsync(groupId, line);
                return SUCCESS(result);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, ex.Message);
            }
        }

        #endregion

        #region Calculation Operations

        /// <summary>
        /// Calculate combined rate for a group
        /// حساب المعدل المجمع لمجموعة
        /// </summary>
        [HttpGet("{id}/combined-rate")]
        public async Task<IActionResult> CalculateCombinedRate(Guid id, [FromQuery] DateTime? effectiveDate = null)
        {
            var rate = await _taxGroupService.CalculateCombinedRateAsync(id, effectiveDate);
            return SUCCESS(new { GroupId = id, CombinedRate = rate, EffectiveDate = effectiveDate ?? DateTime.Today });
        }

        /// <summary>
        /// Calculate combined rate by code
        /// حساب المعدل المجمع بواسطة الرمز
        /// </summary>
        [HttpGet("combined-rate-by-code/{code}")]
        public async Task<IActionResult> CalculateCombinedRateByCode(string code, [FromQuery] DateTime? effectiveDate = null, [FromQuery] long? companyId = null)
        {
            var rate = await _taxGroupService.CalculateCombinedRateByCodeAsync(code, effectiveDate, companyId);
            return SUCCESS(new { Code = code, CombinedRate = rate, EffectiveDate = effectiveDate ?? DateTime.Today });
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
            var exists = await _taxGroupService.CodeExistsAsync(code, companyId, excludeId);
            return SUCCESS(new { Code = code, Exists = exists });
        }

        /// <summary>
        /// Check if tax group can be deleted
        /// التحقق من إمكانية حذف مجموعة الضريبة
        /// </summary>
        [HttpGet("{id}/can-delete")]
        public async Task<IActionResult> CanDelete(Guid id)
        {
            var canDelete = await _taxGroupService.CanDeleteAsync(id);
            return SUCCESS(new { Id = id, CanDelete = canDelete });
        }

        #endregion
    }
}
