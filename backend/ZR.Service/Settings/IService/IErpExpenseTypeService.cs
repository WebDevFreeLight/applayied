using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Settings;
using ZR.Service;

namespace ZR.Service.Settings.IService
{
    /// <summary>
    /// ERP Expense Type Service Interface - واجهة خدمة أنواع المصروفات
    /// </summary>
    public interface IErpExpenseTypeService : IBaseService<ErpExpenseType>
    {
        /// <summary>
        /// Get paginated list of expense types - الحصول على قائمة أنواع المصروفات بالترقيم
        /// </summary>
        PagedInfo<ErpExpenseType> GetList(ErpExpenseType parm, PagerInfo pager);

        /// <summary>
        /// Get expense type by ID - الحصول على نوع المصروف بالمعرف
        /// </summary>
        ErpExpenseType GetInfo(long expenseTypeId);

        /// <summary>
        /// Add expense type - إضافة نوع مصروف
        /// </summary>
        ErpExpenseType AddExpenseType(ErpExpenseType model);

        /// <summary>
        /// Update expense type - تحديث نوع مصروف
        /// </summary>
        int UpdateExpenseType(ErpExpenseType model);

        /// <summary>
        /// Delete expense types - حذف أنواع المصروفات
        /// </summary>
        int Delete(long[] ids);

        /// <summary>
        /// Get active expense types - الحصول على أنواع المصروفات النشطة
        /// </summary>
        List<ErpExpenseType> GetActiveExpenseTypes();
    }
}


