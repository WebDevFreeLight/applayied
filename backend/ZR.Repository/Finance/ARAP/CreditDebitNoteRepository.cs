using System;
using System.Collections.Generic;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ARAP;

namespace ZR.Repository.Finance.ARAP
{
    /// <summary>
    /// Credit/Debit Note Repository - مستودع الإشعارات
    /// </summary>
    [AppService(ServiceType = typeof(ICreditDebitNoteRepository), ServiceLifetime = LifeTime.Transient)]
    public class CreditDebitNoteRepository : BaseRepository<CreditDebitNote>, ICreditDebitNoteRepository
    {
        /// <summary>
        /// Get paginated list of notes
        /// </summary>
        public PagedInfo<CreditDebitNote> GetList(CreditDebitNote parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<CreditDebitNote>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.NoteNumber), it => it.NoteNumber.Contains(parm.NoteNumber));
            predicate = predicate.AndIF(parm.CustomerId.HasValue, it => it.CustomerId == parm.CustomerId);
            predicate = predicate.AndIF(parm.VendorId.HasValue, it => it.VendorId == parm.VendorId);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderByDescending(x => x.NoteDate)
                .ToPage(pager);
        }

        /// <summary>
        /// Get note by ID
        /// </summary>
        public CreditDebitNote GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get note by number
        /// </summary>
        public CreditDebitNote GetByNoteNumber(string noteNumber)
        {
            return GetFirst(x => x.NoteNumber == noteNumber);
        }

        /// <summary>
        /// Get notes by customer
        /// </summary>
        public List<CreditDebitNote> GetByCustomerId(Guid customerId)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.NoteDate)
                .ToList();
        }

        /// <summary>
        /// Get notes by vendor
        /// </summary>
        public List<CreditDebitNote> GetByVendorId(Guid vendorId)
        {
            return Queryable()
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.NoteDate)
                .ToList();
        }
    }
}


