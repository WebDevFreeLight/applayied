using Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZR.Admin.WebApi.Filters;
using ZR.Common;
using ZR.Model;
using ZR.Model.Finance.Zatca.Dto;
using ZR.Service.IService.Finance.Zatca;

namespace ZR.Admin.WebApi.Controllers.Finance.Zatca
{
    /// <summary>
    /// ZATCA Device Controller
    /// تحكم أجهزة زاتكا
    /// </summary>
    [Verify]
    [Route("api/erp/finance/zatca/devices")]
    [ApiController]
    [Authorize]
    public class ZatcaDeviceController : BaseController
    {
        private readonly IZatcaDeviceService _deviceService;

        public ZatcaDeviceController(IZatcaDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        /// <summary>
        /// Get paginated list of devices
        /// الحصول على قائمة الأجهزة
        /// </summary>
        [HttpGet("list")]
        public IActionResult GetList([FromQuery] ZatcaDeviceQueryDto query, [FromQuery] PagerInfo pager)
        {
            var result = _deviceService.GetList(query, pager);
            return SUCCESS(result);
        }

        /// <summary>
        /// Get device by ID
        /// الحصول على جهاز بالمعرف
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var result = _deviceService.GetById(id);
            if (result == null)
                return ToResponse(ResultCode.FAIL, "Device not found - الجهاز غير موجود");
            return SUCCESS(result);
        }

        /// <summary>
        /// Create new device
        /// إنشاء جهاز جديد
        /// </summary>
        [HttpPost]
        [Log(Title = "ZATCA Device", BusinessType = BusinessType.INSERT)]
        public IActionResult Create([FromBody] CreateZatcaDeviceDto dto)
        {
            var result = _deviceService.Create(dto, HttpContext.GetName());
            return SUCCESS(result);
        }

        /// <summary>
        /// Update device
        /// تحديث جهاز
        /// </summary>
        [HttpPut]
        [Log(Title = "ZATCA Device", BusinessType = BusinessType.UPDATE)]
        public IActionResult Update([FromBody] UpdateZatcaDeviceDto dto)
        {
            var result = _deviceService.Update(dto, HttpContext.GetName());
            return ToResponse(result);
        }

        /// <summary>
        /// Delete devices
        /// حذف أجهزة
        /// </summary>
        [HttpDelete("{ids}")]
        [Log(Title = "ZATCA Device", BusinessType = BusinessType.DELETE)]
        public IActionResult Delete(string ids)
        {
            var idArray = ids.Split(',').Select(Guid.Parse).ToArray();
            var result = _deviceService.Delete(idArray);
            return ToResponse(result);
        }

        /// <summary>
        /// Get active devices for dropdown
        /// الحصول على الأجهزة النشطة
        /// </summary>
        [HttpGet("active")]
        public IActionResult GetActiveDevices()
        {
            var result = _deviceService.GetActiveDevices();
            return SUCCESS(result);
        }

        /// <summary>
        /// Get production-ready devices
        /// الحصول على الأجهزة الجاهزة للإنتاج
        /// </summary>
        [HttpGet("production-ready")]
        public IActionResult GetProductionReadyDevices()
        {
            var result = _deviceService.GetProductionReadyDevices();
            return SUCCESS(result);
        }

        /// <summary>
        /// Get devices with expiring certificates
        /// الحصول على الأجهزة ذات الشهادات المنتهية
        /// </summary>
        [HttpGet("expiring-certificates")]
        public IActionResult GetDevicesWithExpiringCertificates([FromQuery] int days = 30)
        {
            var result = _deviceService.GetDevicesWithExpiringCertificates(days);
            return SUCCESS(result);
        }

        /// <summary>
        /// Generate CSR for device
        /// إنشاء CSR للجهاز
        /// </summary>
        [HttpPost("{id}/generate-csr")]
        [Log(Title = "ZATCA Device CSR", BusinessType = BusinessType.UPDATE)]
        public IActionResult GenerateCsr(Guid id)
        {
            var result = _deviceService.GenerateCsr(id, HttpContext.GetName());
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        /// <summary>
        /// Request compliance CSID (onboarding)
        /// طلب معرف الامتثال (التسجيل)
        /// </summary>
        [HttpPost("compliance-csid")]
        [Log(Title = "ZATCA Compliance CSID", BusinessType = BusinessType.UPDATE)]
        public IActionResult RequestComplianceCsid([FromBody] OnboardingRequestDto request)
        {
            var result = _deviceService.RequestComplianceCsid(request, HttpContext.GetName());
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        /// <summary>
        /// Request production CSID
        /// طلب معرف الإنتاج
        /// </summary>
        [HttpPost("production-csid")]
        [Log(Title = "ZATCA Production CSID", BusinessType = BusinessType.UPDATE)]
        public IActionResult RequestProductionCsid([FromBody] ProductionCsidRequestDto request)
        {
            var result = _deviceService.RequestProductionCsid(request, HttpContext.GetName());
            if (!result.Success)
                return ToResponse(ResultCode.FAIL, result.Message);
            return SUCCESS(result);
        }

        /// <summary>
        /// Validate device for submission
        /// التحقق من صلاحية الجهاز للإرسال
        /// </summary>
        [HttpGet("{id}/validate")]
        public IActionResult ValidateDevice(Guid id)
        {
            var (isValid, errors) = _deviceService.ValidateDeviceForSubmission(id);
            return SUCCESS(new { IsValid = isValid, Errors = errors });
        }

        /// <summary>
        /// Check if serial number exists
        /// التحقق من وجود الرقم التسلسلي
        /// </summary>
        [HttpGet("check-serial/{serialNumber}")]
        public IActionResult CheckSerialNumber(string serialNumber, [FromQuery] Guid? excludeId = null)
        {
            var exists = _deviceService.SerialNumberExists(serialNumber, null, excludeId);
            return SUCCESS(new { Exists = exists });
        }
    }
}
