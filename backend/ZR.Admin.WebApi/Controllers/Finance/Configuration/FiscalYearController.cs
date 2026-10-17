using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Admin.WebApi.Controllers.Finance.Configuration
{
    /// <summary>
    /// Fiscal Year Controller - متحكم السنوات المالية
    /// </summary>
    [Verify]
    [Route("finance/configuration/fiscal-years")]
    [ApiController]
    public class FiscalYearController : BaseController
    {
        private readonly IFiscalYearService _fiscalYearService;

        public FiscalYearController(IFiscalYearService fiscalYearService)
        {
            _fiscalYearService = fiscalYearService;
        }

        /// <summary>
        /// Get list of fiscal years - الحصول على قائمة السنوات المالية
        /// </summary>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "finance:configuration:fiscal-year:list")]
        public IActionResult GetList([FromQuery] FiscalYearQueryDto query, [FromQuery] PagerInfo pager)
        {
            return SUCCESS(_fiscalYearService.GetList(query, pager));
        }

        /// <summary>
        /// Get fiscal year by ID - الحصول على السنة المالية بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        [ActionPermissionFilter(Permission = "finance:configuration:fiscal-year:query")]
        public IActionResult GetInfo(Guid id)
        {
            var response = _fiscalYearService.GetById(id);
            if (response == null)
                return ToResponse(ResultCode.FAIL, "Fiscal year not found - السنة المالية غير موجودة");
            return SUCCESS(response);
        }

        /// <summary>
        /// Create fiscal year - إنشاء سنة مالية
        /// </summary>
        [HttpPost]
        [ActionPermissionFilter(Permission = "finance:configuration:fiscal-year:add")]
        [Log(Title = "Fiscal Year", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateFiscalYearDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _fiscalYearService.Create(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update fiscal year - تحديث سنة مالية
        /// </summary>
        [HttpPut]
        [ActionPermissionFilter(Permission = "finance:configuration:fiscal-year:edit")]
        [Log(Title = "Fiscal Year", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateFiscalYearDto dto)
        {
            if (!ModelState.IsValid)
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid parameters - معاملات غير صالحة");

            try
            {
                var result = _fiscalYearService.Update(dto, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete fiscal years - حذف سنوات مالية
        /// </summary>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "finance:configuration:fiscal-year:delete")]
        [Log(Title = "Fiscal Year", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = Array.ConvertAll(ids.Split(','), Guid.Parse);
            var result = _fiscalYearService.Delete(idArray);
            return SUCCESS(result);
        }

        /// <summary>
        /// Open fiscal year - فتح سنة مالية
        /// </summary>
        [HttpPost("{id}/open")]
        [ActionPermissionFilter(Permission = "finance:configuration:fiscal-year:edit")]
        [Log(Title = "Open Fiscal Year", BusinessType = BusinessType.UPDATE)]
        public IActionResult OpenFiscalYear(Guid id)
        {
            try
            {
                var result = _fiscalYearService.OpenFiscalYear(id, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Close fiscal year - إغلاق سنة مالية
        /// </summary>
        [HttpPost("{id}/close")]
        [ActionPermissionFilter(Permission = "finance:configuration:fiscal-year:edit")]
        [Log(Title = "Close Fiscal Year", BusinessType = BusinessType.UPDATE)]
        public IActionResult CloseFiscalYear(Guid id)
        {
            try
            {
                var result = _fiscalYearService.CloseFiscalYear(id, HttpContext.GetName());
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }
    }
}


