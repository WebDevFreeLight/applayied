using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Dto;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Service.IService.Finance.Zatca
{
    /// <summary>
    /// ZATCA Device Service Interface
    /// واجهة خدمة أجهزة زاتكا
    /// </summary>
    public interface IZatcaDeviceService
    {
        #region CRUD Operations

        /// <summary>
        /// Get paginated list of devices
        /// </summary>
        PagedInfo<ZatcaDeviceListDto> GetList(ZatcaDeviceQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get device by ID
        /// </summary>
        ZatcaDeviceDetailDto GetById(Guid id);

        /// <summary>
        /// Get device by ID (async)
        /// </summary>
        Task<ZatcaDeviceDetailDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Create new device
        /// </summary>
        ZatcaDevice Create(CreateZatcaDeviceDto dto, string userName);

        /// <summary>
        /// Create new device (async)
        /// </summary>
        Task<ZatcaDevice> CreateAsync(CreateZatcaDeviceDto dto, string userName);

        /// <summary>
        /// Update device
        /// </summary>
        int Update(UpdateZatcaDeviceDto dto, string userName);

        /// <summary>
        /// Delete devices
        /// </summary>
        int Delete(Guid[] ids);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get active devices for dropdown
        /// </summary>
        List<ZatcaDeviceListDto> GetActiveDevices(long? companyId = null);

        /// <summary>
        /// Get production-ready devices
        /// </summary>
        List<ZatcaDeviceListDto> GetProductionReadyDevices(long? companyId = null);

        /// <summary>
        /// Get devices with expiring certificates
        /// </summary>
        List<ZatcaDeviceListDto> GetDevicesWithExpiringCertificates(int daysBeforeExpiry = 30);

        #endregion

        #region CSR Operations

        /// <summary>
        /// Generate CSR (Certificate Signing Request)
        /// </summary>
        GenerateCsrResponseDto GenerateCsr(Guid deviceId, string userName);

        /// <summary>
        /// Generate CSR (async)
        /// </summary>
        Task<GenerateCsrResponseDto> GenerateCsrAsync(Guid deviceId, string userName);

        #endregion

        #region Onboarding Operations

        /// <summary>
        /// Request compliance CSID from ZATCA
        /// </summary>
        OnboardingResponseDto RequestComplianceCsid(OnboardingRequestDto request, string userName);

        /// <summary>
        /// Request compliance CSID (async)
        /// </summary>
        Task<OnboardingResponseDto> RequestComplianceCsidAsync(OnboardingRequestDto request, string userName);

        /// <summary>
        /// Request production CSID from ZATCA
        /// </summary>
        ProductionCsidResponseDto RequestProductionCsid(ProductionCsidRequestDto request, string userName);

        /// <summary>
        /// Request production CSID (async)
        /// </summary>
        Task<ProductionCsidResponseDto> RequestProductionCsidAsync(ProductionCsidRequestDto request, string userName);

        #endregion

        #region Validation

        /// <summary>
        /// Check if device serial number exists
        /// </summary>
        bool SerialNumberExists(string serialNumber, long? companyId, Guid? excludeId = null);

        /// <summary>
        /// Validate device for invoice submission
        /// </summary>
        (bool IsValid, string[] Errors) ValidateDeviceForSubmission(Guid deviceId);

        #endregion
    }
}
