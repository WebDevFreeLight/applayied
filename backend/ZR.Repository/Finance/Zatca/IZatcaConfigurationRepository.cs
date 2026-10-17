using System;
using System.Threading.Tasks;
using ZR.Model.Finance.Zatca;

namespace ZR.Repository.Finance.Zatca
{
    /// <summary>
    /// ZATCA Configuration Repository Interface
    /// واجهة مستودع إعدادات زاتكا
    /// </summary>
    public interface IZatcaConfigurationRepository : IBaseRepository<ZatcaConfiguration>
    {
        /// <summary>
        /// Get configuration by company ID
        /// </summary>
        ZatcaConfiguration GetByCompanyId(long? companyId);

        /// <summary>
        /// Get configuration by company ID (async)
        /// </summary>
        Task<ZatcaConfiguration> GetByCompanyIdAsync(long? companyId);

        /// <summary>
        /// Get configuration by ID
        /// </summary>
        ZatcaConfiguration GetById(Guid id);

        /// <summary>
        /// Get configuration by ID (async)
        /// </summary>
        Task<ZatcaConfiguration> GetByIdAsync(Guid id);

        /// <summary>
        /// Insert configuration
        /// </summary>
        int InsertConfiguration(ZatcaConfiguration model);

        /// <summary>
        /// Insert configuration (async)
        /// </summary>
        Task<int> InsertConfigurationAsync(ZatcaConfiguration model);

        /// <summary>
        /// Update configuration
        /// </summary>
        int UpdateConfiguration(ZatcaConfiguration model);

        /// <summary>
        /// Update configuration (async)
        /// </summary>
        Task<int> UpdateConfigurationAsync(ZatcaConfiguration model);

        /// <summary>
        /// Check if configuration exists for company
        /// </summary>
        bool ConfigurationExists(long? companyId);

        /// <summary>
        /// Check if configuration exists for company (async)
        /// </summary>
        Task<bool> ConfigurationExistsAsync(long? companyId);

        /// <summary>
        /// Update active device
        /// </summary>
        int UpdateActiveDevice(long? companyId, Guid? deviceId);

        /// <summary>
        /// Update environment
        /// </summary>
        int UpdateEnvironment(long? companyId, Model.Finance.Zatca.Enums.ZatcaEnvironment environment);
    }
}
