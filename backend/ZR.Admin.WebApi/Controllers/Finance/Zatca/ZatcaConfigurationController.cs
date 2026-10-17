using Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model;
using ZR.Model.Finance.Zatca.Dto;
using ZR.Model.Finance.Zatca.Enums;
using ZR.Service.IService.Finance.Zatca;

namespace ZR.Admin.WebApi.Controllers.Finance.Zatca
{
    /// <summary>
    /// ZATCA Configuration Controller
    /// تحكم إعدادات زاتكا
    /// </summary>
    [Verify]
    [Route("api/erp/finance/zatca/configuration")]
    [ApiController]
    [Authorize]
    public class ZatcaConfigurationController : BaseController
    {
        private readonly IZatcaConfigurationService _configurationService;

        public ZatcaConfigurationController(IZatcaConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        /// <summary>
        /// Get ZATCA configuration
        /// الحصول على إعدادات زاتكا
        /// </summary>
        [HttpGet]
        public IActionResult GetConfiguration()
        {
            var config = _configurationService.GetConfiguration();
            return SUCCESS(config);
        }

        /// <summary>
        /// Save ZATCA configuration
        /// حفظ إعدادات زاتكا
        /// </summary>
        [HttpPost]
        [Log(Title = "ZATCA Configuration", BusinessType = BusinessType.UPDATE)]
        public IActionResult SaveConfiguration([FromBody] SaveZatcaConfigurationDto dto)
        {
            var result = _configurationService.SaveConfiguration(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Validate ZATCA configuration
        /// التحقق من إعدادات زاتكا
        /// </summary>
        [HttpGet("validate")]
        public IActionResult ValidateConfiguration()
        {
            var result = _configurationService.ValidateConfiguration();
            return SUCCESS(result);
        }

        /// <summary>
        /// Set active device
        /// تعيين الجهاز النشط
        /// </summary>
        [HttpPost("device/{deviceId}")]
        [Log(Title = "ZATCA Configuration", BusinessType = BusinessType.UPDATE)]
        public IActionResult SetActiveDevice(Guid deviceId)
        {
            var result = _configurationService.SetActiveDevice(null, deviceId);
            return ToResponse(result);
        }

        /// <summary>
        /// Change environment
        /// تغيير البيئة
        /// </summary>
        [HttpPost("environment/{environment}")]
        [Log(Title = "ZATCA Configuration", BusinessType = BusinessType.UPDATE)]
        public IActionResult ChangeEnvironment(ZatcaEnvironment environment)
        {
            var result = _configurationService.ChangeEnvironment(null, environment);
            return ToResponse(result);
        }

        /// <summary>
        /// Test API connection
        /// اختبار الاتصال بـ API
        /// </summary>
        [HttpPost("test-connection")]
        public IActionResult TestApiConnection([FromBody] ZatcaApiTestRequestDto request)
        {
            var result = _configurationService.TestApiConnection(request);
            return SUCCESS(result);
        }

        /// <summary>
        /// Check if ZATCA is enabled
        /// التحقق من تفعيل زاتكا
        /// </summary>
        [HttpGet("is-enabled")]
        public IActionResult IsZatcaEnabled()
        {
            var result = _configurationService.IsZatcaEnabled();
            return SUCCESS(new { IsEnabled = result });
        }
    }
}
