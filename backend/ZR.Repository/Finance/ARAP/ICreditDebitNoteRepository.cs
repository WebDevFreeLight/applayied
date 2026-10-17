using System;
using System.Collections.Generic;
using ZR.Model;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// Credit/Debit Note Repository Interface - واجهة مستودع الإشعارات
    /// </summary>
    public interface ICreditDebitNoteRepository : IBaseRepository<CreditDebitNote>
    {
        /// <summary>
        /// Get paginated list of notes - الحصول على قائمة الإشعارات بالترقيم
        /// </summary>
        PagedInfo<CreditDebitNote> GetList(CreditDebitNote parm, PagerInfo pager);

        /// <summary>
        /// Get note by ID - الحصول على الإشعار بالمعرف
        /// </summary>
        CreditDebitNote GetById(Guid id);

        /// <summary>
        /// Get note by number - الحصول على الإشعار بالرقم
        /// </summary>
        CreditDebitNote GetByNoteNumber(string noteNumber);

        /// <summary>
        /// Get notes by customer - الحصول على إشعارات العميل
        /// </summary>
        List<CreditDebitNote> GetByCustomerId(Guid customerId);

        /// <summary>
        /// Get notes by vendor - الحصول على إشعارات المورد
        /// </summary>
        List<CreditDebitNote> GetByVendorId(Guid vendorId);
    }
}


