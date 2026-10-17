using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Exceptions;
using Infrastructure.Enums;
using Infrastructure.Model;
using Microsoft.AspNetCore.Mvc;
using ZR.Model;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Admin.WebApi.Controllers.Finance.Configuration
{
    /// <summary>
    /// Exchange Rate Controller
    /// وحدة التحكم في أسعار الصرف
    /// </summary>
    [Route("api/finance/exchange-rates")]
    [ApiController]
    public class ExchangeRateController : BaseController
    {
        private readonly IExchangeRateService _exchangeRateService;

        public ExchangeRateController(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService;
        }

        #region Query Endpoints

        /// <summary>
        /// Get paginated list of exchange rates
        /// الحصول على قائمة أسعار الصرف بالترقيم
        /// </summary>
        /// <param name="query">Query parameters</param>
        /// <returns>Paginated list of exchange rates</returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:list")]
        public IActionResult GetList([FromQuery] ExchangeRateQueryDto query)
        {
            var pager = new PagerInfo
            {
                PageNum = query.PageNum,
                PageSize = query.PageSize
            };

            var result = _exchangeRateService.GetList(query, pager);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get exchange rate by ID
        /// الحصول على سعر الصرف بالمعرف
        /// </summary>
        /// <param name="id">Exchange rate ID</param>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:query")]
        public IActionResult GetById(Guid id)
        {
            var result = _exchangeRateService.GetById(id);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "Exchange rate not found");
            }

            return SUCCESS(result);
        }

        /// <summary>
        /// Get exchange rate for specific date
        /// الحصول على سعر الصرف لتاريخ محدد
        /// </summary>
        /// <param name="currencyId">Currency ID</param>
        /// <param name="date">Date</param>
        [HttpGet("by-date")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:query")]
        public IActionResult GetRateForDate([FromQuery] Guid currencyId, [FromQuery] DateTime date)
        {
            var result = _exchangeRateService.GetRateForDate(currencyId, date);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, $"No exchange rate found for currency on or before {date:yyyy-MM-dd}");
            }

            return SUCCESS(result);
        }

        /// <summary>
        /// Get exchange rate by currency code and date
        /// الحصول على سعر الصرف برمز العملة والتاريخ
        /// </summary>
        /// <param name="currencyCode">Currency code (ISO 4217)</param>
        /// <param name="date">Date</param>
        [HttpGet("by-code")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:query")]
        public IActionResult GetRateByCodeAndDate([FromQuery] string currencyCode, [FromQuery] DateTime date)
        {
            if (string.IsNullOrEmpty(currencyCode))
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Currency code is required");
            }

            var result = _exchangeRateService.GetRateByCodeAndDate(currencyCode, date);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, $"No exchange rate found for {currencyCode} on or before {date:yyyy-MM-dd}");
            }

            return SUCCESS(result);
        }

        /// <summary>
        /// Get latest exchange rate for currency
        /// الحصول على أحدث سعر صرف للعملة
        /// </summary>
        /// <param name="currencyId">Currency ID</param>
        [HttpGet("latest/{currencyId}")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:query")]
        public IActionResult GetLatestRate(Guid currencyId)
        {
            var result = _exchangeRateService.GetLatestRate(currencyId);
            if (result == null)
            {
                return ToResponse(ResultCode.FAIL, "No exchange rate found for currency");
            }

            return SUCCESS(result);
        }

        /// <summary>
        /// Get all rates for a specific date
        /// الحصول على جميع الأسعار لتاريخ محدد
        /// </summary>
        /// <param name="date">Date</param>
        [HttpGet("date/{date}")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:query")]
        public IActionResult GetRatesForDate(DateTime date)
        {
            var result = _exchangeRateService.GetRatesForDate(date);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get rate history for currency
        /// الحصول على تاريخ الأسعار للعملة
        /// </summary>
        /// <param name="currencyId">Currency ID</param>
        /// <param name="days">Number of days (default: 30)</param>
        [HttpGet("history/{currencyId}")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:query")]
        public IActionResult GetRateHistory(Guid currencyId, [FromQuery] int days = 30)
        {
            var result = _exchangeRateService.GetRateHistory(currencyId, days);
            return SUCCESS(result);
        }

        /// <summary>
        /// Find gaps in rate history
        /// البحث عن فجوات في تاريخ الأسعار
        /// </summary>
        /// <param name="currencyId">Currency ID</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        [HttpGet("gaps/{currencyId}")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:query")]
        public IActionResult FindRateGaps(Guid currencyId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = _exchangeRateService.FindRateGaps(currencyId, startDate, endDate);
            return SUCCESS(new
            {
                currencyId,
                startDate,
                endDate,
                missingDates = result,
                gapCount = result.Count
            });
        }

        #endregion

        #region Create/Update Endpoints

        /// <summary>
        /// Create exchange rate
        /// إنشاء سعر صرف
        /// </summary>
        /// <param name="dto">Exchange rate data</param>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:exchangerate:add")]
        [Log(Title = "Create Exchange Rate", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateExchangeRateDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Request body is required");
            }

            try
            {
                var userName = HttpContext.GetName();
                var result = _exchangeRateService.Create(dto, userName);
                return SUCCESS(result);
            }
            catch (ArgumentException ex)
            {
                return ToResponse(ResultCode.PARAM_ERROR, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update exchange rate
        /// تحديث سعر الصرف
        /// </summary>
        /// <param name="dto">Updated exchange rate data</param>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:exchangerate:edit")]
        [Log(Title = "Update Exchange Rate", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateExchangeRateDto dto)
        {
            if (dto == null || dto.Id == Guid.Empty)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid request");
            }

            try
            {
                var userName = HttpContext.GetName();
                var result = _exchangeRateService.Update(dto, userName);

                if (result > 0)
                {
                    return SUCCESS("Exchange rate updated successfully");
                }
                else
                {
                    return ToResponse(ResultCode.FAIL, "Failed to update exchange rate");
                }
            }
            catch (ArgumentException ex)
            {
                return ToResponse(ResultCode.PARAM_ERROR, ex.Message);
            }
        }

        /// <summary>
        /// Bulk import exchange rates
        /// استيراد مجموعة أسعار صرف
        /// </summary>
        /// <param name="dto">Bulk import data</param>
        [HttpPost("bulk-import")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:add")]
        [Log(Title = "Bulk Import Exchange Rates", BusinessType = BusinessType.INSERT)]
        public IActionResult BulkImport([FromBody] BulkImportExchangeRatesDto dto)
        {
            if (dto == null || dto.Rates == null || dto.Rates.Length == 0)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "No rates provided for import");
            }

            try
            {
                var userName = HttpContext.GetName();
                var result = _exchangeRateService.BulkImport(dto, userName);
                return SUCCESS(new
                {
                    message = $"Successfully imported {result.Count} exchange rates",
                    importedCount = result.Count,
                    rates = result
                });
            }
            catch (ArgumentException ex)
            {
                return ToResponse(ResultCode.PARAM_ERROR, ex.Message);
            }
        }

        #endregion

        #region Approval & Delete Endpoints

        /// <summary>
        /// Approve exchange rate
        /// الموافقة على سعر الصرف
        /// </summary>
        /// <param name="id">Exchange rate ID</param>
        [HttpPost("approve/{id}")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:approve")]
        [Log(Title = "Approve Exchange Rate", BusinessType = BusinessType.UPDATE)]
        public IActionResult Approve(Guid id)
        {
            if (id == Guid.Empty)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid exchange rate ID");
            }

            var userName = HttpContext.GetName();
            var result = _exchangeRateService.Approve(id, userName);

            if (result > 0)
            {
                return SUCCESS("Exchange rate approved successfully");
            }
            else
            {
                return ToResponse(ResultCode.FAIL, "Failed to approve exchange rate");
            }
        }

        /// <summary>
        /// Delete exchange rate
        /// حذف سعر الصرف
        /// </summary>
        /// <param name="id">Exchange rate ID</param>
        [HttpDelete("{id}")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:delete")]
        [Log(Title = "Delete Exchange Rate", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid exchange rate ID");
            }

            try
            {
                var result = _exchangeRateService.Delete(id);

                if (result > 0)
                {
                    return SUCCESS("Exchange rate deleted successfully");
                }
                else
                {
                    return ToResponse(ResultCode.FAIL, "Failed to delete exchange rate");
                }
            }
            catch (Exception ex) when (!(ex is AppException))
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        #endregion

        #region Validation Endpoints

        /// <summary>
        /// Validate exchange rate for journal
        /// التحقق من صحة سعر الصرف للقيد
        /// </summary>
        /// <param name="currencyId">Currency ID</param>
        /// <param name="journalDate">Journal date</param>
        [HttpGet("validate")]
        [ActionPermissionFilter(Permission = "finance:exchangerate:query")]
        public IActionResult ValidateRateForJournal([FromQuery] Guid currencyId, [FromQuery] DateTime journalDate)
        {
            if (currencyId == Guid.Empty)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Currency ID is required");
            }

            // Will throw AppException (with localized message) if invalid
            _exchangeRateService.ValidateRateForJournal(currencyId, journalDate);

            return SUCCESS(new
            {
                isValid = true,
                message = "Exchange rate is valid for journal"
            });
        }

        /// <summary>
        /// Calculate base amount from foreign amount
        /// حساب المبلغ بالعملة الأساسية
        /// </summary>
        /// <param name="foreignAmount">Foreign currency amount</param>
        /// <param name="exchangeRate">Exchange rate</param>
        [HttpGet("calculate-base")]
        public IActionResult CalculateBaseAmount([FromQuery] decimal foreignAmount, [FromQuery] decimal exchangeRate)
        {
            if (exchangeRate <= 0)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Exchange rate must be greater than 0");
            }

            var baseAmount = _exchangeRateService.CalculateBaseAmount(foreignAmount, exchangeRate);
            return SUCCESS(new
            {
                foreignAmount,
                exchangeRate,
                baseAmount
            });
        }

        /// <summary>
        /// Calculate foreign amount from base amount
        /// حساب المبلغ بالعملة الأجنبية
        /// </summary>
        /// <param name="baseAmount">Base currency amount</param>
        /// <param name="exchangeRate">Exchange rate</param>
        /// <param name="decimals">Decimal places (default: 2)</param>
        [HttpGet("calculate-foreign")]
        public IActionResult CalculateForeignAmount([FromQuery] decimal baseAmount, [FromQuery] decimal exchangeRate, [FromQuery] int decimals = 2)
        {
            if (exchangeRate <= 0)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Exchange rate must be greater than 0");
            }

            try
            {
                var foreignAmount = _exchangeRateService.CalculateForeignAmount(baseAmount, exchangeRate, decimals);
                return SUCCESS(new
                {
                    baseAmount,
                    exchangeRate,
                    decimals,
                    foreignAmount
                });
            }
            catch (Exception ex) when (!(ex is AppException))
            {
                return ToResponse(ResultCode.PARAM_ERROR, ex.Message);
            }
        }

        #endregion
    }
}
