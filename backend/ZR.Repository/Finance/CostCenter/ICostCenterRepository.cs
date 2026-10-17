using System;
using System.Collections.Generic;
using ZR.Model;
using CostCenterEntity = ZR.Model.Finance.CostCenter.CostCenter;

namespace ZR.Repository.Finance.CostCenter
{
    /// <summary>
    /// Cost Center Repository Interface - واجهة مستودع مراكز التكلفة
    /// </summary>
    public interface ICostCenterRepository : IBaseRepository<CostCenterEntity>
    {
        /// <summary>
        /// Get paginated list of cost centers - الحصول على قائمة مراكز التكلفة بالترقيم
        /// </summary>
        PagedInfo<CostCenterEntity> GetList(CostCenterEntity parm, PagerInfo pager);

        /// <summary>
        /// Get cost center by ID - الحصول على مركز التكلفة بالمعرف
        /// </summary>
        CostCenterEntity GetById(long costCenterId);

        /// <summary>
        /// Get cost center by code - الحصول على مركز التكلفة بالرمز
        /// </summary>
        CostCenterEntity GetByCode(string code);

        /// <summary>
        /// Check if cost center code exists - التحقق من وجود رمز مركز التكلفة
        /// </summary>
        bool CodeExists(string code, long? excludeId = null);

        /// <summary>
        /// Get all active cost centers - الحصول على جميع مراكز التكلفة النشطة
        /// </summary>
        List<CostCenterEntity> GetActiveCostCenters(long? branchId = null);

        /// <summary>
        /// Get cost centers by type - الحصول على مراكز التكلفة حسب النوع
        /// </summary>
        List<CostCenterEntity> GetByType(string costCenterType);

        /// <summary>
        /// Get children of a cost center - الحصول على الأبناء
        /// </summary>
        List<CostCenterEntity> GetChildren(long parentId);

        /// <summary>
        /// Get all cost centers for tree - الحصول على جميع مراكز التكلفة للشجرة
        /// </summary>
        List<CostCenterEntity> GetAllForTree();

        /// <summary>
        /// Check if cost center has children - التحقق من وجود أبناء
        /// </summary>
        bool HasChildren(long costCenterId);

        /// <summary>
        /// Get next available code - الحصول على الكود التالي المتاح
        /// </summary>
        string GetNextCode(string prefix);
    }
}
