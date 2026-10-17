using System;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;

namespace ZR.Service.IService.Finance.Configuration
{
    /// <summary>
    /// Tax Configuration Service Interface - واجهة خدمة إعدادات الضرائب
    /// </summary>
    public interface ITaxConfigurationService : IBaseService<TaxConfiguration>
    {
        /// <summary>
        /// Get tax configuration by company ID - الحصول على إعدادات الضريبة حسب الشركة
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>Tax configuration detail or null if not found</returns>
        TaxConfigurationDetailDto GetByCompanyId(long? companyId);

        /// <summary>
        /// Get tax configuration detail by ID - الحصول على تفاصيل إعداد الضريبة بالمعرف
        /// </summary>
        /// <param name="id">Configuration ID</param>
        /// <returns>Tax configuration detail</returns>
        TaxConfigurationDetailDto GetDetail(Guid id);

        /// <summary>
        /// Save tax configuration (create or update) - حفظ إعدادات الضريبة
        /// </summary>
        /// <param name="dto">Save DTO</param>
        /// <param name="userId">Current user ID</param>
        /// <returns>Saved configuration</returns>
        TaxConfiguration SaveConfiguration(SaveTaxConfigurationDto dto, string userId);

        /// <summary>
        /// Get configuration status - الحصول على حالة الإعدادات
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>Configuration status with details</returns>
        TaxConfigurationStatusDto GetConfigurationStatus(long? companyId);

        /// <summary>
        /// Get VAT Input Account ID for a company - الحصول على معرف حساب ضريبة المدخلات
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>VAT Input Account ID or null</returns>
        Guid? GetVATInputAccountId(long? companyId);

        /// <summary>
        /// Get VAT Output Account ID for a company - الحصول على معرف حساب ضريبة المخرجات
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>VAT Output Account ID or null</returns>
        Guid? GetVATOutputAccountId(long? companyId);

        /// <summary>
        /// Get Default AR Account ID for a company - الحصول على معرف حساب الذمم المدينة الافتراضي
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>Default AR Account ID or null</returns>
        Guid? GetDefaultARAccountId(long? companyId);

        /// <summary>
        /// Get Default AP Account ID for a company - الحصول على معرف حساب الذمم الدائنة الافتراضي
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>Default AP Account ID or null</returns>
        Guid? GetDefaultAPAccountId(long? companyId);

        /// <summary>
        /// Ensure tax configuration exists, creating default if needed - التأكد من وجود إعدادات الضريبة
        /// </summary>
        /// <param name="companyId">Company ID</param>
        void EnsureConfigurationExists(long? companyId);
    }
}
