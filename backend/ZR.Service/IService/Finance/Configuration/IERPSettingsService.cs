using System;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;

namespace ZR.Service.IService.Finance.Configuration
{
    /// <summary>
    /// ERP Settings Service Interface - واجهة خدمة إعدادات النظام
    /// </summary>
    public interface IERPSettingsService : IBaseService<ERPSettings>
    {
        /// <summary>
        /// Get paginated list of settings - الحصول على قائمة الإعدادات بالترقيم
        /// </summary>
        PagedInfo<ERPSettingsListDto> GetList(ERPSettingsQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get settings by ID - الحصول على الإعدادات بالمعرف
        /// </summary>
        ERPSettingsDetailDto GetById(Guid id);

        /// <summary>
        /// Get settings by company - الحصول على إعدادات الشركة
        /// </summary>
        ERPSettingsDetailDto GetByCompanyId(long? companyId);

        /// <summary>
        /// Get or Create settings for company - الحصول على أو إنشاء إعدادات الشركة
        /// Returns existing settings if found, creates default settings if not
        /// </summary>
        ERPSettingsDetailDto GetOrCreate(long? companyId, string userName);

        /// <summary>
        /// Create settings - إنشاء إعدادات
        /// </summary>
        ERPSettings Create(CreateERPSettingsDto dto, string userName);

        /// <summary>
        /// Update settings - تحديث إعدادات
        /// </summary>
        int Update(UpdateERPSettingsDto dto, string userName);

        /// <summary>
        /// Delete settings - حذف إعدادات
        /// </summary>
        int Delete(Guid[] ids);
    }
}



