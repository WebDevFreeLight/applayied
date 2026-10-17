using System;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Model.Finance.Zatca.Dto
{
    /// <summary>
    /// ZATCA Device Query DTO - استعلام جهاز زاتكا
    /// </summary>
    public class ZatcaDeviceQueryDto
    {
        public string DeviceSerialNumber { get; set; }
        public string DeviceName { get; set; }
        public string OrganizationIdentifier { get; set; }
        public ZatcaEnvironment? Environment { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsComplianceCompleted { get; set; }
        public bool? IsProductionReady { get; set; }
    }

    /// <summary>
    /// ZATCA Device List DTO - قائمة أجهزة زاتكا
    /// </summary>
    public class ZatcaDeviceListDto
    {
        public Guid Id { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceName { get; set; }
        public string OrganizationIdentifier { get; set; }
        public string OrganizationUnitName { get; set; }
        public string OrganizationNameEn { get; set; }
        public string OrganizationNameAr { get; set; }
        public string InvoiceType { get; set; }
        public ZatcaEnvironment Environment { get; set; }
        public bool IsActive { get; set; }
        public bool IsComplianceCompleted { get; set; }
        public bool IsProductionReady { get; set; }
        public DateTime? CertificateExpiryDate { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime Create_time { get; set; }
    }

    /// <summary>
    /// ZATCA Device Detail DTO - تفاصيل جهاز زاتكا
    /// </summary>
    public class ZatcaDeviceDetailDto : ZatcaDeviceListDto
    {
        public string CountryCode { get; set; }
        public string LocationAddress { get; set; }
        public string IndustryBusinessCategory { get; set; }
        public bool HasCsr { get; set; }
        public bool HasComplianceCertificate { get; set; }
        public bool HasProductionCertificate { get; set; }
        public DateTime? ComplianceCompletedAt { get; set; }
        public DateTime? ProductionActivatedAt { get; set; }
    }

    /// <summary>
    /// Create ZATCA Device DTO - إنشاء جهاز زاتكا
    /// </summary>
    public class CreateZatcaDeviceDto
    {
        /// <summary>
        /// Device Serial Number - الرقم التسلسلي للجهاز
        /// </summary>
        public string DeviceSerialNumber { get; set; }

        /// <summary>
        /// Device Name - اسم الجهاز
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Organization Identifier (VAT Number) - معرف المنشأة (الرقم الضريبي)
        /// 15 digits
        /// </summary>
        public string OrganizationIdentifier { get; set; }

        /// <summary>
        /// Organization Unit Name - اسم الوحدة التنظيمية (10 digits)
        /// </summary>
        public string OrganizationUnitName { get; set; }

        /// <summary>
        /// Organization Name (English) - اسم المنشأة بالإنجليزية
        /// </summary>
        public string OrganizationNameEn { get; set; }

        /// <summary>
        /// Organization Name (Arabic) - اسم المنشأة بالعربية
        /// </summary>
        public string OrganizationNameAr { get; set; }

        /// <summary>
        /// Country Code - رمز الدولة (SA)
        /// </summary>
        public string CountryCode { get; set; } = "SA";

        /// <summary>
        /// Invoice Type - نوع الفاتورة
        /// Binary flags: 1000 = Standard, 0100 = Simplified, 1100 = Both
        /// </summary>
        public string InvoiceType { get; set; } = "1100";

        /// <summary>
        /// Location Address - عنوان الموقع
        /// </summary>
        public string LocationAddress { get; set; }

        /// <summary>
        /// Industry/Business Category - قطاع الأعمال
        /// </summary>
        public string IndustryBusinessCategory { get; set; }

        /// <summary>
        /// Environment - البيئة
        /// </summary>
        public ZatcaEnvironment Environment { get; set; } = ZatcaEnvironment.Sandbox;
    }

    /// <summary>
    /// Update ZATCA Device DTO - تحديث جهاز زاتكا
    /// </summary>
    public class UpdateZatcaDeviceDto : CreateZatcaDeviceDto
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Generate CSR Request DTO - طلب إنشاء CSR
    /// </summary>
    public class GenerateCsrRequestDto
    {
        public Guid DeviceId { get; set; }
    }

    /// <summary>
    /// Generate CSR Response DTO - استجابة إنشاء CSR
    /// </summary>
    public class GenerateCsrResponseDto
    {
        public Guid DeviceId { get; set; }
        public string Csr { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Onboarding Request DTO - طلب التسجيل
    /// </summary>
    public class OnboardingRequestDto
    {
        public Guid DeviceId { get; set; }
        public string Otp { get; set; }
    }

    /// <summary>
    /// Onboarding Response DTO - استجابة التسجيل
    /// </summary>
    public class OnboardingResponseDto
    {
        public Guid DeviceId { get; set; }
        public string ComplianceCsid { get; set; }
        public DateTime? CertificateExpiryDate { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string[] Warnings { get; set; }
        public string[] Errors { get; set; }
    }

    /// <summary>
    /// Production CSID Request DTO - طلب معرف الإنتاج
    /// </summary>
    public class ProductionCsidRequestDto
    {
        public Guid DeviceId { get; set; }
        public string ComplianceRequestId { get; set; }
    }

    /// <summary>
    /// Production CSID Response DTO - استجابة معرف الإنتاج
    /// </summary>
    public class ProductionCsidResponseDto
    {
        public Guid DeviceId { get; set; }
        public string ProductionCsid { get; set; }
        public DateTime? CertificateExpiryDate { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string[] Warnings { get; set; }
        public string[] Errors { get; set; }
    }
}
