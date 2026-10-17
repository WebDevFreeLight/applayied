using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// System Configuration Repository Interface - واجهة مستودع إعدادات النظام
    /// </summary>
    public interface ISystemConfigurationRepository : IBaseRepository<SystemConfiguration>
    {
        /// <summary>
        /// Get paginated list of configurations - الحصول على قائمة الإعدادات بالترقيم
        /// </summary>
        PagedInfo<SystemConfiguration> GetList(SystemConfiguration parm, PagerInfo pager);

        /// <summary>
        /// Get configuration by ID - الحصول على الإعداد بالمعرف
        /// </summary>
        SystemConfiguration GetById(Guid id);

        /// <summary>
        /// Get configuration by ID async - الحصول على الإعداد بالمعرف (غير متزامن)
        /// </summary>
        Task<SystemConfiguration> GetByIdAsync(Guid id);

        /// <summary>
        /// Get configuration by key - الحصول على الإعداد بالمفتاح
        /// </summary>
        SystemConfiguration GetByKey(string key, long? companyId = null);

        /// <summary>
        /// Get configuration by key async - الحصول على الإعداد بالمفتاح (غير متزامن)
        /// </summary>
        Task<SystemConfiguration> GetByKeyAsync(string key, long? companyId = null);

        /// <summary>
        /// Get all configurations - الحصول على جميع الإعدادات
        /// </summary>
        List<SystemConfiguration> ListAll(long? companyId = null);

        /// <summary>
        /// Get all configurations async - الحصول على جميع الإعدادات (غير متزامن)
        /// </summary>
        Task<List<SystemConfiguration>> ListAllAsync(long? companyId = null);

        /// <summary>
        /// Create configuration - إنشاء إعداد
        /// </summary>
        int CreateConfiguration(SystemConfiguration model);

        /// <summary>
        /// Create configuration async - إنشاء إعداد (غير متزامن)
        /// </summary>
        Task<int> CreateConfigurationAsync(SystemConfiguration model);

        /// <summary>
        /// Update configuration - تحديث إعداد
        /// </summary>
        int UpdateConfiguration(SystemConfiguration model);

        /// <summary>
        /// Update configuration async - تحديث إعداد (غير متزامن)
        /// </summary>
        Task<int> UpdateConfigurationAsync(SystemConfiguration model);

        /// <summary>
        /// Delete configuration - حذف إعداد
        /// </summary>
        int DeleteConfiguration(Guid id);

        /// <summary>
        /// Delete configuration async - حذف إعداد (غير متزامن)
        /// </summary>
        Task<int> DeleteConfigurationAsync(Guid id);

        /// <summary>
        /// Check if key exists - التحقق من وجود المفتاح
        /// </summary>
        bool KeyExists(string key, long? companyId = null, Guid? excludeId = null);

        /// <summary>
        /// Check if key exists async - التحقق من وجود المفتاح (غير متزامن)
        /// </summary>
        Task<bool> KeyExistsAsync(string key, long? companyId = null, Guid? excludeId = null);
    }
}

