using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;

namespace ZR.Service.IService.Finance.Configuration
{
    /// <summary>
    /// System Configuration Service Interface - واجهة خدمة إعدادات النظام
    /// </summary>
    public interface ISystemConfigurationService
    {
        /// <summary>
        /// Get paginated list of configurations - الحصول على قائمة الإعدادات بالترقيم
        /// </summary>
        PagedInfo<ConfigurationResponseDto> GetList(SystemConfigurationQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get configuration by ID - الحصول على الإعداد بالمعرف
        /// </summary>
        ConfigurationResponseDto GetById(Guid id);

        /// <summary>
        /// Get configuration by ID async - الحصول على الإعداد بالمعرف (غير متزامن)
        /// </summary>
        Task<ConfigurationResponseDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Get configuration by key - الحصول على الإعداد بالمفتاح
        /// </summary>
        ConfigurationResponseDto GetByKey(string key, long? companyId = null);

        /// <summary>
        /// Get configuration by key async - الحصول على الإعداد بالمفتاح (غير متزامن)
        /// </summary>
        Task<ConfigurationResponseDto> GetByKeyAsync(string key, long? companyId = null);

        /// <summary>
        /// Get all configurations - الحصول على جميع الإعدادات
        /// </summary>
        List<ConfigurationResponseDto> GetAll(long? companyId = null);

        /// <summary>
        /// Get all configurations async - الحصول على جميع الإعدادات (غير متزامن)
        /// </summary>
        Task<List<ConfigurationResponseDto>> GetAllAsync(long? companyId = null);

        /// <summary>
        /// Create configuration - إنشاء إعداد
        /// </summary>
        ConfigurationResponseDto Create(CreateConfigurationDto dto, string createdBy);

        /// <summary>
        /// Create configuration async - إنشاء إعداد (غير متزامن)
        /// </summary>
        Task<ConfigurationResponseDto> CreateAsync(CreateConfigurationDto dto, string createdBy);

        /// <summary>
        /// Update configuration - تحديث إعداد
        /// </summary>
        int Update(UpdateConfigurationDto dto, string updatedBy);

        /// <summary>
        /// Update configuration async - تحديث إعداد (غير متزامن)
        /// </summary>
        Task<int> UpdateAsync(UpdateConfigurationDto dto, string updatedBy);

        /// <summary>
        /// Delete configuration - حذف إعداد
        /// </summary>
        int Delete(Guid id);

        /// <summary>
        /// Delete configuration async - حذف إعداد (غير متزامن)
        /// </summary>
        Task<int> DeleteAsync(Guid id);

        /// <summary>
        /// Get configuration value - الحصول على قيمة الإعداد
        /// </summary>
        string GetValue(string key, long? companyId = null);

        /// <summary>
        /// Get configuration value async - الحصول على قيمة الإعداد (غير متزامن)
        /// </summary>
        Task<string> GetValueAsync(string key, long? companyId = null);

        /// <summary>
        /// Get typed configuration value - الحصول على قيمة الإعداد بالنوع
        /// </summary>
        T GetValue<T>(string key, long? companyId = null) where T : IConvertible;

        /// <summary>
        /// Get typed configuration value async - الحصول على قيمة الإعداد بالنوع (غير متزامن)
        /// </summary>
        Task<T> GetValueAsync<T>(string key, long? companyId = null) where T : IConvertible;
    }
}


