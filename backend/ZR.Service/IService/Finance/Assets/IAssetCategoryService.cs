using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Assets;
using ZR.Model.Finance.Assets.Dto;

namespace ZR.Service.IService.Finance.Assets
{
    /// <summary>
    /// Asset Category Service Interface - واجهة خدمة فئات الأصول
    /// </summary>
    public interface IAssetCategoryService : IBaseService<AssetCategory>
    {
        /// <summary>
        /// Get category list with paging - الحصول على قائمة الفئات مع ترقيم الصفحات
        /// </summary>
        PagedInfo<AssetCategoryListDto> GetList(AssetCategoryQueryDto parm);

        /// <summary>
        /// Get category by ID - الحصول على فئة بالمعرف
        /// </summary>
        AssetCategoryDetailDto GetDetail(Guid id);

        /// <summary>
        /// Create category - إنشاء فئة
        /// </summary>
        AssetCategory Create(CreateAssetCategoryDto dto, string userId);

        /// <summary>
        /// Update category - تحديث فئة
        /// </summary>
        int Update(UpdateAssetCategoryDto dto, string userId);

        /// <summary>
        /// Delete categories - حذف فئات
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Check if category code exists - التحقق من وجود رمز الفئة
        /// </summary>
        bool CodeExists(string code, Guid? excludeId = null, long? companyId = null);

        /// <summary>
        /// Get all active categories - الحصول على جميع الفئات النشطة
        /// </summary>
        List<AssetCategoryListDto> GetActiveCategories(long? companyId = null);

        /// <summary>
        /// Get next category code - الحصول على رمز الفئة التالي
        /// Format: CAT-YYYY-NNNN (e.g., CAT-2025-0001)
        /// </summary>
        string GetNextCategoryCode(long? companyId = null);
    }
}

