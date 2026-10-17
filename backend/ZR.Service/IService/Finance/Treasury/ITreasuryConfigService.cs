using System;
using ZR.Model;
using ZR.Model.Finance.Treasury;
using ZR.Model.Finance.Treasury.Dto;

namespace ZR.Service.IService.Finance.Treasury
{
    /// <summary>
    /// Treasury Config Service Interface - واجهة خدمة إعدادات الخزينة
    /// </summary>
    public interface ITreasuryConfigService : IBaseService<TreasuryConfig>
    {
        /// <summary>
        /// Get config list with paging - الحصول على قائمة الإعدادات مع ترقيم الصفحات
        /// </summary>
        PagedInfo<TreasuryConfigDetailDto> GetList(TreasuryConfigQueryDto parm);

        /// <summary>
        /// Get config by ID - الحصول على إعداد بالمعرف
        /// </summary>
        TreasuryConfigDetailDto GetDetail(Guid id);

        /// <summary>
        /// Get config by tenant - الحصول على إعداد حسب المستأجر
        /// </summary>
        TreasuryConfigDetailDto GetByTenant(long? companyId);

        /// <summary>
        /// Get or Create config for tenant - الحصول على أو إنشاء إعداد للمستأجر
        /// Returns existing config if found, creates default config if not
        /// </summary>
        TreasuryConfigDetailDto GetOrCreate(long? companyId, string userId);

        /// <summary>
        /// Create config - إنشاء إعداد
        /// </summary>
        TreasuryConfig Create(CreateTreasuryConfigDto dto, string userId);

        /// <summary>
        /// Update config - تحديث إعداد
        /// </summary>
        int Update(UpdateTreasuryConfigDto dto, string userId);
    }
}






























