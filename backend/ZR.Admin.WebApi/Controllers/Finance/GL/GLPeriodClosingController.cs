using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Model.Finance.Exceptions;
using ZR.Service.IService.Finance.GL;

namespace ZR.Admin.WebApi.Controllers.Finance.GL
{
    /// <summary>
    /// GL Period Closing Controller - إقفال الفترات المحاسبية
    /// </summary>
    [Verify]
    [Route("api/erp/finance/period")]
    [ApiController]
    public class GLPeriodClosingController : BaseController
    {
        private readonly IGLPeriodClosingService _periodService;

        public GLPeriodClosingController(IGLPeriodClosingService periodService)
        {
            _periodService = periodService;
        }

        /// <summary>
        /// Get paginated list of periods - الحصول على قائمة الفترات
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "erp:finance:period:list")]
        public IActionResult GetList([FromQuery] GLPeriodClosingQueryDto query, [FromQuery] PagerInfo pager)
        {
            var result = _periodService.GetList(query, pager);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get period by ID - الحصول على الفترة بالمعرف
        /// </summary>
        [HttpGet("get/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public IActionResult GetById(Guid id)
        {
            var result = _periodService.GetById(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Period not found - الفترة غير موجودة");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Get period by ID async - الحصول على الفترة بالمعرف (غير متزامن)
        /// </summary>
        [HttpGet("get-async/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await _periodService.GetByIdAsync(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Period not found - الفترة غير موجودة");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Get periods by fiscal year - الحصول على الفترات بالسنة المالية
        /// </summary>
        [HttpGet("by-fiscal-year/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public IActionResult GetByFiscalYear(int fiscalYear, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _periodService.GetByFiscalYear(fiscalYear, actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get open periods - الحصول على الفترات المفتوحة
        /// </summary>
        [HttpGet("open")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public IActionResult GetOpenPeriods([FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _periodService.GetOpenPeriods(actualCompanyId);
            return SUCCESS(result);
        }

        /// <summary>
        /// Create period - إنشاء فترة
        /// </summary>
        [HttpPost("create")]
        [ActionPermissionFilter(Permission = "erp:finance:period:add")]
        [Log(Title = "GL Period Closing", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreatePeriodClosingDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = _periodService.Create(dto, userName);
            return SUCCESS(result, "Period created successfully - تم إنشاء الفترة بنجاح");
        }

        /// <summary>
        /// Create period async - إنشاء فترة (غير متزامن)
        /// </summary>
        [HttpPost("create-async")]
        [ActionPermissionFilter(Permission = "erp:finance:period:add")]
        [Log(Title = "GL Period Closing", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePeriodClosingDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = await _periodService.CreateAsync(dto, userName);
            return SUCCESS(result, "Period created successfully - تم إنشاء الفترة بنجاح");
        }

        /// <summary>
        /// Get period status - الحصول على حالة الفترة
        /// </summary>
        [HttpGet("status/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public IActionResult GetPeriodStatus(Guid id)
        {
            var result = _periodService.GetPeriodStatus(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Period not found - الفترة غير موجودة");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Get period status async - الحصول على حالة الفترة (غير متزامن)
        /// </summary>
        [HttpGet("status-async/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public async Task<IActionResult> GetPeriodStatusAsync(Guid id)
        {
            var result = await _periodService.GetPeriodStatusAsync(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Period not found - الفترة غير موجودة");
            }
            return SUCCESS(result);
        }

        /// <summary>
        /// Validate period for closing - التحقق من الفترة للإقفال
        /// </summary>
        [HttpGet("validate/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public IActionResult ValidateForClosing(Guid id)
        {
            var (canClose, message) = _periodService.ValidateForClosing(id);
            return SUCCESS(new { CanClose = canClose, Message = message });
        }

        /// <summary>
        /// Validate period for closing async - التحقق من الفترة للإقفال (غير متزامن)
        /// </summary>
        [HttpGet("validate-async/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public async Task<IActionResult> ValidateForClosingAsync(Guid id)
        {
            var (canClose, message) = await _periodService.ValidateForClosingAsync(id);
            return SUCCESS(new { CanClose = canClose, Message = message });
        }

        /// <summary>
        /// Close period - إقفال الفترة
        /// </summary>
        [HttpPost("close")]
        [ActionPermissionFilter(Permission = "erp:finance:period:close")]
        [Log(Title = "GL Period Closing - Close", BusinessType = BusinessType.UPDATE)]
        public IActionResult ClosePeriod([FromBody] ClosePeriodDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            // Validate before closing
            var (canClose, message) = _periodService.ValidateForClosing(dto.Id);
            if (!canClose)
            {
                return ToResponse(ResultCode.FAIL, message);
            }

            var userName = HttpContext.GetName();
            var result = _periodService.ClosePeriod(dto, userName);
            if (result > 0)
            {
                return SUCCESS("Period closed successfully - تم إقفال الفترة بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to close period - فشل إقفال الفترة");
        }

        /// <summary>
        /// Close period async - إقفال الفترة (غير متزامن)
        /// </summary>
        [HttpPost("close-async")]
        [ActionPermissionFilter(Permission = "erp:finance:period:close")]
        [Log(Title = "GL Period Closing - Close", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ClosePeriodAsync([FromBody] ClosePeriodDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            // Validate before closing
            var (canClose, message) = await _periodService.ValidateForClosingAsync(dto.Id);
            if (!canClose)
            {
                return ToResponse(ResultCode.FAIL, message);
            }

            var userName = HttpContext.GetName();
            var result = await _periodService.ClosePeriodAsync(dto, userName);
            if (result > 0)
            {
                return SUCCESS("Period closed successfully - تم إقفال الفترة بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to close period - فشل إقفال الفترة");
        }

        /// <summary>
        /// Reopen period - إعادة فتح الفترة
        /// </summary>
        [HttpPost("reopen")]
        [ActionPermissionFilter(Permission = "erp:finance:period:reopen")]
        [Log(Title = "GL Period Closing - Reopen", BusinessType = BusinessType.UPDATE)]
        public IActionResult ReopenPeriod([FromBody] ReopenPeriodDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = _periodService.ReopenPeriod(dto, userName);
            if (result > 0)
            {
                return SUCCESS("Period reopened successfully - تم إعادة فتح الفترة بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to reopen period - فشل إعادة فتح الفترة");
        }

        /// <summary>
        /// Reopen period async - إعادة فتح الفترة (غير متزامن)
        /// </summary>
        [HttpPost("reopen-async")]
        [ActionPermissionFilter(Permission = "erp:finance:period:reopen")]
        [Log(Title = "GL Period Closing - Reopen", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> ReopenPeriodAsync([FromBody] ReopenPeriodDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request - طلب غير صالح");
            }

            var userName = HttpContext.GetName();
            var result = await _periodService.ReopenPeriodAsync(dto, userName);
            if (result > 0)
            {
                return SUCCESS("Period reopened successfully - تم إعادة فتح الفترة بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to reopen period - فشل إعادة فتح الفترة");
        }

        /// <summary>
        /// Lock period (permanent) - قفل الفترة نهائياً
        /// </summary>
        [HttpPost("lock/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:lock")]
        [Log(Title = "GL Period Closing - Lock", BusinessType = BusinessType.UPDATE)]
        public IActionResult LockPeriod(Guid id)
        {
            var userName = HttpContext.GetName();
            var result = _periodService.LockPeriod(id, userName);
            if (result > 0)
            {
                return SUCCESS("Period locked successfully - تم قفل الفترة نهائياً بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to lock period - فشل قفل الفترة");
        }

        /// <summary>
        /// Lock period async (permanent) - قفل الفترة نهائياً (غير متزامن)
        /// </summary>
        [HttpPost("lock-async/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:lock")]
        [Log(Title = "GL Period Closing - Lock", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> LockPeriodAsync(Guid id)
        {
            var userName = HttpContext.GetName();
            var result = await _periodService.LockPeriodAsync(id, userName);
            if (result > 0)
            {
                return SUCCESS("Period locked successfully - تم قفل الفترة نهائياً بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to lock period - فشل قفل الفترة");
        }

        /// <summary>
        /// Check if date is in closed period - التحقق إذا التاريخ في فترة مغلقة
        /// </summary>
        [HttpGet("check-date")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public IActionResult IsDateInClosedPeriod([FromQuery] DateTime date, [FromQuery] long? companyId = null)
        {
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var isClosed = _periodService.IsDateInClosedPeriod(date, actualCompanyId);
            return SUCCESS(new { Date = date, IsClosed = isClosed });
        }

        /// <summary>
        /// Validate if date can accept transactions - التحقق من إمكانية إدخال معاملات في التاريخ
        /// Returns structured validation result with error codes
        /// </summary>
        [HttpGet("validate-date")]
        [ActionPermissionFilter(Permission = "erp:finance:period:query")]
        public IActionResult ValidateDate([FromQuery] DateTime date, [FromQuery] long? companyId = null, [FromQuery] bool allowDrafts = false)
        {
            try
            {
                var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
                // This will throw if period is closed/locked/not found
                _periodService.CheckIfDateInClosedOrLockedPeriod(date, actualCompanyId, allowDrafts);

                // If no exception, date is valid
                return SUCCESS(new
                {
                    isValid = true,
                    date,
                    message = "Date is valid for transactions - التاريخ صالح للمعاملات"
                });
            }
            catch (PeriodNotFoundException ex)
            {
                return SUCCESS(new
                {
                    isValid = false,
                    errorCode = ex.ErrorCode,
                    messageAr = ex.MessageAr,
                    messageEn = ex.MessageEn,
                    date = ex.Date
                });
            }
            catch (PeriodClosedException ex)
            {
                return SUCCESS(new
                {
                    isValid = false,
                    errorCode = ex.ErrorCode,
                    messageAr = ex.MessageAr,
                    messageEn = ex.MessageEn,
                    date = ex.Date
                });
            }
            catch (PeriodLockedException ex)
            {
                return SUCCESS(new
                {
                    isValid = false,
                    errorCode = ex.ErrorCode,
                    messageAr = ex.MessageAr,
                    messageEn = ex.MessageEn,
                    date = ex.Date
                });
            }
        }

        /// <summary>
        /// Generate fiscal year periods - توليد فترات السنة المالية
        /// </summary>
        [HttpPost("generate/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:generate")]
        [Log(Title = "GL Period Closing - Generate", BusinessType = BusinessType.INSERT)]
        public IActionResult GenerateFiscalYearPeriods(int fiscalYear, [FromQuery] PeriodType periodType = PeriodType.Month, [FromQuery] long? companyId = null)
        {
            var userName = HttpContext.GetName();
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = _periodService.GenerateFiscalYearPeriods(fiscalYear, periodType, userName, actualCompanyId);
            return SUCCESS(result, $"Generated {result.Count} periods for fiscal year {fiscalYear} - تم توليد {result.Count} فترة للسنة المالية {fiscalYear}");
        }

        /// <summary>
        /// Generate fiscal year periods async - توليد فترات السنة المالية (غير متزامن)
        /// </summary>
        [HttpPost("generate-async/{fiscalYear}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:generate")]
        [Log(Title = "GL Period Closing - Generate", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> GenerateFiscalYearPeriodsAsync(int fiscalYear, [FromQuery] PeriodType periodType = PeriodType.Month, [FromQuery] long? companyId = null)
        {
            var userName = HttpContext.GetName();
            var actualCompanyId = companyId ?? (long)HttpContext.GetCompanyId();
            var result = await _periodService.GenerateFiscalYearPeriodsAsync(fiscalYear, periodType, userName, actualCompanyId);
            return SUCCESS(result, $"Generated {result.Count} periods for fiscal year {fiscalYear} - تم توليد {result.Count} فترة للسنة المالية {fiscalYear}");
        }

        /// <summary>
        /// Delete period - حذف الفترة
        /// </summary>
        [HttpDelete("delete/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:delete")]
        [Log(Title = "GL Period Closing", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid id)
        {
            var result = _periodService.Delete(id);
            if (result > 0)
            {
                return SUCCESS("Period deleted successfully - تم حذف الفترة بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to delete period - فشل حذف الفترة");
        }

        /// <summary>
        /// Delete period async - حذف الفترة (غير متزامن)
        /// </summary>
        [HttpDelete("delete-async/{id}")]
        [ActionPermissionFilter(Permission = "erp:finance:period:delete")]
        [Log(Title = "GL Period Closing", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await _periodService.DeleteAsync(id);
            if (result > 0)
            {
                return SUCCESS("Period deleted successfully - تم حذف الفترة بنجاح");
            }
            return ToResponse(ResultCode.FAIL, "Failed to delete period - فشل حذف الفترة");
        }
    }
}

