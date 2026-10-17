using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.GL.Dto;
using ZR.Service.Finance.GL;

namespace ZR.Admin.WebApi.Controllers.Finance.GL
{
    /// <summary>
    /// GL Account Opening Balance Controller - متحكم الأرصدة الافتتاحية
    /// Single source of truth for ALL opening balances in the system
    /// </summary>
    [Verify]
    [Route("finance/opening-balances")]
    [ApiController]
    public class GLAccountOpeningBalanceController : BaseController
    {
        private readonly GLAccountOpeningBalanceService _service;

        public GLAccountOpeningBalanceController(GLAccountOpeningBalanceService service)
        {
            _service = service;
        }

        #region Query Operations

        /// <summary>
        /// Get list of opening balances - الحصول على قائمة الأرصدة الافتتاحية
        /// </summary>
        /// <param name="query">Query filters</param>
        /// <param name="pager">Pagination parameters</param>
        /// <returns>Paginated list of opening balances</returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:openingbalance:list")]
        public IActionResult GetList([FromQuery] GLAccountOpeningBalanceQueryDto query, [FromQuery] PagerInfo pager)
        {
            var result = _service.GetList(query, pager);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get opening balance detail by ID - الحصول على تفاصيل الرصيد الافتتاحي
        /// </summary>
        /// <param name="id">Opening balance ID</param>
        /// <returns>Opening balance details</returns>
        [HttpGet("{id:guid}")]
        [ActionPermissionFilter(Permission = "finance:openingbalance:query")]
        public IActionResult GetDetail(Guid id)
        {
            try
            {
                var result = _service.GetDetail(id);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Get opening balance by account and fiscal year - الحصول على الرصيد الافتتاحي للحساب والسنة المالية
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <returns>Opening balance or null</returns>
        [HttpGet("account/{accountId:guid}/year/{fiscalYear:int}")]
        [ActionPermissionFilter(Permission = "finance:openingbalance:query")]
        public IActionResult GetByAccountAndYear(Guid accountId, int fiscalYear)
        {
            var result = _service.GetByAccountAndYear(accountId, fiscalYear);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Opening balance not found - الرصيد الافتتاحي غير موجود");

            return SUCCESS(result);
        }

        #endregion

        #region Create/Update/Delete Operations

        /// <summary>
        /// Create new opening balance - إنشاء رصيد افتتاحي جديد
        /// </summary>
        /// <param name="dto">Opening balance data</param>
        /// <returns>Success/failure result</returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:openingbalance:add")]
        [Log(Title = "Create Opening Balance", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateGLAccountOpeningBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ApiResult.Error(101, "Validation failed - فشل التحقق"));

            try
            {
                var userId = HttpContext.GetUId().ToString();
                var result = _service.Create(dto, userId);

                if (result > 0)
                    return SUCCESS(new { message = "Opening balance created successfully - تم إنشاء الرصيد الافتتاحي بنجاح" });

                return ToResponse(ResultCode.FAIL, "Failed to create opening balance - فشل في إنشاء الرصيد الافتتاحي");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update existing opening balance - تحديث الرصيد الافتتاحي
        /// NOTE: Can only update if not locked
        /// </summary>
        /// <param name="dto">Updated opening balance data</param>
        /// <returns>Success/failure result</returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:openingbalance:edit")]
        [Log(Title = "Update Opening Balance", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateGLAccountOpeningBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ApiResult.Error(101, "Validation failed - فشل التحقق"));

            try
            {
                var userId = HttpContext.GetUId().ToString();
                var result = _service.Update(dto, userId);

                if (result > 0)
                    return SUCCESS(new { message = "Opening balance updated successfully - تم تحديث الرصيد الافتتاحي بنجاح" });

                return ToResponse(ResultCode.FAIL, "Failed to update opening balance - فشل في تحديث الرصيد الافتتاحي");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete opening balance - حذف الرصيد الافتتاحي
        /// NOTE: Can only delete if not locked
        /// </summary>
        /// <param name="id">Opening balance ID</param>
        /// <returns>Success/failure result</returns>
        [HttpDelete("{id:guid}")]
        [ActionPermissionFilter(Permission = "finance:openingbalance:delete")]
        [Log(Title = "Delete Opening Balance", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid id)
        {
            try
            {
                var result = _service.Delete(id);

                if (result > 0)
                    return SUCCESS(new { message = "Opening balance deleted successfully - تم حذف الرصيد الافتتاحي بنجاح" });

                return ToResponse(ResultCode.FAIL, "Failed to delete opening balance - فشل في حذف الرصيد الافتتاحي");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion

        #region Lock/Unlock Operations

        /// <summary>
        /// Lock opening balance to prevent modifications - قفل الرصيد الافتتاحي لمنع التعديلات
        /// </summary>
        /// <param name="id">Opening balance ID</param>
        /// <returns>Success/failure result</returns>
        [HttpPost("{id:guid}/lock")]
        [ActionPermissionFilter(Permission = "finance:openingbalance:lock")]
        [Log(Title = "Lock Opening Balance", BusinessType = BusinessType.UPDATE)]
        public IActionResult Lock(Guid id)
        {
            try
            {
                var userId = HttpContext.GetUId().ToString();
                var result = _service.Lock(id, userId);

                if (result > 0)
                    return SUCCESS(new { message = "Opening balance locked successfully - تم قفل الرصيد الافتتاحي بنجاح" });

                return ToResponse(ResultCode.FAIL, "Failed to lock opening balance - فشل في قفل الرصيد الافتتاحي");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Unlock opening balance to allow modifications - فتح قفل الرصيد الافتتاحي للسماح بالتعديلات
        /// </summary>
        /// <param name="id">Opening balance ID</param>
        /// <returns>Success/failure result</returns>
        [HttpPost("{id:guid}/unlock")]
        [ActionPermissionFilter(Permission = "finance:openingbalance:unlock")]
        [Log(Title = "Unlock Opening Balance", BusinessType = BusinessType.UPDATE)]
        public IActionResult Unlock(Guid id)
        {
            try
            {
                var userId = HttpContext.GetUId().ToString();
                var result = _service.Unlock(id, userId);

                if (result > 0)
                    return SUCCESS(new { message = "Opening balance unlocked successfully - تم فتح قفل الرصيد الافتتاحي بنجاح" });

                return ToResponse(ResultCode.FAIL, "Failed to unlock opening balance - فشل في فتح قفل الرصيد الافتتاحي");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Validate opening balances for fiscal year - التحقق من الأرصدة الافتتاحية للسنة المالية
        /// Ensures that Total Debit = Total Credit
        /// </summary>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <param name="companyId">Company ID (optional)</param>
        /// <returns>Validation result with totals and any errors</returns>
        [HttpGet("validate/year/{fiscalYear:int}")]
        [ActionPermissionFilter(Permission = "finance:openingbalance:validate")]
        public IActionResult ValidateBalanceForFiscalYear(int fiscalYear, [FromQuery] long? companyId = null)
        {
            var result = _service.ValidateBalanceForFiscalYear(fiscalYear, companyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Check if account has opening balance for fiscal year
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <returns>True if opening balance exists</returns>
        [HttpGet("exists/account/{accountId:guid}/year/{fiscalYear:int}")]
        [ActionPermissionFilter(Permission = "finance:openingbalance:query")]
        public IActionResult HasOpeningBalance(Guid accountId, int fiscalYear)
        {
            var result = _service.HasOpeningBalance(accountId, fiscalYear);
            return SUCCESS(new { exists = result });
        }

        #endregion
    }
}
