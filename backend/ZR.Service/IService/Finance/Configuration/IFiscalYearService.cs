using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;

namespace ZR.Service.IService.Finance.Configuration
{
    /// <summary>
    /// Fiscal Year Service Interface - واجهة خدمة السنوات المالية
    /// </summary>
    public interface IFiscalYearService : IBaseService<FiscalYear>
    {
        /// <summary>
        /// Get paginated list of fiscal years - الحصول على قائمة السنوات المالية بالترقيم
        /// </summary>
        PagedInfo<FiscalYearListDto> GetList(FiscalYearQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get fiscal year by ID - الحصول على السنة المالية بالمعرف
        /// </summary>
        FiscalYearDetailDto GetById(Guid id);

        /// <summary>
        /// Create fiscal year - إنشاء سنة مالية
        /// </summary>
        FiscalYear Create(CreateFiscalYearDto dto, string userName);

        /// <summary>
        /// Update fiscal year - تحديث سنة مالية
        /// </summary>
        int Update(UpdateFiscalYearDto dto, string userName);

        /// <summary>
        /// Delete fiscal years - حذف سنوات مالية
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Open fiscal year - فتح سنة مالية
        /// </summary>
        int OpenFiscalYear(Guid id, string userName);

        /// <summary>
        /// Close fiscal year - إغلاق سنة مالية
        /// </summary>
        int CloseFiscalYear(Guid id, string userName);

        /// <summary>
        /// Validate overlapping periods - التحقق من تداخل الفترات
        /// </summary>
        bool ValidateOverlappingPeriods(long? companyId, DateTime startDate, DateTime endDate, Guid? excludeId = null);

        /// <summary>
        /// Get fiscal year containing a specific date - الحصول على السنة المالية التي تحتوي على تاريخ محدد
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="date">Date to find fiscal year for</param>
        /// <returns>Fiscal year containing the date, or null if not found</returns>
        FiscalYear GetFiscalYearByDate(long? companyId, DateTime date);

        /// <summary>
        /// Get all fiscal years for a company ordered by start date descending
        /// الحصول على جميع السنوات المالية للشركة
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of fiscal years ordered by start date descending</returns>
        List<FiscalYear> GetAllFiscalYears(long? companyId);

        /// <summary>
        /// Get earliest fiscal year for a company - الحصول على أقدم سنة مالية للشركة
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>Earliest fiscal year, or null if none exists</returns>
        FiscalYear GetEarliestFiscalYear(long? companyId);
    }
}


