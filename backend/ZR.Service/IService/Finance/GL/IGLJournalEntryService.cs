using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.GL.Dto;

namespace ZR.Service.IService.Finance.GL
{
    /// <summary>
    /// GL Journal Entry Service Interface - واجهة خدمة قيود اليومية
    /// </summary>
    public interface IGLJournalEntryService : IBaseService<GLJournalEntry>
    {
        /// <summary>
        /// Get paginated list of entries - الحصول على قائمة القيود بالترقيم
        /// </summary>
        PagedInfo<JournalEntryResponseDto> GetList(GLJournalEntryQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get entry by ID - الحصول على القيد بالمعرف
        /// </summary>
        JournalEntryResponseDto GetById(Guid id);

        /// <summary>
        /// Create entry - إنشاء قيد
        /// </summary>
        GLJournalEntry Create(CreateJournalEntryDto dto, string userName);

        /// <summary>
        /// Create multiple entries - إنشاء قيود متعددة
        /// </summary>
        int CreateMultiple(List<CreateJournalEntryDto> dtos, Guid journalId, string userName);

        /// <summary>
        /// Update entry - تحديث قيد
        /// </summary>
        int Update(UpdateJournalEntryDto dto, string userName);

        /// <summary>
        /// Delete entries - حذف قيود
        /// </summary>
        int Delete(Guid[] ids);

        /// <summary>
        /// Delete entries by journal ID - حذف قيود بمعرف دفتر اليومية
        /// </summary>
        int DeleteByJournalId(Guid journalId);

        /// <summary>
        /// Get entries by journal ID - الحصول على القيود بمعرف دفتر اليومية
        /// </summary>
        List<JournalEntryResponseDto> GetByJournalId(Guid journalId);

        /// <summary>
        /// Get entries by account ID - الحصول على القيود بمعرف الحساب
        /// </summary>
        List<JournalEntryResponseDto> GetByAccountId(Guid accountId);

        /// <summary>
        /// Get journal totals - الحصول على إجماليات دفتر اليومية
        /// </summary>
        (decimal TotalDebit, decimal TotalCredit) GetJournalTotals(Guid journalId);
    }
}


