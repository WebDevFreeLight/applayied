using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Settings;
using ZR.Service;

namespace ZR.Service.Subscriptions.IService
{
    /// <summary>
    /// ERP Service Category Service Interface - واجهة خدمة فئات الخدمات
    /// </summary>
    public interface IErpServiceCategoryService : IBaseService<ErpServiceCategory>
    {
        /// <summary>
        /// Get paginated list of service categories - الحصول على قائمة فئات الخدمات بالترقيم
        /// </summary>
        PagedInfo<ErpServiceCategory> GetList(ErpServiceCategory parm, PagerInfo pager);

        /// <summary>
        /// Get service category by ID - الحصول على فئة الخدمة بالمعرف
        /// </summary>
        ErpServiceCategory GetInfo(long categoryId);

        /// <summary>
        /// Add service category - إضافة فئة خدمة
        /// </summary>
        ErpServiceCategory AddCategory(ErpServiceCategory model);

        /// <summary>
        /// Update service category - تحديث فئة خدمة
        /// </summary>
        int UpdateCategory(ErpServiceCategory model);

        /// <summary>
        /// Delete service categories - حذف فئات الخدمات
        /// </summary>
        int Delete(long[] ids);

        /// <summary>
        /// Get active service categories - الحصول على فئات الخدمات النشطة
        /// </summary>
        List<ErpServiceCategory> GetActiveCategories();

        /// <summary>
        /// Get service category tree - الحصول على شجرة فئات الخدمات
        /// </summary>
        List<ErpServiceCategory> GetCategoryTree();
    }
}


