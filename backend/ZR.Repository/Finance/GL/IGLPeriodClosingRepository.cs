using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Period Closing Repository Interface - واجهة مستودع إقفال الفترات
    /// </summary>
    public interface IGLPeriodClosingRepository : IBaseRepository<GLPeriodClosing>
    {
        /// <summary>
        /// Get paginated list of periods - الحصول على قائمة الفترات بالترقيم
        /// </summary>
        PagedInfo<GLPeriodClosing> GetList(GLPeriodClosing parm, PagerInfo pager);

        /// <summary>
        /// Get period by ID - الحصول على الفترة بالمعرف
        /// </summary>
        GLPeriodClosing GetById(Guid id);

        /// <summary>
        /// Get period by ID async - الحصول على الفترة بالمعرف (غير متزامن)
        /// </summary>
        Task<GLPeriodClosing> GetByIdAsync(Guid id);

        /// <summary>
        /// Get periods by status - الحصول على الفترات بالحالة
        /// </summary>
        List<GLPeriodClosing> GetByStatus(PeriodStatus status, long? companyId = null);

        /// <summary>
        /// Get periods by status async - الحصول على الفترات بالحالة (غير متزامن)
        /// </summary>
        Task<List<GLPeriodClosing>> GetByStatusAsync(PeriodStatus status, long? companyId = null);

        /// <summary>
        /// Get period by dates - الحصول على الفترة بالتواريخ
        /// </summary>
        GLPeriodClosing GetByDates(DateTime startDate, DateTime endDate, PeriodType periodType, long? companyId = null);

        /// <summary>
        /// Get period by dates async - الحصول على الفترة بالتواريخ (غير متزامن)
        /// </summary>
        Task<GLPeriodClosing> GetByDatesAsync(DateTime startDate, DateTime endDate, PeriodType periodType, long? companyId = null);

        /// <summary>
        /// Get periods by fiscal year - الحصول على الفترات بالسنة المالية
        /// </summary>
        List<GLPeriodClosing> GetByFiscalYear(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Get periods by fiscal year async - الحصول على الفترات بالسنة المالية (غير متزامن)
        /// </summary>
        Task<List<GLPeriodClosing>> GetByFiscalYearAsync(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Get open periods - الحصول على الفترات المفتوحة
        /// </summary>
        List<GLPeriodClosing> GetOpenPeriods(long? companyId = null);

        /// <summary>
        /// Get open periods async - الحصول على الفترات المفتوحة (غير متزامن)
        /// </summary>
        Task<List<GLPeriodClosing>> GetOpenPeriodsAsync(long? companyId = null);

        /// <summary>
        /// Create period - إنشاء فترة
        /// </summary>
        int CreatePeriod(GLPeriodClosing model);

        /// <summary>
        /// Create period async - إنشاء فترة (غير متزامن)
        /// </summary>
        Task<int> CreatePeriodAsync(GLPeriodClosing model);

        /// <summary>
        /// Update period - تحديث فترة
        /// </summary>
        int UpdatePeriod(GLPeriodClosing model);

        /// <summary>
        /// Update period async - تحديث فترة (غير متزامن)
        /// </summary>
        Task<int> UpdatePeriodAsync(GLPeriodClosing model);

        /// <summary>
        /// Update period status - تحديث حالة الفترة
        /// </summary>
        int UpdateStatus(Guid id, PeriodStatus status, Guid? closedBy = null);

        /// <summary>
        /// Update period status async - تحديث حالة الفترة (غير متزامن)
        /// </summary>
        Task<int> UpdateStatusAsync(Guid id, PeriodStatus status, Guid? closedBy = null);

        /// <summary>
        /// Delete period - حذف فترة
        /// </summary>
        int DeletePeriod(Guid id);

        /// <summary>
        /// Delete period async - حذف فترة (غير متزامن)
        /// </summary>
        Task<int> DeletePeriodAsync(Guid id);

        /// <summary>
        /// Check if period exists - التحقق من وجود الفترة
        /// </summary>
        bool PeriodExists(DateTime startDate, DateTime endDate, PeriodType periodType, long? companyId = null, Guid? excludeId = null);

        /// <summary>
        /// Check if period exists async - التحقق من وجود الفترة (غير متزامن)
        /// </summary>
        Task<bool> PeriodExistsAsync(DateTime startDate, DateTime endDate, PeriodType periodType, long? companyId = null, Guid? excludeId = null);

        /// <summary>
        /// Check if date is in closed period - التحقق إذا التاريخ في فترة مغلقة
        /// </summary>
        bool IsDateInClosedPeriod(DateTime date, long? companyId = null);

        /// <summary>
        /// Check if date is in closed period async - التحقق إذا التاريخ في فترة مغلقة (غير متزامن)
        /// </summary>
        Task<bool> IsDateInClosedPeriodAsync(DateTime date, long? companyId = null);

        /// <summary>
        /// Get period containing a specific date - الحصول على الفترة التي تحتوي على تاريخ محدد
        /// Returns the period where date falls between start and end dates
        /// </summary>
        GLPeriodClosing GetPeriodByDate(DateTime date, long? companyId = null);

        /// <summary>
        /// Get period containing a specific date async - الحصول على الفترة التي تحتوي على تاريخ محدد (غير متزامن)
        /// </summary>
        Task<GLPeriodClosing> GetPeriodByDateAsync(DateTime date, long? companyId = null);
    }
}

