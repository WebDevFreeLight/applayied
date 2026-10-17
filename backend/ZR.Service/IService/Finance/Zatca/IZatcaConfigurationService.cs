using System;
using System.Threading.Tasks;
using ZR.Model.Finance.Zatca;
using ZR.Model.Finance.Zatca.Dto;
using ZR.Model.Finance.Zatca.Enums;

namespace ZR.Service.IService.Finance.Zatca
{
    /// <summary>
    /// ZATCA Configuration Service Interface
    /// واجهة خدمة إعدادات زاتكا
    /// </summary>
    public interface IZatcaConfigurationService
    {
        /// <summary>
        /// Get configuration for company
        /// </summary>
        ZatcaConfigurationDto GetConfiguration(long? companyId = null);

        /// <summary>
        /// Get configuration for company (async)
        /// </summary>
        Task<ZatcaConfigurationDto> GetConfigurationAsync(long? companyId = null);

        /// <summary>
        /// Save configuration (create or update)
        /// </summary>
        ZatcaConfiguration SaveConfiguration(SaveZatcaConfigurationDto dto, string userName);

        /// <summary>
        /// Save configuration (async)
        /// </summary>
        Task<ZatcaConfiguration> SaveConfigurationAsync(SaveZatcaConfigurationDto dto, string userName);

        /// <summary>
        /// Validate configuration
        /// </summary>
        ValidateConfigurationResponseDto ValidateConfiguration(long? companyId = null);

        /// <summary>
        /// Validate configuration (async)
        /// </summary>
        Task<ValidateConfigurationResponseDto> ValidateConfigurationAsync(long? companyId = null);

        /// <summary>
        /// Set active device
        /// </summary>
        int SetActiveDevice(long? companyId, Guid deviceId);

        /// <summary>
        /// Change environment
        /// </summary>
        int ChangeEnvironment(long? companyId, ZatcaEnvironment environment);

        /// <summary>
        /// Test API connection
        /// </summary>
        ZatcaApiTestResponseDto TestApiConnection(ZatcaApiTestRequestDto request);

        /// <summary>
        /// Test API connection (async)
        /// </summary>
        Task<ZatcaApiTestResponseDto> TestApiConnectionAsync(ZatcaApiTestRequestDto request);

        /// <summary>
        /// Check if ZATCA is enabled and configured
        /// </summary>
        bool IsZatcaEnabled(long? companyId = null);

        /// <summary>
        /// Get seller information for invoice generation
        /// </summary>
        ZatcaConfiguration GetSellerInfo(long? companyId = null);
    }
}
