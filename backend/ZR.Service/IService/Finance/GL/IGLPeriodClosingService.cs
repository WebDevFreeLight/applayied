using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZR.Model;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// GL Period Closing Service Interface - واجهة خدمة إقفال الفترات
    /// </summary>
    public interface IGLPeriodClosingService
    {
        /// <summary>
        /// Get paginated list of periods - الحصول على قائمة الفترات بالترقيم
        /// </summary>
        PagedInfo<PeriodClosingResponseDto> GetList(GLPeriodClosingQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get period by ID - الحصول على الفترة بالمعرف
        /// </summary>
        PeriodClosingResponseDto GetById(Guid id);

        /// <summary>
        /// Get period by ID async - الحصول على الفترة بالمعرف (غير متزامن)
        /// </summary>
        Task<PeriodClosingResponseDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Get periods by fiscal year - الحصول على الفترات بالسنة المالية
        /// </summary>
        List<PeriodClosingResponseDto> GetByFiscalYear(int fiscalYear, long? companyId = null);

        /// <summary>
        /// Get open periods - الحصول على الفترات المفتوحة
        /// </summary>
        List<PeriodClosingResponseDto> GetOpenPeriods(long? companyId = null);

        /// <summary>
        /// Create period - إنشاء فترة
        /// </summary>
        PeriodClosingResponseDto Create(CreatePeriodClosingDto dto, string createdBy);

        /// <summary>
        /// Create period async - إنشاء فترة (غير متزامن)
        /// </summary>
        Task<PeriodClosingResponseDto> CreateAsync(CreatePeriodClosingDto dto, string createdBy);

        /// <summary>
        /// Close period - إقفال الفترة
        /// </summary>
        int ClosePeriod(ClosePeriodDto dto, string closedBy);

        /// <summary>
        /// Close period async - إقفال الفترة (غير متزامن)
        /// </summary>
        Task<int> ClosePeriodAsync(ClosePeriodDto dto, string closedBy);

        /// <summary>
        /// Reopen period - إعادة فتح الفترة
        /// </summary>
        int ReopenPeriod(ReopenPeriodDto dto, string reopenedBy);

        /// <summary>
        /// Reopen period async - إعادة فتح الفترة (غير متزامن)
        /// </summary>
        Task<int> ReopenPeriodAsync(ReopenPeriodDto dto, string reopenedBy);

        /// <summary>
        /// Lock period (permanent) - قفل الفترة نهائياً
        /// </summary>
        int LockPeriod(Guid id, string lockedBy);

        /// <summary>
        /// Lock period async - قفل الفترة نهائياً (غير متزامن)
        /// </summary>
        Task<int> LockPeriodAsync(Guid id, string lockedBy);

        /// <summary>
        /// Get period status - الحصول على حالة الفترة
        /// </summary>
        PeriodStatusResponseDto GetPeriodStatus(Guid id);

        /// <summary>
        /// Get period status async - الحصول على حالة الفترة (غير متزامن)
        /// </summary>
        Task<PeriodStatusResponseDto> GetPeriodStatusAsync(Guid id);

        /// <summary>
        /// Validate period for closing - التحقق من الفترة للإقفال
        /// </summary>
        (bool CanClose, string Message) ValidateForClosing(Guid id);

        /// <summary>
        /// Validate period for closing async - التحقق من الفترة للإقفال (غير متزامن)
        /// </summary>
        Task<(bool CanClose, string Message)> ValidateForClosingAsync(Guid id);

        /// <summary>
        /// Check if date is in closed period - التحقق إذا التاريخ في فترة مغلقة
        /// </summary>
        bool IsDateInClosedPeriod(DateTime date, long? companyId = null);

        /// <summary>
        /// Check if date is in closed or locked period - التحقق إذا التاريخ في فترة مغلقة أو مقفلة
        /// Throws PeriodClosedException if the date falls in a closed/locked period
        /// </summary>
        /// <param name="date">The date to check</param>
        /// <param name="companyId">Optional company ID</param>
        /// <param name="allowDrafts">If true, allows posting to Draft status periods</param>
        void CheckIfDateInClosedOrLockedPeriod(DateTime date, long? companyId = null, bool allowDrafts = false);

        /// <summary>
        /// Delete period - حذف الفترة
        /// </summary>
        int Delete(Guid id);

        /// <summary>
        /// Delete period async - حذف الفترة (غير متزامن)
        /// </summary>
        Task<int> DeleteAsync(Guid id);

        /// <summary>
        /// Generate fiscal year periods - توليد فترات السنة المالية
        /// </summary>
        List<PeriodClosingResponseDto> GenerateFiscalYearPeriods(int fiscalYear, PeriodType periodType, string createdBy, long? companyId = null);

        /// <summary>
        /// Generate fiscal year periods async - توليد فترات السنة المالية (غير متزامن)
        /// </summary>
        Task<List<PeriodClosingResponseDto>> GenerateFiscalYearPeriodsAsync(int fiscalYear, PeriodType periodType, string createdBy, long? companyId = null);
    }
}


