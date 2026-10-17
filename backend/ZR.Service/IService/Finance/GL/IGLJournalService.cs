using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;
using ZR.Model.Finance.Enums;
using ZR.Service.Finance.Validation;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// GL Journal Service Interface - واجهة خدمة دفتر اليومية
    /// Enhanced with comprehensive validation and workflow methods
    /// </summary>
    public interface IGLJournalService : IBaseService<GLJournal>
    {
        /// <summary>
        /// Get paginated list of journals - الحصول على قائمة دفاتر اليومية بالترقيم
        /// </summary>
        PagedInfo<JournalResponseDto> GetList(GLJournalQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get journal by ID - الحصول على دفتر اليومية بالمعرف
        /// </summary>
        JournalResponseDto GetById(Guid id);

        /// <summary>
        /// Get journal with entries - الحصول على دفتر اليومية مع القيود
        /// </summary>
        JournalResponseDto GetWithEntries(Guid id);

        /// <summary>
        /// Create journal with validation - إنشاء دفتر يومية مع التحقق
        /// </summary>
        GLJournal Create(CreateJournalDto dto, string userName);

        /// <summary>
        /// Update journal with validation - تحديث دفتر يومية مع التحقق
        /// </summary>
        int Update(UpdateJournalDto dto, string userName);

        /// <summary>
        /// Delete journals - حذف دفاتر يومية (soft delete)
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Post journal with comprehensive validation - ترحيل دفتر يومية مع التحقق الشامل
        /// </summary>
        int PostJournal(Guid id, string userName);

        /// <summary>
        /// Cancel journal - إلغاء دفتر يومية (only non-posted)
        /// </summary>
        int CancelJournal(Guid id, string userName);

        /// <summary>
        /// Reverse a posted journal - عكس قيد مرحّل
        /// Creates a new reversal journal with swapped debit/credit
        /// </summary>
        GLJournal ReverseJournal(ReverseJournalDto dto, string userName);

        /// <summary>
        /// Submit journal for approval - تقديم دفتر يومية للاعتماد
        /// </summary>
        int SubmitJournal(Guid id, string userName);

        /// <summary>
        /// Approve journal - اعتماد دفتر يومية
        /// </summary>
        int ApproveJournal(Guid id, string userName);

        /// <summary>
        /// Reject journal - رفض دفتر يومية
        /// </summary>
        int RejectJournal(Guid id, string reason, string userName);

        /// <summary>
        /// Generate next journal number (thread-safe) - توليد رقم دفتر اليومية التالي
        /// </summary>
        string GenerateNextJournalNumber();

        /// <summary>
        /// Check if journal is balanced - التحقق من توازن دفتر اليومية
        /// </summary>
        bool IsJournalBalanced(Guid journalId);

        /// <summary>
        /// Validate journal - returns detailed validation result
        /// </summary>
        JournalValidationResultDto ValidateJournal(Guid journalId);

        /// <summary>
        /// Get journals by date range - الحصول على دفاتر اليومية بنطاق التاريخ
        /// </summary>
        List<GLJournal> GetByDateRange(DateTime startDate, DateTime endDate, long? companyId = null);

        /// <summary>
        /// Get journals by status - الحصول على دفاتر اليومية بالحالة
        /// </summary>
        List<GLJournal> GetByStatus(JournalStatus status, long? companyId = null);

        /// <summary>
        /// Get Year-End Closing Journal for a fiscal year
        /// البحث عن قيد إقفال نهاية السنة للسنة المالية
        /// Used for auto-reversal when reopening a fiscal year period
        /// </summary>
        /// <param name="fiscalYear">The fiscal year to search for</param>
        /// <param name="companyId">Optional company ID for multi-tenant support</param>
        /// <returns>The Year-End Closing Journal if found, null otherwise</returns>
        GLJournal GetYearEndClosingJournal(int fiscalYear, long? companyId = null);
    }
}
