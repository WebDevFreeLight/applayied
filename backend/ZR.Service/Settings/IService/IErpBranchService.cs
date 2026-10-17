using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Settings;
using ZR.Model.HR.EmployeeCore;
using ZR.Service;

namespace ZR.Service.Settings.IService
{
    /// <summary>
    /// ERP Branch Service Interface - واجهة خدمة الفروع
    /// </summary>
    public interface IErpBranchService : IBaseService<ErpBranch>
    {
        /// <summary>
        /// Get paginated list of branches - الحصول على قائمة الفروع بالترقيم
        /// </summary>
        PagedInfo<ErpBranch> GetList(ErpBranch parm, PagerInfo pager);

        /// <summary>
        /// Get all branches - الحصول على جميع الفروع
        /// </summary>
        List<ErpBranch> GetAllBranches();

        /// <summary>
        /// Get branch by ID - الحصول على الفرع بالمعرف
        /// </summary>
        ErpBranch GetInfo(long branchId);

        /// <summary>
        /// Add branch - إضافة فرع
        /// </summary>
        ErpBranch AddBranch(ErpBranch model);

        /// <summary>
        /// Update branch - تحديث فرع
        /// </summary>
        int UpdateBranch(ErpBranch model);

        /// <summary>
        /// Delete branches - حذف فروع
        /// </summary>
        int Delete(long[] ids);

        /// <summary>
        /// Get main branch - الحصول على الفرع الرئيسي
        /// </summary>
        ErpBranch GetMainBranch();

        /// <summary>
        /// Get active branches - الحصول على الفروع النشطة
        /// </summary>
        List<ErpBranch> GetActiveBranches();

        /// <summary>
        /// Set main branch - تعيين الفرع الرئيسي
        /// </summary>
        int SetMainBranch(long branchId);

        /// <summary>
        /// Toggle branch status - تبديل حالة الفرع
        /// </summary>
        int ToggleStatus(long branchId);

        /// <summary>
        /// Get branch employees - الحصول على موظفي الفرع
        /// </summary>
        List<HrEmployeeCore> GetBranchEmployees(long branchId);

        /// <summary>
        /// Get branch statistics - الحصول على إحصائيات الفرع
        /// </summary>
        object GetBranchStatistics(long branchId);
    }
}


