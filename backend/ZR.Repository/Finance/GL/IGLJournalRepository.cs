using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Journal Repository Interface - واجهة مستودع دفتر اليومية
    /// </summary>
    public interface IGLJournalRepository : IBaseRepository<GLJournal>
    {
        /// <summary>
        /// Get paginated list of journals - الحصول على قائمة دفاتر اليومية بالترقيم
        /// </summary>
        PagedInfo<GLJournal> GetList(GLJournal parm, PagerInfo pager);

        /// <summary>
        /// Get journal by ID - الحصول على دفتر اليومية بالمعرف
        /// </summary>
        GLJournal GetById(Guid id);

        /// <summary>
        /// Get journal by number - الحصول على دفتر اليومية بالرقم
        /// </summary>
        GLJournal GetByNumber(string journalNumber);

        /// <summary>
        /// Insert journal - إضافة دفتر يومية
        /// </summary>
        int InsertJournal(GLJournal model);

        /// <summary>
        /// Update journal - تحديث دفتر يومية
        /// </summary>
        int UpdateJournal(GLJournal model);

        /// <summary>
        /// Delete journals by IDs - حذف دفاتر يومية بالمعرفات
        /// </summary>
        int DeleteByIds(Guid[] ids);

        /// <summary>
        /// Get journals by status - الحصول على دفاتر اليومية بالحالة
        /// </summary>
        List<GLJournal> GetByStatus(JournalStatus status);

        /// <summary>
        /// Get journals by date range - الحصول على دفاتر اليومية بنطاق التاريخ
        /// </summary>
        List<GLJournal> GetByDateRange(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get journals by date range and tenant - الحصول على دفاتر اليومية بنطاق التاريخ والمستأجر
        /// </summary>
        List<GLJournal> GetByDateRange(DateTime fromDate, DateTime toDate, Guid? tenantId);

        /// <summary>
        /// Update journal status - تحديث حالة دفتر اليومية
        /// </summary>
        int UpdateStatus(Guid id, JournalStatus status, string updatedBy);

        /// <summary>
        /// Check if journal number exists - التحقق من وجود رقم دفتر اليومية
        /// </summary>
        bool NumberExists(string journalNumber, Guid? excludeId = null);

        /// <summary>
        /// Get journal with entries - الحصول على دفتر اليومية مع القيود
        /// </summary>
        GLJournal GetWithEntries(Guid id);

        /// <summary>
        /// Get journal entries for a journal - الحصول على قيود دفتر اليومية
        /// </summary>
        List<GLJournalEntry> GetJournalEntries(Guid journalId);
    }
}
