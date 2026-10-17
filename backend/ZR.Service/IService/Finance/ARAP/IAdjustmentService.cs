using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;

namespace ZR.Service.IService.Finance.ARAP
{
    /// <summary>
    /// Adjustment Service Interface - واجهة خدمة التعديلات
    /// </summary>
    public interface IAdjustmentService : IBaseService<CreditDebitNote>
    {
        /// <summary>
        /// Get paginated list of notes - الحصول على قائمة الإشعارات بالترقيم
        /// </summary>
        PagedInfo<CreditDebitNoteListDto> GetList(CreditDebitNoteQueryDto query, PagerInfo pager);

        /// <summary>
        /// Get note by ID - الحصول على الإشعار بالمعرف
        /// </summary>
        CreditDebitNoteDetailDto GetById(Guid id);

        /// <summary>
        /// Create credit/debit note - إنشاء إشعار دائن/مدين
        /// </summary>
        CreditDebitNote Create(CreateCreditDebitNoteDto dto, string userName);

        /// <summary>
        /// Apply adjustment to invoice - تطبيق التعديل على الفاتورة
        /// </summary>
        void ApplyAdjustmentToInvoice(Guid noteId, Guid invoiceId);

        /// <summary>
        /// Prepare GL journal data - إعداد بيانات القيد المحاسبي
        /// </summary>
        object PrepareGLJournalData(Guid noteId);
    }
}


