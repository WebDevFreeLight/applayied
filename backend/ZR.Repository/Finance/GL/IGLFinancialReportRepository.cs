using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Financial Report Repository Interface - واجهة مستودع التقارير المالية
    /// </summary>
    public interface IGLFinancialReportRepository : IBaseRepository<GLFinancialReport>
    {
        /// <summary>
        /// Get paginated list of reports - الحصول على قائمة التقارير بالترقيم
        /// </summary>
        PagedInfo<GLFinancialReport> GetList(GLFinancialReport parm, PagerInfo pager);

        /// <summary>
        /// Get report by ID - الحصول على التقرير بالمعرف
        /// </summary>
        GLFinancialReport GetById(Guid id);

        /// <summary>
        /// Get report by ID async - الحصول على التقرير بالمعرف (غير متزامن)
        /// </summary>
        Task<GLFinancialReport> GetByIdAsync(Guid id);

        /// <summary>
        /// Get reports by type - الحصول على التقارير بالنوع
        /// </summary>
        List<GLFinancialReport> GetByType(ReportType reportType, long? companyId = null);

        /// <summary>
        /// Get reports by type async - الحصول على التقارير بالنوع (غير متزامن)
        /// </summary>
        Task<List<GLFinancialReport>> GetByTypeAsync(ReportType reportType, long? companyId = null);

        /// <summary>
        /// Get reports by period - الحصول على التقارير بالفترة
        /// </summary>
        List<GLFinancialReport> GetByPeriod(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get reports by period async - الحصول على التقارير بالفترة (غير متزامن)
        /// </summary>
        Task<List<GLFinancialReport>> GetByPeriodAsync(DateTime periodStart, DateTime periodEnd, long? companyId = null);

        /// <summary>
        /// Get reports by fiscal year - الحصول على التقارير بالسنة المالية
        /// </summary>
        List<GLFinancialReport> GetByFiscalYear(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Create report - إنشاء تقرير
        /// </summary>
        int CreateReport(GLFinancialReport model);

        /// <summary>
        /// Create report async - إنشاء تقرير (غير متزامن)
        /// </summary>
        Task<int> CreateReportAsync(GLFinancialReport model);

        /// <summary>
        /// Update report - تحديث تقرير
        /// </summary>
        int UpdateReport(GLFinancialReport model);

        /// <summary>
        /// Update report async - تحديث تقرير (غير متزامن)
        /// </summary>
        Task<int> UpdateReportAsync(GLFinancialReport model);

        /// <summary>
        /// Delete report - حذف تقرير
        /// </summary>
        int DeleteReport(Guid id);

        /// <summary>
        /// Delete report async - حذف تقرير (غير متزامن)
        /// </summary>
        Task<int> DeleteReportAsync(Guid id);

        /// <summary>
        /// Generate report number - توليد رقم التقرير
        /// </summary>
        string GenerateReportNumber(ReportType reportType, long? companyId = null, int? fiscalYear = null);

        /// <summary>
        /// Generate report number async - توليد رقم التقرير (غير متزامن)
        /// </summary>
        Task<string> GenerateReportNumberAsync(ReportType reportType, long? companyId = null, int? fiscalYear = null);
    }
}

