using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.GL;
using ZR.Model.Finance.Enums;

namespace ZR.Repository.Finance.GL
{
    /// <summary>
    /// GL Journal Repository - مستودع دفتر اليومية
    /// </summary>
    [AppService(ServiceType = typeof(IGLJournalRepository), ServiceLifetime = LifeTime.Transient)]
    public class GLJournalRepository : BaseRepository<GLJournal>, IGLJournalRepository
    {
        /// <summary>
        /// Get paginated list of journals
        /// </summary>
        public PagedInfo<GLJournal> GetList(GLJournal parm, PagerInfo pager)
        {
            var predicate = Expressionable.Create<GLJournal>();
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.JournalNumber), it => it.JournalNumber.Contains(parm.JournalNumber));
            predicate = predicate.AndIF(parm.Status != JournalStatus.Draft || true, it => it.Status == parm.Status);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Source), it => it.Source == parm.Source);
            predicate = predicate.AndIF(parm.CompanyID.HasValue, it => it.CompanyID == parm.CompanyID);

            return Queryable()
                .Where(predicate.ToExpression())
                .OrderBy(x => x.JournalDate, OrderByType.Desc)
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .ToPage(pager);
        }

        /// <summary>
        /// Get journal by ID
        /// </summary>
        public GLJournal GetById(Guid id)
        {
            return GetFirst(x => x.Id == id);
        }

        /// <summary>
        /// Get journal by number
        /// </summary>
        public GLJournal GetByNumber(string journalNumber)
        {
            return GetFirst(x => x.JournalNumber == journalNumber);
        }

        /// <summary>
        /// Insert journal
        /// </summary>
        public int InsertJournal(GLJournal model)
        {
            return Insert(model);
        }

        /// <summary>
        /// Update journal
        /// </summary>
        public int UpdateJournal(GLJournal model)
        {
            return Update(model, it => new
            {
                it.JournalNumber,
                it.DescriptionAr,
                it.DescriptionEn,
                it.JournalDate,
                it.Source,
                it.Status,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Delete journals by IDs (soft delete by setting status to cancelled)
        /// </summary>
        public int DeleteByIds(Guid[] ids)
        {
            return Context.Updateable<GLJournal>()
                .SetColumns(it => it.Status == JournalStatus.Cancelled)
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Get journals by status
        /// </summary>
        public List<GLJournal> GetByStatus(JournalStatus status)
        {
            return Queryable()
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.JournalDate)
                .ToList();
        }

        /// <summary>
        /// Get journals by date range
        /// </summary>
        public List<GLJournal> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            return Queryable()
                .Where(x => x.JournalDate >= fromDate && x.JournalDate <= toDate)
                .OrderByDescending(x => x.JournalDate)
                .ToList();
        }

        /// <summary>
        /// Get journals by date range and tenant
        /// </summary>
        public List<GLJournal> GetByDateRange(DateTime fromDate, DateTime toDate, Guid? tenantId)
        {
            var query = Queryable()
                .Where(x => x.JournalDate >= fromDate && x.JournalDate <= toDate);

            if (tenantId.HasValue)
            {
                var companyId = (long)tenantId.Value.GetHashCode();
                query = query.Where(x => x.CompanyID == companyId);
            }

            return query.OrderByDescending(x => x.JournalDate).ToList();
        }

        /// <summary>
        /// Update journal status
        /// </summary>
        public int UpdateStatus(Guid id, JournalStatus status, string updatedBy)
        {
            return Context.Updateable<GLJournal>()
                .SetColumns(it => new GLJournal { Status = status, Update_by = updatedBy, Update_time = DateTime.Now })
                .Where(it => it.Id == id)
                .ExecuteCommand();
        }

        /// <summary>
        /// Check if journal number exists
        /// </summary>
        public bool NumberExists(string journalNumber, Guid? excludeId = null)
        {
            var predicate = Expressionable.Create<GLJournal>();
            predicate = predicate.And(it => it.JournalNumber == journalNumber);
            predicate = predicate.AndIF(excludeId.HasValue, it => it.Id != excludeId.Value);

            return Any(predicate.ToExpression());
        }

        /// <summary>
        /// Get journal with entries
        /// </summary>
        public GLJournal GetWithEntries(Guid id)
        {
            var journal = GetById(id);
            if (journal != null)
            {
                journal.Entries = Context.Queryable<GLJournalEntry>()
                    .Where(x => x.JournalId == id)
                    .OrderBy(x => x.LineNumber)
                    .ToList();
            }
            return journal;
        }

        /// <summary>
        /// Get journal entries for a journal
        /// </summary>
        public List<GLJournalEntry> GetJournalEntries(Guid journalId)
        {
            return Context.Queryable<GLJournalEntry>()
                .Where(x => x.JournalId == journalId)
                .OrderBy(x => x.LineNumber)
                .ToList();
        }
    }
}
