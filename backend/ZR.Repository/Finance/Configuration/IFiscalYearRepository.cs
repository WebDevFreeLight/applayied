using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.Configuration;

namespace ZR.Repository.Finance.Configuration
{
    /// <summary>
    /// Fiscal Year Repository Interface - واجهة مستودع السنوات المالية
    /// </summary>
    public interface IFiscalYearRepository : IBaseRepository<FiscalYear>
    {
        /// <summary>
        /// Get paginated list of fiscal years - الحصول على قائمة السنوات المالية بالترقيم
        /// </summary>
        PagedInfo<FiscalYear> GetList(FiscalYear parm, PagerInfo pager);

        /// <summary>
        /// Get fiscal year by ID - الحصول على السنة المالية بالمعرف
        /// </summary>
        FiscalYear GetById(Guid id);

        /// <summary>
        /// Get open fiscal year for company - الحصول على السنة المالية المفتوحة للشركة
        /// </summary>
        FiscalYear GetOpenFiscalYear(long? companyId);

        /// <summary>
        /// Check for overlapping fiscal years - التحقق من تداخل السنوات المالية
        /// </summary>
        bool HasOverlappingPeriod(long? companyId, DateTime startDate, DateTime endDate, Guid? excludeId = null);

        /// <summary>
        /// Get fiscal year containing a specific date - الحصول على السنة المالية التي تحتوي على تاريخ محدد
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <param name="date">Date to find fiscal year for</param>
        /// <returns>Fiscal year containing the date, or null if not found</returns>
        FiscalYear GetFiscalYearByDate(long? companyId, DateTime date);

        /// <summary>
        /// Get all fiscal years for a company ordered by start date - الحصول على جميع السنوات المالية للشركة
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


