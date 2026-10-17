using System;
using ZR.Model;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// ERP Settings Repository Interface - واجهة مستودع إعدادات النظام
    /// </summary>
    public interface IERPSettingsRepository : IBaseRepository<ERPSettings>
    {
        /// <summary>
        /// Get paginated list of settings - الحصول على قائمة الإعدادات بالترقيم
        /// </summary>
        PagedInfo<ERPSettings> GetList(ERPSettings parm, PagerInfo pager);

        /// <summary>
        /// Get settings by ID - الحصول على الإعدادات بالمعرف
        /// </summary>
        ERPSettings GetById(Guid id);

        /// <summary>
        /// Get settings by company - الحصول على إعدادات الشركة
        /// </summary>
        ERPSettings GetByCompanyId(long? companyId);
    }
}


