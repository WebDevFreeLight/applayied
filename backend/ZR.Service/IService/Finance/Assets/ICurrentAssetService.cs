using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Dto;

namespace ZR.Service.IService.Finance.Assets
{
    /// <summary>
    /// Current Asset Service Interface - واجهة خدمة الأصول المتداولة
    /// </summary>
    public interface ICurrentAssetService : IBaseService<CurrentAsset>
    {
        /// <summary>
        /// Get asset list with paging - الحصول على قائمة الأصول مع ترقيم الصفحات
        /// </summary>
        PagedInfo<CurrentAssetListDto> GetList(CurrentAssetQueryDto parm);

        /// <summary>
        /// Get asset by ID - الحصول على أصل بالمعرف
        /// </summary>
        CurrentAssetDetailDto GetDetail(Guid id);

        /// <summary>
        /// Create asset - إنشاء أصل
        /// </summary>
        CurrentAsset Create(CreateCurrentAssetDto dto, string userId);

        /// <summary>
        /// Update asset - تحديث أصل
        /// </summary>
        int Update(UpdateCurrentAssetDto dto, string userId);

        /// <summary>
        /// Delete assets - حذف أصول
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Check if item code exists - التحقق من وجود رمز الصنف
        /// </summary>
        bool CodeExists(string code, Guid? excludeId = null, long? companyId = null);

        /// <summary>
        /// Get assets by category - الحصول على الأصول حسب الفئة
        /// </summary>
        List<CurrentAssetListDto> GetByCategory(string category, long? companyId = null);

        /// <summary>
        /// Get total value of current assets - الحصول على إجمالي قيمة الأصول المتداولة
        /// </summary>
        decimal GetTotalValue(long? companyId = null);
    }
}

