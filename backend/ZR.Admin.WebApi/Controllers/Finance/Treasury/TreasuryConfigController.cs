using System;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Model;
using ZR.Model.Finance.Treasury.Dto;
using ZR.Service.IService.Finance.Treasury;

namespace ZR.Admin.WebApi.Controllers.Finance.Treasury
{
    /// <summary>
    /// Treasury Config Controller - تحكم إعدادات الخزينة
    /// </summary>
    [Verify]
    [Route("erp/finance/treasury/[controller]")]
    [ApiController]
    public class TreasuryConfigController : BaseController
    {
        private readonly ITreasuryConfigService _configService;

        public TreasuryConfigController(ITreasuryConfigService configService)
        {
            _configService = configService;
        }

        /// <summary>
        /// Get config list with paging - الحصول على قائمة الإعدادات مع ترقيم الصفحات
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] TreasuryConfigQueryDto parm)
        {
            var response = _configService.GetList(parm);
            return SUCCESS(response);
        }

        /// <summary>
        /// Get config by ID - الحصول على إعداد بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var response = _configService.GetDetail(id);
            if (response == null)
            {
                return ToResponse(ResultCode.FAIL, "Config not found - الإعداد غير موجود");
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Get config by tenant - الحصول على إعداد حسب المستأجر
        /// </summary>
        [HttpGet("tenant")]
        public IActionResult GetByTenant([FromQuery] long? tenantId = null)
        {
            var response = _configService.GetByTenant(tenantId);
            if (response == null)
            {
                // Return default config if not found
                return SUCCESS(new TreasuryConfigDetailDto
                {
                    AutoImportBankStatements = false,
                    AutoReconcile = false,
                    DefaultPaymentApprovalThreshold = 10000,
                    PettyCashLimit = 1000,
                    ApprovalWorkflowEnabled = true
                });
            }
            return SUCCESS(response);
        }

        /// <summary>
        /// Get or Create config for tenant - الحصول على أو إنشاء إعداد للمستأجر
        /// Returns existing config if found, creates default config if not
        /// Recommended endpoint for frontend to use instead of GetByTenant
        /// </summary>
        [HttpGet("get-or-create")]
        public IActionResult GetOrCreate([FromQuery] long? tenantId = null)
        {
            try
            {
                var userId = HttpContext.GetName();
                var response = _configService.GetOrCreate(tenantId, userId);
                return SUCCESS(response);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Create config - إنشاء إعداد
        /// </summary>
        [HttpPost]
        [Log(Title = "Create Treasury Config", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateTreasuryConfigDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var userId = HttpContext.GetName();
                var result = _configService.Create(dto, userId);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Update config - تحديث إعداد
        /// </summary>
        [HttpPut]
        [Log(Title = "Update Treasury Config", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateTreasuryConfigDto dto)
        {
            if (dto == null)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Invalid data - بيانات غير صالحة");
            }

            try
            {
                var userId = HttpContext.GetName();
                var result = _configService.Update(dto, userId);
                if (result > 0)
                {
                    return SUCCESS("Treasury configuration updated successfully - تم تحديث إعدادات الخزينة بنجاح");
                }
                return ToResponse(ResultCode.FAIL, "Failed to update treasury configuration - فشل تحديث إعدادات الخزينة");
            }
            catch (Exception ex)
            {
                return ToResponse(ResultCode.FAIL, ex.Message);
            }
        }

        /// <summary>
        /// Delete config - حذف إعداد
        /// </summary>
        [HttpDelete("{id}")]
        [Log(Title = "Delete Treasury Config", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(Guid id)
        {
            var result = _configService.Delete(new[] { id });
            return ToResponse(result);
        }
    }
}
