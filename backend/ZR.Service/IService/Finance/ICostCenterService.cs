using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.CostCenter.Dto;
using CostCenterEntity = ZR.Model.Finance.CostCenter.CostCenter;

namespace ZR.Service.IService.Finance
{
    /// <summary>
    /// Cost Center Service Interface - واجهة خدمة مراكز التكلفة
    /// </summary>
    public interface ICostCenterService : IBaseService<CostCenterEntity>
    {
        /// <summary>
        /// Get paginated list of cost centers - الحصول على قائمة مراكز التكلفة بالترقيم
        /// </summary>
        PagedInfo<CostCenterListDto> GetList(CostCenterQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get cost center by ID - الحصول على مركز التكلفة بالمعرف
        /// </summary>
        CostCenterDetailDto GetById(long costCenterId);

        /// <summary>
        /// Create cost center - إنشاء مركز تكلفة
        /// </summary>
        CostCenterEntity Create(CreateCostCenterDto dto, string userName);

        /// <summary>
        /// Update cost center - تحديث مركز تكلفة
        /// </summary>
        int Update(UpdateCostCenterDto dto, string userName);

        /// <summary>
        /// Delete cost centers - حذف مراكز التكلفة
        /// </summary>
        int Delete(long[] ids);

        /// <summary>
        /// Get active cost centers for dropdown - مراكز التكلفة النشطة للقائمة المنسدلة
        /// </summary>
        List<CostCenterSelectItemDto> GetActiveCostCenters(long? branchId = null);

        /// <summary>
        /// Get cost centers by type - مراكز التكلفة حسب النوع
        /// </summary>
        List<CostCenterSelectItemDto> GetCostCentersByType(string costCenterType);

        /// <summary>
        /// Get cost center tree - شجرة مراكز التكلفة
        /// </summary>
        List<CostCenterListDto> GetCostCenterTree();

        /// <summary>
        /// Get cost center balance - رصيد مركز التكلفة
        /// </summary>
        CostCenterBalanceDto GetCostCenterBalance(long costCenterId, DateTime? asOfDate = null);

        /// <summary>
        /// Get cost center report - تقرير مراكز التكلفة
        /// </summary>
        List<CostCenterReportItemDto> GetCostCenterReport(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Check if cost center code exists - التحقق من وجود رمز مركز التكلفة
        /// </summary>
        bool CodeExists(string code, long? excludeId = null);

        /// <summary>
        /// Get next cost center code - الحصول على رمز مركز التكلفة التالي
        /// </summary>
        string GetNextCode(string prefix);
    }
}
